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

using JSSoft.Communication.Logging;
using JSSoft.Library.ObjectModel;
using JSSoft.Library.Threading;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication;

public abstract class ServiceContextBase : IServiceContext
{
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 4004;
    private readonly ServiceInstanceBuilder? _instanceBuilder;
    private readonly InstanceContext _instanceContext;
    private readonly bool _isServer;
    public abstract IAdaptorHostProvider AdaptorHostProvider { get; }
    public abstract ISerializerProvider SerializerProvider { get; }
    private ISerializer? _serializer;
    private IAdaptorHost? _adaptorHost;
    private string _host = DefaultHost;
    private int _port = DefaultPort;
    private ServiceToken? _token;
    private Dispatcher? _dispatcher;

    protected ServiceContextBase(IServiceHost[] serviceHost)
    {
        ServiceHosts = new ServiceHostCollection(serviceHost);
        _isServer = IsServer(this);
        _instanceBuilder = ServiceInstanceBuilder.Create();
        _instanceContext = new InstanceContext(this);
    }

    public async Task<Guid> OpenAsync()
    {
        if (ServiceState != ServiceState.None)
            throw new InvalidOperationException();
        ServiceState = ServiceState.Opening;
        _dispatcher = await Dispatcher.CreateAsync(this);
        try
        {
            _token = ServiceToken.NewToken();
            _serializer = SerializerProvider.Create(this);
            Debug($"{SerializerProvider.Name} Serializer created.");
            _adaptorHost = AdaptorHostProvider.Create(this, _instanceContext, _token);
            Debug($"{AdaptorHostProvider.Name} Adaptor created.");
            _adaptorHost.Disconnected += AdaptorHost_Disconnected;
            await _instanceContext.InitializeInstanceAsync();
            foreach (var item in ServiceHosts)
            {
                await item.OpenAsync(_token);
                Debug($"{item.Name} Service opened.");
            }
            await _adaptorHost.OpenAsync(Host, Port);
            await DebugAsync($"{AdaptorHostProvider.Name} Adaptor opened.");
            await _dispatcher.InvokeAsync(() =>
            {
                Debug($"Service Context opened.");
                ServiceState = ServiceState.Open;
                OnOpened(EventArgs.Empty);
            });
            return _token.Guid;
        }
        catch
        {
            ServiceState = ServiceState.None;
            await AbortAsync();
            throw;
        }
    }

    public async Task CloseAsync(Guid token, int closeCode)
    {
        if (ServiceState != ServiceState.Open)
            throw new InvalidOperationException();
        if (token == Guid.Empty || _token!.Guid != token)
            throw new ArgumentException($"invalid token: {token}", nameof(token));
        if (closeCode == int.MinValue)
            throw new ArgumentException($"invalid close code: '{closeCode}'", nameof(closeCode));
        try
        {
            await _adaptorHost!.CloseAsync(closeCode);
            await DebugAsync($"{AdaptorHostProvider!.Name} Adaptor closed.");
            foreach (var item in ServiceHosts.Reverse())
            {
                await item.CloseAsync(_token);
                await Dispatcher.InvokeAsync(() =>
                {
                    Debug($"{item.Name} Service closed.");
                });
            }
            await _instanceContext.ReleaseInstanceAsync();
            await Dispatcher.InvokeAsync(() =>
            {
                _adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                _adaptorHost = null;
                _serializer = null;
                Dispatcher.Dispose();
                _dispatcher = null;
                _token = ServiceToken.Empty;
                ServiceState = ServiceState.None;
                OnClosed(new CloseEventArgs(closeCode));
                Debug($"Service Context closed.");
            });
        }
        catch
        {
            await AbortAsync();
            throw;
        }
    }

    public virtual object? GetService(Type serviceType)
    {
        if (serviceType == typeof(ISerializer))
            return _serializer;
        if (_instanceContext.GetService(serviceType) is { } instanceService)
            return instanceService;
        return null;
    }

    public string AdaptorHostType { get; set; } = Communication.AdaptorHostProvider.DefaultName;

    public string SerializerType { get; set; } = JsonSerializerProvider.DefaultName;

    public ServiceHostCollection ServiceHosts { get; }

    public ServiceState ServiceState { get; private set; }

    public string Host
    {
        get => _host;
        set
        {
            if (ServiceState != ServiceState.None)
                throw new InvalidOperationException($"cannot set host. service state is '{ServiceState}'.");
            _host = value;
        }
    }

    public int Port
    {
        get => _port;
        set
        {
            if (ServiceState != ServiceState.None)
                throw new InvalidOperationException($"cannot set port. service state is '{ServiceState}'.");
            _port = value;
        }
    }

    public Dispatcher Dispatcher
    {
        get
        {
            if (_dispatcher == null)
                throw new InvalidOperationException();
            return _dispatcher;
        }
    }

    public event EventHandler? Opened;

    public event EventHandler<CloseEventArgs>? Closed;

    protected virtual InstanceBase CreateInstance(Type type)
    {
        if (_instanceBuilder == null)
            throw new InvalidOperationException($"cannot create instance of {type}");
        if (type == typeof(void))
            return InstanceBase.Empty;
        var typeName = $"{type.Name}Impl";
        var instanceType = _instanceBuilder.CreateType(typeName, typeof(InstanceBase), type);
        return (InstanceBase)Activator.CreateInstance(instanceType)!;
    }

    protected virtual void OnOpened(EventArgs e)
    {
        Opened?.Invoke(this, e);
    }

    protected virtual void OnClosed(CloseEventArgs e)
    {
        Closed?.Invoke(this, e);
    }

    internal static bool IsServer(ServiceContextBase serviceContext)
    {
        if (serviceContext.GetType().GetCustomAttribute(typeof(ServiceContextAttribute)) is ServiceContextAttribute attribute)
        {
            return attribute.IsServer;
        }
        return false;
    }

    internal static Type GetInstanceType(ServiceContextBase serviceContext, IServiceHost serviceHost)
    {
        var isServer = IsServer(serviceContext);
        if (isServer == true)
        {
            return serviceHost.CallbackType;
        }
        return serviceHost.ServiceType;
    }

    internal static bool IsPerPeer(ServiceContextBase serviceContext, IServiceHost serviceHost)
    {
        if (IsServer(serviceContext) != true)
            return false;
        var serviceType = serviceHost.ServiceType;
        if (serviceType.GetCustomAttribute(typeof(ServiceContractAttribute)) is ServiceContractAttribute attribute)
        {
            return attribute.PerPeer;
        }
        return false;
    }

    internal (object, object) CreateInstance(IServiceHost serviceHost, IPeer peer)
    {
        var adaptorHost = _adaptorHost!;
        var baseType = GetInstanceType(this, serviceHost);
        var instance = CreateInstance(baseType);
        {
            instance.ServiceHost = serviceHost;
            instance.AdaptorHost = adaptorHost;
            instance.Peer = peer;
        }

        var impl = serviceHost.CreateInstance(peer, instance);
        var service = _isServer ? impl : instance;
        var callback = _isServer ? instance : impl;
        return (service, callback);
    }

    internal void DestroyInstance(IServiceHost serviceHost, IPeer peer, object service, object callback)
    {
        if (_isServer == true)
        {
            serviceHost.DestroyInstance(peer, service);
        }
        else
        {
            serviceHost.DestroyInstance(peer, callback);
        }
    }

    private async Task AbortAsync()
    {
        foreach (var item in ServiceHosts)
        {
            await item.CloseAsync(_token!);
        }
        await Task.Run(() =>
        {
            _token = null;
            _serializer = null;
            _adaptorHost = null;
            ServiceState = ServiceState.None;
            Dispatcher?.Dispose();
            _dispatcher = null;
        });
    }

    private void Debug(string message)
    {
        LogUtility.Debug(message);
    }

    private Task DebugAsync(string message)
    {
        return Dispatcher.InvokeAsync(() => Debug(message));
    }

    private void AdaptorHost_Disconnected(object? sender, CloseEventArgs e)
    {
        Task.Run(() => CloseAsync(_token!.Guid, e.CloseCode));
    }

    #region IServiceHost

    IContainer<IServiceHost> IServiceContext.ServiceHosts => ServiceHosts;

    #endregion
}
