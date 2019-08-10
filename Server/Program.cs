using System;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var serverContext = new ServerContext())
            {
                var token = await serverContext.OpenAsync();
                Console.ReadKey();
                await serverContext.CloseAsync(token);
            }
        }
    }
}
