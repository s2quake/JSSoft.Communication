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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication;

public abstract class ServiceContextBase : IServiceContext
{
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 4004;
    private readonly IComponentProvider _componentProvider;
    private readonly ServiceInstanceBuilder _instanceBuilder;
    private readonly InstanceContext _instanceContext;
    private readonly bool _isServer;
    private IAdaptorHostProvider _adpatorHostProvider;
    private ISerializerProvider _serializerProvider;
    private ISerializer _serializer;
    private IAdaptorHost _adaptorHost;
    private string _host;
    private int _port = DefaultPort;
    private ServiceToken _token;

    protected ServiceContextBase(IComponentProvider componentProvider, IServiceHost[] serviceHost)
    {
        this._componentProvider = componentProvider ?? ComponentProvider.Default;
        this.ServiceHosts = new ServiceHostCollection(serviceHost);
        this._isServer = IsServer(this);
        this._instanceBuilder = ServiceInstanceBuilder.Create();
        this._instanceContext = new InstanceContext(this);

        this.ValidateExceptionDescriptors();
    }

    protected ServiceContextBase(IServiceHost[] serviceHost)
        : this(null, serviceHost)
    {

    }

    public async Task<Guid> OpenAsync()
    {
        if (this.ServiceState != ServiceState.None)
            throw new InvalidOperationException();
        this.ServiceState = ServiceState.Opening;
        this.Dispatcher = await Dispatcher.CreateAsync(this);
        try
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                this._token = ServiceToken.NewToken();
                this._serializerProvider = this._componentProvider.GetserializerProvider(this.SerializerType);
                this._serializer = this._serializerProvider.Create(this, this._componentProvider.DataSerializers);
                this.Debug($"{this._serializerProvider.Name} Serializer created.");
                this._adpatorHostProvider = this._componentProvider.GetAdaptorHostProvider(this.AdaptorHostType);
                this._adaptorHost = this._adpatorHostProvider.Create(this, this._instanceContext, _token);
                this.Debug($"{this._adpatorHostProvider.Name} Adaptor created.");
                this._adaptorHost.Disconnected += AdaptorHost_Disconnected;
            });
            await this._instanceContext.InitializeInstanceAsync();
            foreach (var item in this.ServiceHosts)
            {
                await item.OpenAsync(_token);
                await this.DebugAsync($"{item.Name} Service opened.");
            }
            await this._adaptorHost.OpenAsync(this.Host, this.Port);
            await this.DebugAsync($"{this._adpatorHostProvider.Name} Adaptor opened.");
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.Debug($"Service Context opened.");
                this.ServiceState = ServiceState.Open;
                this.OnOpened(EventArgs.Empty);
            });
            return this._token.Guid;
        }
        catch
        {
            this.ServiceState = ServiceState.None;
            await this.AbortAsync();
            throw;
        }
    }

    public async Task CloseAsync(Guid token, int closeCode)
    {
        if (token == Guid.Empty || this._token.Guid != token)
            throw new ArgumentException($"invalid token: {token}", nameof(token));
        if (this.ServiceState != ServiceState.Open)
            throw new InvalidOperationException();
        if (closeCode == int.MinValue)
            throw new ArgumentException($"invalid close code: '{closeCode}'", nameof(closeCode));
        try
        {
            await this._adaptorHost.CloseAsync(closeCode);
            await this.DebugAsync($"{this._adpatorHostProvider.Name} Adaptor closed.");
            foreach (var item in this.ServiceHosts.Reverse())
            {
                await item.CloseAsync(this._token);
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Debug($"{item.Name} Service closed.");
                });
            }
            await this._instanceContext.ReleaseInstanceAsync();
            await this.Dispatcher.InvokeAsync(() =>
            {
                this._adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                this._adaptorHost = null;
                this._serializer = null;
                this.Dispatcher.Dispose();
                this.Dispatcher = null;
                this._token = ServiceToken.Empty;
                this.ServiceState = ServiceState.None;
                this.OnClosed(new CloseEventArgs(closeCode));
                this.Debug($"Service Context closed.");
            });
        }
        catch (Exception e)
        {
            await this.AbortAsync();
            throw e;
        }
    }

    public object GetService(Type serviceType)
    {
        if (serviceType == typeof(ISerializer))
            return this._serializer;
        if (serviceType == typeof(IComponentProvider))
            return this._componentProvider;
        return null;
    }

    public string AdaptorHostType { get; set; }

    public string SerializerType { get; set; }

    public ServiceHostCollection ServiceHosts { get; }

    public ServiceState ServiceState { get; private set; }

    public string Host
    {
        get => this._host ?? DefaultHost;
        set
        {
            if (this.ServiceState != ServiceState.None)
                throw new InvalidOperationException($"cannot set host. service state is '{this.ServiceState}'.");
            this._host = value;
        }
    }

    public int Port
    {
        get => this._port;
        set
        {
            if (this.ServiceState != ServiceState.None)
                throw new InvalidOperationException($"cannot set port. service state is '{this.ServiceState}'.");
            this._port = value;
        }
    }

    public Dispatcher Dispatcher { get; private set; }

    public event EventHandler Opened;

    public event EventHandler<CloseEventArgs> Closed;

    protected virtual InstanceBase CreateInstance(Type type)
    {
        if (this._instanceBuilder == null)
            throw new InvalidOperationException($"cannot create instance of {type}");
        if (type == typeof(void))
            return null;
        var typeName = $"{type.Name}Impl";
        var instanceType = this._instanceBuilder.CreateType(typeName, typeof(InstanceBase), type);
        return TypeDescriptor.CreateInstance(null, instanceType, null, null) as InstanceBase;
    }

    protected virtual void OnOpened(EventArgs e)
    {
        this.Opened?.Invoke(this, e);
    }

    protected virtual void OnClosed(CloseEventArgs e)
    {
        this.Closed?.Invoke(this, e);
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
        if (IsServer(serviceContext) == false)
            return false;
        var serviceType = serviceHost.ServiceType;
        if (serviceType.GetCustomAttribute(typeof(ServiceContractAttribute)) is ServiceContractAttribute attribute)
        {
            return attribute.PerPeer;
        }
        return false;
    }

    internal async Task<(object, object)> CreateInstanceAsync(IServiceHost serviceHost, IPeer peer)
    {
        var adaptorHost = this._adaptorHost;
        var baseType = GetInstanceType(this, serviceHost);
        var instance = this.CreateInstance(baseType);
        if (instance != null)
        {
            instance.ServiceHost = serviceHost;
            instance.AdaptorHost = adaptorHost;
            instance.Peer = peer;
        }

        var impl = await serviceHost.CreateInstanceAsync(peer, instance);
        var service = this._isServer ? impl : instance;
        var callback = this._isServer ? instance : impl;
        return (service, callback);
    }

    internal Task DestroyInstanceAsync(IServiceHost serviceHost, IPeer peer, object service, object callback)
    {
        if (this._isServer == true)
        {
            return serviceHost.DestroyInstanceAsync(peer, service);
        }
        else
        {
            return serviceHost.DestroyInstanceAsync(peer, callback);
        }
    }

    private async Task AbortAsync()
    {
        foreach (var item in this.ServiceHosts)
        {
            await item.CloseAsync(this._token);
        }
        await Task.Run(() =>
        {
            this._token = null;
            this._serializerProvider = null;
            this._serializer = null;
            this._adpatorHostProvider = null;
            this._adaptorHost = null;
            this.ServiceState = ServiceState.None;
            this.Dispatcher?.Dispose();
            this.Dispatcher = null;
        });
    }

    private void Debug(string message)
    {
        LogUtility.Debug(message);
    }

    private Task DebugAsync(string message)
    {
        return this.Dispatcher.InvokeAsync(() => this.Debug(message));
    }

    private void AdaptorHost_Disconnected(object sender, CloseEventArgs e)
    {
        Task.Run(() => this.CloseAsync(this._token.Guid, e.CloseCode));
    }

    private void ValidateExceptionDescriptors()
    {
        var descriptorByID = new Dictionary<Guid, IExceptionDescriptor>(this._componentProvider.ExceptionDescriptors.Length);
        foreach (var item in this._componentProvider.ExceptionDescriptors)
        {
            if (descriptorByID.ContainsKey(item.ID) == true)
            {
                var value = descriptorByID[item.ID];
                var message = $"'{item.ID}: {item.GetType()}' is already used in '{value.GetType()}'.";
                throw new InvalidOperationException(message);
            }
            descriptorByID.Add(item.ID, item);
        }
    }

    #region IServiecHost

    IContainer<IServiceHost> IServiceContext.ServiceHosts => this.ServiceHosts;

    #endregion
}
