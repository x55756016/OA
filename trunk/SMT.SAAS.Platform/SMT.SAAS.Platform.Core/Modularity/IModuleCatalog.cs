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
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ģ��Ŀ¼��ΪModuleManager�ṩģ����Ϣ��
    /// ModuleCatalog����Ӧ�ó���ʹ�õ������/ģ�� ��Ϣ��
    /// ���/ģ����Ϣ������ModuleInfo�У��� ���ơ����͡�λ�õ����ݡ�
    /// </summary>
    public interface IModuleCatalog
    {
        /// <summary>
        /// ��ȡ<see cref="ModuleCatalog"/>Ŀ¼���������<see cref="ModuleInfo"/>ģ����Ϣ��
        /// </summary>
        IEnumerable<ModuleInfo> Modules { get; }

        /// <summary>
        /// ���ݸ�����ģ����Ϣ<paramref name="moduleInfo"/>������������������ģ����Ϣ<see cref="ModuleInfo"/>�б�
        /// </summary>
        /// <param name="moduleInfo">
        /// ��Ҫ��������ģ���<see cref="ModuleInfo"/>
        /// </param>
        /// <returns>
        /// <paramref name="moduleInfo"/>������<see cref="ModuleInfo"/>�б�
        /// </returns>
        IEnumerable<ModuleInfo> GetDependentModules(ModuleInfo moduleInfo);


        /// <summary>
        /// ���ݸ�����ģ���б�<paramref name="modules"/>���ҵ�����������ģ�������ģ�鼯�ϣ�
        /// �����������е�ģ�鶼��������ģ�顣
        /// </summary>
        /// <param name="modules">
        /// ���ڲ�������ģ���ģ�鼯�ϡ�
        /// </param>
        /// <returns>
        /// ��ȡ����ģ����Ϣ������ģ���б��Լ�������ģ�顣
        /// </returns>
        IEnumerable<ModuleInfo> CompleteListWithDependencies(IEnumerable<ModuleInfo> modules);

        /// <summary>
        /// ��ʼ��<see cref="ModuleCatalog"/>�����ڼ��ػ���֤ģ�顣
        /// </summary>
        void Initialize();

        /// <summary>
        /// ��ģ��Ŀ¼<see cref="ModuleCatalog"/>���һ��<see cref="ModuleInfo"/>��
        /// </summary>
        /// <param name="moduleInfo">
        /// Ҫ��ӵ�<see cref="ModuleInfo"/>��
        /// </param>
        void AddModule(ModuleInfo moduleInfo);
    }
}
