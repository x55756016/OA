using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace SMT.SaaS.FrameworkUI
{
    public partial class MasterPage : UserControl
    {
        #region "属性构造"
        /// <summary>
        /// 获取当前菜单栏
        /// </summary>
        public LeftMenu LeftMenus
        {
            get { return this.leftMenu; }
        }

        /// <summary>
        /// 获取当前进度条面板
        /// </summary>
        public Canvas ProgressPanel
        {
            get { return this.waitSpinnerHolder; }
        }
       /// <summary>
        /// 获取当前进度条面板
        /// </summary>
        //public Grid ContenxtPanle
        //{
        //    get { return this.contenxtPanle; }
        //}
        /// <summary>
        /// 获取当前进度条背景面板
        /// </summary>
        public Canvas ProgressBackPanel
        {
            get { return this.waitSpinnerBack; }
        }


        public MasterPage()
        {
            InitializeComponent();            
        }
        // 在 Frame 导航之后，请确保选中表示当前页的 HyperlinkButton
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //foreach (UIElement child in TopTitleBar.LinkCanvas.Children)
            //{
            //    HyperlinkButton hb = child as HyperlinkButton;
            //    if (hb != null && hb.NavigateUri != null)
            //    {
            //        if (hb.NavigateUri.ToString().Equals(e.Uri.ToString()))
            //        {
            //            VisualStateManager.GoToState(hb, "ActiveLink", true);
            //            this.ShowMyShow1.Begin();//播放动画
            //        }
            //        else
            //        {
            //            VisualStateManager.GoToState(hb, "InactiveLink", true);
            //            this.ShowMyShow1.Begin();//播放动画
            //        }
            //    }
            //}
        }

        // 如果导航过程中出现错误，则显示错误窗口
        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //e.Handled = true;
            //ChildWindow errorWin = new ErrorWindow(e.Uri);
            //errorWin.Show();
        }
               #endregion

        #region "进度条控制"
        private void ProgressBar_Cancel(object sender, EventArgs e)
        {
            if (CancelWaiting != null)
            {
                CancelWaiting(this, EventArgs.Empty);
            }
            HideWaitingControl();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Handles beforSpinnerShowShowBorder.Completed event.
        /// </summary>
        private void beforSpinnerShowShowBorder_Completed(object sender, EventArgs e)
        {
            spinnerBackShowBorder.Stop();
            spinnerShowBorder.Stop();
            spinnerBackShowBorder.Begin();
            spinnerShowBorder.Begin();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Occurs when waiting is canceled
        /// </summary>
        public event EventHandler CancelWaiting;

        /// <summary>
        /// Calculates wait spinner size and location.
        /// </summary>
        private void CalculatesSpinner()
        {
            waitSpinnerBack.Width = LayoutRoot.ActualWidth;
            waitSpinnerBack.Height = LayoutRoot.ActualHeight;
            waitSpinner.SetValue(Canvas.TopProperty, waitSpinnerBack.Height / 2 - waitSpinner.ActualHeight / 2 - 100);
            waitSpinner.SetValue(Canvas.LeftProperty, waitSpinnerBack.Width / 2 - waitSpinner.ActualWidth / 2 - 100);
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Hides waiting controls.
        /// </summary>
        public  void HideWaitingControl()
        {
            waitSpinner.Stop();
            waitSpinnerBack.Visibility = Visibility.Collapsed;
            waitSpinner.Visibility = Visibility.Collapsed;
            spinnerBackShowBorder.Stop();
            spinnerShowBorder.Stop();
            beforSpinnerShowShowBorder.Stop();
        }

        //----------------------------------------------------------------------------
        /// <summary>
        /// Shows waiting controls.
        /// </summary>
        public void ShowWaitingControl()
        {
            CalculatesSpinner();
            waitSpinnerBack.Visibility = Visibility.Visible;
            waitSpinner.Visibility = Visibility.Visible;
            waitSpinner.Start();
            beforSpinnerShowShowBorder.Begin();
        }
        #endregion 
    }
}
