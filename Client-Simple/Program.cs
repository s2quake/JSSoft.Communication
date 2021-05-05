// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using JSSoft.Communication.Services;
using System;
using System.Threading.Tasks;

namespace JSSoft.Communication.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] _)
        {
            var userService = new UserService();
            var userServiceHost = new UserServiceHost(userService);
            var serviceContext = new ClientContext(userServiceHost);
            {
                var token = await serviceContext.OpenAsync();
                var userToken = await userService.LoginAsync("admin", "admin");

                Console.WriteLine($"로그인 되었습니다: {token:B}");
                Console.WriteLine("종료하려면 아무 키나 누르세요.");
                Console.ReadKey();

                await userService.LogoutAsync(userToken);
                await serviceContext.CloseAsync(token, 0);
            }
        }
    }
}
