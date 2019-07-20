using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase : IServiceHost
    {
        private Grpc.Core.Server server;
        private ServiceInstanceBuilder instanceBuilder;

        protected ServiceHostBase(IEnumerable<IService> services)
        {
            this.Services = new ServiceCollection(this);
            this.Port = 4004;
            this.instanceBuilder = new ServiceInstanceBuilder();
        }

        public void Open()
        {
            this.server = new Grpc.Core.Server()
            {
                Ports = { new Grpc.Core.ServerPort("localhost", 4004, Grpc.Core.ServerCredentials.Insecure) }
            };
            foreach (var item in this.Services)
            {
                var service = item.Open(this.instanceBuilder);
            }
            this.server.Start();
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
