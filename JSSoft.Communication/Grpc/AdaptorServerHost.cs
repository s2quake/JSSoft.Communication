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
using System.Collections.Generic;
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
    private static readonly TimeSpan PingTimeout = new(0, 0, 30);
    private static readonly string localAddress = "127.0.0.1";
    private readonly IServiceContext _serviceContext;
    private readonly IContainer<IServiceHost> _serviceHosts;
    private readonly Dictionary<IServiceHost, MethodDescriptorCollection> _methodsByServiceHost;
    private int _closeCode;
    private CancellationTokenSource? _cancellationTokenSource;
    private Server? _server;
    private AdaptorServerImpl? _adaptor;
    private ISerializer? _serializer;
    private readonly Timer _timer;
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
        _methodsByServiceHost = _serviceHosts.ToDictionary(item => item, item => new MethodDescriptorCollection(item));
        Peers = new PeerCollection(instanceContext);
        _timer = new Timer(Timer_TimerCallback, null, TimeSpan.Zero, PingTimeout);
    }

    public async Task<OpenReply> Open(OpenRequest request, ServerCallContext context, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid();
        var serviceNames = request.ServiceNames;
        var serviceHosts = serviceNames.Select(item => _serviceHosts[item]).ToArray();
        var peerID = context.Peer;
        var peer = new Peer(token, serviceHosts) { Token = token };
        LogUtility.Debug($"{context.Peer}({token}) Connecting");
        Peers.Add(peer);
        LogUtility.Debug($"{context.Peer}({token}) Connected");
        await Task.CompletedTask;
        return new OpenReply() { Token = $"{token}" };
    }

    public async Task<CloseReply> Close(CloseRequest request, ServerCallContext context, CancellationToken cancellationToken)
    {
        var token = request.Token;
        LogUtility.Debug($"{context.Peer}({token}) Disconnecting");
        Peers.Remove(token, "close");
        LogUtility.Debug($"{context.Peer}({token}) Disconnected");
        await Task.CompletedTask;
        return new CloseReply();
    }

    public async Task<PingReply> Ping(PingRequest request, ServerCallContext context)
    {
        var token = request.Token;
        var dateTime = DateTime.UtcNow;
        if (Peers.TryGetValue(token, out var peer) == true)
        {
            peer.Ping(dateTime);
            LogUtility.Debug($"{context.Peer}({token}) Ping: {dateTime}");
            return new PingReply() { Time = peer.PingTime.Ticks };
        }
        await Task.CompletedTask;
        return new PingReply() { Time = DateTime.MinValue.Ticks };
    }

    public async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
    {
        if (_serializer == null)
            throw new InvalidOperationException();
        if (_serviceHosts.ContainsKey(request.ServiceName) != true)
            throw new InvalidOperationException();
        var serviceHost = _serviceHosts[request.ServiceName];
        var methodDescriptors = _methodsByServiceHost[serviceHost];
        if (methodDescriptors.ContainsKey(request.Name) != true)
            throw new InvalidOperationException($"method '{request.Name}' does not exists.");

        var token = request.Token;
        if (Peers.TryGetValue(token, out var peer) == true)
        {
            var methodDescriptor = methodDescriptors[request.Name];
            var instance = peer.Services[serviceHost];
            var args = _serializer.DeserializeMany(methodDescriptor.ParameterTypes, [.. request.Data]);
            var (id, valueType, value) = await methodDescriptor.InvokeAsync(_serviceContext, instance, args);
            var reply = new InvokeReply()
            {
                ID = $"{id}",
                Data = _serializer.Serialize(valueType, value)
            };
            LogUtility.Debug($"{context.Peer} Invoke: {request.ServiceName}.{methodDescriptor.ShortName}");
            return reply;
        }
        return new InvokeReply();
    }

    public async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
    {
        try
        {
            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource!.Token, context.CancellationToken);
            while (await requestStream.MoveNext(cancellationTokenSource.Token))
            {
                var reply = new PollReply() { Code = int.MinValue };
                var token = requestStream.Current.Token;
                if (Peers.TryGetValue(token, out var peer) == true)
                {
                    var services = peer.ServiceHosts;
                    foreach (var item in services)
                    {
                        var items = Poll2(peer, item);
                        reply.Items.AddRange(items);
                    }
                }
                await responseStream.WriteAsync(reply);
            }
        }
        catch
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _timer.DisposeAsync();
    }

    public PeerCollection Peers { get; }

    public Dispatcher Dispatcher => _serviceContext.Dispatcher;

    event EventHandler<CloseEventArgs>? IAdaptorHost.Disconnected
    {
        add => _disconnectedEventHandler += value;
        remove => _disconnectedEventHandler -= value;
    }

    private void AddCallback(InstanceBase instance, string name, Type[] types, object?[] args)
    {
        if (_serializer == null)
            throw new UnreachableException();

        var data = _serializer.SerializeMany(types, args);
        var peers = instance.Peer is not Peer peer ? Peers.ToArray().Select(item => item.Value) : EnumerableUtility.One(peer);
        var service = instance.ServiceHost;
        foreach (var item in peers)
        {
            lock (item)
            {
                if (item.PollReplyItems.ContainsKey(service) == true)
                {
                    var callbacks = item.PollReplyItems[service];
                    var pollItem = new PollReplyItem()
                    {
                        Name = name,
                        ServiceName = instance.ServiceName
                    };
                    pollItem.Data.AddRange(data);
                    callbacks.Add(pollItem);
                }
            }
        }
    }

    private PollReplyItem[] Poll2(Peer peer, IServiceHost service)
    {
        lock (peer)
        {
            var callbacks = peer.PollReplyItems[service];
            return callbacks.Flush();
        }
    }

    private async void Timer_TimerCallback(object? state)
    {
        // var items = await Dispatcher.InvokeAsync(() =>
        // {
        //     var dateTime = DateTime.UtcNow;
        //     var query = from item in Peers.Values
        //                 where dateTime - item.PingTime > PingTimeout
        //                 select item;
        //     return query.ToArray();
        // });
        // if (items.Length > 0)
        // {
        //     int wqer = 0;
        // }
        // var tasks = items.Select(item => Peers.RemoveAsync(Dispatcher, $"{item.ID}", "timer", CancellationToken.None));
        // await Task.WhenAll(tasks);
    }

    #region IAdaptorHost

    async Task IAdaptorHost.OpenAsync(string host, int port, CancellationToken cancellationToken)
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
        await Task.Run(_server.Start, cancellationToken);
    }

    async Task IAdaptorHost.CloseAsync(int closeCode, CancellationToken cancellationToken)
    {
        _closeCode = closeCode;
        _cancellationTokenSource?.Cancel();
        while (Peers.Any() == true)
        {
            await Task.Delay(1, cancellationToken);
        }
        await _server!.ShutdownAsync();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
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
