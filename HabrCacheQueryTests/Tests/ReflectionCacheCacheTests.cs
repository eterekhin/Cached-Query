using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.MicroKernel;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Tests
{
    #region stab

    public class Dto
    {
        public int One { get; set; }
    }

    public class DtoQuery : IQuery<Dto, Something>
    {
        private readonly IRepository _repository;

        public DtoQuery(IRepository repository)
        {
            _repository = repository;
        }

        public Something Query(Dto input) => _repository.GetSomething();
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

    #endregion

    public class ReflectionCacheCacheTests : CacheUsingCoreContainerBaseTests
    {
        private IQuery<Dto, Something> query { get; set; }
        private IQuery<DtoWithIEnumerable, Something> queryWithIEnumerable { get; set; }

        [Test]
        public void Test1()
        {
            var dto = new Dto {One = 1};
            var dto1 = new Dto {One = 1};
            query.Query(dto);
            query.Query(dto1);
            VerifyOneCall();
        }

        [Test]
        public void Test2()
        {
            var dto = new Dto {One = 1};
            var dto1 = new Dto {One = 2};
            query.Query(dto);
            query.Query(dto1);
            VerifyTwoCall();
        }

        [Test]
        public void Test3()
        {
            queryWithIEnumerable.Query(GetDtoWithIEnumerable());
            queryWithIEnumerable.Query(GetDtoWithIEnumerable());
            VerifyOneCall();
        }

        [Test]
        public void Test4()
        {
            var dto1 = GetDtoWithIEnumerable();
            var dto2 = GetDtoWithIEnumerable();
            dto2.En1 = new[] {1};
            queryWithIEnumerable.Query(dto1);
            queryWithIEnumerable.Query(dto2);
            VerifyTwoCall();
        }


        private DtoWithIEnumerable GetDtoWithIEnumerable()
        {
            var en1 = Enumerable.Range(1, 100).ToList();
            var en2 = Enumerable.Range(1, 100).Select(x => x.ToString()).ToArray();
            var en3 = Enumerable.Range(1, 100).Select(x => x % 2 != 0).ToList();
            var en4 = Enumerable.Range(1, 100).ToDictionary(x => x, x => x.ToString());
            return new DtoWithIEnumerable() {En1 = en1, En2 = en2, En3 = en3, En4 = en4};
        }

        protected override void QueryInitial()
        {
            using (var scope = Scope.ServiceProvider.CreateScope())
            {
                query = scope.ServiceProvider.GetService<IQuery<Dto, Something>>();
                queryWithIEnumerable = scope.ServiceProvider.GetService<IQuery<DtoWithIEnumerable, Something>>();
            }
        }
    }
}