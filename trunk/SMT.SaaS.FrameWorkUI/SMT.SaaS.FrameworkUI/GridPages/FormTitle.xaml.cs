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
    public partial class FormTitle : UserControl
    {
        public FormTitle()
        {
            InitializeComponent();   
        }

        public TextBlock TextTitle
        {
            get { return this.titlename; }
            set { titlename = value;    }
        }
       
    }
}
