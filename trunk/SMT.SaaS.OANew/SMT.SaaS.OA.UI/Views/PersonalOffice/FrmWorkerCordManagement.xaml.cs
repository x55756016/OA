using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.UserControls;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System.IO;
using System.Text;
using SMT.SAAS.Main.CurrentContext;
namespace SMT.SaaS.OA.UI.Views.PersonalOffice
{
    public partial class FrmWorkerCordManagement : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        public FrmWorkerCordManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_WORKRECORD", true);


            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            workerCordManager.GetWorkerCordListByUserIdCompleted += new EventHandler<GetWorkerCordListByUserIdCompletedEventArgs>(workerCordManager_GetWorkerCordListByUserIdCompleted);
            workerCordManager.DeleteWorkerCordListCompleted += new EventHandler<DeleteWorkerCordListCompletedEventArgs>(workerCordManager_DeleteWorkerCordListCompleted);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            ToolBar.btnOutExcel.Visibility = Visibility.Visible;
            //dataPager.PageSize = 23;
            //dataPager.PageIndexChanged += new EventHandler<EventArgs>(dataPager_PageIndexChanged);
            PARENT.Children.Add(loadbar);
            ToolBar.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            #region 初始值设置
            DateTime dpEnd = DateTime.Now.AddHours(23).AddMinutes(59);
            StringBuilder sbFilter = new StringBuilder();  //查询过滤条件 
            dpDate.Text = DateTime.Now.AddMonths(-1).ToShortDateString();
            dpEndDate.Text = DateTime.Now.ToShortDateString();
            paras = new ObservableCollection<object>();
            if (sbFilter.Length != 0)
            {
                sbFilter.Append(" and ");
            }
            sbFilter.Append("PLANTIME >= @" + paras.Count().ToString());
            DateTime dpStart = dpEnd.AddMonths(-1);//一个月前开始
            paras.Add(dpStart);
            sbFilter.Append(" and ");
            sbFilter.Append(" PLANTIME <= @" + paras.Count().ToString());
            paras.Add(dpEnd);
            filterString = sbFilter.ToString();
            #endregion
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);

            this.Loaded += new RoutedEventHandler(FrmWorkerCordManagement_Loaded);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
        }

        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            string data = ExportDataGrid(true, dg);
            SaveFileDialog sfd = new SaveFileDialog()
            {
                DefaultExt = "csv",
                Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*",
                FilterIndex = 1
            };
            if (sfd.ShowDialog() == true)
            {
                using (Stream stream = sfd.OpenFile())
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(data);
                        writer.Close();
                    }
                    stream.Close();
                }
            }
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            selectedList = GetSelectList();
            if (selectedList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            T_OA_WORKRECORD workerCordInfo = selectedList[0];
            WorkerCordForm frmWorkerCord = new WorkerCordForm(workerCordInfo, "view");
            EntityBrowser browser = new EntityBrowser(frmWorkerCord);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 580;
            browser.MinHeight = 505;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void FrmWorkerCordManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_WORKRECORD");
            List<string> listSearch = new List<string>();
            listSearch.Add("最近一个月记录");
            listSearch.Add("最近一周记录");
            dateTimeSearch.ItemsSource = listSearch;
            dateTimeSearch.SelectedIndex = 0;
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_WORKRECORD");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        #region 生成EXCLE
        private string FormatCSVField(string data)
        {
            return String.Format("\"{0}\"", data.Replace("\"", "\"\"\"").Replace("\n", "").Replace("\r", ""));
        }

        public string ExportDataGrid(bool withHeaders, DataGrid grid)
        {
            string colPath;
            System.Reflection.PropertyInfo propInfo;
            System.Windows.Data.Binding binding;
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            System.Collections.IList source = (grid.ItemsSource as System.Collections.IList);
            if (source == null)
                return "";

            List<string> headers = new List<string>();
            grid.Columns.ToList().ForEach(col =>
            {
                if (col is DataGridBoundColumn)
                {
                    //headers.Add(FormatCSVField(col.Header.ToString()));
                    headers.Add(Utility.GetResourceStr(col.Header.ToString()));
                }
            });
            strBuilder.Append(String.Join(",", headers.ToArray())).Append("\r\n");

            foreach (Object data in source)
            {
                List<string> csvRow = new List<string>();
                foreach (DataGridColumn col in grid.Columns)
                {
                    if (col is DataGridBoundColumn)
                    {
                        TextBlock bt = new TextBlock();
                        binding = (col as DataGridBoundColumn).Binding;
                        colPath = binding.Path.Path;
                        propInfo = data.GetType().GetProperty(colPath);
                        if (propInfo != null)
                        {
                            csvRow.Add(FormatCSVField(propInfo.GetValue(data, null).ToString()));
                        }
                    }
                }
                strBuilder.Append(String.Join(",", csvRow.ToArray())).Append("\r\n");
            }
            return strBuilder.ToString();
        }

        public String ExportDataGrid(DataGrid grid)
        {
            string colPath;
            System.Reflection.PropertyInfo propInfo;
            System.Windows.Data.Binding binding;
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            System.Collections.IList source = (grid.DataContext as System.Collections.IList);
            if (source == null)
                return "";
            foreach (Object data in source)
            {
                foreach (DataGridColumn col in grid.Columns)
                {
                    if (col is DataGridBoundColumn)
                    {
                        binding = (col as DataGridBoundColumn).Binding;
                        colPath = binding.Path.Path;
                        propInfo = data.GetType().GetProperty(colPath);
                        if (propInfo != null)
                        {
                            strBuilder.Append(propInfo.GetValue(data, null).ToString());
                            strBuilder.Append(",");
                        }
                    }

                }
                strBuilder.Append("\r\n");
            }
            return strBuilder.ToString();
        }
        #endregion
        //void dataPager_PageIndexChanged(object sender, EventArgs e)
        //{
        //    ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        //}

        void workerCordManager_DeleteWorkerCordListCompleted(object sender, DeleteWorkerCordListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
                Utility.ShowMessageBox("DELETE", false, true);
            }
            else
            {
                Utility.ShowMessageBox("DELETE", false, false);
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //ShowWorkerCordList(dataPager.PageIndex, filterString, paras, orderString);
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            WorkerCordForm frmWorkerCord = new WorkerCordForm(null, "add");
            EntityBrowser browser = new EntityBrowser(frmWorkerCord);
            browser.MinWidth = 580;
            browser.MinHeight = 505;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void browser_ReloadDataEvent()
        {
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        private void frmWorkerCord_RefreshParentForm(object sender, EventArgs e)
        {
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        private SmtOAPersonOfficeClient workerCordManager = new SmtOAPersonOfficeClient();

        private void ShowWorkerCordList(int pageIndex, string filter, ObservableCollection<object> paras)//"PLANTIME"
        {
            loadbar.Start();
            int pageCount = 0;
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            pageIndex++;
            workerCordManager.GetWorkerCordListByUserIdAsync(pageIndex, dataPager.PageSize, "CREATEDATE descending", filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }
        private void workerCordManager_GetWorkerCordListByUserIdCompleted(object sender, GetWorkerCordListByUserIdCompletedEventArgs e)
        {
            loadbar.Stop();
            List<T_OA_WORKRECORD> workerCordList = null;
            if (e.Result != null)
            {
                workerCordList = e.Result.ToList();
            }
            BindDgv(workerCordList, e.pageCount);
        }
        #region 绑定网格
        /// <summary>
        /// 列表显示
        /// </summary>
        private void BindDgv(List<T_OA_WORKRECORD> dataCalendarList, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (dataCalendarList == null || dataCalendarList.Count < 1)
            {
                dg.ItemsSource = null;
                return;
            }
            dg.ItemsSource = dataCalendarList;
        }
        #endregion

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            selectedList = GetSelectList();
            if (selectedList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonByOperationControl(selectedList[0], OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                T_OA_WORKRECORD workerCordInfo = selectedList[0];
                WorkerCordForm frmWorkerCord = new WorkerCordForm(workerCordInfo, "update");
                EntityBrowser browser = new EntityBrowser(frmWorkerCord);
                browser.MinWidth = 580;
                browser.MinHeight = 505;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            selectedList = GetSelectList();
            if (selectedList != null)
            {
                if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonByOperationControl(selectedList[0], OperationType.Delete, Common.CurrentLoginUserInfo.EmployeeID))
                {
                    string Result = "";
                    ComfirmWindow com = new ComfirmWindow();
                    com.OnSelectionBoxClosed += (obj, result) =>
                    {
                        if (selectedList != null)
                        {
                            try
                            { workerCordManager.DeleteWorkerCordListAsync(selectedList); }
                            catch
                            { }
                        }
                    };
                    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
        }

        private ObservableCollection<T_OA_WORKRECORD> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_WORKRECORD> selectList = new ObservableCollection<T_OA_WORKRECORD>();
                foreach (object obj in dg.SelectedItems)
                    selectList.Add((T_OA_WORKRECORD)obj);
                if (selectList != null && selectList.Count > 0)
                {
                    return selectList;
                }
            }
            return null;
        }

        private ObservableCollection<T_OA_WORKRECORD> selectedList = null;

        private ObservableCollection<object> paras = null;
        private string filterString = null;
        private string orderString = "UPDATEDATE desc"; //排序字段
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //dateTimeSearch.ItemsSource = null;
            StringBuilder sbFilter = new StringBuilder();  //查询过滤条件 
            paras = new ObservableCollection<object>();
            if (!string.IsNullOrEmpty(dpDate.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME>=@" + paras.Count().ToString());
                paras.Add(Convert.ToDateTime(dpDate.Text));
            }
            if (!string.IsNullOrEmpty(dpDate.Text) && string.IsNullOrEmpty(dpEndDate.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                return;
            }
            if (!string.IsNullOrEmpty(dpEndDate.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME <=@" + paras.Count().ToString());
                paras.Add(Convert.ToDateTime(dpEndDate.Text.Trim()).AddHours(23).AddMinutes(59));
            }
            if (!string.IsNullOrEmpty(dpEndDate.Text) && string.IsNullOrEmpty(dpDate.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                return;
            }

            string StrStart = string.Empty;
            string StrEnd = string.Empty;
            StrStart = dpDate.Text.ToString();
            StrEnd = dpEndDate.Text.ToString();
            DateTime DtStart = new DateTime();
            DateTime DtEnd = new DateTime();
            if (!string.IsNullOrEmpty(StrStart) && !string.IsNullOrEmpty(StrEnd))
            {
                DtStart = System.Convert.ToDateTime(StrStart);
                DtEnd = System.Convert.ToDateTime(StrEnd);
                if (DtStart > DtEnd)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME", "STARTDATE"));
                    return;
                }
            }

            filterString = sbFilter.ToString();
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            dpEndDate.Text = string.Empty;
            dpDate.Text = string.Empty;
            filterString = null;
            paras = null;
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        private void dataPager_Click(object sender, RoutedEventArgs e)
        {
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        
        /// <summary>
        /// 根据一个星期或是一个月进行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimeSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox dateTimeSearch = Utility.FindChildControl<ComboBox>(controlsToolkitTUV, "dateTimeSearch");
            filterString = null;
            DateTime dpEnd = DateTime.Now.AddDays(1);
            StringBuilder sbFilter = new StringBuilder();  //查询过滤条件 
            paras = new ObservableCollection<object>();

            if (dateTimeSearch != null)
            {
                if (dateTimeSearch.SelectedItem != null)
                {
                    string strName = dateTimeSearch.SelectedItem as string;

                    if (strName == "最近一周记录")
                    {
                        dpDate.Text = DateTime.Now.AddDays(-7).ToShortDateString();
                        dpEndDate.Text = DateTime.Now.ToShortDateString();
                        if (sbFilter.Length != 0)
                        {
                            sbFilter.Append(" and ");
                        }
                        sbFilter.Append("PLANTIME>=@" + paras.Count().ToString());
                        DateTime dpStart = dpEnd.AddDays(-7).AddHours(23).AddMinutes(-58);//一个星期前开始
                        paras.Add(dpStart);
                        sbFilter.Append(" and ");
                        sbFilter.Append(" PLANTIME <=@" + paras.Count().ToString());
                        paras.Add(dpEnd);
                        filterString = sbFilter.ToString();
                    }
                    else if (strName == "最近一个月记录")
                    {
                        dpDate.Text = DateTime.Now.AddMonths(-1).ToShortDateString();
                        dpEndDate.Text = DateTime.Now.ToShortDateString();
                        if (sbFilter.Length != 0)
                        {
                            sbFilter.Append(" and ");
                        }
                        sbFilter.Append("PLANTIME>=@" + paras.Count().ToString());
                        DateTime dpStart = dpEnd.AddMonths(-1).AddHours(-23).AddMinutes(-58);//一个月前开始
                        paras.Add(dpStart);
                        sbFilter.Append(" and ");
                        sbFilter.Append(" PLANTIME <=@" + paras.Count().ToString());
                        paras.Add(dpEnd);
                        filterString = sbFilter.ToString();
                    }

                }
            }
            else
            {
                dpDate.Text = DateTime.Now.AddMonths(-1).ToShortDateString();
                dpEndDate.Text = DateTime.Now.ToShortDateString();
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME >= @" + paras.Count().ToString());
                DateTime dpStart = dpEnd.AddMonths(-1);//一个月前开始
                paras.Add(dpStart);
                sbFilter.Append(" and ");
                sbFilter.Append(" PLANTIME <= @" + paras.Count().ToString());
                paras.Add(dpEnd);
                filterString = sbFilter.ToString();
            }
            ShowWorkerCordList(dataPager.PageIndex, filterString, paras);
        }

        /// <summary>
        /// 重置，情况
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dpDate.Text = string.Empty;
            dpEndDate.Text = string.Empty;
        }
    }
}