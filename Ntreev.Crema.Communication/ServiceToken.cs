using System;

namespace Ntreev.Crema.Communication
{
    public sealed class ServiceToken
    {
        internal ServiceToken(object value)
        {
            this.Value = value;
        }

        internal object Value { get; }

        internal static readonly ServiceToken Empty = new ServiceToken(null);
    }
}
