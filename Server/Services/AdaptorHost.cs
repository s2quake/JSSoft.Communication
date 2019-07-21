using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Ntreev.Crema.Services
{
    [Export(typeof(IAdaptorHost))]
    class AdaptorHost : Grpc.Core.Server, IAdaptorHost
    {
        #region IAdaptorHost

        IAdaptor IAdaptorHost.Create(IService service)
        {
            return new AdaptorImpl(this, service);
        }

        void IAdaptorHost.Open(string host, int port, IEnumerable<IAdaptor> adaptors)
        {
            this.Ports.Add(new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure));
            foreach (var item in adaptors)
            {
                if (item is AdaptorImpl adator)
                {
                    this.Services.Add(Adaptor.BindService(adator));
                }
                else
                {
                    throw new ArgumentException("item is invalid adator", nameof(adaptors));
                }
            }
            this.Start();
        }

        void IAdaptorHost.Close()
        {
            this.ShutdownAsync().Wait();
        }

        #endregion
    }
}