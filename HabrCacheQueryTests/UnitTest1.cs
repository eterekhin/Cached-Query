using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests
{
    public class Test : BaseTest
    {
        private IAsyncQuery<StubForCanCacheMySelf, Something> AsyncQuery1 { get; set; }
        private IAsyncQuery<StubForCanCacheMySelf, Something> AsyncQuery2 { get; set; }

        [SetUp]
        public void Setup()
        {
            using (var service = ServiceScope.ServiceProvider.CreateScope())
            {
                AsyncQuery1 = service.ServiceProvider.GetServices<IAsyncQuery<StubForCanCacheMySelf, Something>>().First();
                AsyncQuery2 = service.ServiceProvider.GetServices<IAsyncQuery<StubForCanCacheMySelf, Something>>().Last();;
            }
        }

        [Test]
        public async Task AsyncQuery()
        {
            var stub = new StubForCanCacheMySelf();
            await AsyncQuery1.Query(stub);
            await AsyncQuery1.Query(stub);
            await AsyncQuery1.Query(stub);
            await AsyncQuery1.Query(stub);
        }

        [Test]
        public async Task Async2Query()
        {
            var stub = new StubForCanCacheMySelf();
            await AsyncQuery2.Query(stub);
            await AsyncQuery2.Query(stub);
            await AsyncQuery2.Query(stub);
            await AsyncQuery2.Query(stub);
        }
    }
}