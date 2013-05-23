
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: XAML中的共享数据
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

using System;
namespace SMT.SAAS.Main.CurrentContext
{
    /// <summary>
    /// XAML数据上下文。
    /// </summary>
    public static class AppContext
    {
        //当前UI SHELL
        public static Host AppHost;

        //登录状态
        public static bool LogOff = false;

        public static bool IsMenuOpen = false;

        public static bool IsLoadingCompleted = false;

        /// <summary>
        /// 标识权限是否需要升级
        /// </summary>
        public static bool IsPermUpdate = false;

        public static void SystemMessage(string message)
        {
            string strTime=DateTime.Now.Hour.ToString()+":"+DateTime.Now.Minute.ToString()+":"
                +DateTime.Now.Second.ToString()+":"+DateTime.Now.Millisecond.ToString();
            if (AppHost == null) return;
            AppHost.txtSystemMessage.Text += System.Environment.NewLine + message +" 记录时间："+ strTime;
        }


        /// <summary>
        /// 显示系统错误信息框
        /// </summary>
        public static void ShowSystemMessageText()
        {
            if (AppHost == null) return;
            AppHost.txtSystemMessage.Height = 20;
            AppHost.systemMessageArea.Height = 25;
            AppHost.systemMessageArea.Visibility = System.Windows.Visibility.Visible;
        }


    }
}
