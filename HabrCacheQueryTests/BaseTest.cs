using System;
using System.Reflection;
using HabrCacheQuery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
   using Microsoft.AspNetCore.Mvc;
namespace Tests
{
    public class BaseTest
    {
        protected IServiceScope ServiceScope => ServiceProvider.CreateScope();
        private readonly IServiceProvider ServiceProvider;

        public BaseTest()
        {
            var builder = new ConfigurationBuilder();
            var configuration = builder.Build();
            var services = new ServiceCollection();
            var startup = new Startup(configuration);
            startup.ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
    }
}