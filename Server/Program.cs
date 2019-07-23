﻿using System;



namespace Server
{
    class Test
    {
        public void Invoke()
        {

        }
    }
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
