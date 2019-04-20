using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading.Tasks;
using HabrCacheQuery.ExampleQuery;
using HabrCacheQuery.Query;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class AddCachedQueriesExtensions
    {
        #region predicate

        private static readonly Func<Type, bool> IsClass = type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition;

        private static Type GetQueryInterface(Type definition, Type destType) =>
            destType.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == definition);

        private static readonly Func<Type, bool> ContainsQueryInterface =
            destType => GetQueryInterface(typeof(IQuery<,>), destType) != null;

        private static readonly Func<Type, bool> ContainsAsyncQueryInterface =
            destType => GetQueryInterface(typeof(IAsyncQuery<,>), destType) != null;

        private static readonly Func<Type, bool> IsCachedQuery = type =>
            GetQueryInterface(typeof(IQuery<,>), type)?.GenericTypeArguments?.FirstOrDefault()?.BaseType ==
            typeof(CanCacheMySelf);

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
            var asyncQueryPredicate = AggregatePredicates(IsClass, IsCachedQuery, ContainsAsyncQueryInterface);
            
            var queryInterface = AggregatePredicates(
                IsClass,
                IsCachedQuery,
                x => !asyncQueryPredicate(x),
                ContainsQueryInterface);

            var asyncQueries = GetAssemblesTypes(asyncQueryPredicate, DestAsyncQuerySourceType);
            var queries = GetAssemblesTypes(queryInterface, DestQuerySourceType);

            serviceCollection.QueryDecorator(asyncQueries, typeof(AsyncCacheQuery<,>));
            serviceCollection.QueryDecorator(queries, typeof(CacheQuery<,>));
        }

        private static void QueryDecorator(this IServiceCollection serviceCollection,
            IEnumerable<(Type dest, Type source)> parameters, Type decoratorDefinition,
            ServiceLifetime lifeTime = ServiceLifetime.Scoped)
        {
            foreach (var (dest, source) in parameters)
            {
                serviceCollection.AddDecorator(
                    decoratorDefinition.MakeGenericType(source.GenericTypeArguments),
                    source,
                    dest,
                    lifeTime);
            }
        }

        private static void AddDecorator(this IServiceCollection serviceCollection, Type dt, Type queryType,
            Type destType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            // ReSharper disable once ConvertToLocalFunction
            Func<IServiceProvider, object> factory = provider => ActivatorUtilities.CreateInstance(provider, dt,
                ActivatorUtilities.GetServiceOrCreateInstance(provider, destType));

            serviceCollection.Add(new ServiceDescriptor(queryType, factory, lifetime));
        }
    }
}