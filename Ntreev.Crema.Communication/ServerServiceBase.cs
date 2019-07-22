using System;

namespace Ntreev.Crema.Communication
{
    public abstract class ServerServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ServerServiceBase()
            : base(typeof(T), typeof(U))
        {

        }

        protected new U Callback => (U)base.Callback;
    }
}