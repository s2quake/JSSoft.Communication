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
    public abstract class ExceptionSerializerBase<T> : IExceptionSerializer
    {
        protected ExceptionSerializerBase(int exceptionCode)
        {
            this.ExceptionCode = exceptionCode;
        }

        public Type ExceptionType => typeof(T);

        public int ExceptionCode { get; }

        public abstract Type[] ArgumentTypes { get; }

        protected abstract T Deserialize(object[] args);

        protected abstract object[] Serialize(T e);

        #region IDataSerializer

        object IExceptionSerializer.Deserialize(string text)
        {
            var args = SerializerUtility.GetArguments(this.ArgumentTypes, text);
            return this.Deserialize(args);
        }

        string IExceptionSerializer.Serialize(object e)
        {
            var args = this.Serialize((T)e);
            return SerializerUtility.GetStrings(this.ArgumentTypes, args);
        }

        #endregion
    }
}