using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: DAL接口，定义数据操作层的基础接口
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.DAL.Interface
{
	/// <summary>
	/// DAL接口，定义数据操作层的基础接口
	/// </summary>
	/// <typeparam name="T">泛型，数据实体。</typeparam>
	public interface IBaseDAL<T> 
	{
		/// <summary>
		/// 添加一个新的<see cref="T"/>到数据库中。
		/// </summary>
		/// <param name="entity">将要添加到数据库的<see cerf="T"/>。</param>
		bool Add(T entity);

		/// <summary>
		/// 更新<see cerf="T">,更新前要判断是否存在此实体。
		/// </summary>
		/// <param name="entity">将要更新的<see cerf="T">,更新前要判断是否存在此实体。</param>
		bool Update(T entity);

		/// <summary>
		/// 从数据库中删除给定的<see cref="T"/>。
		/// </summary>
		bool Delete(T entity);

		/// <summary>
		/// 根据传入的标识，从数据库中获取匹配的<see cref="T"/>，并返回，若没有找到则返回NULL。
		/// </summary>
		/// <param name="key">唯一标识<see cerf="T"/>的主键。用于查找匹配的结果。</param>
		T GetEntityByKey(string key);

		/// <summary>
		/// 根据传入的标识，判断数据库中是否有匹配的<see cref="T"/>，若存在则返回True，若不存在则返回False。
		/// </summary>
		bool IsExists(string key);

		/// <summary>
		/// 分页返回<see cerf="T">集合。根据给定的分页参数查询当前页的数据集合并返回。若发送异常则返回NULL。
		/// </summary>
		/// <param name="index">页码。</param>
		/// <param name="size">单页的条数。</param>
		/// <param name="count">总共页数。</param>
		/// <param name="sort">排序参数</param>
		List<T> GetListByPager(int index, int size, ref int count, string sort);

	}
}

