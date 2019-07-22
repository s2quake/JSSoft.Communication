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
        public ServiceHost([ImportMany]IEnumerable<IService> services)
            : base(services)
        {
     
        }
    }
}