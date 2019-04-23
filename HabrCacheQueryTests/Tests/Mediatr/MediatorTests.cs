using System;
using System.Threading;
using System.Threading.Tasks;
using CacheQueryMediator;
using CacheQueryMediator.CastleCacheInterceptor;
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
        public async Task<MediatrTestOutputDto> Handle(MediatrTestInputDto request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            return new MediatrTestOutputDto();
        }
    }

    public class MediatorTests : BaseCacheTest
    {
        private IMediator Mediator { get; set; }

        [Test]
        public async Task TestCachePipeline()
        {
            var dto = new MediatrTestInputDto();
            await Mediator.Send(dto);
            var task = Mediator.Send(dto);
            Assert.True(task.IsCompleted);
        }

        protected override void QueryInitial()
        {
            using (Scope)
            {
                Mediator = Scope.ServiceProvider.GetService<IMediator>();
            }
        }

        protected override Action<IServiceCollection> Registrations =>
            sc =>
            {
                sc.AddMediatR();
                sc.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachePipelineBehaviour<,>));
                sc.AddScoped<IRepository, MockRepository>();
                sc.AddScoped(typeof(IConcurrentDictionaryFactory<,>), typeof(ConcDictionaryFactory<,>));
            };

        protected override Func<IServiceCollection, IServiceProvider> ServiceProviderFactory =>
            x => x.BuildServiceProvider();
    }
}