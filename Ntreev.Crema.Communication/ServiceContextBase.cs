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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceContextBase : IServiceContext, IDisposable
    {
        private const string defaultHost = "localhost";
        private static readonly int defaultPort = 4004;
        private readonly IComponentProvider componentProvider;
        private readonly Dictionary<Type, IExceptionSerializer> exceptionSerializerByType = new Dictionary<Type, IExceptionSerializer>();
        private readonly ServiceInstanceBuilder instanceBuilder;
        private readonly Dispatcher dispatcher;
        private IAdaptorHostProvider adpatorHostProvider;
        private IDataSerializer dataSerializer;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = defaultPort;
        private bool isOpened;
        private ServiceToken token;

        internal ServiceContextBase(IComponentProvider componentProvider)
        {
            this.componentProvider = componentProvider;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceHostCollection(componentProvider.Services);
            this.dispatcher = new Dispatcher(this);
        }

        public async Task<Guid> OpenAsync()
        {
            var token = ServiceToken.NewToken();
            await this.dispatcher.InvokeAsync((Action)(() =>
            {
                this.dataSerializer = this.componentProvider.GetDataSerializer(this.DataSerializerType);
                this.adpatorHostProvider = this.componentProvider.GetAdaptorHostProvider(this.AdaptorHostType);
                this.adaptorHost = this.adpatorHostProvider.Create(this, token);
                this.adaptorHost.Peers.CollectionChanged += Peers_CollectionChanged;
                this.adaptorHost.Disconnected += AdaptorHost_Disconnected;
            }));
            await this.adaptorHost.OpenAsync(this.Host, this.Port);
            foreach (var item in this.Services)
            {
                await item.OpenAsync(token);
            }
            await this.dispatcher.InvokeAsync(() =>
            {
                this.isOpened = true;
                this.OnOpened(EventArgs.Empty);
            });
            this.token = token;
            return this.token.Guid;
        }

        private void Peers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (IPeer item in e.NewItems)
                        {

                        }
                    }
                    break;
            }
        }

        private void CreateInstance(IAdaptorHost adaptorHost, IPeer peer)
        {
            foreach (var item in peer.Services)
            {
                var remoteType = item.InstanceType;
                var typeName = $"{remoteType.Name}Impl";
                var typeNamespace = remoteType.Namespace;
                var implType = this.instanceBuilder.CreateType(typeName, typeNamespace, typeof(InstanceBase), remoteType);
                var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as InstanceBase;
                instance.Service = item;
                instance.AdaptorHost = adaptorHost;
                instance.Peer = peer;

                var impl = item.CreateInstance(instance);
                peer.AddInstance(item, instance, impl);
            }
        }

        public async Task CloseAsync(Guid token)
        {
            if (token == Guid.Empty || this.token.Guid != token)
                throw new ArgumentException($"invalid token: {token}", nameof(token));
            foreach (var item in this.Services)
            {
                await item.CloseAsync(this.token);
                this.adaptorHost.Disconnected -= AdaptorHost_Disconnected;
                this.adaptorHost.Peers.CollectionChanged -= Peers_CollectionChanged;
                this.adaptorHost = null;
                this.dataSerializer = null;
            }
            await this.adaptorHost.CloseAsync();
            await this.dispatcher.InvokeAsync(() =>
            {
                this.isOpened = false;
                this.OnClosed(EventArgs.Empty);
            });
            this.token = ServiceToken.Empty;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IDataSerializer) && this.IsOpened == true)
                return this.dataSerializer;
            if (serviceType == typeof(IComponentProvider))
                return this.componentProvider;
            return null;
        }

        public string AdaptorHostType { get; set; }

        public string DataSerializerType { get; set; }

        internal void Dispose()
        {
            this.dispatcher.Dispose();
        }

        public ServiceHostCollection Services { get; }

        public string Host
        {
            get => this.host ?? defaultHost;
            set
            {
                if (this.isOpened == true)
                    throw new InvalidOperationException("cannot set host when service is open.");
                this.host = value;
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (this.isOpened == true)
                    throw new InvalidOperationException("cannot set port when service is open.");
                this.port = value;
            }
        }

        public bool IsOpened => this.isOpened;

        public Dispatcher Dispatcher => this.dispatcher;

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

        private async void AdaptorHost_Disconnected(object sender, DisconnectionReasonEventArgs e)
        {
            await this.CloseAsync(this.token.Guid);
        }

        #region IServiecHost

        IContainer<IServiceHost> IServiceContext.Services => this.Services;

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        #endregion
    }
}
