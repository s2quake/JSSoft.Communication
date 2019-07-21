using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    class AdaptorImpl : Adaptor.AdaptorClient, IAdaptor
    {
        private readonly Grpc.Core.Channel channel;
        private readonly IService service;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;

        public AdaptorImpl(Grpc.Core.Channel channel, IService service)
        {
            this.channel = channel;
            this.service = service;
        }
public int ID{get;set;}

        public async Task PollAsync(int id, Action<PollItem> callback, CancellationToken cancellation)
        {
            this.call = this.Poll();
            while (!cancellation.IsCancellationRequested)
            {
                var request = new PollRequest() { Id = id, };
                //var replies = await this.RequestAsync(request, this.cancellation.Token);
                await this.call.RequestStream.WriteAsync(request);
             await this.call.ResponseStream.MoveNext(cancellation);
             var replies = this.call.ResponseStream.Current;
                foreach (var item in replies.Items)
                {
                    if (item.Id >= 0)
                    {
                        var pollItem = ToPollItem(item);
                        callback(pollItem);
                        this.ID = item.Id + 1;
                    }
                }
            }
           this.call.Dispose();
             this.call = null;
        }

        private static PollItem ToPollItem(PollReplyItem replyItem)
        {
            var pollItem = new PollItem()
            {
ID = replyItem.Id,
Name = replyItem.Name,
Types = new Type[replyItem.Types_.Count],
Datas = new string[replyItem.Datas.Count]
            };

            
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
    }
}