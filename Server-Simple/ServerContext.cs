
using System;
using JSSoft.Communication;

namespace JSSoft.Communication.ConsoleApp
{
    class ServerContext : ServerContextBase
    {
        public ServerContext(IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {
        }
    }
}
