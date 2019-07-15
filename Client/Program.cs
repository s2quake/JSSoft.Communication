using System;
using System.Threading;
using System.Threading.Tasks;
using Ntreev.Crema.Services.Users;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var cancellation = new CancellationTokenSource();
            var channel = new Grpc.Core.Channel("localhost:4004", Grpc.Core.ChannelCredentials.Insecure);
            var client = new IUserContextService.IUserContextServiceClient(channel);

            var call = client.Poll();
            var task = Task.Run(async () =>
            {
                var id = 0;
                while (!cancellation.IsCancellationRequested)
                {
                    var request = new PollRequest()
                    {
                        Id = id,
                    };
                    await call.RequestStream.WriteAsync(request);
                    while (await call.ResponseStream.MoveNext(cancellation.Token))
                    {
                        var reply = call.ResponseStream.Current;
                        Console.WriteLine(reply.Name);
                        id = reply.Id;
                    }
                }
            });

            // var reply = client.Subscribe(new Common.Users.SubscribeRequest() { Id = "wow" });
            // Console.WriteLine(reply.Id);
            Console.ReadKey();
            cancellation.Cancel();
            task.Wait();
            call.Dispose();
            channel.ShutdownAsync().Wait();
        }
    }
}
