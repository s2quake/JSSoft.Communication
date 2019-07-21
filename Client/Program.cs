using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Users;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            var serviceHost = Container.GetService<IServiceHost>();
            serviceHost.Open();
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
            cancellation.Cancel();
            //task.Wait();
            serviceHost.Close();
            Container.Release();
        }
    }
}
