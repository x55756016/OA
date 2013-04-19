//----------------------------------------------------
// 文件名：EmployeeSurveysMaster.xaml.cs
//作  用：员工调查方案管理页面
//修改人：勒中玉
//修改时间：2011-8-4
//修改内容：规范化变量命名及添加注释等
//---------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class EmployeeSurveysMaster : BasePage
    {

        #region 全局变量定义

        private SMTLoading loadbar = null;
        private V_EmployeeSurvey selApporvalInfo = null;
        private SmtOAPersonOfficeClient client = null;
        private bool _isQuery = false;
        private string checkState = string.Empty;

        #endregion

        #region 构造函数

        public EmployeeSurveysMaster()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSurveysMaster_Loaded);
        }

        #endregion

        #region 后台事件回调函数

        /// <summary>
        /// 载入时事件
        /// </summary>
        private void EmployeeSurveysMaster_Loaded(object sender, RoutedEventArgs e)
        {
            checkState = ((int)CheckStates.ALL).ToString();
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            EventRegister();//注册、初始化
            selApporvalInfo = new V_EmployeeSurvey();
            QueryData();
            GetEntityLogo("T_OA_REQUIREMASTER");//获取实体LOGO,传递实体名称
        }

        /// <summary>
        /// 新建
        /// </summary>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EmployeeSurveyChildWindow form = new EmployeeSurveyChildWindow(FormTypes.New,"");
            EntityBrowser browser = new EntityBrowser(form);
            browser.FormType = FormTypes.New;
            browser.MinHeight = 510;
            browser.MinWidth = 650;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        /// <summary>
        /// 修改
        /// </summary>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("EDIT"))
            {
                ObservableCollection<V_EmployeeSurvey> selectItems = GetSelectList();
                selApporvalInfo = selectItems[0];
                if (selApporvalInfo.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || selApporvalInfo.RequireMaster.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                {
                    EmployeeSurveyChildWindow form = new EmployeeSurveyChildWindow(FormTypes.Edit, selApporvalInfo.RequireMaster.REQUIREMASTERID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if(IsSelect("DELETE"))
            {
            ObservableCollection<V_EmployeeSurvey> selectItems = GetSelectList();
            if (selectItems != null)
            {
                for (int i = 0; i < dgEmployeeSurvey.SelectedItems.Count; i++)
                {
                    selApporvalInfo = selectItems[i];
                    if (selApporvalInfo.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string _result = string.Empty;
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try 
                            {
                                loadbar.Start();
                                client.DeleteEmployeeSurveyViewListAsync(selectItems); 
                            }
                            catch (Exception ex)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), ex.Message, Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                            }
                        };
                        com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, _result);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(
                            Utility.GetResourceStr("ERROR"),
                            Utility.GetResourceStr("NOTONLYDIDNOSUBMITANDREVIEWTHEDATACANBEDELETEDBY"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
            }
            }
        }
            
        /// <summary>
        /// 审核
        /// </summary>
        private void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("AUDIT"))
            {
                V_EmployeeSurvey ent = dgEmployeeSurvey.SelectedItems[0] as V_EmployeeSurvey;
                if (ent.RequireMaster.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                    ent.RequireMaster.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                    ent.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    EmployeeSurveyChildWindow form = new EmployeeSurveyChildWindow(FormTypes.Audit, ent.RequireMaster.REQUIREMASTERID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }

        /// <summary>
        /// 查看
        /// </summary>
        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("VIEW"))
            {
                ObservableCollection<V_EmployeeSurvey> selectItems = GetSelectList();
                selApporvalInfo = selectItems[0];
                EmployeeSurveyChildWindow form = new EmployeeSurveyChildWindow(FormTypes.Browse, selApporvalInfo.RequireMaster.REQUIREMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 510;
                browser.MinWidth = 650;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        private void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("ReSubmit"))
            {
                ObservableCollection<V_EmployeeSurvey> selectItems = GetSelectList();
                selApporvalInfo = selectItems[0];
                EmployeeSurveyChildWindow form = new EmployeeSurveyChildWindow(FormTypes.Resubmit, selApporvalInfo.RequireMaster.REQUIREMASTERID);
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinWidth = 510;
                browser.MinHeight = 650;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            QueryData();
        }

        /// <summary>
        /// 清空
        /// </summary>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.txtSurveysContent.Text = string.Empty;
            this.txtSurveysTITLE.Text = string.Empty;
            this.dpStart.SelectedDate = null;
            this.dpEnd.SelectedDate = null;
        }

        /// <summary>
        /// 查询
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _isQuery = true;
            QueryData();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        private void GetEmployeeSurveyViewListCompleted(object sender, GetEmployeeSurveyViewListCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            ObservableCollection<V_EmployeeSurvey> dataList = e.Result;
            _isQuery = false;
            if (dataList != null && dataList.Count > 0)
            {
                BindDateGrid(dataList);
            }
            else
            {
                BindDateGrid(null);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void DeleteEmployeeSurveyViewListCompleted(object sender, DeleteEmployeeSurveyViewListCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result > 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EmployeeSurvey"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            QueryData();
        }


        /// <summary>
        /// 根据审核状态加载TooBar及显示相应审核状态的数据
        /// </summary>
        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_REQUIREMASTER");
                checkState = dict.DICTIONARYVALUE.ToString();
                QueryData();
            }
        }

        #endregion

        #region XAML回调函数

        /// <summary>
        /// DataGrid获取ToolBar按钮图片
        /// </summary>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmployeeSurvey, e.Row, "T_OA_REQUIREMASTER");
        }

        /// <summary>
        /// 翻页
        /// </summary>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            QueryData();
        }

        #endregion

        #region 其他函数

        /// <summary>
        /// 后台事件注册及动画加载
        /// </summary>
        private void EventRegister()
        {
            loadbar = new SMTLoading();
            PARENT.Children.Add(loadbar);//加载动画

            client = new SmtOAPersonOfficeClient();
            client.DeleteEmployeeSurveyViewListCompleted += new EventHandler<DeleteEmployeeSurveyViewListCompletedEventArgs>(DeleteEmployeeSurveyViewListCompleted);
            client.GetEmployeeSurveyViewListCompleted += new EventHandler<GetEmployeeSurveyViewListCompletedEventArgs>(GetEmployeeSurveyViewListCompleted);

            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_REQUIREMASTER", true);//显示toolbar按钮
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);//新增按钮事件注册
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);//修改按钮
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);//删除按钮
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);//审核按钮
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);//重新提交按钮
            ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);//查看按钮
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);//刷新按钮
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);//
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);//checkbox选择事件注册
        }

        /// <summary>
        /// 验证是否选中数据
        /// </summary>
        private bool IsSelect(string message)
        {
            if (dgEmployeeSurvey.SelectedItems == null || dgEmployeeSurvey.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", message), Utility.GetResourceStr("CONFIRMBUTTON"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 绑定数据到DataGrid
        /// </summary>
        private void BindDateGrid(ObservableCollection<V_EmployeeSurvey> dataList)
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dgEmployeeSurvey.ItemsSource = pcv;
            }
        }

        /// <summary>
        /// 获取数据，并分页排序
        /// </summary>
        //private void GetData()
        //{
        //    int pageCount = 0;
        //    string filter = "";    //查询过滤条件
        //    ObservableCollection<object> paras = null;//参数值 
        //    if (_isQuery)
        //    {
        //        string _title = string.Empty;
        //        string _content = string.Empty;
        //        string _startTime = string.Empty;
        //        string _endTime = string.Empty;
        //        _startTime = dpStart.Text;
        //        _endTime = dpEnd.Text;
        //        DateTime _timeStart = new DateTime();
        //        DateTime _timeEnd = new DateTime();
        //        _title = this.txtSurveysTITLE.Text.ToString().Trim();
        //        _content = this.txtSurveysContent.Text.ToString().Trim();

        //        if (!string.IsNullOrEmpty(_startTime) && string.IsNullOrEmpty(_endTime))
        //        {
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"),
        //             Utility.GetResourceStr("ENDTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //            return;
        //        }

        //        if (string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
        //        {
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //            return;
        //        }

        //        if (!string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
        //        {
        //            _timeStart = System.Convert.ToDateTime(_startTime);
        //            _timeEnd = System.Convert.ToDateTime(_endTime);
        //            if (_timeStart > _timeEnd)
        //            {
        //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
        //                return;
        //            }
        //            else
        //            {
        //                paras = new ObservableCollection<object>();
        //                if (!string.IsNullOrEmpty(filter))
        //                {
        //                    filter += " and ";
        //                }
        //                filter += "RequireMaster.CREATEDATE >=@" + paras.Count();//开始时间
        //                paras.Add(_timeStart);
        //                filter += " and ";
        //                filter += "RequireMaster.CREATEDATE <=@" + paras.Count();//结束时间
        //                paras.Add(_endTime);
        //            }
        //        }

        //        if (!string.IsNullOrEmpty(_title))
        //        {
        //            if (!string.IsNullOrEmpty(filter))
        //            {
        //                filter += " and ";
        //            }
        //            filter += "RequireMaster.REQUIRETITLE ^@" + paras.Count().ToString();//类型名称
        //            paras.Add(_title);
        //        }
        //        if (!string.IsNullOrEmpty(_content))
        //        {
        //            if (!string.IsNullOrEmpty(filter))
        //            {
        //                filter += " and ";
        //            }
        //            filter += "RequireMaster.CONTENT ^@" + paras.Count().ToString();//类型名称
        //            paras.Add(_content);
        //        }

        //    }
        //    loadbar.Start();
        //    client.GetEmployeeSurveyViewListAsync(dataPager.PageIndex, dataPager.PageSize, "RequireMaster.UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        //}

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
                filter += "RequireMaster.CREATEDATE  >= @" + paras.Count();
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
                filter += "RequireMaster.CREATEDATE <= @" + paras.Count();
                paras.Add(dtEnd);
            }
            if (!string.IsNullOrEmpty(this.dpStart.Text.Trim()) && !string.IsNullOrEmpty(this.dpEnd.Text.Trim()))
            {
                if (dtStart > dtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(this.txtSurveysTITLE.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "RequireMaster.REQUIRETITLE ^@" + paras.Count();//类型名称
                paras.Add(this.txtSurveysTITLE.Text.Trim());
            }

            if (!string.IsNullOrEmpty(this.txtSurveysContent.Text.Trim()))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "RequireMaster.CONTENT ^@" + paras.Count();//类型名称
                paras.Add(this.txtSurveysContent.Text.Trim());
            }

           loadbar.Start();
           client.GetEmployeeSurveyViewListAsync(dataPager.PageIndex, dataPager.PageSize, "RequireMaster.UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
      
        }

        /// <summary>
        /// 加载子页面后执行
        /// </summary>
        private void browser_ReloadDataEvent()
        {
            QueryData();
        }

        /// <summary>
        /// 获取选中项数据
        /// </summary>
        private ObservableCollection<V_EmployeeSurvey> GetSelectList()
        {
            if (dgEmployeeSurvey.ItemsSource != null)
            {
                ObservableCollection<V_EmployeeSurvey> selectList = new ObservableCollection<V_EmployeeSurvey>();
                foreach (V_EmployeeSurvey obj in dgEmployeeSurvey.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }

        #endregion
    }
}