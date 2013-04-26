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
    /// <see cref="IModuleInitializer"/>���������ģ��ʧ��ʱ�������쳣��
    /// </summary>
    public partial class ModuleInitializeException : ModularityException
    {
        /// <summary>
        /// ��ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        public ModuleInitializeException()
        {
        }

        /// <summary>
        /// ���ݸ���������Ϣ�� ��ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        /// <param name="message">�쳣��Ϣ.</param>
        public ModuleInitializeException(string message) : base(message)
        {
        }

        /// <summary>
        /// ���ݸ����Ĵ�����Ϣ���ڲ��쳣����ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        /// <param name="message">�쳣��Ϣ.</param>
        /// <param name="innerException">�ڲ��쳣.</param>
        public ModuleInitializeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// ���ݸ�����ģ����Ϣ�������쳣����ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="moduleAssembly">
        /// ģ�������������ơ�
        /// </param>
        /// <param name="message">
        /// �쳣��Ϣ��
        /// </param>
        public ModuleInitializeException(string moduleName, string moduleAssembly, string message)
            : this(moduleName, message, moduleAssembly, null)
        {
        }

        /// <summary>
        /// ����ģ����Ϣ��������Ϣ���ڲ��쳣����ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="moduleAssembly">
        /// ģ�������������ơ�
        /// </param>
        /// <param name="message">
        /// ���������쳣�Ĵ�����Ϣ��
        /// </param>
        /// <param name="innerException">
        /// ������ǰ�쳣���ڲ��쳣��
        /// ���û���ض����ڲ��쳣��Ϊ <see langword="null"/>��
        /// </param>
        public ModuleInitializeException(string moduleName, string moduleAssembly, string message, Exception innerException)
            : base(
                moduleName,
                String.Format(CultureInfo.CurrentCulture, Resources.FailedToLoadModule, moduleName, moduleAssembly, message),
                innerException)
        {
        }

        /// <summary>
        /// ����ģ�����ơ�������Ϣ���ڲ��쳣����ʼ��<see cref="ModuleInitializeException"/>��ʵ����
        /// </summary>
        /// <param name="moduleName">
        /// ģ�����ơ�
        /// </param>
        /// <param name="message">
        /// ���������쳣�Ĵ�����Ϣ��
        /// </param>
        /// <param name="innerException">
        /// ������ǰ�쳣���ڲ��쳣�����û���ض����ڲ��쳣��Ϊ <see langword="null"/>��
        /// </param>
        public ModuleInitializeException(string moduleName, string message, Exception innerException)
            : base(
                moduleName,
                String.Format(CultureInfo.CurrentCulture, Resources.FailedToLoadModuleNoAssemblyInfo, moduleName, message),
                innerException)
        {
        }
    }
}