using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    class CallbackBase : IServiceInstance, IDisposable
    {
        private List<PollItem> callbackList = new List<PollItem>();

        public CallbackBase()
        {
            this.Dispatcher = new Dispatcher(this);
        }

        public void Dispose()
        {
            if (this.Dispatcher == null)
                throw new InvalidOperationException();
            this.Dispatcher.Dispose();
            this.Dispatcher = null;
        }

        public void Invoke(string name, params object[] args)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                var length = args.Length / 2;
                var pollItem = new PollItem()
                {
                    ID = this.ID++,
                    Name = name,
                    Types = new Type[length],
                    Datas = new string[length],
                };
                for (var i = 0; i < length; i++)
                {
                    var type = (Type)args[i * 2 + 0];
                    var value = args[i * 2 + 1];
                    pollItem.Types[i] = type;
                    pollItem.Datas[i] = value;
                }
                this.callbackList.Add(pollItem);
            });
        }

        public Task InvokeAsync(string name, params object[] args)
        {
throw new NotImplementedException();
        }
        

        public Task<T> InvokeAsyncWithResult<T>(string name, params object[] args)
        {
throw new NotImplementedException();
        }

        public Task<PollItem[]> PollAsync(int id)
        {
            return this.Dispatcher.InvokeAsync(() =>
            {
                var items = new PollItem[this.callbackList.Count - id];
                for (var i = id; i < this.callbackList.Count; i++)
                {
                    items[id - i] = this.callbackList[i];
                }
                return items;
            });
        }

        public Dispatcher Dispatcher { get; private set; }

        public int ID { get; private set; }
    }
}