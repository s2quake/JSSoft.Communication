using System;
using System.Collections;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication.Grpc
{
    class CallbackCollection : IEnumerable<PollReplyItem>, IReadOnlyList<PollReplyItem>
    {
        private readonly List<PollReplyItem> itemList = new List<PollReplyItem>();
        private readonly IService service;

        public CallbackCollection(IService service)
        {
            this.service = service;

        }

        public void Add(PollReplyItem item)
        {
            this.itemList.Add(item);
        }

        public void Remove(PollReplyItem item)
        {
            this.itemList.Remove(item);
        }

        public int IndexOf(PollReplyItem item)
        {
            return this.itemList.IndexOf(item);
        }

        public PollReplyItem this[int index]
        {
            get => this.itemList[index];
        }

        public bool Contains(PollReplyItem item)
        {
            return this.itemList.Contains(item);
        }

        public int Count => this.itemList.Count;

        #region IEnumerable

        IEnumerator<PollReplyItem> IEnumerable<PollReplyItem>.GetEnumerator()
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