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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Grpc.Core;
using Ntreev.Crema.Communication.Grpc;

namespace Ntreev.Crema.Communication.Grpc
{
    class AdaptorServerHost : IAdaptorHost
    {
        private IService[] services;
        private Server server;
        private AdaptorServerImpl adaptor;
        private ServiceInstanceBuilder instanceBuilder = new ServiceInstanceBuilder();

        public AdaptorServerHost(IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public void Open(string host, int port)
        {
            this.adaptor = new AdaptorServerImpl(this.services);
            this.server = new Server()
            {
                Services = { Adaptor.BindService(this.adaptor) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) },
            };
            this.server.Start();
        }

        public void Close()
        {
            this.adaptor.Dispose();
            this.adaptor = null;
            this.server.ShutdownAsync().Wait();
            this.server = null;
        }

        public object Create(IService service)
        {
            var instanceType = service.CallbackType;
            var typeName = $"{instanceType.Name}Impl";
            var typeNamespace = instanceType.Namespace;
            var implType = instanceBuilder.CreateType(typeName, typeNamespace, typeof(ContextBase), instanceType);
            var instance = TypeDescriptor.CreateInstance(null, implType, null, null) as ContextBase;
            instance.Service = service;
            instance.Invoker = this.adaptor;
            return instance;
        }

        public void Dispose()
        {
            
        }
    }
}