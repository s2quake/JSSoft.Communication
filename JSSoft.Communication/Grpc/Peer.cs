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

using JSSoft.Communication.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JSSoft.Communication.Grpc
{
    sealed class Peer : IPeer
    {
        private readonly CancellationTokenSource cancellation = new();

        public Peer(Guid id, IServiceHost[] serviceHosts)
        {
            this.ID = id;
            this.ServiceHosts = serviceHosts;
            this.Ping();
            foreach (var item in serviceHosts)
            {
                this.PollReplyItems.Add(item, new PollReplyItemCollection());
            }
        }

        public void Dispose()
        {
            foreach (var item in this.ServiceHosts)
            {
                this.PollReplyItems.Remove(item);
            }
        }

        public void Abort()
        {
            this.cancellation.Cancel();
            LogUtility.Debug($"{this.ID} Aboreted.");
        }

        public void Ping()
        {
            this.PingTime = DateTime.UtcNow;
        }

        public Guid ID { get; }

        public IServiceHost[] ServiceHosts { get; }

        public Guid Token { get; set; } = Guid.NewGuid();

        public DateTime PingTime { get; set; }

        public PeerDescriptor Descriptor { get; set; }

        public Dictionary<IServiceHost, object> Services => this.Descriptor.Services;

        public Dictionary<IServiceHost, PollReplyItemCollection> PollReplyItems { get; } = new Dictionary<IServiceHost, PollReplyItemCollection>();

        public CancellationToken Cancellation => this.cancellation.Token;
    }
}
