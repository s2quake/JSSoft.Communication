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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc
{
    class AdaptorServerHost : IAdaptorHost
    {
        private static readonly TimeSpan timeout = new(0, 0, 30);
        private static readonly string localAddress = "127.0.0.1";
        private readonly ServerContextBase serviceContext;
        private readonly IContainer<IServiceHost> serviceHosts;
        private int closeCode;
        private CancellationTokenSource cancellation;
        private Server server;
        private AdaptorServerImpl adaptor;
        private ISerializer serializer;
        private Timer timer;

        static AdaptorServerHost()
        {
            var addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            var address = addressList.FirstOrDefault(item => $"{item}" != "127.0.0.1" && item.AddressFamily == AddressFamily.InterNetwork);
            if (address != null)
                localAddress = $"{address}";
        }

        public AdaptorServerHost(ServerContextBase serviceContext)
        {
            this.serviceContext = serviceContext;
            this.serviceHosts = serviceContext.ServiceHosts;
            this.Peers = new PeerCollection(serviceContext);
            this.Peers.CollectionChanged += PeerCollection_CollectionChanged;
        }

        internal async Task<OpenReply> Open(OpenRequest request, ServerCallContext context)
        {
            var token = Guid.NewGuid();
            var serviceNames = request.ServiceNames;
            var serviceHosts = serviceNames.Select(item => this.serviceHosts[item]).ToArray();
            var peerID = context.Peer;
            var peer = new Peer($"{token}", serviceHosts) { Token = token };
            await this.Peers.AddAsync(peer);
            LogUtility.Debug($"{context.Peer}({token}) Connected");
            return new OpenReply() { Token = $"{token}" };
        }

        public async Task<CloseReply> Close(CloseRequest request, ServerCallContext context)
        {
            var token = request.Token;
            var peer = await this.Peers.RemoveAsync(token);
            peer.Dispose();
            LogUtility.Debug($"{context.Peer}({token}) Disconnected");
            return new CloseReply();
        }

        public Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            var token = request.Token;
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peer = this.Peers[token];
                if (peer == null)
                    throw new InvalidOperationException($"invalid token: '{token}'");
                peer.Ping();
                LogUtility.Debug($"{context.Peer}({token}) Ping: {DateTime.Now}");
                return new PingReply() { Time = peer.PingTime.Ticks };
            });
        }

        public async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            if (this.serviceHosts.ContainsKey(request.ServiceName) == false)
                throw new InvalidOperationException();
            var service = this.serviceHosts[request.ServiceName];
            if (service.MethodDescriptors.ContainsKey(request.Name) == false)
                throw new InvalidOperationException($"method '{request.Name}' does not exists.");

            var token = request.Token;
            var methodDescriptor = service.MethodDescriptors[request.Name];
            var peer = this.Peers[token];
            var instance = peer.Services[service];
            var args = this.serializer.DeserializeMany(methodDescriptor.ParameterTypes, request.Datas.ToArray());
            var (code, valueType, value) = await methodDescriptor.InvokeAsync(this.serviceContext, instance, args);
            var reply = new InvokeReply()
            {
                Code = code,
                Data = this.serializer.Serialize(valueType, value)
            };
            LogUtility.Debug($"{context.Peer} Invoke: {request.ServiceName}.{methodDescriptor.ShortName}");
            return reply;
        }

        public async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            var cancellationToken = this.cancellation.Token;
            var peerID = context.Peer;
            var peer = null as Peer;
            while (await requestStream.MoveNext())
            {
                if (peer == null)
                {
                    var token = requestStream.Current.Token;
                    peer = await this.Dispatcher.InvokeAsync(() => this.Peers[token]);
                }
                var request = requestStream.Current;
                var services = peer.ServiceHosts;
                if (this.cancellation.IsCancellationRequested == true)
                {
                    await responseStream.WriteAsync(new PollReply() { Code = this.closeCode });
                    break;
                }
                if (peer.Cancellation.IsCancellationRequested == true)
                {
                    break;
                }
                var reply = new PollReply() { Code = int.MinValue };
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in services)
                    {
                        var items = this.Poll(peer, item);
                        reply.Items.AddRange(items);
                    }
                });
                await responseStream.WriteAsync(reply);
            }
            if (this.cancellation.IsCancellationRequested == true || peer.Cancellation.IsCancellationRequested == true)
            {
                await this.Peers.RemoveAsync(peer.ID);
                peer.Dispose();
            }
        }

        public PeerCollection Peers { get; }

        public Dispatcher Dispatcher => this.serviceContext.Dispatcher;

        public event EventHandler<CloseEventArgs> Disconnected;

        protected virtual void OnDisconnected(CloseEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        private Task AddCallback(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peers = instance.Peer is not Peer peer ? this.Peers : EnumerableUtility.One(peer);
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
            this.Dispatcher.VerifyAccess();
            var callbacks = peer.PollReplyItems[service];
            return callbacks.Flush();
        }

        private void PeerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (this.timer == null)
                        {
                            var milliseconds = (int)timeout.TotalMilliseconds;
                            this.timer = new Timer(Timer_TimerCallback, null, milliseconds, milliseconds);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (this.Peers.Any() == false)
                        {
                            this.timer.Dispose();
                            this.timer = null;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        if (this.timer != null)
                        {
                            this.timer.Dispose();
                            this.timer = null;
                        }
                    }
                    break;
            }
        }

        private void Timer_TimerCallback(object state)
        {
            this.Dispatcher.Invoke(() =>
            {
                var dateTime = DateTime.UtcNow;
                var query = from item in this.Peers
                            where dateTime - item.PingTime > timeout
                            select item;
                var items = query.ToArray();
                foreach (var item in items)
                {
                    item.Abort();
                }
            });
        }

        #region IAdaptorHost

        async Task IAdaptorHost.OpenAsync(string host, int port)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.adaptor = new AdaptorServerImpl(this);
                this.server = new Server()
                {
                    Services = { Adaptor.BindService(this.adaptor) },
                    Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
                };
                if (host == ServiceContextBase.DefaultHost)
                {
                    this.server.Ports.Add(new ServerPort(localAddress, port, ServerCredentials.Insecure));
                }
                this.cancellation = new CancellationTokenSource();
                this.serializer = this.serviceContext.GetService(typeof(ISerializer)) as ISerializer;
                this.closeCode = 0;
            });
            await Task.Run(this.server.Start);
        }

        async Task IAdaptorHost.CloseAsync(int closeCode)
        {
            this.closeCode = closeCode;
            this.cancellation.Cancel();
            while (await this.Dispatcher.InvokeAsync(this.Peers.Any))
            {
                await Task.Delay(1);
            }
            await this.server.ShutdownAsync();
            this.adaptor = null;
            this.serializer = null;
            this.server = null;
        }

        void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object[] args)
        {
            this.AddCallback(instance, name, types, args);
        }

        T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            throw new NotImplementedException();
        }

        Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object[] args)
        {
            throw new NotImplementedException();
        }

        Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
