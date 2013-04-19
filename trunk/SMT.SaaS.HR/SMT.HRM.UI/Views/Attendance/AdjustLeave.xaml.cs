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

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AdjustLeave : BasePage
    {
        public AdjustLeave()
        {
            InitializeComponent();
        }

        // 当用户导航到此网页时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgAskOffList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            
        }
    }
}
