using System;
using System.ServiceModel;
using Ntreev.Crema.Services;

namespace Ntreev.Crema.Services.Users
{
    interface IUserServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);
    }

}