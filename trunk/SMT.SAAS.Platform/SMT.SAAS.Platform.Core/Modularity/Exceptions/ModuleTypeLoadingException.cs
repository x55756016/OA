using System;
using System.Globalization;

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
    /// <see cref="IModuleManager"/>�ӿڵ�ʵ�ּ���ģ��ʧ��ʱ��Ҫ�׳����쳣��
    /// </summary>
    public partial class ModuleTypeLoadingException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="ModuleTypeLoadingException"/>��ʵ��
        /// </summary>
        public ModuleTypeLoadingException()
            : base()
        {
        }

        /// <summary>
        /// ����ָ���Ĵ�����Ϣ����ʼ��<see cref="ModuleTypeLoadingException"/>��ʵ��
        /// </summary>
        /// <param name="message">
        /// �����������Ϣ��
        /// </param>
        public ModuleTypeLoadingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// ����ָ���Ĵ�����Ϣ�Ͳ�����ǰ�쳣���ڲ��쳣����ʼ��<see cref="ModuleTypeLoadingException"/>��ʵ��
        /// </summary>
        /// <param name="message">
        /// ���͡���������ԭ�����Ϣ��
        /// </param>
        /// 
        /// </param>
        public ModuleTypeLoadingException(string message, Exception exception)
            : base(message, exception)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ����Ϣ�ʹ�����Ϣ����ʼ��<see cref="ModuleTypeLoadingException"/>��ʵ��
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ����˵����������ľ���ԭ��
        /// </param>
        public ModuleTypeLoadingException(string moduleName, string message)
            : this(moduleName, message, null)
        {
        }


        /// <summary>
        /// ���ݸ�����ģ����Ϣ�ʹ�����Ϣ���ڲ��쳣����ʼ��<see cref="ModuleTypeLoadingException"/>��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ����˵����������ľ���ԭ��
        /// </param>
        /// <param name="innerException">
        /// ������ǰ�쳣���ڲ��쳣��
        /// ���û���ض����ڲ��쳣��Ϊ <see langword="null"/>��
        /// </param>
        public ModuleTypeLoadingException(string moduleName, string message, Exception innerException)
            : base(moduleName, String.Format(CultureInfo.CurrentCulture, Resources.FailedToRetrieveModule, moduleName, message), innerException)
        {
        }
    }
}