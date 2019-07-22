using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Ntreev.Crema.Communication;

namespace Server
{
    [Export(typeof(IServiceHost))]
    class ServiceHost : ServerHostBase
    {
        [ImportingConstructor]
        public ServiceHost([ImportMany]IEnumerable<IService> services)
            : base(services)
        {
     
        }
    }
}