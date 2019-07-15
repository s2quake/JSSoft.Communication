using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services.Users
{
    class UserService : ServiceBase<IUserServiceCallback>, IDisposable
    {
        private readonly IUserContextService.IUserContextServiceClient client;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        public UserService(IUserContextService.IUserContextServiceClient client)
            : this(client, null)
        {
        }

        public UserService(IUserContextService.IUserContextServiceClient client, object instanceContext)
            : base(instanceContext)
        {
            this.client = client;
        }

        protected async override Task<PollReply> RequestAsync(PollRequest request, CancellationToken cancellation)
        {
            if (this.call == null)
            {
                this.call = this.client.Poll();
            }
            await this.call.RequestStream.WriteAsync(request);
            await this.call.ResponseStream.MoveNext(cancellation);
            return this.call.ResponseStream.Current;
        }
    }
}
