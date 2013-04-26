using System;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 由模块所引发的异常的基类
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 由模块所引发的异常的基类。
    /// </summary>
    public partial class ModularityException : Exception
    {
        /// <summary>
        /// 初始化<see cref="ModularityException"/>类的新实例。
        /// </summary>
        public ModularityException()
            : this(null)
        {
        }

        /// <summary>
        /// 初始化<see cref="ModularityException"/>类的新实例。
        /// </summary>
        /// <param name="message">
        /// 异常信息。
        /// </param>
        public ModularityException(string message)
            : this(null, message)
        {
        }

        /// <summary>
        /// 初始化<see cref="ModularityException"/>类的新实例。
        /// </summary>
        /// <param name="message">
        /// 异常信息。
        /// </param>
        /// <param name="innerException">
        /// 内部异常信息。
        /// </param>
        public ModularityException(string message, Exception innerException)
            : this(null, message, innerException)
        {
        }

        /// <summary>
        /// 根据给定的模块和错误信息，初始化一个异常。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 用于描述为何产生异常的原因。
        /// </param>
        public ModularityException(string moduleName, string message)
            : this(moduleName, message, null)
        {
        }

        /// <summary>
        /// 根据给定的模块、错误信息和内部异常，初始化一个异常。
        /// </summary>
        /// <param name="moduleName">
        /// 模块名称。
        /// </param>
        /// <param name="message">
        /// 用于描述为何产生异常的原因。
        /// </param>
        /// <param name="innerException">
        /// </param>
        public ModularityException(string moduleName, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ModuleName = moduleName;
        }

        /// <summary>
        /// 获取或设置当前异常涉及到的模块名称。
        /// </summary>
        /// <value>
        /// 模块名称。
        /// </value>
        public string ModuleName { get; set; }
    }
}
