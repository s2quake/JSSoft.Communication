using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorServerImpl : Adaptor.AdaptorBase, IContextInvoker
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly Dictionary<string, IService> serviceByName = new Dictionary<string, IService>();
        private readonly Dictionary<string, MethodInfo> methodByName = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<string, CallbackCollection> callbacksByName = new Dictionary<string, CallbackCollection>();
        private readonly Dispatcher dispatcher;

        public AdaptorServerImpl(IEnumerable<IService> services)
        {
            this.serviceByName = services.ToDictionary(item => item.Name);
            this.callbacksByName = services.ToDictionary(item => item.Name, item => new CallbackCollection(item));
            this.dispatcher = new Dispatcher(this);
            foreach (var item in services)
            {
                RegisterMethod(this.methodByName, item);
            }
        }

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.Run(() => new PingReply() { Time = DateTime.UtcNow.Ticks });
        }

        public override async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            if (this.serviceByName.ContainsKey(request.ServiceName) == false)
                throw new InvalidOperationException();
            var service = this.serviceByName[request.ServiceName];
            var methodName = $"{request.ServiceName}.{request.Name}";
            if (this.methodByName.ContainsKey(methodName) == false)
                throw new InvalidOperationException();

            var args = AdaptorUtility.GetArguments(request.Types_, request.Datas);
            var method = methodByName[methodName];
            var value = await Task.Run(() => method.Invoke(service, args));
            var valueType = method.ReturnType;
            if (value is Task task)
            {
                await task;
                var taskType = task.GetType();
                if (taskType.GetGenericArguments().Any() == true)
                {
                    var propertyInfo = taskType.GetProperty("Result");
                    value = propertyInfo.GetValue(task);
                    valueType = propertyInfo.PropertyType;
                }
                else
                {
                    value = null;
                    valueType = typeof(void);
                }
            }
            var reply = new InvokeReply()
            {
                ServiceName = request.ServiceName,
                Type = valueType.AssemblyQualifiedName,
            };
            if (valueType != typeof(void))
            {
                reply.Data = JsonConvert.SerializeObject(value, valueType, settings);
            }

            return reply;
        }

        public override async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var id = request.Id;
                var service = this.serviceByName[request.ServiceName];
                var items = await this.PollAsync(service, id);
                var reply = new PollReply() { ServiceName = request.ServiceName };
                reply.Items.AddRange(items);
                await responseStream.WriteAsync(reply);
            }
        }

        public Task<PollReplyItem[]> PollAsync(IService service, int id)
        {
            return this.dispatcher.InvokeAsync(() =>
            {
                var callbacks = this.callbacksByName[service.Name];
                var items = new PollReplyItem[callbacks.Count - id];
                for (var i = id; i < callbacks.Count; i++)
                {
                    items[i - id] = callbacks[i];
                }
                return items;
            });
        }

        private static void RegisterMethod(Dictionary<string, MethodInfo> methodByName, IService service)
        {
            var methods = service.ServiceType.GetMethods();
            foreach (var item in methods)
            {
                if (item.GetCustomAttribute(typeof(ServiceContractAttribute)) is ServiceContractAttribute attr)
                {
                    var methodName = attr.Name ?? item.Name;
                    methodByName.Add($"{service.Name}.{methodName}", item);
                }
            }
        }

        #region IContextInvoker

        void IContextInvoker.Invoke(IService service, string name, object[] args)
        {
            this.dispatcher.InvokeAsync(() =>
            {
                var length = args.Length / 2;
                var callbacks = this.callbacksByName[service.Name];
                var types = new string[length];
                var datas = new string[length];
                var pollItem = new PollReplyItem()
                {
                    Id = callbacks.Count,
                    Name = name,
                };
                for (var i = 0; i < length; i++)
                {
                    var type = (Type)args[i * 2 + 0];
                    var value = args[i * 2 + 1];
                    types[i] = type.AssemblyQualifiedName;
                    datas[i] = JsonConvert.SerializeObject(value, type, settings);
                }
                pollItem.Types_.AddRange(types);
                pollItem.Datas.AddRange(datas);
                callbacks.Add(pollItem);
            });
        }

        T IContextInvoker.Invoke<T>(IService service, string name, object[] args)
        {
            throw new NotImplementedException();
        }

        Task IContextInvoker.InvokeAsync(IService service, string name, object[] args)
        {
            throw new NotImplementedException();
        }

        Task<T> IContextInvoker.InvokeAsync<T>(IService service, string name, object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}