using System;

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ��ģ����ģ��֮���������ѭ�����ᴥ�����쳣��<br/>
    /// </summary>
    public partial class CyclicDependencyFoundException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="CyclicDependencyFoundException"/>��ʵ����<br/>
        /// </summary>
        public CyclicDependencyFoundException() : base() { }

        /// <summary>
        /// ���ݸ���������Ϣ����ʼ��<see cref="CyclicDependencyFoundException"/>��ʵ����<br/>
        /// </summary>
        /// <param name="message">
        /// �����������Ϣ��<br/>
        /// </param>
        public CyclicDependencyFoundException(string message) : base(message) { }

        /// <summary>
        ///  ���ݸ���������Ϣ���ڲ��쳣����ʼ��<see cref="CyclicDependencyFoundException"/>��ʵ����<br/>
        /// </summary>
        /// <param name="message">
        /// �����쳣ԭ��Ĵ�����Ϣ��<br/>
        /// </param>
        /// <param name="innerException">
        /// ���µ�ǰ�쳣���쳣��<br/>
        /// </param>
        public CyclicDependencyFoundException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        ///  ���ݸ���������Ϣ���ڲ��쳣����ʼ��<see cref="CyclicDependencyFoundException"/>��ʵ����<br/>
        /// </summary>
        /// <param name="moduleName">ģ������</param>
        /// <param name="message">�����쳣ԭ��Ĵ�����Ϣ.</param>
        /// <param name="innerException">
        /// ���µ�ǰ�쳣���쳣
        /// </param>
        public CyclicDependencyFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}