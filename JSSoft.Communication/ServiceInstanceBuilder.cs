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
using System.Threading.Tasks;

namespace JSSoft.Communication
{
    sealed class ServiceInstanceBuilder
    {
        private const string ns = "JSSoft.Communication.Runtime";
        private readonly Dictionary<string, Type> typeByName = new();
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;

        internal ServiceInstanceBuilder()
        {
            this.AssemblyName = new AssemblyName(ns);
            this.assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(this.AssemblyName, AssemblyBuilderAccess.RunAndCollect);
            this.moduleBuilder = assemblyBuilder.DefineDynamicModule(this.AssemblyName.Name);
        }

        internal static ServiceInstanceBuilder Create()
        {
            try
            {
                return new ServiceInstanceBuilder();
            }
            catch
            {
                return null;
            }
        }

        public Type CreateType(string name, Type baseType, Type interfaceType)
        {
            var fullname = $"{this.AssemblyName}.{name}";
            if (this.typeByName.ContainsKey(fullname) == false)
            {
                var type = CreateType(this.moduleBuilder, name, baseType, interfaceType);
                this.typeByName.Add(fullname, type);
            }
            return this.typeByName[fullname];
        }

        public AssemblyName AssemblyName { get; }

        private static Type CreateType(ModuleBuilder moduleBuilder, string typeName, Type baseType, Type interfaceType)
        {
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public, baseType, new Type[] { interfaceType });
            var methods = interfaceType.GetMethods();
            foreach (var item in methods)
            {
                var returnType = item.ReturnType;
                if (returnType == typeof(Task))
                {
                    CreateInvokeAsync(typeBuilder, item, InstanceBase.InvokeAsyncMethod);
                }
                else if (returnType.IsSubclassOf(typeof(Task)) == true)
                {
                    CreateInvokeAsync(typeBuilder, item, InstanceBase.InvokeGenericAsyncMethod);
                }
                else if (returnType == typeof(void))
                {
                    CreateInvoke(typeBuilder, item, InstanceBase.InvokeMethod);
                }
                else
                {
                    CreateInvoke(typeBuilder, item, InstanceBase.InvokeGenericMethod);
                }
            }

            return typeBuilder.CreateType();
        }

        private static void CreateInvoke(TypeBuilder typeBuilder, MethodInfo methodInfo, string methodName)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var returnType = methodInfo.ReturnType;
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, methodAttributes, CallingConventions.Standard, returnType, parameterTypes);
            var invokeMethod = FindInvokeMethod(typeBuilder.BaseType, methodName, returnType);
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var pb = methodBuilder.DefineParameter(i, ParameterAttributes.Lcid, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.DeclareLocal(typeof(Type[]));
            il.DeclareLocal(typeof(object[]));
            if (returnType != typeof(void))
            {
                il.DeclareLocal(returnType);
            }
            il.Emit(OpCodes.Nop);
            il.EmitLdc_I4(parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(Type));
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.EmitLdc_I4(i);
                il.Emit(OpCodes.Ldtoken, parameterTypes[i]);
                il.Emit(OpCodes.Call, typeofMethod);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_0);
            il.EmitLdc_I4(parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.EmitLdc_I4(i);
                il.EmitLdarg(i + 1);
                if (item.ParameterType.IsClass == false)
                {
                    il.Emit(OpCodes.Box, parameterTypes[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, invokeMethod);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }

        private static void CreateInvokeAsync(TypeBuilder typeBuilder, MethodInfo methodInfo, string methodName)
        {
            var parameterInfos = methodInfo.GetParameters();
            var parameterTypes = parameterInfos.Select(i => i.ParameterType).ToArray();
            var returnType = methodInfo.ReturnType;
            var methodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig;
            var methodBuilder = typeBuilder.DefineMethod(methodInfo.Name, methodAttributes, returnType, parameterTypes);
            var invokeMethod = FindInvokeMethod(typeBuilder.BaseType, methodName, returnType);
            var typeofMethod = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));

            for (var i = 0; i < parameterInfos.Length; i++)
            {
                methodBuilder.DefineParameter(i, ParameterAttributes.None, parameterInfos[i].Name);
            }

            var il = methodBuilder.GetILGenerator();
            il.DeclareLocal(typeof(Type[]));
            il.DeclareLocal(typeof(object[]));
            il.DeclareLocal(returnType);
            il.Emit(OpCodes.Nop);
            il.EmitLdc_I4(parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(Type));
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.EmitLdc_I4(i);
                il.Emit(OpCodes.Ldtoken, parameterTypes[i]);
                il.Emit(OpCodes.Call, typeofMethod);
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_0);
            il.EmitLdc_I4(parameterInfos.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var item = parameterInfos[i];
                il.Emit(OpCodes.Dup);
                il.EmitLdc_I4(i);
                il.EmitLdarg(i + 1);
                if (item.ParameterType.IsClass == false)
                {
                    il.Emit(OpCodes.Box, parameterTypes[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc_1);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, MethodDescriptor.GenerateName(methodInfo));
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Call, invokeMethod);
            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ret);
        }

        private static MethodInfo FindInvokeMethod(Type baseType, string methodName, Type returnType)
        {
            var methodInfos = baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var item in methodInfos)
            {
                if (item.GetCustomAttribute<InstanceMethodAttribute>() is InstanceMethodAttribute attr && attr.MethodName == methodName)
                {
                    if (item.IsGenericMethod == true)
                    {
                        if (returnType.IsGenericType == true)
                            return item.MakeGenericMethod(returnType.GetGenericArguments());
                        else
                            return item.MakeGenericMethod(returnType);
                    }
                    return item;
                }
            }
            throw new NotImplementedException();
        }

        private static MethodInfo FindInvokeMethod(MethodInfo[] methodInfos, string methodName)
        {
            foreach (var item in methodInfos)
            {
                if (item.GetCustomAttribute<InstanceMethodAttribute>() is InstanceMethodAttribute attr && attr.MethodName == methodName)
                {
                    return item;
                }
            }
            throw new NotImplementedException();
        }
    }
}
