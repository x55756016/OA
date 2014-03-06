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

using SMT.HRM.UI.Form.Attendance;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.IO;

namespace SMT.HRM.UI.Views.Attendance
{
    public partial class EmployeeLeaveRecord : BasePage, IClient
    {
        #region 全局变量
        public string Checkstate { get; set; }
        AttendanceServiceClient client = new AttendanceServiceClient();
        private SMTLoading loadbar = new SMTLoading();
        private SaveFileDialog dialog = new SaveFileDialog();
        private bool? result;
        //请假类型
        public string strleaveType { get; set; }
        #endregion

        #region 初始化
        public EmployeeLeaveRecord()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(EmployeeLeaveRecord_Loaded);
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewTitles.TextTitle.Text = GetTitleFromURL(e.Uri.ToString());
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            PARENT.Children.Add(loadbar);

            nuYear.Value = DateTime.Now.Year;
            nuMonth.Value = DateTime.Now.Month;
            endYear.Value = DateTime.Now.Year;
            //client.EmployeeLeaveRecordPagingCompleted += new EventHandler<EmployeeLeaveRecordPagingCompletedEventArgs>(client_EmployeeLeaveRecordPagingCompleted);
            client.EmployeeLeaveRecordDeleteCompleted += new EventHandler<EmployeeLeaveRecordDeleteCompletedEventArgs>(client_EmployeeLeaveRecordDeleteCompleted);
            client.EmployeeLeaveRecordPagedCompleted += new EventHandler<EmployeeLeaveRecordPagedCompletedEventArgs>(client_EmployeeLeaveRecordPagedCompleted);
            client.ExportEmployeeLeaveRecordReportsCompleted += new EventHandler<ExportEmployeeLeaveRecordReportsCompletedEventArgs>(client_ExportEmployeeLeaveRecordReportsCompleted);
            toolbar1.btnRefresh.Click += new RoutedEventHandler(btnRefresh_Click);
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnEdit.Click += new RoutedEventHandler(btnEdit_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
            toolbar1.BtnView.Click += new RoutedEventHandler(BtnView_Click);
            toolbar1.btnAudit.Click += new RoutedEventHandler(btnAudit_Click);
            toolbar1.btnReSubmit.Click += new RoutedEventHandler(btnReSubmit_Click);
            toolbar1.btnOutExcel.Visibility = Visibility;
            toolbar1.btnOutExcel.Click += new RoutedEventHandler(btnOutExcel_Click);
            toolbar1.cbxCheckState.SelectionChanged += new SelectionChangedEventHandler(cbxCheckState_SelectionChanged);
        }
        /// <summary>
        /// 导出事件
        /// </summary>
        /// <param name="sender">发送事件对象</param>
        /// <param name="e">发送者</param>
        void client_ExportEmployeeLeaveRecordReportsCompleted(object sender, ExportEmployeeLeaveRecordReportsCompletedEventArgs e)
        {
            loadbar.Stop();
            if (result == true)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        using (Stream stream = dialog.OpenFile())
                        {
                            stream.Write(e.Result, 0, e.Result.Length);
                        }
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("导出成功"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("没有数据可导出"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
            }
        }

        void client_EmployeeLeaveRecordPagedCompleted(object sender, EmployeeLeaveRecordPagedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<V_EmpLeaveRdInfo> list = new List<V_EmpLeaveRdInfo>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgEmpLeaveRdList.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnOutExcel_Click(object sender, RoutedEventArgs e)
        {
            //ExportToCSV.ExportDataGridSaveAs(dgEmpLeaveRdList);
            dialog.DefaultExt = ".xls";
            dialog.Filter = "MS Excel Files|*.xls";
            dialog.FilterIndex = 1;

            result = dialog.ShowDialog();
            if (result.Value == true)
            {
                int pageCount = 0;
                string filter = "";

                //string filter2 = "";
                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
                string startDate = string.Empty, recorderDate = string.Empty,employeeID = string.Empty, leaveTypeSetID = string.Empty;//起始时间和结束时间
                if (lkEmpName.DataContext != null)
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                    if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                    {
                        employeeID = ent.EMPLOYEEID;
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "EMPLOYEEID==@" + paras.Count().ToString();
                        paras.Add(ent.EMPLOYEEID);
                    }
                }

                if (this.lkLeaveTypeName.DataContext != null)
                {
                    T_HR_LEAVETYPESET entLeaveType = this.lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;

                    leaveTypeSetID = entLeaveType.LEAVETYPESETID;
                    if (!string.IsNullOrEmpty(entLeaveType.LEAVETYPESETID))
                    {
                        if (!string.IsNullOrEmpty(filter))
                        {
                            filter += " and ";
                        }
                        filter += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paras.Count().ToString();
                        paras.Add(entLeaveType.LEAVETYPESETID);
                    }
                }

                startDate = nuYear.Value.ToString() + "-" + startMonth.Value.ToString() + "-1";
                recorderDate = endYear.Value.ToString() + "-" + (nuMonth.Value + 1).ToString() + "-1";
                if (nuMonth.Value == 12)
                {
                    recorderDate = (endYear.Value + 1).ToString() + "-1-1";
                }
                if (DateTime.Parse(startDate) <= DateTime.Parse("1900-1-1"))
                {
                    startDate = string.Empty;
                }
                if (DateTime.Parse(recorderDate) <= DateTime.Parse("1900-1-1"))
                {
                    recorderDate = string.Empty;
                }
                //起始时间
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "STARTDATETIME>=@" + paras.Count().ToString();
                    paras.Add(Convert.ToDateTime(startDate));
                }
                //结束时间
                if (!string.IsNullOrEmpty(recorderDate))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "ENDDATETIME<@" + paras.Count().ToString();
                    paras.Add(Convert.ToDateTime(recorderDate));
                }

                if (toolbar1.cbxCheckState.SelectedItem != null)
                {
                    T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                    Checkstate = entDic.DICTIONARYVALUE.ToString();
                }
                client.ExportEmployeeLeaveRecordReportsAsync(1, int.MaxValue, "STARTDATETIME", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, startDate, recorderDate, employeeID, leaveTypeSetID);

                loadbar.Start();
            }
        }

        /// <summary>
        /// 加载审核状态列表
        /// </summary>
        private void BindComboxBox()
        {
            if (toolbar1.cbxCheckState.ItemsSource == null)
            {
                Utility.CbxItemBinder(toolbar1.cbxCheckState, "CHECKSTATE", Convert.ToInt32(CheckStates.All).ToString());
            }
        }

        /// <summary>
        /// 根据查询条件，调用WCF服务获取数据，以便加载数据列表
        /// </summary>
        private void BindGrid()
        {
            int pageCount = 0;
            string filter = "";

            //string filter2 = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            string startDate = string.Empty, recorderDate = string.Empty, employeeID = string.Empty, leaveTypeSetID = string.Empty;//起始时间和结束时间
            if (lkEmpName.DataContext != null)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lkEmpName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (!string.IsNullOrEmpty(ent.EMPLOYEEID))
                {
                    employeeID = ent.EMPLOYEEID;
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "EMPLOYEEID==@" + paras.Count().ToString();
                    paras.Add(ent.EMPLOYEEID);
                }
            }

            if (this.lkLeaveTypeName.DataContext !=null)
            {
                T_HR_LEAVETYPESET entLeaveType = this.lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;

                leaveTypeSetID = entLeaveType.LEAVETYPESETID;
                if (!string.IsNullOrEmpty(entLeaveType.LEAVETYPESETID))
                {
                    if (!string.IsNullOrEmpty(filter))
                    {
                        filter += " and ";
                    }
                    filter += "T_HR_LEAVETYPESET.LEAVETYPESETID==@" + paras.Count().ToString();
                    paras.Add(entLeaveType.LEAVETYPESETID);
                }
            }

            startDate = nuYear.Value.ToString() + "-" + startMonth.Value.ToString() + "-1";
            recorderDate = endYear.Value.ToString() + "-" + (nuMonth.Value+1).ToString() + "-1";
            if (nuMonth.Value==12)
            {
                 recorderDate = (endYear.Value+1).ToString() + "-1-1";
            }
            if (DateTime.Parse(startDate) <= DateTime.Parse("1900-1-1"))
            {
                startDate = string.Empty;
            }
            if (DateTime.Parse(recorderDate) <= DateTime.Parse("1900-1-1"))
            {
                recorderDate = string.Empty;
            }
            //起始时间
            if (!string.IsNullOrEmpty(startDate))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "STARTDATETIME>=@" + paras.Count().ToString();
                paras.Add(Convert.ToDateTime(startDate));
            }
            //结束时间
            if (!string.IsNullOrEmpty(recorderDate))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "ENDDATETIME<@" + paras.Count().ToString();
                paras.Add(Convert.ToDateTime(recorderDate));
            }

            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                Checkstate = entDic.DICTIONARYVALUE.ToString();
            }

            client.EmployeeLeaveRecordPagedAsync(dataPager.PageIndex, dataPager.PageSize, "STARTDATETIME", filter, paras, pageCount, Checkstate, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, startDate, recorderDate, employeeID,leaveTypeSetID);
            loadbar.Start();
        }
        /// </summary>
        void browser_ReloadDataEvent()
        {
            BindGrid();
        }
        #endregion

        #region 事件

        void EmployeeLeaveRecord_Loaded(object sender, RoutedEventArgs e)
        {
            GetEntityLogo("T_HR_EMPLOYEELEAVERECORD");
            RegisterEvents();
            BindComboxBox();
        }

        /// <summary>
        /// 加载请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeLeaveRecordPagingCompleted(object sender, EmployeeLeaveRecordPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<V_EmpLeaveRdInfo> list = new List<V_EmpLeaveRdInfo>();
                if (e.Result != null)
                {
                    list = e.Result.ToList();
                }
                dgEmpLeaveRdList.ItemsSource = list;

                dataPager.PageCount = e.pageCount;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            loadbar.Stop();
        }

        /// <summary>
        /// 返回删除记录后的消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeLeaveRecordDeleteCompleted(object sender, EmployeeLeaveRecordDeleteCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "EMPLOYEELEAVERECORD"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            BindGrid();
        }

        /// <summary>
        /// 选取员工
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
        /// 根据审核状态下拉列表选择项，控制请假记录显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void cbxCheckState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (toolbar1.cbxCheckState.SelectedItem != null)
            {
                T_SYS_DICTIONARY entDic = toolbar1.cbxCheckState.SelectedItem as T_SYS_DICTIONARY;
                Utility.SetToolBarButtonByCheckState(entDic.DICTIONARYVALUE.Value.ToInt32(), toolbar1, "T_HR_EMPLOYEELEAVERECORD");
                BindGrid();
            }
        }

        /// <summary>
        /// 查询，分页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        /// <summary>
        /// 重置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIsNull_Click(object sender, RoutedEventArgs e)
        {
            this.lkEmpName.DataContext = null;
            this.lkLeaveTypeName.DataContext = null;

            this.nuYear.Value = DateTime.Now.Year;
            this.endYear.Value = DateTime.Now.Year;
            this.startMonth.Value = DateTime.Now.Month;
            this.nuMonth.Value = DateTime.Now.Month;
        }

        /// <summary>
        /// Grid首列加载图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgEmpLeaveRdList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SetRowLogo(dgEmpLeaveRdList, e.Row, "T_HR_EMPLOYEELEAVERECORD");
        }

        /// <summary>
        ///  刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindGrid();
        }

        #region 查看，添加，修改，删除, 重新提交
        /// <summary>
        /// 弹出表单子窗口，以便新增请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            //string strCurDateMonth = "2013-09";
            //client.CalculateEmployeeAttendanceMonthlyByEmployeeIDAsync(strCurDateMonth, "98b3e4e0-1e62-492c-97c7-6a5a1599d936");


            EmployeeLeaveRecordForm form = new EmployeeLeaveRecordForm(FormTypes.New, "");
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;
            entBrowser.FormType = FormTypes.New;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            //Utility.CreateFormFromEngine("fefa2813-9117-4c9e-b098-dfa89ced2574", "SMT.HRM.UI.Form.Attendance.EmployeeLeaveRecordForm", "Audit");
        }

        /// <summary>
        /// 弹出表单子窗口，以便浏览请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnView_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "VIEW"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            V_EmpLeaveRdInfo tmpEnt = dgEmpLeaveRdList.SelectedItems[0] as V_EmpLeaveRdInfo;

            EmployeeLeaveRecordForm form = new EmployeeLeaveRecordForm(FormTypes.Browse, tmpEnt.LEAVERECORDID);
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;
            entBrowser.FormType = FormTypes.Browse;

            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        void btnReSubmit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "ReSubmit"));
                return;
            }


            V_EmpLeaveRdInfo tmpEnt = dgEmpLeaveRdList.SelectedItems[0] as V_EmpLeaveRdInfo;

            EmployeeLeaveRecordForm form = new EmployeeLeaveRecordForm(FormTypes.Resubmit, tmpEnt.LEAVERECORDID);
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;
            entBrowser.FormType = FormTypes.Resubmit;
            //form.LeaveRecord = tmpEnt;
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 弹出表单子窗口，以便编辑请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string strAttendanceSolutionID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "EDIT"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }


            V_EmpLeaveRdInfo tmpEnt = dgEmpLeaveRdList.SelectedItems[0] as V_EmpLeaveRdInfo;

            if (tmpEnt.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "不是未提交的单据不可以修改", Utility.GetResourceStr("CONFIRMBUTTON"));
                return; 
            }
            EmployeeLeaveRecordForm form = new EmployeeLeaveRecordForm(FormTypes.Edit, tmpEnt.LEAVERECORDID);
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;
            entBrowser.FormType = FormTypes.Edit;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        /// <summary>
        /// 删除指定的签卡记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            ObservableCollection<string> ids = new ObservableCollection<string>();
            foreach (object ovj in dgEmpLeaveRdList.SelectedItems)
            {
                V_EmpLeaveRdInfo ent = ovj as V_EmpLeaveRdInfo;
                if (ent == null)
                {
                    continue;
                }

                if (ent.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DELETEAUDITERROR"));
                    break;
                }

                ids.Add(ent.LEAVERECORDID);
            }

            string Result = "";
            if (ids.Count > 0)
            {
                ComfirmWindow delComfirm = new ComfirmWindow();
                delComfirm.OnSelectionBoxClosed += (obj, result) =>
                {
                    client.EmployeeLeaveRecordDeleteAsync(ids);
                };
                delComfirm.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
        }

        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnAudit_Click(object sender, RoutedEventArgs e)
        {
            string strLeaveRecordID = string.Empty;
            if (dgEmpLeaveRdList.SelectedItems == null)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            if (dgEmpLeaveRdList.SelectedItems.Count == 0)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("SELECTERROR", "APPOVALBUTTON"), Utility.GetResourceStr("CONFIRMBUTTON"));
                return;
            }

            V_EmpLeaveRdInfo entEvectionRd = dgEmpLeaveRdList.SelectedItems[0] as V_EmpLeaveRdInfo;
            strLeaveRecordID = entEvectionRd.LEAVERECORDID;
            EmployeeLeaveRecordForm form = new EmployeeLeaveRecordForm(FormTypes.Audit, strLeaveRecordID);
            EntityBrowser entBrowser = new EntityBrowser(form);
            //Modified by: Sam
            //Date       : 2011-9-6
            //For        : 此处导致打开Form窗体会出现滚动条
            //form.MinWidth = 820;
            //form.MinHeight = 600;

            entBrowser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            entBrowser.FormType = FormTypes.Audit;
            entBrowser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #endregion

        #region IClient 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
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


        //<summary>
        //选择当前员工的请假类型
        //</summary>
        //<param name="sender"></param>
        //<param name="e"></param>
        private void lkLeaveTypeName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("LEAVETYPENAME", "LEAVETYPENAME");
            cols.Add("ISFREELEAVEDAY", "ISFREELEAVEDAY,ISCHECKED,DICTIONARYCONVERTER");
            //string filter = "";
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            //filter += " ISFREELEAVEDAY=@" + paras.Count().ToString() + "";
            //paras.Add("2");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET ent = lookup.SelectedObj as SMT.Saas.Tools.AttendanceWS.T_HR_LEAVETYPESET;

                if (ent != null)
                {
                    if (!string.IsNullOrEmpty(ent.POSTLEVELRESTRICT))
                    {
                        decimal dlevel = 0, dCheckLevel = 0;

                        //decimal.TryParse(tbEmpLevel.Text, out dlevel);
                        decimal.TryParse(ent.POSTLEVELRESTRICT, out dCheckLevel);

                        if (dlevel > dCheckLevel)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEELEAVERECORD"), Utility.GetResourceStr("LEAVETYPERESTRICT", "POSTLEVELRESTRICT"));
                            return;
                        }
                    }

                    //LeaveRecord = new Saas.Tools.AttendanceWS.T_HR_EMPLOYEELEAVERECORD();
                    //LeaveRecord.T_HR_LEAVETYPESET = ent;

                    this.lkLeaveTypeName.DataContext = ent;

                    strleaveType = ent.LEAVETYPESETID;

                    if (ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString() || ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
                    {
                        //toolbar1.IsEnabled = false;
                        //toolbar1.Visibility = System.Windows.Visibility.Collapsed;
                        //dgLevelDayList.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}
