using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    interface IAdaptorHost
    {
        void Open(string host, int port);

        void Close();

        object CreateInstance(IService service);
    }
}