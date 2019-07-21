using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase : IServiceHost
    {
        private readonly IAdaptorHost adaptorHost;
        protected ServiceHostBase(IAdaptorHost adaptorHost, IEnumerable<IService> services)
        {
            this.adaptorHost = adaptorHost;
            this.Services = new ServiceCollection(this, services);
            this.Host = "localhost";
            this.Port = 4004;
        }

        public void Open()
        {
            //var channel = new Channel(this.Address, Grpc.Core.ChannelCredentials.Insecure);
            foreach (var item in this.Services)
            {
                
                //item.Open(channel);
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            this.OnClosed(EventArgs.Empty);
        }

        public ServiceCollection Services { get; }

        public string Host { get; set; }

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
        
#region IServiceHost

        IReadOnlyList<IService> IServiceHost.Services => this.Services;

        #endregion
    }
}