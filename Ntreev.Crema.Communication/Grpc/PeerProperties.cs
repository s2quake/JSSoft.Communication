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
using System.Collections.Generic;

namespace Ntreev.Crema.Communication.Grpc
{
    class PeerProperties
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public void Add(string peer, string key, object value)
        {
            this.properties.Add(GetKey(peer, key), value);
        }

        public void Contains(string peer, string key)
        {
            this.properties.ContainsKey(GetKey(peer, key));
        }

        public void Remove(string peer, string key)
        {
            this.properties.Remove(GetKey(peer, key));
        }

        public object this[string peer, string key]
        {
            get => this.properties[GetKey(peer, key)];
            set => this.properties[GetKey(peer, key)] = value;
        }

        private static string GetKey(string peer, string key)
        {
            return $"{peer}.{key}";
        }
    }
}
