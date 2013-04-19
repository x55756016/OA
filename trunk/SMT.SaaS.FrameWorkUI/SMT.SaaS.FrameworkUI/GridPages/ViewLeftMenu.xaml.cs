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
    public partial class ViewLeftMenu : UserControl
    {
        public ViewLeftMenu()
        {
            InitializeComponent();
        }

        public TreeView CTreeView
        {
            get { return this.TreeViews; }
        }

        public Expander CExpander
        {
            get { return this.controlsToolkitTUV; }
        }
    }
}
