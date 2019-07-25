using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;
using Ntreev.Library.Threading;

namespace Server.Services
{
    [Export(typeof(IService))]
    class DataService : ServerServiceBase<IDataService, IDataServiceCallback>, IDataService
    {
        [ServiceContract]
        public async Task<int> LoginAsync(string Data)
        {
            await Task.Delay(100);
            Console.WriteLine("LoginAsync");
            return 0;
            //Task.Run(()=> this.Callback.OnAdd("WER", 0));
        }

        [ServiceContract]
        public async Task<(int, string)> LogoutAsync(string Data, int count)
        {
            await Task.Delay(100);
            return (1, "ser");
        }
    }
}