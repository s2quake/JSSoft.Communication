using System;
using System.Collections;
using System.Collections.Generic;

namespace Ntreev.Crema.Services
{
    class ServiceCollection<T> : IEnumerable<T> where T : IService
    {
        private readonly List<T> itemList = new List<T>();
        private readonly IServiceHost serviceHost;

        internal ServiceCollection(IServiceHost serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public void Add(T item)
        {
            this.itemList.Add(item);
        }

        public void Remove(T item)
        {
            this.itemList.Remove(item);
        }

        public int IndexOf(T item)
        {
            return this.itemList.IndexOf(item);
        }

        public T this[int index]
        {
            get => this.itemList[index];
        }

        public bool Contains(T item)
        {
            return this.itemList.Contains(item);
        }

        #region IEnumerable

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
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