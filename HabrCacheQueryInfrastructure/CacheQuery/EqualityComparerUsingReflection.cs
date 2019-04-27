using System.Collections.Generic;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public class EqualityComparerUsingReflection<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEquals.DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => DeepHash.DeepGetHashCode(obj);
    }
}