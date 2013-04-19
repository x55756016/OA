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
using System.Windows.Media.Imaging;
using System.Windows.Browser;
using System.Windows.Navigation;

using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.FrameworkUI
{
    public partial class LeftMenu : UserControl
    {
        /// <summary>
        /// 获取当前Menu
        /// </summary>
        public StackPanel MenuRoot
        {
            get { return this.menuRoot; }

        }

        public LeftMenu()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LeftMenu_Loaded);
        }

        void LeftMenu_Loaded(object sender, RoutedEventArgs e)
        {

        }


        public Accordion CAccordion
        {
            set { toolkitacc = value; }
            get { return toolkitacc; }
        }
    }
}
