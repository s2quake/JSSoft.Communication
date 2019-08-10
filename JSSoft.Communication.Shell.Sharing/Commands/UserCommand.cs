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
using JSSoft.Communication.Shell.Services;
#if MEF
using System.ComponentModel.Composition;
#endif

namespace JSSoft.Communication.Shell.Commands
{
#if MEF
    [Export(typeof(ICommand))]
#endif
    class UserCommand : CommandMethodBase
    {
        private readonly Lazy<Shell> shell = null;
        private readonly Lazy<IUserService> userService = null;

#if MEF
        [ImportingConstructor]
#endif
        public UserCommand(Lazy<Shell> shell, Lazy<IUserService> userService)
        {
            this.shell = shell;
            this.userService = userService;
        }

        [CommandMethod]
        public Task CreateAsync(string userID, string password, Authority authority = Authority.Member)
        {
            return this.UserService.CreateAsync(this.Shell.UserToken, userID, password, Authority.Admin);
        }

        [CommandMethod]
        public Task DeleteAsync(string userID)
        {
            return this.UserService.DeleteAsync(this.Shell.UserToken, userID);
        }

        [CommandMethod]
        public Task RenameAsync(string userName)
        {
            return this.UserService.RenameAsync(this.Shell.UserToken, userName);
        }

        [CommandMethod]
        public Task AuthorityAsync(string userID, Authority authority)
        {
            return this.UserService.SetAuthorityAsync(this.Shell.UserToken, userID, authority);
        }

        [CommandMethod]
        public async Task InfoAsync(string userID)
        {
            var (userName, authority) = await this.UserService.GetInfoAsync(this.Shell.UserToken, userID);
            Console.WriteLine($"UseName: {userName}");
            Console.WriteLine($"Authority: {authority}");
        }

        [CommandMethod]
        public async Task ListAsync()
        {
            var items = await this.UserService.GetUsersAsync(this.Shell.UserToken);
            foreach (var item in items)
            {
                Console.WriteLine(item);
            }
        }

        [CommandMethod]
        public Task SendMessageAsync(string userID, string message)
        {
            return this.UserService.SendMessageAsync(this.Shell.UserToken, userID, message);
        }

        public override bool IsEnabled => this.Shell.UserToken != Guid.Empty;

        protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
        {
            return this.Shell.UserToken != Guid.Empty;
        }

        private IUserService UserService => this.userService.Value;

        private Shell Shell => this.shell.Value;
    }
}
