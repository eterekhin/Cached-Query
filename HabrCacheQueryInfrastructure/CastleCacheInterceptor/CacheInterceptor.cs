using System;
using System.Collections.Concurrent;
using System.Linq;
using Castle.DynamicProxy;

namespace CacheQueryMediator.CastleCacheInterceptor
{
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

    //todo возможно это лучше назвать пулом или как-нибудь иначе
}