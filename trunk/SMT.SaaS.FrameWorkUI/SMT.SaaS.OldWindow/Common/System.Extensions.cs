// (c) Copyright Vitor de Souza (sincorde.com)
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

# region Using Directives

using System.Linq;
using System.Collections.Generic;

# endregion

namespace System
{
    public static class Extensions
    {
        # region string.Replicate()

        public static string Replicate(this string source, int count)
        {
            string ret = string.Empty;

            for (int i = 0; i < count; i++)
            {
                ret += source;
            }

            return ret;
        }

        # endregion

        # region IEnumerable<T>.ForEach()

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
        }

        # endregion

        # region In<T>()

        public static bool In<T>(this T predicate, params T[] values)
        {
            return In<T>(predicate, values.ToList());
        }

        public static bool In<T>(this T predicate, IEnumerable<T> values)
        {
            return values.Any(t => t.Equals(predicate));
        }

        # endregion

        # region IsBetweenOrEquals<T>()

        public static bool IsBetween<T>(this T predicate, T start, T end) where T : IComparable
        {
            return predicate.CompareTo(start) > 0 && predicate.CompareTo(end) < 0;
        }
        public static bool IsBetweenOrEquals<T>(this T predicate, T start, T end) where T : IComparable
        {
            return predicate.CompareTo(start) >= 0 && predicate.CompareTo(end) <= 0;
        }

        # endregion

        # region IsNull()

        public static bool IsNull(this object predicate)
        {
            return predicate == null;
        }

        # endregion

        # region HasValue()

        public static bool HasValue(this object predicate)
        {
            return !predicate.IsNull();
        }

        # endregion

        # region IsNotNull()

        public static bool IsNotNull(this object predicate)
        {
            return !predicate.IsNull();
        }

        # endregion

        # region ValidateIsNotNull()

        public static void ValidateIsNotNull(this object predicate)
        {
            predicate.ValidateIsNotNull(string.Empty);
        }
        public static void ValidateIsNotNull(this object predicate, string message)
        {
            if (predicate.IsNull())
            {
                throw new NullReferenceException(message);
            }
        }

        # endregion

        # region Shuffle

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var array = source.ToArray();

            var random = new Random((int)DateTime.Now.Ticks);

            var set = Enumerable.Range(0, array.Length)
                .Select(i =>
                    new
                    {
                        Index = i,
                        Ponderator = random.NextDouble()
                    }).ToArray();

            var result = set
                .OrderBy(s => s.Ponderator)
                .Select(s => array[s.Index]);

            return result;
        }

        # endregion

        public static void TryDo<T>(this T predicate, Action action)
        {
            if (predicate.IsNotNull())
            {
                action();
            }
        }
        public static void TryDo<T>(this T predicate, Action<T> action)
        {
            if (predicate.IsNotNull())
            {
                action(predicate);
            }
        }
    }
}
