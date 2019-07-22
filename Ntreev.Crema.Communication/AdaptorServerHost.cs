using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Communication
{
    class AdaptorServerHost : IAdaptorHost
    {
        private IService[] services;
        private Grpc.Core.Server server;
        private AdaptorServerImpl adaptor;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorServerHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public void Open(string host, int port)
        {
            this.adaptor = new AdaptorServerImpl(this.services);
            this.server = new Grpc.Core.Server()
            {
                Services = { Adaptor.BindService(this.adaptor) },
                Ports = { new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure) },
            };
            this.server.Start();
        }

        public void Close()
        {
            this.server.ShutdownAsync().Wait();
            this.server = null;
        }

        public object CreateInstance(IService service)
        {
            var instanceType = service.CallbackType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(CallbackBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as CallbackBase;
            instance.Service = service;
            return instance;
        }
    }
}