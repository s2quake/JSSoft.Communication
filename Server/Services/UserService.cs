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
        private List<PollReply> callbackList = new List<PollReply>();
        private int id;

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
                var reply = new PollReply()
                {
                    Id = id++,
                };
                reply.Type.Add(userID.GetType().AssemblyQualifiedName);
                reply.Name = nameof(OnLoggedIn);
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
                var replies = await this.dispatcher.InvokeAsync(() =>
                {
                    var items = new PollReply[this.callbackList.Count - id];
                    for (var i = id; i < this.callbackList.Count; i++)
                    {
                        items[i - id] = this.callbackList[i];
                    }
                    return items;
                });
                foreach (var item in replies)
                {
                    await responseStream.WriteAsync(item);
                }
            }
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
        }
    }
}