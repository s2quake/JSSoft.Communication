using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    public interface IService : IDisposable
    {
        void Open(ServiceToken token, object instance);

        void Close(ServiceToken token);

        Type ServiceType { get; }

        Type CallbackType { get; }

        string Name { get; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}