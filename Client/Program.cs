using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceHost = new ServiceHost();
            serviceHost.Address = "localhost:4004";
            serviceHost.Services.Add(new UserService());
            serviceHost.Open();
            // var channel = new Grpc.Core.Channel("localhost:4004", Grpc.Core.ChannelCredentials.Insecure);
            // var client = new IUserContextService.IUserContextServiceClient(channel);
            // var userService = new Ntreev.Crema.Services.Users.UserService(client);

            //var methods = userService.GetType().GetInterfaceMap(typeof(IUserServiceCallback)).TargetMethods;

            Console.ReadKey();
            serviceHost.Close();
            //userService.Dispose();
            //channel.ShutdownAsync().Wait();
        }
    }
}
