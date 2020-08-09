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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace JSSoft.Communication.Grpc
{
    class AdaptorClientImpl : Adaptor.AdaptorClient, IPeer
    {
        private static readonly TimeSpan timeout = new TimeSpan(0, 0, 10);
        private readonly Dictionary<IServiceHost, object> callbacks = new Dictionary<IServiceHost, object>();
        private Timer timer;

        public AdaptorClientImpl(Channel channel, string id, IServiceHost[] serviceHosts)
            : base(channel)
        {
            this.ID = id;
            this.ServiceHosts = serviceHosts;
        }

        public async Task OpenAsync()
        {
            var request = await Task.Run(() =>
            {
                var serviceNames = this.ServiceHosts.Select(item => item.Name).ToArray();
                var req = new OpenRequest() { Time = DateTime.UtcNow.Ticks };
                req.ServiceNames.AddRange(serviceNames);
                return req;
            });
            var reply = await base.OpenAsync(request);
            await Task.Run(() =>
            {
                this.Token = Guid.Parse(reply.Token);
                this.timer = new Timer(timeout.TotalMilliseconds);
                this.timer.Elapsed += Timer_Elapsed;
                this.timer.Start();
            });
        }

        public async Task CloseAsync()
        {
            await Task.Run(() =>
            {
                this.timer.Dispose();
                this.timer = null;
            });
            var value = await base.CloseAsync(new CloseRequest() { Token = this.Token.ToString() });
        }

        public string ID { get; }

        public Guid Token { get; private set; }

        public IServiceHost[] ServiceHosts { get; }

        public IReadOnlyDictionary<IServiceHost, object> Callbacks => this.callbacks;

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
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
