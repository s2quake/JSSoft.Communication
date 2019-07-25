using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Ntreev.Library.Commands;
using Client;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Commands
{
    [Export(typeof(ICommand))]
    class ExitCommand : CommandAsyncBase
    {
        [Import]
        private Lazy<IShell> shell = null;

        public ExitCommand()
        {

        }

        [CommandProperty(IsRequired = true)]
        [DefaultValue(0)]
        public int ExitCode
        {
            get; set;
        }

        protected override Task OnExecuteAsync()
        {
            return this.shell.Value.StopAsync();
        }
    }
}