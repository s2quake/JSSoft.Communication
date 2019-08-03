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
using System.Threading.Tasks;
using Ntreev.Library.ObjectModel;

namespace Ntreev.Crema.Communication
{
    public interface IServiceHost : IDisposable
    {
        Task OpenAsync(ServiceToken token, IDataSerializer serializer);

        Task CloseAsync(ServiceToken token);

        /// <return>
        /// 1. code
        /// 2. type of value
        /// 3. value
        /// </return>
        Task<(int, Type, object)> InvokeAsync(object instance, string name, object[] args);

        IContainer<MethodDescriptor> Methods { get; }

        object CreateInstance(object obj);

        Type InstanceType { get; }

        Type ImplementedType { get; }

        string Name { get; }

        event EventHandler Opened;

        event EventHandler Closed;
    }
}