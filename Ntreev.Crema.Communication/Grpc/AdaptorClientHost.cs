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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;
using Ntreev.Library.ObjectModel;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost
    {
        private readonly IServiceContext serviceContext;
        private readonly IContainer<IServiceHost> services;
        private readonly Dictionary<Type, IExceptionSerializer> exceptionSerializerByType = new Dictionary<Type, IExceptionSerializer>();
        private readonly Dictionary<int, IExceptionSerializer> exceptionSerializerByCode = new Dictionary<int, IExceptionSerializer>();
        private readonly Dictionary<IServiceHost, object> serviceInstanceByService = new Dictionary<IServiceHost, object>();
        private readonly Dictionary<IServiceHost, object> callbackInstanceByService = new Dictionary<IServiceHost, object>();
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;
        private string token;
        private Channel channel;
        private AdaptorClientImpl adaptorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorClientHost(IServiceContext serviceContext, IEnumerable<IExceptionSerializer> exceptionSerializers)
        {
            this.serviceContext = serviceContext;
            this.services = serviceContext.Services;
            this.exceptionSerializerByType = exceptionSerializers.ToDictionary(item => item.ExceptionType);
            this.exceptionSerializerByCode = exceptionSerializers.ToDictionary(item => item.ExceptionCode);
        }

        public Task OpenAsync(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.adaptorImpl = new AdaptorClientImpl(this.channel);

            var request = new OpenRequest() { Time = DateTime.UtcNow.Ticks };
            request.ServiceNames.AddRange(this.services.Keys);
            var reply = this.adaptorImpl.Open(request);
            this.token = reply.Token;

            this.cancellation = new CancellationTokenSource();
          
            this.task = this.PollAsync(this.cancellation.Token);

            // this.adaptorImpl.Disconnected += AdaptorImpl_Disconnected;
            return Task.Delay(1);
        }

        public async Task CloseAsync()
        {
            this.cancellation.Cancel();
            this.cancellation = null;
            this.task.Wait();
            this.task = null;
            this.serviceInstanceByService.DisposeAll();
            if (this.adaptorImpl != null)
                await this.adaptorImpl.CloseAsync(new CloseRequest() { Token = this.token });
            this.token = null;
            this.adaptorImpl = null;
            await this.channel.ShutdownAsync();
            this.channel = null;
        }

        public void Dispose()
        {

        }

        public IContainer<IPeer> Peers => throw new NotImplementedException();

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
                            var service = this.services[item.ServiceName];
                            this.InvokeCallback(service, reply.Items);
                        }
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
                this.adaptorImpl = null;
                this.OnDisconnected(new DisconnectionReasonEventArgs(exitCode));
            }
        }

        private void InvokeCallback(IServiceHost serviceHost, string name, IReadOnlyList<string> datas)
        {
            if (serviceHost.Methods.ContainsKey(name) == false)
                throw new InvalidOperationException();
            var methodDescriptor = serviceHost.Methods[name];
            methodDescriptor.Invoke(serviceHost, datas);
        }

        private void InvokeCallback(IServiceHost service, IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                this.InvokeCallback(service, item.Name, item.Datas);
            }
        }

        private void ThrowException(int code, string data)
        {
            var serializer = this.GetExceptionSerializer(code);
            var exception = (Exception)serializer.Deserialize(data);
            throw exception;
        }

        private IExceptionSerializer GetExceptionSerializer(int exceptionCode)
        {
            if (this.exceptionSerializerByCode.ContainsKey(exceptionCode) == true)
            {
                return this.exceptionSerializerByCode[exceptionCode];
            }
            return this.exceptionSerializerByCode[-1];
        }

        #region IAdaptorHost

        void IAdaptorHost.Invoke(InstanceBase instance, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
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

        T IAdaptorHost.Invoke<T>(InstanceBase instance, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
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
            return SerializerUtility.GetValue<T>(reply.Data);
        }

        async Task IAdaptorHost.InvokeAsync(InstanceBase instance, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
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

        async Task<T> IAdaptorHost.InvokeAsync<T>(InstanceBase instance, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
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
            return SerializerUtility.GetValue<T>(reply.Data);
        }

        #endregion

    }
}