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

using JSSoft.Communication.Services;
using JSSoft.Library.Commands;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace JSSoft.Communication.ConsoleApp
{
    // [Export(typeof(IApplication))]
    // [Export(typeof(IServiceProvider))]
    // [Export(typeof(Application))]
    sealed class Application : IApplication, IServiceProvider
    {
        private static readonly string postfix = CommandSettings.IsWin32NT == true ? ">" : "$ ";
        private readonly Settings settings;
        // private readonly CommandContext commandContext;
        private readonly IServiceContext serviceHost;
        private readonly INotifyUserService userServiceNotification;
        private bool isDisposed;
        private CancellationTokenSource cancellation;
        private readonly CompositionContainer _container;

        static Application()
        {
            JSSoft.Communication.Logging.LogUtility.Logger = JSSoft.Communication.Logging.ConsoleLogger.Default;
        }

#if SERVER
        private readonly bool isServer = true;
#else
        private readonly bool isServer = false;
#endif

        // private readonly Lazy<Terminal> _terminalLazy;

        public Application()
        {
            _container = new CompositionContainer(new AssemblyCatalog(typeof(Application).Assembly));
            _container.ComposeExportedValue<IApplication>(this);
            _container.ComposeExportedValue<IServiceProvider>(this);
            _container.ComposeExportedValue(this);

            // _terminalLazy = terminalLazy;
            this.settings = Settings.CreateFromCommandLine();
            // this.Prompt = postfix;
            this.serviceHost = serviceHost;
            this.serviceHost.Opened += ServiceHost_Opened;
            this.serviceHost.Closed += ServiceHost_Closed;
            this.userServiceNotification = userServiceNotification;
            this.userServiceNotification.LoggedIn += UserServiceNotification_LoggedIn;
            this.userServiceNotification.LoggedOut += UserServiceNotification_LoggedOut;
            this.userServiceNotification.MessageReceived += UserServiceNotification_MessageReceived;
            // this.commandContext = commandContext;
            this.Title = "Server";
        }

        // public static IApplication Create()
        // {
        //     return Container.GetService<IApplication>();
        // }

        public void Dispose()
        {
            if (this.isDisposed == false)
            {
                _container.Dispose();
                this.isDisposed = true;
            }
        }

        public bool IsOpened { get; private set; }

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        internal void Login(string userID, Guid token)
        {
            this.UserID = userID;
            this.UserToken = token;
            this.UpdatePrompt();
            this.Out.WriteLine("사용자 관련 명령을 수행하려면 'help user' 을(를) 입력하세요.");
        }

        internal void Logout()
        {
            this.UserID = string.Empty;
            this.UserToken = Guid.Empty;
            this.UpdatePrompt();
        }

        internal Guid Token { get; set; }

        internal Guid UserToken { get; private set; }

        internal string UserID { get; private set; } = string.Empty;

        private TextWriter Out => Console.Out;

        private Terminal Terminal => _container.GetExportedValue<Terminal>();

        private void UpdatePrompt()
        {
            var prompt = string.Empty;
            var isOpened = this.IsOpened;
            var host = this.serviceHost.Host;
            var port = this.serviceHost.Port;
            var userID = this.UserID;

            if (isOpened == true)
            {
                prompt = $"{host}:{port}";
                if (userID != string.Empty)
                    prompt += $"@{userID}";
            }
            // Terminal.Prompt = prompt + postfix;
        }

        private void ServiceHost_Opened(object sender, EventArgs e)
        {
            this.IsOpened = true;
            this.UpdatePrompt();

            if (this.isServer)
            {
                this.Title = $"Server {this.serviceHost.Host}:{this.serviceHost.Port}";
                this.Out.WriteLine("서버가 시작되었습니다.");
            }
            else
            {
                this.Title = $"Client {this.serviceHost.Host}:{this.serviceHost.Port}";
                this.Out.WriteLine("서버에 연결되었습니다.");
            }
            this.Out.WriteLine("사용 가능한 명령을 확인려면 'help' 을(를) 입력하세요.");
            this.Out.WriteLine("로그인을 하려면 'login admin admin' 을(를) 입력하세요.");
        }

        private void ServiceHost_Closed(object sender, EventArgs e)
        {
            this.IsOpened = false;
            this.UpdatePrompt();
            if (this.isServer)
            {
                this.Title = $"Server - Closed";
                this.Out.WriteLine("서버가 중단되었습니다.");
                this.Out.WriteLine("서버를 시작하려면 'open' 을(를) 입력하세요.");
            }
            else
            {
                this.Title = $"Client - Disconnected";
                this.Out.WriteLine("서버와 연결이 끊어졌습니다.");
                this.Out.WriteLine("서버에 연결하려면 'open' 을(를) 입력하세요.");
            }
        }

        private void UserServiceNotification_LoggedIn(object sender, UserEventArgs e)
        {
            this.Out.WriteLine($"User logged in: {e.UserID}");
        }

        private void UserServiceNotification_LoggedOut(object sender, UserEventArgs e)
        {
            this.Out.WriteLine($"User logged out: {e.UserID}");
        }

        private void UserServiceNotification_MessageReceived(object sender, UserMessageEventArgs e)
        {
            if (e.Sender == this.UserID)
            {
                var text = TerminalStrings.Foreground($"'{e.Receiver}'에게 귓속말: {e.Message}", TerminalColor.BrightMagenta);
                this.Out.WriteLine(text);
            }
            else if (e.Receiver == this.UserID)
            {
                var text = TerminalStrings.Foreground($"'{e.Receiver}'의 귓속말: {e.Message}", TerminalColor.BrightMagenta);
                this.Out.WriteLine(text);
            }
        }

        #region IServiceProvider

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
                return this;

            if (typeof(IEnumerable).IsAssignableFrom(serviceType) && serviceType.GenericTypeArguments.Length == 1)
            {
                var itemType = serviceType.GenericTypeArguments.First();
                var contractName = AttributedModelServices.GetContractName(itemType);
                var items = _container.GetExportedValues<object>(contractName);
                var listGenericType = typeof(List<>);
                var list = listGenericType.MakeGenericType(itemType);
                var ci = list.GetConstructor(new Type[] { typeof(int) });
                var instance = ci.Invoke(new object[] { items.Count(), }) as IList;
                foreach (var item in items)
                {
                    instance.Add(item);
                }
                return instance;
            }
            else
            {
                var contractName = AttributedModelServices.GetContractName(serviceType);
                return _container.GetExportedValue<object>(contractName);
            }
            // return Container.GetService(serviceType);
        }

        #endregion

        #region IApplication

        public async Task StartAsync()
        {
            this.cancellation = new CancellationTokenSource();
            this.serviceHost.Host = this.settings.Host;
            this.serviceHost.Port = this.settings.Port;
            try
            {
                this.Token = await this.serviceHost.OpenAsync();
            }
            catch (Exception e)
            {
                var text = TerminalStrings.Foreground(e.Message, TerminalColor.BrightRed);
                Console.Error.WriteLine(text);
            }
            await Terminal.StartAsync(this.cancellation.Token);
        }

        public async Task StopAsync(int exitCode)
        {
            this.cancellation.Cancel();
            if (this.serviceHost.ServiceState == ServiceState.Open)
            {
                this.serviceHost.Closed -= ServiceHost_Closed;
                await this.serviceHost.CloseAsync(this.Token, exitCode);
            }
        }

        #endregion
    }
}