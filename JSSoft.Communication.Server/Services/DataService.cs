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

using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Services;

[Export(typeof(IDataService))]
[Export(typeof(DataService))]
class DataService : IDataService
{
    private readonly HashSet<string> _dataBases = new();

    public DataService()
    {
        this.Dispatcher = new Dispatcher(this);
    }

    public Task<DateTime> CreateDataBaseAsync(string dataBaseName)
    {
        return this.Dispatcher.InvokeAsync(() =>
        {
            if (this._dataBases.Contains(dataBaseName) == true)
                throw new ArgumentNullException(nameof(dataBaseName));
            this._dataBases.Add(dataBaseName);
            return DateTime.UtcNow;
        });
    }

    public Dispatcher Dispatcher { get; private set; }

    public void Dispose()
    {
        this.Dispatcher.Dispose();
        this.Dispatcher = null;
    }
}
