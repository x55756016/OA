using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.Permission.UI.Views
{
    public partial class EntityMenuCustomPerm : BasePage
    {
        public EntityMenuCustomPerm()
        {
            InitializeComponent();
            //Utility.DisplayGridToolBarButton(FormToolBar1, "OWNERMENUPERMISSION", true);
            InitControlEvent();
            
            GetEntityLogo("T_SYS_ENTITYMENUCUSTOMPERM"); 
            this.Loaded += new RoutedEventHandler(EntityMenuCustomPerm_Loaded);
        }

        void EntityMenuCustomPerm_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
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
            FormToolBar1.ShowRect();
            LoadData();
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Perm == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                Form.EntityMenuCustomPermForms editForm = new SMT.SaaS.Permission.UI.Form.EntityMenuCustomPermForms(FormTypes.Browse, Perm.ENTITYMENUCUSTOMPERMID);
                EntityBrowser browser = new EntityBrowser(editForm);
                browser.MinHeight = 450;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }
            void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (Perm != null)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ServiceClient.EntityMenuCustomPermDeleteAsync(Perm.ENTITYMENUCUSTOMPERMID);
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
            if (Perm == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                Form.EntityMenuCustomPermForms editForm = new SMT.SaaS.Permission.UI.Form.EntityMenuCustomPermForms(FormTypes.Edit, Perm.ENTITYMENUCUSTOMPERMID);
                EntityBrowser browser = new EntityBrowser(editForm);
                browser.MinHeight = 450;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            Form.EntityMenuCustomPermForms editForm = new SMT.SaaS.Permission.UI.Form.EntityMenuCustomPermForms(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(editForm);
            browser.MinHeight = 450;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        protected PermissionServiceClient ServiceClient;

        //#region 数据绑定及初始化事件
        private void LoadData()
        {
            TextBox txtSearchSystemType = Utility.FindChildControl<TextBox>(expander, "txtSearchSystemType");
            ServiceClient.GetEntityMenuCustomPermByTypeAsync(txtSearchSystemType.Text.Trim());
        }
        

        private void InitControlEvent()
        {
            DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);

            ServiceClient = new PermissionServiceClient();
            
            ServiceClient.GetEntityMenuCustomPermByTypeCompleted += new EventHandler<GetEntityMenuCustomPermByTypeCompletedEventArgs>(ServiceClient_GetEntityMenuCustomPermByTypeCompleted);

            //ServiceClient.EntityMenuCustomPermDeleteCompleted += new EventHandler<EntityMenuCustomPermDeleteCompletedEventArgs>(ServiceClient_EntityMenuCustomPermDeleteCompleted);
            ServiceClient.EntityMenuCustomPermDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_EntityMenuCustomPermDeleteCompleted);
            //ServiceClient.EntityMenuCustomPermDeleteCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ServiceClient_EntityMenuCustomPermDeleteCompleted);
        }

        //void ServiceClient_EntityMenuCustomPermDeleteCompleted(object sender, EntityMenuCustomPermDeleteCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        ComfirmWindow.ConfirmationBox("错误信息", e.Error.Message.ToString(), Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBox("错误信息", Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //        ShowPageStyle();

        //        //刷新数据
        //        LoadData();
        //    }
        //}

        void ServiceClient_EntityMenuCustomPermDeleteCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBox("错误信息", e.Error.Message.ToString(), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                ComfirmWindow.ConfirmationBox("错误信息", Utility.GetResourceStr("DELETESUCCESSEDCONFIRM"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ShowPageStyle();

                //刷新数据
                LoadData();
            }
        }

        void ServiceClient_GetEntityMenuCustomPermByTypeCompleted(object sender, GetEntityMenuCustomPermByTypeCompletedEventArgs e)
        {
            PagedCollectionView pcv = null;
            if (e.Result != null)
            {
                List<T_SYS_ENTITYMENUCUSTOMPERM> menulist = e.Result.ToList();
                var q = from ent in menulist
                        select ent;

                pcv = new PagedCollectionView(q);
                pcv.PageSize = 25;
            }

            dataPager.DataContext = pcv;
            DtGrid.ItemsSource = pcv;

            HidePageStyle();
        }
        //#endregion

        #region 添加，修改，删除及查询
        private T_SYS_ENTITYMENUCUSTOMPERM perm;

        public T_SYS_ENTITYMENUCUSTOMPERM Perm
        {
            get { return perm; }
            set { perm = value; }
        }

        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            if (grid.SelectedItem != null)
            {
                Perm = (T_SYS_ENTITYMENUCUSTOMPERM)grid.SelectedItems[0]; //获取当前选中的行数据并转换为对应的实体  
            }
            //perm.ENDDATE
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_SYS_ENTITYMENUCUSTOMPERM");
        }

        private void dataPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
