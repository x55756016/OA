/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于合同模板的列表展示，将已保存的合同模板数据用DataGrid展示在界面上

*********************************************************************************/
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
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Data;
using System.Windows.Browser;
using System.Globalization;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractTemplatePage : BasePage
    {

        #region 全局变量
        private SmtOADocumentAdminClient ContractManagement;
        private T_OA_CONTRACTTEMPLATE tac;
        private V_ContractTemplate tacInfo;
        private ObservableCollection<string> templateID = new ObservableCollection<string>();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件

        public T_OA_CONTRACTTEMPLATE Tac
        {
            get { return tac; }
            set { tac = value; this.DataContext = value; }
        }
        #endregion

        #region 构造函数
        public ContractTemplatePage()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                PARENT.Children.Add(loadbar);//在父面板中加载loading控件
                GetEntityLogo("T_OA_CONTRACTTEMPLATE");
                Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_CONTRACTTEMPLATE", true);
                this.cbContractLevel.SelectedIndex = -1;
                InitEvent();
                FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
                FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
                FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;
                FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;
                FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;
                FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;
                FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;
                // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
                FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                FormToolBar1.retAudit.Visibility = Visibility.Collapsed;

                #endregion
            };
        }
        #endregion
       
        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_ContractTemplate ent = DaGr.SelectedItems[0] as V_ContractTemplate;

            ContractTemplates AddWin = new ContractTemplates(FormTypes.Browse, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 550;
            browser.MinHeight = 410;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 刷新
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion
       
        #region 新增模板
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            tacInfo = new V_ContractTemplate();
            ContractTemplates AddWin = new ContractTemplates(FormTypes.New, tacInfo);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 550;
            browser.MinHeight = 430;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region 修改模板

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            V_ContractTemplate ent = DaGr.SelectedItems[0] as V_ContractTemplate;

            ContractTemplates AddWin = new ContractTemplates(FormTypes.Edit, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 550;
            browser.MinHeight = 430;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 删除模板

        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            templateID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    templateID.Add((DaGr.SelectedItems[i] as V_ContractTemplate).contractTemplate.CONTRACTTEMPLATEID);
                }
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ContractManagement.DeleteContraTemplateAsync(templateID);
                    LoadData();
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }          
        }
        #endregion

        #region 页面导航
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion
       
        #region 查询、分页LoadData
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值
            T_SYS_DICTIONARY StrContractLevel = cbContractLevel.SelectedItem as T_SYS_DICTIONARY;//合同级别

            if (!string.IsNullOrEmpty(txtTemplateID.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractTemplate.CONTRACTTITLE ^@" + paras.Count().ToString();//模板标题
                paras.Add(txtTemplateID.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtTemplateName.Text.Trim()))//模板名称
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractTemplate.CONTRACTTEMPLATENAME ^@" + paras.Count().ToString();
                paras.Add(txtTemplateName.Text.Trim());
            }
            if (this.cbContractLevel.SelectedIndex > 0) //级别 
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractTemplate.CONTRACTLEVEL ^@" + paras.Count().ToString();
                paras.Add(StrContractLevel.DICTIONARYVALUE.ToString());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            ContractManagement.GetContractTemplateInfoAsync(dpGrid.PageIndex, dpGrid.PageSize, "contractTemplate.CREATEDATE", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region InitEvent 初始化
        private void InitEvent()
        {
            ContractManagement = new SmtOADocumentAdminClient();
            ContractManagement.GetContractTemplateInfoCompleted += new EventHandler<GetContractTemplateInfoCompletedEventArgs>(TemplateRoomClient_GetContractTemplateInfoCompleted);
            ContractManagement.DeleteContraTemplateCompleted += new EventHandler<DeleteContraTemplateCompletedEventArgs>(ContractManagement_DeleteContraTemplateCompleted);
            LoadData();
        }

        void ContractManagement_DeleteContraTemplateCompleted(object sender, DeleteContraTemplateCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }

        void TemplateRoomClient_GetContractTemplateInfoCompleted(object sender, GetContractTemplateInfoCompletedEventArgs e)
        {
            loadbar.Stop();
            try
            {
                if (e.Result != null)
                {
                    if (e.Result.Count == 0)
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SORRYRECORDSCOULDNOTBEFOUND", ""));
                    }
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SORRYRECORDSCOULDNOTBEFOUND", ""));
                }
            }
            catch (Exception ex)
            {
                HtmlPage.Window.Alert(ex.ToString());
            }
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_ContractTemplate> obj, int pageCount)
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

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTTEMPLATE");
        }
        #endregion

        #region 查询按钮事件

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        //#region 查看详细

        //private void myBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    Button DetailBtn = sender as Button;
        //    T_OA_CONTRACTTEMPLATE Templateinfo = DetailBtn.Tag as T_OA_CONTRACTTEMPLATE;
        //    ContractTemplateDetailInfos AddWin = new ContractTemplateDetailInfos(Templateinfo);
        //    EntityBrowser browser = new EntityBrowser(AddWin);
        //    browser.Show();
        //}
        //#endregion

        #region 显示类型名称
        public class ConverterContractTypeName : IValueConverter
        {
            private SmtOADocumentAdminClient ContractTypeClient = new SmtOADocumentAdminClient();
            // value 作为类型表的 ID
            private string tmpTypeName = "";
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                this.GetContractTypeNameByContractTypeID(value.ToString());

                return tmpTypeName;
            }

            private void GetContractTypeNameByContractTypeID(string StrContractTypeID)
            {
                ContractTypeClient.GetContractTypeByIdCompleted += new EventHandler<GetContractTypeByIdCompletedEventArgs>(ContractTypeClient_GetContractTypeByIdCompleted);
                ContractTypeClient.GetContractTypeByIdAsync(StrContractTypeID);
            }

            void ContractTypeClient_GetContractTypeByIdCompleted(object sender, GetContractTypeByIdCompletedEventArgs e)
            {
                if (!e.Cancelled)
                {
                    if (e.Result != null)
                    {
                        T_OA_CONTRACTTYPE TypeObj = e.Result as T_OA_CONTRACTTYPE;
                        tmpTypeName = TypeObj.CONTRACTTYPE;
                    }
                }
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                string StrReturn = "";

                return StrReturn;
            }
        }
        #endregion

        #region 分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtTemplateID.Text = string.Empty;
            txtTemplateName.Text = string.Empty;
            cbContractLevel.SelectedIndex = 0;
        }
        #endregion
    }
}
