using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Services
{
    [Export(typeof(IAdaptorHost))]
    class AdaptorHost : IAdaptorHost
    {
        private IService[] services;
        private Grpc.Core.Server server;

        [ImportingConstructor]
        public AdaptorHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        #region IAdaptorHost

        // IAdaptor IAdaptorHost.Create(IService service)
        // {
        //     return new AdaptorImpl(this, service);
        // }

        void IAdaptorHost.Open(string host, int port)
        {
            this.server = new Grpc.Core.Server()
            {
                Services = {Adaptor.BindService(new AdaptorImpl(this.services)) },
                Ports = {new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure)},
            };
            this.server.Start();
            // this.server.Services.Add
            // this.server.Ports.Add(new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure));
            // foreach (var item in adaptors)
            // {
            //     if (item is AdaptorImpl adator)
            //     {
            //         this.Services.Add(Adaptor.BindService(adator));
            //     }
            //     else
            //     {
            //         throw new ArgumentException("item is invalid adator", nameof(adaptors));
            //     }
            // }
            // this.Start();
        }

        void IAdaptorHost.Close()
        {
            this.server.ShutdownAsync().Wait();
            this.server = null;
        }

        #endregion
    }
}