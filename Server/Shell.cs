using System;
using System.ComponentModel.Composition;
using Ntreev.Crema.Services;
using Ntreev.Library.Commands;

namespace Server
{
    [Export(typeof(IShell))]
    class Shell : CommandContextTerminal, IShell
    {
        private readonly IServiceHost serviceHost;
        private bool isDisposed;

        [ImportingConstructor]
        public Shell(CommandContext commandContext, IServiceHost serviceHost)
           : base(commandContext)
        {
            this.Prompt = "server";
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

        #region IShell

        void IShell.Start()
        {
            this.serviceHost.Open();
            base.Start();
        }

        void IShell.Cancel()
        {
            base.Cancel();
            this.serviceHost.Close();
        }

        #endregion
    }
}