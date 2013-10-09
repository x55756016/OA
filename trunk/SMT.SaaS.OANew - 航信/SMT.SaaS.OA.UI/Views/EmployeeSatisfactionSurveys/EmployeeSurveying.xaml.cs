//----------------------------------------------------
//文件名：EmployeeSurveying.xaml.cs
//作  用：员工调查参与调查管理页面
//修改人：勒中玉
//修改时间：2011-8-11
//修改内容：规范化变量命名及添加注释等
//---------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SMT.SAAS.ClientServices.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.Form.EmployeeSurveys;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class EmployeeSurveying : BasePage
    {
        //#region 全局变量定义
        //private SMTLoading loadbar = null;
        //private SmtOAPersonOfficeClient client = null;
        //private bool _isQuery = false;
        //string _stateFlag = Convert.ToInt32(CheckStates.WaittingApproval).ToString();//未提交
        //#endregion

        //#region 构造函数

        //public EmployeeSurveying()
        //{
        //    InitializeComponent();
        //    this.Loaded += new RoutedEventHandler(EmployeeSurveying_Loaded);
        //}

        //#endregion

        //#region 后台事件回调函数

        //void EmployeeSurveying_Loaded(object sender, RoutedEventArgs e)
        //{
        //    EventRegister();
        //    this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
        //    this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
        //    GetEntityLogo("T_OA_REQUIRERESULT");
        //    GetData(dataPager.PageIndex, _stateFlag);
        //}

        ///// <summary>
        ///// 删除后
        ///// </summary>
        //private void Del_ESurveyAppCompleted(object sender, Del_ESurveyAppCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        return;
        //    }
        //    if (e.Result > 0)
        //    {
        //        GetData(dataPager.PageIndex, _stateFlag);
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"));
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //}

        ///// <summary>
        ///// 获取数据后
        ///// </summary>
        //void Get_MyVisistedSurveyCompleted(object sender, Get_MyVisistedSurveyCompletedEventArgs e)
        //{
        //    loadbar.Stop();
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        return;
        //    }
        //    _isQuery = false;
        //    ObservableCollection<V_MyEusurvey> dataList = e.Result;
        //    if (dataList != null && dataList.Count() > 0)
        //    {
        //        BindDateGrid(dataList);
        //    }
        //    else
        //    {
        //        BindDateGrid(null);
        //    }
        //}

        ///// <summary>
        ///// 查看
        ///// </summary>
        //void btnView_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsSelect("VIEW"))
        //    {
        //        ObservableCollection<V_MyEusurvey> selectItems = GetSelectList();
        //        if (selectItems != null)
        //        {
        //            EmployeeSubmissions_3 frmEmpSurveysSubmit = new EmployeeSubmissions_3(selectItems[0].OAMaster, true, FormTypes.Browse);//empSurveysInfo
        //            frmEmpSurveysSubmit.UserID = Common.CurrentLoginUserInfo.EmployeeID;
        //            EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
        //            browser.FormType = FormTypes.Browse;
        //            browser.MinHeight = 510;
        //            browser.MinWidth = 650;
        //            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        //        }
        //    }
        //}

        ///// <summary>
        ///// 清空
        ///// </summary>
        //private void btnReset_Click(object sender, RoutedEventArgs e)
        //{
        //    this.txtSurveysContent.Text = string.Empty;
        //    this.txtSurveysTITLE.Text = string.Empty;
        //    this.dpStart.SelectedDate = null;
        //    this.dpEnd.SelectedDate = null;
        //}
   
        ///// <summary>
        ///// 查询
        ///// </summary>
        //private void btnSearch_Click(object sender, RoutedEventArgs e)
        //{
        //    _isQuery = true;
        //    GetData(dataPager.PageIndex, _stateFlag);
        //}

        ///// <summary>
        ///// 刷新
        ///// </summary>
        //private void btnRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    GetData(dataPager.PageIndex, _stateFlag);
        //}

        ///// <summary>
        ///// 参与调查
        ///// </summary>
        //private void btnSurveyDetail_Click(object sender, RoutedEventArgs e)
        //{
        //    if (IsSelect("ShowDetail"))
        //    {
        //        ObservableCollection<V_MyEusurvey> selectEmpSurveyList = GetSelectList();
        //        if (selectEmpSurveyList != null)
        //        {
        //            T_OA_REQUIRE empSurveysInfo = selectEmpSurveyList[0].OARequire;
        //            if (empSurveysInfo != null)
        //            {
        //                //if (empSurveysInfo.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
        //                //{
        //                //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRM"));
        //                //    return;
        //                //}
        //                //DateTime _dateNow = DateTime.Now;
        //                //if (empSurveysInfo.ENDDATE < _dateNow)//如果调查时间过期,则不能参与调查
        //                //{
        //                //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STATICFACTIONISENDYOUCANVISIST"), Utility.GetResourceStr("CONFIRM"));
        //                //    return;
        //                //}
        //               // JoinSurveying joinWindow = new JoinSurveying(FormTypes.New, empSurveysInfo.REQUIREID);

        //                JoinSurveyingForm joinWindow = new JoinSurveyingForm(FormTypes.New, empSurveysInfo.REQUIREID);
        //                EntityBrowser browser = new EntityBrowser(joinWindow);
        //                browser.MinHeight = 510;
        //                browser.MinWidth = 650;
        //                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        //            }
        //        }
        //    }
        //}

        //#endregion

        //#region XAML回调函数

        ///// <summary>
        ///// 加载DataGrid图标
        ///// </summary>
        //private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    SetRowLogo(dgSurveying, e.Row, "T_OA_REQUIRERESULT");
        //}

        ///// <summary>
        ///// 分页
        ///// </summary>
        //private void GridPager_Click(object sender, RoutedEventArgs e)
        //{
        //    GetData(dataPager.PageIndex, _stateFlag);
        //}
        //#endregion

        //#region 其他函数

        ///// <summary>
        ///// 后台事件注册及动画加载
        ///// </summary>
        //private void EventRegister()
        //{
        //    loadbar = new SMTLoading();
        //    PARENT.Children.Add(loadbar);//加载动画
        //    client = new SmtOAPersonOfficeClient();
        //    client.Del_ESurveyAppCompleted += new EventHandler<Del_ESurveyAppCompletedEventArgs>(Del_ESurveyAppCompleted);
        //    client.Get_MyVisistedSurveyCompleted += new EventHandler<Get_MyVisistedSurveyCompletedEventArgs>(Get_MyVisistedSurveyCompleted);
        //    Utility.DisplayGridToolBarButton(ToolBar, "T_OA_REQUIRERESULT", true);
        //    ToolBar.btnNew.Visibility = Visibility.Collapsed;
        //    ToolBar.btnEdit.Visibility = Visibility.Collapsed;
        //    ToolBar.btnDelete.Visibility = Visibility.Collapsed;
        //    ToolBar.btnAudit.Visibility = Visibility.Collapsed;
        //    ToolBar.stpCheckState.Visibility = Visibility.Collapsed;

        //    ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);//查看           
        //    ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新
        //    ImageButton btnShowDetail = new ImageButton();//显示详细按钮
        //    btnShowDetail.TextBlock.Text = Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATION");
        //    btnShowDetail.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_18_126.png", UriKind.Relative));
        //    btnShowDetail.Click += new RoutedEventHandler(btnSurveyDetail_Click);
        //    ToolBar.stpOtherAction.Children.Add(btnShowDetail);     

        //}

        ///// <summary>
        ///// 验证是否选中数据
        ///// </summary>
        //private bool IsSelect(string message)
        //{
        //    if (dgSurveying.SelectedItems == null || dgSurveying.SelectedItems.Count == 0)
        //    {
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", message), Utility.GetResourceStr("CONFIRM"));
        //        return false;
        //    }
        //    return true;
        //}

        ///// <summary>
        ///// 获取DataGrid选中项数据
        ///// </summary>
        ///// <returns></returns>
        //private ObservableCollection<V_MyEusurvey> GetSelectList()
        //{
        //    if (dgSurveying.ItemsSource != null)
        //    {
        //        ObservableCollection<V_MyEusurvey> selectList = new ObservableCollection<V_MyEusurvey>();
        //        foreach (V_MyEusurvey obj in dgSurveying.SelectedItems)
        //            selectList.Add(obj);
        //        if (selectList != null && selectList.Count > 0)
        //        {
        //            return selectList;
        //        }
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// 绑定数据
        ///// </summary>
        //private void BindDateGrid(ObservableCollection<V_MyEusurvey> dataList)
        //{
        //    if (dataList != null && dataList.Count > 0)
        //    {
        //        PagedCollectionView pcv = new PagedCollectionView(dataList);
        //        pcv.PageSize = 20;
        //        dataPager.DataContext = pcv;
        //        dgSurveying.ItemsSource = pcv;
        //    }
        //    else
        //    {
        //        dgSurveying.ItemsSource = null;
        //    }
        //}

        ///// <summary>
        ///// 获取数据
        ///// </summary>
        //private void GetData(int pageIndex, string checkState)
        //{
        //    int pageCount = 0;
        //    string filter = string.Empty;    //查询过滤条件
        //    ObservableCollection<object> paras = null;  //参数值          
        //    if (_isQuery)
        //    {
        //        string _title = string.Empty;
        //        string _content = string.Empty;
        //        string _startTime = string.Empty;
        //        string _endTime = string.Empty;
        //        _startTime = dpStart.Text.ToString();
        //        _endTime = dpEnd.Text.ToString();
        //        DateTime _timeStart = new DateTime();
        //        DateTime _timeEnd = new DateTime();
        //        _title = this.txtSurveysTITLE.Text.ToString().Trim();
        //        _content = this.txtSurveysContent.Text.ToString().Trim();
        //        if (!string.IsNullOrEmpty(_startTime) && string.IsNullOrEmpty(_endTime))
        //        {
        //            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"),
        //            Utility.GetResourceStr("ENDTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"));
        //            return;
        //        }
        //        if (string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
        //        {
        //            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"));
        //            return;
        //        }
        //        if (!string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
        //        {
        //            _timeStart = System.Convert.ToDateTime(_startTime);
        //            _timeEnd = System.Convert.ToDateTime(_endTime + " 23:59:59");
        //            if (_timeStart > _timeEnd)
        //            {
        //                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"));
        //                return;
        //            }
        //            else
        //            {
        //                paras = new ObservableCollection<object>();
        //                if (!string.IsNullOrEmpty(filter))
        //                {
        //                    filter += " and ";
        //                }
        //                filter += "OARequire.STARTDATE >=@" + paras.Count().ToString();//开始时间
        //                paras.Add(_timeStart);
        //                filter += " and ";
        //                filter += "OARequire.STARTDATE <=@" + paras.Count().ToString();//结束时间
        //                paras.Add(_timeEnd);
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(_title))
        //        {
        //            if (!string.IsNullOrEmpty(filter))
        //            {
        //                filter += " and ";
        //            }
        //            filter += "OARequire.APPTITLE ^@" + paras.Count().ToString();//类型名称
        //            paras.Add(_title);
        //        }
        //        if (!string.IsNullOrEmpty(_content))
        //        {
        //            if (!string.IsNullOrEmpty(filter))
        //            {
        //                filter += " and ";
        //            }
        //            filter += "OARequire.CONTENT ^@" + paras.Count().ToString();//类型名称
        //            paras.Add(_content);

        //        }
        //    }
        //    client.Get_MyVisistedSurveyAsync(dataPager.PageIndex, dataPager.PageSize, "OARequire.UPDATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState, Common.CurrentLoginUserInfo.UserPosts[0].PostID, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
        //}

        ///// <summary>
        ///// 调用子窗体后,重新获取数据
        ///// </summary>
        //void browser_ReloadDataEvent()
        //{
        //    GetData(dataPager.PageIndex, _stateFlag);
        //}

        //#endregion      


        private SMTLoading loadbar = new SMTLoading();
        private SmtOAPersonOfficeClient client = null;

        public EmployeeSurveying()
        {
            InitializeComponent();
            client = new SmtOAPersonOfficeClient();

            InitEvent();
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            GetEntityLogo("T_OA_REQUIREDISTRIBUTE");//获取实体LOGO,传递实体名称
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_REQUIREDISTRIBUTE", true);//显示toolbar按钮
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            dpStart.Text = DateTime.Now.AddDays(-7).ToShortDateString();
            dpEnd.Text = DateTime.Now.ToShortDateString();
            client = new SmtOAPersonOfficeClient();
            // 注册事件

            ToolBar.btnNew.Visibility = Visibility.Collapsed;
            ToolBar.btnEdit.Visibility = Visibility.Collapsed;
            ToolBar.btnDelete.Visibility = Visibility.Collapsed;
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.stpCheckState.Visibility = Visibility.Collapsed;

            ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);//查看           
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click); //刷新
            ImageButton btnShowDetail = new ImageButton();//显示详细按钮
            btnShowDetail.TextBlock.Text = Utility.GetResourceStr("INVOLVEDINTHEINVESTIGATION");
            btnShowDetail.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_18_126.png", UriKind.Relative));
            btnShowDetail.Click += new RoutedEventHandler(btnSurveyDetail_Click);
            ToolBar.stpOtherAction.Children.Add(btnShowDetail);     
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", "5");
       
            client.GetRequireDistributeCompleted += new EventHandler<GetRequireDistributeCompletedEventArgs>(client_GetRequireDistributeCompleted);
            QueryData();
        }

        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.QueryData();
        }

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (dgSurveying.SelectedItems.Count == 1)
            {
                T_OA_REQUIREDISTRIBUTE distribute = this.dgSurveying.SelectedItem as T_OA_REQUIREDISTRIBUTE;

                string ID = distribute.T_OA_REQUIRE.REQUIREID;
                JoinSurveyingForm form = new JoinSurveyingForm(FormTypes.Browse, ID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.HideLeftMenu();
                form.actionType = FormTypes.Browse;
                browser.FormType = FormTypes.Browse;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), "警告", "请选择一条要查看的数据");
            }
        }

        void browser_ReloadDataEvent()
        {
           this.QueryData();
        }

        /// <summary>
        /// 参与调查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnSurveyDetail_Click(object sender, RoutedEventArgs e)
        {
            if (dgSurveying.SelectedItems.Count == 1)
            {
                T_OA_REQUIREDISTRIBUTE distribute = this.dgSurveying.SelectedItem as T_OA_REQUIREDISTRIBUTE;

                string ID = distribute.T_OA_REQUIRE.REQUIREID;
                JoinSurveyingForm form = new JoinSurveyingForm(FormTypes.New, ID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.HideLeftMenu();
                form.actionType = FormTypes.New;
                browser.FormType = FormTypes.New;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), "警告", "请选择一条要查看的数据");
            }
        }

        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           this.QueryData();
        }

        void client_GetRequireDistributeCompleted(object sender, GetRequireDistributeCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            ObservableCollection<T_OA_REQUIREDISTRIBUTE> dataList = e.Result;
            if (dataList != null && dataList.Count > 0)
            {
                BindDateGrid(dataList,e.pageCount);
            }
            else
            {
                BindDateGrid(null,0);
            }
        }

        /// <summary>
        /// 绑定数据到DataGrid
        /// </summary>
        private void BindDateGrid(ObservableCollection<T_OA_REQUIREDISTRIBUTE> obj, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (obj == null || obj.Count < 1)
            {
                dgSurveying.ItemsSource = null;
                return;
            }
            dgSurveying.ItemsSource = obj;
        }

        /// <summary>
        /// 组织查询条件，查询数据
        /// </summary>
        private void QueryData()
        {
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new ObservableCollection<object>();   //参数值            

            // 开始时间
            if (!string.IsNullOrEmpty(this.dpStart.Text.Trim()))
            {
                dtStart = Convert.ToDateTime(this.dpStart.Text);
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_REQUIRE.STARTDATE >= @" + paras.Count();
                paras.Add(dtStart);
            }
            // 结束时间
            if (!string.IsNullOrEmpty(this.dpEnd.Text.Trim()))
            {
                dtEnd = Convert.ToDateTime(this.dpEnd.Text);
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "T_OA_REQUIRE.ENDDATE <= @" + paras.Count();
                paras.Add(dtEnd);
            }

            if (!string.IsNullOrEmpty(this.dpStart.Text.Trim()) && !string.IsNullOrEmpty(this.dpEnd.Text.Trim()))
            {
                if (dtStart > dtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), "警告", "开始时间不能大于结束时间",MessageIcon.None);
                    return;
                }
            }
            if (!string.IsNullOrEmpty(this.txtSurveysTITLE.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += " @" + paras.Count() + ".Contains(DISTRIBUTETITLE)";
                paras.Add(this.txtSurveysTITLE.Text.Trim());
            }

            // 审核状态
            T_SYS_DICTIONARY audit = this.ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            string checkState = "5";
            if (audit!= null)
            {
                checkState = audit.DICTIONARYVALUE.ToString();
            }

            loadbar.Start();
            client.GetRequireDistributeAsync(dataPager.PageIndex, dataPager.PageSize, "T_OA_REQUIRE.STARTDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            this.QueryData();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.QueryData();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}