using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 枚举，描述系统中WEBPART、快捷方式、系统配置类型
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Model
{
	/// <summary>
	/// 枚举，描述系统中WEBPART、快捷方式、系统配置类型
	/// </summary>
#if !SILVERLIGHT
    [DataContract]
#endif
    public enum SysType : int
	{
		/// <summary>
		/// 系统默认级别
		/// </summary>
		System,
		/// <summary>
		/// 管理员级别
		/// </summary>
		Admin,
		/// <summary>
		/// 用户级别
		/// </summary>
		User,
	}
}
