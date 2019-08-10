
using System;
using JSSoft.Communication;

namespace JSSoft.Communication.Shell
{
    class ClientContext : ClientContextBase
    {
        public ClientContext(IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {
        }
    }
}
