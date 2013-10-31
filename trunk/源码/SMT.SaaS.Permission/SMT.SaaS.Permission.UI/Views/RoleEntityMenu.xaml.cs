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
using SMT.SaaS.Permission.UI.Form;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class RoleEntityMenu : BasePage
    {

        private SMTLoading loadbar = new SMTLoading(); 
        public RoleEntityMenu()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            InitControlEvent();
            LoadData();

            GetEntityLogo("T_SYS_ROLEENTITYMENU");
            this.Loaded += new RoutedEventHandler(RoleEntityMenu_Loaded);
        }

        void RoleEntityMenu_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            
            //隐藏未使用按钮
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnimport.Visibility = Visibility.Collapsed;
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;
            FormToolBar1.retEdit.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.retAudit.Visibility = Visibility.Collapsed;
            //FormToolBar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
        }
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (RoleMenu != null)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ServiceClient.RoleEntityMenuDeleteAsync(RoleMenu.ROLEENTITYMENUID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (RoleMenu == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
               
            }

            Form.RoleEntityMenuForms editForm = new SMT.SaaS.Permission.UI.Form.RoleEntityMenuForms(FormTypes.Edit, RoleMenu.ROLEENTITYMENUID);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 340;
            browser.MinWidth = 400;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysPermMenuForms editForm = new SMT.SaaS.Permission.UI.Form.SysPermMenuForms(FormTypes.New);
            EntityBrowser browser2 = new EntityBrowser(editForm);
            browser2.MinHeight = 240;
            browser2.MinWidth = 300;
            browser2.ReloadDataEvent += new EntityBrowser.refreshGridView(browser2_ReloadDataEvent);
            browser2.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser2_ReloadDataEvent()
        {
            LoadData();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        protected PermissionServiceClient ServiceClient;

        private void InitControlEvent()
        {
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DataGrid_CurrentCellChanged);

            ServiceClient = new PermissionServiceClient();
            ServiceClient.GetRoleEntityMenuByTypeCompleted += new EventHandler<GetRoleEntityMenuByTypeCompletedEventArgs>(ServiceClient_GetRoleEntityMenuByTypeCompleted);
            ServiceClient.RoleEntityMenuDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_RoleEntityMenuDeleteCompleted);
        }

        void ServiceClient_RoleEntityMenuDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
                //刷新数据
                ShowPageStyle();

                LoadData();
            }
        }

        void ServiceClient_GetRoleEntityMenuByTypeCompleted(object sender, GetRoleEntityMenuByTypeCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ROLEENTITYMENU> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
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
            loadbar.Start();
            TextBox txtSearchSystemType = Utility.FindChildControl<TextBox>(expander, "txtSearchSystemType");
            ServiceClient.GetRoleEntityMenuByTypeAsync(txtSearchSystemType.Text.Trim());
        }

        #region "添加，修改，删除"
        private T_SYS_ROLEENTITYMENU roleMenu;

        public T_SYS_ROLEENTITYMENU RoleMenu
        {
            get { return roleMenu; }
            set { roleMenu = value; }
        }
        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                RoleMenu = (T_SYS_ROLEENTITYMENU)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(DtGrid);
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_ROLEENTITYMENU");
        }
    }
}
