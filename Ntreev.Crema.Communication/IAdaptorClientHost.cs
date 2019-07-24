using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    interface IAdaptorClientHost
    {
        void Invoke(string serviceName, string name, object[] args);

        T Invoke<T>(string serviceName, string name, object[] args);

        Task InvokeAsync(string serviceName, string name, object[] args);

        Task<T> InvokeAsync<T>(string serviceName, string name, object[] args);
    }
}
