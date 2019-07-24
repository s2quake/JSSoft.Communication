using System;
using System.ServiceModel;
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;

namespace Ntreev.Crema.Services.Users
{
    public interface IUserServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
        void OnLoggedOut(string userID);
    }
}
