using System;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Services;

namespace Server
{
    class UsersImpl : Adaptor.AdaptorBase
    {
        public override Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public override Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}