using System;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;

namespace Ntreev.Crema.Services
{
    public interface IDataService
    {
        [ServiceContract]
        Task<int> LoginAsync(string user);

        [ServiceContract]
        Task<(int, string)> LogoutAsync(string user, int count);
    }
}
