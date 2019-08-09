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
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;

namespace Ntreev.Crema.Communication
{
    public sealed class MethodDescriptor
    {
        internal MethodDescriptor(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.ParameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
            this.ReturnType = methodInfo.ReturnType;
            if (this.ReturnType == typeof(Task))
            {
                this.ReturnType = typeof(void);
                this.IsAsync = true;
            }
            else if (this.ReturnType.IsSubclassOf(typeof(Task)) == true)
            {
                this.ReturnType = this.ReturnType.GetGenericArguments().First();
                this.IsAsync = true;
            }
            this.Name = GenerateName(methodInfo);
        }

        public async Task<(int, Type, object)> InvokeAsync(IServiceProvider serviceProvider, object instance, object[] args)
        {
            var componentProvider = serviceProvider.GetService(typeof(IComponentProvider)) as IComponentProvider;
            if (componentProvider == null)
            {
                throw new InvalidOperationException("can not get interface of IComponentProvider at serviceProvider");
            }
            try
            {
                var (type, value) = await this.InvokeAsync(instance, args);
                return (0, type, value);
            }
            catch (TargetInvocationException e)
            {
                var exception = e.InnerException ?? e;
                var exceptionSerializer = componentProvider.GetExceptionDescriptor(exception);
                return (exceptionSerializer.ExceptionCode, exception.GetType(), exception);
            }
            catch (Exception e)
            {
                var exceptionSerializer = componentProvider.GetExceptionDescriptor(e);
                return (exceptionSerializer.ExceptionCode, e.GetType(), e);
            }
        }
        
        public string Name { get; }

        public Type[] ParameterTypes { get; }

        public Type ReturnType { get; }

        public bool IsAsync { get; }

        internal static string GenerateName(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
            var parameterTypeNames = string.Join<Type>(", ", parameterTypes);
            return $"{methodInfo.ReturnType} {methodInfo.ReflectedType}.{methodInfo.Name}({parameterTypeNames})";
        }

        internal MethodInfo MethodInfo { get; }

        private async Task<(Type, object)> InvokeAsync(object instance, object[] args)
        {
            var value = await Task.Run(() => this.MethodInfo.Invoke(instance, args));
            var valueType = this.MethodInfo.ReturnType;
            if (value is Task task)
            {
                await task;
                var taskType = task.GetType();
                if (taskType.GetGenericArguments().Any() == true)
                {
                    var propertyInfo = taskType.GetProperty(nameof(Task<object>.Result));
                    value = propertyInfo.GetValue(task);
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
}
