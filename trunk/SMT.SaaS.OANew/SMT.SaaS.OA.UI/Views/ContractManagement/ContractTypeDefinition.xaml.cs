/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于合同类型的数据列表展示，将已保存的合同类型数据用DataGrid展示在界面上

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ContractTypeDefinition : BasePage
    {
        private SmtOADocumentAdminClient ContractManagements;
        private T_OA_CONTRACTTYPE contractType;
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        #region 构造
        public ContractTypeDefinition()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                PARENT.Children.Add(loadbar);//在父面板中加载loading控件
                GetEntityLogo("T_OA_CONTRACTTYPE");
                Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_CONTRACTTYPE", true);
                this.cbContractLevel.SelectedIndex = -1;
                InitEvent();
                FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
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
        
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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

            T_OA_CONTRACTTYPE ent = DaGr.SelectedItems[0] as T_OA_CONTRACTTYPE;

            ContractTypeDefinitionPages AddWin = new ContractTypeDefinitionPages(FormTypes.Browse, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 400;
            browser.MinHeight = 280;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 修改
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

            T_OA_CONTRACTTYPE ent = DaGr.SelectedItems[0] as T_OA_CONTRACTTYPE;

            ContractTypeDefinitionPages AddWin = new ContractTypeDefinitionPages(FormTypes.Edit, ent);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 400;
            browser.MinHeight = 280;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 新增
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            contractType = new T_OA_CONTRACTTYPE();
            ContractTypeDefinitionPages AddWin = new ContractTypeDefinitionPages(FormTypes.New, contractType);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 400;//设置窗体宽度
            browser.MinHeight = 280;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }
        #endregion

        #region 删除按钮事件
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<string> contractTypeID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    contractTypeID.Add((DaGr.SelectedItems[i] as T_OA_CONTRACTTYPE).CONTRACTTYPEID);
                }
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ContractManagements.DeleteContraTypeAsync(contractTypeID);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        #endregion

        #region 查询、分页LoadData
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            T_SYS_DICTIONARY StrContractLevel = cbContractLevel.SelectedItem as T_SYS_DICTIONARY;//合同级别
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(txtContractTypeRoom.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTRACTTYPE ^@" + paras.Count().ToString();//类型名称
                paras.Add(txtContractTypeRoom.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtContractTypeMemo.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTENT ^@" + paras.Count().ToString();
                paras.Add(txtContractTypeMemo.Text.Trim());
            }
            if (this.cbContractLevel.SelectedIndex > 0) //级别
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "CONTRACTLEVEL ^@" + paras.Count().ToString();
                paras.Add(StrContractLevel.DICTIONARYVALUE.ToString());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            ContractManagements.GetContractTypeInfoAsync(dpGrid.PageIndex, dpGrid.PageSize, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region InitEvent
        private void InitEvent()
        {
            ContractManagements = new SmtOADocumentAdminClient();
            ContractManagements.GetContractTypeInfoCompleted += new EventHandler<GetContractTypeInfoCompletedEventArgs>(cmsfc_GetContractTypeInfoCompleted);
            ContractManagements.DeleteContraTypeCompleted += new EventHandler<DeleteContraTypeCompletedEventArgs>(ContractManagements_DeleteContraTypeCompleted);
            LoadData();
        }

        void ContractManagements_DeleteContraTypeCompleted(object sender, DeleteContraTypeCompletedEventArgs e)
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

        void cmsfc_GetContractTypeInfoCompleted(object sender, GetContractTypeInfoCompletedEventArgs e)
        {
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<T_OA_CONTRACTTYPE> obj, int pageCount)
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

        #region OnNavigatedTo
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region DataGrid LoadingRow事件

        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTTYPE");
        }
        #endregion

        #region 查询按钮事件

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
       
        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtContractTypeMemo.Text = string.Empty;
            txtContractTypeRoom.Text = string.Empty;
            cbContractLevel.SelectedIndex = 0;
        }
        #endregion
    }
}
