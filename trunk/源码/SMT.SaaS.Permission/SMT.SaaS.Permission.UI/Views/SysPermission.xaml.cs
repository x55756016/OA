using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.Permission.UI.Form;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysPermission : BasePage
    {
        protected static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
        //protected PermissionServiceClient ServiceClient;
        private SMTLoading loadbar = new SMTLoading(); 
        public SysPermission()
        {
            InitializeComponent();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "SYSPERMISSION", true);
            PARENT.Children.Add(loadbar);
            this.Loaded += new RoutedEventHandler(SysPermission_Loaded);
        }

        void SysPermission_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            //隐藏未使用按钮
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;
            
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            InitControlEvent();
            ServiceClient.GetUserPermissionBySysCodeAndUserAndModelCodeAsync("1","T_OA_APPROVALINFO",Common.CurrentLoginUserInfo.EmployeeID,"3");
            GetEntityLogo("T_SYS_PERMISSION");
            FormToolBar1.ShowRect();
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Permission == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDER"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            SysPermissionForms addForm = new SysPermissionForms(FormTypes.Browse, Permission.PERMISSIONID);
            EntityBrowser browser = new EntityBrowser(addForm);
            browser.MinHeight = 350;
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Permission != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ServiceClient.SysPermissionDeleteAsync(Permission.PERMISSIONID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {                
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请选择需要删除的记录",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Permission == null)
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            SysPermissionForms addForm = new SysPermissionForms(FormTypes.Edit, Permission.PERMISSIONID);
            EntityBrowser browser = new EntityBrowser(addForm);
            browser.MinWidth = 450;
            browser.MinHeight = 350;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});

        }

        void browser_ReloadDataEvent()
        {
            Permission = null;
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            SysPermissionForms addForm = new SysPermissionForms(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(addForm);
            browser.MinWidth = 450;
            browser.MinHeight = 350;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        #region "载入数据及载入事件"
        private void LoadData()
        {
            ShowPageStyle();

            string filter = "";    //查询过滤条件
            string StrName = "";
            int pageCount = 0;
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            TextBox Name = Utility.FindChildControl<TextBox>(expander, "txtName");
            StrName = Name.Text.Trim().ToString();


            if (!string.IsNullOrEmpty(StrName))
            {
                filter += "PERMISSIONNAME ^@" + paras.Count().ToString();//类型名称
                paras.Add(StrName);
            }
            loadbar.Start();
            SMT.Saas.Tools.PermissionWS.LoginUserInfo loginUserInfo = new SMT.Saas.Tools.PermissionWS.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            ServiceClient.GetSysPermissionAllPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE", filter, paras, pageCount, loginUserInfo);
            
        }

        void ServiceClient_GetSysPermissionAllCompleted(object sender, GetSysPermissionAllCompletedEventArgs e)
        {
            List<T_SYS_PERMISSION> perlist = new List<T_SYS_PERMISSION>();
            if (e.Result != null)
            {
                perlist = e.Result.ToList();
            }
            DataBinder(perlist);
        }

        private void DataBinder(List<T_SYS_PERMISSION> perflist)
        {
            PagedCollectionView pcv = null;

            if (perflist != null && perflist.Count > 0)
            {
                var q = from ent in perflist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
            }

            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;
            HidePageStyle();
        }

        private void InitControlEvent()
        {
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);
            Button btnFind = Utility.FindChildControl<Button>(expander, "btnFind");
            btnFind.Click += new RoutedEventHandler(btnFind_Click);

            ServiceClient = new PermissionServiceClient();
            ServiceClient.GetSysPermissionAllCompleted += new EventHandler<GetSysPermissionAllCompletedEventArgs>(ServiceClient_GetSysPermissionAllCompleted);            
            ServiceClient.SysPermissionDeleteCompleted += new EventHandler<SysPermissionDeleteCompletedEventArgs>(ServiceClient_SysPermissionDeleteCompleted);
            ServiceClient.FindSysPermissionByStrCompleted += new EventHandler<FindSysPermissionByStrCompletedEventArgs>(ServiceClient_FindSysPermissionByStrCompleted);
            ServiceClient.GetSysPermissionAllPagingCompleted += new EventHandler<GetSysPermissionAllPagingCompletedEventArgs>(ServiceClient_GetSysPermissionAllPagingCompleted);
        }

        void ServiceClient_SysPermissionDeleteCompleted(object sender, SysPermissionDeleteCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Result))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if (e.Error != null && e.Error.Message != "" )
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"),Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESS"), Utility.GetResourceStr("CONFIRMBUTTON"));
                Permission = null;
                LoadData();
            }
        }

        void ServiceClient_GetSysPermissionAllPagingCompleted(object sender, GetSysPermissionAllPagingCompletedEventArgs e)
        {
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

                        
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
                else
                {
                    
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }


            }

            loadbar.Stop();
        }

        //  绑定DataGird
        private void BindDataGrid(List<T_SYS_PERMISSION> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                DtGrid.ItemsSource = null;
                return;
            }
            DtGrid.ItemsSource = obj;
        }
        #endregion

        #region "添加，修改，删除，查找"
        private T_SYS_PERMISSION permission;
        public T_SYS_PERMISSION Permission
        {
            get { return permission; }
            set { permission = value; }
        }
        void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
            //TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
            //string pername = txtName.Text;
            //ServiceClient.FindSysPermissionByStrAsync(pername);
        }

        void ServiceClient_FindSysPermissionByStrCompleted(object sender, FindSysPermissionByStrCompletedEventArgs e)
        {
            List<T_SYS_PERMISSION> permlist = new List<T_SYS_PERMISSION>();
            if (e.Result != null)
            {
                permlist = e.Result.ToList();
            }
            DataBinder(permlist);
        }

        //选取当前行
        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Permission = (T_SYS_PERMISSION)grid.SelectedItems[0];
            }
        }
                

        void editForm_ReloadDataEvent()
        {
           
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_PERMISSION");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        

        
    }
}
