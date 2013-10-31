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

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysPermMenu : BasePage
    {
        public SysPermMenu()
        {
            InitializeComponent();
            //InitControlEvent();
            //LoadData();
            GetEntityLogo("T_SYS_PERM_MENU");
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        protected static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
       // protected PermissionServiceClient ServiceClient;

        //private void InitControlEvent()
        //{
        //    gridToolBar.BtnAdd.Click += new RoutedEventHandler(BtnAdd_Click);
        //    gridToolBar.BtnAlter.Click += new RoutedEventHandler(BtnAlter_Click);
        //    gridToolBar.BtnDelete.Click += new RoutedEventHandler(BtnDelete_Click);
        //    DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DataGrid_CurrentCellChanged);

        //    ServiceClient = new PermissionServiceClient();
        //    ServiceClient.GetSysPermMenuByTypeCompleted += new EventHandler<GetSysPermMenuByTypeCompletedEventArgs>(ServiceClient_GetSysPermMenuByTypeCompleted);
        //    ServiceClient.SysPermMenuDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysPermMenuDeleteCompleted);
        //}

        //void ServiceClient_SysPermMenuDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        MessageBox.Show(e.Error.Message);
        //    }
        //    else
        //    {
        //        MessageBox.Show(Resource.DELETESUCCESSEDCONFIRMCONFIRM);
        //        //刷新数据
        //        ShowPageStyle();

        //        LoadData();
        //    }
        //}

        //void ServiceClient_GetSysPermMenuByTypeCompleted(object sender, GetSysPermMenuByTypeCompletedEventArgs e)
        //{
        //    PagedCollectionView pcv = null;
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_PERM_MENU> menulist = e.Result.ToList();
        //        var q = from ent in menulist
        //                select ent;

        //        pcv = new PagedCollectionView(q);
        //        pcv.PageSize = 25;
        //    }

        //    dataPager.DataContext = pcv;
        //    DtGrid.ItemsSource = pcv;

        //    HidePageStyle();
        //}

        ///// <summary>
        ///// 加载菜单数据
        ///// </summary>
        //private void LoadData()
        //{
        //    ServiceClient.GetSysPermMenuByTypeAsync(this.txtSearchSystemType.Text.Trim());
        //}


        #region "添加，修改，删除"
        //private T_SYS_PERM_MENU permMenu;

        //public T_SYS_PERM_MENU PermMenu
        //{
        //    get { return permMenu; }
        //    set { permMenu = value; }
        //}
        void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            //DataGrid grid = sender as DataGrid;
            //if (grid.SelectedItem != null)
            //{
            //    PermMenu = (T_SYS_PERM_MENU)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            //}
        }

        void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //LoadData();
        }

        void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            //if (PermMenu != null)
            //{
            //    string Result = "";
            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        ServiceClient.SysPermMenuDeleteAsync(PermMenu.ID);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            //}
        }

        void BtnAlter_Click(object sender, RoutedEventArgs e)
        {
            //if (PermMenu == null)
            //{
            //    MessageBox.Show(Utility.GetResourceStr("SELECTDATAALERT"));
            //}
            
            //Form.SysPermMenuForm editForm = new SMT.SaaS.Permission.UI.Form.SysPermMenuForm(FormTypes.Edit, PermMenu.ID);
            //editForm.ReloadDataEvent += new Form.BaseForm.refreshGridView(addform_ReloadDataEvent);
            //editForm.Show();
        }

        void addform_ReloadDataEvent()
        {
            //LoadData();
        }

        void BtnAdd_Click(object sender, RoutedEventArgs e)
        {

            Form.SysPermMenuForm editForm = new SMT.SaaS.Permission.UI.Form.SysPermMenuForm(FormTypes.New);
            editForm.Show();
            editForm.ReloadDataEvent += new BaseForm.refreshGridView(AddWin_ReloadDataEvent);
        }

        void AddWin_ReloadDataEvent()
        {
            //LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
           // SetRowLogo(DtGrid, e.Row, "T_SYS_PERM_MENU");
        }
    }
}
