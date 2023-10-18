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

using JSSoft.Library.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Services
{
    [Export(typeof(IUserService))]
    [Export(typeof(INotifyUserService))]
    [Export(typeof(UserService))]
    class UserService : IUserService, INotifyUserService
    {
        private readonly Dictionary<string, UserInfo> userByID = new();
        private readonly Dictionary<Guid, UserInfo> userByToken = new();

        private IUserServiceCallback callback;

        public UserService()
        {
            this.userByID.Add("admin", new UserInfo()
            {
                UserID = "admin",
                Password = "admin",
                UserName = "Administrator",
                Authority = Authority.Admin,
            });

            for (var i = 0; i < 10; i++)
            {
                var user = new UserInfo()
                {
                    UserID = $"user{i}",
                    Password = "1234",
                    UserName = $"사용자{i}",
                    Authority = Authority.Member,
                };
                this.userByID.Add(user.UserID, user);
            }
        }

        public Task CreateAsync(Guid token, string userID, string password, Authority authority)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateCreate(token, userID, password);

                var user = this.userByToken[token];
                var userInfo = new UserInfo()
                {
                    UserID = userID,
                    Password = password,
                    UserName = string.Empty,
                    Authority = authority
                };
                this.userByID.Add(userID, userInfo);
                this.callback.OnCreated(userID);
                this.OnCreated(new UserEventArgs(userID));
            });
        }

        public Task DeleteAsync(Guid token, string userID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateDelete(token, userID);

                this.userByID.Remove(userID);
                this.callback.OnDeleted(userID);
                this.OnDeleted(new UserEventArgs(userID));
            });
        }

        public Task<(string, Authority)> GetInfoAsync(Guid token, string userID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateUser(token);
                this.ValidateUser(userID);

                var user = this.userByID[userID];
                return (user.UserName, user.Authority);
            });
        }

        public Task<string[]> GetUsersAsync(Guid token)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateUser(token);

                return this.userByID.Keys.ToArray();
            });
        }

        public Task<bool> IsOnlineAsync(Guid token, string userID)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateUser(token);
                this.ValidateUser(userID);

                var user = this.userByID[userID];
                return user.Token != Guid.Empty;
            });
        }

        public Task<Guid> LoginAsync(string userID, string password)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidatePassword(userID, password);

                var token = Guid.NewGuid();
                var user = this.userByID[userID];
                user.Token = token;
                this.userByToken.Add(token, user);
                this.callback.OnLoggedIn(userID);
                this.OnLoggedIn(new UserEventArgs(userID));
                return token;
            });
        }

        public Task LogoutAsync(Guid token)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateUser(token);

                var user = this.userByToken[token];
                user.Token = Guid.Empty;
                this.userByToken.Remove(token);
                this.callback.OnLoggedOut(user.UserID);
                this.OnLoggedOut(new UserEventArgs(user.UserID));
            });
        }

        public Task SendMessageAsync(Guid token, string userID, string message)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateUser(token);
                this.ValidateOnline(userID);
                this.ValidateMessage(message);

                var user = this.userByToken[token];
                this.callback.OnMessageReceived(user.UserID, userID, message);
                this.OnMessageReceived(new UserMessageEventArgs(user.UserID, userID, message));
            });
        }

        public Task RenameAsync(Guid token, string userName)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateRename(token, userName);

                var user = this.userByToken[token];
                user.UserName = userName;
                this.callback.OnRenamed(user.UserID, userName);
                this.OnRenamed(new UserNameEventArgs(user.UserID, userName));
            });
        }

        public Task SetAuthorityAsync(Guid token, string userID, Authority authority)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                this.ValidateSetAuthority(token, userID);

                var user = this.userByID[userID];
                user.Authority = authority;
                this.callback.OnAuthorityChanged(userID, authority);
                this.OnAuthorityChanged(new UserAuthorityEventArgs(userID, authority));
            });
        }

        public async Task DisposeAsync()
        {
            if (this.Dispatcher == null)
                throw new ObjectDisposedException(nameof(UserService));
            await this.Dispatcher.DisposeAsync();
            this.Dispatcher = null;
        }

        public Dispatcher Dispatcher { get; private set; }

        public event EventHandler<UserEventArgs> Created;

        public event EventHandler<UserEventArgs> Deleted;

        public event EventHandler<UserEventArgs> LoggedIn;

        public event EventHandler<UserEventArgs> LoggedOut;

        public event EventHandler<UserMessageEventArgs> MessageReceived;

        public event EventHandler<UserNameEventArgs> Renamed;

        public event EventHandler<UserAuthorityEventArgs> AuthorityChanged;

        internal void SetCallback(IUserServiceCallback callback)
        {
            this.callback = callback;
            this.Dispatcher = new Dispatcher(this);
        }

        protected virtual void OnCreated(UserEventArgs e)
        {
            this.Created?.Invoke(this, e);
        }

        protected virtual void OnDeleted(UserEventArgs e)
        {
            this.Deleted?.Invoke(this, e);
        }

        protected virtual void OnLoggedIn(UserEventArgs e)
        {
            this.LoggedIn?.Invoke(this, e);
        }

        protected virtual void OnLoggedOut(UserEventArgs e)
        {
            this.LoggedOut?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(UserMessageEventArgs e)
        {
            this.MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnRenamed(UserNameEventArgs e)
        {
            this.Renamed?.Invoke(this, e);
        }

        protected virtual void OnAuthorityChanged(UserAuthorityEventArgs e)
        {
            this.AuthorityChanged?.Invoke(this, e);
        }

        private void ValidateUser(string userID)
        {
            this.Dispatcher.VerifyAccess();
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.userByID.ContainsKey(userID) == false)
                throw new ArgumentException("invalid userID", nameof(userID));
        }

        private void ValidateNotUser(string userID)
        {
            this.Dispatcher.VerifyAccess();
            if (userID == null)
                throw new ArgumentNullException(nameof(userID));
            if (this.userByID.ContainsKey(userID) == true)
                throw new ArgumentException("user is already exists.", nameof(userID));
        }

        private void ValidateUser(Guid token)
        {
            this.Dispatcher.VerifyAccess();
            if (this.userByToken.ContainsKey(token) == false)
                throw new ArgumentException("invalid token.", nameof(token));
        }

        private void ValidatePassword(string password)
        {
            this.Dispatcher.VerifyAccess();
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (password == string.Empty)
                throw new ArgumentException("invalid password.", nameof(password));
            if (password.Length < 4)
                throw new ArgumentException("length of password must be greater or equal than 4.", nameof(password));
        }

        private void ValidatePassword(string userID, string password)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(userID);
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            var user = this.userByID[userID];
            if (user.Password != password)
                throw new InvalidOperationException("wrong userID or password.");
        }

        private void ValidateCreate(Guid token, string userID, string password)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(token);
            this.ValidateNotUser(userID);
            this.ValidatePassword(password);
            var user = this.userByToken[token];
            if (user.Authority != Authority.Admin)
                throw new InvalidOperationException("permission denied.");
        }

        private void ValidateMessage(string message)
        {
            this.Dispatcher.VerifyAccess();
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }

        private void ValidateOnline(string userID)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(userID);
            var user = this.userByID[userID];

            if (user.Token == Guid.Empty)
                throw new InvalidOperationException($"user is offline.");
        }

        private void ValidateRename(Guid token, string userName)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(token);
            if (userName == null)
                throw new ArgumentNullException(nameof(userName));
            if (userName == string.Empty)
                throw new ArgumentException("invalid name.", nameof(userName));
            var user = this.userByToken[token];
            if (user.UserName == userName)
                throw new ArgumentException("same name can not set.", nameof(userName));
            if (user.UserID == "admin")
                throw new InvalidOperationException("permission denied.");
        }

        private void ValidateSetAuthority(Guid token, string userID)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(token);
            this.ValidateUser(userID);
            var user1 = this.userByToken[token];
            if (user1.UserID == userID)
                throw new InvalidOperationException("can not set authority.");
            if (user1.Authority != Authority.Admin)
                throw new InvalidOperationException("permission denied.");
            var user2 = this.userByID[userID];
            if (user2.Token != Guid.Empty)
                throw new InvalidOperationException("can not set authority of online user.");
            if (userID == "admin")
                throw new InvalidOperationException("permission denied.");
        }

        private void ValidateDelete(Guid token, string userID)
        {
            this.Dispatcher.VerifyAccess();
            this.ValidateUser(token);
            this.ValidateNotUser(userID);
            var user1 = this.userByToken[token];
            if (user1.Authority != Authority.Admin)
                throw new InvalidOperationException("permission denied.");
            var user2 = this.userByID[userID];
            if (user2.Token != Guid.Empty)
                throw new InvalidOperationException("can not delete online user.");
            if (userID == "admin")
                throw new InvalidOperationException("permission denied.");
        }
    }
}
