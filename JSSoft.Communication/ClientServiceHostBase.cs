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

[ServiceHost(IsServer = false)]
public abstract class ClientServiceHostBase<T, U> : ServiceHostBase where T : class where U : class
{
    protected ClientServiceHostBase()
        : base(typeof(T), typeof(U))
    {

    }

    protected virtual Task<U> CreateCallbackAsync(IPeer peer, T service)
    {
        return Task.Run(() => Activator.CreateInstance<U>()!);
    }

    protected virtual Task DestroyCallbackAsync(IPeer peer, U callback)
    {
        return Task.Delay(1);
    }

    private protected override async Task<object> CreateInstanceInternalAsync(IPeer peer, object obj)
    {
        return await CreateCallbackAsync(peer, (T)obj);
    }

    private protected override Task DestroyInstanceInternalAsync(IPeer peer, object obj)
    {
        return DestroyCallbackAsync(peer, (U)obj);
    }
}

[ServiceHost(IsServer = false)]
public abstract class ClientServiceHostBase<T> : ServiceHostBase where T : class
{
    protected ClientServiceHostBase()
        : base(typeof(T), typeof(void))
    {

    }

    protected virtual void OnServiceCreated(IPeer peer, T service)
    {

    }

    protected virtual void OnServiceDestroyed(IPeer peer)
    {

    }

    private protected override Task<object> CreateInstanceInternalAsync(IPeer peer, object obj)
    {
        OnServiceCreated(peer, (T)obj);
        return Task.Run(() => new object());
    }

    private protected override Task DestroyInstanceInternalAsync(IPeer peer, object obj)
    {
        OnServiceDestroyed(peer);
        return Task.Delay(1);
    }
}
