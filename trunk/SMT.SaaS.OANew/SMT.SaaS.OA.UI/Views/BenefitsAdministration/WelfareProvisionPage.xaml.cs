/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-18

** 修改人：刘锦

** 修改时间：2010-06-29

** 描述：

**    主要用于福利发放数据信息展示

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

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class WelfareProvisionPage : BasePage
    {

        #region 全局变量
        private SmtOADocumentAdminClient BenefitsAdministration;
        private string checkState = ((int)CheckStates.ALL).ToString();
        private T_OA_WELFAREDISTRIBUTEMASTER WelfareProvisionInfo = new T_OA_WELFAREDISTRIBUTEMASTER();
        private ObservableCollection<string> WelfareProvisionID = new ObservableCollection<string>();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public WelfareProvisionPage()
        {
            InitializeComponent();
            #region 原来的
            /*
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEMASTER");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREDISTRIBUTEMASTER", true);
            InitEvent();
            */
            #endregion
            //SetButtonVisible();
            this.Loaded += new RoutedEventHandler(WelfareProvisionPage_Loaded);
        }
        #endregion

        #region 加载ToolBar
        void WelfareProvisionPage_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREDISTRIBUTEMASTER");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREDISTRIBUTEMASTER", true);
            InitEvent();
            #endregion
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
            //   FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
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

            V_WelfareProvision ent = DaGr.SelectedItems[0] as V_WelfareProvision;

            WelfareProvisionChildWindows AddWin = new WelfareProvisionChildWindows(FormTypes.Resubmit, ent.welfareProvision.WELFAREDISTRIBUTEMASTERID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 590;
            browser.MinHeight = 490;
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
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_WELFAREDISTRIBUTEMASTER");
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

            V_WelfareProvision ent = DaGr.SelectedItems[0] as V_WelfareProvision;

            WelfareProvisionChildWindows AddWin = new WelfareProvisionChildWindows(FormTypes.Browse, ent.welfareProvision.WELFAREDISTRIBUTEMASTERID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 590;
            browser.MinHeight = 490;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 添加福利发放
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            WelfareProvisionChildWindows AddWin = new WelfareProvisionChildWindows(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 590;
            browser.MinHeight = 490;
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
            string StrStart = "";
            string StrEnd = "";
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
            StrStart = ReleaseTime.Text.ToString();
            StrEnd = ReleaseEndTime.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd);
                if (DtStart > DtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SEARCH"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                    return;
                }
                else
                {

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareProvision.DISTRIBUTEDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareProvision.DISTRIBUTEDATE <=@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                }
            }
            else
            {
                //开始时间不为空  结束时间为空   
                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareProvision.DISTRIBUTEDATE <=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                }
                //结束时间不为空
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareProvision.DISTRIBUTEDATE >=@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                }
            }

            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            BenefitsAdministration.GetWelfareProvisionListByUserIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "welfareProvision.CHECKSTATE", filter, paras, pageCount, checkState, loginUserInfo);
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

            V_WelfareProvision ent = DaGr.SelectedItems[0] as V_WelfareProvision;
            if (ent.welfareProvision.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.welfareProvision.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                WelfareProvisionChildWindows AddWin = new WelfareProvisionChildWindows(FormTypes.Edit, ent.welfareProvision.WELFAREDISTRIBUTEMASTERID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 590;
                browser.MinHeight = 490;
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
                    V_WelfareProvision ent = DaGr.SelectedItems[i] as V_WelfareProvision;
                    if (ent.welfareProvision.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        WelfareProvisionID.Add((DaGr.SelectedItems[i] as V_WelfareProvision).welfareProvision.WELFAREDISTRIBUTEMASTERID);

                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            BenefitsAdministration.DeleteWelfareProvisionAsync(WelfareProvisionID);
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

            V_WelfareProvision ent = DaGr.SelectedItems[0] as V_WelfareProvision;
            if (ent.welfareProvision.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.welfareProvision.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.welfareProvision.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {

                WelfareProvisionChildWindows AddWin = new WelfareProvisionChildWindows(FormTypes.Audit, ent.welfareProvision.WELFAREDISTRIBUTEMASTERID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 590;
                browser.MinHeight = 490;
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

        #region  绑定DataGird
        private void BindDataGrid(List<V_WelfareProvision> obj, int pageCount)
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

        #region 获取CheckBox
        private ObservableCollection<V_WelfareProvision> GetSelectEmpSurveys()
        {
            if (DaGr.ItemsSource != null)
            {
                ObservableCollection<V_WelfareProvision> selectedObj = new ObservableCollection<V_WelfareProvision>();
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox;
                        if (ckbSelect.IsChecked == true)
                        {
                            selectedObj.Add((V_WelfareProvision)obj);
                        }
                    }
                }
                if (selectedObj.Count > 0)
                {
                    return selectedObj;
                }
            }
            return null;
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            BenefitsAdministration = new SmtOADocumentAdminClient();
            BenefitsAdministration.GetWelfareProvisionListByUserIdCompleted += new EventHandler<GetWelfareProvisionListByUserIdCompletedEventArgs>(wpsc_GetWelfareProvisionListByUserIdCompleted);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //LoadData();
        }
        private void wpsc_GetWelfareProvisionListByUserIdCompleted(object sender, GetWelfareProvisionListByUserIdCompletedEventArgs e)
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

        #region 查看申请信息详细
        private void btnBro_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_WELFAREDISTRIBUTEMASTER");
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
            txtProvisionName.Text = string.Empty;
            ReleaseTime.Text = string.Empty;
            ReleaseEndTime.Text = string.Empty;
        }
        #endregion
    }
}
