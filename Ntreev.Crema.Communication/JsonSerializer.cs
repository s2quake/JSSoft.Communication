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
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ntreev.Crema.Communication
{
    class JsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();
        private readonly Dictionary<Type, IDataSerializer> dataSerializerByType;

        public JsonSerializer(IDataSerializer[] dataSerializers)
        {
            this.dataSerializerByType = dataSerializers.ToDictionary(item => item.Type);
        }

        public string Serialize(Type type, object data)
        {
            if (this.dataSerializerByType.ContainsKey(type) == true)
            {
                var dataSerializer = this.dataSerializerByType[type];
                return dataSerializer.Serialize(this, data);
            }
            return JsonConvert.SerializeObject(data, type, settings);
        }

        public object Deserialize(Type type, string text)
        {
            if (this.dataSerializerByType.ContainsKey(type) == true)
            {
                var dataSerializer = this.dataSerializerByType[type];
                return dataSerializer.Deserialize(this, text);
            }
            return JsonConvert.DeserializeObject(text, type, settings);
        }
    }
}
