using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    public interface IAdaptorHost
    {
        void Open(string host, int port);

        void Close();

        // Task<InvokeResult> InvokeAsync(InvokeInfo info);

        // Task PollAsync(Action<PollItem> callback, CancellationToken cancellation);
    }
}
