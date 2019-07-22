using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;
using Ntreev.Library.Threading;

// https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugKwDcOyAzEagQMIEDeOBbRNyAbAQJIB2ANwD2AawCmAQQDOAT34BjABTFC/AIYBbcVAIAHdQCct0gsIBGAK3ELgAbQC6BIwHNpASlbtvLbN+/AABaGwgDuBPzi4QBywsC8mnoQ4tr8wOIwAKIAHgriesBgwvxK7hR+7AC+Xmw1HERcADwAKgB8fEJiUnKKLa0q6GpaOvpGJmZWNvZOrh51PvPsQSHhkTFxCUkp4mkZOXkFRSVlddUVtee0BACq0uKGAMr3gmB5mxAEIAx1vv7s1A1GmA0u0ADLCFzAmTyZSqAgAVzuhk8lwWqL+RAA7AQgmBpAA6AQiCTQ3rA4D9DTaYQAMyU4Mh/FJCncumAsj04lpAzwrIRSJO6LYZwxBEWbAB3EaSnJulU7jBEOE8OAzJ5/PuunJBAUyrSKNFv1F/2xuIJRK6zOlsqIgwVSipXLpDKhPRZbI5Tp5fMRmpxnu55L5uvh+vKopFVRwlSAA===
namespace Ntreev.Crema.Services
{
    class ContextBase : IDisposable
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings();
        private List<PollItem> callbackList = new List<PollItem>();
        //private IAdaptor adaptor;

        public ContextBase()
        {
            //this.adaptor = adaptor;
            this.Dispatcher = new Dispatcher(this);
        }

        public IAdaptorHost AdaptorHost { get; set; }

        public Func<InvokeInfo, Task<InvokeResult>> InvokeDelegate { get; set; }

        public string ServiceName { get; set; }

        public void Dispose()
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException();
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        public async Task<T> InvokeAsyncWithResult<T>(string name, params object[] args)
        {
            var length = args.Length / 2;
            var invokeInfo = new InvokeInfo()
            {
                ServiceName = this.ServiceName,
                Name = name,
                Types = new Type[length],
                Datas = new object[length],
            };
            for (var i = 0; i < length; i++)
            {
                var type = (Type)args[i * 2 + 0];
                var value = args[i * 2 + 1];
                invokeInfo.Types[i] = type;
                invokeInfo.Datas[i] = value;
            }
            var result = await this.InvokeDelegate(invokeInfo);
            return (T)result.Data;
        }


        public void Invoke(string name, params object[] args)
        {
            throw new NotImplementedException();
        }


        public Task InvokeAsync(string name, params object[] args)
        {
            throw new NotImplementedException();
        }

        // public Task<InvokeResult> InvokeAsync(InvokeInfo info)
        // {
        //     throw new NotImplementedException();
        //     //Adaptor.AdaptorClient d;
        //     //d.in
        // }

        // public Task<PollItem[]> PollAsync(int id)
        // {
        //     return this.Dispatcher.InvokeAsync(() =>
        //     {
        //         var items = new PollItem[this.callbackList.Count - id];
        //         for (var i = id; i < this.callbackList.Count; i++)
        //         {
        //             items[id - i] = this.callbackList[i];
        //         }
        //         return items;
        //     });
        // }

        public Dispatcher Dispatcher { get; private set; }

        //public int ID { get; private set; }
    }

    class UserServiceImpl : ContextBase, IUserService
    {
        public UserServiceImpl()
        {

        }
        public Task<int> LoginAsync(string user)
        {
            return this.InvokeAsyncWithResult<int>(nameof(LoginAsync), typeof(string), user);
        }

        public Task<(int, string)> LogoutAsync(string user, int count)
        {
            throw new NotImplementedException();
        }
    }
}