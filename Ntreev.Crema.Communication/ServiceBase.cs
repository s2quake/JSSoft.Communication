using System;
using System.Threading.Tasks;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceBase : IService, IDisposable
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        private object instance;
        private Dispatcher dispatcher;

        internal ServiceBase(Type serviceType, Type callbackType, Type validationType)
        {
            if (validationType.IsAssignableFrom(this.GetType()) == false)
                throw new ArgumentException("invalid type", nameof(validationType));
            this.Name = serviceType.Name;
            this.serviceType = serviceType;
            this.callbackType = callbackType;
            this.dispatcher = new Dispatcher(this);
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
            this.dispatcher = null;
        }

        protected object Instance => this.instance;

        public Type ServiceType => this.serviceType;

        public Type CallbackType => this.callbackType;

        public void Open(ServiceToken token, object instance)
        {
            this.instance = instance;
            this.OnOpened(EventArgs.Empty);
        }

        public void Close(ServiceToken token)
        {
            //this.adaptorHost = null;
            this.instance = null;
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
}
