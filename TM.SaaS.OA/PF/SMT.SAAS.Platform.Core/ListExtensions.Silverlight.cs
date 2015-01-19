using System;
using System.Collections.Generic;

namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// Ϊ<see cref="List{T}"/>�������չ������
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// ���ݸ����Ĺ���ɸѡ�������Ӽ������Ƴ���ƥ���Ԫ�ء�
        /// </summary>
        /// <typeparam name="T">
        /// �б�Ԫ�����͡�<see cref="Type"/> ��
        /// </typeparam>
        /// <param name="list">
        /// <see langword="this"/>.
        /// </param>
        /// <param name="filter">
        /// �������Ƴ�Ԫ��������ί�С�
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static void RemoveAll<T>(this List<T> list, Func<T, bool> filter)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (filter == null) throw new ArgumentNullException("filter");
            for (int i = 0; i < list.Count; i++)
            {
                if (filter(list[i]))
                {
                    list.Remove(list[i]);
                }
            }
        }
    }
}