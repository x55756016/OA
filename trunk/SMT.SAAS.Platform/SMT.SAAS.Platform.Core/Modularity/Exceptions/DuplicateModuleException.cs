using System;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 模块信息重复在同一目录中定义时触发的异常
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 模块信息重复在同一目录中定义时触发的异常。
    /// </summary>
    public partial class DuplicateModuleException : ModularityException
    {
        /// <summary>
        /// 初始化<see cref="DuplicateModuleException"/>的实例
        /// </summary>
        public DuplicateModuleException()
        {
        }

        /// <summary>
        /// 初始化<see cref="DuplicateModuleException"/>的实例。
        /// </summary>
        /// <param name="message">
        /// 异常消息。
        /// </param>
        public DuplicateModuleException(string message) : base(message)
        {
        }

        /// <summary>
        /// 初始化<see cref="DuplicateModuleException"/>的实例。
        /// </summary>
        /// <param name="message">
        /// 异常消息
        /// </param>
        /// <param name="innerException">
        /// 内部异常。
        /// The inner exception.</param>
        public DuplicateModuleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// 根据给定错误消息，初始化<see cref="DuplicateModuleException" />的实例。
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="message">解释异常原因的错误消息</param>
        public DuplicateModuleException(string moduleName, string message)
            : base(moduleName, message)
        {
        }

        /// <summary>
        /// 根据给定错误消息和触发当前异常的内部异常信息，初始化<see cref="DuplicateModuleException" />的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 解释异常原因的错误消息。
        /// param>
        /// <param name="innerException">
        /// </param>
        public DuplicateModuleException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
