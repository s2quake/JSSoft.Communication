using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public abstract class ServerHostBase : ServiceHostBase
    {
        protected ServerHostBase(IAdaptorHostProvider adaptorHostProvider, IEnumerable<IService> services)
            : base(adaptorHostProvider, services)
        {

        }
    }
}