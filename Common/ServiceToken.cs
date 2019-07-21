using System;

namespace Ntreev.Crema.Services
{
    public sealed class ServiceToken
    {
#if Server
        internal ServiceToken(IAdaptorHost adaptorHost, object callback)
        {
            this.AdaptorHost = adaptorHost;
            this.Callback = callback;
        }
#endif
#if Client
        internal ServiceToken(IAdaptorHost adaptorHost, object client)
        {
            this.AdaptorHost = adaptorHost;
            this.Client = client;
        }
#endif

        public IAdaptorHost AdaptorHost { get; }

#if Server
        public object Callback { get; }
#endif
#if Client
        public object Client { get; }
#endif

        internal static readonly ServiceToken Empty = new ServiceToken(null, null);
    }
}