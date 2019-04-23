using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CacheQueryMediator.CastleCacheInterceptor;
using HabrCacheQuery.Query;
using HabrCacheQuery.ServiceCollectionExtensions;
using Microsoft.Extensions.DependencyInjection;
using static HabrCacheQuery.ServiceCollectionExtensions.TypeCheckers;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class AddCachedQueriesExtensions
    {
        #region predicates

        private static Type GetQueryInterface(Type definition, Type destType) =>
            destType.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == definition);

        private static readonly Func<Type, bool> ContainsQueryInterface =
            destType => GetQueryInterface(typeof(IQuery<,>), destType) != null;

        private static readonly Func<Type, bool> ContainsAsyncQueryInterface =
            destType => GetQueryInterface(typeof(IAsyncQuery<,>), destType) != null;

        private static bool IsCachedQuery(Type cacheType, Type type) =>
            GetQueryInterface(typeof(IQuery<,>), type)?.GenericTypeArguments?.FirstOrDefault()?.BaseType ==
            cacheType;

        private static readonly Func<Type, (Type dest, Type source)> DestQuerySourceType = type =>
            (type, GetQueryInterface(typeof(IQuery<,>), type));

        private static readonly Func<Type, (Type, Type)> DestAsyncQuerySourceType = type =>
            (type, GetQueryInterface(typeof(IAsyncQuery<,>), type));

        private static Func<T, bool> AggregatePredicates<T>(params Func<T, bool>[] predicates) =>
            predicates.Aggregate<Func<T, bool>, Func<T, bool>>(x => true, (a, c) => x => a(x) && c(x));

        // ReSharper disable once ReturnTypeCanBeEnumerable.Local
        private static (Type dest, Type source)[] GetAssemblesTypes(Func<Type, bool> predicate,
            Func<Type, (Type, Type)> selector) => AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(predicate)
            .Select(selector)
            .ToArray();

        #endregion

        public static void AddCachedQueries(this IServiceCollection serviceCollection)
        {
            var asyncQueryScanPredicate = AggregatePredicates(IsClass, ContainsAsyncQueryInterface);

            var queryScanAssemblesPredicate =
                AggregatePredicates(IsClass, x => !asyncQueryScanPredicate(x), ContainsQueryInterface);

            var asyncQueries = GetAssemblesTypes(asyncQueryScanPredicate, DestAsyncQuerySourceType);
            var queries = GetAssemblesTypes(queryScanAssemblesPredicate, DestQuerySourceType);

            serviceCollection.AddScoped(typeof(IConcurrentDictionaryFactory<,>), typeof(ConcDictionaryFactory<,>));

            serviceCollection.QueryDecorate(asyncQueries, typeof(AsyncQueryCache<,>));
            serviceCollection.QueryDecorate(queries, typeof(QueryCache<,>));
        }

        private static void QueryDecorate(this IServiceCollection serviceCollection,
            IEnumerable<(Type dest, Type source)> parameters, Type cacheType,
            ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            foreach (var (dest, source) in parameters)
                serviceCollection.AddDecorator(
                    cacheType.MakeGenericType(source.GenericTypeArguments[0], source.GenericTypeArguments[1]),
                    source,
                    dest,
                    lifeTime);
        }


        private static void AddDecorator(
            this IServiceCollection serviceCollection,
            Type dt, Type queryType,
            Type destType,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            // ReSharper disable once ConvertToLocalFunction
            Func<IServiceProvider, object> factory = provider => ActivatorUtilities.CreateInstance(provider, dt,
                ActivatorUtilities.GetServiceOrCreateInstance(provider, destType));

            serviceCollection.Add(new ServiceDescriptor(queryType, factory, lifetime));
        }
    }
}