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
    /// ��������ģ����صĽӿڡ�
    /// </summary>
    public interface IModuleTypeLoader
    {
        /// <summary>
        /// ����ǰ��������Ҫ���� <paramref name="moduleInfo"/>����ôҪ�� <see cref="ModuleInfo.Ref"/> ������֤��
        /// </summary>
        /// <param name="moduleInfo">
        /// ģ����Ҫ���м������͡�
        /// </param>
        /// <returns>
        /// ����ǰ�������ܹ��Ҵ�ģ�鷵��<see langword="true"/>�����򷵻� <see langword="false"/>
        /// </returns>
        bool CanLoadModuleType(ModuleInfo moduleInfo);      

        /// <summary>
        /// ���� <paramref name="moduleInfo"/>��
        /// </summary>
        /// <param name="moduleInfo">
        /// ģ����Ҫ�м��ص����͡�
        /// </param>
        void LoadModuleType(ModuleInfo moduleInfo);
   
        /// <summary>
        /// ģ���ں�̨���صĽ��̷��͸���ʱ������
        /// </summary>
        event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        /// <summary>
        /// ģ����سɹ���ʧ��ʱ������
        /// </summary>
        /// <remarks>
        /// </remarks>
        event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;
    }
}
