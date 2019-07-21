using System;
using System.Threading.Tasks;


namespace Ntreev.Crema.Services
{
    public interface IServiceInvoker
    {
        Task<InvokeResult> Invoke(InvokeInfo info);
    }
}