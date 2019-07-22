using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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
            await Task.Delay(100);
            Console.WriteLine("LoginAsync");
            return 0;
            //Task.Run(()=> this.Callback.OnAdd("WER", 0));
        }

        [ServiceContract]
        public async Task<(int, string)> LogoutAsync(string user, int count)
        {
            await Task.Delay(100);
            return (1, "ser");
        }
    }
}