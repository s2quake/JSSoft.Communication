using System;
using System.ComponentModel.Composition;
using Ntreev.Crema.Communication;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;

namespace Client
{
    [Export(typeof(IShell))]
    class Shell : CommandContextTerminal, IShell, IServiceProvider
    {
        private readonly IServiceHost serviceHost;
        private bool isDisposed;

        [ImportingConstructor]
        public Shell(CommandContext commandContext, IServiceHost serviceHost)
           : base(commandContext)
        {
            this.Prompt = "client";
            this.Postfix = ">";
            this.serviceHost = serviceHost;
        }

        public static IShell Create()
        {
            return Container.GetService<IShell>();
        }

        public void Dispose()
        {
            if (this.isDisposed == false)
            {
                Container.Release();
                this.isDisposed = true;
            }
        }

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            return Container.GetService(serviceType);
        }

        #endregion

        #region IShell

        void IShell.Start()
        {
            this.serviceHost.Open();
            base.Start();
        }

        void IShell.Stop()
        {
            base.Cancel();
            this.serviceHost.Close();
        }

        #endregion
    }
}