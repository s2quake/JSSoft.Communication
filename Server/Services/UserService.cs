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
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    public class UserService : IUserService
    {
        private readonly IUserServiceCallback callback;

        private readonly Dictionary<string, UserInfo> userByID = new Dictionary<string, UserInfo>();
        private readonly Dictionary<Guid, UserInfo> userByToken = new Dictionary<Guid, UserInfo>();

        public UserService(IUserServiceCallback callback)
        {
            this.callback = callback;
            this.Dispatcher = new Dispatcher(this);
        }

        public Task CreateAsync(string userID, string password)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (userID == null)
                    throw new ArgumentNullException(nameof(userID));
                if (this.userByID.ContainsKey(userID) == true)
                    throw new ArgumentException("user is already exists.", nameof(userID));
                if (password == null)
                    throw new ArgumentNullException(nameof(password));
                var userInfo = new UserInfo()
                {
                    UserID = userID,
                    Password = password,
                    UserName = string.Empty,
                };
                this.userByID.Add(userID, userInfo);
            });
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
            return this.Dispatcher.InvokeAsync(()=>
            {
                if (this.userByToken.ContainsKey(token) == false)
                    throw new ArgumentException("invalid token", nameof(token));
                return this.userByID.Keys.ToArray();
            });
        }

        public Task<bool> IsOnlineAsync(Guid token, string userID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.userByToken.ContainsKey(token) == false)
                    throw new ArgumentException("invalid token", nameof(token));
                if (userID == null)
                    throw new ArgumentNullException(nameof(userID));
                if (this.userByID.ContainsKey(userID) == false)
                    throw new ArgumentException("invalid userID", nameof(userID));
                var user = this.userByID[userID];
                return user.Token != Guid.Empty;
            });
        }

        public Task<Guid> LoginAsync(string userID, string password)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (userID == null)
                    throw new ArgumentNullException(nameof(userID));
                if (this.userByID.ContainsKey(userID) == false)
                    throw new ArgumentException("invalid userID", nameof(userID));
                if (password == null)
                    throw new ArgumentNullException(nameof(password));
                var token = Guid.NewGuid();
                var user = this.userByID[userID];
                user.Token = token;
                this.userByToken.Add(token, user);
                this.callback.OnLoggedIn(userID);
                return token;
            });
        }

        public Task LogoutAsync(Guid token)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                if (this.userByToken.ContainsKey(token) == false)
                    throw new ArgumentException("invalid token.", nameof(token));
                var user = this.userByToken[token];
                user.Token = Guid.Empty;
                this.userByToken.Remove(token);
                this.callback.OnLoggedOut(user.UserID);
            });
        }

        public Task SendMessageAsync(Guid token, string userID, string message)
        {
            throw new NotImplementedException();
        }

        public Task SetUserInfoAsync(Guid token, string userName)
        {
            throw new NotImplementedException();
        }

        public Dispatcher Dispatcher { get; private set; }

        public void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }
    }
}
