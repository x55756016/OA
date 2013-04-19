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
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.Views.AssetManagement
{
    public partial class TreeAssetManagementPage : BasePage
    {
        #region 全局变量
        private SMTLoading loadbar = new SMTLoading();
        OrganizationServiceClient orgClient;

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;

        #endregion

        #region 初始化
        public TreeAssetManagementPage()
        {
            InitializeComponent();
            orgClient = new OrganizationServiceClient();
            RegisterEvents();
            GetEntityLogo("T_HR_ATTENDMONTHLYBALANCE");
            PARENT.Children.Add(loadbar);
        }

        /// <summary>
        /// 加载页面各项数据
        /// </summary>
        private void InitPage()
        {
            BindComboxBox();
            BindTree();
            //BindGrid();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());

            Utility.DisplayGridToolBarButton(toolbar1, "T_OA_ASSETSCategoryTree", true);
            InitPage();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            if (string.IsNullOrEmpty(txtBalanceYear.Text))
            {
                txtBalanceYear.Text = dtNow.Year.ToString();
            }

            toolbar1.btnNew.Content = Utility.GetResourceStr("BALANCECALCULATE");
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);

            toolbar1.btnEdit.Visibility = Visibility.Collapsed;

            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            orgClient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
            orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
            orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
            //clientAtt.GetAttendMonthlyBalanceListByMultSearchCompleted += new EventHandler<GetAttendMonthlyBalanceListByMultSearchCompletedEventArgs>(clientAtt_GetAttendMonthlyBalanceListByMultSearchCompleted);
            //clientAtt.CalculateEmployeeAttendanceMonthlyByEmployeeIDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_CalculateEmployeeAttendanceMonthlyByEmployeeIDCompleted);
        }
        #endregion

        //绑定树
        private void BindTree()
        {
            orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
           
        }

        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string sType = string.Empty, sValue = string.Empty;
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "0":
                        T_HR_COMPANY company = selectedItem.DataContext as T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        break;
                    case "1":
                        T_HR_DEPARTMENT department = selectedItem.DataContext as T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        break;
                    case "2":
                        T_HR_POST post = selectedItem.DataContext as T_HR_POST;
                        sType = "Post";
                        sValue = post.POSTID;
                        break;
                }
            }
        }

        /// <summary>
        /// 根据审核状态显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //BindGrid();
        }

        /// <summary>
        /// 加载审核状态列表
        /// </summary>
        private void BindComboxBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            if (allCompanys != null)
            {
                foreach (T_HR_COMPANY tmpOrg in allCompanys)
                {
                    if (tmpOrg.T_HR_COMPANY2 == null || string.IsNullOrEmpty(tmpOrg.T_HR_COMPANY2.COMPANYID))
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.Style = Application.Current.Resources["OrganizationItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //标记为公司
                        item.Tag = "0";
                        treeOrganization.Items.Add(item);

                        AddChildOrgItems(item, tmpOrg.COMPANYID);
                    }
                }
            }
        }

        private void AddChildOrgItems(TreeViewItem item, string companyID)
        {
            List<T_HR_COMPANY> childs = GetChildORG(companyID);
            if (childs == null || childs.Count <= 0)
                return;

            foreach (T_HR_COMPANY childOrg in childs)
            {
                TreeViewItem childItem = new TreeViewItem();
                childItem.Header = childOrg.CNAME;
                childItem.DataContext = childOrg;
                childItem.Style = Application.Current.Resources["OrganizationItemStyle"] as Style;

                //标记为公司
                childItem.Tag = "0";
                item.Items.Add(childItem);

                AddChildOrgItems(childItem, childOrg.COMPANYID);
            }
        }

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> GetChildORG(string companyID)
        {
            List<T_HR_COMPANY> orgs = new List<T_HR_COMPANY>();

            foreach (T_HR_COMPANY org in allCompanys)
            {
                if (org.T_HR_COMPANY2 != null && org.T_HR_COMPANY2.COMPANYID == companyID)
                    orgs.Add(org);
            }
            return orgs;
        }

        private void BindDepartment()
        {
            if (allDepartments != null)
            {
                foreach (T_HR_DEPARTMENT tmpDep in allDepartments)
                {
                    if (tmpDep.T_HR_COMPANY == null)
                        continue;

                    TreeViewItem parentItem = GetParentItem("0", tmpDep.T_HR_COMPANY.COMPANYID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();

                        item.Header = tmpDep.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        item.DataContext = tmpDep;
                        item.Style = Application.Current.Resources["DepartmentItemStyle"] as Style;

                        //标记为部门
                        item.Tag = "1";
                        parentItem.Items.Add(item);
                    }
                }
            }
        }

        private TreeViewItem GetParentItem(string parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            foreach (TreeViewItem item in treeOrganization.Items)
            {
                tmpItem = GetParentItemFromChild(item, parentType, parentID);
                if (tmpItem != null)
                {
                    break;
                }
            }
            return tmpItem;
        }

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, string parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType)
            {
                switch (parentType)
                {
                    case "0":
                        T_HR_COMPANY tmpOrg = item.DataContext as T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case "1":
                        T_HR_DEPARTMENT tmpDep = item.DataContext as T_HR_DEPARTMENT;
                        if (tmpDep != null)
                        {
                            if (tmpDep.DEPARTMENTID == parentID)
                                return item;
                        }
                        break;
                }

            }
            if (item.Items != null && item.Items.Count > 0)
            {
                foreach (TreeViewItem childitem in item.Items)
                {
                    tmpItem = GetParentItemFromChild(childitem, parentType, parentID);
                    if (tmpItem != null)
                    {
                        break;
                    }
                }
            }
            return tmpItem;
        }

        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem("1", tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.Style = Application.Current.Resources["PositionItemStyle"] as Style;

                        //标记为岗位
                        item.Tag = "2";
                        parentItem.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    allCompanys = e.Result.ToList();
                }
                BindCompany();
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    allDepartments = e.Result.ToList();
                }
                BindDepartment();
                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                BindPosition();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
        }

        private void dgAMBList_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void lkEmpName_FindClick(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
