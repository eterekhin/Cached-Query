using System;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public abstract class CacheUsingCoreContainerBaseTests : BaseCacheTest
    {
        protected override Action<IServiceCollection> Registrations =>
            collection =>
            {
                collection.AddScoped<IRepository, MockRepository>(x => MockRepositoryObject);
                collection.AddCachedQueries();
            };

        protected override Func<IServiceCollection, IServiceProvider> ServiceProviderFactory =>
            sc => sc.BuildServiceProvider();

        protected abstract override void QueryInitial();
    }
}