using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HabrCacheQuery.Query;
using static HabrCacheQuery.ServiceCollectionExtensions.DeepEquals;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public class BaseCacheQuery<TIn, TOut> : IQuery<TIn, TOut>
    {
        protected virtual ConcurrentDictionary<TIn, TOut> Cache { get; }

        private readonly IQuery<TIn, TOut> _query;


        protected BaseCacheQuery(IQuery<TIn, TOut> query)
        {
            _query = query;
        }

        public TOut Query(TIn input) => Cache.GetOrAdd(input, x => _query.Query(input));
    }

    public abstract class CacheQueryReflectionComparer<TIn, TOut> : BaseCacheQuery<TIn, TOut>
    {
        protected override ConcurrentDictionary<TIn, TOut> Cache { get; } =
            new ConcurrentDictionary<TIn, TOut>(new EqualityComparerUsingReflection<TIn>());

        protected CacheQueryReflectionComparer(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }


    public abstract class BaseCacheQueryWithDtoOverrideEqualsGetHashCode<TIn, TOut> : BaseCacheQuery<TIn, TOut>
    {
        protected override ConcurrentDictionary<TIn, TOut> Cache { get; } = new ConcurrentDictionary<TIn, TOut>();

        protected BaseCacheQueryWithDtoOverrideEqualsGetHashCode(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheQueryWithReflectionComparer<TIn, TOut> : CacheQueryReflectionComparer<TIn, TOut>
    {
        public CacheQueryWithReflectionComparer(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheQuery<TIn, TOut> : BaseCacheQueryWithDtoOverrideEqualsGetHashCode<TIn, TOut>
    {
        public CacheQuery(IQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheAsyncQueryWithReflectionComparer<TIn, TOut> : CacheQueryReflectionComparer<TIn, Task<TOut>>,
        IAsyncQuery<TIn, TOut>
    {
        public CacheAsyncQueryWithReflectionComparer(IAsyncQuery<TIn, TOut> query) : base(query)
        {
        }
    }

    public class CacheAsyncQuery<TIn, TOut> : BaseCacheQueryWithDtoOverrideEqualsGetHashCode<TIn, Task<TOut>>,
        IAsyncQuery<TIn, TOut>
    {
        public CacheAsyncQuery(IAsyncQuery<TIn, TOut> query) : base(query)
        {
        }
    }


    public class EqualityComparerUsingReflection<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y) => DeepEqualsCommonType(x, y);

        public int GetHashCode(TKey obj) => Hash.GetHashCode(obj);
    }
}