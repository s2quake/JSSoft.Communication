using System;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Data
{
    interface IDataService
    {
        Task<int> LoginAsync(string user);

        Task<(int, string)> LogoutAsync(string user, int count);
    }

}