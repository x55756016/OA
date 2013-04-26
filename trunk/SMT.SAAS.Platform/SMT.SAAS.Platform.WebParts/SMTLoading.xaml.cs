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

namespace SMT.SAAS.Platform.WebParts
{
    public partial class SMTLoading : UserControl
    {
        public event EventHandler Cancel;
        public SMTLoading()
        {
            InitializeComponent();
        }

        private void MeasureSpinCenter()
        {
            //btnCancel.SetValue(Canvas.LeftProperty, ActualWidth / 2 - btnCancel.ActualWidth / 2);
            //btnCancel.SetValue(Canvas.TopProperty, ActualHeight / 2 - btnCancel.ActualHeight / 2);
        }
        /// <summary>
        /// 开始进度条动画
        /// </summary>
        public void Start()
        {
            MeasureSpinCenter();
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

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            MeasureSpinCenter();
        }

        private void ProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MeasureSpinCenter();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel != null)
            {
                Cancel(this, EventArgs.Empty);
                LayoutRoot.Visibility = Visibility.Collapsed;
            }
        }
    }
}
