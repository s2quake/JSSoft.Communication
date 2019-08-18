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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Newtonsoft.Json;
using Ntreev.Library.ObjectModel;

namespace JSSoft.Communication.Grpc
{
    class AdaptorClientImpl : Adaptor.AdaptorClient, IPeer
    {
        private static readonly TimeSpan timeout = new TimeSpan(0, 0, 10);
        private readonly Channel channel;
        private readonly Dictionary<IServiceHost, object> callbacks = new Dictionary<IServiceHost, object>();
        private Timer timer;

        public AdaptorClientImpl(Channel channel, string id, IServiceHost[] serviceHosts)
            : base(channel)
        {
            this.channel = channel;
            this.ID = id;
            this.ServiceHosts = serviceHosts;
        }

        public async Task OpenAsync()
        {
            var serviceNames = this.ServiceHosts.Select(item => item.Name).ToArray();
            var request = new OpenRequest() { Time = DateTime.UtcNow.Ticks };
            request.ServiceNames.AddRange(serviceNames);
            var reply = await base.OpenAsync(request);
            this.Token = Guid.Parse(reply.Token);
            this.timer = new Timer(Timer_TimerCallback, null, timeout.Milliseconds, timeout.Milliseconds);
        }

        public async Task CloseAsync()
        {
            await this.timer.DisposeAsync();
            var value = await base.CloseAsync(new CloseRequest() { Token = this.Token.ToString() });
            this.timer = null;
        }

        public string ID { get; }

        public Guid Token { get; private set; }

        public IServiceHost[] ServiceHosts { get; }

        public IReadOnlyDictionary<IServiceHost, object> Callbacks => this.callbacks;

        private async void Timer_TimerCallback(object state)
        {
            var request = new PingRequest()
            {
                Token = this.Token.ToString()
            };
            await this.PingAsync(request);
        }

        #region IPeer

        void IPeer.AddInstance(IServiceHost serviceHost, object service, object callback)
        {
            this.callbacks.Add(serviceHost, callback);
        }

        #endregion
    }
}
