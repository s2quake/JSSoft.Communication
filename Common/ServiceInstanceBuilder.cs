using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

//[assembly: InternalsVisibleTo("Ntreev.Crema.Services.Runtime")]

namespace Ntreev.Crema.Services
{
    class ServiceInstanceBuilder
    {
        private readonly Dictionary<string, Type> typeByName = new Dictionary<string, Type>();
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;

        public ServiceInstanceBuilder(string assemblyName)
        {
            this.AssemblyName = new AssemblyName(assemblyName);
        }

        public Type CreateType(string name, string typeNamespace, Type interfaceType)
        {
            var fullname = $"{typeNamespace}.{name}";
            if (this.typeByName.ContainsKey(fullname) == false)
            {
                var type = CreateType(name, typeNamespace, interfaceType);
                this.typeByName.Add(fullname, type);
            }
            return this.typeByName[fullname];
        }

        public AssemblyName AssemblyName { get; }

        private AssemblyBuilder AssemblyBuilder
        {
            get
            {
                if (this.assemblyBuilder == null)
                {
                    this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(this.AssemblyName, AssemblyBuilderAccess.Run);
                    this.moduleBuilder = assemblyBuilder.DefineDynamicModule(this.AssemblyName.Name);
                }
                return this.assemblyBuilder;
            }
        }

        private ModuleBuilder ModuleBuilder
        {
            get
            {
                if (this.assemblyBuilder == null)
                {
                    this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(this.AssemblyName, AssemblyBuilderAccess.Run);
                    this.moduleBuilder = assemblyBuilder.DefineDynamicModule(this.AssemblyName.Name);
                }
                return this.moduleBuilder;
            }
        }

        private static Type CreateType(ModuleBuilder moduleBuilder, string typeName, string typeNamespace, Type interfaceType)
        {
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, typeof(CallbackBase), new Type[] { interfaceType });
            var methods = interfaceType.GetMethods();
            foreach (var item in methods)
            {
                CreateMethod(typeBuilder, item);
            }

            return typeBuilder.CreateType();
        }
        private static void CreateMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethod(nameof(CallbackBase.InvokeDelegate), BindingFlags.NonPublic | BindingFlags.Instance);
            var arrayCount = parameterInfos.Length * 2;
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, methodInfo.Name);
            il.Emit(OpCodes.Ldc_I4, arrayCount);
            il.Emit(OpCodes.Newarr, typeof(object));

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