using System.Threading.Tasks;
using CacheQueryMediator.CastleCacheInterceptor;
using HabrCacheQuery.Query;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
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
}