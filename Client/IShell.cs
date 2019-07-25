using System;
using System.Threading.Tasks;

namespace Client
{
    public interface IShell : IDisposable
    {
        Task StopAsync();

        Task StartAsync();
    }
}