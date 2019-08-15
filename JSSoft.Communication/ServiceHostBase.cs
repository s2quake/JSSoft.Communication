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
using System.Reflection;
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace JSSoft.Communication
{
    public abstract class ServiceHostBase : IServiceHost
    {
        private ServiceToken token;

        internal ServiceHostBase(Type serviceType, Type callbackType)
        {
            this.ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            this.CallbackType = callbackType ?? throw new ArgumentNullException(nameof(callbackType));
            this.Name = serviceType.Name;
            this.MethodDescriptors = new MethodDescriptorCollection(this);
            this.OnValidate();
        }

        public Type ServiceType { get; }

        public Type CallbackType { get; }

        public Dispatcher Dispatcher { get; private set; }

        public MethodDescriptorCollection MethodDescriptors { get; }

        public async Task OpenAsync(ServiceToken token)
        {
            this.token = token;
            this.Dispatcher = new Dispatcher(this);
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
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
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

        protected virtual void OnValidate()
        {
            if (this.ServiceType.IsInterface == false)
                throw new InvalidOperationException("service type must be interface.");
            if (this.ServiceType.IsNested == true)
                throw new InvalidOperationException("service type can not be nested type.");
            if (IsPublicType(this.ServiceType) == false && IsInternalType(this.ServiceType) == false)
                throw new InvalidOperationException($"'{this.ServiceType.Name}' must be public or internal.");

            if (this.CallbackType != typeof(void))
            {
                if (this.CallbackType.IsInterface == false)
                    throw new InvalidOperationException("callback type must be interface.");
                if (this.CallbackType.IsNested == true)
                throw new InvalidOperationException("callback type can not be nested type.");
                if (IsPublicType(this.CallbackType) == false && IsInternalType(this.CallbackType) == false)
                    throw new InvalidOperationException($"'{this.ServiceType.Name}' type must be public or internal.");
            }
        }

        private static bool IsPublicType(Type type)
        {
            return type.IsVisible == true && type.IsPublic == true && type.IsNotPublic == false;
        }

        private static bool IsInternalType(Type t)
        {
            return t.IsVisible == false && t.IsPublic == false && t.IsNotPublic == true;
        }

        private protected abstract object CreateInstanceInternal(object obj);

        private protected abstract void DestroyInstanceInternal(object obj);

        internal static bool IsServer(ServiceHostBase serviceHost)
        {
            if (serviceHost.GetType().GetCustomAttribute(typeof(ServiceHostAttribute)) is ServiceHostAttribute attribute)
            {
                return attribute.IsServer;
            }
            return false;
        }

        #region IServiceHost

        object IServiceHost.CreateInstance(object obj)
        {
            return this.CreateInstanceInternal(obj);
        }

        void IServiceHost.DestroyInstance(object obj)
        {
            this.DestroyInstanceInternal(obj);
        }

        IContainer<MethodDescriptor> IServiceHost.MethodDescriptors => this.MethodDescriptors;

        #endregion
    }
}
