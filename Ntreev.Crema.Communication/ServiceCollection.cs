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