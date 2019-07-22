using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    interface IServiceInstance
    {
        void Invoke(string name, params object[] args);

        Task InvokeAsync(string name, params object[] args);

        Task<T> InvokeAsyncWithResult<T>(string name, params object[] args);
    }
}
