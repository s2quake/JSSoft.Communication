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

namespace Ntreev.Crema.Communication.Grpc
{
    sealed class PeerDescriptor
    {
        public PeerDescriptor(string peer)
        {
            this.Peer = peer;
        }

        public void Dispose()
        {
            this.ServiceInstances.DisposeAll();
            this.CallbackInstances.DisposeAll();
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        public string Peer { get; }

        public IService[] Services { get; set; }

        public Guid Token { get; set; } = Guid.NewGuid();

        public DateTime Ping { get; set; }

        public Dictionary<IService, object> ServiceInstances { get; } = new Dictionary<IService, object>();

        public Dictionary<IService, object> CallbackInstances { get; } = new Dictionary<IService, object>();

        public Dictionary<IService, CallbackCollection> Callbacks { get; } = new Dictionary<IService, CallbackCollection>();

        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public event EventHandler Disposed;
    }
}