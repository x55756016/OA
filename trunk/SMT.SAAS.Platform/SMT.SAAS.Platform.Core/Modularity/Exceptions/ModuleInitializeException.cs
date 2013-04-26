using System;
using System.Globalization;

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
    /// <see cref="IModuleInitializer"/>派生类加载模块失败时产生的异常。
    /// </summary>
    public partial class ModuleInitializeException : ModularityException
    {
        /// <summary>
        /// 初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        public ModuleInitializeException()
        {
        }

        /// <summary>
        /// 根据给定错误信息， 初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        /// <param name="message">异常消息.</param>
        public ModuleInitializeException(string message) : base(message)
        {
        }

        /// <summary>
        /// 根据给定的错误消息和内部异常，初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        /// <param name="message">异常消息.</param>
        /// <param name="innerException">内部异常.</param>
        public ModuleInitializeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 根据给定的模块信息、错误异常，初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="moduleAssembly">
        /// 模块所属程序集名称。
        /// </param>
        /// <param name="message">
        /// 异常消息。
        /// </param>
        public ModuleInitializeException(string moduleName, string moduleAssembly, string message)
            : this(moduleName, message, moduleAssembly, null)
        {
        }

        /// <summary>
        /// 根据模块信息、错误信息、内部异常，初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="moduleAssembly">
        /// 模块所属程序集名称。
        /// </param>
        /// <param name="message">
        /// 描述产生异常的错误消息。
        /// </param>
        /// <param name="innerException">
        /// 产生当前异常的内部异常，
        /// 如果没有特定的内部异常则为 <see langword="null"/>。
        /// </param>
        public ModuleInitializeException(string moduleName, string moduleAssembly, string message, Exception innerException)
            : base(
                moduleName,
                String.Format(CultureInfo.CurrentCulture, Resources.FailedToLoadModule, moduleName, moduleAssembly, message),
                innerException)
        {
        }

        /// <summary>
        /// 根据模块名称、错误消息和内部异常，初始化<see cref="ModuleInitializeException"/>的实例。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 描述产生异常的错误消息。
        /// </param>
        /// <param name="innerException">
        /// 产生当前异常的内部异常，如果没有特定的内部异常则为 <see langword="null"/>。
        /// </param>
        public ModuleInitializeException(string moduleName, string message, Exception innerException)
            : base(
                moduleName,
                String.Format(CultureInfo.CurrentCulture, Resources.FailedToLoadModuleNoAssemblyInfo, moduleName, message),
                innerException)
        {
        }
    }
}