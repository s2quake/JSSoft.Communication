using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientImpl : Adaptor.AdaptorClient
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly Channel channel;
        private readonly IService[] services;
        private AsyncDuplexStreamingCall<PollRequest, PollReply> call;
        private CancellationTokenSource cancellation;
        private Task task;

        public AdaptorClientImpl(Channel channel, IEnumerable<IService> services)
            : base(channel)
        {
            this.channel = channel;
            this.services = services.ToArray();
            this.cancellation = new CancellationTokenSource();

            this.task = this.PollAsync(this.cancellation.Token);
        }

        public async Task<InvokeResult> InvokeAsync(InvokeInfo info)
        {
            var request = ToInvokeReqeust(info);
            var reply = await Task.Run(() => this.Invoke(request));
            return ToInvokeResult(reply);
        }

        public int ID { get; set; }

        private void OnPoll(PollItem pollItem)
        {

        }

        public async Task PollAsync(CancellationToken cancellation)
        {
            this.call = this.Poll();
            while (!cancellation.IsCancellationRequested)
            {
                var request = new PollRequest() { Id = this.ID, };
                //var replies = await this.RequestAsync(request, this.cancellation.Token);
                await this.call.RequestStream.WriteAsync(request);
                await this.call.ResponseStream.MoveNext(cancellation);
                var replies = this.call.ResponseStream.Current;
                foreach (var item in replies.Items)
                {
                    if (item.Id >= 0)
                    {
                        var pollItem = ToPollItem(item);
                        this.OnPoll(pollItem);
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
            for (var i = 0; i < replyItem.Types_.Count; i++)
            {
                pollItem.Types[i] = Type.GetType(replyItem.Types_[i]);
                pollItem.Datas[i] = JsonConvert.DeserializeObject(replyItem.Datas[i], pollItem.Types[i], settings);
            }
            return pollItem;
        }

        private static InvokeRequest ToInvokeReqeust(InvokeInfo info)
        {
            var types = new string[info.Types.Length];
            var datas = new string[info.Datas.Length];
            var request = new InvokeRequest()
            {
                Name = info.Name,
            };
            for (var i = 0; i < info.Types.Length; i++)
            {
                types[i] = info.Types[i].AssemblyQualifiedName;
                datas[i] = JsonConvert.SerializeObject(info.Datas[i], info.Types[i], settings);
            }
            request.Types_.AddRange(types);
            request.Datas.AddRange(datas);
            return request;
        }

        private static InvokeResult ToInvokeResult(InvokeReply reply)
        {
            var result = new InvokeResult();
            result.Type = Type.GetType(reply.Type);
            result.Data = JsonConvert.DeserializeObject(reply.Data, result.Type, settings);
            return result;
        }
    }
}