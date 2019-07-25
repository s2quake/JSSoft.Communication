using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ntreev.Crema.Communication.Grpc
{
    static class AdaptorUtility
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

        public static T GetValue<T>(string type, string data)
        {
            var runtimeType = Type.GetType(type);
            return (T)JsonConvert.DeserializeObject(data, runtimeType, settings);
        }
    }
}