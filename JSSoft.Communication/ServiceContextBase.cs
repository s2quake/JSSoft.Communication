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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using JSSoft.Communication.Logging;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

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
        private IAdaptorHostProvider adpatorHostProvider;
        private ISerializerProvider serializerProvider;
        private ISerializer serializer;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = DefaultPort;
        private bool isServer;
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
            try
            {
                return await this.OpenInternalAsync();
            }
            catch
            {
                this.serializerProvider = null;
                this.serializer = null;
                this.adpatorHostProvider = null;
                this.adaptorHost = null;
                this.serviceByServiceHost.Clear();
                this.callbackByServiceHost.Clear();
                await this.Dispatcher.DisposeAsync();
                throw;
            }
        }

        public async Task CloseAsync(Guid token)
        {
            try
            {
                await this.CloseInternalAsync(token);
            }
            catch
            {
                throw;
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

        public string Host
        {
            get => this.host ?? DefaultHost;
            set
            {
                if (this.IsOpened == true)
                    throw new InvalidOperationException("cannot set host when service is open.");
                this.host = value;
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (this.IsOpened == true)
                    throw new InvalidOperationException("cannot set port when service is open.");
                this.port = value;
            }
        }

        public bool IsOpened { get; private set; }

        public Dispatcher Dispatcher { get; private set; }

        public event EventHandler Opened;

        public event EventHandler Closed;

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

        protected virtual void OnClosed(EventArgs e)
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

        private async Task<Guid> OpenInternalAsync()
        {
            var token = ServiceToken.NewToken();
            this.Dispatcher = new Dispatcher(this);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.serializerProvider = this.componentProvider.GetserializerProvider(this.SerializerType);
                this.serializer = this.serializerProvider.Create(this, this.componentProvider.DataSerializers);
                LogUtility.Debug($"{this.serializerProvider.Name} Serializer created.");
                this.adpatorHostProvider = this.componentProvider.GetAdaptorHostProvider(this.AdaptorHostType);
                this.adaptorHost = this.adpatorHostProvider.Create(this, token);
                LogUtility.Debug($"{this.adpatorHostProvider.Name} Adaptor created.");
                this.adaptorHost.Peers.CollectionChanged += Peers_CollectionChanged;
                this.adaptorHost.Disconnected += AdaptorHost_Disconnected;
            });
            foreach (var item in this.ServiceHosts)
            {
                this.InitializeInstance(item);
                await item.OpenAsync(token);
                LogUtility.Debug($"{item.Name} Service opened.");
            }
            await this.adaptorHost.OpenAsync(this.Host, this.Port);
            LogUtility.Debug($"{this.adpatorHostProvider.Name} Adaptor opened.");
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.IsOpened = true;
                LogUtility.Debug($"Service Context opened.");
                this.OnOpened(EventArgs.Empty);
            });
            this.token = token;
            return this.token.Guid;
        }

        private async Task CloseInternalAsync(Guid token)
        {
            if (token == Guid.Empty || this.token.Guid != token)
                throw new ArgumentException($"invalid token: {token}", nameof(token));
            await this.adaptorHost.CloseAsync();
            LogUtility.Debug($"{this.adpatorHostProvider.Name} Adaptor closed.");
            foreach (var item in this.ServiceHosts)
            {
                await item.CloseAsync(this.token);
                this.ReleaseInstance(item);
                LogUtility.Debug($"{item.Name} Service closed.");
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                this.adaptorHost.Peers.CollectionChanged -= Peers_CollectionChanged;
                this.adaptorHost = null;
                this.serializer = null;
                this.IsOpened = false;
                this.OnClosed(EventArgs.Empty);
                LogUtility.Debug($"Service Context closed.");
            });
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
            this.token = ServiceToken.Empty;
        }

        private async void AdaptorHost_Disconnected(object sender, DisconnectionReasonEventArgs e)
        {
            await this.CloseAsync(this.token.Guid);
        }

        private void Peers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (IPeer item in e.NewItems)
                        {
                            this.CreateInstance(this.adaptorHost, item);
                        }
                    }
                    break;
            }
        }

        private void InitializeInstance(IServiceHost serviceHost)
        {
            var isPerPeer = IsPerPeer(this, serviceHost);
            if (isPerPeer == true)
                return;
            var (service, callback) = this.CreateInstance(serviceHost, null);
            this.serviceByServiceHost.Add(serviceHost, service);
            this.callbackByServiceHost.Add(serviceHost, callback);
        }

        private void ReleaseInstance(IServiceHost serviceHost)
        {
            var service = null as object;
            if (this.serviceByServiceHost.ContainsKey(serviceHost) == true)
            {
                service = this.serviceByServiceHost[serviceHost];
                this.serviceByServiceHost.Remove(serviceHost);
            }
            var callback = null as object;
            if (this.callbackByServiceHost.ContainsKey(serviceHost) == true)
            {
                callback = this.callbackByServiceHost[serviceHost];
                this.callbackByServiceHost.Remove(serviceHost);
            }
            this.DestroyInstance(serviceHost, service, callback);
        }

        private (object, object) CreateInstance(IServiceHost serviceHost, IPeer peer)
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

            var impl = serviceHost.CreateInstance(instance);
            var service = this.isServer ? impl : instance;
            var callback = this.isServer ? instance : impl;
            return (service, callback);
        }

        private void DestroyInstance(IServiceHost serviceHost, object service, object callback)
        {
            var baseType = GetInstanceType(this, serviceHost);
            if (this.isServer == true)
            {
                serviceHost.DestroyInstance(service);
            }
            else
            {
                serviceHost.DestroyInstance(callback);
            }
        }

        private void CreateInstance(IAdaptorHost adaptorHost, IPeer peer)
        {
            foreach (var item in peer.ServiceHosts)
            {
                var isPerPeer = IsPerPeer(this, item);
                if (isPerPeer == true)
                {
                    var (service, callback) = this.CreateInstance(item, peer);
                    peer.AddInstance(item, service, callback);
                }
                else
                {
                    var service = this.serviceByServiceHost[item];
                    var callback = this.callbackByServiceHost[item];
                    peer.AddInstance(item, service, callback);
                }
            }
        }

        #region IServiecHost

        IContainer<IServiceHost> IServiceContext.ServiceHosts => this.ServiceHosts;

        #endregion
    }
}
