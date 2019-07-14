using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Grpc.Core.Channel("localhost:4004", Grpc.Core.ChannelCredentials.Insecure);
            var client = new Common.Users.UserContext.UserContextClient(channel);

            var reply = client.Subscribe(new Common.Users.SubscribeRequest() { Id = "wow" });
            Console.WriteLine(reply.Id);

            channel.ShutdownAsync().Wait();
        }
    }
}
