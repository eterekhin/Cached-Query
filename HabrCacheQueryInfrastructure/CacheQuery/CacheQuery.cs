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
        protected virtual ConcurrentDictionary<TIn, TOut> Cache { get; }

        private readonly IQuery<TIn, TOut> _query;


        protected BaseCacheQuery(IConcurrentDictionaryFactory<TIn, TOut> factory, IQuery<TIn, TOut> query)
        {
            Cache = factory.Create();
            _query = query;
        }

        public TOut Query(TIn input) => Cache.GetOrAdd(input, x => _query.Query(input));
    }

    public class QueryCache<TIn, TOut> : BaseCacheQuery<TIn, TOut>
    {
        public QueryCache(IConcurrentDictionaryFactory<TIn, TOut> factory, IQuery<TIn, TOut> query)
            : base(factory, query)
        {
        }
    }


    public class AsyncQueryCache<TIn, TOut> : BaseCacheQuery<TIn, TOut>
    {
        public AsyncQueryCache(
            IConcurrentDictionaryFactory<TIn, TOut> factory,
            IQuery<TIn, TOut> query) : base(factory, query)
        {
        }
    }


    public class EqualityComparerUsingReflection<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => Hash.GetHashCode(obj);
    }
}