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
        private Dictionary<IService, int> idByService = new Dictionary<IService, int>();

        public AdaptorClientImpl(Channel channel, IEnumerable<IService> services)
            : base(channel)
        {
            this.channel = channel;
            this.services = services.ToArray();
            this.idByService = services.ToDictionary(item => item, item => 0);
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
            var count = this.idByService.Count;
            this.call = this.Poll();
            while (!cancellation.IsCancellationRequested)
            {
                foreach (var item in this.services)
                {
                    var id = this.idByService[item];
                    var request = new PollRequest() { Id = id, ServiceName = item.Name };
                    await this.call.RequestStream.WriteAsync(request);
                    await this.call.ResponseStream.MoveNext(cancellation);
                    var reply = this.call.ResponseStream.Current;
                    this.idByService[item] = this.InvokeCallback(item, id, reply.Items);
                    await Task.Delay(1);
                }
            }
            this.call.Dispose();
            this.call = null;
        }

        private int InvokeCallback(IService service, int id, IEnumerable<PollReplyItem> pollItems)
        {
            foreach (var item in pollItems)
            {
                if (item.Id >= 0)
                {
                    var pollItem = ToPollItem(item);
                    this.OnPoll(pollItem);
                    id = item.Id + 1;
                }
            }
            return id;
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
                ServiceName = info.ServiceName,
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