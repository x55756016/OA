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
using SMT.SaaS.FrameworkUI.Helper;

namespace SMT.SaaS.FrameworkUI
{
    public partial class BottomFooter : UserControl
    {
        public BottomFooter()
        {
            InitializeComponent();
           
            footer.Text = string.Format("版权所有：神州通投资集团有限公司   当前时间：{0}", System.DateTime.Now.ToString("d"));
        }
		
    }
}
