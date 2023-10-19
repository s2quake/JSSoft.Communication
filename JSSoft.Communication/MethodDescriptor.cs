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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace JSSoft.Communication;

public sealed class MethodDescriptor
{
    internal MethodDescriptor(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
        ParameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
        ReturnType = methodInfo.ReturnType;
        if (ReturnType == typeof(Task))
        {
            ReturnType = typeof(void);
            IsAsync = true;
        }
        else if (ReturnType.IsSubclassOf(typeof(Task)) == true)
        {
            ReturnType = ReturnType.GetGenericArguments().First();
            IsAsync = true;
        }
        Name = GenerateName(methodInfo);
        ShortName = methodInfo.Name;
    }

    public async Task<(string, Type, object?)> InvokeAsync(IServiceProvider serviceProvider, object instance, object?[] args)
    {
        try
        {
            var (type, value) = await InvokeAsync(instance, args);
            return (string.Empty, type, value);
        }
        catch (TargetInvocationException e)
        {
            var exception = e.InnerException ?? e;
            return (exception.GetType().AssemblyQualifiedName!, exception.GetType(), exception);
        }
        catch (Exception e)
        {
            return (e.GetType().AssemblyQualifiedName!, e.GetType(), e);
        }
    }

    public string Name { get; }

    public string ShortName { get; }

    public Type[] ParameterTypes { get; }

    public Type ReturnType { get; }

    public bool IsAsync { get; }

    internal static string GenerateName(MethodInfo methodInfo)
    {
        var parameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
        return GenerateName(methodInfo.ReturnType, methodInfo.ReflectedType!, methodInfo.Name, parameterTypes);
    }

    internal static string GenerateName(MethodInfo methodInfo, Type serviceType)
    {
        var parameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
        return GenerateName(methodInfo.ReturnType, serviceType, methodInfo.Name, parameterTypes);
    }

    internal static string GenerateName(Type returnType, Type reflectedType, string methodName, params Type[] parameterTypes)
    {
        var parameterTypeNames = string.Join<Type>(", ", parameterTypes);
        return $"{returnType} {reflectedType}.{methodName}({parameterTypeNames})";
    }

    internal MethodInfo MethodInfo { get; }

    private async Task<(Type, object?)> InvokeAsync(object? instance, object?[] args)
    {
        var value = await Task.Run(() => MethodInfo.Invoke(instance, args));
        var valueType = MethodInfo.ReturnType;
        if (value is Task task)
        {
            await task;
            var taskType = task.GetType();
            if (taskType.GetGenericArguments().Any() == true)
            {
                var propertyInfo = taskType.GetProperty(nameof(Task<object>.Result));
                value = propertyInfo!.GetValue(task);
                valueType = propertyInfo.PropertyType;
            }
            else
            {
                value = null;
                valueType = typeof(void);
            }
        }
        return (valueType, value);
    }
}
