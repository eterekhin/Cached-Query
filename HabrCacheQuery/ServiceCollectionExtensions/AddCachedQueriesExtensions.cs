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

        private static readonly Func<Type, (Type dest, Type source)> DestQuerySourceType = type =>
            (GetQueryInterface(typeof(IQuery<,>), type), type);

        private static readonly Func<Type, (Type, Type)> DestAsyncQuerySourceType = type =>
            (GetQueryInterface(typeof(IAsyncQuery<,>), type), type);

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
            // Func<Type,bool> который выбирает типы реализующие IAsyncQuery
            var asyncQueryScanPredicate = AggregatePredicates(IsClass, ContainsAsyncQueryInterface);

            // Func<Type,bool> который выбирает типы реализующие IQuery
            var queryScanAssemblesPredicate =
                AggregatePredicates(IsClass, x => !asyncQueryScanPredicate(x), ContainsQueryInterface);

            // все IAsyncQuery в сканируемых сборках
            var asyncQueries = GetAssemblesTypes(asyncQueryScanPredicate, DestAsyncQuerySourceType);
            // все IQuery в сканируемых сборках
            var queries = GetAssemblesTypes(queryScanAssemblesPredicate, DestQuerySourceType);
            // регистрируем фабрику создающую ConcurrentDictionary
            serviceCollection.AddScoped(typeof(IConcurrentDictionaryFactory<,>), typeof(ConcDictionaryFactory<,>));
            // добавляет в services ServiceDescriptor'ы описывающие регистрацию IAsyncQuery
            serviceCollection.QueryDecorate(asyncQueries, typeof(AsyncQueryCache<,>));
            // добавляет в services ServiceDescriptor'ы описывающие регистрацию IQuery
            serviceCollection.QueryDecorate(queries, typeof(QueryCache<,>));
        }

        private static void QueryDecorate(this IServiceCollection serviceCollection,
            IEnumerable<(Type source, Type dest)> parameters, Type cacheType,
            ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            foreach (var (source, dest) in parameters)
                serviceCollection.AddDecorator(
                    cacheType.MakeGenericType(source.GenericTypeArguments[0], source.GenericTypeArguments[1]),
                    source,
                    dest,
                    lifeTime);
        }


        private static void AddDecorator(
            this IServiceCollection serviceCollection,
            Type cacheType, Type querySourceType,
            Type queryDestType,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            // ReSharper disable once ConvertToLocalFunction
            Func<IServiceProvider, object> factory = provider => ActivatorUtilities.CreateInstance(provider, cacheType,
                ActivatorUtilities.GetServiceOrCreateInstance(provider, queryDestType));

            serviceCollection.Add(new ServiceDescriptor(querySourceType, factory, lifetime));
        }
    }
}