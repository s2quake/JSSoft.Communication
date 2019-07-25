using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceHostBase : IServiceHost, IDisposable
    {
        private const string defaultHost = "localhost";
        private static readonly int defaultPort = 4004;
        private readonly IAdaptorHostProvider adpatorHostProvider;
        private readonly ServiceInstanceBuilder instanceBuilder;
        private readonly Dispatcher dispatcher;
        private Dictionary<IService, object> instanceByService;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = defaultPort;
        private bool isOpened;

        internal ServiceHostBase(IAdaptorHostProvider adpatorHostProvider, IEnumerable<IService> services)
        {
            this.adpatorHostProvider = adpatorHostProvider;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceCollection(this, services);
            this.dispatcher = new Dispatcher(this);
        }

        public async Task OpenAsync()
        {
            await this.dispatcher.InvokeAsync(() =>
            {
                this.adaptorHost = this.adpatorHostProvider.Create(this, ServiceToken.Empty);
                this.instanceByService = new Dictionary<IService, object>(this.Services.Count);
                this.adaptorHost.Open(this.Host, this.Port);
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
                this.isOpened = true;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            await this.dispatcher.InvokeAsync(() =>
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
                this.isOpened = false;
                this.OnClosed(EventArgs.Empty);
            });
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
        }

        public ServiceCollection Services { get; }

        public string Host
        {
            get => this.host ?? defaultHost;
            set
            {
                if (this.isOpened == true)
                    throw new InvalidOperationException("cannot set host when service is open.");
                this.host = value;
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (this.isOpened == true)
                    throw new InvalidOperationException("cannot set port when service is open.");
                this.port = value;
            }
        }

        public bool IsOpened => this.isOpened;

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
