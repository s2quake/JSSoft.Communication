
using System;
using JSSoft.Communication;

namespace JSSoft.Communication.Shell
{
    class ServerContext : ServerContextBase
    {
        public ServerContext(IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {
        }
    }
}
