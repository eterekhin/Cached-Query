using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using CacheQueryMediator.CastleCacheInterceptor;
using Castle.DynamicProxy;
using Castle.Facilities.AspNetCore;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using HabrCacheQueryInfrastructure.Query;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using static Castle.MicroKernel.Registration.Component;
using MockRepository = HabrCacheQuery.ExampleQuery.MockRepository;

namespace Tests
{
    public class CastleWindsorTest : BaseCacheTest
    {
        private IQuery<Dto, Something> query1 { get; set; }
        private IQuery<DtoWithIEnumerable, Something> query2 { get; set; }
        private static WindsorContainer container = new WindsorContainer();

        public CastleWindsorTest() : base(sc =>
            {
                container.Register(Classes.FromAssembly(Assembly.GetExecutingAssembly())
                    .BasedOn(typeof(IQuery<,>)).WithServiceBase());
                container.Register(For(typeof(CacheInterceptor<,>)));
                container.Register(
                    For(typeof(IConcurrentDictionaryFactory<,>))
                        .ImplementedBy(typeof(CacheFactory<,>))
                        .LifestyleScoped());
                container.Kernel.ProxyFactory.AddInterceptorSelector(new CacheInterceptorsSelector());
                container.Register(For<IRepository>().UsingFactoryMethod(x => MockRepositoryObject).LifestyleScoped());
            },
            sc => WindsorRegistrationHelper.CreateServiceProvider(container, sc))
        {
        }


        [SetUp]
        public void SetUp()
        {
            using (ServiceScope)
            {
                container.BeginScope();
                query1 = ServiceScope.ServiceProvider.GetService<IQuery<Dto, Something>>();
                query2 = ServiceScope.ServiceProvider.GetService<IQuery<DtoWithIEnumerable, Something>>();
            }
        }

        [Test]
        public void OneQueryTest()
        {
            var dto = new Dto() {One = 12};
            query1.Query(dto);
            query1.Query(dto);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        }
    }
}