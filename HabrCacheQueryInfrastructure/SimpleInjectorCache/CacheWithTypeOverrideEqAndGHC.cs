using System.Collections.Concurrent;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace CacheQueryMediator.SimpleInjectorCache
{
    public class CacheWithTypeOverrideEqAndGHC<TIn, TOut> : BaseCache<TIn, TOut>
    {
        public CacheWithTypeOverrideEqAndGHC(IQuery<TIn, TOut> query) : base(query,
            new ConcurrentDictionary<TIn, TOut>())
        {
        }
    }

    public class DefaultCacheQuery<TIn, TOut> : BaseCache<TIn, TOut>
    {
        public DefaultCacheQuery(IQuery<TIn, TOut> query) : base(query,
            new ConcurrentDictionary<TIn, TOut>(new EqualityComparerUsingReflection<TIn>()))
        {
        }
    }

    public abstract class BaseCache<TIn, TOut> : IQuery<TIn, TOut>
    {
        private readonly IQuery<TIn, TOut> _query;
        private readonly ConcurrentDictionary<TIn, TOut> _concurrentDictionary;

        protected BaseCache(IQuery<TIn, TOut> query, ConcurrentDictionary<TIn, TOut> concurrentDictionary)
        {
            _query = query;
            _concurrentDictionary = concurrentDictionary;
        }

        public TOut Query(TIn input) => _concurrentDictionary.GetOrAdd(input, x => _query.Query(input));
    }
}