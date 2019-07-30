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
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorClientHost : IAdaptorHost
    {
        private IService[] services;
        private IExceptionSerializer[] exceptionSerializers;
        private Channel channel;
        private AdaptorClientImpl adaptorImpl;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorClientHost(IEnumerable<IService> services, IEnumerable<IExceptionSerializer> exceptionSerializers)
        {
            this.services = services.ToArray();
            this.exceptionSerializers = exceptionSerializers.ToArray();
        }

        public Task OpenAsync(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            this.adaptorImpl = new AdaptorClientImpl(this.channel, this.services, this.exceptionSerializers);
            this.adaptorImpl.Disconnected += AdaptorImpl_Disconnected;
            return Task.Delay(1);
        }

        public async Task CloseAsync()
        {
            if (this.adaptorImpl != null)
                await this.adaptorImpl?.DisposeAsync();
            this.adaptorImpl = null;
            await this.channel.ShutdownAsync();
            this.channel = null;
        }

        public object Create(IService service)
        {
            var instanceType = service.ServiceType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(InstanceBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as InstanceBase;
            instance.Service = service;
            instance.Invoker = this.adaptorImpl;
            return instance;
        }

        public void Dispose()
        {

        }

        public event EventHandler<DisconnectionReasonEventArgs> Disconnected;

        private void AdaptorImpl_Disconnected(object sender, DisconnectionReasonEventArgs e)
        {
            this.adaptorImpl = null;
            this.Disconnected?.Invoke(this, e);
        }
    }
}