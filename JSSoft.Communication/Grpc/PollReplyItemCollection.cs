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

using System.Collections;
using System.Collections.Generic;

namespace JSSoft.Communication.Grpc;

class PollReplyItemCollection : IEnumerable<PollReplyItem>, IReadOnlyList<PollReplyItem>
{
    private readonly List<PollReplyItem> _itemList = new();

    public PollReplyItemCollection()
    {
    }

    public void Add(PollReplyItem item)
    {
        this._itemList.Add(item);
    }

    public void Remove(PollReplyItem item)
    {
        this._itemList.Remove(item);
    }

    public int IndexOf(PollReplyItem item)
    {
        return this._itemList.IndexOf(item);
    }

    public PollReplyItem this[int index] => this._itemList[index];

    public bool Contains(PollReplyItem item)
    {
        return this._itemList.Contains(item);
    }

    public PollReplyItem[] Flush()
    {
        var items = this._itemList.ToArray();
        this._itemList.Clear();
        return items;
    }

    public int Count => this._itemList.Count;

    #region IEnumerable

    IEnumerator<PollReplyItem> IEnumerable<PollReplyItem>.GetEnumerator()
    {
        foreach (var item in this._itemList)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in this._itemList)
        {
            yield return item;
        }
    }

    #endregion
}
