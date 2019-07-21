using System;
using System.Collections.Generic;
using Grpc.Core;

namespace Ntreev.Crema.Services
{
    class AdaptorHost : IAdaptorHost
    {
        private Channel channel;

        #region IAdaptorHost
        

        IAdaptor IAdaptorHost.Create(IService service)
        {
            return new AdaptorImpl(this.channel, service);
        }

        void IAdaptorHost.Open(string host, int port, IEnumerable<IAdaptor> adaptors)
        {
            this.channel = new Channel($"{host}:{port}", Grpc.Core.ChannelCredentials.Insecure);
            //this.Ports.Add(new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure));
            foreach (var item in adaptors)
            {
                // if (item is AdaptorImpl adator)
                // {
                //     //this.Services.Add(Adaptor.BindService(adator));
                // }
                // else
                // {
                //     throw new ArgumentException("item is invalid adator", nameof(adaptors));
                // }
            }
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