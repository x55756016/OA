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
using System.Windows.Navigation;
using System.IO;

using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.AttendanceWS;
using SMT.HRM.UI.Form.Personnel;

namespace SMT.HRM.UI.Views.Personnel
{
    public partial class PersonalLandStatistics : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        AttendanceServiceClient clientAtt;
        Stream stream;

        public PersonalLandStatistics()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PersonalLandStatistics_Loaded);
        }

        /// <summary>
        /// 重新设定toolbar应显示的按钮
        /// </summary>
        private void ResetToolBarsVisible()
        {
            toolbar1.btnNew.Visibility = Visibility.Collapsed;
            toolbar1.retNew.Visibility = System.Windows.Visibility.Collapsed;
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnDelete.Visibility = Visibility.Collapsed;
            toolbar1.retDelete.Visibility = Visibility.Collapsed;
            toolbar1.BtnView.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            toolbar1.retAudit.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;
            toolbar1.stpCheckState.Visibility = Visibility.Collapsed;

            toolbar1.btnOutExcel.Visibility = Visibility.Visible;
            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            loadbar.Stop();
            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            if (string.IsNullOrEmpty(txtBalanceYear.Text))
            {
                txtBalanceYear.Text = dtNow.Year.ToString();
            }

            ResetToolBarsVisible();

            clientAtt = new AttendanceServiceClient();
            clientAtt.GetPersonalLandStatisticListByMultSearchCompleted += new EventHandler<GetPersonalLandStatisticListByMultSearchCompletedEventArgs>(clientAtt_GetPersonalLandStatisticListByMultSearchCompleted);
            clientAtt.OutFileLandStatisticListCompleted += new EventHandler<OutFileLandStatisticListCompletedEventArgs>(clientAtt_OutFileLandStatisticListCompleted);

        }

        private void BindGrid()
        {
            loadbar.Start();
            string strSortKey = string.Empty, strOwnerID = string.Empty;
            int iYear = 0;
            string filter = string.Empty;
            ObservableCollection<object> paras = new ObservableCollection<object>();
            
            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            if (!string.IsNullOrEmpty(txtBalanceYear.Text.Trim()))
            {
                int.TryParse(txtBalanceYear.Text, out iYear);
            }

            if (iYear <= 0)
            {
                loadbar.Stop();
                return;
            }

            if (lkCompanyName.DataContext == null)
            {
                loadbar.Stop();
                return;
            }

            T_HR_COMPANY entInitComp = lkCompanyName.DataContext as T_HR_COMPANY;
            string strOrgType = string.Empty, strOrgId = string.Empty;
            strOrgType = "COMPANY";
            strOrgId = entInitComp.COMPANYID;

            if (string.IsNullOrWhiteSpace(strOrgType) || string.IsNullOrWhiteSpace(strOrgId))
            {
                loadbar.Stop();
                return;
            }

            if (strOrgType.ToUpper() != "COMPANY")
            {
                loadbar.Stop();
                return;
            }

            filter += " (p.loginyear ==@" + paras.Count().ToString() + ") ";
            paras.Add(iYear);

            if (strOrgType.ToUpper() == "COMPANY")
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter += " and";
                }

                filter += " ((OWNERCOMPANYID ==@" + paras.Count().ToString() + ") ";
                paras.Add(strOrgId);

                filter += " OR (OWNERCOMPANYID in (SELECT fac.COMPANYID FROM T_HR_COMPANY fac WHERE fac.FATHERCOMPANYID==@" + paras.Count().ToString() + "))) ";
                paras.Add(strOrgId);
            }

            strSortKey = "a.ownercompanyname, a.loginmonth";
            clientAtt.GetPersonalLandStatisticListByMultSearchAsync(strOwnerID, strSortKey, filter, paras);
        }

        private void ShowRecordDetails(int iBudgetRecordType)
        {
            if (iBudgetRecordType == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, "登录记录查询", "当前选中项无单据");
                return;
            }

            string strSignInID = string.Empty;
            if (dgLandStatisticsList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"));
                return;
            }

            if (dgLandStatisticsList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "VIEW"));
                return;
            }

            int iYear = 0, iMonth = 0;

            int.TryParse(txtBalanceYear.Text, out iYear);

            if (iYear <= 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, "登录记录查询", "查询年份必须大于0");
                return;
            }

            V_LandStatistic ent = dgLandStatisticsList.SelectedItems[0] as V_LandStatistic;

            if (string.IsNullOrWhiteSpace(ent.OrganizationID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, "登录记录查询", "合计项不能查看明细");
                return;
            }

            decimal dTimes = 0;
            string strTitle = string.Empty, strMsg = string.Empty;
            switch (iBudgetRecordType)
            {
                case -1:
                    iBudgetRecordType = 3;
                    dTimes = ent.JanTimes;
                    iMonth = 1;
                    strTitle = "一月登录记录明细总览";
                    break;
                case -2:
                    iBudgetRecordType = 3;
                    dTimes = ent.FebTimes;
                    iMonth = 2;
                    strTitle = "二月登录记录明细总览";
                    break;
                case -3:
                    iBudgetRecordType = 3;
                    dTimes = ent.MarTimes;
                    iMonth = 3;
                    strTitle = "三月登录记录明细总览";
                    break;
                case -4:
                    iBudgetRecordType = 3;
                    dTimes = ent.AprTimes;
                    iMonth = 4;
                    strTitle = "四月登录记录明细总览";
                    break;
                case -5:
                    iBudgetRecordType = 3;
                    dTimes = ent.MayTimes;
                    iMonth = 5;
                    strTitle = "五月登录记录明细总览";
                    break;
                case -6:
                    iBudgetRecordType = 3;
                    dTimes = ent.JunTimes;
                    iMonth = 6;
                    strTitle = "六月登录记录明细总览";
                    break;
                case -7:
                    iBudgetRecordType = 3;
                    dTimes = ent.JulTimes;
                    iMonth = 7;
                    strTitle = "七月登录记录明细总览";
                    break;
                case -8:
                    iBudgetRecordType = 3;
                    dTimes = ent.AugTimes;
                    iMonth = 8;
                    strTitle = "八月登录记录明细总览";
                    break;
                case -9:
                    iBudgetRecordType = 3;
                    dTimes = ent.SepTimes;
                    iMonth = 9;
                    strTitle = "九月登录记录明细总览";
                    break;
                case -10:
                    iBudgetRecordType = 3;
                    dTimes = ent.OctTimes;
                    iMonth = 10;
                    strTitle = "十月登录记录明细总览";
                    break;
                case -11:
                    iBudgetRecordType = 3;
                    dTimes = ent.NovTimes;
                    iMonth = 11;
                    strTitle = "十一月登录记录明细总览";
                    break;
                case -12:
                    iBudgetRecordType = 3;
                    dTimes = ent.DecTimes;
                    iMonth = 12;
                    strTitle = "十二月登录记录明细总览";
                    break;
            }

            if (dTimes == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, strTitle, "当前选中项无单据");
                return;
            }

            PersonalLandDetails viewRd = new PersonalLandDetails(FormTypes.Browse, iYear, iMonth, ent);

            EntityBrowser entBrowser = new EntityBrowser(viewRd);

            //entBrowser.MinHeight = double.Parse("670");
            //entBrowser.MinWidth = double.Parse("830");
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void browser_ReloadDataEvent()
        {
            return;
        }

        void PersonalLandStatistics_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            BindGrid();
        }

        void clientAtt_GetPersonalLandStatisticListByMultSearchCompleted(object sender, GetPersonalLandStatisticListByMultSearchCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error == null)
            {
                ObservableCollection<V_LandStatistic> entResList = e.Result;

                dgLandStatisticsList.ItemsSource = entResList;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void clientAtt_OutFileLandStatisticListCompleted(object sender, OutFileLandStatisticListCompletedEventArgs e)
        {
            loadbar.Stop();
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
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESS"), "导出登录记录统计数据完毕！");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "导出登录记录统计数据失败！" + e.Error.Message);
            }
        }


        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 选择公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkCompanyName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();
            lookup.SelectedObjType = OrgTreeItemTypes.Company;
            lookup.MultiSelected = false;

            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                if (ent != null)
                {
                    lkCompanyName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            loadbar.Start();
            string filter = string.Empty;
            ObservableCollection<object> paras = new ObservableCollection<object>();

            if (string.IsNullOrWhiteSpace(txtBalanceYear.Text))
            {
                loadbar.Stop();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), "请选择年份后再导出文件");
                return;
            }

            if (lkCompanyName.DataContext == null)
            {
                return;
            }

            T_HR_COMPANY entInitComp = lkCompanyName.DataContext as T_HR_COMPANY;
            string strOrgType = string.Empty, strOrgId = string.Empty;
            strOrgType = "COMPANY";
            strOrgId = entInitComp.COMPANYID;

            if (string.IsNullOrWhiteSpace(strOrgType) || string.IsNullOrWhiteSpace(strOrgId))
            {
                loadbar.Stop();
                return;
            }

            int iYear = 0;
            int.TryParse(txtBalanceYear.Text, out iYear);

            if (iYear <= 0)
            {
                loadbar.Stop();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), "输入的年份最小必须大于0");
                return;
            }

            filter += " (p.loginyear ==@" + paras.Count().ToString() + ") ";
            paras.Add(iYear);

            if (string.IsNullOrWhiteSpace(strOrgType) || string.IsNullOrWhiteSpace(strOrgId))
            {
                loadbar.Stop();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), "只能导出公司级的统计数据");
                return;
            }

            if (strOrgType.ToUpper() == "COMPANY")
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filter += " and";
                }

                filter += " ((OWNERCOMPANYID ==@" + paras.Count().ToString() + ") ";
                paras.Add(strOrgId);

                filter += " OR (OWNERCOMPANYID in (SELECT fac.COMPANYID FROM T_HR_COMPANY fac WHERE fac.FATHERCOMPANYID==@" + paras.Count().ToString() + "))) ";
                paras.Add(strOrgId);
            }

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                this.stream = dialog.OpenFile();

                clientAtt.OutFileLandStatisticListAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, "OWNERCOMPANYID", filter, paras);
            }
        }

        /// <summary>
        /// 查看指定月份的员工登录明细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hbLandTimes_Click(object sender, RoutedEventArgs e)
        {
            int iBudgetRecordType = 0;
            HyperlinkButton hbn = sender as HyperlinkButton;
            if (hbn.Tag != null)
            {
                string strMonthFlag = hbn.Tag.ToString();
                int.TryParse(strMonthFlag, out iBudgetRecordType);
                if (iBudgetRecordType > 0 && iBudgetRecordType != 3)
                {
                    iBudgetRecordType = 0;
                }

                if (iBudgetRecordType < -12)
                {
                    iBudgetRecordType = 0;
                }
            }

            ShowRecordDetails(iBudgetRecordType);
        }     

    }
}
