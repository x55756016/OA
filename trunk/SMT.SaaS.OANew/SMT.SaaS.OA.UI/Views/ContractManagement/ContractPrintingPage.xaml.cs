/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-22

** 修改人：刘锦

** 修改时间：2010-07-02

** 描述：

**    主要用于合同打印，将已打印的合同申请数据用DataGrid展示在界面上

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.Saas.Tools.PermissionWS;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractPrintingPage : BasePage
    {
        private SmtOADocumentAdminClient caswsc;
        private V_ContractPrint printInfo = new V_ContractPrint();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件

        #region 构造
        
        public ContractPrintingPage()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                PARENT.Children.Add(loadbar);//在父面板中加载loading控件
                GetEntityLogo("T_OA_CONTRACTPRINT");
                Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_CONTRACTPRINT", true);
                this.cbContractLevel.SelectedIndex = -1;
                InitEvent();
                FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
                FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
                FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//新增打印

                //隐藏不需要显示的按钮及分隔符
                FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;//修改
                FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;//删除
                FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
                FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;//检查审核状态
                FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
                FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
                FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
                // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
                FormToolBar1.retEdit.Visibility = Visibility.Collapsed;//修改分隔符
                FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                FormToolBar1.retRead.Visibility = Visibility.Collapsed;//删除分隔符
                #endregion
            };
        }
        #endregion

        #region 添加打印
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ContractPrintUploadControl AddWin = new ContractPrintUploadControl(Action.Print, null);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 580;
            browser.MinHeight = 460;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion
       
        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            V_ContractPrint contractApplications = DaGr.SelectedItem as V_ContractPrint;
            ContractPrintUploadControl AddWin = new ContractPrintUploadControl(Action.Read, contractApplications);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 580;
            browser.MinHeight = 460;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region btnRefresh_Click
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            caswsc = new SmtOADocumentAdminClient();
            caswsc.GetInquiryContractPrintingRecordCompleted += new EventHandler<GetInquiryContractPrintingRecordCompletedEventArgs>(caswsc_GetInquiryContractPrintingRecordCompleted);
            LoadData();
        }

        void caswsc_GetInquiryContractPrintingRecordCompleted(object sender, GetInquiryContractPrintingRecordCompletedEventArgs e)
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion
       
        #region 查询、分页LoadData()
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = ""; //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值
            T_SYS_DICTIONARY StrContractLevel = cbContractLevel.SelectedItem as T_SYS_DICTIONARY;//合同级别

            if (!string.IsNullOrEmpty(txtSearchID.Text.Trim())) //合同编号
            {
                filter += "contractApp.contractApp.CONTRACTCODE ^@" + paras.Count().ToString();
                paras.Add(txtSearchID.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))//标题
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractApp.contractApp.CONTRACTTITLE ^@" + paras.Count().ToString();
                paras.Add(txtSearchType.Text.Trim());
            }
            if (this.cbContractLevel.SelectedIndex > 0) //级别
            {
                filter += "contractApp.contractApp.CONTRACTLEVEL ^@" + paras.Count().ToString();
                paras.Add(StrContractLevel.DICTIONARYVALUE.ToString());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            caswsc.GetInquiryContractPrintingRecordAsync(dpGrid.PageIndex, dpGrid.PageSize, "contractApp.contractApp.CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_ContractPrint> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dpGrid, pageCount);
            if (obj == null || obj.Count < 1)
            {
                DaGr.ItemsSource = null;
                return;
            }
            DaGr.ItemsSource = obj;
        }
        #endregion

        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion
      
        #region DaGr_LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_ContractPrint tmp = (V_ContractPrint)e.Row.DataContext;
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTPRINT");
            ImageButton MyButton_Addbaodao = DaGr.Columns[11].GetCellContent(e.Row).FindName("myBtn") as ImageButton;
            MyButton_Addbaodao.Margin = new Thickness(0);
            MyButton_Addbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/Tool/16_convertactivity.png", Utility.GetResourceStr("UPLOADACCESSORY"));
            MyButton_Addbaodao.Tag = tmp;
        }
        #endregion

        #region GridPager_Click
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 选择上传记录
        private void myBtn_Click(object sender, RoutedEventArgs e)
        {
            V_ContractPrint contractApplications = DaGr.SelectedItem as V_ContractPrint;
            ContractPrintUploadControl AddWin = new ContractPrintUploadControl(Action.FromAnnex, contractApplications);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 580;
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region browser_ReloadDataEvent

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtSearchID.Text = string.Empty;
            txtSearchType.Text = string.Empty;
            cbContractLevel.SelectedIndex = 0;
        }
        #endregion
    }
}
