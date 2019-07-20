using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Services
{
    [Export(typeof(IServiceHost))]
    class ServiceHost : ServiceHostBase
    {
        [ImportingConstructor]
        public ServiceHost([ImportMany]IEnumerable<IService> services)
            : base(services)
        {
     
        }
    }
}