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

namespace Ntreev.Crema.Communication
{
    static class SerializerUtility
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings();

        public static object[] GetArguments(IReadOnlyList<string> types, IReadOnlyList<string> datas)
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
                var type = Type.GetType(types[i]);
                args[i] = JsonConvert.DeserializeObject(datas[i], type, settings);
            }
            return args;
        }

        public static (string[], string[]) GetStrings(object[] args)
        {
            var length = args.Length / 2;
            var types = new string[length];
            var datas = new string[length];
            for (var i = 0; i < length; i++)
            {
                var type = (Type)args[i * 2 + 0];
                var value = args[i * 2 + 1];
                types[i] = type.AssemblyQualifiedName;
                datas[i] = JsonConvert.SerializeObject(value, type, settings);
            }
            return (types, datas);
        }

        public static (Type, object)[] GetArguments(string text)
        {
            var datas = ((string, string)[])JsonConvert.DeserializeObject(text, typeof((string, string)[]), settings);
            var items = new (Type, object)[text.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                var type = datas[i].Item1;
                var value = datas[i].Item2;
                items[i].Item1 = Type.GetType(type);
                items[i].Item2 = JsonConvert.DeserializeObject(value, items[i].Item1, settings);
            }
            return items;
        }

        public static string GetStrings(params (Type, object)[] args)
        {
            var items = new (string, string)[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var type = (Type)args[i].Item1;
                var value = args[i].Item2;
                items[i].Item1 = type.AssemblyQualifiedName;
                items[i].Item2 = JsonConvert.SerializeObject(value, type, settings);
            }
            return JsonConvert.SerializeObject(items, typeof((string, string)), settings);
        }

        public static T GetValue<T>(string type, string data)
        {
            var runtimeType = Type.GetType(type);
            return (T)JsonConvert.DeserializeObject(data, runtimeType, settings);
        }
    }
}
