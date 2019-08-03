using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

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

        internal async Task<(Type, object)> InvokeAsync(object instance, object[] args)
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

        internal void Invoke(object instance, IReadOnlyList<string> datas)
        {
            var args = SerializerUtility.GetArguments(this.ParameterTypes, datas);
            this.MethodInfo.Invoke(instance, args);
        }

        internal static string GenerateName(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
            var parameterTypeNames = string.Join<Type>(", ", parameterTypes);
            return $"{methodInfo.ReturnType} {methodInfo.ReflectedType}.{methodInfo.Name}({parameterTypeNames})";
        }

        public string Name { get; }

        internal MethodInfo MethodInfo { get; }

        public Type[] ParameterTypes { get; }

        public Type ReturnType { get; }

        public bool IsAsync { get; }
    }
}