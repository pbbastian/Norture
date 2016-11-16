using System;
using System.Collections.Generic;
using System.Linq;

namespace Norture.Extensions
{
    public static class EnumerableExtensions
    {
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> keySelector)
        {
            var enumerator = enumerable.GetEnumerator();
            
            enumerator.MoveNext();
            var minValue = enumerator.Current;
            var minKey = keySelector(enumerator.Current);
            
            while (enumerator.MoveNext())
            {
                var key = keySelector(enumerator.Current);
                if (Comparer<TKey>.Default.Compare(key, minKey) < 0)
                {
                    minValue = enumerator.Current;
                    minKey = key;
                }
            }

            return minValue;
        }
    }
}