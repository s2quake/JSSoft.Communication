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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication
{
    public abstract class ServiceContextBase : IServiceContext
    {
        public const string DefaultHost = "localhost";
        public const int DefaultPort = 4004;
        private readonly IComponentProvider componentProvider;
        private readonly InstanceCollection serviceByServiceHost = new InstanceCollection();
        private readonly InstanceCollection callbackByServiceHost = new InstanceCollection();
        private readonly ServiceInstanceBuilder instanceBuilder;
        private readonly bool isServer;
        private IAdaptorHostProvider adpatorHostProvider;
        private ISerializerProvider serializerProvider;
        private ISerializer serializer;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = DefaultPort;
        private ServiceToken token;

        protected ServiceContextBase(IComponentProvider componentProvider, IServiceHost[] serviceHost)
        {
            this.componentProvider = componentProvider ?? ComponentProvider.Default;
            this.ServiceHosts = new ServiceHostCollection(serviceHost);
            this.isServer = IsServer(this);
            this.instanceBuilder = ServiceInstanceBuilder.Create();
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
                    this.token = ServiceToken.NewToken();
                    this.serializerProvider = this.componentProvider.GetserializerProvider(this.SerializerType);
                    this.serializer = this.serializerProvider.Create(this, this.componentProvider.DataSerializers);
                    this.Debug($"{this.serializerProvider.Name} Serializer created.");
                    this.adpatorHostProvider = this.componentProvider.GetAdaptorHostProvider(this.AdaptorHostType);
                    this.adaptorHost = this.adpatorHostProvider.Create(this, token);
                    this.Debug($"{this.adpatorHostProvider.Name} Adaptor created.");
                    this.adaptorHost.Disconnected += AdaptorHost_Disconnected;
                });
                foreach (var item in this.ServiceHosts)
                {
                    await this.InitializeInstanceAsync(item);
                    await item.OpenAsync(token);
                    await this.DebugAsync($"{item.Name} Service opened.");
                }
                await this.adaptorHost.OpenAsync(this.Host, this.Port);
                await this.DebugAsync($"{this.adpatorHostProvider.Name} Adaptor opened.");
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.Debug($"Service Context opened.");
                    this.ServiceState = ServiceState.Open;
                    this.OnOpened(EventArgs.Empty);
                });
                return this.token.Guid;
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
            if (token == Guid.Empty || this.token.Guid != token)
                throw new ArgumentException($"invalid token: {token}", nameof(token));
            if (this.ServiceState != ServiceState.Open)
                throw new InvalidOperationException();
            if (closeCode == int.MinValue)
                throw new ArgumentException($"invalid close code: '{closeCode}'", nameof(closeCode));
            try
            {
                await this.adaptorHost.CloseAsync(closeCode);
                await this.DebugAsync($"{this.adpatorHostProvider.Name} Adaptor closed.");
                foreach (var item in this.ServiceHosts.Reverse())
                {
                    await item.CloseAsync(this.token);
                    await this.ReleaseInstanceAsync(item);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.Debug($"{item.Name} Service closed.");
                    });
                }
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                    this.adaptorHost = null;
                    this.serializer = null;
                    this.Dispatcher.Dispose();
                    this.Dispatcher = null;
                    this.token = ServiceToken.Empty;
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
                return this.serializer;
            if (serviceType == typeof(IComponentProvider))
                return this.componentProvider;
            return null;
        }

        public string AdaptorHostType { get; set; }

        public string SerializerType { get; set; }

        public ServiceHostCollection ServiceHosts { get; }

        public ServiceState ServiceState { get; private set; }

        public string Host
        {
            get => this.host ?? DefaultHost;
            set
            {
                if (this.ServiceState != ServiceState.None)
                    throw new InvalidOperationException($"cannot set host. service state is '{this.ServiceState}'.");
                this.host = value;
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (this.ServiceState != ServiceState.None)
                    throw new InvalidOperationException($"cannot set port. service state is '{this.ServiceState}'.");
                this.port = value;
            }
        }

        public Dispatcher Dispatcher { get; private set; }

        public event EventHandler Opened;

        public event EventHandler<CloseEventArgs> Closed;

        protected virtual InstanceBase CreateInstance(Type type)
        {
            if (this.instanceBuilder == null)
                throw new InvalidOperationException($"cannot create instance of {type}");
            if (type == typeof(void))
                return null;
            var typeName = $"{type.Name}Impl";
            var instanceType = this.instanceBuilder.CreateType(typeName, typeof(InstanceBase), type);
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

        private async Task AbortAsync()
        {
            foreach (var item in this.ServiceHosts)
            {
                await item.CloseAsync(this.token);
            }
            await Task.Run(() =>
            {
                this.token = null;
                this.serializerProvider = null;
                this.serializer = null;
                this.adpatorHostProvider = null;
                this.adaptorHost = null;
                this.serviceByServiceHost.Clear();
                this.callbackByServiceHost.Clear();
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
            Task.Run(() => this.CloseAsync(this.token.Guid, e.CloseCode));
        }

        private async Task InitializeInstanceAsync(IServiceHost serviceHost)
        {
            var isPerPeer = IsPerPeer(this, serviceHost);
            if (isPerPeer == true)
                return;
            var (service, callback) = await this.CreateInstanceAsync(serviceHost, null);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.serviceByServiceHost.Add(serviceHost, service);
                this.callbackByServiceHost.Add(serviceHost, callback);
            });
        }

        private async Task ReleaseInstanceAsync(IServiceHost serviceHost)
        {
            var service = null as object;
            var callback = null as object;
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.serviceByServiceHost.ContainsKey(serviceHost) == true)
                {
                    service = this.serviceByServiceHost[serviceHost];
                    this.serviceByServiceHost.Remove(serviceHost);
                }
                if (this.callbackByServiceHost.ContainsKey(serviceHost) == true)
                {
                    callback = this.callbackByServiceHost[serviceHost];
                    this.callbackByServiceHost.Remove(serviceHost);
                }
            });
            await this.DestroyInstanceAsync(serviceHost, service, callback);
        }

        private async Task<(object, object)> CreateInstanceAsync(IServiceHost serviceHost, IPeer peer)
        {
            var adaptorHost = this.adaptorHost;
            var baseType = GetInstanceType(this, serviceHost);
            var instance = this.CreateInstance(baseType);
            if (instance != null)
            {
                instance.ServiceHost = serviceHost;
                instance.AdaptorHost = adaptorHost;
                instance.Peer = peer;
            }

            var impl = await serviceHost.CreateInstanceAsync(instance);
            var service = this.isServer ? impl : instance;
            var callback = this.isServer ? instance : impl;
            return (service, callback);
        }

        private Task DestroyInstanceAsync(IServiceHost serviceHost, object service, object callback)
        {
            if (this.isServer == true)
            {
                return serviceHost.DestroyInstanceAsync(service);
            }
            else
            {
                return serviceHost.DestroyInstanceAsync(callback);
            }
        }

        internal async Task CreateInstanceAsync(IPeer peer)
        {
            foreach (var item in peer.ServiceHosts)
            {
                var isPerPeer = IsPerPeer(this, item);
                if (isPerPeer == true)
                {
                    var (service, callback) = await this.CreateInstanceAsync(item, peer);
                    peer.AddInstance(item, service, callback);
                }
                else
                {
                    var (service, callback) = await this.Dispatcher.InvokeAsync(() =>
                    {
                        return (this.serviceByServiceHost[item], this.callbackByServiceHost[item]);
                    });
                    peer.AddInstance(item, service, callback);
                }
            }
        }

        internal async Task DestroyInstanceAsync(IPeer peer)
        {
            foreach (var item in peer.ServiceHosts.Reverse())
            {
                var isPerPeer = IsPerPeer(this, item);
                if (isPerPeer == true)
                {
                    var (service, callback) = peer.RemoveInstance(item);
                    await this.DestroyInstanceAsync(item, service, callback);
                }
                else
                {
                    peer.RemoveInstance(item);
                }
            }
        }

        #region IServiecHost

        IContainer<IServiceHost> IServiceContext.ServiceHosts => this.ServiceHosts;

        #endregion
    }
}
