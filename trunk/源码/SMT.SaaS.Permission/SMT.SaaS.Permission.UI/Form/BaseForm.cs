using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SaaS.Permission.UI
{
    public class BaseForm : ChildWindow
    {
        private MainPage currentMain;

        public BaseForm()
        {
            Grid grid = Application.Current.RootVisual as Grid;
            if (grid != null && grid.Children.Count > 0)
            {
                currentMain = grid.Children[0] as MainPage;
            }
        }
        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        /// <summary>
        /// 显示进度条
        /// </summary>
        public void ShowProgress()
        {
            currentMain.ShowWaitingControl();
        }
        /// <summary>
        /// 隐藏进度条
        /// </summary>
        public void HidenProgress()
        {
            currentMain.HideWaitingControl();
        }
    }
}
