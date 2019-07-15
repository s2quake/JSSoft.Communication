using System;

namespace Ntreev.Crema.Services.Users
{
    interface IUserServiceCallback
    {
        void OnLoggedIn(string userID);
    }

}