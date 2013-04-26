using System;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ��ģ�����������쳣�Ļ���
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ��ģ�����������쳣�Ļ��ࡣ
    /// </summary>
    public partial class ModularityException : Exception
    {
        /// <summary>
        /// ��ʼ��<see cref="ModularityException"/>�����ʵ����
        /// </summary>
        public ModularityException()
            : this(null)
        {
        }

        /// <summary>
        /// ��ʼ��<see cref="ModularityException"/>�����ʵ����
        /// </summary>
        /// <param name="message">
        /// �쳣��Ϣ��
        /// </param>
        public ModularityException(string message)
            : this(null, message)
        {
        }

        /// <summary>
        /// ��ʼ��<see cref="ModularityException"/>�����ʵ����
        /// </summary>
        /// <param name="message">
        /// �쳣��Ϣ��
        /// </param>
        /// <param name="innerException">
        /// �ڲ��쳣��Ϣ��
        /// </param>
        public ModularityException(string message, Exception innerException)
            : this(null, message, innerException)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ��ʹ�����Ϣ����ʼ��һ���쳣��
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ��������Ϊ�β����쳣��ԭ��
        /// </param>
        public ModularityException(string moduleName, string message)
            : this(moduleName, message, null)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ�顢������Ϣ���ڲ��쳣����ʼ��һ���쳣��
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ��������Ϊ�β����쳣��ԭ��
        /// </param>
        /// <param name="innerException">
        /// </param>
        public ModularityException(string moduleName, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ModuleName = moduleName;
        }

        /// <summary>
        /// ��ȡ�����õ�ǰ�쳣�漰����ģ�����ơ�
        /// </summary>
        /// <value>
        /// ģ�����ơ�
        /// </value>
        public string ModuleName { get; set; }
    }
}
