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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Newtonsoft.Json;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientImpl : Adaptor.AdaptorClient, IContextInvoker
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly Dictionary<string, IService> serviceByName = new Dictionary<string, IService>();
        private readonly Dictionary<Type, IExceptionSerializer> exceptionSerializerByType = new Dictionary<Type, IExceptionSerializer>();
        private readonly Dictionary<int, IExceptionSerializer> exceptionSerializerByCode = new Dictionary<int, IExceptionSerializer>();
        private readonly Dictionary<string, MethodDescriptor> methodDescriptorByName = new Dictionary<string, MethodDescriptor>();
        private readonly Dictionary<IService, int> idByService = new Dictionary<IService, int>();
        private readonly Channel channel;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;
        private string token;

        public AdaptorClientImpl(Channel channel, IEnumerable<IService> services, IEnumerable<IExceptionSerializer> exceptionSerializers)
            : base(channel)
        {
            this.serviceByName = services.ToDictionary(item => item.Name);
            this.exceptionSerializerByType = exceptionSerializers.ToDictionary(item => item.ExceptionType);
            this.exceptionSerializerByCode = exceptionSerializers.ToDictionary(item => item.ExceptionCode);
            this.idByService = services.ToDictionary(item => item, item => 0);
            var request = new OpenRequest() { Time = DateTime.UtcNow.Ticks };
            request.ServiceNames.AddRange(this.serviceByName.Keys);
            var reply = this.Open(request);
            this.token = reply.Token;
            this.channel = channel;

            this.cancellation = new CancellationTokenSource();
            foreach (var item in services)
            {
                RegisterMethod(this.methodDescriptorByName, item);
            }
            this.task = this.PollAsync(this.cancellation.Token);
        }

        public async Task DisposeAsync()
        {
            this.cancellation.Cancel();
            this.cancellation = null;
            this.task.Wait();
            this.task = null;
            await this.CloseAsync(new CloseRequest() { Token = this.token });
            this.token = null;
        }

        public event EventHandler<DisconnectionReasonEventArgs> Disconnected;

        protected virtual void OnDisconnected(DisconnectionReasonEventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }

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
                using (this.call = this.Poll())
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
                this.OnDisconnected(new DisconnectionReasonEventArgs(exitCode));
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

        void IContextInvoker.Invoke(IService service, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = this.Invoke(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
        }

        T IContextInvoker.Invoke<T>(IService service, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = this.Invoke(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
            return SerializerUtility.GetValue<T>(reply.Data);
        }

        async Task IContextInvoker.InvokeAsync(IService service, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = await this.InvokeAsync(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
        }

        async Task<T> IContextInvoker.InvokeAsync<T>(IService service, string name, object[] args)
        {
            var datas = SerializerUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Datas.AddRange(datas);
            var reply = await this.InvokeAsync(request);
            if (reply.Code != 0)
            {
                this.ThrowException(reply.Code, reply.Data);
            }
            return SerializerUtility.GetValue<T>(reply.Data);
        }

        #endregion
    }
}
