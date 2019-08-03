using System;
using System.Collections.Generic;
using System.Linq;

namespace Ntreev.Crema.Communication
{
    static class EnumerableExtensions
    {
        public static void DisposeAll<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> predicate = null)
        {
            var items = dictionary.Where(predicate ?? new Func<KeyValuePair<TKey, TValue>, bool>((item) => true))
                                  .Select(item => item.Value)
                                  .ToArray();
            foreach (var item in items.OfType<IDisposable>())
            {
                item.Dispose();
            }
        }
    }
}