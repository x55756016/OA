using System.Collections.Generic;
using System.Collections.ObjectModel;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: Description
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// Collection�����չ����
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// ���һ��Ԫ�ص������С�
        /// </summary>
        /// <typeparam name="T">
        /// �����еĶ������͡�
        /// </typeparam>
        /// <param name="collection">
        /// �������Ԫ�صļ��ϡ�
        /// </param>
        /// <param name="items">
        /// Ԫ���б�����ӵ�ָ��������
        /// </param>
        /// <returns>
        /// ������Ӻ�ļ��ϡ�
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// ��<paramref name="collection"/>�� <paramref name="items"/>Ϊ<see langword="null"/>��������<see cref="System.ArgumentNullException"/> �쳣��
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
