using System.Windows;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 主容器
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
      
namespace SMT.SAAS.Platform
{
    /// <summary>
    /// 主容器，应用程序仅有一个对象。
    /// </summary>
    public interface IHost
    {
        void SetRootVisual(UIElement Content);

        void LoginOff();
    }
}
