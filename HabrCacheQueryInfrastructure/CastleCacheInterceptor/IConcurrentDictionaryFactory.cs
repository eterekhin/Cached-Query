using System.Collections.Concurrent;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace CacheQueryMediator.CastleCacheInterceptor
{
    public interface IConcurrentDictionaryFactory<TIn, TOut>
    {
        ConcurrentDictionary<TIn, TOut> Create();
    }
    
    public class ConcDictionaryFactory<TIn, TOut> : IConcurrentDictionaryFactory<TIn, TOut>
    {
        private ConcurrentDictionary<TIn, TOut> _cache { get; set; }

        public ConcurrentDictionary<TIn, TOut> Create() =>
            _cache ?? (_cache = TypeCheckers.EqualsGetHashCodeOverride(typeof(TIn))
                ? new ConcurrentDictionary<TIn, TOut>()
                : new ConcurrentDictionary<TIn, TOut>(new EqualityComparerUsingReflection<TIn>()));
    }
}