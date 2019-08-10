
using System;
using JSSoft.Communication;

namespace JSSoft.Communication.ConsoleApp
{
    class ClientContext : ClientContextBase
    {
        public ClientContext(IServiceHost[] serviceHosts)
            : base(serviceHosts)
        {
        }
    }
}
