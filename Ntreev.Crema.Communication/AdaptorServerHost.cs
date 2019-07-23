using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;

namespace Ntreev.Crema.Communication
{
    class AdaptorServerHost : IAdaptorHost
    {
        private IService[] services;
        private Server server;
        private AdaptorServerImpl adaptor;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorServerHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public void Open(string host, int port)
        {
            this.adaptor = new AdaptorServerImpl(this.services);
            this.server = new Server()
            {
                Services = { Adaptor.BindService(this.adaptor) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
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