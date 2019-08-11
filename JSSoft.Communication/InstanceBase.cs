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
using System.Threading.Tasks;

// https://sharplab.io/#v2:C4LglgNgPgAgTARgLACgYAYAEMEBYDcqG2CArISqgHYCGAtgKYDOADjQMYOYBywATgwYA3AHQBhAXRriA9nToBXKmHY1gYGVVQBvVJn3Y4mAJJUmwGlU4AhGkwZ6DulAdfYAzNlwmqQmQGsGAAocLFpGABpMABUATxYGAG0AXUxgeOYomQAjACsGdmAUzBo+AHMmAEpHN0xnWv0ASGAACz4ZAHdMKgYu7hlgYzoWCAZGKmAGABMAUQAPThZ1TSDKigbMAF9UGrcYT2ifP0CAHmiAPhCEMPoGKLiE4vSEpiy8gqLU0orqlwb6hrNNqdbq9HgDIYjMYMCbTeaLZZUVbrBrbSh/Wr7bAANiOAQYAEEmLErFcbpFMGw+PQmJgcvlCsVvlVdq4AbUge0uj0+hDhqNxpNZgsGEsNEi1qyDGipfosTBsWdznjAkSSewlWTurcolSaXT3oyvuUWRi3Oy3JyQTzwYN+dDYcKEeLkbKtrs0a5dvAfOZLDY7AxIRBMCBfRYrAxbPZdhbXPLvKZjsFQiVyggomAJmmynBfhs4wZGkJSmkMrSALygro4EQPJKpbRlhIyABmZMqUWeDDbQSzwEqWxRG30Jb4Ocr1YNDM+dRzGZzRk2w8t2UDIiT+KC4R77c3gU7zcyE8lZtcnoabqx/ZVDAA4jCGHwVFrvgub98827C7Ux0fJzatb1sUTbdr2ADK/BZmUh5ge2/aDsubquH+zKYFWNr0h8IHzlEn5Dm6jQwAA7Jga72Buvj4ic/aXDuvb7vej7PuwsHlnhJqnhsF61FengKreaqkqmb6Ztmn5RNkMgyCG3zuPm/yEX+3YAWCQEZDhcFBJBz5UDBXYZL2CEGS27ZSTJiErhsqEmuhU5YUac6iYuHFlJ4SFnkWJFkeujFCew263AxVGqsSVhsS8rlVFZ0punxOI0RMyqMQ+PQsf5r7pmJwCLgptQ/pgxalipdmAQgdYaY2R4QVBekRbufYTJZyEGDZFSlWCDmzk2zn4R5GxEaR5EMJRyb+YlwB0UFe4hUxaUqP59WvCeMX6Dx7ooGiQA
namespace JSSoft.Communication
{
    public class InstanceBase
    {
        public const string InvokeMethod = "Invoke";
        public const string InvokeGenericMethod = "InvokeGeneric";
        public const string InvokeAsyncMethod = "InvokeAsync";
        public const string InvokeGenericAsyncMethod = "InvokeGenericAsync";

        internal InstanceBase()
        {

        }

        internal IAdaptorHost AdaptorHost { get; set; }

        internal IServiceHost ServiceHost { get; set; }

        internal string ServiceName => this.ServiceHost.Name;

        internal IPeer Peer { get; set; }

         [InstanceMethod(InvokeMethod)]
        protected void Invoke(string name, Type[] types, object[] args)
        {
            this.AdaptorHost.Invoke(this, name, types, args);
        }

        [InstanceMethod(InvokeGenericMethod)]
        protected T Invoke<T>(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.Invoke<T>(this, name, types, args);
        }

        [InstanceMethod(InvokeAsyncMethod)]
        protected Task InvokeAsync(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.InvokeAsync(this, name, types, args);
        }

        [InstanceMethod(InvokeGenericAsyncMethod)]
        protected Task<T> InvokeAsync<T>(string name, Type[] types, object[] args)
        {
            return this.AdaptorHost.InvokeAsync<T>(this, name, types, args);
        }
    }
}
