using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;

namespace SMT.SaaS.OA.UI.Views.OrganManagement
{
    public partial class FrmLicenseReturnManagement : BasePage,IClient
    {
        private SmtOADocumentAdminClient client;
        private T_OA_LICENSEUSER licenseObj;
        private SMTLoading loadbar = new SMTLoading(); 

        //public V_LicenseBorrow LicenseObj
        //{
        //    get { return licenseObj; }
        //    set 
        //    {
        //        this.DataContext = value;
        //        licenseObj = value; 
        //    }
        //}
        private string lendFlag = "5";
        private T_OA_LICENSEUSER licenseuser;

        public T_OA_LICENSEUSER Licenseuser
        {
            get { return licenseuser; }
            set { licenseuser = value; }
        }

        #region 初始化
        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        public FrmLicenseReturnManagement()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                Utility.DisplayGridToolBarButton(ToolBar, "OAORGANLICENUSER", true);
                PARENT.Children.Add(loadbar);
                InitEvent();
            };
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetBorrowAppListCompleted += new EventHandler<GetBorrowAppListCompletedEventArgs>(client_GetBorrowAppListCompleted);
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.BtnView.Visibility = Visibility.Collapsed;
            
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            //ToolBar.btnRead.Visibility = Visibility.Collapsed;
            //ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            //ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            //ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406in.png", Utility.GetResourceStr("RETURN")).Click += new RoutedEventHandler(FrmLicenseReturnManagement_Click);
            ToolBar.btnImports("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406go.png", Utility.GetResourceStr("LEND")).Click += new RoutedEventHandler(FrmLicenseReturnManagement1_Click);
            Utility.CbxItemBinder(CBLendState, "LENDSTATE", "5");            
            LoadData();
            SetButtonVisible();
            this.Loaded += new RoutedEventHandler(FrmLicenseReturnManagement_Loaded);
            //this.dgLicense.CurrentCellChanged += new EventHandler<EventArgs>(dgLicense_CurrentCellChanged);
            dgLicense.SelectionChanged += new SelectionChangedEventHandler(dgLicense_SelectionChanged);
            ToolBar.ShowRect();
        }

        void dgLicense_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            if (grid.SelectedItems.Count > 0)
            {
                licenseuser = (T_OA_LICENSEUSER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void dgLicense_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                licenseuser = (T_OA_LICENSEUSER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmLicenseReturnManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_LICENSEUSER");
        }

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值            
            if (!string.IsNullOrEmpty(txtLicenseName.Text.Trim()))
            {
                filter += "T_OA_LICENSEMASTER.LICENSENAME ^@" + paras.Count().ToString();
                paras.Add(txtLicenseName.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtContent.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTENT ^@" + paras.Count().ToString();
                paras.Add(txtContent.Text.Trim());
            }
            //if (CBLendState.SelectedIndex > 0)
            //{
            //    if (CBLendState.SelectedIndex == 1)
            //    {
            //        lendFlag = "0";
            //    }
            //    if (CBLendState.SelectedIndex == 2)
            //    {
            //        lendFlag = "1";
            //    }
            //}
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loadbar.Start();
            client.GetBorrowAppListAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, lendFlag, loginUserInfo);
        }
        #endregion

        #region 完成事件
        private void client_GetBorrowAppListCompleted(object sender, GetBorrowAppListCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        BindData(e.Result.ToList(), e.pageCount);
                    }
                    else
                    {
                        BindData(null,0);                        
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 绑定DataGird
        private void BindData(List<T_OA_LICENSEUSER> obj,int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                //HtmlPage.Window.Alert("对不起！未能找到相关记录。");
                dgLicense.ItemsSource = null;
                return;
            }
            dgLicense.ItemsSource = obj;
        }
        #endregion        

        #region 按钮事件
        

        private void FrmLicenseReturnManagement_Click(object sender, RoutedEventArgs e)
        {

            if (Licenseuser == null)
            {
                //HtmlPage.Window.Alert("请先选择需要归还的记录！");

                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "RETURN"));

                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "RETURN"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {
                LicenseBorrowReturn orgFrm = new LicenseBorrowReturn(Action.Return, Licenseuser);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.MinWidth = 410;
                browser.MinHeight = 360;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }

        private void FrmLicenseReturnManagement1_Click(object sender, RoutedEventArgs e)
        {
            if (Licenseuser == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "LEND"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            else
            {  
                LicenseBorrowReturn orgFrm = new LicenseBorrowReturn(Action.Lend, Licenseuser);
                EntityBrowser browser = new EntityBrowser(orgFrm);
                browser.MinWidth = 410;
                browser.MinHeight = 360;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Licenseuser = null;
            LoadData();
        }

        private void browser_ReloadDataEvent()
        {
            Licenseuser = null;
            LoadData();
        }

        private void addForm_ReloadDataEvent()
        {
            Licenseuser = null;
            LoadData();
        }

       
        //归还
        //private void btnEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    if (licenseObj == null)
        //    {
        //        //HtmlPage.Window.Alert("请先选择需要归还的记录！");

        //        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "RETURN"));

        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "RETURN"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //    else
        //    {
        //        CFrmLicenseReturnAdd addForm = new CFrmLicenseReturnAdd(Action.Return, licenseObj);
        //        //addForm.Title = "归还证照";
        //        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("LICENSERETURN"));
        //        addForm.Title = Utility.GetResourceStr("LICENSERETURN");
        //        addForm.Show();
        //        addForm.ReloadDataEvent += new BaseForm.refreshGridView(addForm_ReloadDataEvent);
        //    }
        //}

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            GridHelper.SetUnCheckAll(dgLicense);
            LoadData();
        }

        private void SetButtonVisible()
        {
            switch (lendFlag)
            {
                case "0":  //草稿
                    //ToolBar.btnNew.Visibility = Visibility.Visible;                    
                    //ToolBar.btnEdit.Visibility = Visibility.Collapsed;
                    ToolBar.btnImports("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406go.png", Utility.GetResourceStr("LEND")).Visibility = Visibility.Visible;
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406in.png", Utility.GetResourceStr("RETURN")).Visibility = Visibility.Collapsed;
                    break;
                case "1":  //审批通过
                    ToolBar.btnImports("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406go.png", Utility.GetResourceStr("LEND")).Visibility = Visibility.Collapsed;
                    ToolBar.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4406in.png", Utility.GetResourceStr("RETURN")).Visibility = Visibility.Visible;
                    break;
            }
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            Licenseuser = null;
            LoadData();
        }

        //
        private void CBLendState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lendFlag = Utility.GetCbxSelectItemValue(CBLendState);
            SetButtonVisible();
            
        }
        #endregion

        private void dgLicense_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgLicense, e.Row, "T_OA_LICENSEUSER");
        }

        #region IClient 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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
