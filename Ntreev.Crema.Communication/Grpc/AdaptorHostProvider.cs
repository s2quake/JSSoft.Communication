using System;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Communication.Grpc
{
    [Export(typeof(IAdaptorHostProvider))]
    class AdaptorHostProvider : IAdaptorHostProvider
    {
        public IAdaptorHost Create(IServiceHost serviceHost, ServiceToken token)
        {
            if (serviceHost is ServerHostBase)
                return new AdaptorServerHost(serviceHost.Services);
            else if (serviceHost is ClientHostBase)
                return new AdaptorClientHost(serviceHost.Services);
            throw new NotImplementedException();
        }

        public string Name => "grpc";
    }
}