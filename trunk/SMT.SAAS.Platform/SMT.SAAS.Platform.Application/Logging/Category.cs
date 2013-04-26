
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 定义<see cref="Log"/>类别
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Logging
{
	/// <summary>
	/// 定义<see cref="Log"/>类别
	/// </summary>
	public enum Category : int
	{
		/// <summary>
		/// 调试信息
		/// </summary>
		Debug,
		/// <summary>
		/// 异常信息
		/// </summary>
		Exception,
		/// <summary>
		/// 消息信息
		/// </summary>
		Info,
		/// <summary>
		/// 警告信息
		/// </summary>
		Warn,
	}
}
