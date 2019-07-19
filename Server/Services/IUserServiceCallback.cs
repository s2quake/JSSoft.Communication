using System;
using System.ServiceModel;
using Ntreev.Crema.Services;

namespace Ntreev.Crema.Services.Users
{
    internal interface IUserServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
         void OnAdd(string userID, int test);
    }

}