using System;

namespace Ntreev.Crema.Services
{
    class ServiceInstanceBuilder
    {

        public ServiceInstanceBuilder(string assemblyName)
        {
            this.AssemblyName = assemblyName;
        }

        public string AssemblyName {get;}
        static void Main(string[] args)
        {
            var assemblyName = new AssemblyName("Ntreev.Crema.Services.Runtime");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("Ntreev.Crema.Services.Runtime");
            var typeBuilder = moduleBuilder.DefineType("Wow", TypeAttributes.Class | TypeAttributes.Public, typeof(CallbackBase), new Type[] { typeof(IUserServiceCallback) });
            var methods = typeof(IUserServiceCallback).GetMethods();
            foreach (var item in methods)
            {
                CreateMethod(typeBuilder, item);
            }

            var t = typeBuilder.CreateType();
            //assemblyBuilder.Save("Ntreev.Crema.Services.Runtime.dll");
            var obj = TypeDescriptor.CreateInstance(null, t, null, null);
            t.GetMethod("OnLoggedIn").Invoke(obj, new object[] { "werwer" });
        }
        static void CreateMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethod("InvokeDelegate", BindingFlags.NonPublic | BindingFlags.Instance);

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }
            var arrayCount = parameterInfos.Length * 2;

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, methodInfo.Name);

            il.Emit(OpCodes.Ldc_I4, arrayCount);
            il.Emit(OpCodes.Newarr, typeof(object));


            var typeofMethod = typeof(Type).GetMethod("GetTypeFromHandle");
            for (var i = 0; i < parameterInfos.Length; i++)
            {

                var item = parameterInfos[i];   
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i * 2 + 0);
                il.Emit(OpCodes.Ldtoken, item.ParameterType);
                il.Emit(OpCodes.Call, typeofMethod);
                il.Emit(OpCodes.Stelem_Ref);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i * 2 + 1);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (item.ParameterType.IsClass == false)
                {
                    il.Emit(OpCodes.Box, item.ParameterType);
                }

                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Call, invokeMethod);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }
    }
}