using System;
using System.Threading.Tasks;
using Ntreev.Library.Commands;

namespace Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var settings = new Settings();
                var parser = new CommandLineParser(settings);
                parser.Parse(Environment.CommandLine);
                using (var shell = Shell.Create())
                {
                    await shell.StartAsync(settings);
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
