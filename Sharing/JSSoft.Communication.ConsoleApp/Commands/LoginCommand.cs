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

using JSSoft.Communication.ConsoleApp;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using JSSoft.Library.Commands;
using JSSoft.Communication.Services;
using System.Threading;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Commands;

[Export(typeof(ICommand))]
class LoginCommand : CommandAsyncBase
{
    private readonly Application _application;
    private readonly Lazy<IUserService> _userService = null;

    [ImportingConstructor]
    public LoginCommand(Application application, Lazy<IUserService> userService)
    {
        _application = application;
        this._userService = userService;
    }

    [CommandPropertyRequired]
    public string UserID
    {
        get; set;
    }

    [CommandPropertyRequired]
    public string Password
    {
        get; set;
    }

    public override bool IsEnabled => _application.UserToken == Guid.Empty;

    protected override async Task OnExecuteAsync(CancellationToken cancellationToken)
    {
        var token = await this.UserService.LoginAsync(this.UserID, this.Password);
        _application.Login(this.UserID, token);
    }

    private IUserService UserService => this._userService.Value;
}