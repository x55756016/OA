using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;

using System.Windows.Data;
using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysUsersManagement : BasePage
    {

        SMTLoading loadbar = new SMTLoading();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client;
        private PermissionServiceClient permclient;// = new PermissionServiceClient();
        OrganizationServiceClient orgClient;
        public SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE SelectedEmployee { get; set; }

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;

        public SysUsersManagement()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_HR_EMPLOYEE");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Utility.DisplayGridToolBarButton(ToolBar, "T_HR_EMPLOYEE", false);
            //ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public void InitPara()
        {
            try
            {
                PARENT.Children.Add(loadbar);
              
                DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

                client = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
                client.GetEmployeePagingCompleted += new EventHandler<GetEmployeePagingCompletedEventArgs>(client_GetEmployeePagingCompleted);
                client.EmployeeDeleteCompleted += new EventHandler<EmployeeDeleteCompletedEventArgs>(client_EmployeeDeleteCompleted);

                orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                permclient = new PermissionServiceClient();
                permclient.GetUserByEmployeeIDCompleted += new EventHandler<Saas.Tools.PermissionWS.GetUserByEmployeeIDCompletedEventArgs>(permclient_GetUserByEmployeeIDCompleted);
                this.Loaded += new RoutedEventHandler(Employee_Loaded);
                this.ToolBar.btnNew.Visibility = Visibility.Collapsed;
                //this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
                this.ToolBar.btnEdit.Click += new RoutedEventHandler(BtnAlter_Click);
                //this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDel_Click);
                this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
                this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                this.ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
                this.ToolBar.retNew.Visibility = Visibility.Collapsed;
                this.ToolBar.btnNew.Visibility = Visibility.Collapsed;
                this.ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
                this.ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
                this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
                this.ToolBar.btnAudit.Visibility = Visibility.Collapsed;
                this.ToolBar.retAudit.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
        }

        void permclient_GetUserByEmployeeIDCompleted(object sender, Saas.Tools.PermissionWS.GetUserByEmployeeIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    T_SYS_USER AuthorUser = e.Result;
                    SysUserRole UserInfo = new SysUserRole(AuthorUser);
                    EntityBrowser browser = new EntityBrowser(UserInfo);
                    UserInfo.FormTitleName.Visibility = Visibility.Collapsed;

                    browser.MinHeight = 500;
                    browser.MinWidth = 600;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
        }

        void AddWin_ReloadDataEvent()
        {
            LoadData();
        }


        #region 树形控件的操作
        //绑定树
        private void BindTree()
        {

            if (Application.Current.Resources.Contains("SYS_CompanyInfo"))
            {
                // allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
                BindCompany();
            }
            else
            {
                orgClient.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }

        }

        void orgClient_GetCompanyActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> entTemps = e.Result;
                allCompanys = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
                allCompanys.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allCompanys.Add(item);
                });

                UICache.CreateCache("SYS_CompanyInfo", allCompanys);
                orgClient.GetDepartmentActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        void orgClient_GetDepartmentActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    return;
                }

                ObservableCollection<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> entTemps = e.Result;
                allDepartments = new List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>();
                allDepartments.Clear();
                var ents = entTemps.OrderBy(c => c.FATHERID);
                ents.ForEach(item =>
                {
                    allDepartments.Add(item);
                });

                UICache.CreateCache("SYS_DepartmentInfo", allDepartments);

                BindCompany();

                orgClient.GetPostActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);

            }
        }

        void orgClient_GetDepartmentActivedByCompanyIDCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedByCompanyIDCompletedEventArgs e)
        {

        }

        void orgClient_GetPostActivedCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("SYS_PostInfo", allPositions);
                BindPosition();
            }
        }

        private void BindCompany()
        {
            treeOrganization.Items.Clear();
            allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;

            allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;

            if (allCompanys == null)
            {
                return;
            }

            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> TopCompany = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();

            foreach (SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg in allCompanys)
            {
                //如果当前公司没有父机构的ID，则为顶级公司
                if (string.IsNullOrWhiteSpace(tmpOrg.FATHERID))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = tmpOrg.CNAME;
                    item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                    item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                    item.DataContext = tmpOrg;

                    //状态在未生效和撤消中时背景色为红色
                    SolidColorBrush brush = new SolidColorBrush();
                    if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                    {
                        brush.Color = Colors.Red;
                        item.Foreground = brush;
                    }
                    else
                    {
                        brush.Color = Colors.Black;
                        item.Foreground = brush;
                    }
                    //标记为公司
                    item.Tag = OrgTreeItemTypes.Company;
                    treeOrganization.Items.Add(item);
                    TopCompany.Add(tmpOrg);
                }
                else
                {
                    //查询当前公司是否在公司集合内有父公司存在
                    var ent = from c in allCompanys
                              where tmpOrg.FATHERTYPE == "0" && c.COMPANYID == tmpOrg.FATHERID
                              select c;
                    var ent2 = from c in allDepartments
                               where tmpOrg.FATHERTYPE == "1" && tmpOrg.FATHERID == c.DEPARTMENTID
                               select c;

                    //如果不存在，则为顶级公司
                    if (ent.Count() == 0 && ent2.Count() == 0)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpOrg.CNAME;
                        item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        item.DataContext = tmpOrg;

                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpOrg.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为公司
                        item.Tag = OrgTreeItemTypes.Company;
                        treeOrganization.Items.Add(item);

                        TopCompany.Add(tmpOrg);
                    }
                }
            }
            //开始递归
            foreach (var topComp in TopCompany)
            {
                TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Company, topComp.COMPANYID);
                List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany = (from ent in allCompanys
                                                                              where ent.FATHERTYPE == "0"
                                                                              && ent.FATHERID == topComp.COMPANYID
                                                                              select ent).ToList();

                List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment = (from ent in allDepartments
                                                                                    where ent.FATHERID == topComp.COMPANYID && ent.FATHERTYPE == "0"
                                                                                    select ent).ToList();

                AddOrgNode(lsCompany, lsDepartment, parentItem);
            }
            allPositions = Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>;
            if (allPositions != null)
            {
                BindPosition();
            }
        }

        private void AddOrgNode(List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsCompany, List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsDepartment, TreeViewItem FatherNode)
        {
            //绑定公司的子公司
            foreach (var childCompany in lsCompany)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childCompany.CNAME;
                item.HeaderTemplate = Application.Current.Resources["OrganizationItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                item.DataContext = childCompany;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childCompany.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为公司
                item.Tag = OrgTreeItemTypes.Company;
                FatherNode.Items.Add(item);

                if (lsCompany.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childCompany.COMPANYID && ent.FATHERTYPE == "0"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
            //绑定公司下的部门
            foreach (var childDepartment in lsDepartment)
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = childDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                item.DataContext = childDepartment;
                item.HeaderTemplate = Application.Current.Resources["DepartmentItemStyle"] as DataTemplate;
                item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                //状态在未生效和撤消中时背景色为红色
                SolidColorBrush brush = new SolidColorBrush();
                if (childDepartment.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                {
                    brush.Color = Colors.Red;
                    item.Foreground = brush;
                }
                else
                {
                    brush.Color = Colors.Black;
                    item.Foreground = brush;
                }
                //标记为部门
                item.Tag = OrgTreeItemTypes.Department;
                FatherNode.Items.Add(item);

                if (lsDepartment.Count() > 0)
                {
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> lsTempCom = (from ent in allCompanys
                                                                                  where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                  select ent).ToList();
                    List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> lsTempDep = (from ent in allDepartments
                                                                                     where ent.FATHERID == childDepartment.DEPARTMENTID && ent.FATHERTYPE == "1"
                                                                                     select ent).ToList();

                    AddOrgNode(lsTempCom, lsTempDep, item);
                }
            }
        }

        /// <summary>
        /// 绑定岗位
        /// </summary>
        private void BindPosition()
        {
            if (allPositions != null)
            {
                foreach (SMT.Saas.Tools.OrganizationWS.T_HR_POST tmpPosition in allPositions)
                {
                    if (tmpPosition.T_HR_DEPARTMENT == null || string.IsNullOrEmpty(tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID))
                        continue;
                    TreeViewItem parentItem = GetParentItem(OrgTreeItemTypes.Department, tmpPosition.T_HR_DEPARTMENT.DEPARTMENTID);
                    if (parentItem != null)
                    {
                        TreeViewItem item = new TreeViewItem();
                        item.Header = tmpPosition.T_HR_POSTDICTIONARY.POSTNAME;
                        item.DataContext = tmpPosition;
                        item.HeaderTemplate = Application.Current.Resources["PositionItemStyle"] as DataTemplate;
                        item.Style = Application.Current.Resources["TreeViewItemStyle"] as Style;
                        //状态在未生效和撤消中时背景色为红色
                        SolidColorBrush brush = new SolidColorBrush();
                        if (tmpPosition.CHECKSTATE != ((int)CheckStates.Approved).ToString())
                        {
                            brush.Color = Colors.Red;
                            item.Foreground = brush;
                        }
                        else
                        {
                            brush.Color = Colors.Black;
                            item.Foreground = brush;
                        }
                        //标记为岗位
                        item.Tag = OrgTreeItemTypes.Post;
                        parentItem.Items.Add(item);
                    }
                }
            }
            //树全部展开
            //  treeOrganization.ExpandAll();
            if (treeOrganization.Items.Count > 0)
            {
                TreeViewItem selectedItem = treeOrganization.Items[0] as TreeViewItem;
                selectedItem.IsSelected = true;
            }
        }

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="parentType"></param>
        /// <param name="parentID"></param>
        /// <returns></returns>
        private TreeViewItem GetParentItem(OrgTreeItemTypes parentType, string parentID)
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

        private TreeViewItem GetParentItemFromChild(TreeViewItem item, OrgTreeItemTypes parentType, string parentID)
        {
            TreeViewItem tmpItem = null;
            if (item.Tag != null && item.Tag.ToString() == parentType.ToString())
            {
                switch (parentType)
                {
                    case OrgTreeItemTypes.Company:
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY tmpOrg = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        if (tmpOrg != null)
                        {
                            if (tmpOrg.COMPANYID == parentID)
                                return item;
                        }
                        break;
                    case OrgTreeItemTypes.Department:
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmpDep = item.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
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


        #endregion

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void Employee_Loaded(object sender, RoutedEventArgs e)
        {
            // LoadData();
            BindTree();
        }

        void client_GetEmployeePagingCompleted(object sender, GetEmployeePagingCompletedEventArgs e)
        {
            List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> list = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            loadbar.Stop();
        }

        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
            TextBox txtEmpCode = Utility.FindChildControl<TextBox>(expander, "txtEmpCode");
            if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
            {
                //filter += "EMPLOYEECODE==@" + paras.Count().ToString();
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)";
                paras.Add(txtEmpCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)";
                paras.Add(txtEmpName.Text.Trim());
            }

            string sType = "", sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                        sType = "Company";
                        sValue = company.COMPANYID;
                        break;
                    case "Department":
                        OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as OrganizationWS.T_HR_DEPARTMENT;
                        sType = "Department";
                        sValue = department.DEPARTMENTID;
                        break;
                    case "Post":
                        OrganizationWS.T_HR_POST post = selectedItem.DataContext as OrganizationWS.T_HR_POST;
                        sType = "Post";
                        sValue = post.POSTID;
                        break;
                }
            }
            client.GetEmployeePagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                if (DtGrid.SelectedItems.Count <= 0)
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    return;
                }
                Form.SysUserForms editForm = new SMT.SaaS.Permission.UI.Form.SysUserForms(FormTypes.Browse, SelectedEmployee.EMPLOYEEID);

                EntityBrowser browser = new EntityBrowser(editForm);
                browser.FormType = FormTypes.Browse;

                browser.MinHeight = 450;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        void BtnAlter_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                Form.SysUserForms editForm = new SMT.SaaS.Permission.UI.Form.SysUserForms(FormTypes.Edit, SelectedEmployee.EMPLOYEEID);

                EntityBrowser browser = new EntityBrowser(editForm);
                browser.MinHeight = 450;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectedEmployee = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)grid.SelectedItems[0];
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (OrganizationWS.T_HR_EMPLOYEE tmp in DtGrid.SelectedItems)
                    {
                        ids.Add(tmp.EMPLOYEEID);
                    }
                    client.EmployeeDeleteAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
        }

        void client_EmployeeDeleteCompleted(object sender, EmployeeDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEE"));
                LoadData();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //Form.Personnel.EmployeeForm form = new SMT.HRM.UI.Form.Personnel.EmployeeForm(FormTypes.New, "");
            //EntityBrowser browser = new EntityBrowser(form);

            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_USER");
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE UserT = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)e.Row.DataContext;

            Button MyAuthorBtn = DtGrid.Columns[6].GetCellContent(e.Row).FindName("AuthorizationBtn") as Button;
            MyAuthorBtn.Tag = UserT;
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            LoadData();
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            orgClient.DoClose();
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

        private void AuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {
            Button AuthorBtn = sender as Button;
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE AuthorUser = AuthorBtn.Tag as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            permclient.GetUserByEmployeeIDAsync(AuthorUser.EMPLOYEEID);
            
        }
    }
}
