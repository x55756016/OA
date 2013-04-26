using System;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 定义ModuleInfoGroup的扩展方法
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 定义 <see cref="ModuleInfoGroup"/>的扩展方法。
    /// </summary>
    public static class ModuleInfoGroupExtensions
    {
        /// <summary>
        /// 添加一个静态引用的模块添加到模块信息组。
        /// </summary>
        /// <param name="moduleInfoGroup">
        /// 将要添加的信息组
        /// </param>
        /// <param name="moduleName">
        /// 模块名称
        /// </param>
        /// <param name="moduleType">
        /// 模块类型，类型需要实现<see cref="IModule"/>。
        /// </param>
        /// <param name="dependsOn">
        /// 当前模块依赖的模块名称。
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
