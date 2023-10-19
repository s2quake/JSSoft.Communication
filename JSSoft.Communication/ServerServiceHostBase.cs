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

namespace JSSoft.Communication;

[ServiceHost(IsServer = true)]
public abstract class ServerServiceHostBase<T, U> : ServiceHostBase where T : class where U : class
{
    protected ServerServiceHostBase()
        : base(typeof(T), typeof(U))
    {
    }

    protected virtual Task<T> CreateServiceAsync(IPeer peer, U callback)
    {
        return Task.Run(Activator.CreateInstance<T>);
    }

    protected virtual Task DestroyServiceAsync(IPeer peer, T service)
    {
        return Task.Delay(1);
    }

    private protected override async Task<object> CreateInstanceInternalAsync(IPeer peer, object obj)
    {
        return await CreateServiceAsync(peer, (U)obj);
    }

    private protected override Task DestroyInstanceInternalAsync(IPeer peer, object obj)
    {
        return DestroyServiceAsync(peer, (T)obj);
    }
}

[ServiceHost(IsServer = true)]
public abstract class ServerServiceHostBase<T> : ServiceHostBase where T : class
{
    protected ServerServiceHostBase()
        : base(typeof(T), typeof(void))
    {
    }

    protected virtual Task<T> CreateServiceAsync(IPeer peer)
    {
        return Task.Run(Activator.CreateInstance<T>);
    }

    protected virtual Task DestroyServiceAsync(IPeer peer, T service)
    {
        return Task.Delay(1);
    }

    private protected override async Task<object> CreateInstanceInternalAsync(IPeer peer, object obj)
    {
        return await CreateServiceAsync(peer);
    }

    private protected override async Task DestroyInstanceInternalAsync(IPeer peer, object obj)
    {
        await DestroyServiceAsync(peer, (T)obj);
    }
}
