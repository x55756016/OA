using System.Collections.Generic;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 模块目录，为ModuleManager提供模块信息。
    /// ModuleCatalog保存应用程序使用到的组件/模块 信息。
    /// 组件/模块信息包含在ModuleInfo中，有 名称、类型、位置等数据。
    /// </summary>
    public interface IModuleCatalog
    {
        /// <summary>
        /// 获取<see cref="ModuleCatalog"/>目录里面的所有<see cref="ModuleInfo"/>模块信息。
        /// </summary>
        IEnumerable<ModuleInfo> Modules { get; }

        /// <summary>
        /// 根据给定的模块信息<paramref name="moduleInfo"/>，查找其依赖的其他模块信息<see cref="ModuleInfo"/>列表。
        /// </summary>
        /// <param name="moduleInfo">
        /// 需要检索依赖模块的<see cref="ModuleInfo"/>
        /// </param>
        /// <returns>
        /// <paramref name="moduleInfo"/>的依赖<see cref="ModuleInfo"/>列表。
        /// </returns>
        IEnumerable<ModuleInfo> GetDependentModules(ModuleInfo moduleInfo);


        /// <summary>
        /// 根据给定的模块列表<paramref name="modules"/>，找到其里面所有模块的依赖模块集合，
        /// 不过并非所有的模块都依赖其他模块。
        /// </summary>
        /// <param name="modules">
        /// 用于查找依赖模块的模块集合。
        /// </param>
        /// <returns>
        /// 获取所有模块信息，包含模块列表以及其依赖模块。
        /// </returns>
        IEnumerable<ModuleInfo> CompleteListWithDependencies(IEnumerable<ModuleInfo> modules);

        /// <summary>
        /// 初始化<see cref="ModuleCatalog"/>，用于加载或验证模块。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 向模块目录<see cref="ModuleCatalog"/>添加一个<see cref="ModuleInfo"/>。
        /// </summary>
        /// <param name="moduleInfo">
        /// 要添加的<see cref="ModuleInfo"/>。
        /// </param>
        void AddModule(ModuleInfo moduleInfo);
    }
}
