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
    /// ��<see cref="ModuleManager.ModuleTypeLoaders"/> û��ע��<see cref="IModuleTypeLoader"/>
    /// ʱ�������쳣��
    /// </summary>
    public partial class ModuleTypeLoaderNotFoundException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="ModuleTypeLoaderNotFoundException"/>��ʵ����
        /// </summary>
        public ModuleTypeLoaderNotFoundException()
        {
        }

        /// <summary>
        /// ���ݸ����Ĵ�����Ϣ����ʼ��<see cref="ModuleTypeLoaderNotFoundException"/>��ʵ����
        /// </summary>
        /// <param name="message">
        /// ��������ԭ�����Ϣ��
        /// </param>
        public ModuleTypeLoaderNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// ���ݸ����Ĵ�����Ϣ���ڲ��쳣����ʼ��<see cref="ModuleTypeLoaderNotFoundException"/>��ʵ����
        /// Initializes a new instance of the <see cref="ModuleTypeLoaderNotFoundException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// ��������ԭ�����Ϣ��
        /// </param>
        /// <param name="innerException">
        /// �ڲ��쳣��
        /// </param>
        public ModuleTypeLoaderNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ�����ơ�������Ϣ���ڲ��쳣����ʼ��<see cref="ModuleTypeLoaderNotFoundException"/>��ʵ����
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
        public ModuleTypeLoaderNotFoundException(string moduleName, string message, Exception innerException)
            : base(moduleName, message, innerException)
        {
        }
    }
}
