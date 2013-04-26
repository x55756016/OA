
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
    /// 声明一个用于将模块初始化到应用程序的服务。
    /// </summary>
    public interface IModuleInitializer
    {
        /// <summary>
        /// 初始化指定模块
        /// </summary>
        /// <param name="moduleInfo">
        /// 要初始化的模块。
        /// </param>
        void Initialize(ModuleInfo moduleInfo);
    }
}
