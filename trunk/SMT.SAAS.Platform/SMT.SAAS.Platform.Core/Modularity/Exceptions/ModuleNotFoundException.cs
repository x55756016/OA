using System;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: Description
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// �Ҳ��������<see cref="InitializationMode.OnDemand"/>  <see cref="IModule"/>ʱ�������쳣��
    /// </summary>
    public partial class ModuleNotFoundException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="ModuleNotFoundException" /> ��ʵ����
        /// </summary>
        public ModuleNotFoundException()
        {
        }

        /// <summary>
        /// ���ݸ���������Ϣ����ʼ��<see cref="ModuleNotFoundException" /> ��ʵ����
        /// </summary>
        /// <param name="message">
        /// �����������Ϣ��
        /// </param>
        public ModuleNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// ���ݸ����쳣�ʹ�����Ϣ����ʼ��<see cref="ModuleNotFoundException" /> ��ʵ����
        /// </summary>
        /// <param name="message">
        /// �����������Ϣ��
        /// </param>
        /// <param name="innerException">
        /// �ڲ��쳣��
        /// </param>
        public ModuleNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ�����ƺʹ�����Ϣ����ʼ��<see cref="ModuleNotFoundException" /> ��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// �������������ԭ��
        /// </param>
        public ModuleNotFoundException(string moduleName, string message)
            : base(moduleName, message)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ�����ơ��ڲ��쳣�ʹ�����Ϣ����ʼ��<see cref="ModuleNotFoundException" /> ��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ��������ԭ�����Ϣ��
        /// </param>
        /// <param name="innerException">
        /// ������ǰ�쳣���ڲ��쳣��
        /// ���û���ض����ڲ��쳣��Ϊ <see langword="null"/>��
        /// </param>
        public ModuleNotFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
