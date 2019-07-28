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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;
using Ntreev.Library.Commands;

namespace Server
{
    [Export(typeof(IShell))]
    [Export(typeof(IServiceProvider))]
    [Export(typeof(Shell))]
    class Shell : CommandContextTerminal, IShell, IServiceProvider
    {
        private readonly IServiceHost serviceHost;
        private bool isDisposed;

        [ImportingConstructor]
        public Shell(CommandContext commandContext, IServiceHost serviceHost)
           : base(commandContext)
        {
            this.Prompt = "server";
            this.Postfix = ">";
            this.serviceHost = serviceHost;
        }

        public static IShell Create()
        {
            return Container.GetService<IShell>();
        }

        public void Dispose()
        {
            if (this.isDisposed == false)
            {
                Container.Release();
                this.isDisposed = true;
            }
        }

        internal Guid Token { get; set; }

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            return Container.GetService(serviceType);
        }

        #endregion

        #region IShell

        async Task IShell.StartAsync(Settings settings)
        {
            Console.Title = $"Server localhost:{settings.Port}";
            this.serviceHost.Port = settings.Port;
            this.Token = await this.serviceHost.OpenAsync();
            base.Start();

        }

        async Task IShell.StopAsync()
        {
            base.Cancel();
            if (this.serviceHost.IsOpened == true)
                await this.serviceHost.CloseAsync(this.Token);
        }

        #endregion
    }
}