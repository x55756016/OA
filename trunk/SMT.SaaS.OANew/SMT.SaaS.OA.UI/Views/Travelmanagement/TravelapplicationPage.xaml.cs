/********************************************************************************

** 作者： 刘锦

** 创始时间：2010-02-22

** 修改人：刘锦

** 修改时间：2010-07-12

** 描述：

**    主要用于出差申请信息的数据展示，将已保存的出差申请数据展示在DataGrid列表控件上

*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Globalization;
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Application;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.Logging;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class TravelapplicationPage : BasePage
    {

        #region 全局变量
        private SmtOAPersonOfficeClient Travelmanagement;
        private ObservableCollection<string> businesstripId = new ObservableCollection<string>();//出差申请ID
        private ObservableCollection<string> MissionReportsId = new ObservableCollection<string>();//出差报告ID
        private List<T_OA_TAKETHESTANDARDTRANSPORT> takethestandardtransport = new List<T_OA_TAKETHESTANDARDTRANSPORT>();//交通工具标准设置
        private List<T_OA_CANTAKETHEPLANELINE> cantaketheplaneline = new List<T_OA_CANTAKETHEPLANELINE>();//可乘坐飞机线路设置
        private T_OA_BUSINESSTRIP businesstripInfo = new T_OA_BUSINESSTRIP();
        //private T_OA_BUSINESSREPORT missionReports = new T_OA_BUSINESSREPORT();
        private T_OA_TRAVELREIMBURSEMENT travelReimbursement = new T_OA_TRAVELREIMBURSEMENT();
        private T_OA_TRAVELSOLUTIONS travelsolutions = new T_OA_TRAVELSOLUTIONS();
        private List<T_OA_AREAALLOWANCE> areaallowance = new List<T_OA_AREAALLOWANCE>();
        private ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TrDetail = new ObservableCollection<T_OA_REIMBURSEMENTDETAIL>();//出差报销子表
        private int Tdetail;//用于判断是否存在出差报销子表记录
        //private ObservableCollection<T_OA_BUSINESSREPORTDETAIL> ReportDetail = new ObservableCollection<T_OA_BUSINESSREPORTDETAIL>();//出差报告从表
        private List<T_OA_AREACITY> areacitys = new List<T_OA_AREACITY>();
        private string checkState = ((int)CheckStates.ALL).ToString();
        SMTLoading loadbar = new SMTLoading();//全局定义loading控件
        private string reportid = string.Empty;
        private string travelreimbursementId = string.Empty;
        private string businesstrID = string.Empty;
        private string SearchUserID = string.Empty;//已员工名为查询条件
        private string postId = string.Empty;
        private string postLevel = string.Empty;//出差人的岗位级别
        private PersonnelServiceClient client = new PersonnelServiceClient();
        private V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
        //private bool ReportSwitch = false;//是否报告控制开关
        private bool ReimbursementSwitch = false;//是否报销控制开关
        private bool WhetherToReport = false;//用于控制点击是否报告按钮指定Form中对应的Tab
        private bool WhetherReimbursement = true;//用于控制点击是否报销按钮指定Form中对应的Tab
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表

        private bool IsCanSave = true;//如果能获取到出差方案、补贴则可以保存。否则给予提示
        #endregion

        #region 构造函数
        public TravelapplicationPage()
        {
            InitializeComponent();
            //DateTime dtNow = DateTime.Now;
            //int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);

            //if (string.IsNullOrWhiteSpace(StartTime.Text))
            //{
            //    StartTime.Text = dtNow.AddDays(-iMaxDay).ToString();
            //}
            //if (string.IsNullOrWhiteSpace(EndTime.Text))
            //{
            //    EndTime.Text = dtNow.AddDays(+iMaxDay).ToString();
            //}
            this.Loaded += new RoutedEventHandler(TravelapplicationPage_Loaded);
        }
        #endregion

        #region TravelapplicationPage_Loaded
        void TravelapplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            InitEvent();
            ListDict.Add("CITY");//城市
            PARENT.Children.Add(loadbar);//在父面板中加载loading控件
            GetEntityLogo("T_OA_BUSINESSTRIP");
            Utility.DisplayGridToolBarButton(FormToolBar1, "T_OA_BUSINESSTRIP", true);
            #endregion
            txtSearchID.Text = Common.CurrentLoginUserInfo.EmployeeName;//默认当前用户
            SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;
            postId = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            string Name = "";
            Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtSearchID.Text = Name;
            ToolTipService.SetToolTip(txtSearchID, Name);

            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);//添加
            FormToolBar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改
            FormToolBar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);//删除
            FormToolBar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);//提交审核
            FormToolBar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交
            FormToolBar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);//查看
            FormToolBar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            Utility.CbxItemBinder(FormToolBar1.cbxCheckState, "CHECKSTATE", checkState);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            //隐藏未使用按钮
            //FormToolBar1.btnSumbitAudit.Visibility = Visibility.Collapsed;//提交待审核
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            FormToolBar1.stpCheckState.Visibility = Visibility.Visible;//检查审核状态
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 加载字典
        /// <summary>
        /// 加载字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                SearchUserID = Common.CurrentLoginUserInfo.EmployeeID;

                string Name = "";
                Name = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                txtSearchID.Text = Name;
                ToolTipService.SetToolTip(txtSearchID, Name);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        #region 重新提交
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            V_Travelmanagement ent = new V_Travelmanagement();
            ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

            ///luojie
            ///增加重新提交的判断，审核通过的不允许重新提交
            if (ent.TraveAppCheckState == "2" && ent.TrCheckState == "2")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("已审核通过的出差报销不能重新提交"), Utility.GetResourceStr("确定"), MessageIcon.Exclamation);
                return;
            }
            if (ent.TraveAppCheckState == "2" && ent.TrCheckState != "3")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("已审核通过的出差申请不能重新提交"), Utility.GetResourceStr("确定"), MessageIcon.Exclamation);
                return;
            }
            BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Resubmit, ent.Travelmanagement.BUSINESSTRIPID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.EntityBrowseToolBar.MaxHeight = 0;
            browser.FormType = FormTypes.Resubmit;
            browser.MinWidth = 980;
            browser.MinHeight = 445;
            browser.TitleContent = "出差申请";
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.EntityEditor = AddWin;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 查看
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }
            V_Travelmanagement ent = new V_Travelmanagement();
            ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

            BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Browse, ent.Travelmanagement.BUSINESSTRIPID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.EntityBrowseToolBar.MaxHeight = 0;
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 830;
            browser.MinHeight = 445;
            browser.TitleContent = "出差申请";
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 页面导航
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion 

        #region cbxCheckState_SelectionChanged
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = FormToolBar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), FormToolBar1, "T_OA_BUSINESSTRIP");
                checkState = dict.DICTIONARYVALUE.ToString();
                LoadData();
            }
            //SetButtonVisible();
        }
        #endregion

        #region 查询、分页LoadData()
        private void LoadData()
        {
            loadbar.Start();//打开转动动画
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(txtNoClaims.Text.Trim()))//报销单号
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "NoClaims =@" + paras.Count().ToString();
                paras.Add(txtNoClaims.Text.Trim());
            }
            if (!string.IsNullOrEmpty(SearchUserID))//出差人
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "Travelmanagement.OWNERID =@" + paras.Count().ToString();
                paras.Add(SearchUserID);
            }
            //if (!string.IsNullOrEmpty(postId))//岗位ID
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " and ";
            //    }
            //    filter += "Travelmanagement.OWNERPOSTID =@" + paras.Count().ToString();
            //    paras.Add(postId);
            //}
            if (!string.IsNullOrEmpty(StartTime.Text.Trim()))//开始时间
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "Travelmanagement.CREATEDATE >=@" + paras.Count().ToString();
                paras.Add(DateTime.Parse(StartTime.Text.Trim()));
            }
            if (!string.IsNullOrEmpty(EndTime.Text.Trim()))//结束时间
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "Travelmanagement.CREATEDATE <=@" + paras.Count().ToString();
                paras.Add(DateTime.Parse(EndTime.Text.Trim()));
            }
            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            Travelmanagement.GetTravelmanagementListByUserIdAsync(dpGrid.PageIndex, dpGrid.PageSize, "Travelmanagement.CREATEDATE descending", filter, paras, pageCount, checkState, loginUserInfo);
        }
        #endregion

        //#region 添加报告Button
        //public void NewButton()
        //{
        //    V_Travelmanagement ent = new V_Travelmanagement();
        //    ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

        //    string reportCheckS = ent.ReportCheckState;
        //    string AppCheckS = ent.Travelmanagement.CHECKSTATE; //出差申请的状态
        //    string travelreimbursementCheckS = ent.TrCheckState;

        //    FormToolBar1.stpOtherAction.Children.Clear();
        //    ImageButton _imgbutton = new ImageButton();
        //    if (AppCheckS == "2" && reportCheckS == "2" && travelreimbursementCheckS == "2")
        //    {
        //        _imgbutton.AddButtonAction("SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4211_d.png", "已报销").Click += new RoutedEventHandler(TravelapplicationPage_Click);
        //    }
        //    else
        //    {
        //        _imgbutton.AddButtonAction("SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_5006.png", "未报销").Click += new RoutedEventHandler(TravelapplicationPage_Click);
        //    }
        //    FormToolBar1.stpOtherAction.Children.Add(_imgbutton);
        //}

        //void TravelapplicationPage_Click(object sender, RoutedEventArgs e)
        //{
        //    if (DaGr.SelectedItems == null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "COMPLETEREPORT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
        //        return;
        //    }

        //    if (DaGr.SelectedItems.Count == 0)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "COMPLETEREPORT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
        //        return;
        //    }

        //    V_Travelmanagement ent = DaGr.SelectedItems[0] as V_Travelmanagement;

        //    TravelReportsForm AddWin = new TravelReportsForm(FormTypes.New, "", null);
        //    EntityBrowser browser = new EntityBrowser(AddWin);
        //    browser.MinWidth = 980;
        //    browser.MinHeight = 421;
        //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        //}
        //#endregion

        #region 新增出差申请
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //Utility.CreateFormFromEngine("a7eb1928-c4a9-442d-82a0-2d57ee4cd87a", "SMT.SaaS.OA.UI.UserControls.BusinessApplicationsForm", "VIEW"); return;
            BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.New, "");
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.EntityBrowseToolBar.MaxHeight = 0;
            AddWin.ParentEntityBrowser = browser;
            browser.MinWidth = 980;
            browser.MinHeight = 380;
            browser.TitleContent = "出差申请";
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }
        #endregion

        #region 修改出差申请
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
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

            V_Travelmanagement ent = new V_Travelmanagement();
            ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(ent, "T_OA_BUSINESSTRIP", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, ent.Travelmanagement.BUSINESSTRIPID);
                EntityBrowser browser = new EntityBrowser(AddWin);
                AddWin.ParentEntityBrowser = browser;
                browser.EntityBrowseToolBar.MaxHeight = 0;
                browser.FormType = FormTypes.Edit;
                browser.MinWidth = 980;
                browser.MinHeight = 445;
                browser.TitleContent = "出差申请";
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
            }
        }
        #endregion

        #region 删除出差申请
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //Modified by luojie 2012/10/08
            //添加对出差申请的删除功能，当出差申请为通过出差报销为未提交的时候，可以删除该出差报销单，并取消相应待办
            businesstripId = new ObservableCollection<string>();
            if (DaGr.SelectedItems.Count > 0)
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    #region 删除出差申请

                    T_OA_BUSINESSTRIP Travelent = new T_OA_BUSINESSTRIP();
                    //T_OA_BUSINESSREPORT entReport = new T_OA_BUSINESSREPORT();
                    Travelent = (DaGr.SelectedItems[i] as V_Travelmanagement).Travelmanagement;
                    string trCheckS=(DaGr.SelectedItems[i] as V_Travelmanagement).TrCheckState;
                    //entReport = (DaGr.SelectedItems[i] as V_MissionReports).MissionReportsViews;
                    
                    //判断用户权限，以出差报销的控制权限为准
                    if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Travelent, "T_OA_BUSINESSTRIP", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
                    {
                        if (Travelent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())// -luojie || Travelent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                        {
                            businesstripId.Add((DaGr.SelectedItems[i] as V_Travelmanagement).Travelmanagement.BUSINESSTRIPID);

                            string Result = "";
                            ComfirmWindow com = new ComfirmWindow();
                            com.OnSelectionBoxClosed += (obj, result) =>
                            {
                                bool FBControl = true;
                                Travelmanagement.DeleteTravelmanagementAsync(businesstripId, FBControl);
                                LoadData();
                            };
                            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                        }
                        //删除出差申请已通过的未提交状态的出差报销
                        else if (Travelent.CHECKSTATE == ((int)CheckStates.Approved).ToString() && trCheckS==((int)CheckStates.UnSubmit).ToString())
                        {
                            //Travelreimbursement.deletetr
                            businesstripId.Add((DaGr.SelectedItems[i] as V_Travelmanagement).Travelmanagement.BUSINESSTRIPID);
                            string Result = "";
                            ComfirmWindow com = new ComfirmWindow();
                            com.OnSelectionBoxClosed += (obj, result) =>
                            {
                                bool FBControl = true;
                                Travelmanagement.DeleteTravelReimbursementByBusinesstripIdAsync(businesstripId, FBControl);
                                loadbar.Start();
                                //LoadData();
                            };
                            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                            
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"));
                            return;
                        }
                    }                    
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                    }
                    #endregion

                    //#region 删除出差报告
                    //if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(entReport, "T_OA_BUSINESSREPORT", OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
                    //{
                    //    if (entReport.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || entReport.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                    //    {
                    //        MissionReportsId.Add((DaGr.SelectedItems[i] as V_MissionReports).MissionReportsViews.BUSINESSREPORTID);

                    //        string Result = "";
                    //        ComfirmWindow com = new ComfirmWindow();
                    //        com.OnSelectionBoxClosed += (obj, result) =>
                    //        {
                    //            bool FBControl = true;
                    //            Travelmanagement.DeleteMissionReportsAsync(MissionReportsId, FBControl);
                    //            LoadData();
                    //        };
                    //        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                    //    }
                    //    else
                    //    {
                    //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                    //        return;
                    //    }
                    //}
                    //else
                    //{
                    //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                    //}
                    //#endregion
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        #endregion

        #region 审核
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }
            V_Travelmanagement ent = new V_Travelmanagement();
            ent = (DaGr.SelectedItems[0] as V_Travelmanagement);

            BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Audit, ent.Travelmanagement.BUSINESSTRIPID);
            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.EntityBrowseToolBar.MaxHeight = 0;
            browser.FormType = FormTypes.Audit;
            browser.MinWidth = 728;
            browser.MinHeight = 445;
            browser.TitleContent = "出差申请";
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region 数据初始化(InitEvent)
        private void InitEvent()
        {
            Travelmanagement = new SmtOAPersonOfficeClient();
            Travelmanagement.GetTravelmanagementListByUserIdCompleted += new EventHandler<GetTravelmanagementListByUserIdCompletedEventArgs>(Travelmanagement_GetTravelmanagementListByUserIdCompleted);//根据用户ID查询
            Travelmanagement.DeleteTravelmanagementCompleted += new EventHandler<DeleteTravelmanagementCompletedEventArgs>(Travelmanagement_DeleteTravelmanagementCompleted);
            //删除出差报销
            Travelmanagement.DeleteTravelReimbursementByBusinesstripIdCompleted += new EventHandler<DeleteTravelReimbursementByBusinesstripIdCompletedEventArgs>(Travelmanagement_DeleteTravelReimbursementByBusinesstripIdCompleted);
            //Travelmanagement.DeleteMissionReportsCompleted += new EventHandler<DeleteMissionReportsCompletedEventArgs>(MrSc_DeleteMissionReportsCompleted);
            FormToolBar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            Travelmanagement.GetTravelmanagementByIdCompleted += new EventHandler<GetTravelmanagementByIdCompletedEventArgs>(Travelmanagement_GetTravelmanagementByIdCompleted);
            Travelmanagement.GetBusinesstripDetailCompleted += new EventHandler<GetBusinesstripDetailCompletedEventArgs>(Travelmanagement_GetBusinesstripDetailCompleted);
            Travelmanagement.GetTravelSolutionByCompanyIDCompleted += new EventHandler<GetTravelSolutionByCompanyIDCompletedEventArgs>(Travelmanagement_GetTravelSolutionByCompanyIDCompleted);
            //Travelmanagement.GetMissionReportsByIdCompleted += new EventHandler<GetMissionReportsByIdCompletedEventArgs>(MrSc_GetMissionReportsByIdCompleted);
            //Travelmanagement.GetBusinessPortDetailCompleted += new EventHandler<GetBusinessPortDetailCompletedEventArgs>(MrSc_GetBusinessPortDetailCompleted);
            //Travelmanagement.MissionReportsAddCompleted += new EventHandler<MissionReportsAddCompletedEventArgs>(MrSc_MissionReportsAddCompleted);//添加
            Travelmanagement.TravelReimbursementAddCompleted += new EventHandler<TravelReimbursementAddCompletedEventArgs>(TrC_TravelReimbursementAddCompleted);
            Travelmanagement.GetTravelReimbursementByIdCompleted += new EventHandler<GetTravelReimbursementByIdCompletedEventArgs>(TrC_GetTravelReimbursementByIdCompleted);
            Travelmanagement.GetTravleAreaAllowanceByPostValueCompleted += new EventHandler<GetTravleAreaAllowanceByPostValueCompletedEventArgs>(TrC_GetTravleAreaAllowanceByPostValueCompleted);
            client.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetEmployeePostBriefByEmployeeIDCompleted);
            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);//加载字典
        }
        #endregion

        //#region 添加报告Completed
        //void MrSc_MissionReportsAddCompleted(object sender, MissionReportsAddCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error != null && e.Error.Message != "")
        //        {
        //            if (e.UserState != null)
        //            {
        //                Button btn = e.UserState as Button;
        //                btn.IsEnabled = true;
        //            }
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //        }
        //        else
        //        {
        //            if (e.Result != "")
        //            {
        //                if (e.UserState != null)
        //                {
        //                    Button btn = e.UserState as Button;
        //                    btn.IsEnabled = true;
        //                }
        //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //                return;
        //            }
        //            Travelmanagement.GetMissionReportsByIdAsync(missionReports.BUSINESSREPORTID);//添加完后执行查询
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        if (e.UserState != null)
        //        {
        //            Button btn = e.UserState as Button;
        //            btn.IsEnabled = true;
        //        }
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}
        //#endregion

        #region 根据报销ID查询数据
        void TrC_GetTravelReimbursementByIdCompleted(object sender, GetTravelReimbursementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    travelReimbursement = e.Result;
                    if (!string.IsNullOrEmpty(travelReimbursement.TRAVELREIMBURSEMENTID))
                    {
                        BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID);
                        EntityBrowser browser = new EntityBrowser(AddWin);
                        browser.FormType = FormTypes.Edit;
                        browser.MinWidth = 980;
                        browser.MinHeight = 445;
                        browser.TitleContent = "出差申请";
                        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                    }
                    LoadData();//重新加载数据(主要用于刷新"是否报销"按钮的状态)
                    ReimbursementSwitch = false;//关闭开关
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 添加出差报销Completed
        void TrC_TravelReimbursementAddCompleted(object sender, TravelReimbursementAddCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    Travelmanagement.GetTravelReimbursementByIdAsync(travelReimbursement.TRAVELREIMBURSEMENTID);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 获出差人信息
        void client_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                return;
            }

            if (e.Result != null)
            {
                employeepost = e.Result;
                if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == businesstripInfo.OWNERPOSTID).FirstOrDefault() != null)
                {
                    postLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == businesstripInfo.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取出差人的岗位级别
                }
                else
                {
                    var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
                    postLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
                }
                //if (ReportSwitch == true)
                //{
                //    Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null);//出差方案
                //}
                if (ReimbursementSwitch == true)
                {
                    Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null, e.UserState);//出差方案
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，该员工已离职，不能进行该操作"));
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
            }
        }
        #endregion

        #region 获取交通工具设置
        void TrC_GetTravleAreaAllowanceByPostValueCompleted(object sender, GetTravleAreaAllowanceByPostValueCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                else
                {
                    if (e.Result != null)
                    {
                        areaallowance = e.Result.ToList();
                        areacitys = e.citys.ToList();

                        if (businesstripInfo.BUSINESSTRIPID != null)
                        {
                            Travelmanagement.GetBusinesstripDetailAsync(businesstripInfo.BUSINESSTRIPID);//申请明细
                        }
                        //if (missionReports.BUSINESSREPORTID != null)
                        //{
                        //    Travelmanagement.GetBusinessPortDetailAsync(missionReports.BUSINESSREPORTID, e.UserState);//报告明细
                        //}
                        //if (ReportSwitch == true)
                        //{
                        //    Travelmanagement.GetTravelSolutionByCompanyIDAsync(businesstripInfo.OWNERCOMPANYID, null, null);//出差方案
                        //}
                        //if (ReimbursementSwitch == true)
                        //{
                        //    Travelmanagement.GetTravelSolutionByCompanyIDAsync(missionReports.OWNERCOMPANYID, null, null, e.UserState);//出差方案
                        //}
                    }
                    else
                    {
                        IsCanSave = false;
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司对应的出差方案没有补贴，请重新关联出差方案", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 根据城市值获取相应的出差补贴
        /// <summary>
        /// 根据城市值  获取相应的出差补贴
        /// </summary>
        /// <param name="CityValue"></param>
        private T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            var q = from ent in areaallowance
                    join ac in areacitys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == travelsolutions.TRAVELSOLUTIONSID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }
        #endregion

        #region 获取出差报告子表数据
        /// <summary>
        /// 获取出差报告子表数据(查询完后将报告的明细保存到报销中)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetBusinesstripDetailCompleted(object sender, GetBusinesstripDetailCompletedEventArgs e)//查询报告明细
        {
            try
            {
                if (ReimbursementSwitch == true)
                {
                    List<T_OA_BUSINESSTRIPDETAIL> TravelReportDetail = new List<T_OA_BUSINESSTRIPDETAIL>();
                    if (e.Result.Count > 0)
                    {
                        TrDetail.Clear();//清理报销子表
                        TravelReportDetail = e.Result.ToList();
                        List<string> cityscode = new List<string>();
                        double BusinessDays = 0;
                        int i = 0;
                        double total = 0;

                        foreach (var detail in TravelReportDetail)
                        {
                            i++;
                            double toodays = 0;

                            //计算本次出差的时间
                            List<string> list = new List<string>
                        {
                             detail.BUSINESSDAYS
                        };
                            if (detail.BUSINESSDAYS != null)
                            {
                                double totalHours = System.Convert.ToDouble(list[0]);

                                BusinessDays += totalHours;//总天数
                                toodays = totalHours;//单条数据的天数
                            }
                            double tresult = toodays;//计算本次出差的总天数

                            T_OA_REIMBURSEMENTDETAIL TrListInfo = new T_OA_REIMBURSEMENTDETAIL();
                            TrListInfo.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

                            TrListInfo.STARTDATE = detail.STARTDATE;//开始时间
                            TrListInfo.ENDDATE = detail.ENDDATE;//结束时间
                            TrListInfo.BUSINESSDAYS = detail.BUSINESSDAYS;//出差天数
                            TrListInfo.DEPCITY = detail.DEPCITY;//出发城市
                            TrListInfo.DESTCITY = detail.DESTCITY;//目标城市
                            TrListInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
                            TrListInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//外出开会
                            TrListInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
                            TrListInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
                            TrListInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
                            TrListInfo.CREATEDATE = Convert.ToDateTime(businesstripInfo.UPDATEDATE);//创建时间
                            TrListInfo.CREATEUSERNAME = businesstripInfo.CREATEUSERNAME;//创建人
                            cityscode.Add(TrListInfo.DESTCITY);

                            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                            string cityValue = cityscode[i - 1];//目标城市值
                            entareaallowance = GetAllowanceByCityValue(cityValue);

                            #region 根据本次出差的总天数,根据天数获取相应的补贴
                            if (travelsolutions != null)
                            {
                                if (tresult <= int.Parse(travelsolutions.MINIMUMINTERVALDAYS))//本次出差总时间小于等于设定天数的报销标准
                                {
                                    if (entareaallowance != null)
                                    {
                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;//交通补贴
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    if (entareaallowance.TRANSPORTATIONSUBSIDIES != null)
                                                    {
                                                        TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * toodays).ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//餐费补贴
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * toodays).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            #endregion

                            #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
                            if (travelsolutions != null)
                            {
                                if (tresult > int.Parse(travelsolutions.MAXIMUMRANGEDAYS))
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                                    double middlemoney = (Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS) - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                                    double lastmoney = (tresult - Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS)) * Convert.ToDouble(entareaallowance.OVERSEASSUBSIDIES);
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney + lastmoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            #endregion

                            #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
                            if (travelsolutions != null)
                            {
                                if (tresult >= Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) && tresult <= Convert.ToDouble(travelsolutions.MAXIMUMRANGEDAYS))
                                {
                                    if (entareaallowance != null)
                                    {
                                        double DbTranceport = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES);
                                        double DbMeal = Convert.ToDouble(entareaallowance.MEALSUBSIDIES);
                                        double tfSubsidies = Convert.ToDouble(entareaallowance.TRANSPORTATIONSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);
                                        double mealSubsidies = Convert.ToDouble(entareaallowance.MEALSUBSIDIES) * (Convert.ToDouble(travelsolutions.INTERVALRATIO) / 100);

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1" || detail.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbTranceport;
                                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * tfSubsidies;
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                                }
                                            }
                                        }

                                        if (detail.BUSINESSDAYS != null)
                                        {
                                            if (detail.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else if (detail.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                TrListInfo.MEALSUBSIDIES = 0;
                                            }
                                            else
                                            {
                                                if (int.Parse(postLevel) > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    //最小区间段金额
                                                    double minmoney = Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS) * DbMeal;
                                                    //中间区间段金额
                                                    double middlemoney = (tresult - Convert.ToDouble(travelsolutions.MINIMUMINTERVALDAYS)) * mealSubsidies;
                                                    TrListInfo.MEALSUBSIDIES = decimal.Parse((minmoney + middlemoney).ToString());
                                                }
                                                else
                                                {
                                                    TrListInfo.MEALSUBSIDIES = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (int.Parse(postLevel) <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        TrListInfo.TRANSPORTATIONSUBSIDIES = 0;
                                        TrListInfo.MEALSUBSIDIES = 0;
                                    }
                                }
                            }
                            total += Convert.ToDouble(TrListInfo.TRANSPORTATIONSUBSIDIES + TrListInfo.MEALSUBSIDIES);
                            travelReimbursement.THETOTALCOST = decimal.Parse(total.ToString());//差旅费用总和
                            travelReimbursement.REIMBURSEMENTOFCOSTS = decimal.Parse(total.ToString());//报销费用总和

                            #endregion

                            TrDetail.Add(TrListInfo);
                        }
                        string result = BusinessDays.ToString(); //计算本次出差的总时间,超过24小时天数加1
                        travelReimbursement.COMPUTINGTIME = result;//总时间
                        Button btn = e.UserState as Button;
                        Travelmanagement.TravelReimbursementAddAsync(travelReimbursement, TrDetail, btn);//保存出差报销
                    }
                }
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 获取出差报告主表数据
        /// <summary>
        /// 获取出差报告主表数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelmanagementByIdCompleted(object sender, GetTravelmanagementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result != null)
                    {
                        businesstripInfo = e.Result;

                        //if (ReimbursementSwitch == true)//如果是操作是否报告按钮
                        //{
                        //    if (!string.IsNullOrEmpty(businesstripInfo.BUSINESSTRIPID))//添加成功才能弹出Form
                        //    {
                        //        BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID);
                        //        EntityBrowser browser = new EntityBrowser(AddWin);
                        //        browser.FormType = FormTypes.Edit;
                        //        browser.MinWidth = 980;
                        //        browser.MinHeight = 445;
                        //        browser.TitleContent = "出差申请";
                        //        browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                        //        browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                        //    }
                        //    LoadData();//重新加载数据(主要用于刷新"是否报告"按钮的状态)
                        //    ReimbursementSwitch = false;//关闭开关
                        //}
                        if (ReimbursementSwitch == true)//如果是操作是否报销按钮
                        {
                            travelReimbursement.TRAVELREIMBURSEMENTID = Guid.NewGuid().ToString();
                            travelReimbursement.T_OA_BUSINESSTRIP = businesstripInfo;
                            travelReimbursement.T_OA_BUSINESSTRIP.BUSINESSTRIPID = businesstripInfo.BUSINESSTRIPID;
                            travelReimbursement.CLAIMSWERE = businesstripInfo.OWNERID;
                            travelReimbursement.CLAIMSWERENAME = businesstripInfo.OWNERNAME;
                            travelReimbursement.REIMBURSEMENTTIME = DateTime.Now;
                            travelReimbursement.CHECKSTATE = "0";
                            travelReimbursement.TEL = businesstripInfo.TEL;
                            travelReimbursement.CREATEDATE = businesstripInfo.UPDATEDATE;
                            travelReimbursement.OWNERID = businesstripInfo.OWNERID;
                            travelReimbursement.OWNERNAME = businesstripInfo.OWNERNAME;
                            travelReimbursement.OWNERPOSTID = businesstripInfo.OWNERPOSTID;
                            travelReimbursement.OWNERDEPARTMENTID = businesstripInfo.OWNERDEPARTMENTID;
                            travelReimbursement.OWNERCOMPANYID = businesstripInfo.OWNERCOMPANYID;
                            travelReimbursement.CREATEUSERID = businesstripInfo.CREATEUSERID;
                            travelReimbursement.CREATEUSERNAME = businesstripInfo.CREATEUSERNAME;
                            travelReimbursement.CREATEPOSTID = businesstripInfo.CREATEPOSTID;
                            travelReimbursement.CREATEDEPARTMENTID = businesstripInfo.CREATEDEPARTMENTID;
                            travelReimbursement.CREATECOMPANYID = businesstripInfo.CREATECOMPANYID;

                            client.GetEmployeePostBriefByEmployeeIDAsync(businesstripInfo.OWNERID, e.UserState);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 获取出差方案设置
        /// <summary>
        /// 获取出差方案设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    if (e.UserState != null)
                    {
                        Button btn = e.UserState as Button;
                        btn.IsEnabled = true;
                    }
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                if (e.Result != null)
                {
                    travelsolutions = e.Result;//出差方案
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "没有找到对应的出差方案，不能产生出差报销单", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null)
                {
                    takethestandardtransport = e.StandardObj.ToList();//乘坐交通工具设置
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "没有获取到交通工具设置，不能产生出差报销单", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                //if (businesstripInfo.BUSINESSTRIPID != null)
                //{
                //    Travelmanagement.GetBusinesstripDetailAsync(businesstripInfo.BUSINESSTRIPID);//申请明细
                //}
                //if (missionReports.BUSINESSREPORTID != null)
                //{
                //    Travelmanagement.GetBusinessPortDetailAsync(missionReports.BUSINESSREPORTID, e.UserState);//报告明细
                //}
                Travelmanagement.GetTravleAreaAllowanceByPostValueAsync(postLevel, travelsolutions.TRAVELSOLUTIONSID, null, e.UserState);
            }
            catch (Exception ex)
            {
                if (e.UserState != null)
                {
                    Button btn = e.UserState as Button;
                    btn.IsEnabled = true;
                }
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        //#region 查询出差申请子表数据
        ///// <summary>
        ///// 查询出差申请子表数据
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Travelmanagement_GetBusinesstripDetailCompleted(object sender, GetBusinesstripDetailCompletedEventArgs e)
        //{
        //    try
        //    {
        //        List<T_OA_BUSINESSTRIPDETAIL> TravelDetail = new List<T_OA_BUSINESSTRIPDETAIL>();
        //        if (e.Result != null)
        //        {
        //            ReportDetail.Clear();//清理报告子表
        //            TravelDetail = e.Result.ToList();
        //            foreach (var detail in TravelDetail)
        //            {
        //                T_OA_BUSINESSREPORTDETAIL RepotDetailInfo = new T_OA_BUSINESSREPORTDETAIL();
        //                RepotDetailInfo.BUSINESSREPORTDETAILID = Guid.NewGuid().ToString();

        //                RepotDetailInfo.DEPCITY = detail.DEPCITY;//出发城市
        //                RepotDetailInfo.DESTCITY = detail.DESTCITY;//到达城市
        //                RepotDetailInfo.STARTDATE = detail.STARTDATE;//出发时间
        //                RepotDetailInfo.ENDDATE = detail.ENDDATE;//到达时间
        //                RepotDetailInfo.PRIVATEAFFAIR = detail.PRIVATEAFFAIR;//是否私事
        //                RepotDetailInfo.GOOUTTOMEET = detail.GOOUTTOMEET;//是否是开会
        //                RepotDetailInfo.COMPANYCAR = detail.COMPANYCAR;//公司派车
        //                RepotDetailInfo.TYPEOFTRAVELTOOLS = detail.TYPEOFTRAVELTOOLS;//交通工具类型
        //                RepotDetailInfo.TAKETHETOOLLEVEL = detail.TAKETHETOOLLEVEL;//交通工具级别
        //                ReportDetail.Add(RepotDetailInfo);
        //            }
        //            Travelmanagement.MissionReportsAddAsync(missionReports, ReportDetail);//执行添加
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        if (e.UserState != null)
        //        {
        //            Button btn = e.UserState as Button;
        //            btn.IsEnabled = true;
        //        }
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}
        //#endregion

        //#region 查询出差申请主表数据
        ///// <summary>
        ///// 查询出差申请主表数据
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Travelmanagement_GetTravelmanagementByIdCompleted(object sender, GetTravelmanagementByIdCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error == null)
        //        {
        //            if (e.Result != null)
        //            {
        //                businesstripInfo = e.Result;

        //                //主表添加
        //                missionReports.BUSINESSREPORTID = Guid.NewGuid().ToString();
        //                missionReports.T_OA_BUSINESSTRIP = businesstripInfo;
        //                missionReports.T_OA_BUSINESSTRIP.BUSINESSTRIPID = businesstripInfo.BUSINESSTRIPID;
        //                missionReports.CHECKSTATE = "0";
        //                missionReports.CREATEDATE = businesstripInfo.UPDATEDATE;
        //                missionReports.OWNERID = businesstripInfo.OWNERID;
        //                missionReports.OWNERNAME = businesstripInfo.OWNERNAME;
        //                missionReports.OWNERPOSTID = businesstripInfo.OWNERPOSTID;
        //                missionReports.OWNERDEPARTMENTID = businesstripInfo.OWNERDEPARTMENTID;
        //                missionReports.OWNERCOMPANYID = businesstripInfo.OWNERCOMPANYID;
        //                missionReports.CREATEUSERID = businesstripInfo.CREATEUSERID;
        //                missionReports.CREATEUSERNAME = businesstripInfo.CREATEUSERNAME;
        //                missionReports.CREATEPOSTID = businesstripInfo.CREATEPOSTID;
        //                missionReports.CREATEDEPARTMENTID = businesstripInfo.CREATEDEPARTMENTID;
        //                missionReports.CREATECOMPANYID = businesstripInfo.CREATECOMPANYID;
        //                missionReports.CONTENT = businesstripInfo.CONTENT;
        //                missionReports.TEL = businesstripInfo.TEL;

        //                client.GetEmployeePostBriefByEmployeeIDAsync(missionReports.OWNERID);
        //            }
        //        }
        //        else
        //        {
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        if (e.UserState != null)
        //        {
        //            Button btn = e.UserState as Button;
        //            btn.IsEnabled = true;
        //        }
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}
        //#endregion

        #region 删除出差申请
        void Travelmanagement_DeleteTravelmanagementCompleted(object sender, DeleteTravelmanagementCompletedEventArgs e)
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
                    if (!e.Result) //返回值为假
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                        return;
                    }
                    if (!e.FBControl)
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

        #region 删除出差报销
        void Travelmanagement_DeleteTravelReimbursementByBusinesstripIdCompleted(object sender, DeleteTravelReimbursementByBusinesstripIdCompletedEventArgs e)
        {
            try
            {
                loadbar.Stop();
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    if (!e.Result) //返回值为假
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                        return;
                    }
                    if (!e.FBControl)
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
        //#region 删除出差报告
        //void MrSc_DeleteMissionReportsCompleted(object sender, DeleteMissionReportsCompletedEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Error != null)
        //        {
        //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
        //            return;
        //        }
        //        else
        //        {
        //            if (!e.Result) //返回值为假
        //            {
        //                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
        //                return;
        //            }
        //            if (!e.FBControl)
        //            {
        //                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
        //                return;
        //            }
        //            LoadData();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
        //    }
        //    loadbar.Stop();//读取完数据后，停止动画，隐藏
        //}
        //#endregion

        #region 查询出差数据(申请、报告、报销)
        void Travelmanagement_GetTravelmanagementListByUserIdCompleted(object sender, GetTravelmanagementListByUserIdCompletedEventArgs e)
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

        #region  绑定DataGird
        private void BindDataGrid(List<V_Travelmanagement> obj, int pageCount)
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
        private ObservableCollection<T_OA_BUSINESSTRIP> GetSelectEmpSurveys()
        {
            if (DaGr.ItemsSource != null)
            {
                ObservableCollection<T_OA_BUSINESSTRIP> selectedObj = new ObservableCollection<T_OA_BUSINESSTRIP>();
                foreach (object obj in DaGr.ItemsSource)
                {
                    if (DaGr.Columns[0].GetCellContent(obj) != null)
                    {
                        CheckBox ckbSelect = DaGr.Columns[0].GetCellContent(obj).FindName("myChkBox") as CheckBox;
                        if (ckbSelect.IsChecked == true)
                        {
                            selectedObj.Add((T_OA_BUSINESSTRIP)obj);
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

        #region DataGrid LoadingRow事件
        private void DaGr_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            V_Travelmanagement tmp = (V_Travelmanagement)e.Row.DataContext;
            string reportId = tmp.ReportId;//报告ID
            string bursementId = tmp.TrId;//报销ID
            string reportCheckS = tmp.ReportCheckState;//出差报告审核状态
            string AppCheckS = tmp.Travelmanagement.CHECKSTATE; //出差申请审核状态
            string trCheckS = tmp.TrCheckState;//报销审核状态
            
            SetRowLogo(DaGr, e.Row, "T_OA_BUSINESSTRIP");

            Button ReportButton_Addbaodao = DaGr.Columns[11].GetCellContent(e.Row).FindName("ReportBtn") as Button;
            Button MyButton_Addbaodao = DaGr.Columns[12].GetCellContent(e.Row).FindName("myBtn") as Button;
            MyButton_Addbaodao.Margin = new Thickness(0);
            ReportButton_Addbaodao.Margin = new Thickness(0);
            ReportButton_Addbaodao.IsEnabled = false;//默认将是否已报告的Button禁用
            MyButton_Addbaodao.IsEnabled = false;//默认将是否已报销的Button禁用

            //luojie 20120808
            if (!string.IsNullOrEmpty(bursementId) && bursementId != "空")//如果自动生成了报销
            {
                MyButton_Addbaodao.Content = "未报销";
                MyButton_Addbaodao.IsEnabled = true;
                if (trCheckS == "2")
                {
                    MyButton_Addbaodao.Content = "已报销";
                    MyButton_Addbaodao.IsEnabled = false;
                }
                else if(trCheckS == "1")
                {
                    MyButton_Addbaodao.Content = "报销中";
                    MyButton_Addbaodao.IsEnabled = true;
                }
            }
            else
            {
                if (AppCheckS == "2")
                {
                    MyButton_Addbaodao.IsEnabled = true;//启用弹出Form的Button
                }
                MyButton_Addbaodao.Content = "未报销";
            }
                //出差申请没有审核通过则报销按钮不可用
            if (AppCheckS != "2")
            {
                MyButton_Addbaodao.Content = "未报销";
                MyButton_Addbaodao.IsEnabled = false;
            }
                ////报告判断
                //if (!string.IsNullOrEmpty(reportId) && reportId != "空")//如果自动生成了报告
                //{
                //    ReportButton_Addbaodao.Content = "未报告";
                //    ReportButton_Addbaodao.IsEnabled = true;
                //    if (reportCheckS == "2")
                //    {
                //        ReportButton_Addbaodao.Content = "已报告";
                //        ReportButton_Addbaodao.IsEnabled = false;
                //    }
                //}
                //else               //{
              //    if (AppCheckS == "2")
              //    {
              //        ReportButton_Addbaodao.IsEnabled = true;//启用弹出Form的Button
              //    }
              //    ReportButton_Addbaodao.Content = "未报告";
              //}
              MyButton_Addbaodao.Tag = tmp;
              ReportButton_Addbaodao.Tag = tmp;
        }
        #endregion

        #region SetButtonVisible
        private void SetButtonVisible()
        {
            switch (checkState)
            {
                case "0":  //草稿箱
                    this.DaGr.Columns[12].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
                case "1":  //审批中
                    this.DaGr.Columns[12].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
                case "2":  //审批通过
                    this.DaGr.Columns[12].Visibility = Visibility.Visible;//显示操作列
                    break;
                case "3":  //审批未通过
                    this.DaGr.Columns[12].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
                case "4":  //待审核
                    this.DaGr.Columns[12].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
                case "5":  //所有
                    this.DaGr.Columns[12].Visibility = Visibility.Collapsed;//隐藏操作列
                    break;
            }
        }
        #endregion

        #region 查询
        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = StartTime.Text.ToString();
            StrEnd = EndTime.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd);
                if (DtStart > DtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTBEGREATERTHANENDDATE", "STARTDATE"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                    return;
                }
            }
            LoadData();
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 是否报销
        private void myBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.IsEnabled = false;
            if (DaGr.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            if (DaGr.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
                return;
            }

            ///luojie 20120808
            ///未报销按钮的权限控制，同修改按钮
            V_Travelmanagement entrb = new V_Travelmanagement();
            entrb = DaGr.SelectedItems[0] as V_Travelmanagement;
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(entrb, "T_OA_BUSINESSTRIP", OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                for (int i = 0; i < DaGr.SelectedItems.Count; i++)
                {
                    V_Travelmanagement ent = new V_Travelmanagement();
                    ent = (DaGr.SelectedItems[i] as V_Travelmanagement);

                    reportid = ent.ReportId;
                    businesstrID = ent.Travelmanagement.BUSINESSTRIPID;
                    travelreimbursementId = ent.TrId;
                    Tdetail = ent.Tdetail;
                }

                if (!string.IsNullOrEmpty(travelreimbursementId) && travelreimbursementId != "空" && Tdetail > 0)//如果已生成报销单，直接打开表单提交
                {
                    WhetherReimbursement = false;
                    BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID, WhetherReimbursement);
                    EntityBrowser browser = new EntityBrowser(AddWin);
                    browser.FormType = FormTypes.Edit;
                    browser.EntityBrowseToolBar.MaxHeight = 0;
                    browser.MinWidth = 980;
                    browser.MinHeight = 445;
                    browser.TitleContent = "出差申请";
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
                else
                {
                    ReimbursementSwitch = true;//出差报销开关
                    Travelmanagement.GetTravelmanagementByIdAsync(businesstrID, btn);
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，您没有修改该用户报销的权限"));
            }
        }
        #endregion

        #region 根据出差人查询
        private void btnLookUpUserName_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    string depName = dept.ObjectName;//部门

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司

                    string Mobile = "";
                    string Tel = "";
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE != null)
                        Mobile = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE.ToString();
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE != null)
                        Tel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE.ToString();
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    txtSearchID.Text = StrEmployee;
                    ToolTipService.SetToolTip(txtSearchID, StrEmployee);

                    SearchUserID = userInfo.ObjectID;
                    postId = postid;
                    txtSearchID.Text = StrEmployee;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region DataGridSelectionChanged事件
        private void DaGr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (DaGr.ItemsSource == null)
            //{
            //    return;
            //}

            //for (int i = 0; i < DaGr.SelectedItems.Count; i++)
            //{
            //    T_OA_BUSINESSTRIP ent = DaGr.SelectedItems[i] as T_OA_BUSINESSTRIP;
            //    if (ent.CHECKSTATE == ((int)CheckStates.Approved).ToString())
            //    {
            //        SetButtonVisible();
            //    }
            //}
        }
        #endregion

        #region 清空查询条件
        private void EmptyBtn_Click(object sender, RoutedEventArgs e)
        {
            txtNoClaims.Text = string.Empty;
            txtSearchID.Text = string.Empty;
            StartTime.Text = string.Empty;
            EndTime.Text = string.Empty;
            SearchUserID = string.Empty;
            postId = string.Empty;
        }
        #endregion

        #region 是否报告
        private void ReportBtn_Click(object sender, RoutedEventArgs e)
        {
            //Button btn = sender as Button;
            //btn.IsEnabled = false;
            //if (DaGr.SelectedItems == null)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
            //    return;
            //}

            //if (DaGr.SelectedItems.Count == 0)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "OPERATION"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
            //    return;
            //}

            //for (int i = 0; i < DaGr.SelectedItems.Count; i++)
            //{
            //    V_Travelmanagement ent = new V_Travelmanagement();
            //    ent = (DaGr.SelectedItems[i] as V_Travelmanagement);

            //    businesstrID = ent.Travelmanagement.BUSINESSTRIPID;
            //    reportid = ent.ReportId;
            //}
            //if (!string.IsNullOrEmpty(reportid) && reportid != "空")//如果已生成报告，直接打开表单让用户提交
            //{
            //    WhetherToReport = true;
            //    BusinessApplicationsForm AddWin = new BusinessApplicationsForm(FormTypes.Edit, businesstrID, WhetherToReport);
            //    EntityBrowser browser = new EntityBrowser(AddWin);
            //    browser.FormType = FormTypes.Edit;
            //    browser.EntityBrowseToolBar.MaxHeight = 0;
            //    browser.MinWidth = 980;
            //    browser.MinHeight = 445;
            //    browser.TitleContent = "出差申请";
            //    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            //    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            //}
            //else
            //{
            //    ReportSwitch = true;//出差报告开关
            //    Travelmanagement.GetTravelmanagementByIdAsync(businesstrID, btn);
            //}
        }
        #endregion
    }
}
