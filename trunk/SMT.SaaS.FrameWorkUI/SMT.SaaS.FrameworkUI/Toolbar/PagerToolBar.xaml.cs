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
    public partial class PagerToolBar : UserControl
    {
        public PagerToolBar()
        {
            InitializeComponent();
        }

        public Button ShowButtonBack(string TBack)
        {
            ButtonBack.Visibility = Visibility.Visible;
            this.TBBack.Text = TBack;
            return ButtonBack;
        }
        public Button ShowButtonNext(string TNext)
        {
            ButtonNext.Visibility = Visibility.Visible;
            this.TBNext.Text = TNext;
            return ButtonNext;
        }
        public Button ShowButtonComplete(string TComplete)
        {
            ButtonComplete.Visibility = Visibility.Visible;
            this.TBComplete.Text = TComplete;
            return ButtonComplete;
        }
    }
}
