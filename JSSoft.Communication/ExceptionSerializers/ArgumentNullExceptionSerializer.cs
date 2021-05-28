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
using System.Runtime.Serialization;

namespace JSSoft.Communication.ExceptionSerializers
{
    class ArgumentNullExceptionSerializer : ExceptionSerializerBase<ArgumentNullException>
    {
        public ArgumentNullExceptionSerializer()
            : base("429dfc10-5ee5-4fb0-93da-9e06a85ff3cc")
        {
        }

        public static ArgumentNullExceptionSerializer Default { get; } = new();

        protected override void GetSerializationInfo(IReadOnlyDictionary<string, object> properties, SerializationInfo info)
        {
            base.GetSerializationInfo(properties, info);
            info.AddValue("ParamName", properties["ParamName"], typeof(string));
        }

        protected override void GetProperties(SerializationInfo info, IDictionary<string, object> properties)
        {
            base.GetProperties(info, properties);
            properties.Add("ParamName", info.GetString("ParamName"));
        }     
    }
}
