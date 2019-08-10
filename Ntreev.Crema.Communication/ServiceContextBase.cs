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
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceContextBase : IServiceContext, IDisposable
    {
        public const string DefaultHost = "localhost";
        public const int DefaultPort = 4004;
        private readonly IComponentProvider componentProvider;
        private readonly InstanceCollection serviceByServiceHost = new InstanceCollection();
        private readonly InstanceCollection callbackByServiceHost = new InstanceCollection();
        private readonly ServiceInstanceBuilder instanceBuilder;
        private IAdaptorHostProvider adpatorHostProvider;
        private ISerializer serializer;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = DefaultPort;
        private bool isServer;
        private ServiceToken token;

        internal ServiceContextBase(IComponentProvider componentProvider)
        {
            this.componentProvider = componentProvider;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.ServiceHosts = new ServiceHostCollection(this.componentProvider.Services);
            this.Dispatcher = new Dispatcher(this);
            this.isServer = IsServer(this);
        }

        public async Task<Guid> OpenAsync()
        {
            var token = ServiceToken.NewToken();
            await this.Dispatcher.InvokeAsync((Action)(() =>
            {
                this.serializer = this.componentProvider.Getserializer(this.SerializerType);
                this.adpatorHostProvider = this.componentProvider.GetAdaptorHostProvider(this.AdaptorHostType);
                this.adaptorHost = this.adpatorHostProvider.Create(this, token);
                this.adaptorHost.Peers.CollectionChanged += Peers_CollectionChanged;
                this.adaptorHost.Disconnected += AdaptorHost_Disconnected;
            }));
            foreach (var item in this.ServiceHosts)
            {
                this.InitializeInstance(item);
                await item.OpenAsync(token);
            }
            await this.adaptorHost.OpenAsync(this.Host, this.Port);
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.IsOpened = true;
                this.OnOpened(EventArgs.Empty);
            });
            this.token = token;
            return this.token.Guid;
        }

        public async Task CloseAsync(Guid token)
        {
            if (token == Guid.Empty || this.token.Guid != token)
                throw new ArgumentException($"invalid token: {token}", nameof(token));
            await this.adaptorHost.CloseAsync();
            foreach (var item in this.ServiceHosts)
            {
                await item.CloseAsync(this.token);
                this.ReleaseInstance(item);
            }
            await this.Dispatcher.InvokeAsync(() =>
            {
                this.adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                this.adaptorHost.Peers.CollectionChanged -= Peers_CollectionChanged;
                this.adaptorHost = null;
                this.serializer = null;
                this.IsOpened = false;
                this.OnClosed(EventArgs.Empty);
            });
            this.token = ServiceToken.Empty;
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

        protected virtual void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        internal void Dispose()
        {
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
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

        private Dictionary<object, object> instanceByImpl;

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
            var baseType = GetInstanceType(this, serviceHost);
            var typeName = $"{baseType.Name}Impl";
            var instanceType = this.instanceBuilder.CreateType(typeName, typeof(InstanceBase), baseType);
            var instance = TypeDescriptor.CreateInstance(null, instanceType, null, null) as InstanceBase;
            instance.ServiceHost = serviceHost;
            instance.AdaptorHost = adaptorHost;
            instance.Peer = peer;

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

        #region IDisposable

        void IDisposable.Dispose() => this.Dispose();

        #endregion
    }
}
