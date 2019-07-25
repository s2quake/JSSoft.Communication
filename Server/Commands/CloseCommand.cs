
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;
using Ntreev.Library.Commands;

namespace Server.Commands
{
    [Export(typeof(ICommand))]
    class CloseCommand : CommandAsyncBase
    {
        private readonly IServiceHost serviceHost;

        [ImportingConstructor]
        public CloseCommand(IServiceHost serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public override bool IsEnabled => this.serviceHost.IsOpened;

        protected override Task OnExecuteAsync()
        {
            return this.serviceHost.CloseAsync();
        }
    }
}