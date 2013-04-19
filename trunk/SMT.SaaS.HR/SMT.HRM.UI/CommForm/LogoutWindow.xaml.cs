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

namespace SMT.HRM.UI
{
    public partial class LogoutWindow : ChildWindow
    {
        public LogoutWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ChildWindow chol = new ChildWindow();
            chol.Height = 2400;
            chol.Width = 2300;
            //SL_Project_UI.MainPage mp = new SL_Project_UI.MainPage();
            //mp.Visibility = Visibility.Collapsed;
            chol.Show();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

