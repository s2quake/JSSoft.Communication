using System;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication.Grpc
{
    class PeerProperties
    {
        private readonly Dictionary<string, object> properties = new Dictionary<string, object>();

        public void Add(string peer, string key, object value)
        {
            this.properties.Add($"{peer}.{key}", value);
        }

        public void Contains(string peer, string key)
        {
            this.properties.ContainsKey($"{peer}.{key}");
        }

        public void Remove(string peer, string key)
        {
            this.properties.Remove($"{peer}.{key}");
        }

        public object this[string peer, string key]
        {
            get => this.properties[$"{peer}.{key}"];
            set => this.properties[$"{peer}.{key}"] = value;
        }
    }
}