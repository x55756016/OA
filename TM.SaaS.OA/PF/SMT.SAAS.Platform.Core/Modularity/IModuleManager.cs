using System;

      
namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 定义一组接口，用于应用程序模块的检索或初始化提供服务。
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// 当 <see cref="ModuleCatalog"/>的<see cref="InitializationMode"/>为<see cref="InitializationMode.WhenAvailable"/>时初始化模块。
        /// </summary>
        //void Run();

        /// <summary>
        /// 使用模块名称加载、初始化 <see cref="ModuleCatalog"/>中的模块。
        /// </summary>
        /// <param name="moduleName">请求、实例化模块的名称.</param>
        //void LoadModule(string moduleName);       

        /// <summary>
        /// 模块下载中过程中触发的事件。
        /// </summary>
        //event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        /// <summary>
        /// 模块加载完成或加载时候时触发的事件。
        /// </summary>
        //event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;
    }
}
