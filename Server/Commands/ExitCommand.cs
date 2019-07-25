using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ntreev.Library.Commands;
using Server;

namespace Server.Commands
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