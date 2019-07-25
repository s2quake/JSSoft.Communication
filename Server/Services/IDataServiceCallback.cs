using Ntreev.Crema.Communication;

namespace Ntreev.Crema.Services
{
    public interface IDataServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
         void OnAdd(string userID, int test);
    }
}
