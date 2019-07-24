using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

// https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugKwDcOyAzEagQMIEDeOBbRNyAbAQJIB2ANwD2AawCmAQQDOAT34BjABTFC/AIYBbcVAIAHdQCct0gsIBGAK3ELgAbQC6BIwHNpASlbtvLbN+/AABaGwgDuBPzi4QBywsC8mnoQ4tr8wOIwAKIAHgriesBgwvxK7hR+7AC+Xmw1HERcADwAKgB8fEJiUnKKLa0q6GpaOvpGJmZWNvZOrh51PvPsQSHhkTFxCUkp4mkZOXkFRSVlddUVtee0BACq0uKGAMr3gmB5mxAEIAx1vv7s1A1GmA0u0ADLCFzAmTyZSqAgAVzuhk8lwWqL+RAA7AQgmBpAA6AQiCTQ3rA4D9DTaYQAMyU4Mh/FJCncumAsj04lpAzwrIRSJO6LYZwxBEWbAB3EaSnJulU7jBEOE8OAzJ5/PuunJBAUyrSKNFv1F/2xuIJRK6zOlsqIgwVSipXLpDKhPRZbI5Tp5fMRmpxnu55L5uvh+vKopFVRwlSAA===
namespace Ntreev.Crema.Communication
{
    class ContextBase : IDisposable
    {
        public ContextBase()
        {

        }

        public IAdaptorClientHost AdaptorHost { get; set; }

        public string ServiceName { get; set; }

        public void Dispose()
        {

        }

        public async Task<T> InvokeAsyncWithResult<T>(string name, params object[] args)
        {
            throw new NotImplementedException();
            // var length = args.Length / 2;
            // var invokeInfo = new InvokeInfo()
            // {
            //     ServiceName = this.ServiceName,
            //     Name = name,
            //     Types = new Type[length],
            //     Datas = new object[length],
            // };
            // for (var i = 0; i < length; i++)
            // {
            //     var type = (Type)args[i * 2 + 0];
            //     var value = args[i * 2 + 1];
            //     invokeInfo.Types[i] = type;
            //     invokeInfo.Datas[i] = value;
            // }
            // var result = await this.InvokeDelegate(invokeInfo);
            // return (T)result.Data;
        }

        protected void Invoke(string name, params object[] args)
        {
            this.AdaptorHost.Invoke(this.ServiceName, name, args);
        }

        protected T Invoke<T>(string name, params object[] args)
        {
            return this.AdaptorHost.Invoke<T>(this.ServiceName, name, args);
        }

        protected Task InvokeAsync(string name, params object[] args)
        {
            return this.AdaptorHost.InvokeAsync(this.ServiceName, name, args);
        }

        protected Task<T> InvokeAsync<T>(string name, params object[] args)
        {
            return this.AdaptorHost.InvokeAsync<T>(this.ServiceName, name, args);
        }
    }
}
