
using System.Linq;
using System.Collections.Generic;
namespace System
{
    /// <summary>
    /// 应用程序扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 复制字符串
        /// </summary>
        /// <param name="source">复制对象</param>
        /// <param name="count">负责次数</param>
        /// <returns>返回复制后结果</returns>
        public static string Replicate(this string source, int count)
        {
            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            for (int i = 0; i < count; i++)
            {
                ret.Append(source);
            }
            return ret.ToString();
        }
        /// <summary>
        /// 封装Aciton委托,循环枚举集合将集合中的方法参数传入Action委托
        /// </summary>
        /// <typeparam name="T">方法参数类型</typeparam>
        /// <param name="source">传入参数集合</param>
        /// <param name="action">委托事件</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
            {
                action(element);
            }
        }
        #region 判断对象是否在给定的对象集合中
        /// <summary>
        /// 对象是否在集合中
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">要判断对象</param>
        /// <param name="values">对象集合</param>
        /// <returns>返回结果,如果包含则返回True,否则返回False</returns>
        public static bool In<T>(this T predicate, params T[] values)
        {
            return In<T>(predicate, values.ToList());
        }
        /// <summary>
        /// 对象是否在集合中
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">要判断对象</param>
        /// <param name="values">对象集合</param>
        /// <returns>返回结果,如果包含则返回True,否则返回False</returns>
        public static bool In<T>(this T predicate, IEnumerable<T> values)
        {
            return values.Any(t => t.Equals(predicate));
        }
        #endregion
        #region 判断对象是否介于给定的对象之间
        /// <summary>
        /// 对象是否介于起始和结束对象之间,对象必须为实现了IComparable接口
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">需要判断的对象</param>
        /// <param name="start">起始对象</param>
        /// <param name="end">结束对象</param>
        /// <returns>结果,若在对象</returns>
        public static bool IsBetween<T>(this T predicate, T start, T end) where T : IComparable
        {
            return predicate.CompareTo(start) > 0 && predicate.CompareTo(end) < 0;
        }
        /// <summary>
        /// 对象是否介于起始和结束对象之间或等于某一对象,对象必须为实现了IComparable接口
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">需要判断的对象</param>
        /// <param name="start">起始对象</param>
        /// <param name="end">结束对象</param>
        /// <returns>结果,若在对象</returns>
        public static bool IsBetweenOrEquals<T>(this T predicate, T start, T end) where T : IComparable
        {
            return predicate.CompareTo(start) >= 0 && predicate.CompareTo(end) <= 0;
        }
        #endregion
        #region 判断对象是否为NULL,或验证对象是否存在值
        /// <summary>
        /// 对象是否为NULL
        /// </summary>
        /// <param name="predicate">判断对象</param>
        /// <returns>返回结果</returns>
        public static bool IsNull(this object predicate)
        {
            return predicate == null;
        }
        /// <summary>
        /// 对象是否有值
        /// </summary>
        /// <param name="predicate">判断对象</param>
        /// <returns>结果</returns>
        public static bool HasValue(this object predicate)
        {
            return !predicate.IsNull();
        }
        /// <summary>
        /// 判断对象是否不为NULL值
        /// </summary>
        /// <param name="predicate">对象</param>
        /// <returns>结果</returns>
        public static bool IsNotNull(this object predicate)
        {
            return !predicate.IsNull();
        }
        /// <summary>
        /// 验证对象是否为NULL，若为NLL则抛出对象为NLL值异常
        /// </summary>
        /// <param name="predicate">对象</param>
        public static void ValidateIsNotNull(this object predicate)
        {
            predicate.ValidateIsNotNull(string.Empty);
        }
        /// <summary>
        /// 验证对象是否为NULL，若为NLL,则根据传入信息抛出对象为NLL值异常
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="message"></param>
        public static void ValidateIsNotNull(this object predicate, string message)
        {
            if (predicate.IsNull())
            {
                throw new NullReferenceException(message);
            }
        }
        #endregion
        # region 根据集合对集合元素进行随机排列
        /// <summary>
        /// 根据集合对集合元素进行随即排列
        /// </summary>
        /// <typeparam name="T">集合中的对象类型</typeparam>
        /// <param name="source">对象集合</param>
        /// <returns>随机排列后的集合</returns>
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
        #region 根据对象调用委托
     
        /// <summary>
        /// 根据对象调用Action委托
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">传入的对象</param>
        /// <param name="action">执行的委托</param>
        public static void TryDo<T>(this T predicate, Action action)
        {
            if (predicate.IsNotNull())
            {
                action();
            }
        }
        /// <summary>
        /// 根据对象执行委托
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="predicate">传入的对象</param>
        /// <param name="action">根据对象声明的委托</param>
        public static void TryDo<T>(this T predicate, Action<T> action)
        {
            if (predicate.IsNotNull())
            {
                action(predicate);
            }
        }

        #endregion
    }
}
