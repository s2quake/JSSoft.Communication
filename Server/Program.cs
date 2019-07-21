using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Users;



namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            var serviceHost = Container.GetService<IServiceHost>();
            serviceHost.Open();
            var task = InvokeTest(serviceHost, cancellation.Token);
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
            cancellation.Cancel();
            task.Wait();
            serviceHost.Close();
            Container.Release();
        }

        static Task InvokeTest(IServiceHost serviceHost, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    if (serviceHost.Services[0] is IUserService userService)
                    {
                        userService.LoginAsync("wer");
                    }
                    Thread.Sleep(1000);
                }
            });
        }
    }
}
