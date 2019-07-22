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
    abstract class ServiceBase : IService
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        private object service;
        //private readonly Dictionary<string, MethodInfo> methodInfoByName = new Dictionary<string, MethodInfo>();
        private CancellationTokenSource cancellation;
        private Task task;
        private Dispatcher dispatcher;

        private IAdaptorHost adaptorHost;

        public ServiceBase(Type serviceType, Type callbackType)
        {
            if (callbackType.IsAssignableFrom(this.GetType()) == false)
                throw new ArgumentException("invalid type", nameof(serviceType));
                this.Name = serviceType.Name;
            this.serviceType = serviceType;
            this.callbackType = callbackType;
            this.dispatcher = new Dispatcher(this);
        }

         public Task<InvokeResult> InvokeAsync(InvokeInfo info)
         {
             throw new NotImplementedException();
         }

        public void Dispose()
        {
            this.cancellation.Cancel();
            this.task?.Wait();
            this.task = null;
            this.dispatcher?.Dispose();
            this.dispatcher = null;
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        public Task<InvokeResult> InvokeAsync(object context, InvokeInfo info)
        {
            throw new NotImplementedException();
        }

        public Task<PollItem[]> PollAsync(object context, int id)
        {
            throw new NotImplementedException();
        }

        //protected abstract ClientBase Create(Channel channel);

        //protected abstract Task<PollReply> RequestAsync(PollRequest request, CancellationToken cancellation);

        // protected virtual void OnPollBegun()
        // {

        // }

        // protected virtual void OnPollEnded()
        // {

        // }

        public object Service => this.service;

        public Type ServiceType => this.serviceType;

        public Type CallbackType => this.callbackType;

        public string Name {get;}

        // private async Task PollAsync()
        // {
        //     var id = 0;
        //     this.adaptor.OnPollBegun();
        //     while (!this.cancellation.IsCancellationRequested)
        //     {
        //         var request = new PollRequest() { Id = id, };
        //         var replies = await this.RequestAsync(request, this.cancellation.Token);
        //         foreach (var item in replies.Items)
        //         {
        //             if (item.Id >= 0)
        //             {
        //                 this.InvokeMethod(item);
        //                 id = item.Id + 1;
        //             }
        //         }
        //     }
        //     this.adaptor.OnPollEnded();
        // }

        private void InvokeMethod(PollItem pollItem)
        {
            var dataList = new List<object>(pollItem.Datas.Length);
            var typeList = new List<Type>(pollItem.Datas.Length);
            for (var i = 0; i < pollItem.Datas.Length; i++)
            {
                var type = pollItem.Types[i];
                var data = pollItem.Datas[i];
                typeList.Add(type);
                dataList.Add(data);
            }
            this.InvokeMethod(pollItem.Name, typeList.ToArray(), dataList.ToArray());
        }

        private void InvokeMethod(string name, Type[] types, object[] args)
        {
            var methods = this.serviceType.GetMethods();
            var method = this.FindMethod(methods, name, types);
            if (method == null)
                throw new InvalidOperationException($"method not found: \"{name}\"");
            this.dispatcher.InvokeAsync(() => method.Invoke(this.service, args));
        }

        private MethodInfo FindMethod(MethodInfo[] methods, string name, Type[] types)
        {
            foreach (var item in methods)
            {
                if (item.Name != name && item.Name != $"{ this.serviceType.FullName}.{name}")
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



        public void Open(ServiceToken token)
        {
            this.cancellation = new CancellationTokenSource();
            this.dispatcher = new Dispatcher(this);
            this.adaptorHost = token.AdaptorHost;
            this.service = token.Client;
            //this.task = this.adaptorHost.PollAsync(this.InvokeMethod, this.cancellation.Token);
            this.OnOpened(EventArgs.Empty);
        }

        public void Close(ServiceToken token)
        {
            //this.cancellation.Cancel();
            //this.task.Wait();
            //this.task = null;
            this.dispatcher.Dispose();
            this.dispatcher = null;
            this.adaptorHost = null;
            this.service = null;
            this.OnClosed(EventArgs.Empty);
        }
    }
    abstract class ServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ServiceBase()
            : base(typeof(T), typeof(U))
        {

        }

        // protected override sealed ClientBase Create(Channel channel)
        // {
        //     return this.CreateClient(channel);
        // }

        // protected abstract T CreateClient(Channel channel);


        protected new T Service => base.Service as T;



    }
}