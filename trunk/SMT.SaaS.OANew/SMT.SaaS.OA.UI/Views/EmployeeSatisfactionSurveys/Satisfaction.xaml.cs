//********满意度调查方案管理页******
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

namespace SMT.SaaS.OA.UI.Views.EmployeeSatisfactionSurveys
{
    public partial class Satisfaction : BasePage
    {
        #region 全局变量定义
        private SMTLoading loadbar;//载入动画
        private SmtOAPersonOfficeClient client=null;
        private ObservableCollection<V_Satisfaction> deletedList = new ObservableCollection<V_Satisfaction>();//标记被删除的对象
        private string checkState = ((int)CheckStates.ALL).ToString();
        private bool IsQuery = false;
        //private V_Satisfaction survey;
        #endregion

        #region 构造函数
        public Satisfaction()
        {
            InitializeComponent();
            EventRegister();
        }
        #endregion

        #region 事件注册
        private void EventRegister()
        {
            client = new SmtOAPersonOfficeClient();
            this.Loaded += new RoutedEventHandler(Satisfaction_Loaded);
            client.Del_SSurveysCompleted += new EventHandler<Del_SSurveysCompletedEventArgs>(Del_SSurveysCompleted);
            client.Get_SSurveysCompleted += new EventHandler<Get_SSurveysCompletedEventArgs>(Get_SSurveysCompleted);
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
        private void Satisfaction_Loaded(object sender, RoutedEventArgs e)//页面载入
        {
            loadbar = new SMTLoading();
            this.dpStart.SelectedDate = DateTime.Today.AddDays(-15);
            this.dpEnd.SelectedDate = DateTime.Today.AddDays(15);
            PARENT.Children.Add(loadbar);
            GetEntityLogo("T_OA_SATISFACTIONMASTER");
            GetData();
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", checkState);
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_SATISFACTIONMASTER", true);
        }
        private void Del_SSurveysCompleted(object sender, Del_SSurveysCompletedEventArgs e)//WCF删除
        {
            if (e.Result > 0)
            {
                deletedList.Clear();
                GetData();
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", ""));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("DELETEFAILED", ""));
            }
        }
        private void Get_SSurveysCompleted(object sender, Get_SSurveysCompletedEventArgs e)//WCF取数据
        {
            loadbar.Stop();
            IsQuery = false;
            ObservableCollection<V_Satisfaction> o = e.Result;
            if (o != null)
            {
                List<V_Satisfaction> dataList = o.ToList();
                BindDateGrid(dataList);
            }
            else
                BindDateGrid(null);
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)//新增
        {
            SatisfactionSurveyChildWindow form = new SatisfactionSurveyChildWindow(FormTypes.New,"");
            EntityBrowser browser = new EntityBrowser(form);
            browser.FormType = FormTypes.New;
            browser.MinHeight = 300;
            browser.MinWidth = 580;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)//修改
        {
            ObservableCollection<V_Satisfaction> selectItems = GetSelectList();
            if (selectItems != null)
            {
                V_Satisfaction ent = dg.SelectedItems[0] as V_Satisfaction;
                if (ent.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString() || ent.RequireMaster.CHECKSTATE == ((int)CheckStates.UnApproved).ToString())
                {
                    SatisfactionSurveyChildWindow form = new SatisfactionSurveyChildWindow(FormTypes.Edit,ent.RequireMaster.SATISFACTIONMASTERID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Edit;
                    browser.MinHeight = 500;
                    browser.MinWidth = 650;
                    browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTONLYCANMODIFYTHEDATASUBMITTED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)//删除
        {
            ObservableCollection<V_Satisfaction> selectItems = GetSelectList();
            if (selectItems != null)
            {
                for (int i = 0; i < dg.SelectedItems.Count; i++)
                {
                    V_Satisfaction ent = dg.SelectedItems[i] as V_Satisfaction;
                    if (ent.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                    {
                        string Result = "";
                        ComfirmWindow com = new ComfirmWindow();
                        com.OnSelectionBoxClosed += (obj, result) =>
                        {
                            try { client.Del_SSurveysAsync(selectItems); }
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
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }
        private void btnAudit_Click(object sender, RoutedEventArgs e)//审核
        {
            ObservableCollection<V_Satisfaction> selectItems = GetSelectList();
            if (selectItems != null)
            {
                V_Satisfaction ent = dg.SelectedItems[0] as V_Satisfaction;
                if (ent.RequireMaster.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                    ent.RequireMaster.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString() ||
                    ent.RequireMaster.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                {
                    SatisfactionSurveyChildWindow form = new SatisfactionSurveyChildWindow(FormTypes.Audit, ent.RequireMaster.SATISFACTIONMASTERID);
                    EntityBrowser browser = new EntityBrowser(form);
                    browser.FormType = FormTypes.Audit;
                    browser.MinHeight = 550;
                    browser.MinWidth = 750;
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
            ObservableCollection<V_Satisfaction> selectItems = GetSelectList();
            if (selectItems != null)
            {
                Satisfaction_upd form = new Satisfaction_upd(FormTypes.Resubmit);
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
            ObservableCollection<V_Satisfaction> selectItems = GetSelectList();
            if (selectItems != null)
            {
                SatisfactionSurveyChildWindow form = new SatisfactionSurveyChildWindow(FormTypes.Browse, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                browser.MinHeight = 550;
                browser.MinWidth = 750;
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

        #region XAML事件处理程序
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)//行加载
        {
            SetRowLogo(dg, e.Row, "T_OA_SATISFACTIONMASTER");

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
            this.dpStart.Text = "";
            this.dpEnd.Text = "";
            //this.dpStart.SelectedDate = null;
            //this.dpStart.SelectedDate = null;
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
                        filter += "RequireMaster.CREATEDATE >=@" + paras.Count().ToString();//开始时间
                        paras.Add(DtStart);
                        filter += " and ";
                        filter += "RequireMaster.CREATEDATE <=@" + paras.Count().ToString();//结束时间
                        paras.Add(DtEnd);
                    }
                }

                if (!string.IsNullOrEmpty(StrTitle))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "RequireMaster.SATISFACTIONTITLE ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrTitle);
                }
                if (!string.IsNullOrEmpty(StrContent))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "RequireMaster.CONTENT ^@" + paras.Count().ToString();//类型名称
                    paras.Add(StrContent);

                }

            }
            client.Get_SSurveysAsync(dataPager.PageIndex, dataPager.PageSize, "RequireMaster.UPDATEDATE", filter, paras, pageCount, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.EmployeeID, checkState);
        }
        private ObservableCollection<V_Satisfaction> GetSelectList()//获取DataGrid选中项
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<V_Satisfaction> selectList = new ObservableCollection<V_Satisfaction>();
                foreach (V_Satisfaction obj in dg.SelectedItems)
                    selectList.Add(obj);

                if (selectList != null && selectList.Count > 0)
                    return selectList;
            }
            return null;
        }
        private void BindDateGrid(List<V_Satisfaction> dataList)//绑定数据
        {
            if (dataList != null && dataList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataList);
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
            {
                dg.ItemsSource = null;
            }
        }
        private void browser_ReloadDataEvent()
        {
            GetData();
        }
        #endregion

      
    }
}