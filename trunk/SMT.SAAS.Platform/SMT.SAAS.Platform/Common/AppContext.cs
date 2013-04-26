using SMT.SAAS.Platform.Xamls;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: XAML中的共享数据
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform.Common
{
    /// <summary>
    /// XAML数据上下文。
    /// </summary>
    public static class AppContext
    {
        //当前UI SHELL
        //public static Host Host;

        //登录状态
        public static bool LogOff = false;

        public static bool IsMenuOpen = false;
    }
}
