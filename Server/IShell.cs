using System;
using System.Threading.Tasks;

namespace Server
{
    public interface IShell : IDisposable
    {
        Task StartAsync(Settings settings);

        Task StopAsync();
    }
}