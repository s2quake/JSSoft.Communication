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
    private T? _service;

    protected ClientServiceHostBase()
        : base(typeof(T), typeof(U))
    {
    }

    protected T Service => _service ?? throw new InvalidOperationException();

    protected virtual U CreateCallback(IPeer peer, T service)
    {
        if (typeof(U).IsAssignableFrom(this.GetType()) == true)
            return (this as U)!;
        throw new NotImplementedException();
    }

    protected virtual void DestroyCallback(IPeer peer, U callback)
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        _service = (T?)obj;
        return CreateCallback(peer, (T)obj);
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
        DestroyCallback(peer, (U)obj);
        _service = null;
    }
}

[ServiceHost(IsServer = false)]
public abstract class ClientServiceHostBase<T> : ServiceHostBase where T : class
{
    private T? _service;

    protected ClientServiceHostBase()
        : base(typeof(T), typeof(void))
    {
    }

    protected T Service => _service ?? throw new InvalidOperationException();

    protected virtual void OnServiceCreated(IPeer peer, T service)
    {
    }

    protected virtual void OnServiceDestroyed(IPeer peer)
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        _service = (T?)obj;
        OnServiceCreated(peer, (T)obj);
        return new object();
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
        OnServiceDestroyed(peer);
        _service = null;
    }
}

[ServiceHost(IsServer = false)]
public class ClientServiceHost<TService> : ServiceHostBase where TService : class
{
    public ClientServiceHost()
        : base(serviceType: typeof(TService), callbackType: typeof(void))
    {
    }

    private protected override object CreateInstanceInternal(IPeer peer, object obj)
    {
        return new object();
    }

    private protected override void DestroyInstanceInternal(IPeer peer, object obj)
    {
    }
}
