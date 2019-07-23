using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;

namespace Ntreev.Crema.Communication
{
    class AdaptorClientHost : IAdaptorHost
    {
        private IService[] services;
        private Channel channel;
        private AdaptorClientImpl apdatorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        [ImportingConstructor]
        public AdaptorClientHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public void Open(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.apdatorImpl = new AdaptorClientImpl(this.channel, this.services);
            //this.Ports.Add(new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure));
            // foreach (var item in adaptors)
            // {
            //     // if (item is AdaptorImpl adator)
            //     // {
            //     //     //this.Services.Add(Adaptor.BindService(adator));
            //     // }
            //     // else
            //     // {
            //     //     throw new ArgumentException("item is invalid adator", nameof(adaptors));
            //     // }
            // }
            //this.channel.s
        }

        public void Close()
        {
            this.channel.ShutdownAsync().Wait();
            this.channel = null;
        }

        public object CreateInstance(IService service)
        {
            var instanceType = service.ServiceType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(ContextBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as ContextBase;
            instance.InvokeDelegate = this.apdatorImpl.InvokeAsync;
            instance.ServiceName = service.Name;
            return instance;
        }
    }
}