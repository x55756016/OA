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

using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;

using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysFBAdmins : BasePage
    {
       

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        SMTLoading loadbar = new SMTLoading();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client;
        private PermissionServiceClient permclient;// = new PermissionServiceClient();
        OrganizationServiceClient orgClient;
        public SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW SelectedEmployee { get; set; }

        private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表
        private ObservableCollection<string> companyids = new ObservableCollection<string>();  //员工所在公司
        private  string ListEmployeeids = "";  //员工集合
        private string strSelectEmployeeID = "";//选择的员工ID
        private List<V_FBAdmin> LstFbAdmin = new List<V_FBAdmin>();
        public SysFBAdmins()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_SYS_FBADMIN");
            ListDict.Add("SYSTEMTYPE");//系统类型
            this.Loaded += new RoutedEventHandler(SysUserManagement_Loaded);
                            
        }

        void SysUserManagement_Loaded(object sender, RoutedEventArgs e)
        {
            
            InitGetData();
            InitToolBarEvent();
        }

        public void InitGetData()
        {
            BindTree();
            DictManager.LoadDictionary(ListDict);        
            if (Common.CurrentLoginUserInfo.UserPosts.Count() > 1)
            {
                for (int i = 0; i < Common.CurrentLoginUserInfo.UserPosts.Count(); i++)
                {
                    companyids.Add(Common.CurrentLoginUserInfo.UserPosts[i].CompanyID);
                }
            }
            else
            {
                companyids.Add(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            }
            
            permclient.getFbAdminsAsync(Common.CurrentLoginUserInfo.EmployeeID, companyids);
        }

        public void InitPara()
        {
            try
            {
                PARENT.Children.Add(loadbar);
                loadbar.Start();

                DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
                client = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
                client.GetEmployeePagingCompleted += new EventHandler<GetEmployeePagingCompletedEventArgs>(client_GetEmployeePagingCompleted);
                //client.GetEmployeesWithOutPermissionsCompleted += new EventHandler<GetEmployeesWithOutPermissionsCompletedEventArgs>(client_GetEmployeesWithOutPermissionsCompleted);
                client.GetEmployeeViewsPagingCompleted += new EventHandler<GetEmployeeViewsPagingCompletedEventArgs>(client_GetEmployeeViewsPagingCompleted);
                client.EmployeeDeleteCompleted += new EventHandler<EmployeeDeleteCompletedEventArgs>(client_EmployeeDeleteCompleted);

                orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                permclient = new PermissionServiceClient();
                permclient.GetUserByEmployeeIDCompleted += new EventHandler<Saas.Tools.PermissionWS.GetUserByEmployeeIDCompletedEventArgs>(permclient_GetUserByEmployeeIDCompleted);
                permclient.getFbAdminsCompleted += new EventHandler<getFbAdminsCompletedEventArgs>(permclient_getFbAdminsCompleted);
                //this.Loaded += new RoutedEventHandler(Employee_Loaded);
                permclient.DeleteFbAdminCompleted += new EventHandler<DeleteFbAdminCompletedEventArgs>(permclient_DeleteFbAdminCompleted);
                
            }
            catch (Exception ex)
            {
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "SysUserManagement", "Views/SysUserManagement", "SysUserManagement", ex);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
        }

        void permclient_DeleteFbAdminCompleted(object sender, DeleteFbAdminCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (string.IsNullOrEmpty(e.Result))
                {
                    if (ListEmployeeids.IndexOf(",") > 0)
                    {
                        //如果存在多个,Z则使用员工ID+","替换后来被删除的员工ID
                        ListEmployeeids = ListEmployeeids.Replace(strSelectEmployeeID + ",", "");
                    }

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), "删除预算管理员成功",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "删除预算管理员失败",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "删除预算管理员失败，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            InitGetData();
        }

        void permclient_getFbAdminsCompleted(object sender, getFbAdminsCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                loadbar.Stop();
            }
            else
            {
                if (e.Result != null)
                {
                    LstFbAdmin = e.Result.ToList();
                    ListEmployeeids = "";
                    if (LstFbAdmin.Count() > 0)
                    {
                        LstFbAdmin.ForEach(item =>
                        {
                            ListEmployeeids += item.EMPLOYEEID + ",";
                        });

                    }
                    
                    ListEmployeeids = ListEmployeeids.Substring(0, ListEmployeeids.Length - 1);//去掉最后一个,
                    LoadData();
                }
                else
                {
                    loadbar.Stop();
                }
            }
        }

        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                //InitToolBarEvent();
                
                //LoadData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        private void InitToolBarEvent()
        {
            
            this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
            
            //this.ToolBar.btnEdit.Click += new RoutedEventHandler(BtnAlter_Click);
            this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDel_Click);
            //this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            this.ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            this.ToolBar.retNew.Visibility = Visibility.Collapsed;
            this.ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            this.ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            this.ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            
            this.ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            this.ToolBar.retAudit.Visibility = Visibility.Collapsed;
            this.ToolBar.BtnView.Visibility = Visibility.Collapsed;
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

            ToolBar.stpOtherAction.Children.Clear();
            
            ImageButton ChangeMeetingBtn = new ImageButton();                
            ChangeMeetingBtn.TextBlock.Text = "预算管理员可以为子公司设置管理员，还可以进行跨级设置，请根据实际情况进行操作";             
            SolidColorBrush brush = new SolidColorBrush();                    
            brush.Color = Colors.Red;
            
            ChangeMeetingBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            ChangeMeetingBtn.TextBlock.Foreground = brush;
            ToolBar.stpOtherAction.Children.Add(ChangeMeetingBtn);
            
            
            
        }

      
        void permclient_GetUserByEmployeeIDCompleted(object sender, Saas.Tools.PermissionWS.GetUserByEmployeeIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    //T_SYS_FBADMIN AuthorUser = e.Result;
                    //SysUserRole UserInfo = new SysUserRole(AuthorUser);
                    //EntityBrowser browser = new EntityBrowser(UserInfo);
                    //UserInfo.FormTitleName.Visibility = Visibility.Collapsed;
                    //browser.MinHeight = 300;
                    //browser.MinWidth = 600;
                    //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
                    //browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
        }

        void AddWin_ReloadDataEvent()
        {
            InitGetData();
            //LoadData();
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
                loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetCompanyActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
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
                
                loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetDepartmentActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
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
                loadbar.Stop();//有错误停止转圈                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--GetDepartmentActived");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    allPositions = e.Result.ToList();
                }
                UICache.CreateCache("SYS_PostInfo", allPositions);
                
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
            if (allDepartments == null)
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
                    if (allCompanys != null && allDepartments != null)
                    {
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
            InitGetData();
            //LoadData();
        }

        void Employee_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            BindTree();
        }
        void client_GetEmployeeViewsPagingCompleted(object sender, GetEmployeeViewsPagingCompletedEventArgs e)
        {
            List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW> list = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW>();
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    if (e.Result.ToList().Count() > 0)
                    {
                        e.Result.ToList().ForEach(item => {
                            var Employees = from ent in LstFbAdmin
                                            where ent.OWNERCOMPANYID == item.OWNERCOMPANYID && ent.OWNERPOSTID == item.OWNERPOSTID
                                            && ent.OWNERDEPARTMENTID == item.OWNERDEPARTMENTID 
                                            select ent;
                            if (Employees != null)
                            {
                                if (Employees.Count() > 0)
                                {
                                    list.Add(item);
                                }
                            }
                        });
                    }
                    //list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            loadbar.Stop();
        }

        
        void client_GetEmployeesWithOutPermissionsCompleted(object sender, GetEmployeesWithOutPermissionsCompletedEventArgs e)
        {
           
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
            if (!string.IsNullOrEmpty(ListEmployeeids))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                //filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEEID)";
                if (ListEmployeeids.IndexOf(',') > 0)
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    string[] ArrEmployeeids = ListEmployeeids.Split(',');
                    if (ArrEmployeeids.Count() > 1)
                    {
                        filter += "( ";
                        for (int i = 0; i < ArrEmployeeids.Count(); i++)
                        {
                            if (i > 0)
                            {
                                filter += " or ";
                            }
                            filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEEID)";
                            paras.Add(ArrEmployeeids[i].ToString());
                        }
                        filter += " ) ";
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEEID)";
                    paras.Add(ListEmployeeids);
                }
                //filter += " EMPLOYEEID.Contains(@" + paras.Count().ToString() + ")";
                //paras.Add(ListEmployeeids);
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
                OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as OrganizationWS.T_HR_COMPANY;
                sType = "Company";
                sValue = company.COMPANYID;
                
            }
            
            
            client.GetEmployeeViewsPagingAsync(dataPager.PageIndex, dataPager.PageSize,"EMPLOYEECNAME", filter, paras, pageCount, sType, sValue,SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            InitGetData();
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                if (DtGrid.SelectedItems.Count <= 0)
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRMBUTTON"));
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
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRMBUTTON"));
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
                SelectedEmployee = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW)grid.SelectedItems[0];
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                if (this.DtGrid.SelectedItems != null)
                {
                    V_EMPLOYEEVIEW employee = DtGrid.SelectedItem as V_EMPLOYEEVIEW;
                    //var Ents = from ent in LstFbAdmin
                    //           where ent.OWNERCOMPANYID == employee.OWNERCOMPANYID && ent.OWNERPOSTID == employee.OWNERPOSTID
                    //           && ent.OWNERDEPARTMENTID == employee.OWNERDEPARTMENTID
                    //           && ent.EMPLOYEEID == employee.EMPLOYEEID
                    //           select ent;
                    //if (Ents != null)
                    //{
                    //    if (Ents.Count() > 0)
                    //    { 
                    //        //if(Ents.FirstOrDefault().)
                    //    }
                    //}
                    if (DtGrid.SelectedItems.Count > 1)
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "只能删除单条记录，请重新选择！", Utility.GetResourceStr("CONFIRMBUTTON"));
                    }
                    else
                    {
                        if (employee != null)
                        {
                            strSelectEmployeeID = employee.EMPLOYEEID;
                            permclient.DeleteFbAdminAsync(employee.EMPLOYEEID, employee.OWNERCOMPANYID);
                        }
                    }
                    
                }         

                
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择需要删除的信息", Utility.GetResourceStr("CONFIRMBUTTON"));
            }

            
        }

        void client_EmployeeDeleteCompleted(object sender, EmployeeDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                
                SMT.SAAS.Application.ExceptionManager.SendException("用户管理", "Views/SysUserManagement--EmployeeDelete"+e.Error.Message);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Form.FbAdminForm form = new Form.FbAdminForm();

            EntityBrowser browser = new EntityBrowser(form);

            browser.MinHeight = 370;
            browser.MinWidth = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });

            //form.Width = 400;
            //form.Height = 400;
            //EntityBrowser browser = new EntityBrowser(form);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "预算员设置", (result) => { });
        }
        void browser_ReloadDataEvent()
        {            
            InitGetData();            
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_FBADMIN");
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW UserT = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW)e.Row.DataContext;

            //Button MyAuthorBtn = DtGrid.Columns[7].GetCellContent(e.Row).FindName("AuthorizationBtn") as Button;
            //MyAuthorBtn.Tag = UserT;
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

        

    }
}
