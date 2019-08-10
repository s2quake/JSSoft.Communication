
using System;
using Ntreev.Crema.Communication;

namespace Server
{
    class ServerContext : ServerContextBase
    {
        public ServerContext()
            : base(new IServiceHost[] { })
        {
        }
    }
}
