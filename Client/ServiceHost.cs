using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Ntreev.Crema.Services;

namespace Client
{
    [Export(typeof(IServiceHost))]
    class ServiceHost : ServiceHostBase
    {
        [ImportingConstructor]
        public ServiceHost(IAdaptorHost adaptorHost, [ImportMany]IEnumerable<IService> services)
            : base(adaptorHost, services)
        {
     
        }
    }
}