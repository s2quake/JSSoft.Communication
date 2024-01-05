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
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Communication;

public abstract class ServiceHostBase(Type serviceType, Type callbackType) : IServiceHost
{
    private ServiceToken? _serviceToken;

    public Type ServiceType { get; } = ValidateServiceType(serviceType);

    public Type CallbackType { get; } = ValidateCallbackType(callbackType);

    public string Name { get; } = serviceType.Name;

    public ServiceState ServiceState { get; private set; }

    protected virtual Task OnOpenAsync() => Task.CompletedTask;

    protected virtual Task OnCloseAsync() => Task.CompletedTask;

    protected virtual Task OnAbortAsync() => Task.CompletedTask;

    private static Type ValidateServiceType(Type ServiceType)
    {
        if (ServiceType.IsInterface != true)
            throw new InvalidOperationException("service type must be interface.");

        if (IsNestedPublicType(ServiceType) != true && IsPublicType(ServiceType) != true && IsInternalType(ServiceType) != true)
            throw new InvalidOperationException($"'{ServiceType.Name}' must be public or internal.");
        return ServiceType;
    }

    private static Type ValidateCallbackType(Type CallbackType)
    {
        if (CallbackType != typeof(void))
        {
            if (CallbackType.IsInterface != true)
                throw new InvalidOperationException("callback type must be interface.");
            if (IsNestedPublicType(CallbackType) != true && IsPublicType(CallbackType) != true && IsInternalType(CallbackType) != true)
                throw new InvalidOperationException($"'{CallbackType.Name}' must be public or internal.");
        }
        return CallbackType;
    }

    private static bool IsNestedPublicType(Type type)
    {
        return type.IsNested == true && type.IsNestedPublic == true;
    }

    private static bool IsPublicType(Type type)
    {
        return type.IsVisible == true && type.IsPublic == true && type.IsNotPublic != true;
    }

    private static bool IsInternalType(Type t)
    {
        return t.IsVisible != true && t.IsPublic != true && t.IsNotPublic == true;
    }

    private protected abstract object CreateInstance(IPeer peer, object obj);

    private protected abstract void DestroyInstance(IPeer peer, object obj);

    internal static bool IsServer(IServiceHost serviceHost)
    {
        if (serviceHost.GetType().GetCustomAttribute(typeof(ServiceHostAttribute)) is ServiceHostAttribute attribute)
        {
            return attribute.IsServer;
        }
        return false;
    }

    #region IServiceHost


    async Task IServiceHost.OpenAsync(ServiceToken serviceToken, CancellationToken cancellationToken)
    {
        if (ServiceState != ServiceState.None)
            throw new InvalidOperationException();

        try
        {
            ServiceState = ServiceState.Opening;
            await OnOpenAsync();
            _serviceToken = serviceToken;
            ServiceState = ServiceState.Open;
        }
        catch
        {
            ServiceState = ServiceState.Faulted;
        }
    }

    async Task IServiceHost.CloseAsync(ServiceToken serviceToken, CancellationToken cancellationToken)
    {
        if (ServiceState != ServiceState.Open)
            throw new InvalidOperationException();
        if (serviceToken != _serviceToken)
            throw new ArgumentException("Invalid Token", nameof(serviceToken));

        try
        {
            ServiceState = ServiceState.Closing;
            await OnCloseAsync();
            _serviceToken = null;
            ServiceState = ServiceState.Closed;
        }
        catch
        {
            ServiceState = ServiceState.Faulted;
        }
    }

    async Task IServiceHost.AbortAsync(ServiceToken serviceToken)
    {
        if (ServiceState != ServiceState.Faulted)
            throw new InvalidOperationException();
        if (serviceToken != _serviceToken)
            throw new ArgumentException("Invalid Token", nameof(serviceToken));

        ServiceState = ServiceState.Closing;
        await OnAbortAsync();
        _serviceToken = null;
        ServiceState = ServiceState.None;
    }

    object IServiceHost.CreateInstance(ServiceToken serviceToken, IPeer peer, object obj)
    {
        if (ServiceState != ServiceState.Open)
            throw new InvalidOperationException();
        if (serviceToken != _serviceToken)
            throw new ArgumentException("Invalid Token", nameof(serviceToken));

        return CreateInstance(peer, obj);
    }

    void IServiceHost.DestroyInstance(ServiceToken serviceToken, IPeer peer, object obj)
    {
        if (ServiceState != ServiceState.Open)
            throw new InvalidOperationException();
        if (serviceToken != _serviceToken)
            throw new ArgumentException("Invalid Token", nameof(serviceToken));

        DestroyInstance(peer, obj);
    }

    #endregion
}
