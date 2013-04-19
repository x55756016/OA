using System.Windows;
using System.Windows.Controls;

namespace SMT.SaaS.FrameworkUI
{
    public partial class SMTLoading : UserControl
    {
         public SMTLoading()
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
