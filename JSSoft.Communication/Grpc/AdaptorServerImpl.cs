// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Grpc.Core;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc
{
    class AdaptorServerImpl : Adaptor.AdaptorBase
    {
        private readonly AdaptorServerHost adaptorHost;

        public AdaptorServerImpl(AdaptorServerHost adaptorHost)
        {
            this.adaptorHost = adaptorHost;
        }

        public override Task<OpenReply> Open(OpenRequest request, ServerCallContext context)
        {
            return this.adaptorHost.Open(request, context);
        }

        public override Task<CloseReply> Close(CloseRequest request, ServerCallContext context)
        {
            return this.adaptorHost.Close(request, context);
        }

        public override Task<PingReply> Ping(PingRequest request, ServerCallContext context)
        {
            return this.adaptorHost.Ping(request, context);
        }

        public override Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        {
            return this.adaptorHost.Invoke(request, context);
        }

        public override Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        {
            return this.adaptorHost.Poll(requestStream, responseStream, context);
        }
    }
}
