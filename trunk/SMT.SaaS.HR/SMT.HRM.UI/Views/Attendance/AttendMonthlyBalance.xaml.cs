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

using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.IO;
using System.Windows.Media.Imaging;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class AttendMonthlyBalance : BasePage, IClient
    {
        #region 全局变量
        AttendanceServiceClient clientAtt;
        private SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        #endregion

        #region 初始化
        public AttendMonthlyBalance()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(AttendMonthlyBalance_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 私有方法
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
                nudBalanceMonth.Value = dtNow.AddMonths(-1).Month;
            }

            toolbar1.btnImport.Visibility = Visibility.Visible;


            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1022.png", Utility.GetResourceStr("BALANCECALCULATE")).Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnImport.Click += new RoutedEventHandler(btnImport_Click);

            //toolbar1.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1022.png", Utility.GetResourceStr("导出报表")).Click += new RoutedEventHandler(btnExportReports_Click);

            ImageButton ChangeMeetingBtn = new ImageButton();
            ChangeMeetingBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png", UriKind.Relative));
            ChangeMeetingBtn.TextBlock.Text = Utility.GetResourceStr("考勤备案报表");// 考勤备案
            ChangeMeetingBtn.Image.Width = 16.0;
            ChangeMeetingBtn.Image.Height = 22.0;
            ChangeMeetingBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.Click += new RoutedEventHandler(btnExportReports_Click);
            toolbar1.stpOtherAction.Children.Add(ChangeMeetingBtn);


            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //orgClient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            clientAtt.GetAttendMonthlyBalanceListByMultSearchCompleted += new EventHandler<GetAttendMonthlyBalanceListByMultSearchCompletedEventArgs>(clientAtt_GetAttendMonthlyBalanceListByMultSearchCompleted);
            clientAtt.CalculateEmployeeAttendanceMonthlyByEmployeeIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceMonthlyByEmployeeIDCompleted);
            clientAtt.RemoveAttendMonthlyBalanceCompleted += new EventHandler<RemoveAttendMonthlyBalanceCompletedEventArgs>(clientAtt_RemoveAttendMonthlyBalanceCompleted);
            clientAtt.ExportAttendMonthlyBalanceRdListReportsCompleted += new EventHandler<ExportAttendMonthlyBalanceRdListReportsCompletedEventArgs>(clientAtt_ExportAttendMonthlyBalanceRdListReportsCompleted);
            treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
            //this.Loaded += new RoutedEventHandler(AttendMonthlyBalance_Loaded);
        }

        void clientAtt_ExportAttendMonthlyBalanceRdListReportsCompleted(object sender, ExportAttendMonthlyBalanceRdListReportsCompletedEventArgs e)
        {
            loadbar.Stop();
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }

            
        }        

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 重新设定toolbar应显示的按钮
        /// </summary>
        private void ResetToolBarsVisible()
        {
            toolbar1.btnNew.Visibility = Visibility.Collapsed;
            toolbar1.retNew.Visibility = Visibility.Collapsed;
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnReSubmit.Visibility = Visibility.Collapsed;
            toolbar1.BtnView.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            Utility.DisplayGridToolBarButton(toolbar1, "T_HR_ATTENDMONTHLYBALANCE", true);
            BindComboxBox();
            // BindTree();
            // BindGrid();
        }

        //绑定树
        //private void BindTree()
        //{
        //    orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //}

        /// <summary>
        /// 加载审核状态列表
        /// </summary>
        private void BindComboxBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            }
        }

        /// <summary>
        /// 绑定Grid
        /// </summary>
        private void BindGrid()
        {
            string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
            decimal dBalanceYear = 0, dBalanceMonth = 0;
            int pageIndex = 0, pageSize = 0, pageCount = 0;

            ResetToolBarsVisible();
            strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            strSortKey = "BALANCEYEAR, BALANCEMONTH";
            CheckInputFilter(ref strEmployeeID, ref dBalanceYear, ref dBalanceMonth, ref strCheckState);

            if (strCheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString() || strCheckState == Convert.ToInt32(CheckStates.WaittingApproval).ToString())
            {
                toolbar1.btnAudit.Visibility = System.Windows.Visibility.Visible;
                toolbar1.retAudit.Visibility = System.Windows.Visibility.Visible;
            }

            pageIndex = dataPager.PageIndex;
            pageSize = dataPager.PageSize;

            string sType = string.Empty, sValue = string.Empty;
            sType = treeOrganization.sType;
            sValue = treeOrganization.sValue;
            //TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            //if (selectedItem != null)
            //{
            //    string IsTag = selectedItem.Tag.ToString();
            //    switch (IsTag)
            //    {
            //        case "0":
            //            T_HR_COMPANY company = selectedItem.DataContext as T_HR_COMPANY;
            //            sType = "Company";
            //            sValue = company.COMPANYID;
            //            break;
            //        case "1":
            //            T_HR_DEPARTMENT department = selectedItem.DataContext as T_HR_DEPARTMENT;
            //            sType = "Department";
            //            sValue = department.DEPARTMENTID;
            //            break;
            //        case "2":
            //            T_HR_POST post = selectedItem.DataContext as T_HR_POST;
            //            sType = "Post";
            //            sValue = post.POSTID;
            //            break;
            //    }
            //}

            clientAtt.GetAttendMonthlyBalanceListByMultSearchAsync(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey, pageIndex, pageSize, pageCount);
            loadbar.Start();
        }

        /// <summary>
        /// 效验输入内容
        /// </summary>
        /// <param name="strEmployeeID"></param>
        /// <param name="dtPunchDateFrom"></param>
        /// <param name="dtPunchDateTo"></param>
        private void CheckInputFilter(ref string strEmployeeID, ref decimal dBalanceYear, ref decimal dBalanceMonth, ref string strCheckState)
        {
            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    strEmployeeID = ent.EMPLOYEEID.Trim();
                }
            }

            if (!string.IsNullOrEmpty(txtBalanceYear.Text.Trim()))
            {
                decimal.TryParse(txtBalanceYear.Text, out dBalanceYear);
            }

            decimal.TryParse(nudBalanceMonth.Value.ToString(), out dBalanceMonth);

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                strCheckState = entDic.DICTIONARYVALUE.ToString();
            }
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void browser_ReloadDataEvent()
        {
            BindGrid();
        }

        /// <summary>
        /// 
        /// </summary>
        //private void BindCompany()
        //{
        //    treeOrganization.Items.Clear();
        //    if (allCompanys != null)
        //    {
        //        foreach (T_HR_COMPANY tmpOrg in allCompanys)
        //        {
        //            if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpOrg.CNAME;
        //                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
        //                item.DataContext = tmpOrg;

        //                //标记为公司
        //                item.Tag = "0";
        //                treeOrganization.Items.Add(item);

        //                AddChildOrgItems(item, tmpOrg.COMPANYID);
        //            }
        //        }
        //    }
        //}

        //private void AddChildOrgItems(TreeViewItem item, string companyID)
        //{
        //    List<T_HR_COMPANY> childs = GetChildORG(companyID);
        //    if (childs == null || childs.Count <= 0)
        //        return;

        //    foreach (T_HR_COMPANY childOrg in childs)
        //    {
        //        TreeViewItem childItem = new TreeViewItem();
        //        childItem.Header = childOrg.CNAME;
        //        childItem.DataContext = childOrg;
        //        childItem.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
        //        childItem.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //        //标记为公司
        //        childItem.Tag = "0";
        //        item.Items.Add(childItem);

        //        AddChildOrgItems(childItem, childOrg.COMPANYID);
        //    }
        //}

        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> GetChildORG(string companyID)
        //{
        //    List<T_HR_COMPANY> orgs = new List<T_HR_COMPANY>();

        //    foreach (T_HR_COMPANY org in allCompanys)
        //    {
        //        if (org.T_HR_COMPANY2 != null && org.T_HR_COMPANY2.COMPANYID == companyID)
        //            orgs.Add(org);
        //    }
        //    return orgs;
        //}

        //private void BindDepartment()
        //{
        //    if (allDepartments != null)
        //    {
        //        foreach (T_HR_DEPARTMENT tmpDep in allDepartments)
        //        {
        //            if (tmpDep.T_HR_COMPANY == null)
        //                continue;

        //            TreeViewItem parentItem = GetParentItem("0", tmpDep.T_HR_COMPANY.COMPANYID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();

        //                item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
        //                item.DataContext = tmpDep;
        //                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //                //标记为部门
        //                item.Tag = "1";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //}

        //private TreeViewItem GetParentItem(string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    foreach (TreeViewItem item in treeOrganization.Items)
        //    {
        //        tmpItem = GetParentItemFromChild(item, parentType, parentID);
        //        if (tmpItem != null)
        //        {
        //            break;
        //        }
        //    }
        //    return tmpItem;
        //}

        //private TreeViewItem GetParentItemFromChild(TreeViewItem item, string parentType, string parentID)
        //{
        //    TreeViewItem tmpItem = null;
        //    if (item.Tag != null && item.Tag.ToString() == parentType)
        //    {
        //        switch (parentType)
        //        {
        //            case "0":
        //                T_HR_COMPANY tmpOrg = item.DataContext as T_HR_COMPANY;
        //                if (tmpOrg != null)
        //                {
        //                    if (tmpOrg.COMPANYID == parentID)
        //                        return item;
        //                }
        //                break;
        //            case "1":
        //                T_HR_DEPARTMENT tmpDep = item.DataContext as T_HR_DEPARTMENT;
        //                if (tmpDep != null)
        //                {
        //                    if (tmpDep.DEPARTMENTID == parentID)
        //                        return item;
        //                }
        //                break;
        //        }

        //    }
        //    if (item.Items != null && item.Items.Count > 0)
        //    {
        //        foreach (TreeViewItem childitem in item.Items)
        //        {
        //            tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
        //            if (tmpItem != null)
        //            {
        //                break;
        //            }
        //        }
        //    }
        //    return tmpItem;
        //}

        //private void BindPosition()
        //{
        //    if (allPositions != null)
        //    {
        //        foreach (T_HR_POST tmpPosition in allPositions)
        //        {
        //            if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
        //                continue;
        //            TreeViewItem parentItem = GetParentItem("1", tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
        //            if (parentItem != null)
        //            {
        //                TreeViewItem item = new TreeViewItem();
        //                item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
        //                item.DataContext = tmpPosition;
        //                item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
        //                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;

        //                //标记为岗位
        //                item.Tag = "2";
        //                parentItem.Items.Add(item);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 单人审核
        /// </summary>
        private void AuditPerBalance()
        {
            string strMonthlyBalanceID = string.Empty;
            //if (dgAMBList.SelectedItems == null)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}

            //if (dgAMBList.SelectedItems.Count == 0)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
            //    return;
            //}

            T_HR_ATTENDMONTHLYBALANCE entAttBalance = dgAMBList.SelectedItems[0] as T_HR_ATTENDMONTHLYBALANCE;
            strMonthlyBalanceID = entAttBalance.MONTHLYBALANCEID;
            AttendMonthlyBalanceForm formAttBalance = new AttendMonthlyBalanceForm(FormTypes.Audit, strMonthlyBalanceID);
            EntityBrowser browser = new EntityBrowser(formAttBalance);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.FormType = FormTypes.Audit;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 批量审核
        /// </summary>
        private void AuditBalance(string sType, string sValue)
        {
            decimal dBalanceYear = 0, dBalanceMonth = 0;
            string strCheckState = string.Empty;

            if (!string.IsNullOrEmpty(txtBalanceYear.Text.Trim()))
            {
                decimal.TryParse(txtBalanceYear.Text, out dBalanceYear);
            }

            decimal.TryParse(nudBalanceMonth.Value.ToString(), out dBalanceMonth);

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                if (entDic.DICTIONARYVALUE != null)
                {
                    strCheckState = entDic.DICTIONARYVALUE.Value.ToString();
                }
            }

            AttendMonthlyBalanceAudit formAttBalance = new AttendMonthlyBalanceAudit(FormTypes.Audit, sType, sValue, dBalanceYear, dBalanceMonth, strCheckState);
            EntityBrowser entBrowser = new EntityBrowser(formAttBalance);

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.FormType = FormTypes.Audit;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 事件

        /// <summary>
        /// 根据审核状态显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_ATTENDMONTHLYBALANCE");
                BindGrid();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void orgClient_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            allCompanys = e.Result.ToList();
        //        }
        //        BindCompany();
        //        orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //}

        //void orgClient_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            allDepartments = e.Result.ToList();
        //        }
        //        BindDepartment();
        //        orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //}

        //void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            allPositions = e.Result.ToList();
        //        }
        //        BindPosition();
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //}

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendMonthlyBalance_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            orgClient = new OrganizationServiceClient();
            RegisterEvents();
            GetEntityLogo("T_HR_ATTENDMONTHLYBALANCE");
            InitPage();
        }

        /// <summary>
        /// 返回考勤月度结算数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendMonthlyBalanceListByMultSearchCompleted(object sender, GetAttendMonthlyBalanceListByMultSearchCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<T_HR_ATTENDMONTHLYBALANCE> entAMBList = e.Result;

                dgAMBList.ItemsSource = entAMBList;
                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        void clientAtt_CalculateEmployeeAttendanceMonthlyByEmployeeIDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }


        /// <summary>
        /// 删除考勤结算信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_RemoveAttendMonthlyBalanceCompleted(object sender, RemoveAttendMonthlyBalanceCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            BindGrid();
        }

        /// <summary>
        /// 查询员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            BindGrid();
        }

        /// <summary>
        /// 根据指定条件，查询员工月度考勤数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgAMBList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgAMBList, e.Row, "T_HR_ATTENDMONTHLYBALANCE");

            TextBlock tborder = dgAMBList.Columns[1].GetCellContent(e.Row).FindName("tbOrder") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
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

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            CalculateEmployeeAttendanceMonthlyForm form = new CalculateEmployeeAttendanceMonthlyForm();
            EntityBrowser entBrowser = new EntityBrowser(form);
            form.MinWidth = 580;
            form.MinHeight = 200;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        void btnExportReports_Click(object sender, RoutedEventArgs e)
        {
            
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;
            
            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                string strEmployeeID = string.Empty, strSortKey = string.Empty, strOwnerID = string.Empty, strCheckState = string.Empty;
                decimal dBalanceYear = 0, dBalanceMonth = 0;
                //int pageIndex = 0, pageSize = 0, pageCount = 0;

                //ResetToolBarsVisible();
                strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                strSortKey = "BALANCEYEAR, BALANCEMONTH";
                CheckInputFilter(ref strEmployeeID, ref dBalanceYear, ref dBalanceMonth, ref strCheckState);

                if (strCheckState == Convert.ToInt32(CheckStates.UnSubmit).ToString() || strCheckState == Convert.ToInt32(CheckStates.WaittingApproval).ToString())
                {
                    //toolbar1.btnAudit.Visibility = System.Windows.Visibility.Visible;
                    //toolbar1.retAudit.Visibility = System.Windows.Visibility.Visible;
                }

                //pageIndex = dataPager.PageIndex;
                //pageSize = dataPager.PageSize;

                string sType = string.Empty, sValue = string.Empty;
                sType = treeOrganization.sType;
                sValue = treeOrganization.sValue;

                loadbar.Start();
                clientAtt.ExportAttendMonthlyBalanceRdListReportsAsync(sType, sValue, strOwnerID, strCheckState, strEmployeeID, dBalanceYear, dBalanceMonth, strSortKey);
                
            }
            
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string strID = "";
            if (dgAMBList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAMBList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            foreach (object ovj in dgAMBList.SelectedItems)
            {
                T_HR_ATTENDMONTHLYBALANCE ent = ovj as T_HR_ATTENDMONTHLYBALANCE;

                string Result = "";
                if (ent != null)
                {
                    strID = ent.MONTHLYBALANCEID.ToString();
                    if (ent.CHECKSTATE == Convert.ToInt32(CheckStates.Approving).ToString() || ent.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString() || ent.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETEAUDITERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }

                    ComfirmWindow delComfirm = new ComfirmWindow();
                    delComfirm.OnSelectionBoxClosed += (obj, result) =>
                    {
                        clientAtt.RemoveAttendMonthlyBalanceAsync(strID);
                    };
                    delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
            }
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string sType = string.Empty, sValue = string.Empty;

            string IsTag = treeOrganization.sType;
            switch (IsTag)
            {
                case "Company":
                    sType = (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString();
                    sValue = treeOrganization.sValue;
                    break;
                case "Department":
                    sType = (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString();
                    sValue = treeOrganization.sValue;
                    break;
                case "Post":
                    sType = (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString();
                    sValue = treeOrganization.sValue;
                    break;
            }

            if (string.IsNullOrEmpty(sValue))
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgAMBList.ItemsSource == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择有效的数据进行审核", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            IEnumerable<T_HR_ATTENDMONTHLYBALANCE> entAMBList = dgAMBList.ItemsSource as IEnumerable<T_HR_ATTENDMONTHLYBALANCE>;
            if (entAMBList.Count() == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择有效的数据进行审核", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (string.IsNullOrEmpty(sType) || string.IsNullOrEmpty(sValue))
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择有效的数据进行审核", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            AuditBalance(sType, sValue);

        }

        /// <summary>
        /// 读取打卡的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnImport_Click(object sender, RoutedEventArgs e)
        {
            ImportAttendbalanceForm form = new ImportAttendbalanceForm();
            EntityBrowser entBrowser = new EntityBrowser(form);

            form.MinWidth = 600;
            form.MinHeight = 200;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            clientAtt.DoClose();
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
