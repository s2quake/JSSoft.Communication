using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Threading;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Communication;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services.Users
{
    [Export(typeof(IService))]
    class UserService : ServerServiceBase<IUserService, IUserServiceCallback>, IUserService
    {
        [ServiceContract]
        public async Task<int> LoginAsync(string user)
        {
            await Task.Delay(1);
            Console.WriteLine($"login: {user}");
            this.Callback.OnLoggedIn(user);
            return 0;
        }

        [ServiceContract]
        public async Task<(int, string)> LogoutAsync(string user, int count)
        {
            await Task.Delay(1);
            this.Callback.OnLoggedOut(user);
            return (1, "ser");
        }
    }
}