using System;
using System.Linq;
using System.Reflection;
using HabrCacheQuery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using MockRepository = HabrCacheQuery.ExampleQuery.MockRepository;

namespace Tests
{
    [TestFixture]
    public abstract class BaseCacheTest
    {
        protected IServiceScope Scope => ServiceProvider.CreateScope();
        protected IServiceProvider ServiceProvider { get; set; }

        [SetUp]
        public void Setup()
        {
            ServiceProviderInitial();
            QueryInitial();
        }

        protected BaseCacheTest()
        {
            ServiceProviderInitial();
        }

        private void ServiceProviderInitial()
        {
            RepositoryMock = new Mock<MockRepository>();
            var collection = new ServiceCollection();

            Registrations(collection);
            ServiceProvider = ServiceProviderFactory(collection);
        }

        protected abstract void QueryInitial();
        protected abstract Action<IServiceCollection> Registrations { get; }
        protected abstract Func<IServiceCollection, IServiceProvider> ServiceProviderFactory { get; }

        protected static Mock<MockRepository> RepositoryMock { get; private set; } = new Mock<MockRepository>();
        protected static MockRepository MockRepositoryObject => RepositoryMock.Object;

        protected static void VerifyOneCall() => RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        protected static void VerifyTwoCall() => RepositoryMock.Verify(x => x.GetSomething(), Times.Exactly(2));
    }
}