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
using JSSoft.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost
    {
        private readonly IServiceContext serviceContext;
        private readonly IInstanceContext instanceContext;
        private readonly IContainer<IServiceHost> serviceHosts;
        private CancellationTokenSource cancellation;
        private Task task;
        private Channel channel;
        private AdaptorClientImpl adaptorImpl;
        private ISerializer serializer;
        private PeerDescriptor descriptor;

        public AdaptorClientHost(IServiceContext serviceContext, IInstanceContext instanceContext)
        {
            this.serviceContext = serviceContext;
            this.instanceContext = instanceContext;
            this.serviceHosts = serviceContext.ServiceHosts;
        }

        public async Task OpenAsync(string host, int port)
        {
            try
            {
                await Task.Run(() =>
                {
                    this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
                    this.adaptorImpl = new AdaptorClientImpl(this.channel, Guid.NewGuid(), this.serviceHosts.ToArray());
                });
                await this.adaptorImpl.OpenAsync();
                this.descriptor = await this.instanceContext.CreateInstanceAsync(this.adaptorImpl);
                await Task.Run(() =>
                {
                    this.cancellation = new CancellationTokenSource();
                    this.serializer = this.serviceContext.GetService(typeof(ISerializer)) as ISerializer;
                });
                this.task = this.PollAsync(this.cancellation.Token);
            }
            catch
            {
                if (this.channel != null)
                {
                    await this.channel.ShutdownAsync();
                    this.channel = null;
                }
                throw;
            }
        }

        public async Task CloseAsync(int closeCode)
        {
            await Task.Run(() =>
            {
                this.cancellation?.Cancel();
                this.cancellation = null;
                this.task?.Wait();
                this.task = null;
            });
            if (this.adaptorImpl != null)
                await this.instanceContext.DestroyInstanceAsync(this.adaptorImpl);
            if (this.adaptorImpl != null)
                await this.adaptorImpl.CloseAsync();
            this.adaptorImpl = null;
            if (this.channel != null)
                await this.channel.ShutdownAsync();
            this.channel = null;
        }

        public event EventHandler<CloseEventArgs> Disconnected;

        protected virtual void OnDisconnected(CloseEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        private async Task PollAsync(CancellationToken cancellationToken)
        {
            var closeCode = int.MinValue;
            try
            {
                using var call = this.adaptorImpl.Poll();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var request = new PollRequest()
                    {
                        Token = $"{this.adaptorImpl.Token}"
                    };
                    await call.RequestStream.WriteAsync(request);
                    await call.ResponseStream.MoveNext();
                    var reply = call.ResponseStream.Current;
                    if (reply.Code != int.MinValue)
                    {
                        closeCode = reply.Code;
                        break;
                    }
                    this.InvokeCallback(reply.Items);
                    reply.Items.Clear();
                    await Task.Delay(1);
                }
                await call.RequestStream.CompleteAsync();
                await call.ResponseStream.MoveNext();
            }
            catch (Exception e)
            {
                closeCode = -1;
                GrpcEnvironment.Logger.Error(e, e.Message);
            }
            if (closeCode != int.MinValue)
            {
                this.task = null;
                await this.adaptorImpl.AbortAsync();
                this.adaptorImpl = null;
                this.OnDisconnected(new CloseEventArgs(closeCode));
            }
        }

        private void InvokeCallback(IServiceHost serviceHost, string name, string[] datas)
        {
            if (serviceHost.MethodDescriptors.ContainsKey(name) == false)
                throw new InvalidOperationException();
            var methodDescriptor = serviceHost.MethodDescriptors[name];
            var args = this.serializer.DeserializeMany(methodDescriptor.ParameterTypes, datas);
            var instance = this.descriptor.Callbacks[serviceHost];
            Task.Run(() => methodDescriptor.InvokeAsync(this.serviceContext, instance, args));
        }

        private void InvokeCallback(IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                var service = this.serviceHosts[item.ServiceName];
                this.InvokeCallback(service, item.Name, item.Datas.ToArray());
            }
        }

        private void ThrowException(Guid id, string data)
        {
            if (this.serviceContext.GetService(typeof(IComponentProvider)) is not IComponentProvider componentProvider)
            {
                throw new InvalidOperationException("can not get interface of IComponentProvider at serviceProvider");
            }
            var exceptionDescriptor = componentProvider.GetExceptionDescriptor(id);
            var exception = (Exception)this.serializer.Deserialize(exceptionDescriptor.ExceptionType, data);
            throw exception;
        }

        #region IAdaptorHost

        void IAdaptorHost.Invoke(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var token = $"{this.adaptorImpl.Token}";
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
                Token = token
            };
            request.Datas.AddRange(datas);
            var reply = this.adaptorImpl.Invoke(request);
            var id = Guid.Parse(reply.ID);
            if (id != Guid.Empty)
            {
                this.ThrowException(id, reply.Data);
            }
        }

        T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var token = $"{this.adaptorImpl.Token}";
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
                Token = token
            };
            request.Datas.AddRange(datas);
            var reply = this.adaptorImpl.Invoke(request);
            var id = Guid.Parse(reply.ID);
            if (id != Guid.Empty)
            {
                this.ThrowException(id, reply.Data);
            }
            return (T)this.serializer.Deserialize(typeof(T), reply.Data);
        }

        async Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var token = $"{this.adaptorImpl.Token}";
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
                Token = token
            };
            request.Datas.AddRange(datas);
            var reply = await this.adaptorImpl.InvokeAsync(request);
            var id = Guid.Parse(reply.ID);
            if (id != Guid.Empty)
            {
                this.ThrowException(id, reply.Data);
            }
        }

        async Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, Type[] types, object[] args)
        {
            var token = $"{this.adaptorImpl.Token}";
            var datas = this.serializer.SerializeMany(types, args);
            var request = new InvokeRequest()
            {
                ServiceName = instance.ServiceName,
                Name = name,
                Token = token
            };
            request.Datas.AddRange(datas);
            var reply = await this.adaptorImpl.InvokeAsync(request);
            var id = Guid.Parse(reply.ID);
            if (id != Guid.Empty)
            {
                this.ThrowException(id, reply.Data);
            }
            return (T)this.serializer.Deserialize(typeof(T), reply.Data);
        }

        #endregion
    }
}
