using System;
using System.Collections.Concurrent;
using System.Linq;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Proxy;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace CacheQueryMediator.CastleCacheInterceptor
{
    public class CacheInterceptorsSelector : IModelInterceptorsSelector
    {
        public bool HasInterceptors(ComponentModel model)
        {
            return model.Services.All(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQuery<,>));
        }

        public InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors)
        {
            return new[] {InterceptorReference.ForType(typeof(CacheInterceptor<,>))};
        }
    }


    public  class CacheInterceptor<TIn, TOut> : IInterceptor
    {
        private readonly ConcurrentDictionary<TIn, TOut> _cache;

        public CacheInterceptor(IConcurrentDictionaryFactory<TIn, TOut> cacheFactory)
        {
            _cache = cacheFactory.Create();
        }

        public void Intercept(IInvocation invocation)
        {
            var input = (TIn) invocation.Arguments.Single();
            if (_cache.TryGetValue(input, out var value))
                invocation.ReturnValue = value;
            else
            {
                invocation.Proceed();
                _cache.TryAdd(input, (TOut) invocation.ReturnValue);
            }
        }
    }

    public interface IConcurrentDictionaryFactory<TIn, TOut>
    {
        ConcurrentDictionary<TIn, TOut> Create();
    }

    //todo возможно это лучше назвать пулом или как-нибудь иначе
    public class CacheFactory<TIn, TOut> : IConcurrentDictionaryFactory<TIn, TOut>
    {
        private ConcurrentDictionary<TIn, TOut> _cache { get; set; }

        public ConcurrentDictionary<TIn, TOut> Create() =>
            _cache ?? (_cache = TypeCheckers.EqualsGetHashCodeOverride(typeof(TIn))
                ? new ConcurrentDictionary<TIn, TOut>()
                : new ConcurrentDictionary<TIn, TOut>(new EqualityComparerUsingReflection<TIn>()));
    }
}