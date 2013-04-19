//----------------------------------------------------
// 文件名：EmployeeSurveysApp.xaml.cs
//作  用：员工调查申请管理页面
//修改人：勒中玉
//修改时间：2011-8-14
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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class EmployeeSurveysDistribute : BasePage
    {
        #region 全局变量定义

        private SMTLoading loadbar = null;//动画加载
        private SmtOAPersonOfficeClient client = null;
        private bool _isQuery = false;
        private string _checkState =string.Empty;
        private T_OA_REQUIREDISTRIBUTE requiredistributeInfo = null;//发布申请
        #endregion

        #region 构造函数

        public EmployeeSurveysDistribute()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(EmployeeSurveysDistribute_Loaded);
         }

        #endregion

        #region 后台事件回调函数

        void EmployeeSurveysDistribute_Loaded(object sender, RoutedEventArgs e)
        {
            EventRegister();
            _checkState = Convert.ToInt32(CheckStates.ALL).ToString();
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            GetEntityLogo("T_OA_RequireDistribute");
            GetData();
        }

        /// <summary>
        /// 异步获取数据后绑定
        /// </summary>
        private void Get_ESurveyResultsCompleted(object sender, Get_ESurveyResultsCompletedEventArgs e)
        {
            loadbar.Stop();
            ObservableCollection<T_OA_REQUIREDISTRIBUTE> dataList = e.Result;
            if (dataList != null && dataList.Count() > 0)
            {
                BindDateGrid(dataList);
            }
            else
            {
                BindDateGrid(null);
            }
        }

        /// <summary>
        /// 异步删除后,提示并再次获取数据
        /// </summary>
        private void Del_ESurveyResultCompleted(object sender, Del_ESurveyResultCompletedEventArgs e)
        {
            loadbar.Stop();
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            if (e.Result > 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EmployeeSurveyApp"), Utility.GetResourceStr("CONFIRM"));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            GetData();
        }

        /// <summary>
        /// 新增
        /// </summary>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            EmployeeSurveyDistributeChildWindow form = new EmployeeSurveyDistributeChildWindow(FormTypes.New,"");
            EntityBrowser browser = new EntityBrowser(form);
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
                ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectItems = GetSelectList();
                if (selectItems != null && selectItems.Count() > 0)
                {
                    requiredistributeInfo = selectItems[0];
                    if (requiredistributeInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || requiredistributeInfo.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                    {
                        EmployeeSurveyDistributeChildWindow form = new EmployeeSurveyDistributeChildWindow(FormTypes.Edit, selectItems.FirstOrDefault().REQUIREDISTRIBUTEID);
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
                ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectItems = GetSelectList();
                if (selectItems != null)
                {
                    for (int i = 0; i < dgDistribute.SelectedItems.Count; i++)
                    {
                        requiredistributeInfo = selectItems[i];
                        if (requiredistributeInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            string _result = string.Empty;
                            ComfirmWindow com = new ComfirmWindow();
                            com.OnSelectionBoxClosed += (obj, result) =>
                            {
                                try
                                {
                                    loadbar.Start();
                                    client.Del_ESurveyResultAsync(selectItems);
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
        /// 查看
        /// </summary>
        void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("VIEW"))
            {
                ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectItems = GetSelectList();
                if (selectItems != null&&selectItems.Count()>0)
                {
                    EmployeeSurveyDistributeChildWindow form = new EmployeeSurveyDistributeChildWindow(FormTypes.Browse, selectItems.FirstOrDefault().REQUIREDISTRIBUTEID);
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
        /// 审核
        /// </summary>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("AUDIT"))
            {
                T_OA_REQUIREDISTRIBUTE ent = dgDistribute.SelectedItems[0] as T_OA_REQUIREDISTRIBUTE;
                if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    EmployeeSurveyDistributeChildWindow form = new EmployeeSurveyDistributeChildWindow(FormTypes.Audit, ent.REQUIREDISTRIBUTEID);
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
        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelect("ReSubmit"))
            {
                ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectItems = GetSelectList();
                if (selectItems != null&&selectItems.Count()>0)
                {
                    EmployeeSurveyDistributeChildWindow form = new EmployeeSurveyDistributeChildWindow(FormTypes.Resubmit, selectItems.FirstOrDefault().REQUIREDISTRIBUTEID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Resubmit;
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
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_REQUIREDISTRIBUTE");
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
        /// 设置DataGrid上按钮图标
        /// </summary>
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgDistribute, e.Row, "T_OA_RequireDistribute");
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
            client.Del_ESurveyResultCompleted += new EventHandler<Del_ESurveyResultCompletedEventArgs>(Del_ESurveyResultCompleted);
            client.Get_ESurveyResultsCompleted += new EventHandler<Get_ESurveyResultsCompletedEventArgs>(Get_ESurveyResultsCompleted);

            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_REQUIREDISTRIBUTE", true);

            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);

            ImageButton btnShowResult = new ImageButton();//显示结果按钮
            btnShowResult.TextBlock.Text = Utility.GetResourceStr("SHOWRESULTS");
            btnShowResult.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_18_5.png", UriKind.Relative));
            btnShowResult.Click += new RoutedEventHandler(btnShowResult_Click);
            ToolBar.stpOtherAction.Children.Add(btnShowResult);

            ImageButton btn = new ImageButton();//查看详细按钮
            btn.TextBlock.Text = Utility.GetResourceStr("VIEWDETAILS");
            btn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1071_d.png", UriKind.Relative));
            btn.Click += new RoutedEventHandler(btnShowDetail_Click);
            ToolBar.stpOtherAction.Children.Add(btn);

            //ImageButton IssueDocumentBtn = new ImageButton();// 发布调查结果
            //IssueDocumentBtn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_crmemail.png", UriKind.Relative));
            //IssueDocumentBtn.TextBlock.Text = Utility.GetResourceStr("ISSUEDOCUMENT");
            //IssueDocumentBtn.Image.Width = 16.0;
            //IssueDocumentBtn.Image.Height = 22.0;
            //IssueDocumentBtn.TextBlock.Margin = new Thickness(1, 0, 0, 0);
            //IssueDocumentBtn.Style = (Style)Application.Current.Resources["ButtonStyle"];
            //IssueDocumentBtn.Click += new RoutedEventHandler(IssueBtn_Click);
            //ToolBar.stpOtherAction.Children.Add(IssueDocumentBtn);

            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", _checkState);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);

        }

        /// <summary>
        /// 验证是否选中数据
        /// </summary>
        private bool IsSelect(string message)
        {
            if (dgDistribute.SelectedItems == null || dgDistribute.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", message), Utility.GetResourceStr("CONFIRMBUTTON"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="_checkState"></param>
        private void GetData()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = null;
            if (_isQuery)
            {
                string _title = string.Empty;
                string _content = string.Empty;
                string _startTime = dpStart.Text.ToString();
                string _endTime = dpEnd.Text.ToString();
                DateTime _timeStart = new DateTime();
                DateTime _timeEnd = new DateTime();
                _title = this.txtSurveysTITLE.Text.ToString().Trim();
                _content = this.txtSurveysContent.Text.ToString().Trim();
                if (!string.IsNullOrEmpty(_startTime) && string.IsNullOrEmpty(_endTime))
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"),
                    Utility.GetResourceStr("ENDTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"));
                    return;
                }
                if (string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTNULL"), Utility.GetResourceStr("CONFIRM"));
                    return;
                }
                if (!string.IsNullOrEmpty(_startTime) && !string.IsNullOrEmpty(_endTime))
                {
                    _timeStart = System.Convert.ToDateTime(_startTime);
                    _timeEnd = System.Convert.ToDateTime(_endTime + " 23:59:59");
                    if (_timeStart > _timeEnd)
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"), Utility.GetResourceStr("CONFIRM"));
                        return;
                    }
                    else
                    {
                        paras = new ObservableCollection<object>();
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "CREATEDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(_timeStart);
                        filter += " and ";
                        filter += "CREATEDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(_timeEnd);
                    }
                }

                if (!string.IsNullOrEmpty(_title))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "DISTRIBUTETITLE ^@" + paras.Count().ToString();//类型名称
                    paras.Add(_title);
                }
                if (!string.IsNullOrEmpty(_content))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CONTENT ^@" + paras.Count().ToString();//类型名称
                    paras.Add(_content);

                }
            }
            loadbar.Start();
            client.Get_ESurveyResultsAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, _checkState);
        }

        /// <summary>
        /// 获取选中项数据
        /// </summary>
        private ObservableCollection<T_OA_REQUIREDISTRIBUTE> GetSelectList()
        {
            if (dgDistribute.ItemsSource != null)
            {
                ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectList = new ObservableCollection<T_OA_REQUIREDISTRIBUTE>();
                foreach (T_OA_REQUIREDISTRIBUTE obj in dgDistribute.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }

        /// <summary>
        /// 加载子页面后执行
        /// </summary>
        void browser_ReloadDataEvent()
        {
            GetData();
        }

        /// <summary>
        /// 绑定数据到DataGrid
        /// </summary>
        private void BindDateGrid(ObservableCollection<T_OA_REQUIREDISTRIBUTE> dataList)
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dgDistribute.ItemsSource = pcv;
            }
            else
            {
                dgDistribute.ItemsSource = null;
            }
        }

        #endregion

        //#region 发布调查结果
        //private void IssueBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectEmpSurveyList = GetSelectList();
        //    if (selectEmpSurveyList != null)
        //    {
        //        requiredistributeInfo = selectEmpSurveyList.FirstOrDefault();
        //        T_OA_REQUIREDISTRIBUTE empSurveysInfo = selectEmpSurveyList[0];
        //        if (empSurveysInfo != null)
        //        {
        //            if (empSurveysInfo.CHECKSTATE == "0")
        //            {
        //                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //                return;
        //            }

        //            //发布结果
        //            EmployeeSubmissions_1 frmEmpSurveysSubmit = new EmployeeSubmissions_1();//empSurveysInfo
        //            frmEmpSurveysSubmit.Require = empSurveysInfo;
        //            EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
        //            browser.MinHeight = 550;
        //            browser.MinWidth = 750;
        //            browser.FormType = FormTypes.Browse;
        //            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
        //            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        //        }
        //    }
        //    else
        //    {
        //        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SelectSurveys"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //    }
        //}
        //#endregion

       
        
       

     
        
      
       

      
       
     


       







        
        //显示结果
        private void btnShowResult_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectEmpSurveyList = GetSelectList();
            if (selectEmpSurveyList != null)
            {
                requiredistributeInfo = selectEmpSurveyList.FirstOrDefault();
                T_OA_REQUIREDISTRIBUTE empSurveysInfo = selectEmpSurveyList[0];
                if (empSurveysInfo != null)
                {
                    if (empSurveysInfo.CHECKSTATE == "0")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    EmployeeSubmissions_1 frmEmpSurveysSubmit = new EmployeeSubmissions_1();
                    frmEmpSurveysSubmit.Require = empSurveysInfo;
                    EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                    browser.FormType = FormTypes.Browse;
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SelectSurveys"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        //显示明细结果
        private void btnShowDetail_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_REQUIREDISTRIBUTE> selectEmpSurveyList = GetSelectList();
            if (selectEmpSurveyList != null)
            {
                T_OA_REQUIREDISTRIBUTE empSurveysInfo = selectEmpSurveyList[0];
                if (empSurveysInfo != null)
                {
                    if (empSurveysInfo.CHECKSTATE == "0")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    EmployeeList frmEmpSurveysSubmit = new EmployeeList();//empSurveysInfo
                    frmEmpSurveysSubmit.Require = empSurveysInfo;
                    EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                    browser.MinHeight = 510;
                    browser.MinWidth = 650;
                    browser.FormType = FormTypes.Browse;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SelectSurveys"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        
     

      
    }
}