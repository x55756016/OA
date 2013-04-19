//********满意度调查发布管理页******
//修改人：lezy
//修改时间：2011-6-1
//完成时间：2011-6-30
//*********************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.FrameworkUI;
using System.Windows.Data;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Media.Imaging;

namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class SatisfactionDistribute : BasePage
    {
        #region 全局变量定义
        private SMTLoading loadbar = null;
        private SmtOAPersonOfficeClient client=null;
        private ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> deletedList = new ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE>();//标记被删除的对象
        private string checkState = ((int)CheckStates.ALL).ToString();
        private bool IsQuery = false;
        private T_OA_SATISFACTIONDISTRIBUTE satisfactiondistribute = null;
        #endregion

        #region  构造函数
        public SatisfactionDistribute()
        {
            InitializeComponent();
            EventRegister();
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            ImageButton btnShowResult = new ImageButton();
            btnShowResult.TextBlock.Text = "显示结果";
            btnShowResult.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_18_5.png", UriKind.Relative));
            btnShowResult.Click += new RoutedEventHandler(btnShowResult_Click);
            ToolBar.stpOtherAction.Children.Add(btnShowResult);

            ImageButton btn = new ImageButton();
            btn.TextBlock.Text = "查看详情";
            btn.Image.Source = new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1071_d.png", UriKind.Relative));
            btn.Click += new RoutedEventHandler(btnShowDetail_Click);
            ToolBar.stpOtherAction.Children.Add(btn);

        }
        #endregion

        #region 事件注册
        private void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            this.Loaded += new RoutedEventHandler(SatisfactionDistribute_Loaded);
            client.Del_SSurveyResultCompleted += new EventHandler<Del_SSurveyResultCompletedEventArgs>(Del_SSurveyResultCompleted);
            client.Get_SSurveyResultsCompleted += new EventHandler<Get_SSurveyResultsCompletedEventArgs>(Get_SSurveyResultsCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            ToolBar.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }
        #endregion

        #region 事件注册处理程序
        private void SatisfactionDistribute_Loaded(object sender, RoutedEventArgs e)//页面载入
        {
            loadbar = new SMTLoading();
            PARENT.Children.Add(loadbar);
            GetData();
            GetEntityLogo("T_OA_SATISFACTIONDISTRIBUTE");
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_SATISFACTIONDISTRIBUTE", true);
        }
        private void Del_SSurveyResultCompleted(object sender, Del_SSurveyResultCompletedEventArgs e)//WCF删除
        {
            if (e.Result > 0)
            {
                deletedList.Clear();
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
        }
        private void Get_SSurveyResultsCompleted(object sender, Get_SSurveyResultsCompletedEventArgs e)//WCF取数据
        {
            loadbar.Stop();
            IsQuery = false;
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> o = e.Result;
            if (o != null)
            {
                List<T_OA_SATISFACTIONDISTRIBUTE> dataList = o.ToList();
                BindDateGrid(dataList);
            }
            else
                BindDateGrid(null);
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)//新增
        {
            SatisfactionDistribute_add form = new SatisfactionDistribute_add(FormTypes.New);
            EntityBrowser browser = new EntityBrowser(form);
            browser.MinHeight = 500;
            browser.MinWidth = 700;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)//修改
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                T_OA_SATISFACTIONDISTRIBUTE ent = dg.SelectedItems[0] as T_OA_SATISFACTIONDISTRIBUTE;
                if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                {
                    SatisfactionDistribute_upd form = new SatisfactionDistribute_upd(FormTypes.Edit);
                    form._Survey = selectItems[0];
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.MinHeight = 500;
                    browser.MinWidth = 700;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"));
                    return;
                }
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)//删除
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    T_OA_SATISFACTIONDISTRIBUTE ent = dg.SelectedItems[i] as T_OA_SATISFACTIONDISTRIBUTE;
                    if (ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try { client.Del_SSurveyResultAsync(selectItems); }
                            catch { }
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
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        private void btnAudit_Click(object sender, RoutedEventArgs e)//审核
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                T_OA_SATISFACTIONDISTRIBUTE ent = dg.SelectedItems[0] as T_OA_SATISFACTIONDISTRIBUTE;
                if (ent.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                    ent.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    SatisfactionDistribute_aud form = new SatisfactionDistribute_aud(FormTypes.Audit, ent.SATISFACTIONDISTRIBUTEID);
                    form._Survey = selectItems[0];
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 550;
                    browser.MinWidth = 700;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDNOTOPERATEPLEASEAGAIN"));
                    return;
                }
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        private void btnReSubmit_Click(object sender, RoutedEventArgs e)//重新提交
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                SatisfactionDistribute_upd form = new SatisfactionDistribute_upd(FormTypes.Resubmit);
                form._Survey = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Resubmit;
                browser.MinHeight = 500;
                browser.MinWidth = 700;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        private void BtnView_Click(object sender, RoutedEventArgs e)//查看
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectItems = GetSelectList();
            if (selectItems != null)
            {
                satisfactiondistribute = selectItems.FirstOrDefault();
                SatisfactionDistribute_aud form = new SatisfactionDistribute_aud(FormTypes.Browse, satisfactiondistribute.SATISFACTIONDISTRIBUTEID);
                form._Survey = selectItems[0];
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 550;
                browser.MinWidth = 700;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)//刷新
        {
            GetData();
        }
        private void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)//审核状态过滤
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                checkState = Utility.GetCbxSelectItemValue(ToolBar.cbxCheckState);
                SMT.SaaS.FrameworkUI.Common.Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_OA_SATISFACTIONMASTER");
                checkState = dict.DICTIONARYVALUE.ToString();
                GetData();
            }
        }
        #endregion

        #region  XAML事件处理程序
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)//行加载
        {
            SetRowLogo(dg, e.Row, "T_OA_SATISFACTIONDISTRIBUTE");
        }
        private void GridPager_Click(object sender, RoutedEventArgs e)//翻页
        {
            GetData();
        }
        private void SearchBtn_Click(object sender, RoutedEventArgs e)//查询
        {
            IsQuery = true;
            GetData();
        }
        private void BtnReset_Click(object sender, RoutedEventArgs e)//清空
        {
            this.txtSurveysContent.Text = string.Empty;
            this.txtSurveysTITLE.Text = string.Empty;
            this.dpStart.SelectedDate = null;
            this.dpEnd.SelectedDate = null;
        }
        #endregion

        #region 其他函数
        private void GetData()//主页面获取数据
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值          
            if (IsQuery)
            {
                string StrTitle = "";
                string StrContent = "";

                string StrStart = "";
                string StrEnd = "";
                StrStart = dpStart.Text.ToString();
                StrEnd = dpEnd.Text.ToString();
                DateTime DtStart = new DateTime();
                DateTime DtEnd = new DateTime();
                StrTitle = this.txtSurveysTITLE.Text.ToString().Trim();
                StrContent = this.txtSurveysContent.Text.ToString().Trim();

                if (!string.IsNullOrEmpty(StrStart) && string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    //MessageBox.Show("结束时间不能为空");
                    return;
                }
                if (string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                    //MessageBox.Show("开始时间不能为空");
                    return;
                }
                if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
                {
                    DtStart = System.Convert.ToDateTime(StrStart);
                    DtEnd = System.Convert.ToDateTime(StrEnd + " 23:59:59");
                    if (DtStart > DtEnd)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARING"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"));
                        return;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "CREATEDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "CREATEDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "DISTRIBUTETITLE ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrTitle);
                }
                if (!string.IsNullOrEmpty(StrContent))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "CONTENT ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrContent);

                }

            }
            client.Get_SSurveyResultsAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }
        private ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> GetSelectList()//获取DataGrid选中项
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectList = new ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE>();
                foreach (T_OA_SATISFACTIONDISTRIBUTE obj in dg.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }
        private void BindDateGrid(List<T_OA_SATISFACTIONDISTRIBUTE> dataList)//绑定数据
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
                dg.ItemsSource = null;
        }
        private void browser_ReloadDataEvent()
        {
            GetData();
        }
        #endregion

       

         private void SetDataForm()
        { }
        
         private void btnShowResult_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectEmpSurveyList = GetSelectList();
            if (selectEmpSurveyList != null)
            {
                T_OA_SATISFACTIONDISTRIBUTE empSurveysInfo = selectEmpSurveyList[0];
                if (empSurveysInfo != null)
                {
                    if (empSurveysInfo.CHECKSTATE == "0")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }

                    //1看结果
                    Satisfaction_1 frmEmpSurveysSubmit = new Satisfaction_1();//empSurveysInfo
                    frmEmpSurveysSubmit.Distribute = empSurveysInfo;
                    EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                    browser.MinHeight = 650;
                    browser.MinWidth = 800;
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
        
        private void btnShowDetail_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_SATISFACTIONDISTRIBUTE> selectEmpSurveyList = GetSelectList();
            if (selectEmpSurveyList != null)
            {
                T_OA_SATISFACTIONDISTRIBUTE empSurveysInfo = selectEmpSurveyList[0];
                if (empSurveysInfo != null)
                {
                    if (empSurveysInfo.CHECKSTATE == "0")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("WARING"), Utility.GetResourceStr("NOSUBMISSIONMEETING"), Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    EmployeeList_sat frmEmpSurveysSubmit = new EmployeeList_sat();//empSurveysInfo
                    frmEmpSurveysSubmit.Require = empSurveysInfo.T_OA_SATISFACTIONREQUIRE;
                    EntityBrowser browser = new EntityBrowser(frmEmpSurveysSubmit);
                    browser.MinHeight = 550;
                    browser.MinWidth = 750;
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