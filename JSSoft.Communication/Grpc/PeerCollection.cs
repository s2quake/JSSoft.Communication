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

using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication.Grpc
{
    class PeerCollection : ContainerBase<Peer>
    {
        private readonly ServerContextBase serviceContext;

        public PeerCollection(ServerContextBase serviceContext)
        {
            this.serviceContext = serviceContext;
        }

        public async Task AddAsync(Peer item)
        {
            await this.serviceContext.AddPeerAsync(item);
            await this.Dispatcher.InvokeAsync(() => base.AddBase(item.ID, item));
        }

        public async Task<Peer> RemoveAsync(string id)
        {
            var peer = await this.Dispatcher.InvokeAsync(() =>
            {
                var item = base[id];
                base.RemoveBase(id);
                return item;
            });
            await this.serviceContext.RemovePeekAsync(peer);
            return peer;
        }

        public Dispatcher Dispatcher => this.serviceContext?.Dispatcher;
    }
}
