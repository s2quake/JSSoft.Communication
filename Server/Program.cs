using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Ntreev.Crema.Services.Users;

namespace Server
{
    class Program
    {
        const int port = 4004;
        static void Main(string[] args)
        {
            var assemblyName = new AssemblyName("Ntreev.Crema.Services.Runtime");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("moduleBuilder");
            var typeBuilder = moduleBuilder.DefineType("Test");

            typeBuilder.AddInterfaceImplementation(typeof(IUserServiceCallback));

            foreach (var item in typeof(IUserServiceCallback).GetMethods())
            {
                var methodBuilder = typeBuilder.DefineMethod(item.Name, MethodAttributes.Public);
                var parameterTypes = item.GetParameters().Select(i => i.ParameterType).ToArray();
                methodBuilder.SetParameters(parameterTypes);
                methodBuilder.SetReturnType(item.ReturnType);
                methodBuilder.DefineParameter(0, ParameterAttributes.None, parameterTypes[0].Name);
                var il = methodBuilder.GetILGenerator();

            }

            return;
            var cancellation = new System.Threading.CancellationTokenSource();
            var userService = new Ntreev.Crema.Services.Users.UserService();
            var server = new Grpc.Core.Server()
            {
                Services = { Ntreev.Crema.Services.Adaptor.BindService(userService) },
                Ports = { new Grpc.Core.ServerPort("localhost", 4004, Grpc.Core.ServerCredentials.Insecure) }
            };

            var task = Task.Run(() =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    var words = Ntreev.Library.Random.RandomUtility.NextWord();
                    userService.OnLoggedIn(words);
                    Console.WriteLine(words);
                    Thread.Sleep(1000);
                }
            });

            server.Start();
            Console.WriteLine("press any key to exit.");
            Console.ReadKey();
            cancellation.Cancel();
            task.Wait();
            userService.Dispose();
            server.ShutdownAsync().Wait();
        }
    }
}
