using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Server
{
    class Program
    {
        const int port = 4004;
        static void Main(string[] args)
        {
            var cancellation = new System.Threading.CancellationTokenSource();
            var userService = new Ntreev.Crema.Services.Users.UserService();
            var server = new Grpc.Core.Server()
            {
                Services = { Ntreev.Crema.Services.Users.IUserContextService.BindService(userService) },
                Ports = { new Grpc.Core.ServerPort("localhost", 4004, Grpc.Core.ServerCredentials.Insecure) }
            };

            var task = Task.Run(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var words = Ntreev.Library.Random.RandomUtility.NextWord();
                    userService.OnLoggedIn(words);
                    Console.WriteLine(words);
                    Thread.Sleep(1000);
                }
            });

            server.Start();
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
            cancellation.Cancel();
            task.Wait();
            userService.Dispose();
            server.ShutdownAsync().Wait();
        }
    }
}
