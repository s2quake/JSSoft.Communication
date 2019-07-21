using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    class AdaptorImpl : Adaptor.AdaptorBase, IAdaptor
    {
        private readonly IServiceInvoker serviceInvoker;
        private readonly CallbackBase callback;

        public AdaptorImpl(IServiceInvoker serviceInvoker, CallbackBase callback)
        {
            this.serviceInvoker = serviceInvoker;
            this.callback = callback;
        }
        public override async Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            var info = ToInvokeInfo(request);
            var result = await this.serviceInvoker.Invoke(info);
            return ToInvokeReply(result);
        }

        public override async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var id = request.Id;
                var reply = new PollReply();
                await this.callback.PollAsync(reply, id);
                await responseStream.WriteAsync(reply);
            }
        }

        private static InvokeInfo ToInvokeInfo(InvokeRequest request)
        {
            var info = new InvokeInfo()
            {
                Name = request.Name,
                Types = new string[request.Types_.Count],
                Datas = new string[request.Datas.Count]
            };
            for (var i = 0; i < request.Types_.Count; i++)
            {
                info.Types[i] = request.Types_[i];
            }
            for (var i = 0; i < request.Datas.Count; i++)
            {
                info.Types[i] = request.Datas[i];
            }
            return info;
        }

        private static InvokeReply ToInvokeReply(InvokeResult result)
        {
            var reply = new InvokeReply();
            reply.Types_.AddRange(result.Types);
            reply.Datas.AddRange(result.Datas);
            return reply;
        }
    }
}