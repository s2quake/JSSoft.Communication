using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Ntreev.Library.Commands;

namespace Client
{
    [Export(typeof(CommandContext))]
    class CommandContext : CommandContextBase
    {
        [ImportingConstructor]
        public CommandContext([ImportMany]IEnumerable<ICommand> commands, [ImportMany]IEnumerable<ICommandProvider> methods)
            : base(commands, methods)
        {

        }
    }
}