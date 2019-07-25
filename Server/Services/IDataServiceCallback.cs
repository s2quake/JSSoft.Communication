using Ntreev.Crema.Communication;

namespace Server.Services
{
    public interface IDataServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
         void OnAdd(string userID, int test);
    }
}
