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

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class SysRolePerm : BasePage
    {
        public SysRolePerm()
        {
            InitializeComponent();
            //InitControlEvent();
            //LoadData();
            GetEntityLogo("T_SYS_ROLE_PERM"); 
            this.Loaded += new RoutedEventHandler(SysRolePerm_Loaded);
        }

        void SysRolePerm_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
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

            FormToolBar1.ShowRect();
        }

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //if (RolePerm != null)
            //{
            //    ComfirmBox deleComfirm = new ComfirmBox();
            //    deleComfirm.Title = Resource.DELETECONFIRM;
            //    deleComfirm.MessageTextBox.Text = Resource.DELETEALTER;
            //    deleComfirm.ButtonOK.Click +=new RoutedEventHandler(ButtonOK_Click);
            //    deleComfirm.Show();
            //}
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //if (RolePerm == null)
            //{
            //    MessageBox.Show(Utility.GetResourceStr("SELECTDATAALERT"));
            //}

            //Form.SysRolePermForm editForm = new SMT.SaaS.Permission.UI.Form.SysRolePermForm(FormTypes.Edit, RolePerm.ID);
            //editForm.ReloadDataEvent += new SMT.SaaS.Permission.UI.Form.BaseForm.refreshGridView(editForm_ReloadDataEvent);
            //editForm.Show();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.SysRolePermForms editForm = new SMT.SaaS.Permission.UI.Form.SysRolePermForms(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(editForm);
            
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        void browser_ReloadDataEvent()
        {
            //LoadData();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        protected static PermissionServiceClient ServiceClient = new PermissionServiceClient();//龙康才新增
        //protected PermissionServiceClient ServiceClient;

        //#region 数据绑定及初始化事件
        //private void LoadData()
        //{
        //    ServiceClient.GetSysRolePermByTypeAsync(this.txtSearchSystemType.Text.Trim());
        //}

        //void ServiceClient_GetSysRolePermByTypeCompleted(object sender, GetSysRolePermByTypeCompletedEventArgs e)
        //{
        //    PagedCollectionView pcv = null;
        //    if (e.Result != null)
        //    {
        //        List<T_SYS_ROLE_PERM> menulist = e.Result.ToList();
        //        var q = from ent in menulist
        //                select ent;

        //        pcv = new PagedCollectionView(q);
        //        pcv.PageSize = 25;
        //    }

        //    dataPager.DataContext = pcv;
        //    DtGrid.ItemsSource = pcv;

        //    HidePageStyle();
        //}

        //private void InitControlEvent()
        //{
        //    gridToolBar.BtnAdd.Click += new RoutedEventHandler(BtnAdd_Click);
        //    gridToolBar.BtnAlter.Click += new RoutedEventHandler(BtnAlter_Click);
        //    gridToolBar.BtnDelete.Click += new RoutedEventHandler(BtnDelete_Click);
        //    DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

        //    ServiceClient = new PermissionServiceClient();
        //    ServiceClient.GetSysRolePermByTypeCompleted += new EventHandler<GetSysRolePermByTypeCompletedEventArgs>(ServiceClient_GetSysRolePermByTypeCompleted);
        //    ServiceClient.SysRolePermDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_SysRolePermDeleteCompleted);
        //}
        //#endregion

        #region 添加，修改，删除及查询
        //private T_SYS_ROLE_PERM rolePerm;

        //public T_SYS_ROLE_PERM RolePerm
        //{
        //    get { return rolePerm; }
        //    set { rolePerm = value; }
        //}

        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            //DataGrid grid = (DataGrid)sender;
            //if (grid.SelectedItem != null)
            //{
            //    RolePerm = (T_SYS_ROLE_PERM)grid.SelectedItems[0]; //获取当前选中的行数据并转换为对应的实体  
            //}
        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //ServiceClient.SysRolePermDeleteAsync(RolePerm.ID);
        }

        void ServiceClient_SysRolePermDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Error.Message, Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ShowPageStyle();

                //刷新数据
                //LoadData();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //SetRowLogo(DtGrid, e.Row, "T_SYS_ROLE_PERM");
        }
    }
}
