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

using SMT.SaaS.Permission.UI.UserControls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysRole : BasePage
    {
        //private SMT.SaaS.Permission.UI.SysRoleManagementWS.T_SYS_ROLE_V role;
        private T_SYS_ROLE_V role;
        private ObservableCollection<string> DelInfosList = new ObservableCollection<string>();
        private SMTLoading loadbar = new SMTLoading();

        public T_SYS_ROLE_V SelectRole { get; set; }
        //private OrganizationServiceClient organClient = new OrganizationServiceClient();
        private List<T_HR_COMPANY> allCompanys;
        private ObservableCollection<string> DepartMentIDList = new ObservableCollection<string>();

        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allTreeCompanys;
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions;
        private DictionaryManager DictManager = new DictionaryManager();
        private List<string> ListDict = new List<string>(); //字典列表

        private BindOrgTree bingtree = new BindOrgTree();

        public T_SYS_ROLE_V Role
        {
            get { return role; }
            set { role = value; }
        }
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public SysRole()
        {
            InitializeComponent();
            InitControlEvent();
            PARENT.Children.Add(loadbar);
            ListDict.Add("SYSTEMTYPE");//系统类型
            //Utility.DisplayGridToolBarButton(FormToolBar1, "SYSROLE", true);
            this.Loaded += new RoutedEventHandler(SysRole_Loaded);
            GetEntityLogo("T_SYS_ROLE_V");

            bingtree.BindOrgTreeCompleted += new EventHandler(bingtree_BindOrgTreeCompleted);
            TreeViewItem item = new TreeViewItem();
            item.Header = "正在加载组织架构,请稍等......";
            treeOrganization.Items.Add(item);
        }

        void bingtree_BindOrgTreeCompleted(object sender, EventArgs e)
        {
            BingdOrgTreeArg arg = e as BingdOrgTreeArg;
            loadbar.Stop();
        }

        private void SysRole_Loaded(object sender, RoutedEventArgs e)
        {
            DictManager.LoadDictionary(ListDict);

            bingtree.treeOrganization = this.treeOrganization;
            bingtree.BeginBind();
        }

        private void InitToolBar()
        {
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            //隐藏未使用按钮
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;

            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;

            //FormToolBar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            DtGrid.SelectionChanged += new SelectionChangedEventHandler(DtGrid_SelectionChanged);
            FormToolBar1.ShowRect();
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DataGrid_CurrentCellChanged);
            ServiceClient.GetSysDictionaryByCategoryAsync("SYSTEMTYPE");
        }
      
        private void DtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                SelectRole = (T_SYS_ROLE_V)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体   
                
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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

        #region 编辑当前选中行
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectRole != null)
            {
                if (!string.IsNullOrEmpty(SelectRole.ROLEID))
                {
                    Form.SysRoleForms editForm = new SMT.SaaS.Permission.UI.Form.SysRoleForms(FormTypes.Edit, SelectRole.ROLEID);
                    EntityBrowser browser = new EntityBrowser(editForm);
                    browser.MinHeight = 300;
                    browser.MinWidth = 400;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        private void BtnView_Click(object sender, RoutedEventArgs e)
        {
            SMT.Saas.Tools.PermissionWS.PermissionServiceClient ps = new PermissionServiceClient();
            ps.GetFlowUserInfoByRoleIDAsync("fbce7ce8-7e45-4bd9-93db-69f40d0a4d47");
            if (SelectRole != null)
            {
                if (!string.IsNullOrEmpty(SelectRole.ROLEID))
                {
                    Form.SysRoleForms editForm = new SMT.SaaS.Permission.UI.Form.SysRoleForms(FormTypes.Browse, SelectRole.ROLEID);
                    EntityBrowser browser = new EntityBrowser(editForm);
                    browser.MinHeight = 300;
                    browser.MinWidth = 400;
                    browser.FormType = FormTypes.Browse;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTRECORDER"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }
        #endregion

        #region 新建数据
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysRoleForms editForm = new SMT.SaaS.Permission.UI.Form.SysRoleForms(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(editForm);

            browser.MinHeight = 300;
            browser.MinWidth = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void browser_ReloadDataEvent()
        {
            SelectRole = null;
            LoadData();
        }
        #endregion

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DelInfosList.Clear();
            if (this.DtGrid.SelectedItems != null)
            {
                foreach (T_SYS_ROLE_V n in this.DtGrid.SelectedItems)
                {
                    
                    if (!(DelInfosList.IndexOf(n.ROLEID) > 0))
                    {
                        DelInfosList.Add(n.ROLEID);
                    }
                }
                //if (!(DelInfosList.IndexOf(SelectRole.ROLEID) > 0))
                //{
                //    DelInfosList.Add(SelectRole.ROLEID);
                //}
            }            

            if (DelInfosList.Count() > 0)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ServiceClient.SysRoleBatchDelAsync(DelInfosList);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请您选择需要删除的数据！", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        protected static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
       // protected PermissionServiceClient ServiceClient;
        //private SysRoleManagementServiceClient SysRoleClient = new SysRoleManagementServiceClient();

        private void InitControlEvent()
        {
            

            ServiceClient = new PermissionServiceClient();            
            ServiceClient.SysRoleBatchDelCompleted += new EventHandler<SysRoleBatchDelCompletedEventArgs>(SysRoleClient_SysRoleBatchDelCompleted);
            
            ServiceClient.GetSysDictionaryByCategoryCompleted+=new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(ServiceClient_GetSysDictionaryByCategoryCompleted);
            //ServiceClient.GetSysRoleInfosPagingCompleted += new EventHandler<GetSysRoleInfosPagingCompletedEventArgs>(ServiceClient_GetSysRoleInfosPagingCompleted);
            //ServiceClient.GetSysRoleInfosPagingByCompanyIDsCompleted += new EventHandler<GetSysRoleInfosPagingByCompanyIDsCompletedEventArgs>(ServiceClient_GetSysRoleInfosPagingByCompanyIDsCompleted);
            ServiceClient.GetRoleViewCompleted += ServiceClient_GetRoleViewCompleted;
            //绑定系统类型
            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
        }

        void ServiceClient_GetRoleViewCompleted(object sender, GetRoleViewCompletedEventArgs e)
        {
            loadbar.Stop();
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    try
                    {
                        if (e.Result != null)
                        {
                            if (e.Result.Count() > 0)
                            {
                                BindDataGrid(e.Result.ToList(), e.pageCount);
                            }
                            else
                            {
                                BindDataGrid(null, 0);
                            }
                        }
                        else
                        {
                            BindDataGrid(null, 0);
                        }
                    }
                    catch (Exception ex)
                    {SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "SysRole", "Views/SysRole.xaml", "SysRole", ex);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        return;
                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
                    SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "Views/SysRole.xaml--GetSysRoleInfosPagingByCompanyIDs");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }


            }
        }

        private void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                InitToolBar();               
                LoadData();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        //private void ServiceClient_GetSysRoleInfosPagingByCompanyIDsCompleted(object sender, GetSysRoleInfosPagingByCompanyIDsCompletedEventArgs e)
        //{
        //    //loadbar.Stop();
        //    //if (!e.Cancelled)
        //    //{
        //    //    if (e.Error == null)
        //    //    {
        //    //        try
        //    //        {
        //    //            if (e.Result != null)
        //    //            {


        //    //                if (e.Result.Count() > 0)
        //    //                {
        //    //                    BindDataGrid(e.Result.ToList(), e.pageCount);
        //    //                }
        //    //                else
        //    //                {
        //    //                    BindDataGrid(null, 0);
        //    //                }

        //    //            }
        //    //            else
        //    //            {
        //    //                BindDataGrid(null, 0);
        //    //            }
                        
        //    //        }
        //    //        catch (Exception ex)
        //    //        {

        //    //            //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    //            SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "SysRole", "Views/SysRole.xaml", "SysRole", ex);
        //    //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),                            
        //    //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    //            return;
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.ToString());
        //    //        SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "Views/SysRole.xaml--GetSysRoleInfosPagingByCompanyIDs");
        //    //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
        //    //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    //        return;
        //    //    }


        //    //}
        //}
    
        private void ServiceClient_GetSysRoleInfosPagingCompleted(object sender, GetSysRoleInfosPagingCompletedEventArgs e)
        {
            //loadbar.Stop();
            //if (!e.Cancelled)
            //{
            //    if (e.Error == null)
            //    {
            //        try
            //        {
            //            if (e.Result != null)
            //            {

            //                BindDataGrid(e.Result.ToList(), e.pageCount);
            //            }
            //            else
            //            {
            //                BindDataGrid(null, 0);
            //            }
                        
            //        }
            //        catch (Exception ex)
            //        {
            //            SMT.SAAS.Application.ExceptionManager.SendException("角色管理","SysRole","Views/SysRole.xaml","SysRole", ex);
                     
            //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
            //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        //Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),e.Error.ToString());
            //        SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "Views/SysRole.xaml--GetSysRoleInfosPaging");
            //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
            //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            //        return;
            //    }


            //}
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_ROLE_V> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DtGrid.ItemsSource = null;
                return;
            }
            DtGrid.ItemsSource = obj;

            DtGrid.CacheMode = new BitmapCache();
        }

        private void ServiceClient_SysRoleDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "Views/SysRole.xaml--SysRoleDelete"+e.Error.Message);
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSED"), Utility.GetResourceStr("CONFIRMBUTTON"));
               
                //刷新数据
                ShowPageStyle();

                LoadData();
            }
        }

        private void SysRoleClient_GetSysRoleInfosCompleted(object sender, GetSysRoleInfosCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 20;
            }

            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;
            loadbar.Stop();
            HidePageStyle();
        }

        
        /// <summary>
        /// 加载菜单数据
        /// </summary>
        private void LoadData()
        {
            //ServiceClient.GetSysRoleByTypeAsync(this.txtSearchSystemType.Text.Trim());
            string filter = "";    //查询过滤条件
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
            TextBox RoleName = Utility.FindChildControl<TextBox>(expander, "TxtRoleName");
            T_SYS_DICTIONARY dict = cbxSystemType.SelectedItem as T_SYS_DICTIONARY;
            string StrRoleName = "";
            StrRoleName = RoleName.Text;

           
            if (!string.IsNullOrEmpty(StrRoleName))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                
                //filter += " @" + paras.Count().ToString() + ".Contains(ROLENAME)";//类型名称
                //filter += "ROLENAME  ^@" + paras.Count().ToString();//类型名称
                filter += " @" + paras.Count().ToString() + ".Contains(ROLENAME)";
                paras.Add(StrRoleName);
            }
            //if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
            //{
            //    //filter += "EMPLOYEECODE==@" + paras.Count().ToString();
            //    filter += " @" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)";
            //    paras.Add(txtEmpCode.Text.Trim());
            //}


            string systype = "";
            if(dict != null)
                systype = dict.DICTIONARYVALUE == null ? "" : dict.DICTIONARYVALUE.GetValueOrDefault().ToString();
            if (!string.IsNullOrEmpty(systype))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "SYSTEMTYPE ==@" + paras.Count().ToString();//类型名称
                paras.Add(systype);
            }
            string sValue = "";
            TreeViewItem selectedItem = treeOrganization.SelectedItem as TreeViewItem;
            if (selectedItem != null)
            {
                string IsTag = selectedItem.Tag.ToString();
                switch (IsTag)
                {
                    case "Company":
                        SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                        //sType = "Company";
                        sValue = company.COMPANYID;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OWNERCOMPANYID ==@" + paras.Count().ToString();//类型名称
                        paras.Add(sValue);
                        break;
                    case "Department":
                        SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = selectedItem.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                        //sType = "Department";
                        sValue = department.DEPARTMENTID;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "OWNERDEPARTMENTID ==@" + paras.Count().ToString();//类型名称
                        paras.Add(sValue);
                        break;
                    
                }
            }
            if (string.IsNullOrEmpty(filter))
            {
                //默认为自己公司的角色
                filter += "OWNERCOMPANYID ==@" + paras.Count().ToString();//类型名称
                paras.Add(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            }
            
            loadbar.Start();
            SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            //只能查看自己所在公司的角色
            //CompanyIDList.Clear();
            //CompanyIDList.Add(Common.CurrentLoginUserInfo.UserPosts);
            //if (Common.CurrentLoginUserInfo.UserPosts.Count() > 0)
            //{
            //    foreach (var str in Common.CurrentLoginUserInfo.UserPosts)
            //    {
            //        CompanyIDList.Add(str.CompanyID);
            //    }
            //}
            var userPost = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts;
            string ownerDepartmentid = string.Empty;
            foreach (var post in userPost)
            {
                ownerDepartmentid = ownerDepartmentid + post.DepartmentID + ",";
            }
            DepartMentIDList.Add(ownerDepartmentid);
            //ServiceClient.GetSysRoleInfosPagingByCompanyIDsAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", 
            //    filter, paras, pageCount, loginUserInfo,DepartMentIDList);
            ServiceClient.GetRoleViewAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE",
                filter, paras, pageCount, loginUserInfo, DepartMentIDList);
            DepartMentIDList.Clear();
            
        }

        private void ServiceClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            //绑定系统类型
            if (e.Result != null)
            {
                List<T_SYS_DICTIONARY> dicts = e.Result.ToList();
                ComboBox cbxSystemType = Utility.FindChildControl<ComboBox>(expander, "cbxSystemType");
                T_SYS_DICTIONARY NewDict = new T_SYS_DICTIONARY();
                NewDict.DICTIONARYNAME = "--请选择--";
                NewDict.DICTIONARYVALUE = null;
                dicts.Insert(0,NewDict);
                if (cbxSystemType != null)
                {
                    cbxSystemType.ItemsSource = dicts;
                    cbxSystemType.DisplayMemberPath = "DICTIONARYNAME";
                    cbxSystemType.SelectedIndex = 0;
                }
            }
        }

        #region "添加，修改，删除"

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Role = (T_SYS_ROLE_V)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void SysRoleClient_SysRoleBatchDelCompleted(object sender, SysRoleBatchDelCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "删除数据成功!", Utility.GetResourceStr("CONFIRMBUTTON"));
                    this.LoadData();
                }
                else
                {
                    if (e.Result == "error")
                    {
                        SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "Views/SysRole.xaml--SysRoleDelete" + e.Error.Message);
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "系统出现错误，请与管理员联系!", Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Result.ToString(), Utility.GetResourceStr("CONFIRMBUTTON"));              
                        this.LoadData();
                    }
                }
            }
        }

        private void ButtonDeleteOK_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void addform_ReloadDataEvent()
        {
            LoadData();
        }

        #endregion

        #region LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //T_OA_SENDDOC OrderInfoT = (T_OA_SENDDOC)e.Row.DataContext;
            T_SYS_ROLE_V RoleT = (T_SYS_ROLE_V)e.Row.DataContext;
           
            Button MyAuthorBtn = DtGrid.Columns[5].GetCellContent(e.Row).FindName("AuthorizationBtn") as Button;

            MyAuthorBtn.Tag = RoleT;

            SetRowLogo(DtGrid, e.Row, "T_SYS_ROLE_V");
        }

        #endregion

        #region 授权
        private void AuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {
            Button AuthorBtn = sender as Button;
            T_SYS_ROLE_V AuthorRole = AuthorBtn.Tag as T_SYS_ROLE_V;
            T_SYS_ROLE role = new T_SYS_ROLE();
            role.ROLEID = AuthorRole.ROLEID;

            //SysRoleSetMenu UserInfo = new SysRoleSetMenu(AuthorRole);
            SysRoleSetMenu2 UserInfo = new SysRoleSetMenu2(role);//龙康才新增
            EntityBrowser browser = new EntityBrowser(UserInfo);
            browser.MinWidth = 850;
            browser.MinHeight = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{},true);
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void CustomAuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {
            string strRoleID = string.Empty;

            if (DtGrid.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DtGrid.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_SYS_ROLE_V entRole = DtGrid.SelectedItems[0] as T_SYS_ROLE_V;
            //SysRoleSetMenu UserInfo = new SysRoleSetMenu(AuthorRole);
            strRoleID = entRole.ROLEID;
            //RoleCustomMenuPermission UserInfo = new RoleCustomMenuPermission(FormTypes.Edit, strRoleID);//自定义权限
            EntityMenuCustomerPermission2 UserInfo = new EntityMenuCustomerPermission2(FormTypes.Edit, strRoleID);//自定义权限
            EntityBrowser browser = new EntityBrowser(UserInfo);
            browser.MinWidth = 850;
            browser.MinHeight = 520;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{},true);

            
        }
    }
}
