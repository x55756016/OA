using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.OrganManagement
{
    public partial class FrmLicenseManagement : BasePage,IClient
    {
        private SmtOADocumentAdminClient client;
        private ObservableCollection<string> licenseID = new ObservableCollection<string>();
        private SMTLoading loadbar = new SMTLoading();
        private T_OA_LICENSEMASTER licensemaster;

        public T_OA_LICENSEMASTER Licensemaster
        {
            get { return licensemaster; }
            set { licensemaster = value; }
        }
        #region 初始化
        public FrmLicenseManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "OAORGANLICEN", true);
            PARENT.Children.Add(loadbar);
            this.Loaded += new RoutedEventHandler(FrmLicenseManagement_Loaded);
            
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //
            //ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
            //LoadData();
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.GetLicenseListPagingCompleted += new EventHandler<GetLicenseListPagingCompletedEventArgs>(client_GetLicenseListPagingCompleted);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            //ToolBar.btnRead.Visibility = Visibility.Collapsed;
            
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.retNew.Visibility = Visibility.Collapsed;
            
            //this.dgLicense.CurrentCellChanged += new EventHandler<EventArgs>(dgLicense_CurrentCellChanged);
            dgLicense.SelectionChanged += new SelectionChangedEventHandler(dgLicense_SelectionChanged);
            ToolBar.ShowRect();
        }

        void dgLicense_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItems.Count == 0)
                return;
            if (grid.SelectedItems.Count  > 0 )
            {
                Licensemaster = (T_OA_LICENSEMASTER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (Licensemaster != null)
            {
                LicenseDetailForm form = new LicenseDetailForm(FormTypes.Browse,Licensemaster.LICENSEMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                //browser.AuditCtrl.Visibility = Visibility.Collapsed;
                browser.FormType = FormTypes.Browse;
                browser.HideLeftMenu();
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void dgLicense_CurrentCellChanged(object sender, EventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            if (grid.SelectedItem != null)
            {
                Licensemaster = (T_OA_LICENSEMASTER)grid.SelectedItems[0];//获取当前选中的行数据并转换为对应的实体     
            }
        }

        void FrmLicenseManagement_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            LoadData();
            GetEntityLogo("T_OA_LICENSEMASTER");
        }        
        
        #endregion

        #region 完成事件 

        private void client_GetLicenseListPagingCompleted(object sender, GetLicenseListPagingCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Error != null)
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        BindData(e.Result.ToList(),e.pageCount);
                    }
                    else
                    {
                        BindData(null,0);                        
                    }                    
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        #endregion

        #region 绑定网格

        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            //TextBox txtOrgCode = Utility.FindChildControl<TextBox>(controlsToolkitTUV, "txtOrgCode");
            //TextBox txtLicenseName = Utility.FindChildControl<TextBox>(controlsToolkitTUV, "txtLicenseName");
            if (!string.IsNullOrEmpty(txtOrgCode.Text.Trim()))
            {
                filter += "T_OA_ORGANIZATION.ORGCODE ==@" + paras.Count().ToString();
                paras.Add(txtOrgCode.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtLicenseName.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "LICENSENAME ==@" + paras.Count().ToString();
                paras.Add(txtLicenseName.Text);
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter += " and ";
            }
            filter += "ISVALID==@" + paras.Count().ToString();
            paras.Add("1");
            loadbar.Start();
            client.GetLicenseListPagingAsync(dataPager.PageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }

        private void BindData(List<T_OA_LICENSEMASTER> obj,int pageCount)
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
        //修改
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Licensemaster != null)
            {
                LicenseDetailForm form = new LicenseDetailForm(FormTypes.Edit,Licensemaster.LICENSEMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                //browser.AuditCtrl.Visibility = Visibility.Collapsed;
                browser.HideLeftMenu();
                browser.MinHeight = 550;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }    
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void browser_ReloadDataEvent()
        {
            LoadData();
        }

        private void Frm_ReloadDataEvent()
        {
            LoadData();
        }

        //模板列取消选中
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                licenseID.Remove(chkbox.Tag.ToString());
                GridHelper.SetUnCheckAll(dgLicense);
            }
        }

        //模板列选中
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.Value)
            {
                licenseID.Add(chkbox.Tag.ToString());
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        private void dgLicense_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgLicense, e.Row, "T_OA_LICENSEMASTER");
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
