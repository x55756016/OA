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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.HRM.UI;

namespace SMT.HRM.UI.Report
{
    public partial class ReportInfos : System.Windows.Controls.Window, IClient
    {
        #region 定义变量
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        SMTLoading loadbar = new SMTLoading();
        string sType = "", sValue = "";
        // bool ispaging = true;
        public int iLandYear = DateTime.Now.Year;
        public int iLandMonthStart = DateTime.Now.Month;
        public int iLandMonthEnd = DateTime.Now.Month;
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;

        ReportsType.Reports ReportType;
        #endregion

        public ReportInfos()
        {
            InitializeComponent();
        }
        public ReportInfos(ReportsType.Reports Types)
        {
            InitializeComponent();
            nuYear.Value = DateTime.Now.Year;
            nuMonth.Value = DateTime.Now.Month;
            ReportType = Types;
            if (ReportType == ReportsType.Reports.EmployeeCollect)
            {
                this.TitleContent = Utility.GetResourceStr("员工统计信息报表");
            }
            else if (ReportType == ReportsType.Reports.EmployeePension)
            {
                this.TitleContent = Utility.GetResourceStr("薪酬明细报表");
            }
            else if (ReportType == ReportsType.Reports.EmployeeTruct)
            {
                this.TitleContent = Utility.GetResourceStr("员工结构统计报表");
            }
            
            initEvents();
            
        }
        void initEvents()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            T_HR_COMPANY companyInit = new T_HR_COMPANY();
            companyInit.COMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            companyInit.CNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            sType = "Company";
            sValue = companyInit.COMPANYID;
            lkSelectObj.DisplayMemberPath = "CNAME";
            lkSelectObj.DataContext = companyInit;
            
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            // ispaging = false;
            dialog.Filter = "MS csv Files|*.xls";
            dialog.FilterIndex = 1;
            result = dialog.ShowDialog();
            if (result.Value == true)
            {

                
                string filter = "";
                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

                if (sType == "Company")
                {
                    filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                if (sType == "Department")
                {
                    filter += " OWNERDEPARTMENTID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                if (sType == "Post")
                {
                    filter += " OWNERPOSTID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }
                if (string.IsNullOrEmpty(sType))
                {
                    sType = "Company";
                    sValue = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    filter += " OWNERCOMPANYID==@" + paras.Count().ToString();
                    paras.Add(sValue);
                }

                

                loadbar.Start();
                
            }

        }

        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Company;

            lookup.TitleContent = Utility.GetResourceStr("ORGANNAME");

            lookup.SelectedClick += (obj, ev) =>
            {
                lkSelectObj.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY)
                {
                    lkSelectObj.DisplayMemberPath = "CNAME";
                    sType = "Company";
                    sValue = (lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY).COMPANYID;
                }
                
                
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        
        #region IClient
        public void ClosedWCFClient()
        {
            personClient.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}

