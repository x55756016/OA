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
    public partial class ViewTitle : UserControl
    {
        public ViewTitle()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ViewTitle_Loaded);
        }

        //判断是否隐藏当前标题显示
        void ViewTitle_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Controls.Window.IsShowtitle == false)
            {
                this.BorderViewTitle.Height = 30.0;
            }
            else
            {
                this.BorderViewTitle.Visibility = Visibility.Collapsed;
            }
        }
        public TextBlock TextTitle
        {
            get { return this.titlename; }
        }

    }
}
