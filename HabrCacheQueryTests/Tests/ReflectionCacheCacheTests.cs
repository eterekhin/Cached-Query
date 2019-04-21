using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests
{
    #region stab

    public class DtoQuery : IQuery<Dto, Something>
    {
        private readonly IRepository _repository;

        public DtoQuery(IRepository repository)
        {
            _repository = repository;
        }

        public Something Query(Dto input) => _repository.GetSomething();
    }

    public class Dto
    {
        public int One { get; set; }
    }

    public class DtoWithIEnumerable
    {
        public IEnumerable<int> En1 { get; set; }
        public string[] En2 { get; set; }
        public ICollection<bool> En3 { get; set; }
        public Dictionary<int, string> En4 { get; set; }
    }

    public class DtoWithIEnumerableQuery : IQuery<DtoWithIEnumerable, Something>
    {
        private readonly IRepository _repository;

        public DtoWithIEnumerableQuery(IRepository repository)
        {
            _repository = repository;
        }

        public Something Query(DtoWithIEnumerable input) => _repository.GetSomething();
    }

    public class DtoWithClass
    {
        public int Int { get; set; }
        public DtoWithIEnumerable DtoWithIEnumerable { get; set; }
    }

    public class DtoWithClassQuery : IQuery<DtoWithClass, Something>
    {
        private readonly IRepository _repository;

        public DtoWithClassQuery(IRepository repository)
        {
            _repository = repository;
        }

        public Something Query(DtoWithClass input) => _repository.GetSomething();
    }

    #endregion

    public class ReflectionCacheCacheTests : BaseCacheTest
    {
        private IQuery<Dto, Something> query { get; set; }
        private IQuery<DtoWithIEnumerable, Something> queryWithIEnumerable { get; set; }
        private IQuery<DtoWithClass, Something> queryWithClass { get; set; }

        public ReflectionCacheCacheTests() : base(sc => { })
        {
        }

        [SetUp]
        public void OnTimeSetup()
        {
            ServiceProviderInitial(sc => { });
            using (var scope = ServiceScope)
            {
                query = scope.ServiceProvider.GetService<IQuery<Dto, Something>>();
                queryWithClass = scope.ServiceProvider.GetService<IQuery<DtoWithClass, Something>>();
                queryWithIEnumerable = scope.ServiceProvider.GetService<IQuery<DtoWithIEnumerable, Something>>();
            }
        }

        [Test]
        public void Test1()
        {
            var dto = new Dto() {One = 1};
            var dto1 = new Dto() {One = 1};
            query.Query(dto);
            query.Query(dto1);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        }

        [Test]
        public void Test2()
        {
            var dto = new Dto() {One = 1};
            var dto1 = new Dto() {One = 2};
            query.Query(dto);
            query.Query(dto1);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Exactly(2));
        }

        [Test]
        public void Test3()
        {
            queryWithIEnumerable.Query(GetDtoWithIEnumerable());
            queryWithIEnumerable.Query(GetDtoWithIEnumerable());
            RepositoryMock.Verify(x => x.GetSomething(), Times.Once);
        }

        [Test]
        public void Test4()
        {
            var dto1 = GetDtoWithIEnumerable();
            var dto2 = GetDtoWithIEnumerable();
            dto2.En1 = new[] {1};
            queryWithIEnumerable.Query(dto1);
            queryWithIEnumerable.Query(dto2);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Exactly(2));
        }

        [Test]
        public void Test5()
        {
            queryWithClass.Query(GetDtoWithClass());
            queryWithClass.Query(GetDtoWithClass());
            RepositoryMock.Verify(x => x.GetSomething(), Times.Exactly(1));
        }


        [Test]
        public void Test6()
        {
            var dto1 = GetDtoWithClass();
            var dto2 = GetDtoWithClass();
            dto2.DtoWithIEnumerable.En2 = Enumerable.Range(1, 1000).Select(x => x.ToString()).ToArray();
            queryWithClass.Query(dto1);
            queryWithClass.Query(dto2);
            RepositoryMock.Verify(x => x.GetSomething(), Times.Exactly(2));
        }

        private DtoWithIEnumerable GetDtoWithIEnumerable()
        {
            var en1 = Enumerable.Range(1, 100).ToList();
            var en2 = Enumerable.Range(1, 100).Select(x => x.ToString()).ToArray();
            var en3 = Enumerable.Range(1, 100).Select(x => x % 2 != 0).ToList();
            var en4 = Enumerable.Range(1, 100).ToDictionary(x => x, x => x.ToString());
            return new DtoWithIEnumerable() {En1 = en1, En2 = en2, En3 = en3, En4 = en4};
        }

        private DtoWithClass GetDtoWithClass()
        {
            return new DtoWithClass {DtoWithIEnumerable = GetDtoWithIEnumerable(), Int = 1};
        }
    }
}