using System;

namespace Ntreev.Crema.Services
{
    public struct InvokeInfo
    {
        public string ServiceName {get;set;}

        public string Name { get; set; }

        public Type[] Types { get; set; }

        public object[] Datas { get; set; }
    }

    public struct InvokeResult
    {
        public string ServiceName {get;set;}

        public Type Type { get; set; }

        public object Data { get; set; }
    }

    public struct PollItem
    {
        public string ServiceName {get;set;}
        
        public int ID { get; set; }

        public string Name { get; set; }

        public Type[] Types { get; set; }

        public object[] Datas { get; set; }
    }
}
