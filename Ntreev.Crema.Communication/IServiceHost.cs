using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    public interface IServiceHost
    {
        void Open();

        void Close();

        IReadOnlyList<IService> Services { get; }

        int Port { get; set; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}