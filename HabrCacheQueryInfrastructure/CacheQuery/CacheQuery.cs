using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CacheQueryMediator.CastleCacheInterceptor;
using HabrCacheQuery.Query;
using static HabrCacheQuery.ServiceCollectionExtensions.DeepEquals;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public class BaseCacheQuery<TIn, TOut> : IQuery<TIn, TOut>
    {
        private readonly ConcurrentDictionary<TIn, TOut> _cache;

        private readonly IQuery<TIn, TOut> _query;


        protected BaseCacheQuery(
            IQuery<TIn, TOut> query,
            IConcurrentDictionaryFactory<TIn, TOut> factory)
        {
            _cache = factory.Create();
            _query = query;
        }

        public TOut Query(TIn input) => _cache
            .GetOrAdd(input, x => _query.Query(input));
    }

    public class QueryCache<TIn, TOut> : BaseCacheQuery<TIn, TOut>
    {
        public QueryCache(IConcurrentDictionaryFactory<TIn, TOut> factory, IQuery<TIn, TOut> query)
            : base(factory, query)
        {
        }
    }

    public class AsyncQueryCache<TIn, TOut> : BaseCacheQuery<TIn, Task<TOut>>, IAsyncQuery<TIn, TOut>
    {
        public AsyncQueryCache(
            IConcurrentDictionaryFactory<TIn, Task<TOut>> factory,
            IAsyncQuery<TIn, TOut> query) : base(factory, query)
        {
        }
    }


    public class EqualityComparerUsingReflection<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => Hash.GetHashCode(obj);
    }

    public class Key
    {
        protected bool Equals(Key other)
        {
            return Field1 == other.Field1 && Field2 == other.Field2;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Key) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Field1 * 397) ^ Field2;
            }
        }

        public int Field1 { get; set; }
        public int Field2 { get; set; }
    }

    public class Value
    {
    }

    public class SimpleCache
    {
        private ConcurrentDictionary<Key, Value> _cache { get; set; }
    }
}