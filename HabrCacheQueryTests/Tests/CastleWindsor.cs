using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using Castle.Facilities.AspNetCore;
using Castle.Windsor;
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
            container.Register(For<IQuery<Dto, Something>>().ImplementedBy<DtoQuery>().LifestyleScoped());
            container.Register(For<IQuery<DtoWithIEnumerable, Something>>().ImplementedBy<DtoWithIEnumerableQuery>()
                .LifestyleScoped());
            container.Register(For<IRepository>().ImplementedBy<MockRepository>().LifestyleScoped());
            container.Register(For(typeof(IQuery<,>)).ImplementedBy(typeof(CacheQueryWithCacheStrategy<,>))
                .LifestyleScoped());
            sc.AddWindsor(container, x => { }, () => sc.BuildServiceProvider());
        })
        {
        }


        [SetUp]
        public void SetUp()
        {
            using (ServiceScope)
            {
                query1 = ServiceScope.ServiceProvider.GetService<IQuery<Dto, Something>>();
                query2 = ServiceScope.ServiceProvider.GetService<IQuery<DtoWithIEnumerable, Something>>();
            }
        }

        [Test]
        public void OneQueryTest()
        {
            var dto = new Dto();
            query1.Query(dto);
            query1.Query(dto);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        }
    }
}