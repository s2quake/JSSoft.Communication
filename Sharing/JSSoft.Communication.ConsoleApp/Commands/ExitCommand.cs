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
using JSSoft.Library.Commands;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Commands
{
    [Export(typeof(ICommand))]
    class ExitCommand : CommandAsyncBase
    {
        private readonly IApplication _application;

        [ImportingConstructor]
        public ExitCommand(IApplication application)
        {
            _application = application;
        }

        [CommandPropertyRequired(DefaultValue = 0)]
        public int ExitCode
        {
            get; set;
        }

        protected override Task OnExecuteAsync(CancellationToken cancellationToken)
        {
            return _application.StopAsync(this.ExitCode);
        }
    }
}