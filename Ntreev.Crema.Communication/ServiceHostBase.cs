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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Communication
{
    public abstract class ServiceHostBase : IServiceHost, IDisposable
    {
        private const string defaultHost = "localhost";
        private static readonly int defaultPort = 4004;
        private readonly IAdaptorHostProvider adpatorHostProvider;
        private readonly ServiceInstanceBuilder instanceBuilder;
        private readonly Dispatcher dispatcher;
        private Dictionary<IService, object> instanceByService;
        private IAdaptorHost adaptorHost;
        private string host;
        private int port = defaultPort;
        private bool isOpened;

        internal ServiceHostBase(IAdaptorHostProvider adpatorHostProvider, IEnumerable<IService> services)
        {
            this.adpatorHostProvider = adpatorHostProvider;
            this.instanceBuilder = new ServiceInstanceBuilder();
            this.Services = new ServiceCollection(this, services);
            this.dispatcher = new Dispatcher(this);
        }

        public async Task OpenAsync()
        {
            await this.dispatcher.InvokeAsync(() =>
            {
                this.adaptorHost = this.adpatorHostProvider.Create(this, ServiceToken.Empty);
                this.adaptorHost.Disconnected += AdaptorHost_Disconnected;
                this.instanceByService = new Dictionary<IService, object>(this.Services.Count);
            });
            await this.adaptorHost.OpenAsync(this.Host, this.Port);
            var instanceByService = await this.dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this.Services)
                {
                    var instance = this.adaptorHost.Create(item);
                    this.instanceByService.Add(item, instance);
                }
                return this.instanceByService.ToArray();
            });
            foreach (var item in instanceByService)
            {
                var service = item.Key;
                var instance = item.Value;
                await service.OpenAsync(ServiceToken.Empty, instance);
            }
            await this.dispatcher.InvokeAsync(() =>
            {
                this.isOpened = true;
                this.OnOpened(EventArgs.Empty);
            });
        }

        public async Task CloseAsync()
        {
            var instanceByService = await this.dispatcher.InvokeAsync(() => this.instanceByService.ToArray());
            foreach (var item in instanceByService)
            {
                var service = item.Key;
                var instance = item.Value;
                await service.CloseAsync(ServiceToken.Empty);
                if (instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                this.adaptorHost.Disconnected -= AdaptorHost_Disconnected;
            }
            await this.adaptorHost.CloseAsync();
            await this.dispatcher.InvokeAsync(() =>
            {
                this.isOpened = false;
                this.OnClosed(EventArgs.Empty);
            });
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
        }

        public ServiceCollection Services { get; }

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
            await this.CloseAsync();
        }

        #region IServiecHost

        IReadOnlyList<IService> IServiceHost.Services => this.Services;

        #endregion
    }
}
