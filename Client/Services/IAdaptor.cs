using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    public interface IAdaptor
    {
        Task PollAsync(Action<PollItem> callback, CancellationToken cancellation);
    }
}