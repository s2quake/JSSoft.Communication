using System;
using System.Threading.Tasks;
using JSSoft.Communication.Services;

namespace JSSoft.Communication.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var userService = new UserService();
            using (var userServiceHost = new UserServiceHost(userService))
            using (var serviceContext = new ServerContext(new IServiceHost[] { userServiceHost }))
            {
                var token = await serviceContext.OpenAsync();

                Console.WriteLine("종료하려면 아무 키나 누르세요.");
                Console.ReadKey();

                await serviceContext.CloseAsync(token);
            }
        }
    }
}
