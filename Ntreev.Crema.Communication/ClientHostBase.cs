using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public abstract class ClientHostBase : ServiceHostBase
    {
        protected ClientHostBase(IAdaptorHostProvider adpatorHostProvider, IEnumerable<IService> services)
            : base(adpatorHostProvider, services)
        {

        }
    }
}