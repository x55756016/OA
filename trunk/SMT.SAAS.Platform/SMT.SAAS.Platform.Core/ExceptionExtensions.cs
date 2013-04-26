using System;
using System.Collections.Generic;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: Description
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// Exception����ṩ��չ������
    /// 
    /// Class that provides extension methods for the Exception class. These extension methods provide
    /// a mechanism for developers to get more easily to the root cause of an exception, especially in combination with 
    /// DI-containers such as Unity. 
    /// </summary>
    public static class ExceptionExtensions
    {
        private static List<Type> frameworkExceptionTypes = new List<Type>();

        /// <summary>
        /// ע����������ɿ��������쳣��
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
        /// �鿴�����ڲ�<paramref name="exception"/>�Ĳ������ҳ����п��ܲ����쳣�ĸ���ԭ��
        /// ������̽���������ע����쳣�����͡�
        /// </summary>
        /// <param name="exception">
        /// ���ڼ����쳣����
        /// </param>
        /// <returns>
        /// �������п��ܲ����쳣��ԭ�������������쳣���Ὣ����쳣���ء�
        /// </returns>
        /// <remarks>
        /// ������������ܰٷְ��ҵ����⣬�����Ը��߿�����Ա���п��ܵĵط���ָ�����ҵķ���
        /// �Ⲣû���滻�ڲ��쳣����Ϊ�п��ܻ��ڸ���ʵ��ԭ��
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
