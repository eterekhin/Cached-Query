using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class DeepHash
    {
        public static int DeepGetHashCode(object obj) => MatchAndAction(obj);

        private static int PrimitiveHashCode(object obj) => obj.GetHashCode();

        private static int ClassHashCode(object obj) =>
            DeepEquals.GetPropFieldValue(obj).Aggregate(0, (a, c) => a ^ DeepGetHashCode(c.value) * 357);

        private static int IEnumerableGetHashCode(object obj)
        {
            var enumerator = (obj as IEnumerable).GetEnumerator();
            var hash = 0;
            while (enumerator.MoveNext())
                hash ^= DeepGetHashCode(enumerator.Current);

            return hash;
        }

        private static int KeyValuePairGetHashCode(object obj)
        {
            dynamic keyValuePair = obj;
            return DeepGetHashCode(keyValuePair.Key) ^ DeepGetHashCode(keyValuePair.Value);
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