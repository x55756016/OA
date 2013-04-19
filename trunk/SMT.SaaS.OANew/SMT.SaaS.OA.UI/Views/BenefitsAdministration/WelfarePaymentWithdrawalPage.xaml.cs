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
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class WelfarePaymentWithdrawalPage : BasePage
    {

        #region 全局变量
        private V_WelfareProvision WelfareProvision = new V_WelfareProvision();
        private SmtOADocumentAdminClient BenefitsAdministration;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private ObservableCollection<string> WelfareProvisionID = new ObservableCollection<string>();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public WelfarePaymentWithdrawalPage()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEUNDO");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREDISTRIBUTEUNDO", true);
            InitEvent();
            this.Loaded += new RoutedEventHandler(WelfareProvisionPage_Loaded);
        }
        #endregion

        #region 加载ToolBar
        void WelfareProvisionPage_Loaded(object sender, RoutedEventArgs e)
        {
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);//删除
            FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);//提交审核
            FormToolBar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //隐藏未使用按钮
            // FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作
        }


        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
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

            V_WelfarePaymentWithdrawal ent = DaGr.SelectedItems[0] as V_WelfarePaymentWithdrawal;

            WelfarePaymentWithdrawalControl AddWin = new WelfarePaymentWithdrawalControl(FormTypes.Resubmit, ent.beingWithdrawn.WELFAREDISTRIBUTEUNDOID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 590;
            browser.MinHeight = 390;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                checkState = Utility.GetCbxSelectItemValue(FormToolBar1.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_WELFAREDISTRIBUTEUNDO");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
            //SetButtonVisible();
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

            V_WelfarePaymentWithdrawal ent = DaGr.SelectedItems[0] as V_WelfarePaymentWithdrawal;

            WelfarePaymentWithdrawalControl AddWin = new WelfarePaymentWithdrawalControl(FormTypes.Browse, ent.beingWithdrawn.WELFAREDISTRIBUTEUNDOID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 590;
            browser.MinHeight = 390;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 添加发放撤销
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            WelfarePaymentWithdrawalControl AddWin = new WelfarePaymentWithdrawalControl(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 590;
            browser.MinHeight = 390;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
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
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(txtProvisionName.Text.Trim()))//福利发放名
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "welfareProvision.WELFAREDISTRIBUTETITLE ^@" + paras.Count().ToString();
                paras.Add(txtProvisionName.Text.Trim());
            }
            if (!string.IsNullOrEmpty(txtProvisionContent.Text.Trim()))//发放内容
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "welfareProvision.CONTENT ^@" + paras.Count().ToString();
                paras.Add(txtProvisionContent.Text.Trim());
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            BenefitsAdministration.GetWelfarePaymentWithdrawalAsync(dpGrid.PageIndex, dpGrid.PageSize, "beingWithdrawn.CHECKSTATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        #endregion

        #region 修改福利发放
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

            V_WelfarePaymentWithdrawal ent = DaGr.SelectedItems[0] as V_WelfarePaymentWithdrawal;
            if (ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                WelfarePaymentWithdrawalControl AddWin = new WelfarePaymentWithdrawalControl(FormTypes.Edit, ent.beingWithdrawn.WELFAREDISTRIBUTEUNDOID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 590;
                browser.MinHeight = 390;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                return;
            }
        }
        #endregion

        #region 删除福利发放
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WelfareProvisionID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_WelfarePaymentWithdrawal ent = DaGr.SelectedItems[i] as V_WelfarePaymentWithdrawal;
                    if (ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        WelfareProvisionID.Add((DaGr.SelectedItems[i] as V_WelfarePaymentWithdrawal).beingWithdrawn.WELFAREDISTRIBUTEUNDOID);

                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            BenefitsAdministration.DeletePaymentWithdrawalAsync(WelfareProvisionID);
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

        void BenefitsAdministration_DeletePaymentWithdrawalCompleted(object sender, DeletePaymentWithdrawalCompletedEventArgs e)
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

        #region 提交审核
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

            V_WelfarePaymentWithdrawal ent = DaGr.SelectedItems[0] as V_WelfarePaymentWithdrawal;
            if (ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.beingWithdrawn.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                WelfarePaymentWithdrawalControl AddWin = new WelfarePaymentWithdrawalControl(FormTypes.Audit, ent.beingWithdrawn.WELFAREDISTRIBUTEUNDOID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 590;
                browser.MinHeight = 390;
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

        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region AddWin_ReloadDataEvent
        void AddWin_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region  绑定DataGird
        private void BindDataGrid(List<V_WelfarePaymentWithdrawal> obj, int pageCount)
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

        #region 初始化
        private void InitEvent()
        {
            BenefitsAdministration = new SmtOADocumentAdminClient();
            BenefitsAdministration.GetWelfarePaymentWithdrawalCompleted += new EventHandler<GetWelfarePaymentWithdrawalCompletedEventArgs>(BenefitsAdministration_GetWelfarePaymentWithdrawalCompleted);
            BenefitsAdministration.DeletePaymentWithdrawalCompleted += new EventHandler<DeletePaymentWithdrawalCompletedEventArgs>(BenefitsAdministration_DeletePaymentWithdrawalCompleted);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //LoadData();
        }

        void BenefitsAdministration_GetWelfarePaymentWithdrawalCompleted(object sender, GetWelfarePaymentWithdrawalCompletedEventArgs e)
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
            loadbar.Stop();//读取完数据后，停止动画，隐藏
        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_WELFAREDISTRIBUTEUNDO");
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

        #region 查询
        private void Search_Click(object sender, RoutedEventArgs e)
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
            txtProvisionContent.Text = string.Empty;
            txtProvisionName.Text = string.Empty;
        }
        #endregion
    }
}
