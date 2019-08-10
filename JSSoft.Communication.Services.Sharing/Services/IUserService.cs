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
using JSSoft.Communication;

namespace JSSoft.Communication.Services
{
    [ServiceContract]
    public interface IUserService
    {
        [OperationContract]
        Task CreateAsync(Guid token, string userID, string password, Authority authority);

        [OperationContract]
        Task DeleteAsync(Guid token, string userID);

        [OperationContract]
        Task RenameAsync(Guid token, string userName);

        [OperationContract]
        Task SetAuthorityAsync(Guid token, string userID, Authority authority);

        [OperationContract]
        Task<Guid> LoginAsync(string userID, string password);

        [OperationContract]
        Task LogoutAsync(Guid token);

        [OperationContract]
        Task<(string userName, Authority authority)> GetInfoAsync(Guid token, string userID);

        [OperationContract]
        Task<string[]> GetUsersAsync(Guid token);

        [OperationContract]
        Task<bool> IsOnlineAsync(Guid token, string userID);

        [OperationContract]
        Task SendMessageAsync(Guid token, string userID, string message);
    }
}
