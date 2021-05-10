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
using JSSoft.Library.Commands;
using JSSoft.Communication.Commands;
using JSSoft.Communication.Services;
using JSSoft.Communication.ConsoleApp;
using JSSoft.Communication;

namespace System.ComponentModel.Composition
{
    static class Container
    {
        private static readonly List<ICommand> commandList = new();
        private static readonly Shell shell;
        private static readonly CommandContext commandContext;
        private static readonly List<object> instanceList = new();
#if SERVER
        private static readonly ServerContext serviceContext;
#else
        private static readonly ClientContext serviceContext;
#endif
        private static readonly IServiceHost[] serviceHosts;

        static Container()
        {
            var lazyIShell = new Lazy<IShell>(GetShell);
            var lazyShell = new Lazy<Shell>(GetShell);
            var userService = new UserService();
            var userServiceHost = new UserServiceHost(userService);
            var lazyUserService = new Lazy<IUserService>(() => userService);
            var dataService = new DataService();
            var dataServiceHost = new DataServiceHost(dataService);
            var lazyDataService = new Lazy<IDataService>(() => dataService);

            instanceList.Add(userService);
            instanceList.Add(dataService);
            instanceList.Add(userServiceHost);
            instanceList.Add(dataServiceHost);

            serviceHosts = new IServiceHost[] { userServiceHost, dataServiceHost };
#if SERVER
            serviceContext = new ServerContext(serviceHosts);
            instanceList.Add(serviceHosts);
#else
            serviceContext = new ClientContext(serviceHosts);
            instanceList.Add(serviceHosts);
#endif

            commandList.Add(new CloseCommand(serviceContext, lazyShell));
            commandList.Add(new ExitCommand(lazyIShell));
            commandList.Add(new LoginCommand(lazyShell, lazyUserService));
            commandList.Add(new LogoutCommand(lazyShell, lazyUserService));
            commandList.Add(new OpenCommand(serviceContext, lazyShell));
            commandList.Add(new UserCommand(lazyShell, lazyUserService));
            commandList.Add(new DataCommand(lazyShell, lazyDataService));
            commandContext = new CommandContext(commandList);
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
            for (var i = instanceList.Count - 1; i >= 0; i--)
            {
                var item = instanceList[i];
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private static Shell GetShell()
        {
            return shell;
        }
    }
}
