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

namespace JSSoft.Communication.Grpc;

class PeerCollection : ContainerBase<Peer>
{
    private readonly IServiceContext _serviceContext;
    private readonly IInstanceContext _instanceContext;

    public PeerCollection(IServiceContext serviceContext, IInstanceContext instanceContext)
    {
        _serviceContext = serviceContext;
        _instanceContext = instanceContext;
    }

    public async Task AddAsync(Peer item)
    {
        var descrptor = await _instanceContext.CreateInstanceAsync(item);
        item.Descriptor = descrptor;
        await Dispatcher.InvokeAsync(() => base.AddBase($"{item.ID}", item));
    }

    public async Task RemoveAsync(string id)
    {
        var peer = await Dispatcher.InvokeAsync(() =>
        {
            if (base.ContainsKey(id) == true)
            {
                var item = base[id];
                base.RemoveBase(id);

                return item;
            }
            return null;
        });
        if (peer != null)
        {
            await _instanceContext.DestroyInstanceAsync(peer);
            peer.Descriptor = null;
            peer.Dispose();
        }
    }

    public Dispatcher Dispatcher => _serviceContext.Dispatcher;
}
