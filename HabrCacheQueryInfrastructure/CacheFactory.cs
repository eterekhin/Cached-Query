using System.Collections.Concurrent;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace CacheQueryMediator
{
    public class CacheFactory<TDto, TResult> : ICacheFactory<TDto, TResult>
        where TResult : class
    {
        public ConcurrentDictionary<TDto, TResult> Create()
        {
            return TypeCheckers.EqualsGetHashCodeOverride(typeof(TDto))
                ? new ConcurrentDictionary<TDto, TResult>(new EqualityComparerUsingReflection<TDto>())
                : new ConcurrentDictionary<TDto, TResult>();
        }
    }
}