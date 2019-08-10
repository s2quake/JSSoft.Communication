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
#if MEF
using System.ComponentModel.Composition;
#endif

namespace JSSoft.Communication.Services
{
#if MEF
    [Export(typeof(IUserService))]
    [Export(typeof(INotifyUserService))]
    [Export(typeof(UserService))]
#endif
    class UserService : IUserService, IUserServiceCallback, INotifyUserService
    {
        private IUserService userService;

        public Task CreateAsync(Guid token, string userID, string password, Authority authority)
        {
            return this.userService.CreateAsync(token, userID, password, authority);
        }

        public Task DeleteAsync(Guid token, string userID)
        {
            return this.userService.DeleteAsync(token, userID);
        }

        public Task<(string userName, Authority authority)> GetInfoAsync(Guid token, string userID)
        {
            return this.userService.GetInfoAsync(token, userID);
        }

        public Task<string[]> GetUsersAsync(Guid token)
        {
            return this.userService.GetUsersAsync(token);
        }

        public Task<bool> IsOnlineAsync(Guid token, string userID)
        {
            return this.userService.IsOnlineAsync(token, userID);
        }

        public Task<Guid> LoginAsync(string userID, string password)
        {
            return this.userService.LoginAsync(userID, password);
        }

        public Task LogoutAsync(Guid token)
        {
            return this.userService.LogoutAsync(token);
        }

        public Task RenameAsync(Guid token, string userName)
        {
            return this.userService.RenameAsync(token, userName);
        }

        public Task SendMessageAsync(Guid token, string userID, string message)
        {
            return this.userService.SendMessageAsync(token, userID, message);
        }

        public Task SetAuthorityAsync(Guid token, string userID, Authority authority)
        {
            return this.userService.SetAuthorityAsync(token, userID, authority);
        }

        public void SetUserService(IUserService userService)
        {
            this.userService = userService;
        }

        public event EventHandler<UserEventArgs> LoggedIn;

        public event EventHandler<UserEventArgs> LoggedOut;

        public event EventHandler<UserEventArgs> Created;

        public event EventHandler<UserEventArgs> Deleted;

        public event EventHandler<UserMessageEventArgs> MessageReceived;

        public event EventHandler<UserNameEventArgs> Renamed;

        public event EventHandler<UserAuthorityEventArgs> AuthorityChanged;

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

        #region IUserServiceCallback

        void IUserServiceCallback.OnCreated(string userID)
        {
            this.OnCreated(new UserEventArgs(userID));
        }

        void IUserServiceCallback.OnDeleted(string userID)
        {
            this.OnDeleted(new UserEventArgs(userID));
        }

        void IUserServiceCallback.OnLoggedIn(string userID)
        {
            this.OnLoggedIn(new UserEventArgs(userID));
        }

        void IUserServiceCallback.OnLoggedOut(string userID)
        {
            this.OnLoggedOut(new UserEventArgs(userID));
        }

        void IUserServiceCallback.OnMessageReceived(string sender, string receiver, string message)
        {
            this.OnMessageReceived(new UserMessageEventArgs(sender, receiver, message));
        }

        void IUserServiceCallback.OnRenamed(string userID, string userName)
        {
            this.OnRenamed(new UserNameEventArgs(userID, userName));
        }

        void IUserServiceCallback.OnAuthorityChanged(string userID, Authority authority)
        {
            this.OnAuthorityChanged(new UserAuthorityEventArgs(userID, authority));
        }

        #endregion
    }
}