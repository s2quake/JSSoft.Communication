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
    class AdaptorServerImpl : Adaptor.AdaptorBase, IAdaptorServerHost
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
            var info = ToInvokeInfo(request);
            if (this.serviceByName.ContainsKey(info.ServiceName) == false)
                throw new InvalidOperationException();
            var service = this.serviceByName[info.ServiceName];
            var methodName = $"{info.ServiceName}.{info.Name}";
            if (this.methodByName.ContainsKey(methodName) == false)
                throw new InvalidOperationException();
            var method = methodByName[methodName];
            var value = await Task.Run(() => method.Invoke(service, info.Datas));
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
                ServiceName = info.ServiceName,
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

        public void InvokeDelegate(string serviceName, string name, object[] args)
        {
            this.dispatcher.InvokeAsync(() =>
            {
                var length = args.Length / 2;
                var callbacks = this.callbacksByName[serviceName];
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

        private static InvokeInfo ToInvokeInfo(InvokeRequest request)
        {
            var info = new InvokeInfo()
            {
                ServiceName = request.ServiceName,
                Name = request.Name,
                Types = new Type[request.Types_.Count],
                Datas = new object[request.Datas.Count]
            };
            for (var i = 0; i < request.Types_.Count; i++)
            {
                info.Types[i] = Type.GetType(request.Types_[i]);
                info.Datas[i] = JsonConvert.DeserializeObject(request.Datas[i], info.Types[i], settings);
            }
            return info;
        }

        private static InvokeReply ToInvokeReply(InvokeResult result)
        {
            var reply = new InvokeReply()
            {
                Type = result.Type.AssemblyQualifiedName,
                Data = JsonConvert.SerializeObject(result.Data, result.Type, settings)
            };
            return reply;
        }

        private static PollReply ToPollReply(PollItem[] results)
        {
            var replyItemList = new List<PollReplyItem>(results.Length);
            var reply = new PollReply();
            for (var i = 0; i < results.Length; i++)
            {
                var item = results[i];
                replyItemList.Add(ToPollReplyItem(item));
            }
            reply.Items.AddRange(replyItemList);
            return reply;
        }

        private static PollReplyItem ToPollReplyItem(PollItem pollItem)
        {
            var types = new string[pollItem.Types.Length];
            var datas = new string[pollItem.Datas.Length];
            var replyItem = new PollReplyItem()
            {
                Id = pollItem.ID,
                Name = pollItem.Name,
            };
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = pollItem.Types[i].AssemblyQualifiedName;
                datas[i] = JsonConvert.SerializeObject(pollItem.Datas[i], pollItem.Types[i], settings);
            }
            replyItem.Types_.AddRange(types);
            replyItem.Datas.AddRange(datas);
            return replyItem;
        }
    }
}