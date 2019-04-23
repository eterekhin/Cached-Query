using System;
using System.Reflection;
using CacheQueryMediator.CastleCacheInterceptor;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using static Castle.MicroKernel.Registration.Component;

namespace Tests
{
    public class CastleWindsorTest : BaseCacheTest
    {
        private IQuery<Dto, Something> query1 { get; set; }
        private IQuery<DtoWithIEnumerable, Something> query2 { get; set; }
        private static WindsorContainer container = new WindsorContainer();


        [Test]
        public void OneQueryTest()
        {
            var dto = new Dto {One = 12};
            query1.Query(dto);
            query1.Query(dto);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        }

        protected override void QueryInitial()
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                using (container.BeginScope())
                {
                    query1 = scope.ServiceProvider.GetService<IQuery<Dto, Something>>();
                    query2 = scope.ServiceProvider.GetService<IQuery<DtoWithIEnumerable, Something>>();
                }
            }
        }


        protected override Action<IServiceCollection> Registrations =>
            sc =>
            {
                container = new WindsorContainer();
                container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
                    .BasedOn(typeof(IQuery<,>)).WithServiceBase());
                container.Register(
                    For(typeof(IConcurrentDictionaryFactory<,>))
                        .ImplementedBy(typeof(ConcDictionaryFactory<,>))
                        .LifestyleScoped());
                container.Register(For(typeof(CacheInterceptor<,>)));
                container.Kernel.ProxyFactory.AddInterceptorSelector(new CacheInterceptorsSelector());
                container.Register(For<IRepository>().UsingFactoryMethod(x => MockRepositoryObject).LifestyleScoped());
            };


        protected override Func<IServiceCollection, IServiceProvider> ServiceProviderFactory
            => sc => WindsorRegistrationHelper.CreateServiceProvider(container, sc);
    }
}