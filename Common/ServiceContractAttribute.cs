using System;

namespace Ntreev.Crema.Services
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceContractAttribute : Attribute
    {
        public ServiceContractAttribute()
        {

        }
    }
}