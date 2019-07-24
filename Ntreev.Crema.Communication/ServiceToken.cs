using System;

namespace Ntreev.Crema.Communication
{
    public sealed class ServiceToken
    {
        internal ServiceToken(IAdaptorHost adaptorHost)
        {
            this.AdaptorHost = adaptorHost;
        }

        internal IAdaptorHost AdaptorHost { get; }

        internal static readonly ServiceToken Empty = new ServiceToken(null);
    }
}
