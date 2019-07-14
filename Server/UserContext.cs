using System;
using System.Threading.Tasks;

namespace Server
{
    class UserContext : Common.Users.UserContext.UserContextBase
    {
        public override Task<Common.Users.SubscribeReply> Subscribe(Common.Users.SubscribeRequest request, Grpc.Core.ServerCallContext context)
        {
            return Task.Run(() =>
            {
                return new Common.Users.SubscribeReply() { Id = "message", };
            });
        }
    }
}