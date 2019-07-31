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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ntreev.Crema.Communication
{
    static class SerializerUtility
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();

        public static object[] GetArguments(IReadOnlyList<Type> types, IReadOnlyList<string> datas)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            if (datas == null)
                throw new ArgumentNullException(nameof(datas));
            if (types.Count != datas.Count)
                throw new ArgumentException($"length of '{nameof(types)}' and '{nameof(datas)}' is different.");
            var args = new object[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                args[i] = JsonConvert.DeserializeObject(datas[i], type, settings);
            }
            return args;
        }

        public static string[] GetStrings(object[] args)
        {
            var length = args.Length / 2;
            var datas = new string[length];
            for (var i = 0; i < length; i++)
            {
                var type = (Type)args[i * 2 + 0];
                var value = args[i * 2 + 1];
                datas[i] = JsonConvert.SerializeObject(value, type, settings);
            }
            return datas;
        }

        public static string GetString(object value, Type type)
        {
            if (type == typeof(void))
                return null;
            return JsonConvert.SerializeObject(value, type, settings);
        }

        public static object[] GetArguments(Type[] types, string text)
        {
            var datas = (string[])JsonConvert.DeserializeObject(text, typeof(string[]), settings);
            var items = new object[text.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                var type = types[i];
                var value = datas[i];
                items[i] = JsonConvert.DeserializeObject(value, type, settings);
            }
            return items;
        }

        public static string GetStrings(Type[] types, object[] args)
        {
            var items = new string[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var type = (Type)types[i];
                var value = args[i];
                items[i] = JsonConvert.SerializeObject(value, type, settings);
            }
            return JsonConvert.SerializeObject(items, typeof((string, string)), settings);
        }

        public static T GetValue<T>(string data)
        {
            return (T)JsonConvert.DeserializeObject(data, typeof(T), settings);
        }
    }
}
