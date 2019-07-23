using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Communication;

namespace Ntreev.Crema.Services.Users
{
    [Export(typeof(IService))]
    [Export(typeof(IUserService))]
    class UserService : ClientServiceBase<IUserService, IUserServiceCallback>, IUserServiceCallback
    {
        //private AsyncDuplexStreamingCall<PollRequest, PollReply> call;

        public UserService()
            : base()
        {
            
        }

        // protected async override Task<PollReply> RequestAsync(PollRequest request, CancellationToken cancellation)
        // {
        //     await this.call.RequestStream.WriteAsync(request);
        //     await this.call.ResponseStream.MoveNext(cancellation);
        //     return this.call.ResponseStream.Current;
        // }

        // protected override Adaptor.AdaptorClient CreateClient(Channel channel)
        // {
        //     return new Adaptor.AdaptorClient(channel);
        // }

        // protected override void OnPollBegun()
        // {
        //     this.call = this.Client.Poll();
        // }

        // protected override void OnPollEnded()
        // {
        //     this.call.Dispose();
        //     this.call = null;
        // }

        #region IUserServiceCallback

        void IUserServiceCallback.OnLoggedIn(string userID)
        {
            Console.WriteLine(userID);
        }

        void IUserServiceCallback.OnAdd(string userID, int test)
        {
            
        }

        #endregion
    }
}
