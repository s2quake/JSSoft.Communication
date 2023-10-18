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
    [Export]
    sealed class Terminal : TerminalBase
    {
        private static readonly string postfix = CommandSettings.IsWin32NT == true ? ">" : "$ ";
        // private readonly Settings settings;
        // private readonly CommandContext commandContext;
        // private readonly IServiceContext serviceHost;
        // private readonly INotifyUserService userServiceNotification;
        // private bool isDisposed;
        // private CancellationTokenSource cancellation;
        private readonly Application _application;

        // static Shell()
        // {
        //     JSSoft.Communication.Logging.LogUtility.Logger = JSSoft.Communication.Logging.ConsoleLogger.Default;
        // }

// #if SERVER
//         private readonly bool isServer = true;
// #else
//         private readonly bool isServer = false;
// #endif

        [ImportingConstructor]
        public Terminal(Application application, CommandContext commandContext)
           : base(commandContext)
        {
            _application = application;
        }

        // public static IShell Create()
        // {
        //     return Container.GetService<IShell>();
        // }

        protected override string FormatPrompt(string prompt)
        {
            if (_application.UserID == string.Empty)
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
    }
}