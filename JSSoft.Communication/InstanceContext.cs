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

using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JSSoft.Communication;

sealed class InstanceContext : IInstanceContext, IPeer
{
    private readonly Dictionary<IPeer, PeerDescriptor> _descriptorByPeer = new();
    private readonly PeerDescriptor _descriptor = new();
    private readonly ServiceContextBase _serviceContext;

    public InstanceContext(ServiceContextBase serviceContext)
    {
        _serviceContext = serviceContext;
        ID = Guid.NewGuid();
    }

    public async Task InitializeInstanceAsync()
    {
        var query = from item in _serviceContext.ServiceHosts
                    where ServiceContextBase.IsPerPeer(_serviceContext, item) == false
                    select item;
        foreach (var item in query)
        {
            var (service, callback) = await _serviceContext.CreateInstanceAsync(item, this);
            _descriptor.AddInstance(item, service, callback);
        }
    }

    public async Task ReleaseInstanceAsync()
    {
        var isServer = ServiceContextBase.IsServer(_serviceContext);
        var query = from item in _serviceContext.ServiceHosts.Reverse()
                    where ServiceContextBase.IsPerPeer(_serviceContext, item) == false
                    select item;
        foreach (var item in query)
        {
            var (service, callback) = _descriptor.RemoveInstance(item);
            await _serviceContext.DestroyInstanceAsync(item, this, service, callback);
        }
    }

    public async Task<PeerDescriptor> CreateInstanceAsync(IPeer peer)
    {
        var peerDescriptor = new PeerDescriptor();
        foreach (var item in _serviceContext.ServiceHosts)
        {
            var isPerPeer = ServiceContextBase.IsPerPeer(_serviceContext, item);
            if (isPerPeer == true)
            {
                var (service, callback) = await _serviceContext.CreateInstanceAsync(item, peer);
                peerDescriptor.AddInstance(item, service, callback);
            }
            else
            {
                var (service, callback) = (_descriptor.Services[item], _descriptor.Callbacks[item]);
                peerDescriptor.AddInstance(item, service, callback);
            }
        }
        _descriptorByPeer.Add(peer, peerDescriptor);
        return peerDescriptor;
    }

    public async Task DestroyInstanceAsync(IPeer peer)
    {
        var peerDescriptor = _descriptorByPeer[peer];
        foreach (var item in _serviceContext.ServiceHosts.Reverse())
        {
            var isPerPeer = ServiceContextBase.IsPerPeer(_serviceContext, item);
            if (isPerPeer == true)
            {
                var (service, callback) = peerDescriptor.RemoveInstance(item);
                await _serviceContext.DestroyInstanceAsync(item, peer, service, callback);
            }
            else
            {
                peerDescriptor.RemoveInstance(item);
            }
        }
        peerDescriptor.Dispose();
        _descriptorByPeer.Remove(peer);
    }

    public Dispatcher Dispatcher => _serviceContext.Dispatcher;

    public Guid ID { get; }
}
