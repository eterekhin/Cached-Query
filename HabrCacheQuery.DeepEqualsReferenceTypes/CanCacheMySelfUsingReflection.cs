using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
    #region Hash

    public static class Hash
    {
        public static int GetHashCode(object obj) => MatchAndAction(obj);

        private static int PrimitiveHashCode(object obj) => obj.GetHashCode();

        private static int ClassHashCode(object obj) =>
            DeepEquals.GetPropFieldValue(obj).Aggregate(0, (a, c) => a ^ GetHashCode(c.value) * 357);

        private static int IEnumerableGetHashCode(object obj)
        {
            var enumerator = (obj as IEnumerable).GetEnumerator();
            var hash = 0;
            while (enumerator.MoveNext())
                hash ^= GetHashCode(enumerator.Current);

            return hash;
        }

        private static int KeyValuePairGetHashCode(object obj)
        {
            dynamic keyValuePair = obj;
            return GetHashCode(keyValuePair.Key) ^ GetHashCode(keyValuePair.Value);
        }

        private static int MatchAndAction(object obj)
        {
            IEnumerable<(Func<Type, bool>, Func<object, int>)> matches()
            {
                yield return (TypeCheckers.isPrimitive, PrimitiveHashCode);
                yield return (TypeCheckers.isEnumerable, IEnumerableGetHashCode);
                yield return (TypeCheckers.isClass, ClassHashCode);
                yield return (TypeCheckers.isKeyValue, KeyValuePairGetHashCode);
            }

            return matches().First(x => x.Item1.Invoke(obj.GetType())).Item2.Invoke(obj);
        }
    }

    #endregion

    #region TypeCheckers

    public static class TypeCheckers
    {
        public static bool isEnumerable(Type propType) => typeof(IEnumerable).IsAssignableFrom(propType);
        public static bool isClass(Type type) => type.IsClass && type != typeof(string);
        public static bool isPrimitive(Type type) => type.IsPrimitive || type == typeof(string);

        public static bool isKeyValue(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
    }

    #endregion

    #region DeepEquals

    public static class DeepEquals
    {
        public static Boolean DeepEqualsCommonType(object o1, object o2)
        {
            if (!(o1.GetType() == o2.GetType()))
                throw new ArgumentException();
            return MatchAndAction(o1, o2, o1.GetType());
        }

        private static bool EqualsClass(object o1, object o2)
        {
            if (o1.GetType() != o2.GetType()) throw new ArgumentException();
            return GetPropFieldValue(o1).Zip(GetPropFieldValue(o2), (x, y) => (x.memberType, x.value, y.value))
                .Aggregate(true, (a, c) => a && DeepEqualsCommonType(c.Item2, c.Item3));
        }

        private static bool EqualsPrimitive(object o1, object o2)
        {
            return o1.Equals(o2);
        }

        private static bool EqualsIEnumerable(object o1, object o2)
        {
            var equals = true;
            if (o1 is IEnumerable en1 && o2 is IEnumerable en2)
            {
                var enumerator1 = en1.GetEnumerator();
                var enumerator2 = en2.GetEnumerator();

                while (enumerator1.MoveNext() & enumerator2.MoveNext())
                    equals &= DeepEqualsCommonType(enumerator1.Current, enumerator2.Current);
            }

            else throw new ArgumentException();

            return equals;
        }

        private static IEnumerable<T> GetIEnumerable<T>(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        private static bool MatchAndAction(object o1, object o2, Type type)
        {
            IEnumerable<(Func<Type, bool>, Func<object, object, bool>)> matchAction()
            {
                yield return (TypeCheckers.isPrimitive, EqualsPrimitive);
                yield return (TypeCheckers.isEnumerable, EqualsIEnumerable);
                yield return (TypeCheckers.isClass, EqualsClass);
                yield return (TypeCheckers.isKeyValue, EqualsKeyValue);
            }

            return matchAction().FirstOrDefault(x => x.Item1(type)).Item2(o1, o2);
        }

        private static bool EqualsKeyValue(object o1, object o2)
        {
            if (!TypeCheckers.isKeyValue(o1.GetType()) || !TypeCheckers.isKeyValue(o2.GetType()))
                throw new NotSupportedException();
            var keyValue1 = (dynamic) o1;
            var keyValue2 = (dynamic) o2;
            return DeepEqualsCommonType(keyValue1.Key, keyValue2.Key)
                   && DeepEqualsCommonType(keyValue1.Value, keyValue2.Value);
        }


        public static IEnumerable<(Type memberType, object value)> GetPropFieldValue(object obj)
        {
            var type = obj.GetType();
            var props = type.GetProperties().Select(x => (x.PropertyType, x.GetValue(obj)));
            var fields = type.GetFields().Select(x => (x.FieldType, x.GetValue(obj)));
            return fields.Concat(props).Where(x => x.Item1 != null || x.Item2 != null);
        }
    }

    #endregion
}