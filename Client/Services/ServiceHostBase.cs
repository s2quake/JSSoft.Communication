using System;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    abstract class ServiceHostBase
    {
        private Channel channel;
        protected ServiceHostBase()
        {
            this.Services = new ServiceCollection(this);
            this.Address = "localhost:4004";
        }

        public void Open()
        {
            var channel = new Channel(this.Address, Grpc.Core.ChannelCredentials.Insecure);
            foreach (var item in this.Services)
            {
                item.Open(channel);
            }
            this.OnOpened(EventArgs.Empty);
        }

        public void Close()
        {
            this.OnClosed(EventArgs.Empty);
        }

        public ServiceCollection Services { get; }

        public string Address { get; set; }

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