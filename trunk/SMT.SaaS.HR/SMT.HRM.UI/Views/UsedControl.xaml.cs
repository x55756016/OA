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

namespace SMT.HRM.UI
{
    public partial class UsedControl : Page
    {
        public UsedControl()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
		
		private void Back_MLBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void Back_ME(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Back.Height = Back.Height + 20;
            Back.Width = Back.Width + 20;
        }

        private void Back_ML(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Back.Width = Back.Width - 20;
            Back.Height = Back.Height - 20;
        }
    }
}
