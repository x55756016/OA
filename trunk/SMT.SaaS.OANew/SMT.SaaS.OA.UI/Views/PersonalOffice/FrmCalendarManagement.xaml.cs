using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.Views.PersonalOffice
{
    public partial class FrmCalendarManagement : BasePage
    {
        private SMTLoading loadbar = new SMTLoading();
        string SearchUserID = "";
        public FrmCalendarManagement()
        {
            InitializeComponent();
            Utility.DisplayGridToolBarButton(ToolBar, "T_OA_CALENDAR", true);
            calendarManagement.GetCalendarListByUserIDCompleted += new EventHandler<GetCalendarListByUserIDCompletedEventArgs>(calendarManagement_GetCalendarListByUserIDCompleted);
            calendarManagement.DelCalendarListCompleted += new EventHandler<DelCalendarListCompletedEventArgs>(calendarManagement_DelCalendarListCompleted);
            ToolBar.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            ToolBar.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            ToolBar.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            ToolBar.btnAudit.Visibility = Visibility.Collapsed;
            ToolBar.cbxCheckState.Visibility = Visibility.Collapsed;
            ToolBar.txtCheckStateName.Visibility = Visibility.Collapsed;
            dataPager.PageSize = 20;
            //dataPager.PageIndexChanged += new EventHandler<EventArgs>(dataPager_PageIndexChanged);
            PARENT.Children.Add(loadbar);
            ToolBar.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            #region 初始值设置
            searchParas = new ObservableCollection<object>();
            DateTime dpEndDate = DateTime.Now.AddHours(23).AddMinutes(59);
            System.Text.StringBuilder sbFilter = new System.Text.StringBuilder();  //查询过滤条件 
            cndDateCanlendar.Text = DateTime.Now.AddDays(-7).ToShortDateString();//开始时间
            EndDate.Text = DateTime.Now.ToShortDateString();//结束时间
            DateTime dpStart = dpEndDate.AddDays(-7);//一个星期前开始 开始时间
            if (sbFilter.Length != 0)
            {
                sbFilter.Append(" and ");
            }
            sbFilter.Append("PLANTIME>=@" + searchParas.Count().ToString());
            searchParas.Add(dpStart);
            sbFilter.Append(" and ");
            sbFilter.Append(" PLANTIME <=@" + searchParas.Count().ToString());
            searchParas.Add(dpEndDate);
            filterString = sbFilter.ToString();
            #endregion
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
            this.Loaded += new RoutedEventHandler(FrmCalendarManagement_Loaded);
            ToolBar.BtnView.Click += new RoutedEventHandler(BtnView_Click);
        }

        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CALENDAR> selCalendarList = GetSelectList();
            if (selCalendarList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            CFrmCalendarInfo calendarInfo = new CFrmCalendarInfo(selCalendarList[0], "view");
            EntityBrowser browser = new EntityBrowser(calendarInfo);
            browser.FormType = FormTypes.Browse;
            browser.MinWidth = 560;
            browser.MinHeight = 425;
            //browser.ReloadDataEvent += new EntityBrowser.refreshGridView(calendarInfo_OkClick);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }

        void FrmCalendarManagement_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_OA_CALENDAR");
            List<string> listSearch = new List<string>();
            listSearch.Add("最近一周记录");
            listSearch.Add("最近一个月记录");
            dateTimeSearch.ItemsSource = listSearch;
            dateTimeSearch.SelectedIndex = 0;
        }
        private void dg_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dg, e.Row, "T_OA_CALENDAR");

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }
        bool isPageChange = false;
        void dataPager_PageIndexChanged(object sender, EventArgs e)
        {
            if (isPageChange)
            {
                GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
            }
            isPageChange = true;
        }

        void calendarManagement_DelCalendarListCompleted(object sender, DelCalendarListCompletedEventArgs e)
        {
            if (e.Result == 1)
            {
                GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
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
            //GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }

        /// <summary>
        /// 选择日期
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cndDateCanlendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }

        #region 增删改按钮
        /// <summary>
        /// 新建按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            DateTime selDateTime = new DateTime();
            if (cndDateCanlendar.SelectedDate != null)
            {
                selDateTime = cndDateCanlendar.SelectedDate.Value;
            }
            else
            {
                selDateTime = System.DateTime.Now;
            }
            CFrmCalendarInfo calendarInfo = new CFrmCalendarInfo(selDateTime, "add");
            EntityBrowser browser = new EntityBrowser(calendarInfo);
            browser.MinWidth = 560;
            browser.MinHeight = 425;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(calendarInfo_OkClick);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
        }
        /// <summary>
        /// 删除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CALENDAR> selectList = GetSelectList();
            if (selectList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonByOperationControl(selectList[0], OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    try
                    {
                        calendarManagement.DelCalendarListAsync(selectList);
                    }
                    catch
                    {

                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
            }
        }
        /// <summary>
        /// 获取选中的信息
        /// </summary>
        /// <returns></returns>
        private ObservableCollection<T_OA_CALENDAR> GetSelectList()
        {
            if (dg.ItemsSource != null)
            {
                ObservableCollection<T_OA_CALENDAR> selectList = new ObservableCollection<T_OA_CALENDAR>();
                foreach (object obj in dg.SelectedItems)
                    selectList.Add((T_OA_CALENDAR)obj);
                if (selectList != null && selectList.Count > 0)
                {
                    return selectList;
                }
            }
            return null;
        }

        /// <summary>
        /// 修改按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<T_OA_CALENDAR> selCalendarList = GetSelectList();
            if (selCalendarList == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }
            if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonByOperationControl(selCalendarList[0], OperationType.Edit, Common.CurrentLoginUserInfo.EmployeeID))
            {
                CFrmCalendarInfo calendarInfo = new CFrmCalendarInfo(selCalendarList[0], "update");
                EntityBrowser browser = new EntityBrowser(calendarInfo);
                browser.MinWidth = 560;
                browser.MinHeight = 425;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(calendarInfo_OkClick);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("YOUDONOTHAVEPERMISSIONTOOPERATETHEDATA"));
            }
        }

        void calendarInfo_OkClick()
        {
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }
        #endregion

        #region 获取数据
        private void calendarManagement_GetCalendarListByUserIDCompleted(object sender, GetCalendarListByUserIDCompletedEventArgs e)
        {
            loadbar.Stop();
            List<T_OA_CALENDAR> calendarList = null;
            System.DateTime myDateTime = System.DateTime.Now;
            if (e.Result != null)
            {
                calendarList = e.Result.ToList();
            }
            BindDgv(calendarList, e.pageCount);
        }
        private SmtOAPersonOfficeClient calendarManagement = new SmtOAPersonOfficeClient();

        /// <summary>
        /// 用户名 查日程
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="selDateTime"></param>
        private void GetCalendarListSelectDate(DateTime dateTime, int pageIndex, string filter, ObservableCollection<object> paras, string orderString)//
        {
            loadbar.Start();
            int pageCount = 0;
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            pageIndex++;
            calendarManagement.GetCalendarListByUserIDAsync(pageIndex, dataPager.PageSize, orderString, filter, paras, pageCount, Common.CurrentLoginUserInfo.EmployeeID);
        }
        /// <summary>
        /// 日期筛选
        /// </summary>
        /// <param name="selDateTime"></param>
        private List<T_OA_CALENDAR> GetCalendarListByDate(DateTime selDateTime, List<T_OA_CALENDAR> calendarList)
        {
            if (calendarList != null)
            {
                List<T_OA_CALENDAR> selCalendarListByDate = new List<T_OA_CALENDAR>();
                foreach (T_OA_CALENDAR calendarInfo in calendarList)
                {
                    if (calendarInfo.REPARTREMINDER.ToString().Trim() == "NOTHING" && Convert.ToDateTime(calendarInfo.REMINDERRMODEL).ToShortDateString() == selDateTime.ToShortDateString())
                    {
                        selCalendarListByDate.Add(calendarInfo);
                    }
                    if (calendarInfo.REPARTREMINDER.ToString().Trim() == "DAY")
                    {
                        DateTime dtModel = Convert.ToDateTime(calendarInfo.REMINDERRMODEL);
                        DateTime dtNewDateTime = new DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, dtModel.Hour, dtModel.Minute, 0);
                        if (dtNewDateTime > DateTime.Now)//判断是是否过期
                        {
                            selCalendarListByDate.Add(calendarInfo);
                        }
                    }
                    if (calendarInfo.REPARTREMINDER.ToString().Trim() == "WEEK" && calendarInfo.REMINDERRMODEL.Trim() == selDateTime.DayOfWeek.ToString())
                    {
                        selCalendarListByDate.Add(calendarInfo);
                    }
                    if (calendarInfo.REPARTREMINDER.ToString().Trim() == "MONTH" && Convert.ToInt32(calendarInfo.REMINDERRMODEL) == selDateTime.Day)
                    {
                        selCalendarListByDate.Add(calendarInfo);
                    }
                    if (calendarInfo.REPARTREMINDER.ToString().Trim() == "YEAR" && Convert.ToDateTime(calendarInfo.REMINDERRMODEL.Trim()).Month == selDateTime.Month && Convert.ToDateTime(calendarInfo.REMINDERRMODEL.Trim()).Day == selDateTime.Day)
                    {
                        selCalendarListByDate.Add(calendarInfo);
                    }
                }
                return selCalendarListByDate;
            }
            return null;
        }
        #endregion

        #region 绑定网格
        /// <summary>
        /// 列表显示
        /// </summary>
        private void BindDgv(List<T_OA_CALENDAR> dataCalendarList, int pageCount)
        {
            GridHelper.HandleDataPageDisplay(dataPager, pageCount);
            if (dataCalendarList != null && dataCalendarList.Count > 0)
            {
                PagedCollectionView pcv = new PagedCollectionView(dataCalendarList);
                Type a = ((PagedCollectionView)pcv).CurrentItem.GetType();
                pcv.PageSize = 20;
                dataPager.DataContext = pcv;
                dg.ItemsSource = pcv;
            }
            else
            {
                dg.ItemsSource = null;
            }
        }
        #endregion

        #region 设置默认值
        //private DateTime selectDateTime;
        //private void SetDefaultDate()
        //{
        //    if (cmbSelectDate != null)
        //    {
        //        cmbSelectDate.Items.Clear();
        //    }
        //    if (selectDateTime == null)
        //    {
        //        cmbSelectDate.Items.Add(System.DateTime.Now.ToShortDateString());
        //    }
        //    else
        //    {
        //        cmbSelectDate.Items.Add(selectDateTime.ToShortDateString());
        //    }
        //    cmbSelectDate.SelectedIndex = 0;
        //}
        //private void SetDefaultYearDate()
        //{
        //    if (cmbSelectDate != null)
        //    {
        //        cmbSelectDate.Items.Clear();
        //    }
        //    if (selectDateTime == null)
        //    {
        //        cmbSelectDate.Items.Add(System.DateTime.Now.Month + "-" + System.DateTime.Now.Day);
        //    }
        //    else
        //    {
        //        cmbSelectDate.Items.Add(selectDateTime.Month + "-" + selectDateTime.Day);
        //    }
        //    cmbSelectDate.SelectedIndex = 0;
        //}
        #endregion

        private ObservableCollection<object> searchParas = null;
        private string filterString = null;
        private string orderString = "UPDATEDATE desc"; //排序字段
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {            //已将日期选择移至查询里，查询时要加入日期条件
            System.Text.StringBuilder sbFilter = new System.Text.StringBuilder();  //查询过滤条件 
            searchParas = new ObservableCollection<object>();
            if (!string.IsNullOrEmpty(cndDateCanlendar.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME>=@" + searchParas.Count().ToString());
                searchParas.Add(Convert.ToDateTime(cndDateCanlendar.Text));
            }
            if (!string.IsNullOrEmpty(cndDateCanlendar.Text) && string.IsNullOrEmpty(EndDate.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PERIODENDDATECANNOTBEEMPTY"));
                return;
            }
            if (!string.IsNullOrEmpty(EndDate.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME <=@" + searchParas.Count().ToString());
                searchParas.Add(Convert.ToDateTime(EndDate.Text.Trim()).AddHours(23).AddMinutes(59));
            }
            if (!string.IsNullOrEmpty(EndDate.Text) && string.IsNullOrEmpty(cndDateCanlendar.Text))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PERIODSTARTDATECANNOTBEEMPTY"));
                return;
            }
            //创建者
            if (!string.IsNullOrEmpty(txtOwnerName.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("CREATEUSERNAME==@" + searchParas.Count().ToString());
                searchParas.Add(txtOwnerName.Text);
            }
            if (cmbStyle.SelectedIndex > -1)
            {
                string alarmFlag = null;
                switch (cmbStyle.SelectedIndex)
                {
                    case 0:
                        alarmFlag = "NOTHING";//一次提醒
                        break;
                    case 1:
                        alarmFlag = "DAY";//每天提醒
                        break;
                    case 2:
                        alarmFlag = "WEEK";//每周提醒
                        break;
                    case 3:
                        alarmFlag = "MONTH";//每月提醒
                        break;
                    case 4:
                        alarmFlag = "YEAR";//每年提醒
                        break;
                }
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("REPARTREMINDER==@" + searchParas.Count().ToString());
                searchParas.Add(alarmFlag);
            }
            filterString = sbFilter.ToString();
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            cndDateCanlendar.Text = string.Empty;
            EndDate.Text = string.Empty;
            txtOwnerName.Text = string.Empty;
            filterString = null;
            searchParas = null;
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }


        #region 选择人员
        private void btnLookUpOwner_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();
                    SearchUserID = userInfo.ObjectID;
                    txtOwnerName.Text = userInfo.ObjectName.ToString();
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        /// <summary>
        /// 根据一个星期或是一个月进行查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimeSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox dateTimeSearch = Utility.FindChildControl<ComboBox>(controlsToolkitTUV, "dateTimeSearch");
            DateTime dpEndDate = DateTime.Now;
            System.Text.StringBuilder sbFilter = new System.Text.StringBuilder();  //查询过滤条件 
            searchParas = new ObservableCollection<object>();
            filterString = null;
            if (dateTimeSearch != null)
            {
                if (dateTimeSearch.SelectedItem != null)
                {
                    string strName = dateTimeSearch.SelectedItem as string;

                    if (strName == "最近一周记录")
                    {
                        cndDateCanlendar.Text = DateTime.Now.AddDays(-7).ToShortDateString();//开始时间
                        EndDate.Text = DateTime.Now.ToShortDateString();//结束时间
                        if (sbFilter.Length != 0)
                        {
                            sbFilter.Append(" and ");
                        }
                        sbFilter.Append("PLANTIME>=@" + searchParas.Count().ToString());
                        DateTime dpStart = dpEndDate.AddDays(-7).AddHours(23).AddMinutes(-58);//一个星期前开始
                        searchParas.Add(dpStart);
                        sbFilter.Append(" and ");
                        sbFilter.Append(" PLANTIME <=@" + searchParas.Count().ToString());
                        searchParas.Add(dpEndDate);
                        filterString = sbFilter.ToString();
                    }
                    else if (strName == "最近一个月记录")
                    {
                        cndDateCanlendar.Text = DateTime.Now.AddMonths(-1).ToShortDateString();//开始时间
                        EndDate.Text = DateTime.Now.ToShortDateString();//结束时间
                        if (sbFilter.Length != 0)
                        {
                            sbFilter.Append(" and ");
                        }
                        sbFilter.Append("PLANTIME>=@" + searchParas.Count().ToString());
                        DateTime dpStart = dpEndDate.AddMonths(-1).AddHours(-23).AddMinutes(-58);//一个月前开始
                        searchParas.Add(dpStart);
                        sbFilter.Append(" and ");
                        sbFilter.Append(" PLANTIME <=@" + searchParas.Count().ToString());
                        searchParas.Add(dpEndDate);
                        filterString = sbFilter.ToString();
                    }
                }
            }
            else
            {
                cndDateCanlendar.Text = DateTime.Now.AddDays(-7).ToShortDateString();//开始时间
                EndDate.Text = DateTime.Now.ToShortDateString();//结束时间
                DateTime dpStart = dpEndDate.AddDays(-7);//一个星期前开始 开始时间
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("PLANTIME>=@" + searchParas.Count().ToString());
                searchParas.Add(dpStart);
                sbFilter.Append(" and ");
                sbFilter.Append(" PLANTIME <=@" + searchParas.Count().ToString());
                searchParas.Add(dpEndDate);
                //filterString = sbFilter.ToString();
            }
            //创建者
            if (!string.IsNullOrEmpty(txtOwnerName.Text))
            {
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("CREATEUSERNAME==@" + searchParas.Count().ToString());
                searchParas.Add(txtOwnerName.Text);
            }
            if (cmbStyle.SelectedIndex > -1)
            {
                string alarmFlag = null;
                switch (cmbStyle.SelectedIndex)
                {
                    case 0:
                        alarmFlag = "NOTHING";//一次提醒
                        break;
                    case 1:
                        alarmFlag = "DAY";//每天提醒
                        break;
                    case 2:
                        alarmFlag = "WEEK";//每周提醒
                        break;
                    case 3:
                        alarmFlag = "MONTH";//每月提醒
                        break;
                    case 4:
                        alarmFlag = "YEAR";//每年提醒
                        break;
                }
                if (sbFilter.Length != 0)
                {
                    sbFilter.Append(" and ");
                }
                sbFilter.Append("REPARTREMINDER==@" + searchParas.Count().ToString());
                searchParas.Add(alarmFlag);
            }
            filterString = sbFilter.ToString();
            GetCalendarListSelectDate(System.DateTime.Now, dataPager.PageIndex, filterString, searchParas, "CREATEDATE descending");
        }

        /// <summary>
        /// 重置，清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtOwnerName.Text = string.Empty;
            cndDateCanlendar.Text = string.Empty;
            EndDate.Text = string.Empty;
        }

    }
}