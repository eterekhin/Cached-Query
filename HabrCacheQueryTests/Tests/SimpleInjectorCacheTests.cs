using System;
using System.Collections.Concurrent;
using System.Linq;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

namespace Tests
{
    public class SimpleInjectorCacheTests : BaseCacheTest
    {
        protected SimpleInjectorCacheTests() : base(sc =>
        {
            var container = new Container();
//            container.Register();
//            container.RegisterConditional(
//                x => TypeCheckers.EqualsGetHashCodeOverride(x.ServiceType.GenericTypeArguments.First()));
//            sc.UseSimpleInjectorAspNetRequestScoping(container);
        })
        {
        }
    }
}