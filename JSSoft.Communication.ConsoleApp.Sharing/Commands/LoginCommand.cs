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
using System.ComponentModel;
using System.Threading.Tasks;
using JSSoft.Communication.Services;
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
    class LoginCommand : CommandAsyncBase
    {
        private readonly Lazy<Shell> shell = null;
        private readonly Lazy<IUserService> userService = null;

#if MEF
        [ImportingConstructor]
#endif
        public LoginCommand(Lazy<Shell> shell, Lazy<IUserService> userService)
        {
            this.shell = shell;
            this.userService = userService;
        }

        [CommandPropertyRequired]
        public string UserID
        {
            get; set;
        }

        [CommandPropertyRequired]
        public string Password
        {
            get; set;
        }

        public override bool IsEnabled => this.Shell.UserToken == Guid.Empty;

        protected override async Task OnExecuteAsync()
        {
            var token = await this.UserService.LoginAsync(this.UserID, this.Password);
            this.Shell.Login(this.UserID, token);
        }

        private IUserService UserService => this.userService.Value;

        private Shell Shell => this.shell.Value;
    }
}