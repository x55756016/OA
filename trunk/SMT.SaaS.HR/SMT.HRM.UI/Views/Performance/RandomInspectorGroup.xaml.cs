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
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PerformanceWS;

namespace SMT.HRM.UI.Views.Performance
{
    public partial class RandomInspectorGroup : BasePage
    {
        private PerformanceServiceClient client = new PerformanceServiceClient();
        SMTLoading loadbar = new SMTLoading();
        public RandomInspectorGroup()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
