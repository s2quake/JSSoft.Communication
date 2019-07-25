using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    public interface IServiceHost
    {
        Task OpenAsync();

        Task CloseAsync();

        IReadOnlyList<IService> Services { get; }

        string Host { get; set; }
        
        int Port { get; set; }

        bool IsOpened { get; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}