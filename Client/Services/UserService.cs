using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services.Users
{
    class UserService : IUserServiceCallback, IDisposable
    {
        private readonly IUserContextService.IUserContextServiceClient client;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly object instanceContext;
        private readonly Dictionary<string, MethodInfo> methodInfoByName = new Dictionary<string, MethodInfo>();
        private Task task;
        private Dispatcher dispatcher;

        public UserService(IUserContextService.IUserContextServiceClient client)
            : this(client, null)
        {
        }

        public UserService(IUserContextService.IUserContextServiceClient client, object instanceContext)
        {
            this.client = client;
            this.instanceContext = instanceContext ?? this;
            this.dispatcher = new Dispatcher(this);
            this.task = this.PollAsync();
        }

        public void Dispose()
        {
            this.cancellation.Cancel();
            this.task.Wait();
            this.dispatcher.Dispose();
        }

        void IUserServiceCallback.OnLoggedIn(string userID)
        {
            Console.WriteLine(userID);
        }

        private async Task PollAsync()
        {
            var call = this.client.Poll();
            var id = 0;
            while (!this.cancellation.IsCancellationRequested)
            {
                var request = new PollRequest() { Id = id, };
                await call.RequestStream.WriteAsync(request);
                await call.ResponseStream.MoveNext(this.cancellation.Token);
                var replies = call.ResponseStream.Current;
                foreach (var item in replies.Items)
                {
                    if (item.Id >= 0)
                    {
                        this.InvokeMethod(item);
                        id = item.Id + 1;
                    }
                }
            }
        }

        private void InvokeMethod(PollReplyItem replyItem)
        {
            var argList = new List<object>(replyItem.Data.Count);
            var typeList = new List<Type>(replyItem.Data.Count);
            for (var i = 0; i < replyItem.Data.Count; i++)
            {
                var typeName = replyItem.Type[i];
                var data = replyItem.Data[i];
                var type = Type.GetType(typeName);
                var obj = JsonConvert.DeserializeObject(data, type);
                typeList.Add(type);
                argList.Add(obj);
            }
            this.InvokeMethod(replyItem.Name, typeList.ToArray(), argList.ToArray());
        }

        private void InvokeMethod(string name, Type[] types, object[] args)
        {
            var methods = this.instanceContext.GetType().GetInterfaceMap(typeof(IUserServiceCallback)).TargetMethods;
            var method = this.FindMethod(methods, name, types);
            if (method == null)
                throw new InvalidOperationException($"method not found: \"{name}\"");
            this.dispatcher.InvokeAsync(() => method.Invoke(this.instanceContext, args));
        }

        private MethodInfo FindMethod(MethodInfo[] methods, string name, Type[] types)
        {
            foreach (var item in methods)
            {
                if (item.Name != name && item.Name != $"{typeof(IUserServiceCallback).FullName}.{name}")
                    continue;

                var parameters = item.GetParameters();
                if (parameters.Length != types.Length)
                    continue;

                var isSame = true;
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != types[i])
                    {
                        isSame = false;
                    }
                }
                if (isSame == true)
                {
                    return item;
                }
            }
            return null;
        }
    }
}
