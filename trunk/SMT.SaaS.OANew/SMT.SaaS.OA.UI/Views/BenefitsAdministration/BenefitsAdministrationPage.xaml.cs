/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-20

** 修改人：刘锦

** 修改时间：2010-06-28

** 描述：

**    主要用于福利标准定义数据的展示

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Collections.ObjectModel;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.SaaS.OA.UI.Views.BenefitsAdministration
{
    public partial class BenefitsAdministrationPage : BasePage
    {

        #region 全局变量
        public SmtOADocumentAdminClient wssc = new SmtOADocumentAdminClient();
        private string checkState = ((int)CheckStates.ALL).ToString();
        private ObservableCollection<string> WelfareStandardID = new ObservableCollection<string>();
        private T_OA_WELFAREMASERT welfareT = new T_OA_WELFAREMASERT();
        private V_WelfareStandard v_welfareT = new V_WelfareStandard();
        FormToolBar ToolBar = new FormToolBar();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        #endregion

        #region 构造函数
        public BenefitsAdministrationPage()
        {
            InitializeComponent();
            #region 原来的
            /*
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREMASERT");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREMASERT", true);
            InitEvent();
            //SetButtonVisible();
            this.cbWelfareID.SelectedIndex = 0;
            */
            #endregion
            this.Loaded += new RoutedEventHandler(BenefitsAdministrationPage_Loaded);
        }
        #endregion

        #region 加载ToolBar
        void BenefitsAdministrationPage_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_WELFAREMASERT");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_WELFAREMASERT", true);
            InitEvent();
            //SetButtonVisible();
            this.cbWelfareID.SelectedIndex = -1;
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
            //FormToolBar1.btnRead.Visibility = Visibility.Collapsed;//读取
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            //  FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
            FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
            FormToolBar1.stpOtherAction.Visibility = Visibility.Collapsed;//其他动作

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

            V_WelfareStandard ent = DaGr.SelectedItems[0] as V_WelfareStandard;

            BenefitsAdministrationChildWindows AddWin = new BenefitsAdministrationChildWindows(FormTypes.Resubmit, ent.welfareStandard.WELFAREID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 650;
            browser.MinHeight = 550;
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

            V_WelfareStandard ent = DaGr.SelectedItems[0] as V_WelfareStandard;

            BenefitsAdministrationChildWindows AddWin = new BenefitsAdministrationChildWindows(FormTypes.Browse, ent.welfareStandard.WELFAREID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 650;
            browser.MinHeight = 550;
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
            string StrStart = "";
            string StrEnd = "";
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();//参数值
            T_SYS_DICTIONARY StrWelfareId = cbWelfareID.SelectedItem as T_SYS_DICTIONARY;//从字典里查询福利名称

            if (this.cbWelfareID.SelectedIndex >= 0) //福利名称
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "welfareStandard.WELFAREPROID ^@" + paras.Count().ToString();
                paras.Add(StrWelfareId.DICTIONARYVALUE.ToString());
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
                    filter += "welfareStandard.STARTDATE >=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareStandard.STARTDATE <=@" + paras.Count().ToString();
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
                    filter += "welfareStandard.STARTDATE <=@" + paras.Count().ToString();
                    paras.Add(DtStart);
                }
                //结束时间不为空
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "welfareStandard.STARTDATE >=@" + paras.Count().ToString();
                    paras.Add(DtEnd);
                }
            }
            SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOADocumentAdminService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            wssc.GetWelfareListByUserIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "welfareStandard.CHECKSTATE", filter, paras, pageCount, checkState, loginUserInfo);
        }
        #endregion

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                checkState = Utility.GetCbxSelectItemValue(FormToolBar1.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_WELFAREMASERT");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
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

            V_WelfareStandard ent = DaGr.SelectedItems[0] as V_WelfareStandard;
            if (ent.welfareStandard.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                ent.welfareStandard.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                ent.welfareStandard.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {

                BenefitsAdministrationChildWindows AddWin = new BenefitsAdministrationChildWindows(FormTypes.Audit, ent.welfareStandard.WELFAREID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Audit;
                browser.MinWidth = 650;
                browser.MinHeight = 550;
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

        #region 删除福利标准
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WelfareStandardID = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_WelfareStandard ent = DaGr.SelectedItems[i] as V_WelfareStandard;
                    if (ent.welfareStandard.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        WelfareStandardID.Add((DaGr.SelectedItems[i] as V_WelfareStandard).welfareStandard.WELFAREID);

                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            wssc.DeleteWelfareStandardAsync(WelfareStandardID);
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

        void wssc_DeleteWelfareStandardCompleted(object sender, DeleteWelfareStandardCompletedEventArgs e)
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

        #region 修改福利标准
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

            V_WelfareStandard ent = DaGr.SelectedItems[0] as V_WelfareStandard;
            if (ent.welfareStandard.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.welfareStandard.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
            {
                BenefitsAdministrationChildWindows AddWin = new BenefitsAdministrationChildWindows(FormTypes.Edit, ent.welfareStandard.WELFAREID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 650;
                browser.MinHeight = 550;
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

        #region 添加福利标准
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            BenefitsAdministrationChildWindows AddWin = new BenefitsAdministrationChildWindows(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinWidth = 650;
            browser.MinHeight = 550;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region AddWin_ReloadDataEvent
        void AddWin_ReloadDataEvent()
        {
            InitEvent();
        }
        #endregion

        #region 当用户导航到此页面时执行
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            wssc.GetWelfareListByUserIdCompleted += new EventHandler<GetWelfareListByUserIdCompletedEventArgs>(wssc_GetWelfareListByUserIdCompleted);//根据用户ID查询
            wssc.DeleteWelfareStandardCompleted += new EventHandler<DeleteWelfareStandardCompletedEventArgs>(wssc_DeleteWelfareStandardCompleted);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            //LoadData();
        }

        private void wssc_GetWelfareListByUserIdCompleted(object sender, GetWelfareListByUserIdCompletedEventArgs e)
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

        #region  绑定DataGird
        private void BindDataGrid(List<V_WelfareStandard> obj, int pageCount)
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

        #region 查看申请事件
        private void btnBro_Click(object sender, RoutedEventArgs e)
        {
            if (WelfareStandardID.Count == 0)
            {
                HtmlPage.Window.Alert("请先选择需要查看的申请记录！");
            }
            else
            {
                wssc.IsWelfareCanBrowserAsync(WelfareStandardID[0]);
            }
        }
        #endregion

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DaGr, e.Row, "T_OA_WELFAREMASERT");
        }
        #endregion

        #region 动态分页
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            cbWelfareID.SelectedIndex = 0;
            ReleaseTime.Text = string.Empty;
            ReleaseEndTime.Text = string.Empty;
        }
        #endregion
    }
}
