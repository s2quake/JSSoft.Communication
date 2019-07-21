using System;

namespace Ntreev.Crema.Services
{
    public struct InvokeInfo
    {
        public string Name { get; set; }

        public Type[] Types { get; set; }

        public object[] Datas { get; set; }
    }

    public struct InvokeResult
    {
        public Type[] Types { get; set; }

        public object[] Datas { get; set; }
    }

    public struct PollItem
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public Type[] Types { get; set; }

        public object[] Datas { get; set; }
    }
}
