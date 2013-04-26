using System.Collections.Generic;
using System.Collections.ObjectModel;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// Collection类的扩展方法
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 添加一组元素到集合中。
        /// </summary>
        /// <typeparam name="T">
        /// 集合中的对象类型。
        /// </typeparam>
        /// <param name="collection">
        /// 用于添加元素的集合。
        /// </param>
        /// <param name="items">
        /// 元素列表，将添加到指定集合中
        /// </param>
        /// <returns>
        /// 返回添加后的集合。
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// 若<paramref name="collection"/>和 <paramref name="items"/>为<see langword="null"/>。将触发<see cref="System.ArgumentNullException"/> 异常。
        /// </exception>
        public static Collection<T> AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new System.ArgumentNullException("collection");
            if (items == null) throw new System.ArgumentNullException("items");

            foreach (var each in items)
            {
                collection.Add(each);
            }

            return collection;
        }
    }
}
