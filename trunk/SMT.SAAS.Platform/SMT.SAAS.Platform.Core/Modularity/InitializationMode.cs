
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
    /// 模块的初始化方式
    /// </summary>
    public enum InitializationMode
    {
        /// <summary>
        /// 此类型表示，当应用程序启动的时候将进行初始化，默认值。
        /// </summary>
        WhenAvailable,

        /// <summary>
        /// 此类型表示，当在需要请求此模块的时候进行初始化，而非项目启动阶段。
        /// </summary>
        OnDemand
    }
}
