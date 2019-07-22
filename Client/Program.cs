using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Crema.Services;
using Ntreev.Crema.Services.Users;

namespace Client
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
