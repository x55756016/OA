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
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.Permission.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.Globalization;


namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysUserRole : BasePage,IEntityEditor
    {
        private static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
        
       // PermissionServiceClient ServiceClient;
        private string  tmpUserID="";
        private T_SYS_USER tmpUser = new T_SYS_USER();
        
        public SysUserRole(T_SYS_USER UserObj)
        {
            InitializeComponent();
            
            tmpUser = UserObj;
            tmpUserID = tmpUser.SYSUSERID;
            InitControlEvent();
            //LoadData();
            
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //tmpUserID = this.NavigationContext.QueryString.Count()==0 ? "" : this.NavigationContext.QueryString["userid"].ToString();
            //ServiceClient.GetSysUserSingleInfoByIdCompleted += new EventHandler<GetSysUserSingleInfoByIdCompletedEventArgs>(ServiceClient_GetSysUserSingleInfoByIdCompleted);
            
            
        }
        void ServiceClient_GetUserByIDCompleted(object sender, GetUserByIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpUser = e.Result;
                    tmpUserID = tmpUser.SYSUSERID;
                    LoadData();

                }
            }
        }

        #region 绑定数据及载入事件
        private void LoadData()
        {
            //ServiceClient.GetSysUserRoleByTypeAsync(this.txtSearchSystemType.Text.Trim());
            RefreshUI(RefreshedTypes.ShowProgressBar);
            ServiceClient.GetSysUserRoleByUserAsync(tmpUserID);
        }

        void ServiceClient_GetSysUserRoleByTypeCompleted(object sender, GetSysUserRoleByTypeCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_USERROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
            }

            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;

            HidePageStyle();
        }

        void ServiceClient_GetSysUserRoleByUserCompleted(object sender, GetSysUserRoleByUserCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_USERROLE> menulist = e.Result.ToList();
                var q = from ent in menulist
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
            GetEntityLogo("T_SYS_USERROLE"); 
            gridToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            gridToolBar.btnNew.Content = "添加角色";
            gridToolBar.btnEdit.Visibility = Visibility.Collapsed;
            //gridToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            //gridToolBar.btnEdit.Visibility = Visibility.Collapsed;
            gridToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            gridToolBar.btnAudit.Visibility = Visibility.Collapsed;
            gridToolBar.BtnView.Visibility = Visibility.Collapsed;
            gridToolBar.btnAudit.Visibility = Visibility.Collapsed;
            gridToolBar.stpCheckState.Visibility = Visibility.Collapsed;
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);
            gridToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);

            gridToolBar.ShowRect();
            ServiceClient = new PermissionServiceClient();

            //ServiceClient.GetSysUserRoleByTypeCompleted += new EventHandler<GetSysUserRoleByTypeCompletedEventArgs>(ServiceClient_GetSysUserRoleByTypeCompleted);
            ServiceClient.GetSysUserRoleByUserCompleted += new EventHandler<GetSysUserRoleByUserCompletedEventArgs>(ServiceClient_GetSysUserRoleByUserCompleted);
            ServiceClient.SysUserRoleDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysUserRoleDeleteCompleted);
            ServiceClient.GetUserByIDCompleted += new EventHandler<GetUserByIDCompletedEventArgs>(ServiceClient_GetUserByIDCompleted);
            
            ServiceClient.GetUserByIDAsync(tmpUserID);
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 添加，修改，删除及查询
        private T_SYS_USERROLE userRole;

        public T_SYS_USERROLE UserRole
        {
            get { return userRole; }
            set { userRole = value; }
        }
        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count > 0)
            {
                UserRole = (T_SYS_USERROLE)grid.SelectedItems[0]; //获取当前选中的行数据并转换为对应的实体
            }
            else
            {
                UserRole = null;
            }
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            SysUserRoleSet UserRole = new SysUserRoleSet(tmpUser,null);
            EntityBrowser browser = new EntityBrowser(UserRole);
            browser.MinHeight = 300;
            browser.MinWidth = 650;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        void AddWin_ReloadDataEvent()
        {
            LoadData();
        }

        void form_ReloadDataEvent()
        {
            LoadData();
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (UserRole == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            SysUserRoleSet UserRoleForm = new SysUserRoleSet(tmpUser, UserRole);            
            EntityBrowser browser = new EntityBrowser(UserRoleForm);
            browser.MinHeight = 640;
            browser.MinWidth = 500;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(AddWin_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);            
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (UserRole != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ServiceClient.SysUserRoleDeleteAsync(UserRole.USERROLEID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "请选择角色！", Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
        }

        void ServiceClient_SysUserRoleDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ShowPageStyle();
                //刷新数据
                LoadData();
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
               
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_USERROLE");
        }

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("SYSUERROLE");
            return tmpUser.EMPLOYEENAME + "所拥有的角色";
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            SaveAndClose();
        }

        #region 保存
        private void Save()
        {
            
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        #endregion

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "角色",
                Tooltip = "员工角色"
            };
            items.Add(item);
            item = new NavigateItem
            {
                Title = "员工信息",
                Tooltip = "详细信息",
                Url = "/SysUserInfoPage.xaml?userid=" + tmpUser.SYSUSERID
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = "关闭",
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
            };

            items.Add(item);
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
