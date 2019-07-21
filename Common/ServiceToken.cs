using System;

namespace Ntreev.Crema.Services
{
    public sealed class ServiceToken
    {
        internal ServiceToken(IAdaptor adaptor, CallbackBase callback)
        {
            this.Adaptor = adaptor;
            this.Callback = callback;
        }

        public IAdaptor Adaptor {get;}

        public CallbackBase Callback{get;}
    }
}