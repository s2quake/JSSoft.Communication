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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace JSSoft.Communication.Grpc
{
    class PeerCollection : ContainerBase<Peer>
    {
        private static readonly TimeSpan timeout = new TimeSpan(0, 0, 30);
        private readonly AdaptorServerHost adaptorHost;
        private Timer timer;

        public PeerCollection(AdaptorServerHost adaptorHost)
        {
            this.adaptorHost = adaptorHost;
            this.CollectionChanged += PeerCollection_CollectionChanged;
        }

        public void Add(Peer item)
        {
            base.AddBase(item.ID, item);
            item.Disposed += Item_Disposed;
        }

        public void Remove(string peer)
        {
            var item = base[peer];
            item.Disposed -= Item_Disposed;
            base.RemoveBase(peer);
        }

        public Dispatcher Dispatcher => this.adaptorHost?.Dispatcher;

        private void PeerCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (this.timer == null)
                        {
                            var milliseconds = (int)timeout.TotalMilliseconds;
                            this.timer = new Timer(Timer_TimerCallback, null, milliseconds, milliseconds);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (this.Any() == false)
                        {
                            this.timer.Dispose();
                            this.timer = null;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        if (this.timer != null)
                        {
                            this.timer.Dispose();
                            this.timer = null;
                        }
                    }
                    break;
            }
        }

        private void Item_Disposed(object sender, EventArgs e)
        {
            if (sender is Peer item)
                base.RemoveBase(item.ID);
        }

        private void Timer_TimerCallback(object state)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                var dateTime = DateTime.UtcNow;
                var query = from item in this
                            where dateTime - item.PingTime > timeout
                            select item;
                var items = query.ToArray();
                foreach (var item in items)
                {
                    item.Abort();
                }
            });
        }
    }
}
