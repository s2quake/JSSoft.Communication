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
using System.Reflection;
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceHostBase : IServiceHost, IDisposable
    {
        private ServiceToken token;

        internal ServiceHostBase(Type serviceType, Type callbackType)
        {
            this.Name = serviceType.Name;
            this.ServiceType = serviceType;
            this.CallbackType = callbackType;
            this.Dispatcher = new Dispatcher(this);
            this.MethodDescriptors = new MethodDescriptorCollection(this);
        }

        public Type ServiceType { get; }

        public Type CallbackType { get; }

        public Dispatcher Dispatcher { get; private set; }

        public MethodDescriptorCollection MethodDescriptors { get; }

        public async Task OpenAsync(ServiceToken token)
        {
            this.token = token;
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync(ServiceToken token)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.OnClosed(EventArgs.Empty);
            });
        }

        public abstract object CreateInstance(object obj);

        public abstract void DestroyInstance(object obj);

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
            if (this.Dispatcher == null)
                throw new ObjectDisposedException($"{this.GetType()}");
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        internal static bool IsServer(ServiceHostBase serviceHost)
        {
            if (serviceHost.GetType().GetCustomAttribute(typeof(ServiceHostAttribute)) is ServiceHostAttribute attribute)
            {
                return attribute.IsServer;
            }
            return false;
        }

        #region IServiceHost

        IContainer<MethodDescriptor> IServiceHost.MethodDescriptors => this.MethodDescriptors;

        #endregion

        #region IDisposable

        void IDisposable.Dispose() => this.Dispose();

        #endregion
    }
}
