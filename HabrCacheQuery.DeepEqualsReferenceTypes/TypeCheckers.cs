using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    public static class TypeCheckers
    {
        public static bool isEnumerable(Type propType) => typeof(IEnumerable).IsAssignableFrom(propType);
        public static bool isClass(Type type) => type.IsClass && type != typeof(string);
        public static bool isPrimitive(Type type) => type.IsPrimitive || type == typeof(string);

        public static bool isKeyValue(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

        public static readonly Func<Type, bool> EqualsGetHashCodeOverride = type => type
            .GetMethods().Where(x => new[] {nameof(GetHashCode), nameof(Equals)}.Contains(x.Name))
            .Any(x => x.DeclaringType != typeof(object));

        public static readonly Func<Type, bool> IsClass = type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition;

        public static readonly Func<Type, bool> IsQuery = type =>
            type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition;
    }
}