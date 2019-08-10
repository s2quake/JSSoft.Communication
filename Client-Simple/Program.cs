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
            using (var serviceContext = new ClientContext(new IServiceHost[] { userServiceHost }))
            {
                var token = await serviceContext.OpenAsync();
                var userToken = await userService.LoginAsync("admin", "admin");

                Console.WriteLine($"로그인 되었습니다: {token:B}");
                Console.WriteLine("종료하려면 아무 키나 누르세요.");
                Console.ReadKey();

                await userService.LogoutAsync(userToken);
                await serviceContext.CloseAsync(token);
            }
        }
    }
}
