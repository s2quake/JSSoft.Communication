using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    interface IContextInvoker
    {
        void Invoke(IService service, string name, object[] args);

        T Invoke<T>(IService service, string name, object[] args);

        Task InvokeAsync(IService service, string name, object[] args);

        Task<T> InvokeAsync<T>(IService service, string name, object[] args);
    }
}
