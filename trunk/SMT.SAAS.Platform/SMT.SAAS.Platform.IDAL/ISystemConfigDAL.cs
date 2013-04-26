using SMT.SAAS.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为系统配置信息的数据访问提供接口规范
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.DAL.Interface
{
	/// <summary>
	/// 为系统配置信息的数据访问提供接口规范
	/// </summary>
    public interface ISystemConfigDAL: IBaseDAL<SystemConfig>
	{
		/// <summary>
		/// 根据给定的用户标识，返回此用户的个性化配置信息。
		/// </summary>
		/// <param name="userID">用户的唯一标识，用于根据标识查询出当前用户的配置信息</param>
        SystemConfig GetSystemConfigByUser(string userID);

	}
}

