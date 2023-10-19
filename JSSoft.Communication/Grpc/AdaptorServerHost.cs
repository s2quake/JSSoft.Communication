// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Grpc.Core;
using JSSoft.Communication.Logging;
using JSSoft.Library.Linq;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc;

sealed class AdaptorServerHost : IAdaptorHost
{
    private static readonly TimeSpan timeout = new(0, 0, 30);
    private static readonly string localAddress = "127.0.0.1";
    private readonly IServiceContext _serviceContext;
    private readonly IContainer<IServiceHost> _serviceHosts;
    private int _closeCode;
    private CancellationTokenSource? _cancellationTokenSource;
    private Server? _server;
    private AdaptorServerImpl? _adaptor;
    private ISerializer? _serializer;
    private Timer? _timer;
    private EventHandler<CloseEventArgs>? _disconnectedEventHandler;

    static AdaptorServerHost()
    {
        var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        var address = addressList.FirstOrDefault(item => $"{item}" != "127.0.0.1" && item.AddressFamily == AddressFamily.InterNetwork);
        if (address != null)
            localAddress = $"{address}";
    }

    public AdaptorServerHost(IServiceContext serviceContext, IInstanceContext instanceContext)
    {
        _serviceContext = serviceContext;
        _serviceHosts = serviceContext.ServiceHosts;
        Peers = new PeerCollection(serviceContext, instanceContext);
        Peers.CollectionChanged += PeerCollection_CollectionChanged;
    }

    public async Task<OpenReply> Open(OpenRequest request, ServerCallContext context)
    {
        var token = Guid.NewGuid();
        var serviceNames = request.ServiceNames;
        var serviceHosts = serviceNames.Select(item => _serviceHosts[item]).ToArray();
        var peerID = context.Peer;
        var peer = new Peer(token, serviceHosts) { Token = token };
        await Peers.AddAsync(peer);
        LogUtility.Debug($"{context.Peer}({token}) Connected");
        return new OpenReply() { Token = $"{token}" };
    }

    public async Task<CloseReply> Close(CloseRequest request, ServerCallContext context)
    {
        var token = request.Token;
        await Peers.RemoveAsync(token);
        LogUtility.Debug($"{context.Peer}({token}) Disconnected");
        return new CloseReply();
    }

    public Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        var token = request.Token;
        return Dispatcher.InvokeAsync(() =>
        {
            var peer = Peers[token];
            if (peer == null)
                throw new InvalidOperationException($"invalid token: '{token}'");
            peer.Ping();
            LogUtility.Debug($"{context.Peer}({token}) Ping: {DateTime.Now}");
            return new PingReply() { Time = peer.PingTime.Ticks };
        });
    }

    public async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
    {
        if (_serializer == null)
            throw new InvalidOperationException();
        if (_serviceHosts.ContainsKey(request.ServiceName) == false)
            throw new InvalidOperationException();
        var service = _serviceHosts[request.ServiceName];
        if (service.MethodDescriptors.ContainsKey(request.Name) == false)
            throw new InvalidOperationException($"method '{request.Name}' does not exists.");

        var token = request.Token;
        var methodDescriptor = service.MethodDescriptors[request.Name];
        var peer = Peers[token];
        var instance = peer.Services[service];
        var args = _serializer.DeserializeMany(methodDescriptor.ParameterTypes, request.Datas.ToArray());
        var (id, valueType, value) = await methodDescriptor.InvokeAsync(_serviceContext, instance, args);
        var reply = new InvokeReply()
        {
            ID = $"{id}",
            Data = _serializer.Serialize(valueType, value)
        };
        LogUtility.Debug($"{context.Peer} Invoke: {request.ServiceName}.{methodDescriptor.ShortName}");
        return reply;
    }

    public async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
    {
        var cancellationToken = _cancellationTokenSource!.Token;
        var peerID = context.Peer;
        Peer? peer = null;
        while (await requestStream.MoveNext())
        {
            if (peer == null)
            {
                var token = requestStream.Current.Token;
                peer = await Dispatcher.InvokeAsync(() => Peers[token]);
            }
            var request = requestStream.Current;
            var services = peer.ServiceHosts;
            if (_cancellationTokenSource.IsCancellationRequested == true)
            {
                await responseStream.WriteAsync(new PollReply() { Code = _closeCode });
                break;
            }
            if (peer.Cancellation.IsCancellationRequested == true)
            {
                break;
            }
            var reply = new PollReply() { Code = int.MinValue };
            await Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in services)
                {
                    var items = Poll(peer, item);
                    reply.Items.AddRange(items);
                }
            });
            await responseStream.WriteAsync(reply);
        }
        await Peers.RemoveAsync($"{peer!.ID}");
    }

    public PeerCollection Peers { get; }

    public Dispatcher Dispatcher => _serviceContext.Dispatcher;

    event EventHandler<CloseEventArgs>? IAdaptorHost.Disconnected
    {
        add => _disconnectedEventHandler += value;
        remove => _disconnectedEventHandler -= value;
    }

    private Task AddCallback(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_serializer == null)
            throw new UnreachableException();

        var datas = _serializer.SerializeMany(types, args);
        return Dispatcher.InvokeAsync(() =>
        {
            var peers = instance.Peer is not Peer peer ? Peers : EnumerableUtility.One(peer);
            var service = instance.ServiceHost;
            foreach (var item in peers)
            {
                if (item.PollReplyItems.ContainsKey(service) == true)
                {
                    var callbacks = item.PollReplyItems[service];
                    var pollItem = new PollReplyItem()
                    {
                        Name = name,
                        ServiceName = instance.ServiceName
                    };
                    pollItem.Datas.AddRange(datas);
                    callbacks.Add(pollItem);
                }
            }
        });
    }

    private PollReplyItem[] Poll(Peer peer, IServiceHost service)
    {
        Dispatcher.VerifyAccess();
        var callbacks = peer.PollReplyItems[service];
        return callbacks.Flush();
    }

    private void PeerCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    if (_timer == null)
                    {
                        var milliseconds = (int)timeout.TotalMilliseconds;
                        _timer = new Timer(Timer_TimerCallback, null, milliseconds, milliseconds);
                    }
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                {
                    if (Peers.Any() == false)
                    {
                        _timer?.Dispose();
                        _timer = null;
                    }
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                {
                    if (_timer != null)
                    {
                        _timer?.Dispose();
                        _timer = null;
                    }
                }
                break;
        }
    }

    private async void Timer_TimerCallback(object? state)
    {
        var items = await Dispatcher.InvokeAsync(() =>
        {
            var dateTime = DateTime.UtcNow;
            var query = from item in Peers
                        where dateTime - item.PingTime > timeout
                        select item;
            return query.ToArray();
        });
        var tasks = items.Select(item => Peers.RemoveAsync($"{item.ID}"));
        await Task.WhenAll(tasks);
    }

    #region IAdaptorHost

    async Task IAdaptorHost.OpenAsync(string host, int port)
    {
        await Dispatcher.InvokeAsync(() =>
        {
            _adaptor = new AdaptorServerImpl(this);
            _server = new Server()
            {
                Services = { Adaptor.BindService(_adaptor) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
            };
            if (host == ServiceContextBase.DefaultHost)
            {
                _server.Ports.Add(new ServerPort(localAddress, port, ServerCredentials.Insecure));
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _serializer = _serviceContext.GetService(typeof(ISerializer)) as ISerializer;
            _closeCode = 0;
        });
        await Task.Run(_server!.Start);
    }

    async Task IAdaptorHost.CloseAsync(int closeCode)
    {
        _closeCode = closeCode;
        _cancellationTokenSource!.Cancel();
        while (await Dispatcher.InvokeAsync(Peers.Any))
        {
            await Task.Delay(1);
        }
        await _server!.ShutdownAsync();
        _adaptor = null;
        _serializer = null;
        _server = null;
    }

    void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        AddCallback(instance, name, types, args);
    }

    T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        throw new NotImplementedException();
    }

    Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        throw new NotImplementedException();
    }

    Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        throw new NotImplementedException();
    }

    #endregion
}
