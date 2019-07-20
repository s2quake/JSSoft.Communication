using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase : IServiecHost
    {
        private Grpc.Core.Server server;
        private ServiceInstanceBuilder instanceBuilder;
        protected ServiceHostBase()
        {
            this.Services = new ServiceCollection<ServiceBase>(this);
            this.Port = 4004;
        }

        public void Open()
        {
            this.server = new Grpc.Core.Server();
            this.instanceBuilder = new ServiceInstanceBuilder();
            // var channel = new Channel(this.Address, Grpc.Core.ChannelCredentials.Insecure);
            foreach (var item in this.Services)
            {
                item.Open(this.instanceBuilder);
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            this.OnClosed(EventArgs.Empty);
        }

        public ServiceCollection<ServiceBase> Services { get; }

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

        IEnumerable<IService> IServiecHost.Services => this.Services;

        #endregion
    }
}