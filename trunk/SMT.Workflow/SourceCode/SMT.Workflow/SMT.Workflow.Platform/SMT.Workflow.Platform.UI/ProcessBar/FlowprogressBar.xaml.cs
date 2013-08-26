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

namespace SMT.Workflow.Platform.UI.ProcessBar
{
    public partial class FlowprogressBar : UserControl
    {
        public FlowprogressBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始进度条动画
        /// </summary>
        public void Start()
        {
            spinStBoard.Begin();

            LayoutRoot.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 结束进度条动画
        /// </summary>
        public void Stop()
        {
            spinStBoard.Stop();

            LayoutRoot.Visibility = Visibility.Collapsed;
        }
    }
}
