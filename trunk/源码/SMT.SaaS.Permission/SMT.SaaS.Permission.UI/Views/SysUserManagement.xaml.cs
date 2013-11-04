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
    /// <summary>
    /// 系统用户列表-ken
    /// </summary>
    public partial class SysUserManagement : BasePage
    {

        SMTLoading loadbar = new SMTLoading();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient client;
        private PermissionServiceClient permclient;// = new PermissionServiceClient();
       
        public SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW SelectedEmployee { get; set; }
        private BindOrgTree bingtree = new BindOrgTree();
       
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表

        public SysUserManagement()
        {
            InitializeComponent();
            InitPara();
            GetEntityLogo("T_SYS_USER");
            ListDict.Add("SYSTEMTYPE");//系统类型
            this.Loaded += new RoutedEventHandler(SysUserManagement_Loaded);
            bingtree.BindOrgTreeCompleted += new EventHandler(bingtree_BindOrgTreeCompleted);
            TreeViewItem item = new TreeViewItem();
            item.Header = "正在加载组织架构,请稍等......";
            treeOrganization.Items.Add(item);
            //if (Application.Current.Resources["DictionaryConverter"] == null)
            //{
            //    Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.Permission.UI.DictionaryConverter());
            //}
        }

        void bingtree_BindOrgTreeCompleted(object sender, EventArgs e)
        {
            BingdOrgTreeArg arg = e as BingdOrgTreeArg;
            loadbar.Stop();
        }

        void SysUserManagement_Loaded(object sender, RoutedEventArgs e)
        {            
            //BindTree();
            bingtree.treeOrganization = this.treeOrganization;
            bingtree.BeginBind();
            DictManager.LoadDictionary(ListDict);
            
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
                loadbar.Start();

                DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
                client = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
                client.GetEmployeePagingCompleted += new EventHandler<GetEmployeePagingCompletedEventArgs>(client_GetEmployeePagingCompleted);
                //client.GetEmployeesWithOutPermissionsCompleted += new EventHandler<GetEmployeesWithOutPermissionsCompletedEventArgs>(client_GetEmployeesWithOutPermissionsCompleted);
                client.GetEmployeeViewsPagingCompleted += new EventHandler<GetEmployeeViewsPagingCompletedEventArgs>(client_GetEmployeeViewsPagingCompleted);
                client.EmployeeDeleteCompleted += new EventHandler<EmployeeDeleteCompletedEventArgs>(client_EmployeeDeleteCompleted);

                //client.GetEmployeeViewsActivedPagingCompleted += new EventHandler<GetEmployeeViewsActivedPagingCompletedEventArgs>(client_GetEmployeeViewsActivedPagingCompleted);
                  
                permclient = new PermissionServiceClient();
                permclient.GetUserByEmployeeIDCompleted += new EventHandler<Saas.Tools.PermissionWS.GetUserByEmployeeIDCompletedEventArgs>(permclient_GetUserByEmployeeIDCompleted);
                //this.Loaded += new RoutedEventHandler(Employee_Loaded);
                
                
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

        //void client_GetEmployeeViewsActivedPagingCompleted(object sender, GetEmployeeViewsActivedPagingCompletedEventArgs e)
        //{
        //    List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW> list = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW>();
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //        }
        //        DtGrid.ItemsSource = list;
        //        dataPager.PageCount = e.pageCount;
        //    }
        //    loadbar.Stop();
        //}

        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                InitToolBarEvent();
                
                LoadData();
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

            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);
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
                    browser.MinHeight = 300;
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

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
            }
            loadbar.Stop();
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
           // client.GetEmployeeViewsActivedPagingAsync(dataPager.PageIndex, dataPager.PageSize,"EMPLOYEECNAME", filter, paras, pageCount, sType, sValue,SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            client.GetEmployeeViewsPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEECNAME", filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
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
            //Form.Personnel.EmployeeForm form = new SMT.HRM.UI.Form.Personnel.EmployeeForm(FormTypes.New, "");
            //EntityBrowser browser = new EntityBrowser(form);

            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_USER");
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW UserT = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW)e.Row.DataContext;

            Button MyAuthorBtn = DtGrid.Columns[7].GetCellContent(e.Row).FindName("AuthorizationBtn") as Button;
            MyAuthorBtn.Tag = UserT;
        }

        private void treeOrganization_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            LoadData();
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                OrgTreeItemTypes type = (OrgTreeItemTypes)(selectedItem.Tag);
                if (type == OrgTreeItemTypes.Department)//部门时才加载
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                    string sValue = department.DEPARTMENTID;
                    if (bingtree.depIDsCach.ContainsKey(sValue)) return;//如果有加载过就不再再加载一次
                    bingtree.BindPost(sValue);
                }
            }
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();

            //orgClient.DoClose();
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
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW AuthorUser = AuthorBtn.Tag as SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW;
            permclient.GetUserByEmployeeIDAsync(AuthorUser.EMPLOYEEID);

        }
    }
}
