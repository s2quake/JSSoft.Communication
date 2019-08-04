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

namespace Ntreev.Crema.Communication
{
    public abstract class ExceptionSerializerBase<T> : IExceptionDescriptor, IDataSerializer
    {
        protected ExceptionSerializerBase(int exceptionCode)
        {
            this.ExceptionCode = exceptionCode;
        }

        public Type ExceptionType => typeof(T);

        public int ExceptionCode { get; }

        public abstract Type[] PropertyTypes { get; }

        protected abstract T CreateInstance(object[] args);

        protected abstract object[] SelectProperties(T e);

        #region IDataSerializer

        object IDataSerializer.Deserialize(ISerializer serializer, string text)
        {
            var datas = (string[])serializer.Deserialize(typeof(string[]), text);
            var args = serializer.DeserializeMany(this.PropertyTypes, datas);
            return this.CreateInstance(args);
        }

        string IDataSerializer.Serialize(ISerializer serializer, object data)
        {
            var args = this.SelectProperties((T)data);
            var datas = serializer.SerializeMany(this.PropertyTypes, args);
            return serializer.Serialize(typeof(string[]), datas);
        }

        Type IDataSerializer.Type => this.ExceptionType;

        #endregion
    }
}