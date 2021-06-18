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
using System;
using System.Collections.Generic;
using System.Threading;

namespace JSSoft.Communication
{
    public sealed class PeerDescriptor
    {
        public void Dispose()
        {
            this.Callbacks.DisposeAll();
        }

        public void AddInstance(IServiceHost serviceHost, object service, object callback)
        {
            this.Services.Add(serviceHost, service);
            this.Callbacks.Add(serviceHost, callback);
        }

        public (object service, object callback) RemoveInstance(IServiceHost serviceHost)
        {
            var value = (this.Services[serviceHost], this.Callbacks[serviceHost]);
            this.Services.Remove(serviceHost);
            this.Callbacks.Remove(serviceHost);
            return value;
        }

        public Dictionary<IServiceHost, object> Services { get; } = new Dictionary<IServiceHost, object>();

        public Dictionary<IServiceHost, object> Callbacks { get; } = new Dictionary<IServiceHost, object>();
    }
}
