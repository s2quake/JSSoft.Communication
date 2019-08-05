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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using Ntreev.Library.Linq;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorServerHost : IAdaptorHost
    {
        private readonly IServiceContext serviceContext;
        private readonly IContainer<IServiceHost> serviceHosts;
        private readonly HashSet<string> peerHashes = new HashSet<string>();
        private ILogger logger;
        private CancellationTokenSource cancellation;
        private Server server;
        private AdaptorServerImpl adaptor;
        private ISerializer serializer;

        public AdaptorServerHost(IServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.serviceHosts = serviceContext.ServiceHosts;
            this.logger = GrpcEnvironment.Logger;
        }

        internal async Task<OpenReply> Open(OpenRequest request, ServerCallContext context)
        {
            var token = await this.Dispatcher.InvokeAsync(() =>
            {
                var serviceNames = request.ServiceNames;
                var serviceHosts = serviceNames.Select(item => this.serviceHosts[item]).ToArray();
                var peer = context.Peer;
                var peerDescriptor = new PeerDescriptor(peer, serviceHosts);
                this.Peers.Add(peerDescriptor);
                return peerDescriptor.Token;
            });
            this.logger.Debug($"Connected: {context.Peer}");
            return new OpenReply() { Token = $"{token}" };
        }

        internal async Task<CloseReply> Close(CloseRequest request, ServerCallContext context)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                var peerDescriptor = this.Peers[context.Peer];
                peerDescriptor.Dispose();
            });
            this.logger.Debug($"Disconnected: {context.Peer}");
            return new CloseReply();
        }

        internal Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peerDescriptor = this.Peers[context.Peer];
                peerDescriptor.Ping = DateTime.UtcNow;
                return new PingReply() { Time = peerDescriptor.Ping.Ticks };
            });
        }

        internal async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            if (this.serviceHosts.ContainsKey(request.ServiceName) == false)
                throw new InvalidOperationException();
            var service = this.serviceHosts[request.ServiceName];
            if (service.Methods.ContainsKey(request.Name) == false)
                throw new InvalidOperationException();

            var methodDescriptor = service.Methods[request.Name];
            var peerDescriptor = this.Peers[context.Peer];
            var instance = peerDescriptor.Services[service];
            var args = this.serializer.DeserializeMany(methodDescriptor.ParameterTypes, request.Datas.ToArray());
            var (code, valueType, value) = await methodDescriptor.InvokeAsync(this.serviceContext, instance, args);
            var reply = new InvokeReply()
            {
                Code = code,
                Data = this.serializer.Serialize(valueType, value)
            };
            return reply;
        }

        internal async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            var cancellationToken = this.cancellation.Token;
            this.peerHashes.Add(context.Peer);
            var peerDescriptor = await this.Dispatcher.InvokeAsync(() => this.Peers[context.Peer]);
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var services = peerDescriptor.ServiceHosts;
                if (this.cancellation.IsCancellationRequested == true)
                {
                    await responseStream.WriteAsync(new PollReply() { Code = -1 });
                    break;
                }
                var reply = new PollReply();
                await this.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var item in services)
                    {
                        var items = this.Poll(peerDescriptor, item);
                        reply.Items.AddRange(items);
                    }
                });
                await responseStream.WriteAsync(reply);
            }
            this.peerHashes.Remove(context.Peer);
        }

        public void Dispose()
        {

        }

        public PeerCollection Peers { get; } = new PeerCollection();

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
            var pollItem = new PollReplyItem()
            {
                Name = name,
                ServiceName = instance.ServiceName
            };
            return this.Dispatcher.InvokeAsync(() =>
            {
                var peerDescriptor = instance.Peer as PeerDescriptor;
                var peers = peerDescriptor == null ? this.Peers : EnumerableUtility.One(peerDescriptor);
                var service = instance.ServiceHost;
                foreach (var item in peers)
                {
                    var callbacks = item.PollReplyItems[service];
                    pollItem.Datas.AddRange(datas);
                    callbacks.Add(pollItem);
                }
            });
        }

        private PollReplyItem[] Poll(PeerDescriptor peerDescriptor, IServiceHost service)
        {
            this.Dispatcher.VerifyAccess();
            var callbacks = peerDescriptor.PollReplyItems[service];
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
            this.cancellation = new CancellationTokenSource();
            this.serializer = this.serviceContext.GetService(typeof(ISerializer)) as ISerializer;
            return Task.Run(this.server.Start);
        }

        async Task IAdaptorHost.CloseAsync()
        {
            this.adaptor = null;
            this.cancellation.Cancel();
            while (this.peerHashes.Any())
            {
                await Task.Delay(1);
            }
            this.serializer = null;
            await this.server.ShutdownAsync();
            this.server = null;
            await this.Dispatcher.DisposeAsync();
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