
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
    /// ����һ�����ڽ�ģ���ʼ����Ӧ�ó���ķ���
    /// </summary>
    public interface IModuleInitializer
    {
        /// <summary>
        /// ��ʼ��ָ��ģ��
        /// </summary>
        /// <param name="moduleInfo">
        /// Ҫ��ʼ����ģ�顣
        /// </param>
        void Initialize(ModuleInfo moduleInfo);
    }
}
