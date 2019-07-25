using System;
using System.ComponentModel;
using Ntreev.Library.Commands;

namespace Server
{
    public class Settings
    {
        [CommandProperty]
        [DefaultValue(4004)]
        public int Port
        {
            get; set;
        }
    }
}