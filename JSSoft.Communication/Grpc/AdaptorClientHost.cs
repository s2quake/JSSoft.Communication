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
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Library.ObjectModel;

namespace JSSoft.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost
    {
        private readonly IServiceContext serviceContext;
        private readonly IContainer<IServiceHost> serviceHosts;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;
        private Channel channel;
        private AdaptorClientImpl adaptorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();
        private ISerializer serializer;
        private PeerCollectionSurrogate peers;

        public AdaptorClientHost(IServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
            this.serviceHosts = serviceContext.ServiceHosts;
            this.peers = new PeerCollectionSurrogate();
        }

        public async Task OpenAsync(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.adaptorImpl = new AdaptorClientImpl(this.channel, host, this.serviceHosts.ToArray());
            await this.adaptorImpl.OpenAsync();
            this.peers.Set(this.adaptorImpl);
            this.cancellation = new CancellationTokenSource();
            this.serializer = this.serviceContext.GetService(typeof(ISerializer)) as ISerializer;
            this.task = this.PollAsync(this.cancellation.Token);
        }

        public async Task CloseAsync()
        {
            this.peers.Unset();
            if (this.adaptorImpl != null)
                await this.adaptorImpl.CloseAsync();
            this.cancellation.Cancel();
            this.cancellation = null;
            this.task?.Wait();
            this.task = null;
            this.adaptorImpl = null;
            await this.channel.ShutdownAsync();
            this.channel = null;
        }

        public event EventHandler<DisconnectionReasonEventArgs> Disconnected;

        protected virtual void OnDisconnected(DisconnectionReasonEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        private async Task PollAsync(CancellationToken cancellationToken)
        {
            var exitCode = 0;
            try
            {
                using (this.call = this.adaptorImpl.Poll())
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var request = new PollRequest();
                        await this.call.RequestStream.WriteAsync(request);
                        await this.call.ResponseStream.MoveNext();
                        var reply = this.call.ResponseStream.Current;
                        if (reply.Code != 0)
                        {
                            exitCode = reply.Code;
                            break;
                        }
                        foreach (var item in reply.Items)
                        {
                            var service = this.serviceHosts[item.ServiceName];
                            this.InvokeCallback(service, reply.Items);
                        }
                        reply.Items.Clear();
                        await Task.Delay(1);
                    }
                    await this.call.RequestStream.CompleteAsync();
                    await this.call.ResponseStream.MoveNext();
                }
                this.call = null;
            }
            catch (Exception e)
            {
                exitCode = 1;
                GrpcEnvironment.Logger.Error(e, e.Message);
            }
            if (exitCode != 0)
            {
                this.task = null;
                this.adaptorImpl = null;
                this.OnDisconnected(new DisconnectionReasonEventArgs(exitCode));
            }
        }

        private async void InvokeCallback(IServiceHost serviceHost, string name, string[] datas)
        {
            if (serviceHost.MethodDescriptors.ContainsKey(name) == false)
                throw new InvalidOperationException();
            var methodDescriptor = serviceHost.MethodDescriptors[name];
            var args = this.serializer.DeserializeMany(methodDescriptor.ParameterTypes, datas);
            var instance = this.adaptorImpl.Callbacks[serviceHost];
            await methodDescriptor.InvokeAsync(this.serviceContext, instance, args);
        }

        private void InvokeCallback(IServiceHost service, IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                this.InvokeCallback(service, item.Name, item.Datas.ToArray());
            }
        }

        private void ThrowException(int code, string data)
        {
            var componentProvider = this.serviceContext.GetService(typeof(IComponentProvider)) as IComponentProvider;
            if (componentProvider == null)
            {
                throw new InvalidOperationException("can not get interface of IComponentProvider at serviceProvider");
            }
            var exceptionDescriptor = componentProvider.GetExceptionDescriptor(code);
            var exception = (Exception)this.serializer.Deserialize(exceptionDescriptor.ExceptionType, data);
            throw exception;
        }

        #region IAdaptorHost

        void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = this.adaptorImpl.Invoke(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
        }

        T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = this.adaptorImpl.Invoke(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
            return (T)this.serializer.Deserialize(typeof(T), reply.Data);
        }

        async Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = await this.adaptorImpl.InvokeAsync(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
        }

        async Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = await this.adaptorImpl.InvokeAsync(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
            return (T)this.serializer.Deserialize(typeof(T), reply.Data);
        }

        IContainer<IPeer> IAdaptorHost.Peers => this.peers;

        #endregion
    }
}
