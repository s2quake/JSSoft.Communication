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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JSSoft.Communication.Shell.Commands;
using JSSoft.Communication.Shell.Services;
using Ntreev.Library.Commands;

namespace JSSoft.Communication.Shell
{
    static class Container
    {
        private static readonly List<ICommand> commandList = new List<ICommand>();
        private static Shell shell;
        private static CommandContext commandContext;
#if SERVER
        private static ServerContext serviceContext;
#else
        private static ClientContext serviceContext;
#endif
        private static IServiceHost[] serviceHosts;

        static Container()
        {
            var lazyIShell = new Lazy<IShell>(GetShell);
            var lazyShell = new Lazy<Shell>(GetShell);
            var userService = new UserService();
            var userServiceHost = new UserServiceHost(userService);
            var lazyUserService = new Lazy<IUserService>(() => userService);
#if SERVER
            var dataServiceHost = new DataServiceHost();
            serviceHosts = new IServiceHost[] { userServiceHost, dataServiceHost };
            serviceContext = new ServerContext(serviceHosts);
#else
            serviceHosts = new IServiceHost[] { userServiceHost };
            serviceContext = new ClientContext(serviceHosts);
#endif

            commandList.Add(new CloseCommand(serviceContext, lazyShell));
            commandList.Add(new ExitCommand(lazyIShell));
            commandList.Add(new LoginCommand(lazyShell, lazyUserService));
            commandList.Add(new OpenCommand(serviceContext, lazyShell));
            commandList.Add(new UserCommand(lazyShell, lazyUserService));
            commandContext = new CommandContext(commandList, Enumerable.Empty<ICommandProvider>());
            shell = new Shell(commandContext, serviceContext, userService);
        }

        public static T GetService<T>() where T : class
        {
            if (typeof(T) == typeof(IShell))
            {
                return shell as T;
            }
            throw new NotImplementedException();
        }

        public static object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public static void Release()
        {
            if (serviceContext is IDisposable s)
            {
                s.Dispose();
            }

            foreach (var item in serviceHosts.Reverse())
            {
                item.Dispose();
            }
        }

        private static Shell GetShell() => shell;
    }
}
