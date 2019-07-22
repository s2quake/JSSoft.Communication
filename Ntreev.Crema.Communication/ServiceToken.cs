using System;

namespace Ntreev.Crema.Communication
{
    public sealed class ServiceToken
    {
        internal ServiceToken(IAdaptorHost adaptorHost, object instance)
        {
            this.AdaptorHost = adaptorHost;
            this.Instance = instance;
        }

        internal IAdaptorHost AdaptorHost { get; }

        internal object Instance { get; }

        internal static readonly ServiceToken Empty = new ServiceToken(null, null);
    }
}
