// MIT License
// 
// Copyright (c) 2019 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ntreev.Library.Threading;

// https://sharplab.io/#v2:C4LglgNgNAJiDUAfAAgJgIwFgBQyAMABMugCwDcO+R6ArBdjgHYCGAtgKYDOADswMbsCAOWAAnduwBuAOgDC41szkB7VqwCujMH2bAwyxjgDeOAmaKoCsg8HYAPYACFmndqfMns570QDMREgIASUZJZQBrdgAKYkIWDigCXlE2TgJlACMAK3Y+YABtAF0CZlEAc04ASncfAk9aswBIYAALUWUAdwJGdi6hZWAg1m4Idg5GWxgAUTsBbj0DKMr6BoIAXxwan2R/ABVg0Ij2AB5dgD4Y9Di2dkTk1PTs3ILi0orqrwb6hua2zu7esIBkMRmN2BN2NNZux5vpGEsVg0NgxPrUdkQAGwHMKRACCnAAnow+JdrgkkqUHpkcnkiiVylUtt5vrVfu0uj0+sDhqNxpMZnMFvDlkzzMjRWZ0cgMacztijviiXxZaTujc7pTWGlqc86W9GaifCyfGz/pygYMeWCIVDBXCERL1ltkd4tmgrDZ7E4XOwQRACCAPRCHM5XFtjd4pYEQjjorF6WUPqsI+ZGhkfdIY0covF2MoAGZRLORSqJYAE7h5wsAZTEYEYicSbxFhu8LoajvR9eA8siAHFwexRNpVc3HSmmsgAOwEdOuTOHSLHbsXXMFouL9gDnrDvilgjlyvr2vDhv75uI2rt2qd/zS3vsRXE0flJNfR2NaezjPFx+E59roWv5PnuZYVlWUQnvWjYJi2qzXj4t6YsuExyr+25DtoIEvom44fl+c7sAusYgShwCrjc67oYOu4gfuh4QVBZ5Nq+l4+AhyLIkAA=
namespace Ntreev.Crema.Communication
{
    class InstanceBase : IDisposable
    {
        public InstanceBase()
        {

        }

        public void Dispose()
        {

        }

        public IContextInvoker Invoker { get; set; }

        public IService Service { get; set; }

        public string ServiceName => this.Service.Name;

        public string Peer { get; set; }

        protected void Invoke(string name, params object[] args)
        {
            this.Invoker.Invoke(this, name, args);
        }

        protected T Invoke<T>(string name, params object[] args)
        {
            return this.Invoker.Invoke<T>(this, name, args);
        }

        protected Task InvokeAsync(string name, params object[] args)
        {
            return this.Invoker.InvokeAsync(this, name, args);
        }

        protected Task<T> InvokeAsync<T>(string name, params object[] args)
        {
            return this.Invoker.InvokeAsync<T>(this, name, args);
        }
    }
}
