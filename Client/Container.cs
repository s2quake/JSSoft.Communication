using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    static class Container
    {
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
    }
}
