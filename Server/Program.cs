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

[assembly: InternalsVisibleTo("Ntreev.Crema.Services.Runtime")]

namespace Server
{
    public class C
    {
        public void M(params Object[] args)
        {
        }
    }
    class Program
    {
        const int port = 4004;

        // public class T : C{
        //     public void OnLoad(string userID) {
        //         base.M(userID);
        //     }
        // }

        static void Main(string[] args)
        {
            var assemblyName = new AssemblyName("Ntreev.Crema.Services.Runtime");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Ntreev.Crema.Services.Runtime");
            var typeBuilder = moduleBuilder.DefineType($"{nameof(IUserServiceCallback)}Impl", TypeAttributes.Public, typeof(CallbackBase), new Type[] { typeof(IUserServiceCallback) });
            var methods = typeof(IUserServiceCallback).GetMethods();
            foreach (var item in methods)
            {
                var parameterInfos = item.GetParameters();
                var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(item.Name, MethodAttributes.Public, typeof(void), parameterTypes);

                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
                }


                var il = methodBuilder.GetILGenerator();
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, item.Name);

                for (var i = 0; i < parameterInfos.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }
                var m = null as MethodInfo;
                var ms = typeof(CallbackBase).GetMethods();
                for (var i = 0; i < ms.Length; i++)
                {
                    if (ms[i].Name == $"{nameof(CallbackBase.Invoke)}" && ms[i].GetGenericArguments().Length == parameterInfos.Length)
                    {
                        m = ms[i];
                        break;
                    }
                }

                il.Emit(OpCodes.Call, m);
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ret);
            }


            var t = typeBuilder.CreateType();
            var obj = TypeDescriptor.CreateInstance(null, t, null, null);
            t.GetMethod("OnLoggedIn").Invoke(obj, new object[] { "werwer" });

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
