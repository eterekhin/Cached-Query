using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HabrCacheQuery.ServiceCollectionExtensions
{
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
}