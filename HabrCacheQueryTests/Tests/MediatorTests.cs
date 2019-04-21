using System;
using System.Threading;
using System.Threading.Tasks;
using CacheQueryMediator;
using HabrCacheQuery.ExampleQuery;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests
{
    public class MediatrTestInputDto : IRequest<MediatrTestOutputDto>
    {
    }

    public class MediatrTestOutputDto
    {
    }

    public class MediatrTestRequestHandler : IRequestHandler<MediatrTestInputDto, MediatrTestOutputDto>
    {
        public MediatrTestRequestHandler()
        {
        }

        public async Task<MediatrTestOutputDto> Handle(MediatrTestInputDto request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            return new MediatrTestOutputDto();
        }
    }

    public class MediatorTests : BaseCacheTest
    {
        private IMediator Mediator { get; set; }

        public MediatorTests() : base(sc =>
        {
            sc.AddMediatR();
            sc.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachePipelineBehaviour<,>));
            sc.AddScoped<IRepository, MockRepository>();
            sc.AddScoped(typeof(ICacheFactory<,>), typeof(CacheFactory<,>));
        })
        {
        }

        [OneTimeSetUp]
        public void oneTimeSetUp()
        {
            using (ServiceScope)
            {
                Mediator = ServiceScope.ServiceProvider.GetService<IMediator>();
            }
        }

        [Test]
        public async Task TestCachePipeline()
        {
            var dto = new MediatrTestInputDto();
            await Mediator.Send(dto);
            var task = Mediator.Send(dto);
            Assert.True(task.IsCompleted);
        }
    }
}