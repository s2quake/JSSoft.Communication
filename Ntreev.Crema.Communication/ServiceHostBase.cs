using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Grpc.Core;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceHostBase : IServiceHost
    {
        private readonly IAdaptorHostProvider adpatorHostProvider;
        private readonly ServiceInstanceBuilder instanceBuilder;
        private Dictionary<IService, object> instanceByService;
        private IAdaptorHost adaptorHost;

        internal ServiceHostBase(IAdaptorHostProvider adpatorHostProvider, IEnumerable<IService> services)
        {
            this.adpatorHostProvider = adpatorHostProvider;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceCollection(this, services);
            this.Port = 4004;
        }

        public void Open()
        {
            this.adaptorHost = this.adpatorHostProvider.Create(this, ServiceToken.Empty);
            this.instanceByService = new Dictionary<IService, object>(this.Services.Count);
            this.adaptorHost.Open("localhost", 4004);
            foreach (var item in this.Services)
            {
                var instance = this.adaptorHost.Create(item);
                this.instanceByService.Add(item, instance);
                item.Open(ServiceToken.Empty, instance);
            }
            
            foreach (var item in this.Services)
            {
                var token = this.instanceByService[item];
                
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            foreach (var item in this.Services)
            {
                var instance = this.instanceByService[item];
                item.Close(ServiceToken.Empty);
                if (instance is IDisposable disposable)
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
