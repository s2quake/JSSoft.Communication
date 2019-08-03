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

// https://sharplab.io/#v2:C4LglgNgPgAgTARgLACgYAYAEMEBYDcqG2CArISqgHYCGAtgKYDOADjQMYOYBywATgwYA3AHQBhAXRriA9nToBXKmHY1gYGVVQBvVJn3Y4mAJJUmwGlU4AhGkwZ6DulAdfYAzNlwmqQmQGsGAAocLFpGABpMABUATxYGAG0AXUxgeOYomQAjACsGdmAUzBo+AHMmAEpHN0xnWv0ASGAACz4ZAHdMKgYu7hlgYzoWCAZGKmAGABMAUQAPThZ1TSDKigbMAF9UGrcYT2ifP0CAHmiAPhCEMPoGKLiE4vSEpiy8gqLU0orqlwb6hrNNqdbq9HgDIYjMYMCbTeaLZZUVbrBrbSh/Wr7bAANiOAQYAEEmLErFcbpFMGw+PQmJgcvlCsVvlVdq4AbUge0uj0+hDhqNxpNZgsGEsNEi1qyDGipfosTBsWdznjAkSSewlWTurcolSaXT3oyvuUWRi3Oy3JyQTzwYN+dDYcKEeLkbKtrs0a5dvAfOZLDY7AxIRBMCBfRYrAxbPZdhbXPLvKZjsFQiVyggomAJmmynBfhs4wZGkJSmkMrSALygro4EQPJKpbRlhIyABmZMqUWeDDbQSzwEqWxRG30Jb4Ocr1YNDM+dRzGZzRk2w8t2UDIiT+KC4R77c3gU7zcyE8lZtcnoabqx/ZVDAA4jCGHwVFrvpns98EPn/m7XGOj5ONq1vWxRNt2vYAMr8FmZSHuB7b9oOy6/gY/7MpgVY2vSHygTmUSfkObqNDAADsmBrvYG6+PiJz9pcO69vu96Ps+7BweW+EmqeGwXrUV6eAqt5qqSqbfN+tSFk0pHkeuTHCew263Ix1GqsSVjsS27ZQc+VCwZxsEruebr8TitETMqTEPj0rHya+5TieaRHSRRDBUcm8lmcA9FKXuKnMdZKjyRpu5BNpMGHmJhnSh6qBokAA==
namespace Ntreev.Crema.Communication
{
    public class InstanceBase
    {
        internal InstanceBase()
        {

        }

        internal IAdaptorHost AdaptorHost { get; set; }

        internal IServiceHost Service { get; set; }

        internal string ServiceName => this.Service.Name;

        internal IPeer Peer { get; set; }

        protected void Invoke(string name, Type[] types, object[] args)
        {
            this.AdaptorHost.Invoke(this, name, types, args);
        }

        protected T Invoke<T>(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.Invoke<T>(this, name, types, args);
        }

        protected Task InvokeAsync(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.InvokeAsync(this, name, types, args);
        }

        protected Task<T> InvokeAsync<T>(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.InvokeAsync<T>(this, name, types, args);
        }
    }
}
