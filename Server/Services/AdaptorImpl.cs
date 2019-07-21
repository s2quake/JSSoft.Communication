using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;

namespace Ntreev.Crema.Services
{
    class AdaptorImpl : Adaptor.AdaptorBase, IAdaptor
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly AdaptorHost adaptorHost;
        private IService service;

        public AdaptorImpl(AdaptorHost adaptorHost, IService service)
        {
            this.adaptorHost = adaptorHost;
            this.service = service;
        }

        public override async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            var info = ToInvokeInfo(request);
            var result = await this.service.InvokeAsync(context, info);
            return ToInvokeReply(result);
        }

        public override async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var id = request.Id;
                var reply = new PollReply();
                var results = await this.service.PollAsync(context, id);
                await responseStream.WriteAsync(reply);
            }
        }

        private static InvokeInfo ToInvokeInfo(InvokeRequest request)
        {
            // pollItem.Types[i] = type.AssemblyQualifiedName;
            //         pollItem.Datas[i] = JsonConvert.SerializeObject(value, type, this.settings);

            var info = new InvokeInfo()
            {
                Name = request.Name,
                Types = new Type[request.Types_.Count],
                Datas = new object[request.Datas.Count]
            };
            for (var i = 0; i < request.Types_.Count; i++)
            {
                info.Types[i] = Type.GetType(request.Types_[i]);
                info.Datas[i] = JsonConvert.DeserializeObject(request.Datas[i], info.Types[i], settings);
            }
            return info;
        }

        private static InvokeReply ToInvokeReply(InvokeResult result)
        {
            var reply = new InvokeReply()
            {
                Type = result.Type.AssemblyQualifiedName,
                Data = JsonConvert.SerializeObject(result.Data, result.Type, settings)
            };
            return reply;
        }

        private static PollReply ToPollReply(PollItem[] results)
        {
            var replyItemList = new List<PollReplyItem>(results.Length);
            var reply = new PollReply();
            for (var i = 0; i < results.Length; i++)
            {
                var item = results[i];
                replyItemList.Add(ToPollReplyItem(item));
            }
            reply.Items.AddRange(replyItemList);
            return reply;
        }


        private static PollReplyItem ToPollReplyItem(PollItem pollItem)
        {
            var types = new string[pollItem.Types.Length];
            var datas = new string[pollItem.Datas.Length];
            var replyItem = new PollReplyItem()
            {
                Id = pollItem.ID,
                Name = pollItem.Name,
            };
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = pollItem.Types[i].AssemblyQualifiedName;
                datas[i] = JsonConvert.SerializeObject(pollItem.Datas[i], pollItem.Types[i], settings);
            }
            replyItem.Types_.AddRange(types);
            replyItem.Datas.AddRange(datas);
            return replyItem;
        }
    }
}