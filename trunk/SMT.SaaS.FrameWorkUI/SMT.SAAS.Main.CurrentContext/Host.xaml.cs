using System;
using System.Windows.Controls;
using System.Windows.Browser;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 提供界面实现的宿主,主外壳。承载MainPage
// 完成日期：2011-05-16 
// 版    本：V1.0 
// 作    者：王玲 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Main.CurrentContext
{
    /// <summary>
    /// 系统容器外壳，用于承载MainPage、Login等
    /// 在加载顺序上为RootVisual->Host->Login 成功后->MainPage
    /// </summary>
    public partial class Host : UserControl
    {
        public Host()
        {
            InitializeComponent();
            //初始化系统继承引导程序
            //ViewModel.Context.Host = this;
            //if (ViewModel.Context.Managed == null)
            //    ViewModel.Context.Managed = new ViewModel.Managed();
        }

        public void setroot(UserControl userControl)
        {
            //默认设置登录页面。
            SetRoot(userControl);
        }

        /// <summary>
        /// 设置外壳容器的内容。
        /// </summary>
        /// <param name="userControl">当前要设置的容器。</param>
        public void SetRoot(UserControl userControl)
        {
            Root.Children.Clear();
            Root.Children.Add(userControl);
        }

        /// <summary>
        /// 设置外壳容器的内容。
        /// </summary>
        /// <param name="Content">当前要设置的容器。</param>
        public void SetRootVisual(System.Windows.UIElement Content)
        {
            UserControl uc = Content as UserControl;
            if (uc == null)
                throw new Exception("Content is not a UserControl");

            SetRoot(uc);
        }

        /// <summary>
        /// 系统注销方法，注销后将返回到登录界面。
        /// 注销动作由MainPage触发，将情况用户的所有相关数据。
        /// </summary>
        public void LoginOff()
        {
            AppContext.LogOff = false;

            string strHost = SMT.SAAS.Main.CurrentContext.Common.HostIP;
            string strUrl = "http://" + strHost + "/";

            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri(strUrl);
            wd.Navigate(uri);
            //Common.AppContext.LogOff = false;
            //ViewModel.Context.Host.SetRootVisual(new LoginPart.Login());
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            systemMessageArea.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            txtSystemMessage.Height = 100;
            txtSystemMessage.Visibility = System.Windows.Visibility.Visible;
            systemMessageArea.Height = 100;
            systemMessageArea.Visibility = System.Windows.Visibility.Visible;
        }


    }
}
