using SMT.SAAS.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为快捷方式的数据访问提供接口规范
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.DAL.Interface
{
	/// <summary>
	/// 为快捷方式的数据访问提供接口规范
	/// </summary>
    public interface IShortCutDAL: IBaseDAL<ShortCut>
	{
		/// <summary>
		/// 根据传入的用户ID标识，获取当前用户的所有快捷方式，快捷方式包含系统默认快捷方式。
		/// </summary>
		/// <param name="userID">用户ID，用户的唯一标识，用与查询用户的快捷方式。</param>
        List<ShortCut> GetShortCutListByUser(string userID);

        bool AddByUser(ShortCut entity, string userid);

        bool DeleteShortCutByUser(string shortCutID, string userID);
	}
}

