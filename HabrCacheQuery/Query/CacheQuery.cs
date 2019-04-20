using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HabrCacheQuery.Query;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public abstract class CanCacheMySelf
    {
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
    }

    public class CacheQuery<TIn, TOut> : IQuery<TIn, TOut> where TIn : CanCacheMySelf
    {
        private readonly ConcurrentDictionary<TIn, TOut> _cache = new ConcurrentDictionary<TIn, TOut>();
        private readonly IQuery<TIn, TOut> _query;

        public CacheQuery(IQuery<TIn, TOut> query)
        {
            _query = query;
        }    

        public TOut Query(TIn input) => _cache.GetOrAdd(input, x => _query.Query(input));
    }

    public class AsyncCacheQuery<TIn, TOut> : CacheQuery<TIn, Task<TOut>>, IAsyncQuery<TIn, TOut>
        where TIn : CanCacheMySelf
    {
        public Task<TOut> Query(TIn input) => base.Query(input);

        public AsyncCacheQuery(IAsyncQuery<TIn, TOut> query) : base(query)
        {
        }
    }
}