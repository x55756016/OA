/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-01-22

** 修改人：刘锦

** 修改时间：2010-07-12

** 描述：

**    主要用于合同申请数据信息的展示

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
    public partial class ApplicationsForContracts : BasePage
    {

        #region 全局变量
        private SmtOADocumentAdminClient caswsc;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private ObservableCollection<string> ApplicationsID = new ObservableCollection<string>();
        private T_OA_CONTRACTAPP infosT = new T_OA_CONTRACTAPP();
        private V_ContractApplications ApprovalInfo;
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public ApplicationsForContracts()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                PARENT.Children.Add(loadbar);//在父面板中加载loading控件
                GetEntityLogo("T_OA_CONTRACTAPP");
                Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_CONTRACTAPP", true);
                InitEvent();
                //SetButtonVisible();
                FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
                FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
                FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);//删除
                FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
                FormToolBar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
                FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
                FormToolBar1.btnPrint.Click += new RoutedEventHandler(btnPrint_Click);//打印
                FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
                Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
                FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
                //隐藏未使用按钮
                FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
                FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
                FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
                // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
                this.cbContractLevel.SelectedIndex = -1;
                #endregion
            };
        }
        #endregion

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_ContractApplications ent = DaGr.SelectedItems[0] as V_ContractApplications;

            ApplicationsForContractsPages AddWin = new ApplicationsForContractsPages(FormTypes.Resubmit, ent.contractApp.CONTRACTAPPID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 610;
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 打印
        void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            ApprovalInfo = new V_ContractApplications();
            ContractPrintUploadControl AddWin = new ContractPrintUploadControl(Action.Print, null);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
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

            V_ContractApplications ent = DaGr.SelectedItems[0] as V_ContractApplications;

            ApplicationsForContractsPages AddWin = new ApplicationsForContractsPages(FormTypes.Browse, ent.contractApp.CONTRACTAPPID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 610;
            browser.MinHeight = 480;
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

        #region 查询、分页LoadData()
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值
            T_SYS_DICTIONARY StrContractLevel = cbContractLevel.SelectedItem as T_SYS_DICTIONARY;//合同级别

            if (!string.IsNullOrEmpty(txtSearchID.Text.Trim())) //合同编号
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractApp.CONTRACTCODE ^@" + paras.Count().ToString();
                paras.Add(txtSearchID.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtSearchType.Text.Trim()))//标题
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractApp.CONTRACTTITLE ^@" + paras.Count().ToString();
                paras.Add(txtSearchType.Text.Trim());
            }
            if (this.cbContractLevel.SelectedIndex > 0) //级别
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractApp.CONTRACTLEVEL ^@" + paras.Count().ToString();
                paras.Add(StrContractLevel.DICTIONARYVALUE.ToString());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            caswsc.GetApprovalListByUserIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "contractApp.CHECKSTATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        #endregion

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                checkState = Utility.GetCbxSelectItemValue(FormToolBar1.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_CONTRACTAPP");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
            //SetButtonVisible();
        }
        #endregion

        #region 审批事件
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_ContractApplications ent = DaGr.SelectedItems[0] as V_ContractApplications;
            if (ent.contractApp.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.contractApp.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.contractApp.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {

                ApplicationsForContractsPages AddWin = new ApplicationsForContractsPages(FormTypes.Audit, ent.contractApp.CONTRACTAPPID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 610;
                browser.MinHeight = 480;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                return;
            }
        }
        #endregion

        #region 删除按钮事件
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ApplicationsID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_ContractApplications ent = DaGr.SelectedItems[i] as V_ContractApplications;
                    if (ent.contractApp.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        ApplicationsID.Add((DaGr.SelectedItems[i] as V_ContractApplications).contractApp.CONTRACTAPPID);

                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            caswsc.DeleteContraApprovalAsync(ApplicationsID);
                            LoadData();
                        };
                        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"));
                        return;
                    }
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        void caswsc_DeleteContraApprovalCompleted(object sender, DeleteContraApprovalCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    if (!e.Result)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                        return;
                    }
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion

        #region 修改按钮事件
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveTypeSetID = string.Empty;
            if (DaGr.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"));
                return;
            }

            V_ContractApplications ent = DaGr.SelectedItems[0] as V_ContractApplications;
            if (ent.contractApp.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.contractApp.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                ApplicationsForContractsPages AddWin = new ApplicationsForContractsPages(FormTypes.Edit, ent.contractApp.CONTRACTAPPID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 610;
                browser.MinHeight = 480;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                return;
            }
        }
        #endregion

        #region 添加按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ApplicationsForContractsPages AddWin = new ApplicationsForContractsPages(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 610;
            browser.MinHeight = 480;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region OnNavigatedTo
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region browser_ReloadDataEvent
        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTAPP");
        }
        #endregion

        #region 初始化

        private void InitEvent()
        {
            caswsc = new SmtOADocumentAdminClient();
            caswsc.GetApprovalListByUserIdCompleted += new EventHandler<GetApprovalListByUserIdCompletedEventArgs>(caswsc_GetApprovalListByUserIdCompleted);//根据ID查
            caswsc.DeleteContraApprovalCompleted += new EventHandler<DeleteContraApprovalCompletedEventArgs>(caswsc_DeleteContraApprovalCompleted);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }

        private void caswsc_GetApprovalListByUserIdCompleted(object sender, GetApprovalListByUserIdCompletedEventArgs e)
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_ContractApplications> obj, int pageCount)
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

        #region SetButtonVisible
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //未提交
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                    break;
                case "5":  //所有
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                    break;
            }
        }
        #endregion

        #region 查询事件
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region GridPager_Click

        private void GridPager_Click(object sender, RoutedEventArgs e)
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

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //ctrFile.Load_fileData(StrContentID);
        }
    }
}
