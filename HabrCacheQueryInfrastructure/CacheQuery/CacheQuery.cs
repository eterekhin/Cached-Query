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
        public QueryCache(IQuery<TIn, TOut> query,
            IConcurrentDictionaryFactory<TIn, TOut> factory)
            : base(query, factory)
        {
        }
    }

    public class AsyncQueryCache<TIn, TOut> : BaseCacheQuery<TIn, Task<TOut>>, IAsyncQuery<TIn, TOut>
    {
        public AsyncQueryCache(
            IAsyncQuery<TIn, TOut> query,
            IConcurrentDictionaryFactory<TIn, Task<TOut>> factory)
            : base(query, factory)
        {
        }
    }


    public class EqualityComparerUsingReflection<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => Hash.GetHashCode(obj);
    }
}