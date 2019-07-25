using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

// https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugCwDcO+R6ArBdjgHYCGAtgKYDOADswMbsCAOWAAnduwBuAOgDC41szkB7VqwCujMH2bAwyxjgDeOAmaKoCsg8HYAPYACFmndqfMns570QDMREgIASUZJZQBrdgAKYkIWDigCXlE2TgJlACMAK3Y+YABtAF0CZlEAc04ASncfAk9aswBIYAALUWUAdwJGdi6hZWAg1m4Idg5GWxgAUTsBbj0DKMr6BoIAXxwan2R/ABVg0Ij2AB5dgD4Y9Di2dkTk1PTs3ILi0orqrwb6hua2zu7esIBkMRmN2BN2NNZux5vpGEsVg0NgxPrUdkQAGwHMKRACCnAAnow+JdrgkkqUHpkcnkiiVylUtt5vrVfu0uj0+sDhqNxpMZnMFvDlkzzMjRWZ0cgMacztijviiXxZaTujc7pTWGlqc86W9GaifCyfGz/pygYMeWCIVDBXCERL1ltkd4tmgrDZ7E4XOwQRACCAPRCHM5XFtjd4pYEQjjorF6WUPqsI+ZGhkfdIY0covF2MoAGZRLORSqJYAE7h5wsAZTEYEYicSbxFhu8LoajvR9eA8siAHFwexRNpVc3HSmmsgAOwEdOuTOHSLHbsXXMFouL9gDnrDvilgjlyvr2vDhv75uI2rt2qd/zS3vsRXE0flJNfR2NaezjPFx+E59roWv5PnuZYVlWUQnvWjYJi2qzXj4t6YsuExyr+25DtoIEvom44fl+c7sAusYgShwCrjc67oYOu4gfuh4QVBZ5Nq+l4+AhyLIkAA=
namespace Ntreev.Crema.Communication
{
    class ContextBase : IDisposable
    {
        public ContextBase()
        {

        }

        public IContextInvoker Invoker { get; set; }

        public IService Service { get; set; }

        public void Dispose()
        {

        }

        protected void Invoke(string name, params object[] args)
        {
            this.Invoker.Invoke(this.Service, name, args);
        }

        protected T Invoke<T>(string name, params object[] args)
        {
            return this.Invoker.Invoke<T>(this.Service, name, args);
        }

        protected Task InvokeAsync(string name, params object[] args)
        {
            return this.Invoker.InvokeAsync(this.Service, name, args);
        }

        protected Task<T> InvokeAsync<T>(string name, params object[] args)
        {
            return this.Invoker.InvokeAsync<T>(this.Service, name, args);
        }
    }
}
