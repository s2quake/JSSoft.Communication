using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    public class CallbackBase
    {
        private JsonSerializerSettings settings = new JsonSerializerSettings();
        private List<PollReplyItem> callbackList = new List<PollReplyItem>();

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

        public void InvokeDelegate(string name, params object[] args)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                var reply = new PollReplyItem()
                {
                    Id = this.ID++,
                    Name = name,
                };
                var length = args.Length / 2;
                for (var i = 0; i < length; i++)
                {
                    var type = (Type)args[i * 2 + 0];
                    var value = args[i * 2 + 1];
                    reply.Type.Add(type.AssemblyQualifiedName);
                    reply.Data.Add(JsonConvert.SerializeObject(value, type, this.settings));
                }
                this.callbackList.Add(reply);
            });
        }

        public Task PollAsync(PollReply reply, int id)
        {
            return this.Dispatcher.InvokeAsync(() =>
                {
                    var items = new PollReplyItem[this.callbackList.Count - id];
                    for (var i = id; i < this.callbackList.Count; i++)
                    {
                        reply.Items.Add(this.callbackList[i]);
                    }
                    return items;
                });
        }

        public Dispatcher Dispatcher { get; private set; }

        public int ID { get; private set; }

        public List<PollReplyItem> Items => this.callbackList;
    }
}