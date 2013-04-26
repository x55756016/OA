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
    /// 当<see cref="ModuleManager.ModuleTypeLoaders"/> 没有注册<see cref="IModuleTypeLoader"/>
    /// 时产生的异常。
    /// </summary>
    public partial class ModuleTypeLoaderNotFoundException : ModularityException
    {
        /// <summary>
        /// 初始化<see cref="ModuleTypeLoaderNotFoundException"/>的实例。
        /// </summary>
        public ModuleTypeLoaderNotFoundException()
        {
        }

        /// <summary>
        /// 根据给定的错误消息，初始化<see cref="ModuleTypeLoaderNotFoundException"/>的实例。
        /// </summary>
        /// <param name="message">
        /// 描述错误原因的消息。
        /// </param>
        public ModuleTypeLoaderNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 根据给定的错误消息和内部异常，初始化<see cref="ModuleTypeLoaderNotFoundException"/>的实例。
        /// Initializes a new instance of the <see cref="ModuleTypeLoaderNotFoundException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// 描述错误原因的消息。
        /// </param>
        /// <param name="innerException">
        /// 内部异常。
        /// </param>
        public ModuleTypeLoaderNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 根据给定的模块名称、错误消息和内部异常，初始化<see cref="ModuleTypeLoaderNotFoundException"/>的实例。
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
        public ModuleTypeLoaderNotFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
