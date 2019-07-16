using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    abstract class ServiceBase
    {
        private readonly Type instanceType;
        private readonly object instanceContext;

        private readonly Dictionary<string, MethodInfo> methodInfoByName = new Dictionary<string, MethodInfo>();
        private CancellationTokenSource cancellation;
        private Task task;
        private Dispatcher dispatcher;
        private ClientBase client;

        protected ServiceBase(Type instanceType, object instanceContext)
        {
            this.instanceType = instanceType ?? throw new ArgumentNullException(nameof(instanceType));
            if (this.instanceType.GetType().IsInterface == false)
                new ArgumentException($"{nameof(instanceType)} is not interface type.");
            this.instanceContext = instanceContext ?? this;
            if (this.instanceType.IsAssignableFrom(this.instanceContext.GetType()) == false)
                throw new ArgumentException($"{nameof(instanceContext)} must implement type {this.instanceType.Name}.");
        }

        public void Dispose()
        {
            this.cancellation.Cancel();
            this.task?.Wait();
            this.task = null;
            this.dispatcher?.Dispose();
            this.dispatcher = null;
        }

        protected virtual void OnOpened(EventArgs e)
        {

        }

        protected virtual void OnClosed(EventArgs e)
        {

        }

        protected abstract ClientBase Create(Channel channel);

        protected abstract Task<PollReply> RequestAsync(PollRequest request, CancellationToken cancellation);

        protected virtual void OnPollBegun()
        {

        }

        protected virtual void OnPollEnded()
        {

        }

        protected ClientBase Client => this.client;


        private async Task PollAsync()
        {
            var id = 0;
            this.OnPollBegun();
            while (!this.cancellation.IsCancellationRequested)
            {
                var request = new PollRequest() { Id = id, };
                var replies = await this.RequestAsync(request, this.cancellation.Token);
                foreach (var item in replies.Items)
                {
                    if (item.Id >= 0)
                    {
                        this.InvokeMethod(item);
                        id = item.Id + 1;
                    }
                }
            }
            this.OnPollEnded();
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
            var methods = this.instanceType.GetMethods();
            var method = this.FindMethod(methods, name, types);
            if (method == null)
                throw new InvalidOperationException($"method not found: \"{name}\"");
            this.dispatcher.InvokeAsync(() => method.Invoke(this.instanceContext, args));
        }

        private MethodInfo FindMethod(MethodInfo[] methods, string name, Type[] types)
        {
            foreach (var item in methods)
            {
                if (item.Name != name && item.Name != $"{ this.instanceType.FullName}.{name}")
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



        internal void Open(Channel channel)
        {
            this.cancellation = new CancellationTokenSource();
            this.dispatcher = new Dispatcher(this);
            this.client = this.Create(channel);
            this.task = this.PollAsync();
            this.OnOpened(EventArgs.Empty);
        }

        internal void Close()
        {
            this.OnClosed(EventArgs.Empty);
        }
    }
    abstract class ServiceBase<T, U> : ServiceBase where T : ClientBase where U : class
    {
        protected ServiceBase()
            : base(typeof(U), null)
        {

        }

        protected override sealed ClientBase Create(Channel channel)
        {
            return this.CreateClient(channel);
        }

        protected abstract T CreateClient(Channel channel);


        protected new T Client => base.Client as T;



    }
}