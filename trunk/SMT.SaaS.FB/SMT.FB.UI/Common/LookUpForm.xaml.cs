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
using SMT.SaaS.FrameworkUI;

namespace SMT.FB.UI.Common
{
    public partial class LookUpForm : ChildWindow
    {
        public LookUpForm()
        {
            InitializeComponent();
        }
        //private LookUp LookUp;
        //public LookUpForm(LookUp lookUp)
        //    : this()
        //{
        //    LookUp = lookUp;
        //}　

        public void AddControl(Control control)
        {
            this.LayoutRoot.Children.Add(control);
        }
    }
}

