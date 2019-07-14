using System;
using Grpc.Core;

namespace Server
{
    class Program
    {
        const int port = 4004;
        static void Main(string[] args)
        {
            var server = new Grpc.Core.Server()
            {
                Services = { Common.Users.UserContext.BindService(new Server.UserContext()) },
                Ports = { new Grpc.Core.ServerPort("localhost", 4004, Grpc.Core.ServerCredentials.Insecure) }
            };

            server.Start();
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
