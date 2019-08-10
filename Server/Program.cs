using System;
using System.Threading.Tasks;

namespace JSSoft.Communication.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (var shell = Shell.Create())
                {
                    await shell.StartAsync();
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
