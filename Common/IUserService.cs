using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    interface IUserService
    {
        Task LoginAsync(string user);

        Task LogoutAsync();
    }

}