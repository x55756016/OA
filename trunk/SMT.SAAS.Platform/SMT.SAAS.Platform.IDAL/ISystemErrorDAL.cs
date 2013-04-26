using SMT.SAAS.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为系统错误日志的数据访问提供接口规范
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.DAL.Interface
{
	/// <summary>
	/// 为系统错误日志的数据访问提供接口规范
	/// </summary>
    public interface ISystemErrorDAL: IBaseDAL<SystemError>
	{
		/// <summary>
		/// 根据给定的应用程序标识，返回与此应用程序相关联的所有运行错误。
		/// </summary>
		/// <param name="appKey">应用程序标识，根据应用程序的所有错误信息</param>
        List<SystemError> GetErrorListByAppKey(string appKey);

	}
}

