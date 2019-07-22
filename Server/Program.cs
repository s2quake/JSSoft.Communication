using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Users;



namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var shell = Shell.Create())
                {
                    shell.Start();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Environment.Exit(1);
            }
        }
    }
}
