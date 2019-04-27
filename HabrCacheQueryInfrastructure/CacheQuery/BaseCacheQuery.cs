using System.Collections.Concurrent;
using CacheQueryMediator.CastleCacheInterceptor;
using HabrCacheQuery.Query;

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
}