using System;
using System.ServiceModel;
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;

namespace Ntreev.Crema.Services.Data
{
    public interface IDataServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
         void OnAdd(string userID, int test);
    }
}
