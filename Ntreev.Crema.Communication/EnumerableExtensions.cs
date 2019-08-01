using System;
using System.Collections.Generic;
using System.Linq;

namespace Ntreev.Crema.Communication
{
    static class EnumerableExtensions
    {
        public static void DisposeAll<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            var items = dictionary.Values.ToArray();
            foreach (var item in items.OfType<IDisposable>())
            {
                item.Dispose();
            }
        }
    }
}