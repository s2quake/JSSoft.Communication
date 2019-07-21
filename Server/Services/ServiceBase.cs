using System;

namespace Ntreev.Crema.Services
{
    abstract class ServiceBase
    {
        private readonly Type callbackType;
        protected ServiceBase(Type callbackType)
        {
        }
    }
}