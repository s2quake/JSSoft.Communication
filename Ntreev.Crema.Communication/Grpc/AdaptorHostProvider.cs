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
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Communication.Grpc
{
    [Export(typeof(IAdaptorHostProvider))]
    class AdaptorHostProvider : IAdaptorHostProvider
    {
        private readonly IEnumerable<IServiceHost> services;
        private readonly IEnumerable<IExceptionSerializer> exceptionSerializers;

        [ImportingConstructor]
        public AdaptorHostProvider([ImportMany]IEnumerable<IServiceHost> services,
                                   [ImportMany]IEnumerable<IExceptionSerializer> exceptionSerializers)
        {
            //Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            this.services = services.ToArray();
            this.exceptionSerializers = exceptionSerializers.ToArray();
        }

        public IAdaptorHost Create(ICommunicationService serviceHost, ServiceToken token)
        {
            if (serviceHost is ServerCommunicationServiceBase)
                return new AdaptorServerHost(this.services, this.exceptionSerializers);
            else if (serviceHost is ClientCommunicationServiceBase)
                return new AdaptorClientHost(this.services, this.exceptionSerializers);
            throw new NotImplementedException();
        }

        public string Name => "grpc";
    }
}