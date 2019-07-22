using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Users
{
    public interface IUserService
    {
        Task<int> LoginAsync(string user);

        Task<(int, string)> LogoutAsync(string user, int count);
    }
}
