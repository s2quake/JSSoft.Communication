using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Ntreev.Crema.Services.Runtime")]

namespace Ntreev.Crema.Services
{
    sealed class ServiceInstanceBuilder
    {
        private readonly Dictionary<string, Type> typeByName = new Dictionary<string, Type>();
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;

        internal ServiceInstanceBuilder()
        {
            this.AssemblyName = new AssemblyName("Ntreev.Crema.Services.Runtime");
        }

        public Type CreateType(string name, string typeNamespace, Type baseType, Type interfaceType)
        {
            var fullname = $"{typeNamespace}.{name}";
            if (this.typeByName.ContainsKey(fullname) == false)
            {
                var type = CreateType(this.ModuleBuilder, name, typeNamespace, baseType, interfaceType);
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

        private static Type CreateType(ModuleBuilder moduleBuilder, string typeName, string typeNamespace, Type baseType, Type interfaceType)
        {
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, baseType, new Type[] { interfaceType });
            var methods = interfaceType.GetMethods();
            foreach (var item in methods)
            {
                var returnType = item.ReturnType;
                if (returnType.IsSubclassOf(typeof(Task)) == true)
                {
                    if (returnType.GetGenericArguments().Length > 0)
                        CreateAsyncMethodWithResult(typeBuilder, item);
                    else
                        CreateAsyncMethod(typeBuilder, item);
                }
                else
                {
                    CreateMethod(typeBuilder, item);
                }
            }

            return typeBuilder.CreateType();
        }
        private static void CreateMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethod(nameof(IServiceInstance.Invoke));
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

        private static void CreateAsyncMethod(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, typeof(void), parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethod(nameof(IServiceInstance.InvokeAsync));
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

        private static void CreateAsyncMethodWithResult(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, 
                                                CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethod(nameof(IServiceInstance.InvokeAsyncWithResult));
            var arrayCount = parameterInfos.Length * 2;
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            var label = il.DefineLabel();
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

var sss = invokeMethod.MakeGenericMethod(methodInfo.ReturnType.GetGenericArguments());
            il.Emit(OpCodes.Call, sss);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Br_S, label);
            il.MarkLabel(label);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);
        }
    }
}