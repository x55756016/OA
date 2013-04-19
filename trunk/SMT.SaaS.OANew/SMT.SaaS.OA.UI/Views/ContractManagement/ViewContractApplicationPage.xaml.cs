/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-04-22

** 修改人：刘锦

** 修改时间：2010-07-22

** 描述：

**    主要用于合同查看申请的数据展示，获取已保存的查看申请数据展示在DataGrid列表控件上

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.Views.ContractManagement
{
    public partial class ViewContractApplicationPage : BasePage
    {

        #region 全局变量
        private SmtOADocumentAdminClient caswsc;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_CONTRACTVIEW infosT = new T_OA_CONTRACTVIEW();
        private ObservableCollection<string> contractviewID = new ObservableCollection<string>();
        private V_ContractPrint ContractPrintInfo = new V_ContractPrint();
        SMT.SaaS.FrameworkUI.AuditControl.AuditControl AuditContractApp = new SMT.SaaS.FrameworkUI.AuditControl.AuditControl();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public ViewContractApplicationPage()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                #region 原来的
                PARENT.Children.Add(loadbar);//在父面板中加载loading控件
                GetEntityLogo("T_OA_CONTRACTVIEW");
                Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_CONTRACTVIEW", true);
                InitEvent();
                FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
                FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
                FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
                FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);//审核
                FormToolBar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
                FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
                FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
                FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
                FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
                Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
                FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
                //隐藏未使用按钮
                FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
                FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
                FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
                //  FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
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

            V_ContractView ent = DaGr.SelectedItems[0] as V_ContractView;

            ViewContractApplicationControl AddWin = new ViewContractApplicationControl(FormTypes.Resubmit, ent.viewContract.CONTRACTVIEWID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 500;
            browser.MinHeight = 300;
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

            V_ContractView ent = DaGr.SelectedItems[0] as V_ContractView;
            if (ent.viewContract.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.viewContract.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                ViewContractApplicationControl AddWin = new ViewContractApplicationControl(FormTypes.Edit, ent.viewContract.CONTRACTVIEWID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 550;
                browser.MinHeight = 180;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        #region 删除按钮事件
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            contractviewID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_ContractView ent = DaGr.SelectedItems[i] as V_ContractView;
                    if (ent.viewContract.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        contractviewID.Add((DaGr.SelectedItems[i] as V_ContractView).viewContract.CONTRACTVIEWID);

                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            caswsc.DeleteViewapplicationsAsync(contractviewID);
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

        void caswsc_DeleteViewapplicationsCompleted(object sender, DeleteViewapplicationsCompletedEventArgs e)
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

        #region 新增查看申请
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            ViewContractApplicationControl AddWin = new ViewContractApplicationControl(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 550;
            browser.MinHeight = 180;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
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

            V_ContractView ent = DaGr.SelectedItems[0] as V_ContractView;

            ViewContractApplicationControl AddWin = new ViewContractApplicationControl(FormTypes.Browse, ent.viewContract.CONTRACTVIEWID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 500;
            browser.MinHeight = 300;
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

        #region 审批
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

            V_ContractView ent = DaGr.SelectedItems[0] as V_ContractView;
            if (ent.viewContract.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.viewContract.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.viewContract.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                ViewContractApplicationControl AddWin = new ViewContractApplicationControl(FormTypes.Audit, ent.viewContract.CONTRACTVIEWID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 550;
                browser.MinHeight = 180;
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
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
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
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "contractApp.contractApp.CONTRACTLEVEL ^@" + paras.Count().ToString();
                paras.Add(StrContractLevel.DICTIONARYVALUE.ToString());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;

            caswsc.GetInquiryViewContractApplicationAsync(dpGrid.PageIndex, dpGrid.PageSize, "viewContract.CHECKSTATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        #endregion

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                checkState = Utility.GetCbxSelectItemValue(FormToolBar1.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_CONTRACTVIEW");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
            SetButtonVisible();
        }
        #endregion

        #region 初始化

        private void InitEvent()
        {
            caswsc = new SmtOADocumentAdminClient();
            caswsc.GetInquiryViewContractApplicationCompleted += new EventHandler<GetInquiryViewContractApplicationCompletedEventArgs>(caswsc_GetInquiryViewContractApplicationCompleted);//查询合同查看申请
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            caswsc.DeleteViewapplicationsCompleted += new EventHandler<DeleteViewapplicationsCompletedEventArgs>(caswsc_DeleteViewapplicationsCompleted);
        }

        void caswsc_GetInquiryViewContractApplicationCompleted(object sender, GetInquiryViewContractApplicationCompletedEventArgs e)
        {
            loadbar.Stop();
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
        }
        #endregion

        #region SetButtonVisible
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿箱
                    FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;
                    FormToolBar1.retAudit.Visibility = Visibility.Collapsed;//审核分隔符
                    break;
                case "1":  //审批中 
                    FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
                    break;
                case "2":  //审批通过  审核人身份
                    FormToolBar1.BtnView.Visibility = Visibility.Visible;
                    break;
                case "3":  //审批未通过
                    FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
                    break;
                case "4":  //待审核
                    FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
                    break;
                case "5":  //所有
                    FormToolBar1.BtnView.Visibility = Visibility.Collapsed;
                    FormToolBar1.btnAudit.Visibility = Visibility.Visible;
                    FormToolBar1.retAudit.Visibility = Visibility.Visible;//审核分隔符
                    break;
            }
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_ContractView> obj, int pageCount)
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

        #region DataGrid LoadingRow
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_CONTRACTVIEW");
        }
        #endregion

        #region 动态分页
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
