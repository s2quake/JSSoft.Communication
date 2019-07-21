using System;

namespace Ntreev.Crema.Services
{
    public interface IService
    {
        void Open(ServiceToken token);

        void Close(ServiceToken token);

        Type ServiceType { get; }
        Type CallbackType { get; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}