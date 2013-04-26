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
    /// ����һ��ӿڣ�����Ӧ�ó���ģ��ļ������ʼ���ṩ����
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// �� <see cref="ModuleCatalog"/>��<see cref="InitializationMode"/>Ϊ<see cref="InitializationMode.WhenAvailable"/>ʱ��ʼ��ģ�顣
        /// </summary>
        void Run();

        /// <summary>
        /// ʹ��ģ�����Ƽ��ء���ʼ�� <see cref="ModuleCatalog"/>�е�ģ�顣
        /// </summary>
        /// <param name="moduleName">����ʵ����ģ�������.</param>
        void LoadModule(string moduleName);       

        /// <summary>
        /// ģ�������й����д������¼���
        /// </summary>
        event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        /// <summary>
        /// ģ�������ɻ����ʱ��ʱ�������¼���
        /// </summary>
        event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;
    }
}
