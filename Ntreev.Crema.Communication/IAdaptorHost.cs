using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public interface IAdaptorHost : IDisposable
    {
        void Open(string host, int port);

        void Close();

        object Create(IService service);
    }
}