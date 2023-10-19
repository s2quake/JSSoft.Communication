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
using JSSoft.Communication.Services;
using JSSoft.Library.Commands;
using System;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

namespace JSSoft.Communication.Commands;

[Export(typeof(ICommand))]
class UserCommand : CommandMethodBase
{
    private readonly Application _application;
    private readonly Lazy<IUserService> _userService;

    [ImportingConstructor]
    public UserCommand(Application application, Lazy<IUserService> userService)
    {
        _application = application;
        _userService = userService;
    }

    [CommandMethod]
    public Task CreateAsync(string userID, string password, Authority authority = Authority.Member)
    {
        return UserService.CreateAsync(_application.UserToken, userID, password, authority);
    }

    [CommandMethod]
    public Task DeleteAsync(string userID)
    {
        return UserService.DeleteAsync(_application.UserToken, userID);
    }

    [CommandMethod]
    public Task RenameAsync(string userName)
    {
        return UserService.RenameAsync(_application.UserToken, userName);
    }

    [CommandMethod]
    public Task AuthorityAsync(string userID, Authority authority)
    {
        return UserService.SetAuthorityAsync(_application.UserToken, userID, authority);
    }

    [CommandMethod]
    public async Task InfoAsync(string userID)
    {
        var (userName, authority) = await UserService.GetInfoAsync(_application.UserToken, userID);
        Out.WriteLine($"UseName: {userName}");
        Out.WriteLine($"Authority: {authority}");
    }

    [CommandMethod]
    public async Task ListAsync()
    {
        var items = await UserService.GetUsersAsync(_application.UserToken);
        foreach (var item in items)
        {
            Out.WriteLine(item);
        }
    }

    [CommandMethod]
    public Task SendMessageAsync(string userID, string message)
    {
        return UserService.SendMessageAsync(_application.UserToken, userID, message);
    }

    public override bool IsEnabled => _application.UserToken != Guid.Empty;

    protected override bool IsMethodEnabled(CommandMethodDescriptor descriptor)
    {
        return _application.UserToken != Guid.Empty;
    }

    private IUserService UserService => _userService.Value;
}
