//----------------------------------------------------
// 文件名：EmployeeSurveysApp.xaml.cs
//作  用：员工调查申请管理页面
//修改人：勒中玉
//修改时间：2011-8-5
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
    public partial class EmployeeSurveysApp : BasePage
    {

        #region 全局变量定义

        SMTLoading loadbar = null;//加载动画
        SmtOAPersonOfficeClient client = null;
        T_OA_REQUIRE selApporvalInfo = null;
        bool _isQuery = false;
        string _checkState = string.Empty;

        #endregion

        #region 构造函数

        public EmployeeSurveysApp()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSurveysApp_Loaded);
        }

        #endregion

        #region 后台事件回调函数

        private void EmployeeSurveysApp_Loaded(object sender, RoutedEventArgs e)
        {
            _checkState = ((int)CheckStates.ALL).ToString();
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            EventRegister();
            GetData();
            GetEntityLogo("T_OA_REQUIRE");//获得实体LOGO
        }

        /// <summary>
        /// 删除后
        /// </summary>
        private void Del_ESurveyAppCompleted(object sender, Del_ESurveyAppCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result > 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED","EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            GetData();
        }

        /// <summary>
        /// 获取数据后
        /// </summary>
        private void Get_ESurveyAppsCompleted(object sender, Get_ESurveyAppsCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("GETDATAFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            //if (e.Result == null)
            //{
            //    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DIDNOTFINDRELEVANT", "EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"));
            //    return;
            //}
            ObservableCollection<T_OA_REQUIRE> dataList = e.Result;
            _isQuery = false;
            if (dataList != null)
            {
                BindDateGrid(dataList);
            }
            else
            {
                BindDateGrid(null);
            }
        }

        /// <summary>
        /// 新建
        /// </summary>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EmployeesSurveyAppChildWindow form = new EmployeesSurveyAppChildWindow(FormTypes.New, "");
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
                ObservableCollection<T_OA_REQUIRE> selectItems = GetSelectList();
                if (selectItems != null)
                {
                    selApporvalInfo = selectItems[0];
                    if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                    {
                        EmployeesSurveyAppChildWindow form = new EmployeesSurveyAppChildWindow(FormTypes.Edit, selApporvalInfo.REQUIREID);
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
        }

        /// <summary>
        /// 删除
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("DELETE"))
            {
                ObservableCollection<T_OA_REQUIRE> selectItems = GetSelectList();
                if (selectItems != null)
                {
                    for (int i = 0; i < dgEmployeeApp.SelectedItems.Count; i++)
                    {
                        selApporvalInfo = selectItems[i];
                        if (selApporvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            string _result = string.Empty;
                            ComfirmWindow com = new ComfirmWindow();
                            com.OnSelectionBoxClosed += (obj, result) =>
                            {
                                try
                                {
                                    loadbar.Start();
                                    client.Del_ESurveyAppAsync(selectItems);
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
                T_OA_REQUIRE ent = dgEmployeeApp.SelectedItems[0] as T_OA_REQUIRE;
                if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    EmployeesSurveyAppChildWindow form = new EmployeesSurveyAppChildWindow(FormTypes.Audit, ent.REQUIREID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }

        /// <summary>
        /// 重新提交
        /// </summary>
        private void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("ReSubmit"))
            {
                ObservableCollection<T_OA_REQUIRE> selectItems = GetSelectList();
                if (selectItems != null && selectItems.Count > 0)
                {
                    EmployeesSurveyAppChildWindow form = new EmployeesSurveyAppChildWindow(FormTypes.Resubmit, selApporvalInfo.REQUIREID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
                    browser.MinWidth = 510;
                    browser.MinHeight = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
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
                ObservableCollection<T_OA_REQUIRE> selectItems = GetSelectList();
                if (selectItems != null)
                {
                    selApporvalInfo = selectItems[0];
                    EmployeesSurveyAppChildWindow form = new EmployeesSurveyAppChildWindow(FormTypes.Browse, selApporvalInfo.REQUIREID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Browse;
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetData();
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
            GetData();
        }

        /// <summary>
        /// 根据审核状态加载TooBar及显示相应审核状态的数据
        /// </summary>
        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_REQUIRE");//根据审核状态控制ToolBar
                _checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }

        /// <summary>
        /// 翻页
        /// </summary>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }
        #endregion

        #region XAML回调函数

        /// <summary>
        /// 加载DataGrid对应LOGO
        /// </summary>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmployeeApp, e.Row, "T_OA_REQUIRE");
        }

        #endregion

        #region 其他函数

        /// <summary>
        /// 后台事件注册及动画加载
        /// </summary>
        private void EventRegister()
        {
            loadbar = new SMTLoading();
            PARENT.Children.Add(loadbar);
            client = new SmtOAPersonOfficeClient();
            client.Del_ESurveyAppCompleted += new EventHandler<Del_ESurveyAppCompletedEventArgs>(Del_ESurveyAppCompleted);
            client.Get_ESurveyAppsCompleted += new EventHandler<Get_ESurveyAppsCompletedEventArgs>(Get_ESurveyAppsCompleted);

            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_REQUIRE", true);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", _checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

        }

        /// <summary>
        /// 验证是否选中数据
        /// </summary>
        private bool IsSelect(string message)
        {
            if (dgEmployeeApp.SelectedItems == null || dgEmployeeApp.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", message), Utility.GetResourceStr("CONFIRMBUTTON"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        private void GetData()
        {
            int pageCount = 0;
            string filter = string.Empty;    //查询过滤条件
            ObservableCollection<object> paras = new ObservableCollection<object>();
            if (_isQuery)
            {
                DateTime dtStart = new DateTime();
                DateTime dtEnd = new DateTime();
              
                // 开始时间
                if (!string.IsNullOrEmpty(this.dpStart.Text.Trim()))
                {
                    dtStart = Convert.ToDateTime(this.dpStart.Text);
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "STARTDATE  >= @" + paras.Count();
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
                    filter += "STARTDATE <= @" + paras.Count();
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
                    filter += " @" + paras.Count().ToString() + ".Contains(APPTITLE)";
                    paras.Add(this.txtSurveysTITLE.Text.Trim());
                }

                if (!string.IsNullOrEmpty(this.txtSurveysContent.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += " @" + paras.Count().ToString() + ".Contains(CONTENT)";
                    paras.Add(this.txtSurveysContent.Text.Trim());
                }
            }
            loadbar.Start();
            client.Get_ESurveyAppsAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, _checkState);
        }

        /// <summary>
        /// 绑定到DataGrid
        /// </summary>
        private void BindDateGrid(ObservableCollection<T_OA_REQUIRE> dataList)
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dgEmployeeApp.ItemsSource = pcv;
            }
            else
            {
                dgEmployeeApp.ItemsSource = null;
            }
        }

        /// <summary>
        /// 获取选中项数据
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<T_OA_REQUIRE> GetSelectList()
        {
            if (dgEmployeeApp.ItemsSource != null)
            {
                ObservableCollection<T_OA_REQUIRE> selectList = new ObservableCollection<T_OA_REQUIRE>();
                foreach (T_OA_REQUIRE obj in dgEmployeeApp.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }

        /// <summary>
        /// 加载子页面后执行
        /// </summary>
        private void browser_ReloadDataEvent()
        {
            GetData();
        }

        #endregion
    }
}