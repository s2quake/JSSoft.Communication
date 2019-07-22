using System;

namespace Ntreev.Crema.Communication
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceContractAttribute : Attribute
    {
        public ServiceContractAttribute()
        {

        }
    }
}