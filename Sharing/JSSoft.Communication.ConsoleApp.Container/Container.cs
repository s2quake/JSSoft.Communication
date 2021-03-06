﻿// MIT License
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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace JSSoft.Communication.ConsoleApp
{
    static class Container
    {
        private static readonly CompositionContainer container;

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
    }
}
