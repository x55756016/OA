using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SMT.Saas.Tools.AttendanceWS;
using System.IO;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class PersonalLandDetails : BaseForm, IEntityEditor
    {
        public FormTypes FormType { set; get; }
        public int iLandYear { get; set; }
        public int iLandMonth { get; set; }
        public V_LandStatistic entLand { set; get; }
        public string sOrgType { get; set; }
        public string sOrgValue { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        Stream stream;

        AttendanceServiceClient clientAtt;

        public PersonalLandDetails()
        {
            InitializeComponent();
        }

        public PersonalLandDetails(FormTypes formType, int iYear, int iMonth, V_LandStatistic entRev)
        {
            InitializeComponent();
            FormType = formType;
            iLandYear = iYear;
            iLandMonth = iMonth;
            entLand = entRev;
            InitPage();
        }

        private void InitPage()
        {
            UnVisibleGridToolControl();

            if (entLand == null)
            {
                this.IsEnabled = false;
                return;
            }

            sOrgType = "Company";
            sOrgValue = entLand.OrganizationID;

            nuYear.Value = iLandYear;
            nuMonth.Value = iLandMonth;

            clientAtt = new AttendanceServiceClient();

            clientAtt.GetPersonalLandDetailListByMultSearchCompleted += new EventHandler<GetPersonalLandDetailListByMultSearchCompletedEventArgs>(clientAtt_GetPersonalLandDetailListByMultSearchCompleted);
            clientAtt.OutFilePersonalLandDetailListCompleted += new EventHandler<OutFilePersonalLandDetailListCompletedEventArgs>(clientAtt_OutFilePersonalLandDetailListCompleted);

            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            BindGrid();
        }

        /// <summary>
        /// 隐藏当前页不需要使用的吃GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.btnDelete.Visibility = Visibility.Collapsed;
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnNew.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.BtnView.Visibility = Visibility.Collapsed;

            toolbar1.retNew.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.retDelete.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;

            toolbar1.btnRefresh.Visibility = Visibility.Visible;
            toolbar1.btnOutExcel.Visibility = Visibility.Visible;
        }

        private void BindGrid()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            int pageCount = 0;
            string filter = string.Empty, strOwnerID = string.Empty, strOrderBy = string.Empty;
            ObservableCollection<object> paras = new ObservableCollection<object>();

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            if (iLandYear <= 0 || iLandMonth <= 0)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

            iLandYear = nuYear.Value.ToInt32();
            iLandMonth = nuMonth.Value.ToInt32();

            filter += " p.loginyear = @" + paras.Count().ToString();
            paras.Add(iLandYear.ToString());

            filter += " and p.loginmonth = @" + paras.Count().ToString();
            paras.Add(iLandMonth.ToString());

            switch (sOrgType.ToUpper())
            {
                case "COMPANY":
                    filter += " and (OWNERCOMPANYID ==@" + paras.Count().ToString();
                    filter += " OR ((OWNERCOMPANYID in (select t.companyid from t_hr_company t ";
                    filter += " where t.companyid in (select cc.companyid from t_hr_company cc";
                    filter += " start with cc.fathercompanyid == @" + paras.Count().ToString();
                    filter += " connect by prior cc.companyid = cc.fathercompanyid))))) ";
                    paras.Add(sOrgValue);
                    break;
                case "DEPARTMENT":
                    filter += " and (OWNERDEPARTMENTID ==@" + paras.Count().ToString() + ") ";
                    paras.Add(sOrgValue);
                    break;
                case "POST":
                    filter += " and (OWNERPOSTID ==@" + paras.Count().ToString() + ") ";
                    paras.Add(sOrgValue);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(txtEmpName.Text))
            {
                filter += " and (p.OWNERNAME like ('%" + txtEmpName.Text + "%')) ";
            }

            strOrderBy = "p.logindate";
            int ilogintimes = 0, iloginpersoncount = 0;
            clientAtt.GetPersonalLandDetailListByMultSearchAsync(strOwnerID, strOrderBy, dataPager.PageIndex, dataPager.PageSize, pageCount, ilogintimes, iloginpersoncount, filter, paras);
        }

        private void OutExcelFile()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string filter = string.Empty, strOwnerID = string.Empty, strOrderBy = string.Empty;
            ObservableCollection<object> paras = new ObservableCollection<object>();

            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            if (iLandYear <= 0 || iLandMonth <= 0)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }


            filter += " p.loginyear = @" + paras.Count().ToString();
            paras.Add(iLandYear.ToString());

            filter += " and p.loginmonth = @" + paras.Count().ToString();
            paras.Add(iLandMonth.ToString());

            switch (sOrgType.ToUpper())
            {
                case "COMPANY":
                    filter += " and (OWNERCOMPANYID ==@" + paras.Count().ToString();
                    filter += " OR ((OWNERCOMPANYID in (select t.companyid from t_hr_company t ";
                    filter += " where t.companyid in (select cc.companyid from t_hr_company cc";
                    filter += " start with cc.fathercompanyid == @" + paras.Count().ToString();
                    filter += " connect by prior cc.companyid = cc.fathercompanyid))))) ";
                    paras.Add(sOrgValue);
                    break;
                case "DEPARTMENT":
                    filter += " and (OWNERDEPARTMENTID ==@" + paras.Count().ToString() + ") ";
                    paras.Add(sOrgValue);
                    break;
                case "POST":
                    filter += " and (OWNERPOSTID ==@" + paras.Count().ToString() + ") ";
                    paras.Add(sOrgValue);
                    break;
            }



            if (!string.IsNullOrWhiteSpace(txtEmpName.Text))
            {
                filter += " and (p.OWNERNAME like ('%" + txtEmpName.Text + "%')) ";
            }

            strOrderBy = "p.logindate";

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                this.stream = dialog.OpenFile();

                clientAtt.OutFilePersonalLandDetailListAsync(strOwnerID, strOrderBy, filter, paras);
            }
        }

        void clientAtt_GetPersonalLandDetailListByMultSearchCompleted(object sender, GetPersonalLandDetailListByMultSearchCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                ObservableCollection<V_LandDetail> entResList = e.Result;

                dgLandDetailList.ItemsSource = entResList;
                dataPager.PageCount = e.pageCount;

                txtLoginTimes.Text = e.iLoginTimes.ToString();
                txtLoginPersonCount.Text = e.iLoginPersonCount.ToString();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_OutFilePersonalLandDetailListCompleted(object sender, OutFilePersonalLandDetailListCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                byte[] byExport = e.Result;

                if (this.stream != null)
                {
                    this.stream.Write(byExport, 0, byExport.Length);
                    this.stream.Close();
                    this.stream.Dispose();
                    this.stream = null;
                }
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESS"), "导出员工登录记录明细列表数据完毕！");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "导出员工登录记录列表数据失败！" + e.Error.Message);
            }
        }

        #region IEntityEditor 成员
        public string GetTitle()
        {
            return "登录记录明细总览";
        }

        public string GetStatus()
        {
            string strTemp = string.Empty;

            return strTemp;
        }

        public void DoAction(string actionType)
        {
            return;
        }

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion IEntityEditor 成员

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            sOrgType = treeOrganization.sType;
            sOrgValue = treeOrganization.sValue;

            BindGrid();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 手动导入打卡记录(记录为指定格式的Excel文件)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            OutExcelFile();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }
    }
}
