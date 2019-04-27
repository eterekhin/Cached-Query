using System;
using System.Linq;
using CacheQueryMediator;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HabrCacheQuery
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCachedQueries();
        }
    }
}