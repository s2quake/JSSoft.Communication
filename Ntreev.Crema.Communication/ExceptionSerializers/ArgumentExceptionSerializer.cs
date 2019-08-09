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

namespace Ntreev.Crema.Communication.ExceptionSerializers
{
    [Export(typeof(IExceptionDescriptor))]
    class ArgumentExceptionSerializer : ExceptionSerializerBase<ArgumentException>
    {
        private readonly Dictionary<string, string> messageByParam = new Dictionary<string, string>();

        public ArgumentExceptionSerializer()
            : base(-2)
        {

        }

        public override Type[] PropertyTypes => new Type[]
        {
            typeof(string),
            typeof(string)
        };

        protected override ArgumentException CreateInstance(object[] args)
        {
            var paramName = args[0] as string;
            var message = args[1] as string;
            if (paramName != null && message != null)
                new ArgumentException(message, paramName);
            else if (paramName == null)
                return new ArgumentException(message);
            return new ArgumentException();
        }

        protected override object[] SelectProperties(ArgumentException e)
        {
            var paramName = e.ParamName;
            if (this.messageByParam.ContainsKey(paramName) == false)
            {
                var exception = new ArgumentException(paramName);
                this.messageByParam.Add(paramName, exception.Message);
            }
            var message = e.Message == this.messageByParam[paramName] ? null : e.Message;;
            return new object[] { paramName, message };
        }
    }
}
