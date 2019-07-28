using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication.Grpc
{
    class PeerProperties
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public void Add(string peer, string key, object value)
        {
            this.properties.Add(GetKey(peer, key), value);
        }

        public void Contains(string peer, string key)
        {
            this.properties.ContainsKey(GetKey(peer, key));
        }

        public void Remove(string peer, string key)
        {
            this.properties.Remove(GetKey(peer, key));
        }

        public object this[string peer, string key]
        {
            get => this.properties[GetKey(peer, key)];
            set => this.properties[GetKey(peer, key)] = value;
        }

        private static string GetKey(string peer, string key)
        {
            return $"{peer}.{key}";
        }
    }
}
