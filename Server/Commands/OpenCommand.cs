
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;
using Ntreev.Library.Commands;

namespace Server.Commands
{
    [Export(typeof(ICommand))]
    class OpenCommand : CommandAsyncBase
    {
        private readonly IServiceHost serviceHost;

        [ImportingConstructor]
        public OpenCommand(IServiceHost serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public override bool IsEnabled => this.serviceHost.IsOpened == false;

        protected override Task OnExecuteAsync()
        {
            return this.serviceHost.OpenAsync();
        }
    }
}