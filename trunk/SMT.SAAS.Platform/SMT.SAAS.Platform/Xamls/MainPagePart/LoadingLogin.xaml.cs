//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 登录界面-加载动画
// 完成日期：2011-05-19 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
using System.Windows.Controls;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    /// <summary>
    /// 登录界面-加载动画
    /// </summary>
    public partial class LoadingLogin : UserControl
    {
        /// <summary>
        /// 创建一个LoadingLogin的新实例
        /// </summary>
        public LoadingLogin()
        {
            InitializeComponent();
            SBRolling.Begin();
        }
    }
}
