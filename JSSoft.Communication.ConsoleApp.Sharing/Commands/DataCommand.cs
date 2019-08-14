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
using Ntreev.Library.Commands;
using System.Threading.Tasks;
using JSSoft.Communication.Services;
using JSSoft.Communication.ConsoleApp;
#if MEF
using System.ComponentModel.Composition;
#endif

namespace JSSoft.Communication.Commands
{
#if MEF
    [Export(typeof(ICommand))]
#endif
    class DataCommand : CommandMethodBase
    {
        private readonly Lazy<Shell> shell = null;
        private readonly Lazy<IDataService> dataService = null;

#if MEF
        [ImportingConstructor]
#endif
        public DataCommand(Lazy<Shell> shell, Lazy<IDataService> dataService)
        {
            this.shell = shell;
            this.dataService = dataService;
        }

        [CommandMethod]
        public Task CreateAsync(string dataBaseName)
        {
            return this.DataService.CreateDataBaseAsync(dataBaseName);
        }

        public override bool IsEnabled => this.Shell.UserToken != Guid.Empty;

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            return this.Shell.UserToken != Guid.Empty;
        }

        private IDataService DataService => this.dataService.Value;

        private Shell Shell => this.shell.Value;
    }
}
