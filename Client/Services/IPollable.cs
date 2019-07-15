using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services
{
    interface IPollable
    {
        Task PollAsync();
    }
}