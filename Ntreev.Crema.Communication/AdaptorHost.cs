using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Ntreev.Crema.Communication
{
    [Export(typeof(IAdaptorHost))]
    class AdaptorHost : IAdaptorHost
    {
        private IService[] services;
        private Grpc.Core.Server server;
        private AdaptorImpl adaptor;

        [ImportingConstructor]
        public AdaptorHost([ImportMany]IEnumerable<IService> services)
        {
            this.services = services.ToArray();
        }

        #region IAdaptorHost

        void IAdaptorHost.Open(string host, int port)
        {
            this.adaptor = new AdaptorImpl(this.services);
            this.server = new Grpc.Core.Server()
            {
                Services = { Adaptor.BindService(this.adaptor) },
                Ports = { new Grpc.Core.ServerPort(host, port, Grpc.Core.ServerCredentials.Insecure) },
            };
            this.server.Start();
        }

        void IAdaptorHost.Close()
        {
            this.server.ShutdownAsync().Wait();
            this.server = null;
        }

        #endregion
    }
}