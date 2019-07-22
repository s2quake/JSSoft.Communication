using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Ntreev.Crema.Communication;

namespace Server
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