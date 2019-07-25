using Ntreev.Crema.Communication;

namespace Ntreev.Crema.Services
{
    public interface IUserServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
        void OnLoggedOut(string userID);
    }
}
