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

namespace SMT.HRM.UI.Views.Performance
{
    public partial class GroupPersonList : BasePage
    {
        public GroupPersonList()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        SMTLoading loadbar = new SMTLoading();

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

        private void DtGrid_LoadingRow(object sender, RoutedEventArgs e)
        {
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
