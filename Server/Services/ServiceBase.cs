using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Ntreev.Crema.Services.Users;
using Ntreev.Library.Threading;

namespace Ntreev.Crema.Services
{
    abstract class ServiceBase : IService, IServiceInvoker
    {
        private readonly Type serviceType;
        private readonly Type callbackType;
        private CallbackBase callback;
        private Dispatcher dispatcher;
        private readonly PollReplyItem nullReply = new PollReplyItem() { Id = -1 };

        private IAdaptor adaptor;

        public ServiceBase(Type serviceType, Type callbackType)
        {
            if (serviceType.IsAssignableFrom(this.GetType()) == false)
                throw new ArgumentException("invalid type", nameof(serviceType));
            this.serviceType = serviceType;
            this.callbackType = callbackType;
            this.dispatcher = new Dispatcher(this);
        }

        // public override Task<InvokeReply> Invoke(InvokeRequest request, ServerCallContext context)
        // {
        //     throw new NotImplementedException();
        // }

        // public override async Task Poll(IAsyncStreamReader<PollRequest> requestStream, IServerStreamWriter<PollReply> responseStream, ServerCallContext context)
        // {
        //     while (await requestStream.MoveNext())
        //     {
        //         var request = requestStream.Current;
        //         var id = request.Id;
        //         var reply = new PollReply();
        //         await this.callback.PollAsync(reply, id);
        //         await responseStream.WriteAsync(reply);
        //     }
        // }

        public Task<InvokeResult> Invoke(InvokeInfo info)
        {
            
        }

        public void Dispose()
        {
            this.dispatcher.Dispose();
            this.dispatcher = null;
        }
        public object Callback => this.callback;

        public Type ServiceType => this.serviceType;

        public Type CallbackType => this.callbackType;

        public void Open(ServiceToken token)
        {
            this.adaptor = token.Adaptor;
            this.callback = token.Callback;
            this.OnOpened(EventArgs.Empty);
        }

        public void Close(ServiceToken token)
        {
            this.adaptor = null;
            this.callback = null;
        }

        public event EventHandler Opened;

        public event EventHandler Closed;

        protected virtual void OnOpened(EventArgs e)
        {
            this.OnOpened(EventArgs.Empty);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            this.OnClosed(EventArgs.Empty);
        }
    }
    abstract class ServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ServiceBase()
            : base(typeof(T), typeof(U))
        {

        }

        public new U Callback => (U)base.Callback;
    }
}