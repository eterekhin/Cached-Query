using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CacheQueryMediator.CastleCacheInterceptor;
using MediatR;
using MediatR.Pipeline;

namespace CacheQueryMediator
{
    public class CachePipelineBehaviour<TDto, TResult> : IPipelineBehavior<TDto, TResult>
    {
        private readonly ConcurrentDictionary<TDto, Task<TResult>> _cache;

        public CachePipelineBehaviour(IConcurrentDictionaryFactory<TDto, Task<TResult>> cacheFactory)
        {
            _cache = cacheFactory.Create();
        }

        public async Task<TResult> Handle(TDto request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResult> next) =>
            await _cache.GetOrAdd(request, x => next());
    }
}