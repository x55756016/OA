using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.Logging
{
    /// <summary>
    /// 默认日志实现类，提供对系统日志的访问和操作支持
    /// </summary>
    public class Logger : ILoggerFacade
    {
        private volatile static Logger uniqueInstance;
		private static object syncRoot = new Object();
        

        private Logger()
		{ 
        }

        public static Logger Current{

            get {
                return GetInstance();
            }
        }
        private static Logger GetInstance()
		{
			if(uniqueInstance == null)
			{
				lock (syncRoot)
				{
					if(uniqueInstance == null)
					{
                        uniqueInstance = new Logger();
					}
				}
			}
			return uniqueInstance;
		}

        private static List<Log> _systemLogs = new List<Log>();

        /// <summary>
        /// 根据参数写入一个日志
        /// </summary>
        /// <param name="messager">日志消息内容</param>
        /// <param name="category">日志类型</param>
        /// <param name="priority">日志优先级</param>
        public void Log(string messager, Category category, Priority priority)
        {
            Log("Platform", "SystemLog", messager, category, priority);
        }

        /// <summary>
        /// 根据参数写入一个日志
        /// </summary>
        /// <param name="appName">系统名称</param>
        /// <param name="moduleName">模块名称</param>
        /// <param name="messager">日志消息内容</param>
        /// <param name="category">消息类别</param>
        /// <param name="priority">消息优先级</param>
        public void Log(string appName, string moduleName, string messager, Category category, Priority priority)
        {
            Log("", appName, moduleName, messager, null, category, priority);
        }

        /// <summary>
        /// 根据参数写入一个日志
        /// </summary>
        /// <param name="appName">系统名称</param>
        /// <param name="moduleName">模块名称</param>
        /// <param name="messager">日志消息内容</param>
        /// <param name="exception">异常信息</param>
        /// <param name="category">消息类别</param>
        /// <param name="priority">消息优先级</param>
        public void Log(string logCode, string appName, string moduleName, string messager, Exception exception, Category category, Priority priority)
        {
            if (_systemLogs.Count >= 50)
                _systemLogs.Clear();

            _systemLogs.Add(new Logging.Log()
            {
                AppCode = appName,
                ModuleCode = moduleName,
                Exception = exception,
                LogCode = logCode,
                Message = messager,
                Category = category,
                Priority = priority,
                CreateTime = DateTime.Now
            });
        }

        /// <summary>
        /// 清楚消息记录器中的内容。
        /// </summary>
        public void Clear()
        {
            _systemLogs.Clear();
        }

        /// <summary>
        /// 消息反馈，将指定消息发送到服务器。
        /// </summary>
        /// <param name="Log">要发送的日志信息。</param>
        public void Feedback(Log Log)
        {
             
        }

        /// <summary>
        /// 当前包含的日志总数
        /// </summary>
        public int Count
        {
            get
            {
                return _systemLogs.Count;
            }
        }

        /// <summary>
        /// 当前的所有日志
        /// </summary>
        public List<Log> Logs
        {
            get
            {
                return _systemLogs;
            }
        }
    }
}

