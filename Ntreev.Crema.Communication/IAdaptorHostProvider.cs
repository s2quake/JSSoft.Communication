using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public interface IAdaptorHostProvider
    {
        IAdaptorHost Create(IServiceHost serviceHost, ServiceToken token);

        string Name { get; }
    }
}