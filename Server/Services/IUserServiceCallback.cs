using Ntreev.Crema.Communication;

namespace Server.Services
{
    public interface IUserServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
        void OnLoggedOut(string userID);
    }
}
