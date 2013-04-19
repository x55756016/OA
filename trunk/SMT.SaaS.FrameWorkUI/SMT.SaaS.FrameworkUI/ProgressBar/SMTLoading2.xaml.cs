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

namespace SMT.SaaS.FrameworkUI.ProgressBar
{
    public partial class SMTLoading2 : UserControl
    {
        public SMTLoading2()
        {
            InitializeComponent();
        }
        public void setloadingMessage( string msg)
        {
            txtloadingMessage.Text = msg;
        }
        /// <summary>
        /// 开始进度条动画
        /// </summary>
        public void Start()
        {
            LoadingBallS.Begin();
            LayoutRoot.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 结束进度条动画
        /// </summary>
        public void Stop()
        {
            LoadingBallS.Stop();
            LayoutRoot.Visibility = Visibility.Collapsed;
        }
    }
}
