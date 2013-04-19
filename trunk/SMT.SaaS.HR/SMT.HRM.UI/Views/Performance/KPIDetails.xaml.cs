/// <summary>
/// Log No.： 1
/// Modify Desc： 等待控制，显示用户和业务信息，审核状态，组织结构样式，按钮隔线
/// Modifier： 冉龙军
/// Modify Date： 2010-08-10
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PerformanceWS;
using System.Windows.Navigation;
using SMT.HRM.UI.Form.Performance;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.HRM.UI.Views.Performance
{
    public partial class KPIDetails : BasePage, IClient
    {
        private PersonnelServiceClient client; //人员服务
        // private OrganizationServiceClient orgClient;//机构服务
        private PerformanceServiceClient kpiClient;//绩效考核服务
        public FormTypes FormType { get; set; }//窗口状态

        public T_HR_KPIRECORD SelectedPerson { get; set; }//当前选择的抽查组人员的关联表实体

        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;//所有公司
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;//所有部门
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions; //所有岗位

        private bool myDetail = false;
        SMTLoading loadbar = new SMTLoading();

        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }
        // 1s 冉龙军
        public string Checkstate { get; set; }
        // 1e
        public KPIDetails()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(KPIDetails_Loaded);            
        }

        void KPIDetails_Loaded(object sender, RoutedEventArgs e)
        {
            InitPara();
        }

        /// <summary>
        /// 获取页面数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Employee_Loaded(object sender, RoutedEventArgs e)
        {
            // 1s 冉龙军
            // 2s
            //Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
            // 2e
            // 1e
            // LoadData();
            // BindTree();
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "KPIDetails", false);
            // 1e

            //this.ToolBar.cbxCheckState.Visibility = Visibility.Visible;
            // this.ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 构造页面触发事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                // 1s 冉龙军
                PARENT.Children.Add(loadbar);
                loadbar.Stop();
                // 1e
                client = new PersonnelServiceClient();

                kpiClient = new PerformanceServiceClient();
                kpiClient.GetKPIRecordPagingCompleted += new EventHandler<GetKPIRecordPagingCompletedEventArgs>(client_GetKPIRecordPagingCompleted);

                //orgClient = new OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);

                DtGrid.CurrentCellChanged += new EventHandler<EventArgs>(DtGrid_CurrentCellChanged);
                this.Loaded += new RoutedEventHandler(Employee_Loaded);
                this.Loaded += new RoutedEventHandler(AuditState_Loaded);
                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);

                //this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
                this.ToolBar.btnNew.Visibility = Visibility.Collapsed;
                this.ToolBar.btnEdit.Visibility = Visibility.Collapsed;
                this.ToolBar.btnDelete.Visibility = Visibility.Collapsed;
                this.ToolBar.btnRefresh.Visibility = Visibility.Collapsed;
                this.ToolBar.BtnView.Visibility = Visibility.Collapsed;
                // 1s 冉龙军              
                this.ToolBar.btnAudit.Visibility = Visibility.Collapsed;

                this.ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
                this.ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
                this.ToolBar.retNew.Visibility = Visibility.Collapsed;
                this.ToolBar.retEdit.Visibility = Visibility.Collapsed;
                this.ToolBar.retDelete.Visibility = Visibility.Collapsed;
                this.ToolBar.retRefresh.Visibility = Visibility.Collapsed;
                this.ToolBar.retAudit.Visibility = Visibility.Collapsed;
                this.ToolBar.retAuditNoPass.Visibility = Visibility.Collapsed;
                this.ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;

                // 1e
                ImageButton btnComplain = new ImageButton();
                btnComplain.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_4405_d.png",
                    Utility.GetResourceStr("COMPLAIN")).Click += new RoutedEventHandler(btnComplain_Click);

                ImageButton btnEditComplain = new ImageButton();
                btnEditComplain.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png",
                    Utility.GetResourceStr("EDIT")).Click += new RoutedEventHandler(btnEditComplain_Click);

                ImageButton btnViewComplain = new ImageButton();
                btnViewComplain.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/18_service.png",
                    Utility.GetResourceStr("VIEWCOMPLAIN")).Click += new RoutedEventHandler(btnViewComplain_Click);

                ImageButton btnViewReview = new ImageButton();
                btnViewReview.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/Tool/ico_16_1036.png",
                     Utility.GetResourceStr("VIEWREVIEW")).Click += new RoutedEventHandler(btnReview_Click);

                this.ToolBar.stpOtherAction.Children.Add(btnComplain);
                // 1s 冉龙军


                //this.ToolBar.stpOtherAction.Children.Add(btnEditComplain);  //修改


                this.ToolBar.stpOtherAction.Children.Add(ToolBar.AddPartition());
                // 1e
                // this.ToolBar.stpOtherAction.Children.Add(btnViewComplain);
                // 1s 冉龙军
                this.ToolBar.stpOtherAction.Children.Add(ToolBar.AddPartition());
                // 1e
                // this.ToolBar.stpOtherAction.Children.Add(btnViewReview);

            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void treeOrganization_SelectedClick(object sender, EventArgs e)
        {
            myDetail = false;
            LoadData();
        }
        // 1s 冉龙军
        /// <summary>
        /// KPI明细状态列表
        /// </summary>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            //if (dict != null)
            //{
            //    Checkstate = dict.DICTIONARYVALUE.ToString();
            //    //Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SUMPERFORMANCERECORD");
            //    LoadData();
            //}

        }

        void AuditState_Loaded(object sender, RoutedEventArgs e)
        {
            //审核状态绑定
            //Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.Approved).ToString());
        }

        // 1e
        /// <summary>
        /// 读取列表信息
        /// </summary>
        void LoadData()
        {
            loadbar.Start();
            int pageCount = 0;
            string filter = "", sType = "", sValue = "";
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (myDetail)
            {
                // 1s 冉龙军
                //filter += "APPRAISEEID==@" + paras.Count().ToString();
                filter += "T_HR_KPIRECORD.APPRAISEEID==@" + paras.Count().ToString();
                // 1e
                paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            else
            {
                TextBox txtEmpName = Utility.FindChildControl<TextBox>(expander, "txtEmpName");
                TextBox txtEmpCode = Utility.FindChildControl<TextBox>(expander, "txtEmpCode");

                if (!string.IsNullOrEmpty(txtEmpCode.Text.Trim()))
                {
                    filter += "@" + paras.Count().ToString() + ".Contains(EMPLOYEECODE)"; ;
                    paras.Add(txtEmpCode.Text.Trim());
                }
                if (!string.IsNullOrEmpty(txtEmpName.Text.Trim()))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "@" + paras.Count().ToString() + ".Contains(EMPLOYEECNAME)"; ;
                    paras.Add(txtEmpName.Text.Trim());
                }

                sType = treeOrganization.sType;
                sValue = treeOrganization.sValue;
                if (string.IsNullOrEmpty(sType))
                {
                    loadbar.Stop();
                    return;
                }
            }


            DatePicker dpStartDate = Utility.FindChildControl<DatePicker>(expander, "dpStartDate");
            DatePicker dpEndDate = Utility.FindChildControl<DatePicker>(expander, "dpEndDate");

            string StartDate = string.Empty;
            string EndDate = string.Empty;

            if (dpStartDate != null)
            {
                StartDate = dpStartDate.Text;
            }
            if (dpEndDate != null)
            {
                EndDate = dpEndDate.Text;
            }

            string strState = "";
            //if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            //{
            //    strState = Checkstate;
            //}
            kpiClient.GetKPIRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "T_HR_KPIRECORD.UPDATEDATE", filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, StartDate, EndDate, strState);
        }

        #region 所有的事件
        ///// <summary>
        ///// 获取所有模块定义后事件
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void fClient_GetModelFlowRelationInfosListBySearchCompleted(object sender, GetModelFlowRelationInfosListBySearchCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            //添加流程字典
        //            Application.Current.Resources.Add("SYS_FlowInfo", e.Result);
        //        }
        //    }
        //}

        /// <summary>
        /// 获取人员后触发事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetKPIRecordPagingCompleted(object sender, GetKPIRecordPagingCompletedEventArgs e)
        {
            // 1s 冉龙军
            //List<T_HR_KPIRECORD> list = new List<T_HR_KPIRECORD>();
            List<V_KPIRECORD> list = new List<V_KPIRECORD>();
            // 1e
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                DtGrid.ItemsSource = list;
                dataPager.PageCount = e.pageCount;
                //DtGrid.SelectedIndex = 0;
                //DtGrid.SelectedItem = DtGrid.SelectedItem;
            }
            // 1s 冉龙军
            loadbar.Stop();
            // 1e
        }

        /// <summary>
        /// 查询事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            //PerformanceList l = new PerformanceList();

            //EntityBrowser browser = new EntityBrowser(l);
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            myDetail = false;
            LoadData();
        }

        /// <summary>
        /// 点击列表的单元格改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DtGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                // 1s 冉龙军
                //SelectedPerson = DtGrid.SelectedItem as T_HR_KPIRECORD;
                SelectedPerson = (DtGrid.SelectedItem as V_KPIRECORD).T_HR_KPIRECORD;
                // 1e

                //kpiClient.GetRandomGroupPersonByIDAsync(employee.EMPLOYEEID);
            }
        }

        /// <summary>
        /// 读取每行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            /*/获取该行人员信息
            T_HR_KPIRECORD ent = e.Row.DataContext as T_HR_KPIRECORD;
            //获取该行的CheckBox
            Button btnComplain = DtGrid.Columns[15].GetCellContent(e.Row).FindName("btnComplain") as Button;
            Button btnViewComplain = DtGrid.Columns[16].GetCellContent(e.Row).FindName("btnViewComplain") as Button;
            Button btnReview = DtGrid.Columns[17].GetCellContent(e.Row).FindName("btnViewReview") as Button;
            //申诉状态
            if (ent.COMPLAINSTATUS.Equals("0"))
            {
                btnViewComplain.IsEnabled = false;
            }
            else if (ent.COMPLAINSTATUS.Equals("1"))
            {
                btnComplain.IsEnabled = false;
            }
            //增加申诉事件
            btnComplain.Click += new RoutedEventHandler(btnComplain_Click);
            btnViewComplain.Click += new RoutedEventHandler(btnViewComplain_Click);
            btnReview.Click += new RoutedEventHandler(btnReview_Click);
             * */

            SetRowLogo(DtGrid, e.Row, "T_HR_KPIRECORD");
        }

        /// <summary>
        /// 翻页条事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        /// <summary>
        /// 每行的提出申诉事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnComplain_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                V_KPIRECORD tmpEnt = DtGrid.SelectedItems[0] as V_KPIRECORD;
                if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != tmpEnt.T_HR_KPIRECORD.APPRAISEEID)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("只能申诉自己的考核"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("只能申诉自己的考核"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                // 1s 冉龙军
                //ComplainProcess form = new ComplainProcess(FormTypes.New, ((T_HR_KPIRECORD)DtGrid.SelectedItem).KPIRECORDID, "");
                //ComplainProcess form = new ComplainProcess(FormTypes.New, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                // 1e
                PerformanceComplain form = new PerformanceComplain(FormTypes.New, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.New;
                form.MinHeight = 350;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 修改申诉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEditComplain_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                PerformanceComplain form = new PerformanceComplain(FormTypes.Edit, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");

                EntityBrowser browser = new EntityBrowser(form);

                browser.FormType = FormTypes.Edit;
                form.MinHeight = 350;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        void browser_ReloadDataEvent()
        {
            LoadData();
        }

        /// <summary>
        /// 每行的查看申诉事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewComplain_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                // 1s 冉龙军
                //ComplainProcess form = new ComplainProcess(FormTypes.Browse, ((T_HR_KPIRECORD)DtGrid.SelectedItem).KPIRECORDID, "");
                //ComplainProcess form = new ComplainProcess(FormTypes.Browse, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                // 1e

                PerformanceComplain form = new PerformanceComplain(FormTypes.Browse, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Browse;
                //form.MinHeight = 350;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 每行的审核事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReview_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), "功能未实现");
                // 1s 冉龙军
                //ComplainProcess form = new ComplainProcess(FormTypes.Audit, ((T_HR_KPIRECORD)DtGrid.SelectedItem).KPIRECORDID, "");
                //ComplainProcess form = new ComplainProcess(FormTypes.Audit, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                // 1e



                PerformanceComplain form = new PerformanceComplain(FormTypes.Audit, ((V_KPIRECORD)DtGrid.SelectedItem).T_HR_KPIRECORD.KPIRECORDID, "");
                EntityBrowser browser = new EntityBrowser(form);
                browser.FormType = FormTypes.Audit;
                //form.MinHeight = 350;
                form.MinWidth = 500;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #endregion 所有的事件

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "KPIDetails", false);
            // 1e
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        private void btnMyKpiDetails_Click(object sender, RoutedEventArgs e)
        {
            myDetail = true;
            LoadData();
        }

        private void dpStartDate_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void dpEndDate_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

    }
}
