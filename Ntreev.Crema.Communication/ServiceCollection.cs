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
    public class ServiceCollection : IEnumerable<IService>, IReadOnlyList<IService>
    {
        private readonly List<IService> itemList;
        private readonly IServiceHost serviceHost;

        internal ServiceCollection(IServiceHost serviceHost)
            : this(serviceHost, Enumerable.Empty<IService>())
        {
            this.serviceHost = serviceHost;
        }

        internal ServiceCollection(IServiceHost serviceHost, IEnumerable<IService> services)
        {
            this.serviceHost = serviceHost;
            this.itemList = new List<IService>(services); 
        }

        public int IndexOf(IService item)
        {
            return this.itemList.IndexOf(item);
        }

        public IService this[int index]
        {
            get => this.itemList[index];
        }

        public bool Contains(IService item)
        {
            return this.itemList.Contains(item);
        }

        public int Count => this.itemList.Count;

        #region IEnumerable

        IEnumerator<IService> IEnumerable<IService>.GetEnumerator()
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