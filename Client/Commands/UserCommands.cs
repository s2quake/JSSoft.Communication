using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Ntreev.Library.Commands;
using Client;
using Ntreev.Crema.Services.Users;
using System.Threading.Tasks;

namespace Ntreev.Crema.Services.Commands
{
    [Export(typeof(ICommand))]
    class UserCommands : CommandMethodBase
    {
        [Import]
        private Lazy<IUserService> userService = null;

        public UserCommands()
            : base("user")
        {
            
        }

        public Task LoginAsync(string userID)
        {
            return this.UserService.LoginAsync(userID);
        }

        private IUserService UserService => this.userService.Value;
    }
}
