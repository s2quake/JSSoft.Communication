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

namespace JSSoft.Communication.Grpc;

class AdaptorClientImpl : Adaptor.AdaptorClient, IPeer
{
    private static readonly TimeSpan timeout = new(0, 0, 15);
    private Timer? _timer;

    public AdaptorClientImpl(Channel channel, Guid id, IServiceHost[] serviceHosts)
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
            this._timer = new Timer(timeout.TotalMilliseconds);
            this._timer.Elapsed += Timer_Elapsed;
            this._timer.Start();
        });
    }

    public async Task CloseAsync()
    {
        await Task.Run(() =>
        {
            this._timer.Dispose();
            this._timer = null;
        });
        await base.CloseAsync(new CloseRequest() { Token = this.Token.ToString() });
    }

    public async Task AbortAsync()
    {
        await Task.Run(() =>
        {
            this._timer.Dispose();
            this._timer = null;
        });
    }

    public Guid ID { get; }

    public Guid Token { get; private set; }

    public IServiceHost[] ServiceHosts { get; }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        var request = new PingRequest()
        {
            Token = this.Token.ToString()
        };
        Task.Run(async () =>
        {
            try
            {
                await this.PingAsync(request);
            }
            catch
            {

            }
        });
    }
}
