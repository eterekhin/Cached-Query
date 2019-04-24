using System.Linq;
using Castle.Core;
using Castle.MicroKernel.Proxy;
using HabrCacheQuery.Query;

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
}