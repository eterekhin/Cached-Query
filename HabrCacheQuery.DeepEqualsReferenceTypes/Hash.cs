using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class Hash
    {
        public static int GetHashCode(object obj) => MatchAndAction(obj);

        private static int PrimitiveHashCode(object obj) => obj.GetHashCode();

        private static int ClassHashCode(object obj) =>
            DeepEquals.GetPropFieldValue(obj).Aggregate(0, (a, c) => a ^ GetHashCode(c.value) * 357);

        private static int IEnumerableGetHashCode(object obj)
        {
            var enumerator = (obj as IEnumerable).GetEnumerator();
            var hash = 0;
            while (enumerator.MoveNext())
                hash ^= GetHashCode(enumerator.Current);

            return hash;
        }

        private static int KeyValuePairGetHashCode(object obj)
        {
            dynamic keyValuePair = obj;
            return GetHashCode(keyValuePair.Key) ^ GetHashCode(keyValuePair.Value);
        }

        private static int MatchAndAction(object obj)
        {
            IEnumerable<(Func<Type, bool>, Func<object, int>)> matches()
            {
                yield return (TypeCheckers.isPrimitive, PrimitiveHashCode);
                yield return (TypeCheckers.isEnumerable, IEnumerableGetHashCode);
                yield return (TypeCheckers.isClass, ClassHashCode);
                yield return (TypeCheckers.isKeyValue, KeyValuePairGetHashCode);
            }

            return matches().First(x => x.Item1.Invoke(obj.GetType())).Item2.Invoke(obj);
        }
    }
}