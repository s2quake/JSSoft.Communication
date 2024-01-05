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
public abstract class ServerServiceHostBase<TService, TCallback>
    : ServiceHostBase
    where TService : class
    where TCallback : class
{
    private TCallback? _callback;

    protected ServerServiceHostBase()
        : base(typeof(TService), typeof(TCallback))
    {
    }

    public TCallback Callback => _callback ?? throw new InvalidOperationException();

    protected virtual TService CreateService(IPeer peer)
    {
        if (typeof(TService).IsAssignableFrom(this.GetType()) == true)
            return (this as TService)!;
        return Activator.CreateInstance<TService>();
    }

    protected virtual void DestroyService(IPeer peer, TService service)
    {
    }

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        _callback = (TCallback)obj;
        return CreateService(peer);
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
        DestroyService(peer, (TService)obj);
        _callback = null;
    }
}

[ServiceHost(IsServer = true)]
public abstract class ServerServiceHostBase<TService>
    : ServiceHostBase
    where TService : class
{
    protected ServerServiceHostBase()
        : base(typeof(TService), typeof(void))
    {
    }

    protected virtual TService CreateService(IPeer peer)
    {
        if (typeof(TService).IsAssignableFrom(this.GetType()) == true)
            return (this as TService)!;
        throw new NotImplementedException();
    }

    protected virtual void DestroyService(IPeer peer, TService service)
    {
    }

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        return CreateService(peer);
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
        DestroyService(peer, (TService)obj);
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

    private protected override object CreateInstance(IPeer peer, object obj)
    {
        return CreateService(peer);
    }

    private protected override void DestroyInstance(IPeer peer, object obj)
    {
        DestroyService(peer, (TService)obj);
    }
}
