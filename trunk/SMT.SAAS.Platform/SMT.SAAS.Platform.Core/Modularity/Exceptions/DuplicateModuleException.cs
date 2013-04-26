using System;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ģ����Ϣ�ظ���ͬһĿ¼�ж���ʱ�������쳣
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ģ����Ϣ�ظ���ͬһĿ¼�ж���ʱ�������쳣��
    /// </summary>
    public partial class DuplicateModuleException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="DuplicateModuleException"/>��ʵ��
        /// </summary>
        public DuplicateModuleException()
        {
        }

        /// <summary>
        /// ��ʼ��<see cref="DuplicateModuleException"/>��ʵ����
        /// </summary>
        /// <param name="message">
        /// �쳣��Ϣ��
        /// </param>
        public DuplicateModuleException(string message) : base(message)
        {
        }

        /// <summary>
        /// ��ʼ��<see cref="DuplicateModuleException"/>��ʵ����
        /// </summary>
        /// <param name="message">
        /// �쳣��Ϣ
        /// </param>
        /// <param name="innerException">
        /// �ڲ��쳣��
        /// The inner exception.</param>
        public DuplicateModuleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// ���ݸ���������Ϣ����ʼ��<see cref="DuplicateModuleException" />��ʵ����
        /// </summary>
        /// <param name="moduleName">ģ������</param>
        /// <param name="message">�����쳣ԭ��Ĵ�����Ϣ</param>
        public DuplicateModuleException(string moduleName, string message)
            : base(moduleName, message)
        {
        }

        /// <summary>
        /// ���ݸ���������Ϣ�ʹ�����ǰ�쳣���ڲ��쳣��Ϣ����ʼ��<see cref="DuplicateModuleException" />��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// �����쳣ԭ��Ĵ�����Ϣ��
        /// param>
        /// <param name="innerException">
        /// </param>
        public DuplicateModuleException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
