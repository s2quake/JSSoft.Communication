using System;

namespace Ntreev.Crema.Communication
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ServiceContractAttribute : Attribute
    {
        public ServiceContractAttribute()
        {

        }

        public ServiceContractAttribute(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            Ntreev.Library.IdentifierValidator.Validate(this.Name);
        }

        public string Name { get; }
    }
}
