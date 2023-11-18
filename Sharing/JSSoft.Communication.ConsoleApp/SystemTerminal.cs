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
using JSSoft.Library.Commands.Extensions;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using JSSoft.Library.Terminals;

namespace JSSoft.Communication.ConsoleApp;

[Export]
sealed class SystemTerminal : SystemTerminalBase
{
    private static readonly string postfix = CommandSettings.IsWin32NT == true ? ">" : "$ ";
    private readonly Application _application;
    private readonly CommandContext _commandContext;

    [ImportingConstructor]
    public SystemTerminal(Application application, CommandContext commandContext)
    {
        _application = application;
        _commandContext = commandContext;
    }

    // public static IShell Create()
    // {
    //     return Container.GetService<IShell>();
    // }

    protected override Task OnExecuteAsync(string command, CancellationToken cancellationToken)
    {
        return _commandContext.ExecuteAsync(command, cancellationToken);
    }

    protected override void OnInitialize(TextWriter @out, TextWriter error)
    {
        _commandContext.Out = @out;
        _commandContext.Error = error;
    }

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
            tb.Foreground = TerminalColorType.BrightGreen;
            tb.Append(match.Groups[2].Value);
            tb.Foreground = null;
            tb.Append(postfix);
            return tb.ToString();
        }
    }
}