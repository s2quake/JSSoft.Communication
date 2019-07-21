using System;

namespace Ntreev.Crema.Services
{
    public sealed class ServiceToken
    {
#if Server
        internal ServiceToken(IAdaptor adaptor, object callback)
        {
            this.Adaptor = adaptor;
            this.Callback = callback;
        }
#endif
#if Client
        internal ServiceToken(IAdaptor adaptor, object client)
        {
            this.Adaptor = adaptor;
            this.Client = client;
        }
#endif

        public IAdaptor Adaptor { get; }

#if Server
        public object Callback { get; }
#endif
#if Client
        public object Client { get; }
#endif

        internal static readonly ServiceToken Empty = new ServiceToken(null, null);
    }
}