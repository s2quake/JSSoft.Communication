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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;
using Server;

namespace Server.Commands
{
    [Export(typeof(ICommand))]
    class LogoutCommand : CommandAsyncBase
    {
        [Import]
        private Lazy<Shell> shell = null;
        [Import]
        private Lazy<IUserService> userService = null;

        public LogoutCommand()
        {

        }

        public override bool IsEnabled => this.Shell.UserToken != Guid.Empty;

        protected override async Task OnExecuteAsync()
        {
            await this.UserService.LogoutAsync(this.Shell.UserToken);
            this.Shell.Logout();
        }

        private IUserService UserService => this.userService.Value;

        private Shell Shell => this.shell.Value;
    }
}