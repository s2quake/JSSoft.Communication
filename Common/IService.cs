using System;

namespace Ntreev.Crema.Services
{
    public interface IService
    {
        void Open();

        void Close();

        Type ServiceType { get; }
        Type CallbackType { get; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}