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

namespace JSSoft.Communication;

[ServiceHost(IsServer = true)]
public abstract class ServerServiceHostBase<T, U> : ServiceHostBase where T : class where U : class
{
    private U? _callback;

    protected ServerServiceHostBase()
        : base(typeof(T), typeof(U))
    {
    }

    public U Callback => _callback ?? throw new InvalidOperationException();

    protected virtual T CreateService(IPeer peer)
    {
        if (typeof(T).IsAssignableFrom(this.GetType()) == true)
            return (this as T)!;
        return Activator.CreateInstance<T>();
    }

    protected virtual void DestroyService(IPeer peer, T service)
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        _callback = (U)obj;
        return CreateService(peer);
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
        DestroyService(peer, (T)obj);
        _callback = null;
    }
}

[ServiceHost(IsServer = true)]
public abstract class ServerServiceHostBase<T> : ServiceHostBase where T : class
{
    protected ServerServiceHostBase()
        : base(typeof(T), typeof(void))
    {
    }

    protected virtual T CreateService(IPeer peer)
    {
        if (typeof(T).IsAssignableFrom(this.GetType()) == true)
            return (this as T)!;
        throw new NotImplementedException();
    }

    protected virtual void DestroyService(IPeer peer, T service)
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        return CreateService(peer);
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
        DestroyService(peer, (T)obj);
    }
}

[ServiceHost(IsServer = true)]
public class ServerServiceHost<TService>(TService service)
    : ServiceHostBase(serviceType: typeof(TService), callbackType: typeof(void))
    where TService : class
{
    private readonly TService _service = service;

    protected virtual TService CreateService(IPeer peer)
    {
        return _service;
    }

    protected virtual void DestroyService(IPeer peer, TService service)
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        return CreateService(peer);
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
        DestroyService(peer, (TService)obj);
    }
}
