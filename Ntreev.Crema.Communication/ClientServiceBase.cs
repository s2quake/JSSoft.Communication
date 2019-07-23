using System;

namespace Ntreev.Crema.Communication
{
    public abstract class ClientServiceBase<T, U> : ServiceBase where T : class where U : class
    {
        protected ClientServiceBase()
            : base(typeof(T), typeof(U), typeof(U))
        {

        }

        protected T Service => (T)base.Instance;
    }
}