using System.Threading.Tasks;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;

namespace HabrCacheQuery.ExampleQuery
{
    public class StubForCanCacheMySelf : CanCacheMySelf
    {
        public int StubInt { get; set; }

        protected bool Equals(StubForCanCacheMySelf other)
        {
            return StubInt == other.StubInt;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StubForCanCacheMySelf) obj);
        }

        public override int GetHashCode()
        {
            return StubInt;
        }
    }

    public class Something
    {
    }

    public interface IRepository
    {
    }

    public class Repository : IRepository
    {
    }

    public class DelayQuery1 : IAsyncQuery<StubForCanCacheMySelf, Something>
    {
        public DelayQuery1(IRepository repository)
        {
        }

        public async Task<Something> Query(StubForCanCacheMySelf input)
        {
            await Task.Delay(1000);
            return new Something();
        }
    }

    public class DelayQuery2 : IAsyncQuery<StubForCanCacheMySelf, Something>
    {
        public async Task<Something> Query(StubForCanCacheMySelf input)
        {
            await Task.Delay(2000);
            return new Something();
        }
    }
}