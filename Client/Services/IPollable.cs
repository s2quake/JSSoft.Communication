using System;

namespace Ntreev.Crema.Services
{
    interface IPollable
    {
        Task PollAsync();
    }
}