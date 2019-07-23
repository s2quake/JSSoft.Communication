using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Communication;

namespace Ntreev.Crema.Services.Users
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
            Console.WriteLine(userID);
        }

        void IUserServiceCallback.OnAdd(string userID, int test)
        {

        }

        #endregion
    }
}
