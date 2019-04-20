using System.Threading.Tasks;

namespace HabrCacheQuery.Query
{
    public interface IQuery<TIn, TOut>
    {
        TOut Query(TIn input);
    }

    public interface IAsyncQuery<TIn, TOut> : IQuery<TIn, Task<TOut>>
    {
    }
}