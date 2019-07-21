using System;
using System.ServiceModel;
using Ntreev.Crema.Services;

namespace Ntreev.Crema.Services.Data
{
    internal interface IDataServiceCallback
    {
        [ServiceContract]
        void OnLoggedIn(string userID);

        [ServiceContract]
         void OnAdd(string userID, int test);
    }

}