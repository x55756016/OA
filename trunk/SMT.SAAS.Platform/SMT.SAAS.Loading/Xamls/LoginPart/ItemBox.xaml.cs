using System;
using System.Windows;
using System.Windows.Controls;
  
namespace SMT.SAAS.Platform.Xamls.LoginPart
{
    /// <summary>
    /// 登录界面中的切换控件
    /// </summary>
    public partial class ItemBox : UserControl
    {
        /// <summary>
        /// 创建一个ItemBox的新实例。
        /// </summary>
        public ItemBox()
        {
            InitializeComponent();
            this.ChangePwdForm.SetparentWindow(this);
            this.loginForm.SetparentWindow(this);
            ShowLayoutRoot.LayoutUpdated += this.GoToInitialState;
        }

        private void GoToInitialState(object sender, EventArgs eventArgs)
        {
            VisualStateManager.GoToState(this, "Normal", false);
            ShowLayoutRoot.LayoutUpdated -= this.GoToInitialState;
        }

        public virtual void NavigateToLogin()
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }

        public virtual void NavigateToRegistration()
        {
            VisualStateManager.GoToState(this, "Flipped", true);
        }
    }
}
