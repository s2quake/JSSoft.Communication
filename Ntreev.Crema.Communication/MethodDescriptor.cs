using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

namespace Ntreev.Crema.Communication
{
    sealed class MethodDescriptor
    {
        public MethodDescriptor(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.ParameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
            this.Name = GenerateName(methodInfo);
        }

        public async Task<(Type, object)> InvokeAsync(object instance, IReadOnlyList<string> datas)
        {
            var args = SerializerUtility.GetArguments(this.ParameterTypes, datas);
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

        public void Invoke(object instance, IReadOnlyList<string> datas)
        {
            var args = SerializerUtility.GetArguments(this.ParameterTypes, datas);
            this.MethodInfo.Invoke(instance, args);
        }

        public static string GenerateName(MethodInfo methodInfo)
        {
            var parameterTypes = methodInfo.GetParameters().Select(item => item.ParameterType).ToArray();
            var parameterTypeNames = string.Join<Type>(", ", parameterTypes);
            return $"{methodInfo.ReturnType} {methodInfo.ReflectedType}.{methodInfo.Name}({parameterTypeNames})";
        }

        public string Name { get; }

        public MethodInfo MethodInfo { get; }

        public Type[] ParameterTypes { get; }
    }
}