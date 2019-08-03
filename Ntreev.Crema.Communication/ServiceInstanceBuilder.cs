// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Ntreev.Crema.Communication.Runtime")]

namespace Ntreev.Crema.Communication
{
    sealed class ServiceInstanceBuilder
    {
        private readonly Dictionary<string, Type> typeByName = new Dictionary<string, Type>();
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;

        internal ServiceInstanceBuilder()
        {
            this.AssemblyName = new AssemblyName("Ntreev.Crema.Communication.Runtime");
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
                if (returnType == typeof(Task))
                {
                    CreateInvoke(typeBuilder, item, nameof(IAdaptorHost.InvokeAsync), false);
                }
                else if (returnType.IsSubclassOf(typeof(Task)) == true)
                {
                    CreateInvoke(typeBuilder, item, nameof(IAdaptorHost.InvokeAsync), true);
                }
                else if (returnType == typeof(void))
                {
                    CreateInvoke(typeBuilder, item, nameof(IAdaptorHost.Invoke), false);
                }
                else
                {
                    CreateInvoke(typeBuilder, item, nameof(IAdaptorHost.Invoke), true);
                }
            }

            return typeBuilder.CreateType();
        }

        private static void CreateInvoke(TypeBuilder typeBuilder, MethodInfo methodInfo, string baseMethodName, bool isGenericMethod)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, 
                                                        CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                   .FirstOrDefault(item => item.Name == baseMethodName && item.IsGenericMethod == isGenericMethod);
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(Type));
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldtoken, item.ParameterType);
                il.Emit(OpCodes.Call, typeofMethod);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_0);

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldarg, i + 1);
                if (item.ParameterType.IsClass == false)
                {
                    il.Emit(OpCodes.Box, item.ParameterType);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, invokeMethod);
            il.Emit(OpCodes.Ret);
        }

        private static void CreateGenericInvoke(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual, 
                                                        CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                   .FirstOrDefault(item => item.Name == nameof(IAdaptorHost.Invoke) && item.IsGenericMethod == true);
            var arrayCount = parameterInfos.Length * 2;
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
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

        private static void CreateInvokeAsync(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                                                    CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                   .FirstOrDefault(item => item.Name == nameof(IAdaptorHost.InvokeAsync) && item.IsGenericMethod == false);
            var arrayCount = parameterInfos.Length * 2;
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
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

        private static void CreateGenericInvokeAsync(TypeBuilder typeBuilder, MethodInfo methodInfo)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, MethodAttributes.Public | MethodAttributes.Virtual,
                                                CallingConventions.Standard, methodInfo.ReturnType, parameterTypes);
            var invokeMethod = typeBuilder.BaseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                                                   .FirstOrDefault(item => item.Name == nameof(IAdaptorHost.InvokeAsync) && item.IsGenericMethod == true);
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
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
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

            var genericMethod = invokeMethod.MakeGenericMethod(methodInfo.ReturnType.GetGenericArguments());
            il.Emit(OpCodes.Call, genericMethod);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }
    }
}