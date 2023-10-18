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

using JSSoft.Communication.ConsoleApp;
using JSSoft.Communication.Services;
using JSSoft.Library.Commands;
using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Commands
{
    [Export(typeof(ICommand))]
    class DataCommand : CommandMethodBase
    {
        private readonly Application _application = null;
        private readonly Lazy<IDataService> _dataServiceLazy = null;

        [ImportingConstructor]
        public DataCommand(Application application, Lazy<IDataService> dataServiceLazy)
        {
            _application = application;
            _dataServiceLazy = dataServiceLazy;
        }

        [CommandMethod]
        public Task CreateAsync(string dataBaseName)
        {
            return this.DataService.CreateDataBaseAsync(dataBaseName);
        }

        public override bool IsEnabled => _application.UserToken != Guid.Empty;

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            return _application.UserToken != Guid.Empty;
        }

        private IDataService DataService => _dataServiceLazy.Value;
    }
}
