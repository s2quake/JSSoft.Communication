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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ntreev.Crema.Communication
{
    public class ServiceHostCollection : IEnumerable<IServiceHost>, IReadOnlyList<IServiceHost>
    {
        private readonly List<IServiceHost> itemList;
        private readonly IService serviceHost;

        internal ServiceHostCollection(IService serviceHost)
            : this(serviceHost, Enumerable.Empty<IServiceHost>())
        {
            this.serviceHost = serviceHost;
        }

        internal ServiceHostCollection(IService serviceHost, IEnumerable<IServiceHost> services)
        {
            this.serviceHost = serviceHost;
            this.itemList = new List<IServiceHost>(services); 
        }

        public int IndexOf(IServiceHost item)
        {
            return this.itemList.IndexOf(item);
        }

        public IServiceHost this[int index]
        {
            get => this.itemList[index];
        }

        public bool Contains(IServiceHost item)
        {
            return this.itemList.Contains(item);
        }

        public int Count => this.itemList.Count;

        #region IEnumerable

        IEnumerator<IServiceHost> IEnumerable<IServiceHost>.GetEnumerator()
        {
            foreach (var item in this.itemList)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this.itemList)
            {
                yield return item;
            }
        }

        #endregion
    }
}