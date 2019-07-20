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
            var serviceHost = Container.GetService<IServiceHost>();
            serviceHost.Open();
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
            serviceHost.Close();
        }
    }
}
