using System;

namespace Ntreev.Crema.Services
{
    class UserInfo
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public Guid Token { get; set; }
    }
}