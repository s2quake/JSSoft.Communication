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

namespace JSSoft.Communication.ExceptionSerializers
{
    class ExceptionSerializer : ExceptionSerializerBase<Exception>
    {
        private static readonly Exception empty = new();

        public ExceptionSerializer()
            : base(new Guid("417fd69f-76ba-49cf-908e-19bc0b41b656"))
        {

        }

        public override Type[] PropertyTypes => new Type[] { typeof(string) };

        public static readonly ExceptionSerializer Default = new();

        protected override Exception CreateInstance(object[] args)
        {
            if (args[0] is not string message)
                return new Exception();
            return new Exception(message);
        }

        protected override object[] SelectProperties(Exception e)
        {
            if (e.Message == empty.Message)
                return new object[] { null };
            return new object[] { e.Message };
        }
    }
}
