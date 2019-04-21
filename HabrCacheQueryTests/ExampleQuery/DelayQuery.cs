using System.Threading.Tasks;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace HabrCacheQuery.ExampleQuery
{
    public class StubForFodyCanCacheMySelf : CanCacheMySelfUsingFody
    {
        public int StubInt { get; set; }
    }

    public class Something
    {
    }

    public class DelayQuery1 : IAsyncQuery<StubForFodyCanCacheMySelf, Something>
    {
        public async Task<Something> Query(StubForFodyCanCacheMySelf input)
        {
            await Task.Delay(1000);
            return new Something();
        }
    }

    //Clone DelayQuery1
    public class DelayQuery2 : IAsyncQuery<StubForFodyCanCacheMySelf, Something>
    {
        public async Task<Something> Query(StubForFodyCanCacheMySelf input)
        {
            await Task.Delay(2000);
            return new Something();
        }
    }

    public interface IRepository
    {
        Something GetSomething();
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class MockRepository : IRepository
    {
        public virtual Something GetSomething()
        {
            return new Something();
        }
    }

    public class SomethingQuery : IQuery<StubForFodyCanCacheMySelf, Something>
    {
        private readonly IRepository _repository;

        public SomethingQuery(IRepository repository)
        {
            _repository = repository;
        }

        public Something Query(StubForFodyCanCacheMySelf input)
        {
            return _repository.GetSomething();
        }
    }
}