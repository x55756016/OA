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
using SMT.SaaS.Permission.UI.UserControls;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class UserRoleManagement : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        private string checkState = ((int)CheckStates.All).ToString();
        private PermissionServiceClient permClient = new Saas.Tools.PermissionWS.PermissionServiceClient();
        private T_SYS_ROLE roleInfo = null;
        private string SearchUserID = "";//已员工名为查询条件
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表
        public UserRoleManagement()
        {
            InitializeComponent();
            InitEvent();
            ListDict.Add("SYSTEMTYPE");//系统类型
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            
            this.Loaded += new RoutedEventHandler(UserRoleManagement_Loaded);
        }

        #region 数据初始化
        private void InitEvent()
        {
            
            

            permClient.SysRoleBatchDelCompleted += new EventHandler<SysRoleBatchDelCompletedEventArgs>(SysRoleClient_SysRoleBatchDelCompleted);

            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
            //ServiceClient.GetSysRoleInfosPagingCompleted += new EventHandler<GetSysRoleInfosPagingCompletedEventArgs>(ServiceClient_GetSysRoleInfosPagingCompleted);
            //permClient.GetSysRoleInfosPagingByCompanyIDsCompleted += new EventHandler<GetSysRoleInfosPagingByCompanyIDsCompletedEventArgs>(ServiceClient_GetSysRoleInfosPagingByCompanyIDsCompleted);
            //绑定系统类型
            permClient.GetRoleInfosByUserCompleted += new EventHandler<Saas.Tools.PermissionWS.GetRoleInfosByUserCompletedEventArgs>(permClient_GetRoleInfosByUserCompleted);

        }
        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                InitFormToolBar();
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        void permClient_GetRoleInfosByUserCompleted(object sender, Saas.Tools.PermissionWS.GetRoleInfosByUserCompletedEventArgs e)
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

                            BindDataGrid(e.Result.ToList(), e.pageCount);
                        }
                        else
                        {
                            BindDataGrid(null, 0);
                        }

                    }
                    catch (Exception ex)
                    {

                        //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        SMT.SAAS.Application.ExceptionManager.SendException("角色管理", "SysRole", "Views/SysRole.xaml", "SysRole", ex);
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
        void ServiceClient_GetSysRoleInfosPagingByCompanyIDsCompleted(object sender, GetSysRoleInfosPagingByCompanyIDsCompletedEventArgs e)
        {
            
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_ROLE> obj, int pageCount)
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

        void SysRoleClient_SysRoleBatchDelCompleted(object sender, SysRoleBatchDelCompletedEventArgs e)
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
        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                roleInfo = (T_SYS_ROLE)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }
        #endregion
        #region TOOLBAR按钮事件
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DtGrid.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DtGrid.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_SYS_ROLE ent = DtGrid.SelectedItems[0] as T_SYS_ROLE;
            if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                UserRoleApplyForm frm = new UserRoleApplyForm(FormTypes.Audit, ent.ROLEID);
                //frm.ApprovalInfo = roleInfo;
                EntityBrowser browser = new EntityBrowser(frm);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 760;
                browser.MinHeight = 340;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                return;
            }
        }
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_SYS_ROLE> roleInfoList = GetSelectedList(Permissions.Browse);
            if (roleInfoList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            roleInfo = roleInfoList.FirstOrDefault();
            UserRoleApplyForm frm = new UserRoleApplyForm(FormTypes.Browse, roleInfo.ROLEID);
            
            EntityBrowser browser = new EntityBrowser(frm);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 760;
            browser.MinHeight = 360;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_SYS_ROLEAPP");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_SYS_ROLE> roleInfoList = GetSelectedList(Permissions.Edit);
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

            roleInfo = roleInfoList[0];
            //if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(roleInfo, "T_SYS_ROLEAPP", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            //{
                if (roleInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() )
                {
                    //ApprovalForm_upd frm = new ApprovalForm_upd(FormTypes.Edit, roleInfo.APPROVALID);
                    UserRoleApplyForm frm = new UserRoleApplyForm(FormTypes.Edit, roleInfo.ROLEID);
                    //frm.ApprovalInfo = roleInfo;
                    EntityBrowser browser = new EntityBrowser(frm);
                    browser.FormType = FormTypes.Edit;
                    browser.MinWidth = 860;
                    browser.MinHeight = 500;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            //}
            //else
            //{


            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);

            //}
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        #region 删除
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_SYS_ROLE> selectItemList = new ObservableCollection<T_SYS_ROLE>();

            foreach (object obj in DtGrid.SelectedItems)
            {

                if (((T_SYS_ROLE)obj).CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    selectItemList.Add((T_SYS_ROLE)obj);
                }
                else
                {
                    string strState = ((T_SYS_ROLE)obj).CHECKSTATE;
                    switch (strState)
                    {
                        case "1":
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核中的数据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            break;
                        case "2":
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核通过的数据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            break;
                        case "3":
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "不能删除审核不通过的数据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            break;
                    }
                    return;
                }



            }
            if (selectItemList == null)
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

            string Result = "";
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (objcom, result) =>
            {

                if (selectItemList.Count > 0)
                {
                    try
                    {
                        string StrTip = "";
                        int i = 0;
                        bool IsTrue = true;
                        foreach (object obj in selectItemList)
                        {
                            if (!(SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission((T_SYS_ROLE)obj, "T_SYS_ROLEAPP", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID)))
                            {
                                //StrTip = "您不能删除您选中的第" + (i + 1).ToString() + "条，标题为" + ((T_SYS_ROLE)obj).APPROVALTITLE + "的信息";
                                //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrTip, Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                //IsTrue = false;
                                //return;
                            }
                        }

                        //approvalManagement.DeleteApporvalListAsync(selectItemList);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
        }
        //获取选中列表
        private ObservableCollection<T_SYS_ROLE> GetSelectedList(Permissions PermissionState)
        {
            if (DtGrid.ItemsSource != null)
            {
                ObservableCollection<T_SYS_ROLE> selectItemList = new ObservableCollection<T_SYS_ROLE>();
                if (PermissionState == Permissions.Delete)
                {

                    foreach (object obj in DtGrid.SelectedItems)
                    {

                        if (((T_SYS_ROLE)obj).CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            selectItemList.Add((T_SYS_ROLE)obj);
                        }


                    }
                }
                else
                {
                    foreach (object obj in DtGrid.SelectedItems)
                    {
                        selectItemList.Add((T_SYS_ROLE)obj);

                    }
                }
                if (selectItemList.Count > 0)
                {
                    return selectItemList;
                }
            }
            return null;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_SYS_ROLE> selectItems = GetSelectedList(Permissions.Edit);
            if (selectItems != null)
            {
                roleInfo = selectItems.FirstOrDefault();
                UserRoleApplyForm form = new UserRoleApplyForm(FormTypes.Resubmit, roleInfo.ROLEID);
                //form.ApprovalInfo = roleInfo;
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 750;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        #endregion
        #region 新建按钮
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //ApprovalForm_add frm = new ApprovalForm_add(FormTypes.New);
            UserRoleApplyForm frm = new UserRoleApplyForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(frm);
            browser.MinWidth = 860;
            browser.MinHeight = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        #endregion
        #endregion
        

        #endregion
        void UserRoleManagement_Loaded(object sender, RoutedEventArgs e)
        {
            DictManager.LoadDictionary(ListDict);

            
            //txtOwnerName.Text = Name;
            //ToolTipService.SetToolTip(txtOwnerName, Name);
        }

        private void InitFormToolBar()
        {
            
            GetEntityLogo("T_SYS_ROLEAPP");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_SYS_ROLEAPP", true);

            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            FormToolBar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DataGrid_CurrentCellChanged);
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            string Name = "";
            //loadbar.Start();
            LoadData();
            Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
        }
        #region 获取数据
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
            if (dict != null)
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
            string sType = "", sValue = "";
            
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

            //permClient.GetSysRoleInfosPagingByCompanyIDsAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo, null);
            permClient.GetRoleInfosByUserAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, checkState, loginUserInfo);

        }
        #endregion
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void AuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CustomAuthorizationBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

    }
}
