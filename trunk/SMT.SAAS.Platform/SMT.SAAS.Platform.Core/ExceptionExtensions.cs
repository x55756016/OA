using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// Exception类的提供扩展方法。
    /// 
    /// Class that provides extension methods for the Exception class. These extension methods provide
    /// a mechanism for developers to get more easily to the root cause of an exception, especially in combination with 
    /// DI-containers such as Unity. 
    /// </summary>
    public static class ExceptionExtensions
    {
        private static List<Type> frameworkExceptionTypes = new List<Type>();

        /// <summary>
        /// 注册这个类型由框架引起的异常。
        /// Register the type of an Exception that is thrown by the framework. The <see cref="GetRootException"/> method uses
        /// this list of Exception types to find out if something has gone wrong.  
        /// </summary>
        /// <param name="frameworkExceptionType">The type of exception to register.</param>
        public static void RegisterFrameworkExceptionType(Type frameworkExceptionType)
        {
            if (frameworkExceptionType == null) throw new ArgumentNullException("frameworkExceptionType");

            if (!frameworkExceptionTypes.Contains(frameworkExceptionType))
                frameworkExceptionTypes.Add(frameworkExceptionType);
        }


        /// <summary>
        /// Determines whether the exception type is already registered using the <see cref="RegisterFrameworkExceptionType"/> 
        /// method
        /// </summary>
        /// <param name="frameworkExceptionType">The type of framework exception to find.</param>
        /// <returns>
        /// 	<c>true</c> if the exception type is already registered; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFrameworkExceptionRegistered(Type frameworkExceptionType)
        {
            return frameworkExceptionTypes.Contains(frameworkExceptionType);
        }

        /// <summary>
        /// 查看所有内部<paramref name="exception"/>的参数，找出最有可能产生异常的根本原因。
        /// 这个过程将跳过所有注册过异常的类型。
        /// </summary>
        /// <param name="exception">
        /// 用于检测的异常对象。
        /// </param>
        /// <returns>
        /// 查找最有可能产生异常的原因，如果发现这个异常，会将这个异常返回。
        /// </returns>
        /// <remarks>
        /// 这个方法并不能百分百找到问题，但可以告诉开发人员最有可能的地方或指明查找的方向。
        /// 这并没有替换内部异常，因为有可能会掩盖真实的原因。
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We have to catch exception. This method is used in exception handling code, so it must not fail.")]
        public static Exception GetRootException(this Exception exception)
        {
            Exception rootException = exception;
            try
            {
                while (true)
                {
                    if (rootException == null)
                    {
                        rootException = exception;
                        break;
                    }

                    if (!IsFrameworkException(rootException))
                    {
                        break;
                    }
                    rootException = rootException.InnerException;
                }
            }
            catch (Exception)
            {
                rootException = exception;
            }
            return rootException;
        }

        private static bool IsFrameworkException(Exception exception)
        {
            bool isFrameworkException = frameworkExceptionTypes.Contains(exception.GetType());
            bool childIsFrameworkException = false;

            if (exception.InnerException != null)
            {
                childIsFrameworkException = frameworkExceptionTypes.Contains(exception.InnerException.GetType());
            }

            return isFrameworkException || childIsFrameworkException;
        }
    }
}
