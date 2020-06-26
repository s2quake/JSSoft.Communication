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
using System.Threading.Tasks;
using JSSoft.Communication.ConsoleApp;
using Ntreev.Library.Commands;
#if MEF
using System.ComponentModel.Composition;
#endif

namespace JSSoft.Communication.Commands
{
#if MEF
    [Export(typeof(ICommand))]
#endif
    class OpenCommand : CommandAsyncBase
    {
        private readonly IServiceContext serviceHost;
        private readonly Lazy<Shell> shell = null;

#if MEF
        [ImportingConstructor]
#endif
        public OpenCommand(IServiceContext serviceHost, Lazy<Shell> shell)
        {
            this.serviceHost = serviceHost;
            this.shell = shell;
        }

        public override bool IsEnabled => this.serviceHost.ServiceState == ServiceState.None;

        protected override async Task OnExecuteAsync(object source)
        {
            this.Shell.Token = await this.serviceHost.OpenAsync();
        }

        private Shell Shell => this.shell.Value;
    }
}