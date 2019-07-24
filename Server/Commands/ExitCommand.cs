using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Ntreev.Library.Commands;
using Server;

namespace Ntreev.Crema.Services.Commands
{
    [Export(typeof(ICommand))]
    class ExitCommand : CommandBase
    {
        [Import]
        private Lazy<IShell> shell = null;

        public ExitCommand()
            : base("exit")
        {

        }

        [CommandProperty(IsRequired = true)]
        [DefaultValue(0)]
        public int ExitCode
        {
            get; set;
        }

        protected override void OnExecute()
        {
            this.shell.Value.Stop();
        }
    }
}