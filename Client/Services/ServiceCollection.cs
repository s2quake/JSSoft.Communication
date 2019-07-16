using System;
using System.Collections;
using System.Collections.Generic;

namespace Ntreev.Crema.Services
{
    class ServiceCollection : IEnumerable<ServiceBase>
    {
        private readonly List<ServiceBase> itemList = new List<ServiceBase>();
        private readonly ServiceHostBase serviceHost;

        internal ServiceCollection(ServiceHostBase serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public void Add(ServiceBase item)
        {
            this.itemList.Add(item);
        }

        public void Remove(ServiceBase item)
        {
            this.itemList.Remove(item);
        }

        public int IndexOf(ServiceBase item)
        {
            return this.itemList.IndexOf(item);
        }

        public ServiceBase this[int index]
        {
            get => this.itemList[index];
        }

        public bool Contains(ServiceBase item)
        {
            return this.itemList.Contains(item);
        }

        #region IEnumerable

        IEnumerator<ServiceBase> IEnumerable<ServiceBase>.GetEnumerator()
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