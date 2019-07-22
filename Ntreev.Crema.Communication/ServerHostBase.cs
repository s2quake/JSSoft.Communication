using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public abstract class ServerHostBase : ServiceHostBase
    {
        protected ServerHostBase(IEnumerable<IService> services)
            : base(new AdaptorServerHost(services), services)
        {

        }
    }
}