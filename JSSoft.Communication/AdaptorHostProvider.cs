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

namespace JSSoft.Communication
{
    class AdaptorHostProvider : IAdaptorHostProvider
    {
        public const string DefaultName = "grpc";

        public IAdaptorHost Create(IServiceContext serviceContext, ServiceToken token)
        {
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            Environment.SetEnvironmentVariable("GRPC_TRACE", "all");
            global::Grpc.Core.GrpcEnvironment.SetLogger(new global::Grpc.Core.Logging.ConsoleLogger());

            if (serviceContext is ServerContextBase)
                return new Grpc.AdaptorServerHost(serviceContext);
            else if (serviceContext is ClientContextBase)
                return new Grpc.AdaptorClientHost(serviceContext);
            throw new NotImplementedException();
        }

        public string Name => DefaultName;

        public static readonly AdaptorHostProvider Default = new AdaptorHostProvider();
    }
}
