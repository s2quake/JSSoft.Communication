using System;

namespace Ntreev.Crema.Services
{
    public struct InvokeInfo
    {
        public string Name { get; set; }
        public string[] Types { get; set; }
        public string[] Datas { get; set; }
    }

    public struct InvokeResult
    {
        public string[] Types { get; set; }
        public string[] Datas { get; set; }
    }
}
