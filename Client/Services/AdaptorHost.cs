using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    [Export(typeof(IAdaptorHost))]
    class AdaptorHost : IAdaptorHost
    {
        private IService[] services;
        private Channel channel;
        private AdaptorImpl apdatorImpl;

        [ImportingConstructor]
        public AdaptorHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        public  Task<InvokeResult> InvokeAsync(InvokeInfo info)
        {
            return this.apdatorImpl.InvokeAsync(info);

        }
        
        public Task PollAsync(Action<PollItem> callback, CancellationToken cancellation)
        {
            return this.apdatorImpl.PollAsync(callback, cancellation);
        }

        #region IAdaptorHost
        
        // IAdaptor IAdaptorHost.Create(IService service)
        // {
        //     return new AdaptorImpl(this.channel, service);
        // }

        void IAdaptorHost.Open(string host, int port)
        {
            this.channel = new Channel($"{host}:{port}", Grpc.Core.ChannelCredentials.Insecure);
            this.apdatorImpl = new AdaptorImpl(this.channel, this.services);
            //this.Ports.Add(new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure));
            // foreach (var item in adaptors)
            // {
            //     // if (item is AdaptorImpl adator)
            //     // {
            //     //     //this.Services.Add(Adaptor.BindService(adator));
            //     // }
            //     // else
            //     // {
            //     //     throw new ArgumentException("item is invalid adator", nameof(adaptors));
            //     // }
            // }
            //this.channel.s
        }

        void IAdaptorHost.Close()
        {
            this.channel.ShutdownAsync().Wait();
            this.channel = null;
        }

        #endregion
    }
}