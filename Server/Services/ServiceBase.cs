using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    public abstract class ServiceBase : IService
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        private CallbackBase callback;
        private Dispatcher dispatcher;
        private IAdaptorHost adaptorHost;

        public ServiceBase(Type serviceType, Type callbackType)
        {
            if (serviceType.IsAssignableFrom(this.GetType()) == false)
                throw new ArgumentException("invalid type", nameof(serviceType));
            this.Name = serviceType.Name;
            this.serviceType = serviceType;
            this.callbackType = callbackType;
            this.dispatcher = new Dispatcher(this);
        }

        Task<InvokeResult> IService.InvokeAsync(object context, InvokeInfo info)
        {
            throw new NotImplementedException();
        }

        Task<PollItem[]> IService.PollAsync(object context, int id)
        {
            return this.callback.PollAsync(id);
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
            this.dispatcher = null;
        }

        protected object Callback => this.callback;

        public Type ServiceType => this.serviceType;

        public Type CallbackType => this.callbackType;

        public void Open(ServiceToken token)
        {
            this.adaptorHost = token.AdaptorHost;
            this.callback = token.Callback as CallbackBase;
            this.OnOpened(EventArgs.Empty);
        }

        public void Close(ServiceToken token)
        {
            this.adaptorHost = null;
            this.callback = null;
            this.OnClosed(EventArgs.Empty);
        }

        public string Name {get;}

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
    }

    public abstract class ServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ServiceBase()
            : base(typeof(T), typeof(U))
        {

        }

        protected new U Callback => (U)base.Callback;
    }
}
