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

[ServiceHost(IsServer = false)]
public abstract class ClientServiceHostBase<TService, TCallback>
    : ServiceHostBase
    where TService : class
    where TCallback : class
{
    private TService? _service;

    protected ClientServiceHostBase()
        : base(typeof(TService), typeof(TCallback))
    {
    }

    protected TService Service => _service ?? throw new InvalidOperationException();

    protected virtual TCallback CreateCallback(IPeer peer, TService service)
    {
        if (typeof(TCallback).IsAssignableFrom(this.GetType()) == true)
            return (this as TCallback)!;
        throw new NotImplementedException();
    }

    protected virtual void DestroyCallback(IPeer peer, TCallback callback)
    {
    }

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        _service = (TService?)obj;
        return CreateCallback(peer, (TService)obj);
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
        DestroyCallback(peer, (TCallback)obj);
        _service = null;
    }
}

[ServiceHost(IsServer = false)]
public abstract class ClientServiceHostBase<TService>
    : ServiceHostBase where TService : class
{
    private TService? _service;

    protected ClientServiceHostBase()
        : base(typeof(TService), typeof(void))
    {
    }

    protected TService Service => _service ?? throw new InvalidOperationException();

    protected virtual void OnServiceCreated(IPeer peer, TService service)
    {
    }

    protected virtual void OnServiceDestroyed(IPeer peer)
    {
    }

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        _service = (TService)obj;
        OnServiceCreated(peer, (TService)obj);
        return new object();
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
        OnServiceDestroyed(peer);
        _service = null;
    }
}

[ServiceHost(IsServer = false)]
public class ClientServiceHost<TService>
    : ServiceHostBase
    where TService : class
{
    public ClientServiceHost()
        : base(serviceType: typeof(TService), callbackType: typeof(void))
    {
    }

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        return new object();
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
    }
}
