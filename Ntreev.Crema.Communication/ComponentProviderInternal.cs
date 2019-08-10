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
using System.Linq;

namespace Ntreev.Crema.Communication
{
    class ComponentProviderInternal : IComponentProvider
    {
        public ComponentProviderInternal(IEnumerable<IServiceHost> serviceHosts)
        {
            this.Services = serviceHosts.ToArray();
            this.AdaptorHostProviders = new IAdaptorHostProvider[]
            {
                new AdaptorHostProvider(),
            };
            this.SerializerProviders = new ISerializerProvider[]
            {
                new JsonSerializerProvider(),
            };
            this.DataSerializers = new IDataSerializer[]
            {
                new ExceptionSerializers.ArgumentExceptionSerializer(),
                new ExceptionSerializers.ArgumentNullExceptionSerializer(),
                new ExceptionSerializers.ExceptionSerializer(),
                new ExceptionSerializers.NotImplementedExceptionSerializer(),
            };
            this.ExceptionSerializers = this.DataSerializers.OfType<IExceptionDescriptor>().ToArray();
        }

        public IAdaptorHostProvider[] AdaptorHostProviders { get; }

        public IServiceHost[] Services { get; }

        public ISerializerProvider[] SerializerProviders { get; }

        public IDataSerializer[] DataSerializers { get; }

        public IExceptionDescriptor[] ExceptionSerializers { get; }
    }
}
