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
#if MEF
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
#else // MEF
using Ntreev.Library.Commands;
using JSSoft.Communication.Commands;
using JSSoft.Communication.Services;
#endif // MEF

namespace JSSoft.Communication.ConsoleApp
{
    static class Container
    {
#if MEF
        private static CompositionContainer container;

        static Container()
        {
            var entryPath = Assembly.GetEntryAssembly().Location;
            var path = Path.GetDirectoryName(entryPath);
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(path));
            if (Path.GetExtension(entryPath) == ".exe")
            {
                catalog.Catalogs.Add(new AssemblyCatalog(typeof(Container).Assembly));
            }
            container = new CompositionContainer(catalog);
        }

        public static T GetService<T>()
        {
            return container.GetExportedValue<T>();
        }

        public static object GetService(Type serviceType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(serviceType) && serviceType.GenericTypeArguments.Length == 1)
            {
                var itemType = serviceType.GenericTypeArguments.First();
                var contractName = AttributedModelServices.GetContractName(itemType);
                var items = container.GetExportedValues<object>(contractName);
                var listGenericType = typeof(List<>);
                var list = listGenericType.MakeGenericType(itemType);
                var ci = list.GetConstructor(new Type[] { typeof(int) });
                var instance = ci.Invoke(new object[] { items.Count(), }) as IList;
                foreach (var item in items)
                {
                    instance.Add(item);
                }
                return instance;
            }
            else
            {
                var contractName = AttributedModelServices.GetContractName(serviceType);
                return container.GetExportedValue<object>(contractName);
            }
        }

        public static void Release()
        {
            container.Dispose();
        }
#else // MEF
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
            var dataService = new DataService();
            var dataServiceHost = new DataServiceHost(dataService);
            var lazyDataService = new Lazy<IDataService>(() => dataService);
            serviceHosts = new IServiceHost[] { userServiceHost, dataServiceHost };
#if SERVER
            serviceContext = new ServerContext(serviceHosts);
#else
            serviceContext = new ClientContext(serviceHosts);
#endif

            commandList.Add(new CloseCommand(serviceContext, lazyShell));
            commandList.Add(new ExitCommand(lazyIShell));
            commandList.Add(new LoginCommand(lazyShell, lazyUserService));
            commandList.Add(new LogoutCommand(lazyShell, lazyUserService));
            commandList.Add(new OpenCommand(serviceContext, lazyShell));
            commandList.Add(new UserCommand(lazyShell, lazyUserService));
            commandList.Add(new DataCommand(lazyShell, lazyDataService));
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
#endif // MEF
    }
}
