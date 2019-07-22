using System;

namespace Ntreev.Crema.Communication
{
    public sealed class ServiceToken
    {
        internal ServiceToken(IAdaptorHost adaptorHost, object callback)
        {
            this.AdaptorHost = adaptorHost;
            this.Callback = callback;
        }

        public IAdaptorHost AdaptorHost { get; }

        public object Callback { get; }

        internal static readonly ServiceToken Empty = new ServiceToken(null, null);
    }
}