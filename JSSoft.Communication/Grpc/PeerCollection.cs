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

using System.Collections.Concurrent;

namespace JSSoft.Communication.Grpc;

sealed class PeerCollection : ConcurrentDictionary<string, Peer>
{
    private readonly IInstanceContext _instanceContext;

    public PeerCollection(IInstanceContext instanceContext)
    {
        _instanceContext = instanceContext;
    }

    public void Add(Peer item)
    {
        var descriptor = _instanceContext.CreateInstance(item);
        item.Descriptor = descriptor;
        TryAdd($"{item.ID}", item);
    }

    public void Remove(string id, string reason)
    {
        Logging.LogUtility.Debug($"{id} {reason}");
        if (TryRemove(id, out var peer) == true)
        {
            _instanceContext.DestroyInstance(peer);
            peer.Descriptor = null;
            peer.Dispose();
        }
    }
}
