using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase : IServiceHost
    {
        private readonly IAdaptorHost adaptorHost;
        private readonly ServiceInstanceBuilder instanceBuilder;
        private Dictionary<IService, ServiceToken> tokenByService;

        protected ServiceHostBase(IAdaptorHost adaptorHost, IEnumerable<IService> services)
        {
            this.adaptorHost = adaptorHost;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceCollection(this, services);
            this.Port = 4004;
        }

        public void Open()
        {
            this.tokenByService = new Dictionary<IService, ServiceToken>(this.Services.Count);
            foreach (var item in this.Services)
            {
                var callbackType = item.CallbackType;
                var typeName = $"{callbackType.Name}Impl";
                var typeNamespace = callbackType.Namespace;
                var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(CallbackBase), callbackType);
                var callback = TypeDescriptor.CreateInstance(null, implType, null, null);
                var token = new ServiceToken(this.adaptorHost, callback);
                this.tokenByService.Add(item, token);
            }
            this.adaptorHost.Open("localhost", 4004);
            foreach (var item in this.Services)
            {
                var token = this.tokenByService[item];
                item.Open(token);
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            foreach (var item in this.Services)
            {
                item.Close(ServiceToken.Empty);
            }
            foreach (var item in this.tokenByService.Values)
            {
                if (item.Callback is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            this.adaptorHost.Close();
            this.OnClosed(EventArgs.Empty);
        }

        public void Dispose()
        {
            foreach (var item in this.Services)
            {
                item.Dispose();
            }
        }

        public ServiceCollection Services { get; }

        public int Port { get; set; }

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

        #region IServiecHost

        IReadOnlyList<IService> IServiceHost.Services => this.Services;

        #endregion
    }
}
