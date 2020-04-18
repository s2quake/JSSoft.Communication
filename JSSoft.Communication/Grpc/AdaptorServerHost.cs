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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using JSSoft.Communication.Logging;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace JSSoft.Communication.Grpc
{
    class AdaptorServerHost : IAdaptorHost
    {
        private static readonly string localAddress;
        private readonly IServiceContext serviceContext;
        private readonly IContainer<IServiceHost> serviceHosts;
        private CancellationTokenSource cancellation;
        private Server server;
        private AdaptorServerImpl adaptor;
        private ISerializer serializer;

        static AdaptorServerHost()
        {
            localAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(item => $"{item}" != "127.0.0.1" && item.AddressFamily == AddressFamily.InterNetwork).ToString();
        }

        public AdaptorServerHost(IServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.serviceHosts = serviceContext.ServiceHosts;
            this.Peers = new PeerCollection(this);
        }

        internal async Task<OpenReply> Open(OpenRequest request, ServerCallContext context)
        {
            var token = await this.Dispatcher.InvokeAsync(() =>
            {
                var serviceNames = request.ServiceNames;
                var serviceHosts = serviceNames.Select(item => this.serviceHosts[item]).ToArray();
                var peerID = context.Peer;
                var peer = new Peer(peerID, serviceHosts);
                this.Peers.Add(peer);
                return peer.Token;
            });
            LogUtility.Debug($"{context.Peer} Connected");
            return new OpenReply() { Token = $"{token}" };
        }

        internal async Task<CloseReply> Close(CloseRequest request, ServerCallContext context)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var peer = this.Peers[context.Peer];
                peer.Dispose();
            });
            LogUtility.Debug($"{context.Peer} Disconnected");
            return new CloseReply();
        }

        internal Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peer = this.Peers[context.Peer];
                peer.Ping();
                LogUtility.Debug($"{context.Peer} Ping: {DateTime.Now}");
                return new PingReply() { Time = peer.PingTime.Ticks };
            });
        }

        internal async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            if (this.serviceHosts.ContainsKey(request.ServiceName) == false)
                throw new InvalidOperationException();
            var service = this.serviceHosts[request.ServiceName];
            if (service.MethodDescriptors.ContainsKey(request.Name) == false)
                throw new InvalidOperationException($"method '{request.Name}' does not exists.");

            var methodDescriptor = service.MethodDescriptors[request.Name];
            var peer = this.Peers[context.Peer];
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

        internal async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            var cancellationToken = this.cancellation.Token;
            var peerID = context.Peer;
            var peer = await this.Dispatcher.InvokeAsync(() => this.Peers[peerID]);
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var services = peer.ServiceHosts;
                if (this.cancellation.IsCancellationRequested == true)
                {
                    await responseStream.WriteAsync(new PollReply() { Code = -1 });
                    break;
                }
                if (peer.Cancellation.IsCancellationRequested == true)
                {
                    break;
                }
                var reply = new PollReply();
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
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.Peers.ContainsKey(peer.ID) == true)
                {
                    peer.Dispose();
                }
            });
        }

        public PeerCollection Peers { get; }

        public Dispatcher Dispatcher => this.serviceContext.Dispatcher;

        public event EventHandler<DisconnectionReasonEventArgs> Disconnected;

        protected virtual void OnDisconnected(DisconnectionReasonEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        private void ValidateToken(string token)
        {
            this.Dispatcher.VerifyAccess();
            if (token == null)
                throw new ArgumentNullException(nameof(token));
        }

        private Task AddCallback(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peer = instance.Peer as Peer;
                var peers = peer == null ? this.Peers : EnumerableUtility.One(peer);
                var service = instance.ServiceHost;
                foreach (var item in peers)
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
            });
        }

        private PollReplyItem[] Poll(Peer peer, IServiceHost service)
        {
            this.Dispatcher.VerifyAccess();
            var callbacks = peer.PollReplyItems[service];
            return callbacks.Flush();
        }

        #region IAdaptorHost

        Task IAdaptorHost.OpenAsync(string host, int port)
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
            return Task.Run(this.server.Start);
        }

        async Task IAdaptorHost.CloseAsync()
        {
            this.cancellation.Cancel();
            while (this.Peers.Any())
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

        IContainer<IPeer> IAdaptorHost.Peers => this.Peers;

        #endregion
    }
}
