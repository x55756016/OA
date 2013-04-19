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

namespace SMT.SaaS.FrameworkUI
{
    public partial class ComfirmBox : ChildWindow
    {

        /// <summary>
        /// 获取设置当前确认对话框信息内容
        /// </summary>
        public TextBlock MessageTextBox
        {
            get { return this.txtMessage; }
            set { this.txtMessage = value; }
        }

        /// <summary>
        /// 获取设置当前确认对话框信息内容
        /// </summary>
        public Button ButtonOK
        {
            get { return this.OKButton; }
        }

        public ComfirmBox()
        {
            InitializeComponent();            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

