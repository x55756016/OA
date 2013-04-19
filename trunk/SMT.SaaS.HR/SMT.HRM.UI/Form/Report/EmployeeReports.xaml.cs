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
using SMT.Saas.Tools.PersonnelWS;
using SMT.HRM.UI.Form.Personnel;
using SMT.HRM.UI;

namespace SMT.HRM.UI.Report
{
    public partial class EmployeeReports : BasePage, IClient
    {
        PersonnelServiceClient client;

        public EmployeeReports()
        {
            InitializeComponent();
            client = new PersonnelServiceClient();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
        #region 员工报表        
        private void EmployeeReports_Click(object sender, RoutedEventArgs e)
        {
            Form.Personnel.EmployeeReports form = new SMT.HRM.UI.Form.Personnel.EmployeeReports();
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        #endregion

        #region 异动报表
        private void BtnChangeReports_Click(object sender, RoutedEventArgs e)
        {
            //LoadData();
        }
        #endregion

        #region 员工离职报表
        
        
        private void BtnLeftOfficeReports_Click(object sender, RoutedEventArgs e)
        {
            //LoadData();
        }
        #endregion

        #region 员工统计报表


        private void BtnCollectReports_Click(object sender, RoutedEventArgs e)
        {
            ReportInfos form = new ReportInfos(ReportsType.Reports.EmployeeCollect);            
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 员工薪酬明细报表

        private void BtnPensionReports_Click(object sender, RoutedEventArgs e)
        {
            ReportInfos form = new ReportInfos(ReportsType.Reports.EmployeePension);            
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 员工结构统计报表
        
        private void BtnTructReports_Click(object sender, RoutedEventArgs e)
        {
            ReportInfos form = new ReportInfos(ReportsType.Reports.EmployeeTruct);
            form.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
        
    }
}
