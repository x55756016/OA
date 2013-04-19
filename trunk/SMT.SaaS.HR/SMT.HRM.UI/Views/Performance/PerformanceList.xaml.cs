/// <summary>
/// Log No.： 1
/// Modify Desc： 等待控制，显示员工信息，审核铵钮和状态字典以及审核人，组织结构样式
/// Modifier： 冉龙军
/// Modify Date： 2010-08-11
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using SMT.Saas.Tools.PerformanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.Form.Performance;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.Collections.ObjectModel;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Media;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Views.Performance
{
    public partial class PerformanceList : BasePage
    {
        SMTLoading loadbar = new SMTLoading();

        //private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orgClient;//机构服务
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys;//所有公司
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments;//所有部门
        //private List<SMT.Saas.Tools.OrganizationWS.T_HR_POST> allPositions; //所有岗位

        private PerformanceServiceClient client = new PerformanceServiceClient();  //绩效考核服务
        public T_HR_SUMPERFORMANCERECORD SumPerformance { get; set; }                  //当前所选绩效考核
        public string groupPersonIDs = string.Empty;                               //当前所选绩效考核的人员ID串
        public ObservableCollection<T_HR_SUMPERFORMANCERECORD> SumPerformanceList = new ObservableCollection<T_HR_SUMPERFORMANCERECORD>();  //绩效考核列表
        // 1s 冉龙军
        //public ObservableCollection<T_HR_PERFORMANCERECORD> PerformanceRecordList = new ObservableCollection<T_HR_PERFORMANCERECORD>();  //当前绩效考核人员关联列表
        public ObservableCollection<V_PERFORMANCERECORD> PerformanceRecordList = new ObservableCollection<V_PERFORMANCERECORD>();  //当前绩效考核人员关联列表
        public string Checkstate { get; set; }
        // 1e

        private bool myDetail = false;
        private string isTag;

        public string IsTag
        {
            get { return isTag; }
            set { isTag = value; }
        }

        public PerformanceList()
        {
            InitializeComponent();
            //InitPara();
            //GetEntityLogo("T_HR_SUMPERFORMANCERECORD");
            this.Loaded +=new RoutedEventHandler(PerformanceList_Loaded);
        }

        /// <summary>
        /// 读取页面事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PerformanceList_Loaded(object sender, RoutedEventArgs e)
        {
            // 1s 冉龙军
            Utility.CbxItemBinder(ToolBar.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.UnSubmit).ToString());
            // 1e
            // LoadData();
            // BindTree();
            // 1s 冉龙军
            Utility.DisplayGridToolBarButton(ToolBar, "SUMPERFORMANCE", true);
            // 1e
            InitPara();
            GetEntityLogo("T_HR_SUMPERFORMANCERECORD");
        }

        /// <summary>
        /// 当用户导航到此页面时执行
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 1s 冉龙军
            //Utility.DisplayGridToolBarButton(ToolBar, "PerformanceList", false);
            // 1e
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        public void InitPara()
        {
            try
            {
                // 1s 冉龙军
                //LayoutRoot.Children.Add(loadbar);
                //loadbar.Start();
                Parent.Children.Add(loadbar);
                loadbar.Stop();
                // 1e
                client.GetSumPerformancePagingCompleted += new EventHandler<GetSumPerformancePagingCompletedEventArgs>(client_GetSumPerformancePagingCompleted);
                client.DeleteSumPerformancesCompleted += new EventHandler<DeleteSumPerformancesCompletedEventArgs>(client_DeleteSumPerformancesCompleted);
                // 1s 冉龙军
                //client.GetPerformanceAllBySumIDCompleted += new EventHandler<GetPerformanceAllBySumIDCompletedEventArgs>(client_GetPerformanceAllBySumIDCompleted);
                client.GetPerformanceEmployeeAllBySumIDCompleted += new EventHandler<GetPerformanceEmployeeAllBySumIDCompletedEventArgs>(client_GetPerformanceEmployeeAllBySumIDCompleted);
                // 1e


                // client.GetEmployePerformanceCompleted += new EventHandler<GetEmployePerformanceCompletedEventArgs>(client_GetEmployePerformanceCompleted);

                //orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
                //orgClient.GetCompanyActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyActivedCompletedEventArgs>(orgClient_GetCompanyActivedCompleted);
                //orgClient.GetDepartmentActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentActivedCompletedEventArgs>(orgClient_GetDepartmentActivedCompleted);
                //orgClient.GetPostActivedCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostActivedCompletedEventArgs>(orgClient_GetPostActivedCompleted);
                treeOrganization.SelectedClick += new EventHandler(treeOrganization_SelectedClick);
                //this.Loaded += new RoutedEventHandler(PerformanceList_Loaded);
                this.ToolBar.btnNew.Click += new RoutedEventHandler(btnAdd_Click);
                this.ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
                this.ToolBar.btnDelete.Click += new RoutedEventHandler(btnDel_Click);
                this.ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
                this.ToolBar.BtnView.Click += new RoutedEventHandler(btnView_Click);
                // 1s 冉龙军
                //this.ToolBar.retNew.Visibility = Visibility.Collapsed;
                this.ToolBar.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
                this.ToolBar.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
                // 1e

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
            dtgPerformance.ItemsSource = null;
            DtGrid.ItemsSource = null;
            LoadData();
        }

        // 1s 冉龙军
        /// <summary>
        /// 绩效考核状态列表
        /// </summary>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = ToolBar.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                //Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), ToolBar, "T_HR_SUMPERFORMANCERECORD");
                LoadData();
            }
        }
        // 1e
        /// <summary>
        /// 刷新绩效考核列表
        /// </summary>
        void LoadData()
        {
            int pageCount = 0;
            string filter = "", sType = "", sValue = "";
            bool isSelf = false;
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            // 1s 冉龙军
            string strState = "";

            if (Checkstate != Convert.ToInt32(CheckStates.All).ToString())
            {
                strState = Checkstate;
            }
            //if (!string.IsNullOrEmpty(strState))
            //{
            //    if (!string.IsNullOrEmpty(filter))
            //    {
            //        filter += " AND";
            //    }

            //    filter += " CHECKSTATE == @" + paras.Count();
            //    paras.Add(strState);
            //}
            // 1e
            if (myDetail)
            {
                isSelf = true;
            }
            sType = treeOrganization.sType;
            sValue = treeOrganization.sValue;
            SumPerformanceList.Clear();

            client.GetSumPerformancePagingAsync(dataPager.PageIndex, dataPager.PageSize, "UPDATEDATE", filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, isSelf, strState);

            loadbar.Start();
        }

        #region 事件

        /// <summary>
        /// 获取绩效考核当前页后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetSumPerformancePagingCompleted(object sender, GetSumPerformancePagingCompletedEventArgs e)
        {
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
                    SumPerformanceList = e.Result;
                }
                DtGrid.ItemsSource = SumPerformanceList;
                dataPager.PageCount = e.pageCount;
            }
            ToolBar.btnRefresh.IsEnabled = true;
            loadbar.Stop();
        }

        /// <summary>
        /// 删除当前绩效考核后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DeleteSumPerformancesCompleted(object sender, DeleteSumPerformancesCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                LoadData();
            }
        }

        /// <summary>
        /// 获取所有绩效考核人员后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // 1s 冉龙军
        //private void client_GetPerformanceAllBySumIDCompleted(object sender, GetPerformanceAllBySumIDCompletedEventArgs e)
        private void client_GetPerformanceEmployeeAllBySumIDCompleted(object sender, GetPerformanceEmployeeAllBySumIDCompletedEventArgs e)
        // 1e
        {
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
                    PerformanceRecordList = e.Result;
                    dpGrid.PageSize = 20;
                    PagedCollectionView pager = new PagedCollectionView(PerformanceRecordList);
                    dtgPerformance.ItemsSource = pager;
                }


                //dataProjectPager.PageCount = e.;
            }
            loadbar.Stop();
        }

        /// <summary>
        /// 获取所有绩效考核人员后的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void client_GetEmployePerformanceCompleted(object sender, GetEmployePerformanceCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            Dictionary<string, decimal> dic = e.Result;
        //        }
        //    }
        //}

        /// <summary>
        /// 添加绩效考核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //貌似无用 先注掉
            //ObservableCollection<string> x = new ObservableCollection<string>();
            //x.Add("d32ad3d3-bd42-4552-874c-484f595e4286");
            //x.Add("e25c9a33-6ee7-4a61-824a-78f073006990");
            //client.GetEmployePerformanceAsync(x, new DateTime(2010, 6, 1), new DateTime(2010, 6, 30));

            EditPerformance form = new EditPerformance(FormTypes.New, SumPerformance);
            EntityBrowser browser = new EntityBrowser(form);
            form.MinWidth = 780;
            form.MinHeight = 570;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            //LoadData();
            // 1s 冉龙军
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

            //  LoadData();
            // 1e
        }

        /// <summary>
        /// 查看绩效考核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnView_Click(object sender, RoutedEventArgs e)
        {
            if (SumPerformance != null)
            {
                EditPerformance form = new EditPerformance(FormTypes.Browse, SumPerformance);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 780;
                form.MinHeight = 570;
                // 1s 冉龙军
                browser.FormType = FormTypes.Browse;
                // 1e
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 修改绩效考核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (SumPerformance != null)
            {
                EditPerformance form = new EditPerformance(FormTypes.Edit, SumPerformance);
                EntityBrowser browser = new EntityBrowser(form);
                form.MinWidth = 780;
                form.MinHeight = 570;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
                // LoadData();
            }
            else
            {
                //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTDATAALERT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 删除绩效考核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            string Result = "";
            if (DtGrid.SelectedItems.Count > 0)
            {
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<string> ids = new ObservableCollection<string>();

                    foreach (T_HR_SUMPERFORMANCERECORD tmp in DtGrid.SelectedItems)
                    {
                        if (tmp.CHECKSTATE.Equals("1"))
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        else if (tmp.CHECKSTATE.Equals("2"))
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("Msg_NoDeleteOrder"));
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("Msg_NoDeleteOrder"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        ids.Add(tmp.SUMID);
                    }
                    client.DeleteSumPerformancesAsync(ids);
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 刷新绩效考核列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ToolBar.btnRefresh.IsEnabled = false;
            LoadData();
        }

        // 1s 冉龙军
        /// <summary>
        /// 审核绩效考核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (SumPerformance != null)
            {
                EditPerformance form = new EditPerformance(FormTypes.Audit, SumPerformance);
                EntityBrowser browser = new EntityBrowser(form);

                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(LoadData);
                browser.FormType = FormTypes.Audit;
                browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "AUDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "AUDIT"),
        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        // 1e
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            myDetail = false;
            LoadData();
        }

        private void btnMyKpiDetails_Click(object sender, RoutedEventArgs e)
        {
            myDetail = true;
            LoadData();
            // 1s 冉龙军
            myDetail = false;
            // 1e
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(DtGrid, e.Row, "T_HR_SUMPERFORMANCERECORD");
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

        private void DtGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DtGrid.SelectedItem != null)
            {
                SumPerformance = (T_HR_SUMPERFORMANCERECORD)DtGrid.SelectedItems[0];
                PerformanceRecordList.Clear();
                // 1s 冉龙军
                //client.GetPerformanceAllBySumIDAsync(SumPerformance.SUMID);
                client.GetPerformanceEmployeeAllBySumIDAsync(SumPerformance.SUMID);
                // 1e
            }

        }

        #endregion 事件

    }
}
