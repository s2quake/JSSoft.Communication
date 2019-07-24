using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Ntreev.Crema.Communication;

namespace Client
{
    [Export(typeof(IServiceHost))]
    class ServiceHost : ClientHostBase
    {
        [ImportingConstructor]
        public ServiceHost(IAdaptorHostProvider adaptorHostProvider, [ImportMany]IEnumerable<IService> services)
            : base(adaptorHostProvider, services)
        {
     
        }
    }
}