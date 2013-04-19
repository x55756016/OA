using System;
using System.Windows;
using System.Windows.Controls;


namespace SMT.SaaS.FrameworkUI
{
    public partial class SMTProgressBar
    {
        public event EventHandler Cancel;

        public SMTProgressBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始进度条动画
        /// </summary>
        public void Start()
        {
            Storyboardeclipse.Begin();
        }

        /// <summary>
        /// 结束进度条动画
        /// </summary>
        public void Stop()
        {
            Storyboardeclipse.Stop();
        }
    }
}
