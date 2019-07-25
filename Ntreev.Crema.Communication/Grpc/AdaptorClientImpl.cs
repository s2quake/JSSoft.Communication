using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientImpl : Adaptor.AdaptorClient, IContextInvoker
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly Dictionary<string, IService> serviceByName = new Dictionary<string, IService>();
        private readonly Dictionary<string, MethodInfo> methodByName = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<IService, int> idByService = new Dictionary<IService, int>();
        private readonly Channel channel;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;

        public AdaptorClientImpl(Channel channel, IEnumerable<IService> services)
            : base(channel)
        {
            this.Ping(new PingRequest() { Time = DateTime.UtcNow.Ticks });
            this.channel = channel;
            this.serviceByName = services.ToDictionary(item => item.Name);
            this.idByService = services.ToDictionary(item => item, item => 0);
            this.cancellation = new CancellationTokenSource();
            foreach (var item in services)
            {
                RegisterMethod(this.methodByName, item);
            }
            this.task = this.PollAsync(this.cancellation.Token);
        }

        public void Dispose()
        {
            this.cancellation.Cancel();
            this.cancellation = null;
            this.task.Wait();
            this.task = null;
        }

        private static void RegisterMethod(Dictionary<string, MethodInfo> methodByName, IService service)
        {
            var methods = service.CallbackType.GetMethods();
            foreach (var item in methods)
            {
                if (item.GetCustomAttribute(typeof(ServiceContractAttribute)) is ServiceContractAttribute attr)
                {
                    var methodName = attr.Name ?? item.Name;
                    methodByName.Add($"{service.Name}.{methodName}", item);
                }
            }
        }

        private async Task PollAsync(CancellationToken cancellation)
        {
            var count = this.idByService.Count;
            this.call = this.Poll();
            while (!cancellation.IsCancellationRequested)
            {
                foreach (var item in this.serviceByName.Values)
                {
                    var id = this.idByService[item];
                    var request = new PollRequest() { Id = id, ServiceName = item.Name };
                    await this.call.RequestStream.WriteAsync(request);
                    await this.call.ResponseStream.MoveNext(cancellation);
                    var reply = this.call.ResponseStream.Current;
                    this.idByService[item] = this.InvokeCallback(item, id, reply.Items);
                    await Task.Delay(1);
                }
            }
            this.call.Dispose();
            this.call = null;
        }

        private void InvokeCallback(IService service, string name, object[] args)
        {
            var methodName = $"{service.Name}.{name}";
            if (this.methodByName.ContainsKey(methodName) == false)
                throw new InvalidOperationException();
            var methodInfo = this.methodByName[methodName];
            methodInfo.Invoke(service, args);
        }

        private int InvokeCallback(IService service, int id, IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                if (item.Id >= 0)
                {
                    var args = AdaptorUtility.GetArguments(item.Types_, item.Datas);
                    this.InvokeCallback(service, item.Name, args);
                    id = item.Id + 1;
                }
            }
            return id;
        }

        #region IContextInvoker

        void IContextInvoker.Invoke(IService service, string name, object[] args)
        {
            var (types, datas) = AdaptorUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Types_.AddRange(types);
            request.Datas.AddRange(datas);
            this.Invoke(request);
        }

        T IContextInvoker.Invoke<T>(IService service, string name, object[] args)
        {
            var (types, datas) = AdaptorUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Types_.AddRange(types);
            request.Datas.AddRange(datas);
            var reply = this.Invoke(request);
            return AdaptorUtility.GetValue<T>(reply.Type, reply.Data);
        }

        async Task IContextInvoker.InvokeAsync(IService service, string name, object[] args)
        {
            var (types, datas) = AdaptorUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Types_.AddRange(types);
            request.Datas.AddRange(datas);
            await this.InvokeAsync(request);
        }

        async Task<T> IContextInvoker.InvokeAsync<T>(IService service, string name, object[] args)
        {
            var (types, datas) = AdaptorUtility.GetStrings(args);
            var request = new InvokeRequest()
            {
                ServiceName = service.Name,
                Name = name,
            };
            request.Types_.AddRange(types);
            request.Datas.AddRange(datas);
            var reply = await this.InvokeAsync(request);
            return AdaptorUtility.GetValue<T>(reply.Type, reply.Data);
        }

        #endregion
    }
}
