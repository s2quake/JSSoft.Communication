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

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost, IContextInvoker
    {
        private readonly Dictionary<string, IService> serviceByName = new Dictionary<string, IService>();
        private readonly Dictionary<Type, IExceptionSerializer> exceptionSerializerByType = new Dictionary<Type, IExceptionSerializer>();
        private readonly Dictionary<int, IExceptionSerializer> exceptionSerializerByCode = new Dictionary<int, IExceptionSerializer>();
        private readonly Dictionary<string, MethodDescriptor> methodDescriptorByName = new Dictionary<string, MethodDescriptor>();
        // private readonly Dictionary<IService, int> idByService = new Dictionary<IService, int>();
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;
        private string token;

        private Channel channel;
        private AdaptorClientImpl adaptorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();
        private readonly Dictionary<IService, object> serviceInstanceByService = new Dictionary<IService, object>();
        private readonly Dictionary<IService, object> callbackInstanceByService = new Dictionary<IService, object>();

        public AdaptorClientHost(IEnumerable<IService> services, IEnumerable<IExceptionSerializer> exceptionSerializers)
        {
            this.serviceByName = services.ToDictionary(item => item.Name);
            this.exceptionSerializerByType = exceptionSerializers.ToDictionary(item => item.ExceptionType);
            this.exceptionSerializerByCode = exceptionSerializers.ToDictionary(item => item.ExceptionCode);
            // this.idByService = services.ToDictionary(item => item, item => 0);
            foreach (var item in services)
            {
                RegisterMethod(this.methodDescriptorByName, item);
            }
        }

        public Task OpenAsync(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.adaptorImpl = new AdaptorClientImpl(this.channel);

            var request = new OpenRequest() { Time = DateTime.UtcNow.Ticks };
            request.ServiceNames.AddRange(this.serviceByName.Keys);
            var reply = this.adaptorImpl.Open(request);
            this.token = reply.Token;

            this.cancellation = new CancellationTokenSource();
            foreach (var item in this.serviceByName.Values)
            {
                var serviceInstance = this.Create(item);
                var callbackInstance = item.CreateInstance(serviceInstance);
                this.serviceInstanceByService.Add(item, serviceInstance);
                this.callbackInstanceByService.Add(item, callbackInstance);
            }

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
            this.callbackInstanceByService.DisposeAll();
            if (this.adaptorImpl != null)
                await this.adaptorImpl.CloseAsync(new CloseRequest() { Token = this.token });
            this.token = null;
            this.adaptorImpl = null;
            await this.channel.ShutdownAsync();
            this.channel = null;
        }

        public object Create(IService service)
        {
            var instanceType = service.ServiceType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(InstanceBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as InstanceBase;
            instance.Service = service;
            instance.Invoker = this;
            return instance;
        }

        public void Dispose()
        {

        }

        public event EventHandler<DisconnectionReasonEventArgs> Disconnected;

        protected virtual void OnDisconnected(DisconnectionReasonEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

        public event EventHandler<PeerEventArgs> PeerAdded;
        public event EventHandler<PeerEventArgs> PeerRemoved;

        // private void AdaptorImpl_Disconnected(object sender, DisconnectionReasonEventArgs e)
        // {
        //     this.adaptorImpl = null;
        //     this.Disconnected?.Invoke(this, e);
        // }

        private static void RegisterMethod(Dictionary<string, MethodDescriptor> methodDescriptorByName, IService service)
        {
            var methods = service.CallbackType.GetMethods();
            foreach (var item in methods)
            {
                if (item.GetCustomAttribute(typeof(OperationContractAttribute)) is OperationContractAttribute attr)
                {
                    var methodName = attr.Name ?? item.Name;
                    var methodDescriptor = new MethodDescriptor(item);
                    methodDescriptorByName.Add(methodDescriptor.Name, methodDescriptor);
                }
            }
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
                            var service = this.serviceByName[item.ServiceName];
                            this.InvokeCallback(service, item.Id, reply.Items);
                            this.idByService[service] = item.Id;
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

        private void InvokeCallback(IService service, string name, IReadOnlyList<string> datas)
        {
            if (this.methodDescriptorByName.ContainsKey(name) == false)
                throw new InvalidOperationException();
            var methodDescriptor = this.methodDescriptorByName[name];
            methodDescriptor.Invoke(service, datas);
        }

        private int InvokeCallback(IService service, int id, IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                if (item.Id >= 0)
                {
                    this.InvokeCallback(service, item.Name, item.Datas);
                    id = item.Id + 1;
                }
            }
            return id;
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

        #region IContextInvoker

        void IContextInvoker.Invoke(InstanceBase instance, string name, object[] args)
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

        T IContextInvoker.Invoke<T>(InstanceBase instance, string name, object[] args)
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

        async Task IContextInvoker.InvokeAsync(InstanceBase instance, string name, object[] args)
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

        async Task<T> IContextInvoker.InvokeAsync<T>(InstanceBase instance, string name, object[] args)
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