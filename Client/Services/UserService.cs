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
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Services
{
    [Export(typeof(IUserService))]
    [Export(typeof(INotifyUserService))]
    [Export(typeof(UserService))]
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

        protected virtual void OnLoggedIn(UserEventArgs e)
        {
            this.LoggedIn?.Invoke(this, e);
        }

        protected virtual void OnLoggedOut(UserEventArgs e)
        {
            this.LoggedOut?.Invoke(this, e);
        }

        protected virtual void OnCreated(UserEventArgs e)
        {
            this.Created?.Invoke(this, e);
        }

        protected virtual void OnDeleted(UserEventArgs e)
        {
            this.Deleted?.Invoke(this, e);
        }

        #region IUserServiceCallback

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
            // Console.WriteLine($"[{userID}]");
        }

        void IUserServiceCallback.OnRenamed(string userID, string userName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}