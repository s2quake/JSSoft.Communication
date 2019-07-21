using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Services
{
    public interface IAdaptorHost
    {
        void Open(string host, int port);

        void Close();
    }
}