using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase : IServiceHost
    {
        private IAdaptorHost adaptorHost;
        private ServiceInstanceBuilder instanceBuilder;

        protected ServiceHostBase(IAdaptorHost adaptorHost, IEnumerable<IService> services)
        {
            this.adaptorHost = adaptorHost;
            this.Services = new ServiceCollection(this);
            this.Port = 4004;
            this.instanceBuilder = new ServiceInstanceBuilder();
        }

        public void Open()
        {
            var tokenByService = new Dictionary<IService, ServiceToken>(this.Services.Count);
            foreach (var item in this.Services)
            {
                var callbackType = item.CallbackType;
                var typeName = $"{callbackType.Name}Impl";
                var typeNamespace = callbackType.Namespace;
                var implType = instanceBuilder.CreateType(typeName, typeNamespace, callbackType);
                var callback = TypeDescriptor.CreateInstance(null, implType, null, null) as CallbackBase;
                var adaptor = this.adaptorHost.Create(item, callback);
                var token = new ServiceToken(adaptor, callback);
                tokenByService.Add(item, token);
            }
            var adaptors = tokenByService.Values.Select(item => item.Adaptor);
            this.adaptorHost.Open("localhost", 4004, adaptors);
            foreach (var item in this.Services)
            {
                var token = tokenByService[item];
                item.Open(token);
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            foreach (var item in this.Services)
            {
                item.Close();
            }
            this.server.ShutdownAsync().Wait();
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

        IEnumerable<IService> IServiceHost.Services => this.Services;

        #endregion
    }
}
