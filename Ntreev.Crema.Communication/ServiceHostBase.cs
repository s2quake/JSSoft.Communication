using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Grpc.Core;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceHostBase : IServiceHost
    {
        private readonly IAdaptorHost adaptorHost;
        private readonly ServiceInstanceBuilder instanceBuilder;
        private Dictionary<IService, ServiceToken> tokenByService;

        internal ServiceHostBase(IAdaptorHost adaptorHost, IEnumerable<IService> services)
        {
            this.adaptorHost = adaptorHost;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceCollection(this, services);
            this.Port = 4004;
        }

        public void Open()
        {
            this.tokenByService = new Dictionary<IService, ServiceToken>(this.Services.Count);
            this.adaptorHost.Open("localhost", 4004);
            foreach (var item in this.Services)
            {
                var instance = this.adaptorHost.CreateInstance(item);
                var token = new ServiceToken(this.adaptorHost, instance);
                this.tokenByService.Add(item, token);
                item.Open(token);
            }
            
            foreach (var item in this.Services)
            {
                var token = this.tokenByService[item];
                
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
                if (item.Instance is IDisposable disposable)
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
