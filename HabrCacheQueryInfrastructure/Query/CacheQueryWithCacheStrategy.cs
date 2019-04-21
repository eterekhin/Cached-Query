using System.Collections.Concurrent;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace HabrCacheQueryInfrastructure.Query
{
    public class CacheQueryWithCacheStrategy<TIn, TOut> : IQuery<TIn, TOut>
    {
        private readonly IQuery<TIn, TOut> _query;
        private readonly ConcurrentDictionary<TIn, TOut> Cache;

        public CacheQueryWithCacheStrategy(IQuery<TIn, TOut> query)
        {
            _query = query;
            if (TypeCheckers.EqualsGetHashCodeOverride(typeof(TIn)))
                Cache = new ConcurrentDictionary<TIn, TOut>(new EqualityComparerUsingReflection<TIn>());
            else Cache = new ConcurrentDictionary<TIn, TOut>();
        }

        public TOut Query(TIn input) => Cache.GetOrAdd(input, x => _query.Query(input));
    }
}