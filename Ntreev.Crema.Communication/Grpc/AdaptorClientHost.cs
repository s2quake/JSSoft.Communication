using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost
    {
        private IService[] services;
        private Channel channel;
        private AdaptorClientImpl apdatorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorClientHost(IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public void Open(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.apdatorImpl = new AdaptorClientImpl(this.channel, this.services);
        }

        public void Close()
        {
            this.apdatorImpl.Dispose();
            this.apdatorImpl = null;
            this.channel.ShutdownAsync().Wait();
            this.channel = null;
        }

        public object Create(IService service)
        {
            var instanceType = service.ServiceType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(ContextBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as ContextBase;
            instance.Service = service;
            instance.Invoker = this.apdatorImpl;
            return instance;
        }

        public void Dispose()
        {

        }
    }
}