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
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceBase : IService, IDisposable
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        // private object instance;
        private Dispatcher dispatcher;

        internal ServiceBase(Type serviceType, Type callbackType, Type validationType)
        {
            if (validationType.IsAssignableFrom(this.GetType()) == false)
                throw new ArgumentException("invalid type", nameof(validationType));
            this.Name = serviceType.Name;
            this.serviceType = serviceType;
            this.callbackType = callbackType;
            this.dispatcher = new Dispatcher(this);
        }

        // protected object Instance => this.instance;

        public Type ServiceType => this.serviceType;

        public Type CallbackType => this.callbackType;

        public Dispatcher Dispatcher => this.dispatcher;

        public async Task OpenAsync(ServiceToken token)
        {
            await this.dispatcher.InvokeAsync(() =>
            {
                // this.instance = instance;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync(ServiceToken token)
        {
            await this.dispatcher.InvokeAsync(() =>
            {
                // this.instance = null;
                this.OnClosed(EventArgs.Empty);
            });
        }

        public string Name { get; }

        public event EventHandler Opened;

        public event EventHandler Closed;

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        internal void Dispose()
        {
            this.dispatcher.Dispose();
            this.dispatcher = null;
        }

        #region IDisposable

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        #endregion
    }
}
