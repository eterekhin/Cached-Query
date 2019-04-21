using System;
using System.Collections.Generic;
using System.Linq;
using HabrCacheQuery.Query;
using Microsoft.Extensions.DependencyInjection;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class AddCachedQueriesExtensions
    {
        #region predicates

        private static readonly Func<Type, bool> IsClass = type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition;

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
            var asyncQueryScanAssemblesPredicate = AggregatePredicates(IsClass, ContainsAsyncQueryInterface);

            var queryScanAssemblesPredicate =
                AggregatePredicates(IsClass, x => !asyncQueryScanAssemblesPredicate(x), ContainsQueryInterface);

            var asyncQueries = GetAssemblesTypes(asyncQueryScanAssemblesPredicate, DestAsyncQuerySourceType);
            var queries = GetAssemblesTypes(queryScanAssemblesPredicate, DestQuerySourceType);

            serviceCollection.QueryDecorator(asyncQueries, (sourceType, destType) =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return (TypeCheckers.EqualsGetHashCodeOverride(sourceType)
                    ? typeof(CacheAsyncQuery<,>)
                    : typeof(CacheAsyncQueryWithReflectionComparer<,>)).MakeGenericType(sourceType, destType);
            });
            serviceCollection.QueryDecorator(queries, (sourceType, destType) =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return (TypeCheckers.EqualsGetHashCodeOverride(sourceType)
                    ? typeof(CacheQuery<,>)
                    : typeof(CacheQueryWithReflectionComparer<,>)).MakeGenericType(sourceType, destType);
            });
        }

        private static void QueryDecorator(this IServiceCollection serviceCollection,
            IEnumerable<(Type dest, Type source)> parameters,
            Func<Type, Type, Type> sourceTypeDestTypeToDecoratorType,
            ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            foreach (var (dest, source) in parameters)
                serviceCollection.AddDecorator(
                    sourceTypeDestTypeToDecoratorType(source.GenericTypeArguments[0], source.GenericTypeArguments[1]),
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