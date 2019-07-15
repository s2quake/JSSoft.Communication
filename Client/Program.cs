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
            var channel = new Grpc.Core.Channel("localhost:4004", Grpc.Core.ChannelCredentials.Insecure);
            var client = new IUserContextService.IUserContextServiceClient(channel);
            var userService = new Ntreev.Crema.Services.Users.UserService(client);

            //var methods = userService.GetType().GetInterfaceMap(typeof(IUserServiceCallback)).TargetMethods;

            Console.ReadKey();
            userService.Dispose();
            channel.ShutdownAsync().Wait();
        }
    }
}
