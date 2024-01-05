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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication;

sealed class InstanceContext : IInstanceContext, IPeer
{
    private readonly ConcurrentDictionary<IPeer, PeerDescriptor> _descriptorByPeer = new();
    private readonly PeerDescriptor _descriptor = new();
    private readonly ServiceContextBase _serviceContext;

    public InstanceContext(ServiceContextBase serviceContext)
    {
        _serviceContext = serviceContext;
        ID = Guid.NewGuid();
    }

    public async Task InitializeInstanceAsync(CancellationToken cancellationToken)
    {
        var query = from item in _serviceContext.ServiceHosts
                    where ServiceContextBase.IsPerPeer(_serviceContext, item) != true
                    select item;
        foreach (var item in query)
        {
            var (service, callback) = await Task.Run(() => _serviceContext.CreateInstance(item, this), cancellationToken);
            _descriptor.AddInstance(item, service, callback);
        }
    }

    public async Task ReleaseInstanceAsync(CancellationToken cancellationToken)
    {
        var isServer = ServiceContextBase.IsServer(_serviceContext);
        var query = from item in _serviceContext.ServiceHosts.Reverse()
                    where ServiceContextBase.IsPerPeer(_serviceContext, item) != true
                    select item;
        foreach (var item in query)
        {
            var (service, callback) = await Task.Run(() => _descriptor.RemoveInstance(item), cancellationToken);
            _serviceContext.DestroyInstance(item, this, service, callback);
        }
    }

    public PeerDescriptor CreateInstance(IPeer peer)
    {
        var peerDescriptor = new PeerDescriptor();
        foreach (var item in _serviceContext.ServiceHosts)
        {
            var isPerPeer = ServiceContextBase.IsPerPeer(_serviceContext, item);
            if (isPerPeer == true)
            {
                var (service, callback) = _serviceContext.CreateInstance(item, peer);
                peerDescriptor.AddInstance(item, service, callback);
            }
            else
            {
                var (service, callback) = (_descriptor.Services[item], _descriptor.Callbacks[item]);
                peerDescriptor.AddInstance(item, service, callback);
            }
        }
        _descriptorByPeer.TryAdd(peer, peerDescriptor);
        return peerDescriptor;
    }

    public void DestroyInstance(IPeer peer)
    {
        if (_descriptorByPeer.TryRemove(peer, out var peerDescriptor) == false)
            return;
        foreach (var item in _serviceContext.ServiceHosts.Reverse())
        {
            var isPerPeer = ServiceContextBase.IsPerPeer(_serviceContext, item);
            if (isPerPeer == true)
            {
                var (service, callback) = peerDescriptor.RemoveInstance(item);
                _serviceContext.DestroyInstance(item, peer, service, callback);
            }
            else
            {
                peerDescriptor.RemoveInstance(item);
            }
        }
        peerDescriptor.Dispose();

    }

    public object? GetService(Type serviceType)
    {
        var query = from descriptor in _descriptorByPeer.Values
                    from service in descriptor.Services.Values
                    where serviceType.IsAssignableFrom(service.GetType()) == true
                    select service;
        return query.SingleOrDefault();
    }

    public Guid ID { get; }
}
