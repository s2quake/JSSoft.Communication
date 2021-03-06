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

using JSSoft.Communication.Services;
using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication.ConsoleApp
{
    [Export(typeof(IServiceContext))]
    class ClientContext : ClientContextBase
    {
        [ImportingConstructor]
        public ClientContext(IComponentProvider componentProvider, [ImportMany] IServiceHost[] serviceHosts)
            : base(componentProvider, serviceHosts)
        {

        }

        protected override InstanceBase CreateInstance(Type type)
        {
            if (type == typeof(IUserService))
                return new UserServiceInstance();
            return base.CreateInstance(type);
        }
    }

    class UserServiceInstance : InstanceBase, IUserService
    {
        public Task CreateAsync(Guid token, string userID, string password, Authority authority)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID, password, authority));
        }

        public Task DeleteAsync(Guid token, string userID)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID));
        }

        public Task<(string userName, Authority authority)> GetInfoAsync(Guid token, string userID)
        {
            return this.InvokeAsync<(string, Authority)>(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID));
        }

        public Task<string[]> GetUsersAsync(Guid token)
        {
            return this.InvokeAsync<string[]>(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token));
        }

        public Task<bool> IsOnlineAsync(Guid token, string userID)
        {
            return this.InvokeAsync<bool>(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID));
        }

        public Task<Guid> LoginAsync(string userID, string password)
        {
            return this.InvokeAsync<Guid>(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), userID, password));
        }

        public Task LogoutAsync(Guid token)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token));
        }

        public Task RenameAsync(Guid token, string userName)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userName));
        }

        public Task SendMessageAsync(Guid token, string userID, string message)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID, message));
        }

        public Task SetAuthorityAsync(Guid token, string userID, Authority authority)
        {
            return this.InvokeAsync(Info(MethodInfo.GetCurrentMethod() as MethodInfo, typeof(IUserService), token, userID, authority));
        }
    }

}