using System;

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 当模块与模块之间出现依赖循环将会触发此异常。<br/>
    /// </summary>
    public partial class CyclicDependencyFoundException : ModularityException
    {
        /// <summary>
        /// 初始化<see cref="CyclicDependencyFoundException"/>的实例。<br/>
        /// </summary>
        public CyclicDependencyFoundException() : base() { }

        /// <summary>
        /// 根据给定错误消息，初始化<see cref="CyclicDependencyFoundException"/>的实例。<br/>
        /// </summary>
        /// <param name="message">
        /// 描述错误的消息。<br/>
        /// </param>
        public CyclicDependencyFoundException(string message) : base(message) { }

        /// <summary>
        ///  根据给定错误信息和内部异常，初始化<see cref="CyclicDependencyFoundException"/>的实例。<br/>
        /// </summary>
        /// <param name="message">
        /// 解释异常原因的错误消息。<br/>
        /// </param>
        /// <param name="innerException">
        /// 导致当前异常的异常。<br/>
        /// </param>
        public CyclicDependencyFoundException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        ///  根据给定错误信息和内部异常，初始化<see cref="CyclicDependencyFoundException"/>的实例。<br/>
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="message">解释异常原因的错误消息.</param>
        /// <param name="innerException">
        /// 导致当前异常的异常
        /// </param>
        public CyclicDependencyFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}