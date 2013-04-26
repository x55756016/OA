using System;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ����ModuleInfoGroup����չ����
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ���� <see cref="ModuleInfoGroup"/>����չ������
    /// </summary>
    public static class ModuleInfoGroupExtensions
    {
        /// <summary>
        /// ���һ����̬���õ�ģ����ӵ�ģ����Ϣ�顣
        /// </summary>
        /// <param name="moduleInfoGroup">
        /// ��Ҫ��ӵ���Ϣ��
        /// </param>
        /// <param name="moduleName">
        /// ģ������
        /// </param>
        /// <param name="moduleType">
        /// ģ�����ͣ�������Ҫʵ��<see cref="IModule"/>��
        /// </param>
        /// <param name="dependsOn">
        /// ��ǰģ��������ģ�����ơ�
        /// </param>
        /// <returns>
        /// </returns>
        public static ModuleInfoGroup AddModule(
                    this ModuleInfoGroup moduleInfoGroup,
                    string moduleName,
                    Type moduleType,
                    params string[] dependsOn)
        {
            if (moduleType == null) throw new ArgumentNullException("moduleType");
            if (moduleInfoGroup == null) throw new ArgumentNullException("moduleInfoGroup");

            ModuleInfo moduleInfo = new ModuleInfo(moduleName, moduleType.AssemblyQualifiedName);
            moduleInfo.DependsOn.AddRange(dependsOn);
            moduleInfoGroup.Add(moduleInfo);
            return moduleInfoGroup;
        }

        /// <summary>
        /// Adds a new module that is statically referenced to the specified module info group.
        /// </summary>
        /// <param name="moduleInfoGroup">The group where to add the module info in.</param>
        /// <param name="moduleType">The type for the module. This type should be a descendant of <see cref="IModule"/>.</param>
        /// <param name="dependsOn">The names for the modules that this module depends on.</param>
        /// <returns>Returns the instance of the passed in module info group, to provide a fluid interface.</returns>
        /// <remarks>The name of the module will be the type name.</remarks>
        public static ModuleInfoGroup AddModule(
                    this ModuleInfoGroup moduleInfoGroup,
                    Type moduleType,
                    params string[] dependsOn)
        {
            if (moduleType == null) throw new ArgumentNullException("moduleType");
            return AddModule(moduleInfoGroup, moduleType.Name, moduleType, dependsOn);
        }
    }
}
