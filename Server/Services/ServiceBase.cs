using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    abstract class ServiceBase : Adaptor.AdaptorBase, IService
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        private object callback;


        public ServiceBase(Type serviceType, Type callbackType)
        {
            this.serviceType = serviceType;
            this.callback = callbackType;
        }
        
        public override Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public object Callback => this.callback;
        internal ServerServiceDefinition Open(ServiceInstanceBuilder instanceBuilder)
        {
            var typeName = $"{this.callbackType.Name}Impl";
            var typeNamespace = this.callbackType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, this.callbackType);
            this.callback = TypeDescriptor.CreateInstance(null, implType, null, null);
        }
    }
    abstract class ServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ServiceBase()
            : base(typeof(T), typeof(U))
        {

        }

        public new U Callback => (U)base.Callback;
    }
}