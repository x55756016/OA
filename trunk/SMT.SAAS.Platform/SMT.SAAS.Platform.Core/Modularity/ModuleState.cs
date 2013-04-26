
//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ����ģ��״̬�������ڼ���ģ����ʼ����ʱ�����
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ����<see cref="ModuleInfo"/>״̬�������ڼ���ģ����ʼ����ʱ����ʡ�
    /// </summary>
    public enum ModuleState
    {
        /// <summary>
        /// <see cref="ModuleInfo"/>��ʼ״̬��<see cref="ModuleInfo"/>�Ѷ��壬��û�м��أ���ʼ����������
        /// </summary>
        NotStarted,

        /// <summary>
        /// ������ǰģ��ĳ�������ʹ��<see cref="IModuleTypeLoader"/>���м��ء�
        /// <see cref="IModuleTypeLoader"/>. 
        /// </summary>
        LoadingTypes,

        /// <summary>
        /// ������ģ��ĳ����Ѿ����ڡ�
        /// ��˼����<see cref="IModule"/> ���Խ���ʵ�������ʼ����
        /// </summary>
        ReadyForInitialization,

        /// <summary>
        /// ģ������ʹ��<see cref="IModuleInitializer"/>��ʼ����
        /// </summary>
        Initializing,

        /// <summary>
        /// ģ���Ѿ���ʼ��������ʹ�á�
        /// </summary>
        Initialized
    }
}
