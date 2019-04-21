using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabrCacheQuery.Query;
using static HabrCacheQuery.ServiceCollectionExtensions.DeepEquals;
using static HabrCacheQuery.ServiceCollectionExtensions.Hash;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public abstract class BaseCacheQuery<TIn, TOut> : IQuery<TIn, TOut>
    {
        private readonly ConcurrentDictionary<TIn, TOut> _cache =
            new ConcurrentDictionary<TIn, TOut>(new EqualityComparer<TIn>());

        private readonly IQuery<TIn, TOut> _query;

        public BaseCacheQuery(IQuery<TIn, TOut> query)
        {
            _query = query;
        }

        public TOut Query(TIn input) => _cache.GetOrAdd(input, x => _query.Query(input));
    }

    public class EqualityComparer<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => Hash.GetHashCode(obj);
    }

    public abstract class BaseAsyncCacheQuery<TIn, TOut> : BaseCacheQuery<TIn, Task<TOut>>, IAsyncQuery<TIn, TOut>
    {
        public Task<TOut> Query(TIn input) => base.Query(input);

        public BaseAsyncCacheQuery(IAsyncQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheQueryUsingFody<TIn, TOut> : BaseCacheQuery<TIn, TOut> where TIn : CanCacheMySelfUsingFody
    {
        public CacheQueryUsingFody(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class AsyncCacheQueryUsingFody<TIn, TOut> : BaseCacheQuery<TIn, TOut> where TIn : CanCacheMySelfUsingFody
    {
        public AsyncCacheQueryUsingFody(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheQueryUsingReflection<TIn, TOut> : BaseCacheQuery<TIn, TOut>
        where TIn : CanCacheMySelfUsingReflection
    {
        public CacheQueryUsingReflection(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class AsyncCacheQueryUsingReflection<TIn, TOut> : BaseCacheQuery<TIn, TOut>
        where TIn : CanCacheMySelfUsingFody
    {
        public AsyncCacheQueryUsingReflection(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }
}