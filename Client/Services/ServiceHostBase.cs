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
            this.Host = "localhost";
            this.Port = 4004;
        }

        public void Open()
        {
            this.tokenByService = new Dictionary<IService, ServiceToken>(this.Services.Count);
            this.adaptorHost.Open("localhost", 4004);
            foreach (var item in this.Services)
            {
                var serviceType = item.ServiceType;
                var typeName = $"{serviceType.Name}Impl";
                var typeNamespace = serviceType.Namespace;
                var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(ContextBase), serviceType);
                var serviceInstance = TypeDescriptor.CreateInstance(null, implType, null, null) as ContextBase;
                var adaptor = this.adaptorHost.Create(item);
                serviceInstance.Adaptor = adaptor;
                (serviceInstance as Users.IUserService).LoginAsync("wer");
            }
            var adaptors = this.tokenByService.Values.Select(item => item.Adaptor);
            
            // foreach (var item in this.Services)
            // {
            //     var token = this.tokenByService[item];
            //     item.Open(token);
            // }
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