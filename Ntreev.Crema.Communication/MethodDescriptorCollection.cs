using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Ntreev.Library.ObjectModel;

namespace Ntreev.Crema.Communication
{
    public sealed class MethodDescriptorCollection : ContainerBase<MethodDescriptor>
    {
        public MethodDescriptorCollection(ServiceHostBase serviceHost)
        {
            var isServer = ServiceHostBase.IsServer(serviceHost);
            var instanceType = isServer ? serviceHost.ServiceType : serviceHost.CallbackType;
            var methods = instanceType.GetMethods();
            foreach (var item in methods)
            {
                if (item.GetCustomAttribute(typeof(OperationContractAttribute)) is OperationContractAttribute attr)
                {
                    var methodName = attr.Name ?? item.Name;
                    var methodDescriptor = new MethodDescriptor(item);
                    this.AddBase(methodDescriptor.Name, methodDescriptor);
                }
            }
        }
    }
}