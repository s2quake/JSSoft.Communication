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
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;

namespace Client.Services
{
    [Export(typeof(IServiceHost))]
    class UserServiceHost : ClientServiceHostBase<IUserService, IUserServiceCallback>, IUserServiceCallback, INotifyPropertyChanged, INotifyUserService
    {
        private IUserService userService;

        public UserServiceHost()
            : base()
        {

        }

        

        public override object CreateInstance(object obj)
        {
            this.userService = obj as IUserService;
            this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(UserService)));
            return this;
        }

        public override void DestroyInstance(object obj)
        {
            
        }

        [Export(typeof(IUserService))]
        public IUserService UserService => this.userService;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<UserEventArgs> LoggedIn;

        public event EventHandler<UserEventArgs> LoggedOut;

        public event EventHandler<UserEventArgs> Created;

        public event EventHandler<UserEventArgs> Deleted;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

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
            // if (receiver)
            // Console.WriteLine($"[{userID}]");
        }

        void IUserServiceCallback.OnRenamed(string userID, string userName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
