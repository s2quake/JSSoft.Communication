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
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ntreev.Crema.Communication;
using Ntreev.Library.Commands;

namespace Ntreev.Crema.Services
{
    [Export(typeof(IShell))]
    [Export(typeof(IServiceProvider))]
    [Export(typeof(Shell))]
    class Shell : CommandContextTerminal, IShell, IServiceProvider, IPartImportsSatisfiedNotification
    {
        private readonly Settings settings;
        private readonly IServiceContext serviceHost;
        private readonly CommandContext commandContext;
        private bool isDisposed;
#if SERVER
        private bool isServer = true;
#else
        private bool isServer = false;
#endif

        [Import]
        private INotifyUserService userServiceNotification = null;

        [ImportingConstructor]
        public Shell(CommandContext commandContext, IServiceContext serviceHost)
           : base(commandContext)
        {
            this.settings = Settings.CreateFromCommandLine();
            this.Prompt = "";
            this.Postfix = ">";
            this.serviceHost = serviceHost;
            this.serviceHost.Opened += ServiceHost_Opened;
            this.serviceHost.Closed += ServiceHost_Closed;
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

        protected override void OnDrawPrompt(TextWriter writer, string prompt)
        {
            if (this.UserID == string.Empty)
            {
                base.OnDrawPrompt(writer, prompt);
            }
            else
            {
                var pattern = $"(.+@)(.+){this.Postfix}";
                var match = Regex.Match(prompt, pattern);
                var p1 = prompt.TrimStart();
                var p2 = prompt.TrimEnd();
                var prefix = prompt.Substring(p1.Length);
                var postfix = prompt.Substring(p2.Length);
                writer.Write(match.Groups[1].Value);
                using (TerminalColor.SetForeground(ConsoleColor.Green))
                {
                    writer.Write(match.Groups[2].Value);
                }
                Console.ResetColor();
                writer.Write(this.Postfix);
            }
        }

        internal void Login(string userID, Guid token)
        {
            this.UserID = userID;
            this.UserToken = token;
            this.UpdatePrompt();
            this.Out.WriteLine("사용자 관련 명령을 수행하려면 'help user' 을(를) 입력하세요.");
            this.Out.WriteLine();
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
            if (this.IsOpened == true)
            {
                this.Prompt = $"{this.serviceHost.Host}:{this.serviceHost.Port}";
                if (this.UserID != string.Empty)
                    this.Prompt += $"@{this.UserID}";
            }
            else
            {
                this.Prompt = string.Empty;
            }
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
            this.Out.WriteLine();
            this.Out.WriteLine("로그인을 하려면 'login admin admin' 을(를) 입력하세요.");
            this.Out.WriteLine();
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
            this.Out.WriteLine();
        }

        private void UserServiceNotification_LoggedIn(object sender, UserEventArgs e)
        {
            this.Out.WriteLine($"User logged in: {e.UserID}");
            this.Out.WriteLine();
        }

        private void UserServiceNotification_LoggedOut(object sender, UserEventArgs e)
        {
            this.Out.WriteLine($"User logged out: {e.UserID}");
            this.Out.WriteLine();
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
            this.serviceHost.Host = this.settings.Host;
            this.serviceHost.Port = this.settings.Port;
            this.Token = await this.serviceHost.OpenAsync();
            base.Start();
        }

        async Task IShell.StopAsync()
        {
            base.Cancel();
            if (this.serviceHost.IsOpened == true)
            {
                this.serviceHost.Closed -= ServiceHost_Closed;
                await this.serviceHost.CloseAsync(this.Token);
            }
        }

        #endregion

        #region IPartImportsSatisfiedNotification

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            this.userServiceNotification.LoggedIn += UserServiceNotification_LoggedIn;
            this.userServiceNotification.LoggedOut += UserServiceNotification_LoggedOut;
        }

        #endregion
    }
}