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
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Communication
{
    [Export(typeof(IExceptionSerializer))]
    class ArgumentExceptionSerializer : ExceptionSerializerBase<ArgumentException>
    {
        protected override ArgumentException Deserialize((Type, object)[] args)
        {
            var message = args[0].Item2 as string;
            var paramName = args[1].Item2 as string;
            return new ArgumentException(message, paramName);
        }

        protected override (Type, object)[] Serialize(ArgumentException e)
        {
            return new (Type, object)[]
            {
                (typeof(string), e.Message),
                (typeof(string), e.ParamName)
            };
        }
    }
}