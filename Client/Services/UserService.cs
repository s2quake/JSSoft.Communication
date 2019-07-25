using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;

namespace Client.Services
{
    [Export(typeof(IService))]
    [Export(typeof(IUserService))]
    class UserService : ClientServiceBase<IUserService, IUserServiceCallback>, IUserService, IUserServiceCallback
    {
        public UserService()
            : base()
        {

        }

        #region IUserServiceCallback

        public Task<int> LoginAsync(string user)
        {
            return this.Service.LoginAsync(user);
        }

        public Task<(int, string)> LogoutAsync(string user, int count)
        {
            return this.Service.LogoutAsync(user, count);
        }

        #endregion

        #region IUserServiceCallback

        void IUserServiceCallback.OnLoggedIn(string userID)
        {
            Console.WriteLine($"{nameof(IUserServiceCallback.OnLoggedIn)}: '{userID}'");
        }

        void IUserServiceCallback.OnLoggedOut(string userID)
        {
            Console.WriteLine($"{nameof(IUserServiceCallback.OnLoggedOut)}: '{userID}'");
        }

        #endregion
    }
}
