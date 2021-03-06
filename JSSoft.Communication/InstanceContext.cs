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
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication
{
    sealed class InstanceContext : IInstanceContext, IPeer
    {
        private readonly Dictionary<IPeer, PeerDescriptor> descriptorByPeer = new();
        private readonly PeerDescriptor descriptor = new();
        private readonly ServiceContextBase serviceContext;

        public InstanceContext(ServiceContextBase serviceContext)
        {
            this.serviceContext = serviceContext;
            this.ID = Guid.NewGuid();
        }

        public async Task InitializeInstanceAsync()
        {
            var query = from item in this.serviceContext.ServiceHosts
                        where ServiceContextBase.IsPerPeer(this.serviceContext, item) == false
                        select item;
            foreach (var item in query)
            {
                var (service, callback) = await this.serviceContext.CreateInstanceAsync(item, this);
                this.descriptor.AddInstance(item, service, callback);
            }
        }

        public async Task ReleaseInstanceAsync()
        {
            var isServer = ServiceContextBase.IsServer(this.serviceContext);
            var query = from item in this.serviceContext.ServiceHosts.Reverse()
                        where ServiceContextBase.IsPerPeer(this.serviceContext, item) == false
                        select item;
            foreach (var item in query)
            {
                var (service, callback) = this.descriptor.RemoveInstance(item);
                await this.serviceContext.DestroyInstanceAsync(item, this, service, callback);
            }
        }

        public async Task<PeerDescriptor> CreateInstanceAsync(IPeer peer)
        {
            var peerDescriptor = new PeerDescriptor();
            foreach (var item in this.serviceContext.ServiceHosts)
            {
                var isPerPeer = ServiceContextBase.IsPerPeer(this.serviceContext, item);
                if (isPerPeer == true)
                {
                    var (service, callback) = await this.serviceContext.CreateInstanceAsync(item, peer);
                    peerDescriptor.AddInstance(item, service, callback);
                }
                else
                {
                    var (service, callback) = (descriptor.Services[item], descriptor.Callbacks[item]);
                    peerDescriptor.AddInstance(item, service, callback);
                }
            }
            this.descriptorByPeer.Add(peer, peerDescriptor);
            return peerDescriptor;
        }

        public async Task DestroyInstanceAsync(IPeer peer)
        {
            var peerDescriptor = this.descriptorByPeer[peer];
            foreach (var item in this.serviceContext.ServiceHosts.Reverse())
            {
                var isPerPeer = ServiceContextBase.IsPerPeer(this.serviceContext, item);
                if (isPerPeer == true)
                {
                    var (service, callback) = peerDescriptor.RemoveInstance(item);
                    await this.serviceContext.DestroyInstanceAsync(item, peer, service, callback);
                }
                else
                {
                    peerDescriptor.RemoveInstance(item);
                }
            }
            peerDescriptor.Dispose();
            this.descriptorByPeer.Remove(peer);
        }

        public Dispatcher Dispatcher => this.serviceContext.Dispatcher;

        public Guid ID { get; }
    }
}
