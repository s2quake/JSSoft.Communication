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
using System.Threading.Tasks;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services.Services
{
    class DataService : IDataService
    {
        private readonly Dictionary<string, string> typeByName = new Dictionary<string, string>();
        private readonly Dictionary<string, string> tableByName = new Dictionary<string, string>();
        private readonly IDataServiceCallback callback;

        public DataService(IDataServiceCallback callback)
        {
            this.callback = callback;
            this.Dispatcher = new Dispatcher(this);
        }

        public Task<DateTime> CreateTypeAsync(string typeName)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.typeByName.ContainsKey(typeName) == true)
                    throw new ArgumentNullException(nameof(typeByName));
                this.typeByName.Add(typeName, string.Empty);
                return DateTime.UtcNow;
            });
        }

        public Task<DateTime> CreateTableAsync(string tableName)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.tableByName.ContainsKey(tableName) == true)
                    throw new ArgumentNullException(nameof(tableName));
                this.tableByName.Add(tableName, string.Empty);
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
}