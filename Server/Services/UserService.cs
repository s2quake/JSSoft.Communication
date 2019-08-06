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
using Ntreev.Crema.Communication;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    public class UserService : IUserService
    {
        private readonly IUserServiceCallback callback;

        private readonly Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        public UserService(IUserServiceCallback callback)
        {
            this.callback = callback;
            this.Dispatcher = new Dispatcher(this);
        }

        public Task CreateAsync(string userID, string password)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid token, string userID)
        {
            throw new NotImplementedException();
        }

        public Task<(string, string)> GetUserInfoAsync(Guid token, string userID)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetUsersAsync(Guid token)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsOnlineAsync(Guid token, string userID)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> LoginAsync(string userID, string password)
        {
            return this.Dispatcher.InvokeAsync(()=>
            {
                this.callback.OnLoggedIn(userID);
                return Guid.NewGuid();
            });
        }

        public Task LogoutAsync(Guid token)
        {
            throw new NotImplementedException();
        }

        public Task SendMessageAsync(Guid token, string userID, string message)
        {
            throw new NotImplementedException();
        }

        public Task SetUserInfoAsync(Guid token, string userName)
        {
            throw new NotImplementedException();
        }

        public Dispatcher Dispatcher { get; private set;}

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }
    }
}
