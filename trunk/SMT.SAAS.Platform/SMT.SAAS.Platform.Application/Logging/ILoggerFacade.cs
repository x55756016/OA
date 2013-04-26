using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 系统消息接口，用于定义获消息、记录消息、统计消息、反馈消息。
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Logging
{
	/// <summary>
	/// 系统消息接口，用于定义获消息、记录消息、统计消息、反馈消息。
	/// </summary>
    public interface ILoggerFacade 
	{
		/// <summary>
		/// 消息总条数。
		/// </summary>
		int Count { get;}

		/// <summary>
		/// 所有消息
		/// </summary>
		List<Log> Logs { get;}

		/// <summary>
		/// 根据参数写入一个日志
		/// </summary>
		/// <param name="messager">日志消息内容</param>
		void Log(string messager, Category category, Priority priority);

		/// <summary>
		/// 根据参数写入一个日志
		/// </summary>
		/// <param name="appName">系统名称</param>
		/// <param name="moduleName">模块名称</param>
		/// <param name="messager">日志消息内容</param>
		/// <param name="category">消息类别</param>
		/// <param name="priority">消息优先级</param>
        void Log(string appName, string moduleName, string messager, Category category, Priority priority);

		/// <summary>
		/// 根据参数写入一个日志
		/// </summary>
        /// <param name="logCode">日志编号</param>
		/// <param name="appName">系统名称</param>
		/// <param name="moduleName">模块名称</param>
		/// <param name="messager">日志消息内容</param>
		/// <param name="exception">异常信息</param>
		/// <param name="category">消息类别</param>
		/// <param name="priority">消息优先级</param>
        void Log(string logCode,string appName, string moduleName, string messager, Exception exception, Category category, Priority priority);

		/// <summary>
		/// 清楚消息记录器中的内容。
		/// </summary>
		void Clear();

		/// <summary>
		/// 消息反馈，将指定消息发送到服务器。
		/// </summary>
		void Feedback(Log Log);

	}
}

