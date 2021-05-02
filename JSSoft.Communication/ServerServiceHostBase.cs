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

using System.ComponentModel;
using System.Threading.Tasks;

namespace JSSoft.Communication
{
    [ServiceHost(IsServer = true)]
    public abstract class ServerServiceHostBase<T, U> : ServiceHostBase where T : class where U : class
    {
        protected ServerServiceHostBase()
            : base(typeof(T), typeof(U))
        {

        }

        protected virtual Task<T> CreateServiceAsync(U callback)
        {
            return Task.Run(() => TypeDescriptor.CreateInstance(null, this.ServiceType, null, null) as T);
        }

        protected virtual Task DestroyServiceAsync(T service)
        {
            return Task.CompletedTask;
        }

        private protected override async Task<object> CreateInstanceInternalAsync(object obj)
        {
            return await this.CreateServiceAsync(obj as U);
        }

        private protected override Task DestroyInstanceInternalAsync(object obj)
        {
            return this.DestroyServiceAsync(obj as T);
        }
    }

    [ServiceHost(IsServer = true)]
    public abstract class ServerServiceHostBase<T> : ServiceHostBase where T : class
    {
        protected ServerServiceHostBase()
            : base(typeof(T), typeof(void))
        {

        }

        protected virtual Task<T> CreateServiceAsync()
        {
            return Task.Run(() => TypeDescriptor.CreateInstance(null, this.ServiceType, null, null) as T);
        }

        protected virtual Task DestroyServiceAsync(T service)
        {
            return Task.CompletedTask;
        }

        private protected override async Task<object> CreateInstanceInternalAsync(object obj)
        {
            return await this.CreateServiceAsync();
        }

        private protected override async Task DestroyInstanceInternalAsync(object obj)
        {
            await this.DestroyServiceAsync(obj as T);
        }
    }
}
