using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services.Users
{
    class UserService : IUserContextService.IUserContextServiceBase, IUserServiceCallback
    {
        private readonly Dispatcher dispatcher;
        private List<PollReplyItem> callbackList = new List<PollReplyItem>();
        private int id;
        private readonly PollReplyItem nullReply = new PollReplyItem() { Id = -1 };

        public UserService()
        {
            this.dispatcher = new Dispatcher(this);
        }
        public override Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            throw new NotImplementedException();
        }

        public void OnLoggedIn(string userID)
        {
            this.dispatcher.InvokeAsync(() =>
            {
                var reply = new PollReplyItem()
                {
                    Id = id++,
                };
                reply.Name = nameof(OnLoggedIn);
                reply.Type.Add(userID.GetType().AssemblyQualifiedName);
                reply.Data.Add(JsonConvert.SerializeObject(userID));
                this.callbackList.Add(reply);
            });
        }

        public override async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var id = request.Id;
                var reply = new PollReply();
                await this.dispatcher.InvokeAsync(() =>
                {
                    var items = new PollReplyItem[this.callbackList.Count - id];
                    for (var i = id; i < this.callbackList.Count; i++)
                    {
                        reply.Items.Add(this.callbackList[i]);
                    }
                    return items;
                });
                await responseStream.WriteAsync(reply);
            }
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
        }
    }
}