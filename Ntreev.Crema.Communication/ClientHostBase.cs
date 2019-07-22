using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public abstract class ClientHostBase : ServiceHostBase
    {
        protected ClientHostBase(IEnumerable<IService> services)
            : base(new AdaptorClientHost(services), services)
        {

        }
    }
}