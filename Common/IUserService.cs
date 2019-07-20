using System;

namespace Ntreev.Crema.Services.Users
{
    interface IUserService
    {
        void Login(string user);

        void Logout();
    }

}