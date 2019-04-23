using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using CacheQueryMediator.SimpleInjectorCache;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleInjector;

namespace Tests
{
    public class SimpleInjectorCacheTests : BaseCacheTest
    {
        private IQuery<Dto, Something> _query { get; set; }
        private static Container Container { get; set; }

        [Test]
        public void Test()
        {
            var dto = new Dto() {One = 1};
            _query.Query(dto);
            _query.Query(dto);
            VerifyOneCall();
        }

        protected override void QueryInitial()
        {
            using (Scope)
            {
                _query = Scope.ServiceProvider.GetService<IQuery<Dto, Something>>();
            }  
        }

        protected override Action<IServiceCollection> Registrations =>
            sc =>
            {
                var queries = Assembly.GetExecutingAssembly()
                    .ExportedTypes
                    .Select(x => new
                    {
                        source = x.GetInterfaces().FirstOrDefault(
                            xx => xx.IsGenericType && xx.GetGenericTypeDefinition() == typeof(IQuery<,>)),
                        dest = x
                    })
                    .Where(x => x.source != null).GroupBy(x => x.source).ToList();
                foreach (var query in queries)
                {
                    var first = query.First();
                    // todo добавить множественную регистрацию
                    Container.Register(first.source, first.dest);
                }

                Container.RegisterDecorator(
                    typeof(IQuery<,>),
                    typeof(CacheWithTypeOverrideEqAndGHC<,>),
                    x => TypeCheckers.EqualsGetHashCodeOverride(x.ServiceType.GetGenericArguments()[0]));

                Container.RegisterDecorator(
                    typeof(IQuery<,>),
                    typeof(DefaultCacheQuery<,>),
                    x => !TypeCheckers.EqualsGetHashCodeOverride(x.ServiceType.GetGenericArguments()[0]));

                Container.Register<IRepository, MockRepository>();
                sc.EnableSimpleInjectorCrossWiring(Container);
                sc.UseSimpleInjectorAspNetRequestScoping(Container);
            };

        protected override Func<IServiceCollection, IServiceProvider> ServiceProviderFactory =>
            sc => sc.BuildServiceProvider();
    }
}