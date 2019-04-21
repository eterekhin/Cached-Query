using System.Collections.Concurrent;

namespace CacheQueryMediator
{
    public interface ICacheFactory<TDto, TResult>
    {
        ConcurrentDictionary<TDto, TResult> Create();
    }
}