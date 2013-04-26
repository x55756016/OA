using System;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供模块加载完成或加载失败后的事件参数
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 提供模块加载完成或加载失败后的信息。
    /// </summary>
    public class LoadModuleCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化<see cref="LoadModuleCompletedEventArgs"/>的新实例。
        /// </summary>
        /// <param name="moduleInfo">模块信息。</param>
        /// <param name="error">产生的异常。</param>
        public LoadModuleCompletedEventArgs(ModuleInfo moduleInfo, Exception error)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }

            this.ModuleInfo = moduleInfo;
            this.Error = error;
        }

        /// <summary>
        /// 获取模块信息。
        /// </summary>
        /// <value>The module info.</value>
        public ModuleInfo ModuleInfo { get; private set; }

        /// <summary>
        /// 获取产生的异常。
        /// </summary>
        /// <value>
        /// 任何可能产生的异常，否则设置为NULL。
        /// </value>
        public Exception Error { get; private set; }

        /// <summary>
        ///  获取或设置一个值，该值指明错误是否已经由事件订阅对象处理。
        /// Gets or sets a value indicating whether the error has been handled by the event subscriber.
        /// </summary>
        /// <value>若事件已经处理为<c>true</c>; 否则为<c>false</c>.</value>
        /// <remarks>
        /// 若事件有错误，事件订阅对象未设置值为True，那么事件发布对象将会产生异常
        /// </remarks>
        public bool IsErrorHandled { get; set; }
    }
}
