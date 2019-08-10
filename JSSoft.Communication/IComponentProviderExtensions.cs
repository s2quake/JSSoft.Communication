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
using JSSoft.Communication.ExceptionSerializers;
using Ntreev.Library.ObjectModel;
using Ntreev.Library.Threading;

namespace JSSoft.Communication
{
    static class IComponentProviderExtensions
    {
        public static IAdaptorHostProvider GetAdaptorHostProvider(this IComponentProvider componentProvider, string adaptorHostType)
        {
            var type = adaptorHostType ?? AdaptorHostProvider.DefaultName;
            return componentProvider.AdaptorHostProviders.First(item => item.Name == type);
        }

        public static ISerializerProvider GetserializerProvider(this IComponentProvider componentProvider, string serializerType)
        {
            var type = serializerType ?? JsonSerializerProvider.DefaultName;
            return componentProvider.SerializerProviders.First(item => item.Name == type);
        }

        public static IExceptionDescriptor GetExceptionDescriptor(this IComponentProvider componentProvider, Exception e)
        {
            var exceptionSerializer = componentProvider.ExceptionDescriptors.FirstOrDefault(item => item.ExceptionType == e.GetType());
            return exceptionSerializer ?? ExceptionSerializer.Default;
        }

        public static IExceptionDescriptor GetExceptionDescriptor(this IComponentProvider componentProvider, int exceptionCode)
        {
            var exceptionSerializer = componentProvider.ExceptionDescriptors.FirstOrDefault(item => item.ExceptionCode == exceptionCode);
            return exceptionSerializer ?? ExceptionSerializer.Default;
        }
    }
}