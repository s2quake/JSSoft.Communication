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

namespace JSSoft.Communication.ConsoleApp
{
    [Export(typeof(IShell))]
    [Export(typeof(IServiceProvider))]
    [Export(typeof(Shell))]
    class Shell : CommandContextTerminal, IShell, IServiceProvider
    {
        private static readonly string postfix = Terminal.IsWin32NT == true ? ">" : "$ ";
        private readonly Settings settings;
        private readonly CommandContext commandContext;
        private readonly IServiceContext serviceHost;
        private readonly INotifyUserService userServiceNotification;
        private bool isDisposed;
        private CancellationTokenSource cancellation;

        static Shell()
        {
            JSSoft.Communication.Logging.LogUtility.Logger = JSSoft.Communication.Logging.ConsoleLogger.Default;
        }

#if SERVER
        private readonly bool isServer = true;
#else
        private readonly bool isServer = false;
#endif

        [ImportingConstructor]
        public Shell(CommandContext commandContext, IServiceContext serviceHost, INotifyUserService userServiceNotification)
           : base(commandContext)
        {
            this.settings = Settings.CreateFromCommandLine();
            this.Prompt = postfix;
            this.serviceHost = serviceHost;
            this.serviceHost.Opened += ServiceHost_Opened;
            this.serviceHost.Closed += ServiceHost_Closed;
            this.userServiceNotification = userServiceNotification;
            this.userServiceNotification.LoggedIn += UserServiceNotification_LoggedIn;
            this.userServiceNotification.LoggedOut += UserServiceNotification_LoggedOut;
            this.userServiceNotification.MessageReceived += UserServiceNotification_MessageReceived;
            this.commandContext = commandContext;
            this.Title = "Server";
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

        public bool IsOpened { get; private set; }

        public string Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        protected override string FormatPrompt(string prompt)
        {
            if (this.UserID == string.Empty)
            {
                return prompt;
            }
            else
            {
                var tb = new TerminalStringBuilder();
                var pattern = $"(.+@)(.+).{{{postfix.Length}}}";
                var match = Regex.Match(prompt, pattern);
                tb.Append(match.Groups[1].Value);
                tb.Foreground = TerminalColor.BrightGreen;
                tb.Append(match.Groups[2].Value);
                tb.Foreground = null;
                tb.Append(postfix);
                return tb.ToString();
            }
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

        private TextWriter Out => this.commandContext.Out;

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
            this.Prompt = prompt + postfix;
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

            return Container.GetService(serviceType);
        }

        #endregion

        #region IShell

        async Task IShell.StartAsync()
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
            await base.StartAsync(this.cancellation.Token);
        }

        async Task IShell.StopAsync(int exitCode)
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