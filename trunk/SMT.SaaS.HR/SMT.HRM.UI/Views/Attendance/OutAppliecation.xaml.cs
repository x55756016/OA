using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.HRM.UI.Form.Attendance;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.HRM.UI.OutApplyWS;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class OutAppliecation : BasePage, IClient
    {
        #region 全局变量
        public string Checkstate { get; set; }
        OutAppliecrecordServiceClient clientAtt = new OutAppliecrecordServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        private string departmentid;
        #endregion

        #region 初始化
        public OutAppliecation()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OverTime_Loaded);
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnReSubmit.Visibility = System.Windows.Visibility.Collapsed;
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
            clientAtt.EmployeeOutApplyRecordPagingCompleted += clientAtt_EmployeeOutApplyRecordPagingCompleted;
            clientAtt.DeleteOutApplyCompleted += clientAtt_DeleteOutApplyCompleted;
        }

        void clientAtt_DeleteOutApplyCompleted(object sender, DeleteOutApplyCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "删除员工外出申请成功");
            }

            LoadData();
        }

        void clientAtt_EmployeeOutApplyRecordPagingCompleted(object sender, EmployeeOutApplyRecordPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<T_HR_OUTAPPLYRECORD> list = new List<T_HR_OUTAPPLYRECORD>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgOTList.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }

            loadbar.Stop();
        }

        void OverTime_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);
            if (string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            {
                dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            }

            if (string.IsNullOrWhiteSpace(dpClockInRdEndDate.Text))
            {
                dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            }

            RegisterEvents();
            GetEntityLogo("T_HR_OUTAPPLYRECORD");
            BindComboBox();
        }
        #endregion

        #region 初始化及构造
        #endregion

        #region 增删改查

        #region 查询刷新

        /// <summary>
        /// 触发绑定DataGrid事件
        /// </summary>
        private void LoadData()
        {
            int pageCount = 0;
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "EMPLOYEENAME==@" + paras.Count().ToString();
                    paras.Add(ent.EMPLOYEECNAME);
                }
            }

            if (this.dpClockInRdStartDate.Text != null && this.dpClockInRdEndDate.Text != null)
            {
                //开始时间
                DateTime startDate = DateTime.Parse(this.dpClockInRdStartDate.Text);
                //结束时间
                DateTime endDate = DateTime.Parse(this.dpClockInRdEndDate.Text);
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "STARTDATE >=@" + paras.Count().ToString();
                paras.Add(startDate);

                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ENDDATE <=@" + paras.Count().ToString();
                paras.Add(endDate);
            }

            clientAtt.EmployeeOutApplyRecordPagingAsync(dataPager.PageIndex, dataPager.PageSize, "STARTDATE", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            loadbar.Start();
        }

        /// <summary>
        /// 提交子窗口的表单后，回刷父页面
        /// </summary>
        void browser_ReloadDataEvent()
        {
            LoadData();
        }


        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
        #endregion

        #region 新建
        /// <summary>
        /// 弹出表单子窗口，以便新增加班申请
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param> 
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string strOverTimeRecordID = string.Empty;
            OutAppliecationForm formOverTime = new OutAppliecationForm(FormTypes.New, strOverTimeRecordID);
            EntityBrowser entBrowser = new EntityBrowser(formOverTime);
            entBrowser.FormType = FormTypes.New;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //Utility.CreateFormFromEngine("F66FB573-5194-4732-BF51-B7745079C050", "SMT.HRM.UI.Form.Attendance.AttendMonthlyBalanceAudit", "Audit");
        }
        #endregion

        #region 删除

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgOTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgOTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (object ovj in dgOTList.SelectedItems)
            {
                T_HR_OUTAPPLYRECORD ent = ovj as T_HR_OUTAPPLYRECORD;
                if (ent == null)
                {
                    continue;
                }

                if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEAUDITERROR"));
                    break;
                }

                ids.Add(ent.OUTAPPLYID);
            }

            string Result = "";
            ComfirmWindow delComfirm = new ComfirmWindow();
            delComfirm.OnSelectionBoxClosed += (obj, result) =>
            {
                clientAtt.DeleteOutApplyAsync(ids);
            };
            delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);



        }
        #endregion

        #region 查看

        /// <summary>
        /// 查看
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            if (dgOTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgOTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_OUTAPPLYRECORD tmpEnt = dgOTList.SelectedItems[0] as T_HR_OUTAPPLYRECORD;

            OutAppliecationForm form = new OutAppliecationForm(FormTypes.Browse, tmpEnt.OUTAPPLYID);

            EntityBrowser entBrowser = new EntityBrowser(form);
            entBrowser.FormType = FormTypes.Browse;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        #endregion

        #region 修改

        /// <summary>
        /// 弹出表单子窗口，以便编辑指定加班申请记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {

            if (dgOTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgOTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_OUTAPPLYRECORD tmpEnt = dgOTList.SelectedItems[0] as T_HR_OUTAPPLYRECORD;

            //修改 如果是已审核的单据 提示不容许修改
            if (tmpEnt.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEAUDITERROR"));
                MessageBox.Show("已审核的单据不容许修改.");
                return;
            }

            OutAppliecationForm form = new OutAppliecationForm(FormTypes.Edit, tmpEnt.OUTAPPLYID);

            EntityBrowser entBrowser = new EntityBrowser(form);
            entBrowser.FormType = FormTypes.Edit;
            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        #endregion

        #endregion

        #region 页面控件事件
        /// <summary>
        /// 绑定审核状态下拉列表
        /// </summary>
        void BindComboBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                //审核状态绑定
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            }
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }

        /// <summary>
        /// 重置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIsNull_Click(object sender, RoutedEventArgs e)
        {
            this.lkEmpName.DataContext = null;

            DateTime dtNow = DateTime.Now;
            int iMaxDay = DateTime.DaysInMonth(dtNow.Year, dtNow.Month);
            if (string.IsNullOrWhiteSpace(dpClockInRdStartDate.Text))
            {
                dpClockInRdStartDate.Text = dtNow.ToString("yyyy-M") + "-1";
            }

            if (string.IsNullOrWhiteSpace(dpClockInRdEndDate.Text))
            {
                dpClockInRdEndDate.Text = dtNow.ToString("yyyy-M") + "-" + iMaxDay.ToString();
            }
            //this.lkLeaveTypeName.DataContext = null;

            //this.nuYear.Value = DateTime.Now.Year;
            //this.nuMonth.Value = DateTime.Now.Month;
        }

        /// <summary>
        /// 根据选取的审核状态，过滤DataGrid中的记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY dict = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
            if (dict != null)
            {
                Checkstate = dict.DICTIONARYVALUE.ToString();
                Utility.SetToolBarButtonByCheckState(dict.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_OUTAPPLYRECORD");
                toolbar1.btnReSubmit.Visibility = System.Windows.Visibility.Collapsed;//隐藏重新提交按钮
                LoadData();
            }
        }

        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmpName_FindClick(object sender, EventArgs e)
        {
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                if (ent != null)
                {
                    lkEmpName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 加载图片列
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOTList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgOTList, e.Row, "T_HR_OUTAPPLYRECORD");
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dpClockInRdEndDate.Text != null && dpClockInRdStartDate.Text != null)
            {
                DateTime EndDate = DateTime.Parse(dpClockInRdEndDate.Text);
                DateTime StartDat = DateTime.Parse(dpClockInRdStartDate.Text);

                if (EndDate < StartDat)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATETIME"), Utility.GetResourceStr("DATECOMPARE", "ENDDATETIME,STARTDATETIME"));
                }
                else
                {
                    LoadData();
                }
            }
        }

        #endregion

        #region 审核

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            if (dgOTList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgOTList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            T_HR_OUTAPPLYRECORD tmpEnt = dgOTList.SelectedItems[0] as T_HR_OUTAPPLYRECORD;
            OutAppliecationForm form = new OutAppliecationForm(FormTypes.Audit, tmpEnt.OUTAPPLYID);
            EntityBrowser browser = new EntityBrowser(form);
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.FormType = FormTypes.Audit;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            clientAtt.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
