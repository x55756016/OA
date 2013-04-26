using System;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 找不到请求的<see cref="InitializationMode.OnDemand"/>  <see cref="IModule"/>时引发的异常。
    /// </summary>
    public partial class ModuleNotFoundException : ModularityException
    {
        /// <summary>
        /// 初始化<see cref="ModuleNotFoundException" /> 的实例。
        /// </summary>
        public ModuleNotFoundException()
        {
        }

        /// <summary>
        /// 根据给定错误消息，初始化<see cref="ModuleNotFoundException" /> 的实例。
        /// </summary>
        /// <param name="message">
        /// 描述错误的消息。
        /// </param>
        public ModuleNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 根据给定异常和错误消息，初始化<see cref="ModuleNotFoundException" /> 的实例。
        /// </summary>
        /// <param name="message">
        /// 描述错误的消息。
        /// </param>
        /// <param name="innerException">
        /// 内部异常。
        /// </param>
        public ModuleNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 根据给定的模块名称和错误消息，初始化<see cref="ModuleNotFoundException" /> 的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 描述错误产生的原因。
        /// </param>
        public ModuleNotFoundException(string moduleName, string message)
            : base(moduleName, message)
        {
        }

        /// <summary>
        /// 根据给定的模块名称、内部异常和错误消息，初始化<see cref="ModuleNotFoundException" /> 的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 描述错误原因的消息。
        /// </param>
        /// <param name="innerException">
        /// 产生当前异常的内部异常，
        /// 如果没有特定的内部异常则为 <see langword="null"/>。
        /// </param>
        public ModuleNotFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
