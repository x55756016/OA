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

using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.AttendanceWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class EmployeeLeaveRecordForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量

        //产前检查假，特殊处理，每月只能选择一天，过月作废，不累加
        public string LeaveRecordType { set; get; }
        public FormTypes FormType { set; get; }
        public string LeaveRecordID { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        //休假记录
        public T_HR_EMPLOYEELEAVERECORD LeaveRecord { get; set; }
        //解决方案
        public T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }
        decimal dCurLevelDays = 0;
        AttendanceServiceClient clientAtt;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient perClient;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private bool canSubmit = false;//能否提交审核
        private bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        #endregion

        #region 初始化
        //Modified by:Sam
        //Date:2011-10-9
        //For:增加一个无参数的构造函数来实现待办任务新建单据
        public EmployeeLeaveRecordForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            LeaveRecordID = string.Empty;

            this.Loaded += new RoutedEventHandler(EmployeeLeaveRecordForm_Loaded);
        }

        public EmployeeLeaveRecordForm(FormTypes type, string strID)
        {
            InitializeComponent();
            //查看、编辑
            //T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            //LeaveRecordType = entLeave.LEAVETYPENAME;
            FormType = type;
            LeaveRecordID = strID;

            this.Loaded += new RoutedEventHandler(EmployeeLeaveRecordForm_Loaded);
        }

        void EmployeeLeaveRecordForm_Loaded(object sender, RoutedEventArgs e)
        {
            clientAtt = new AttendanceServiceClient();
            perClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            RegisterEvents();
            InitParas();
            UnVisibleGridToolControl();
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEELEAVERECORD");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "";
        }
        public void DoAction(string actionType)
        {
            switch (actionType)
            {
                case "0":
                    Save();
                    break;
                case "1":
                    Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        public void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
            }
        }
        #endregion

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEELEAVERECORD Info)
        {
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;


            decimal? postlevelValue = Convert.ToDecimal(tbEmpLevel.Text.Trim());
            string postLevelName = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == postlevelValue).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERPOSTNAME", tbOrgName.Text.Trim(), tbOrgName.Text.Trim()));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "POSTLEVEL", tbEmpLevel.Text.Trim(), postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "EMPLOYEEORGNAME", Info.EMPLOYEENAME, tbEmpName.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "EMPLOYEENAME", Info.EMPLOYEENAME, Info.EMPLOYEENAME));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "TOTALADJUSTLEAVEDAYS", nudTotalAdjustLeaveDays.Value.ToString(), nudTotalAdjustLeaveDays.Value.ToString()));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "DEDUCTDAYS", nudDeductDays.Value.ToString(), nudDeductDays.Value.ToString()));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "TOTALVACATIONDAYS", nudTotalVacationDays.Value.ToString(), nudTotalVacationDays.Value.ToString()));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "LEAVETYPESETID", Info.T_HR_LEAVETYPESET.LEAVETYPESETID, lkLeaveTypeName.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEELEAVERECORD", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //    AutoList.Add(basedata("T_HR_LEFTOFFICE", "LEFTOFFICECATEGORY", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYVALUE.ToString() : "0", LEFTOFFICECATEGORY != null ? LEFTOFFICECATEGORY.DICTIONARYNAME : ""));
            string a = mx.TableToXml(Info, null, StrSource, AutoList);

            return a;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTLEVEL", tbEmpLevel.Text.Trim());
            para.Add("OWNERPOSTNAME", tbOrgName.Text.Trim());
            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEELEAVERECORD>(LeaveRecord, para, "HR");
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, LeaveRecord);
            Utility.SetAuditEntity(entity, "T_HR_EMPLOYEELEAVERECORD", LeaveRecordID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            //entAttendanceSolutionAsign.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

            if (LeaveRecord.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
            {
                LeaveRecord.CHECKSTATE = state;
                Save();
            }
            else
            {
                clientAtt.AuditLeaveRecordAsync(LeaveRecordID, entAdjustleaves, state);
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (LeaveRecord != null)
            {
                state = LeaveRecord.CHECKSTATE;
                if (FormType == FormTypes.New)
                {
                    state = "";
                }
            }

            return state;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetLeaveRecordByIDCompleted += new EventHandler<GetLeaveRecordByIDCompletedEventArgs>(clientAtt_GetLeaveRecordByIDCompleted);
            clientAtt.GetAdjustLeaveDetailListByLeaveRecordIDCompleted += new EventHandler<GetAdjustLeaveDetailListByLeaveRecordIDCompletedEventArgs>(clientAtt_GetAdjustLeaveDetailListByLeaveRecordIDCompleted);

            clientAtt.GetAttendanceSolutionByEmployeeIDAndDateCompleted += new EventHandler<GetAttendanceSolutionByEmployeeIDAndDateCompletedEventArgs>(clientAtt_GetAttendanceSolutionByEmployeeIDAndDateCompleted);
            clientAtt.GetCurLevelDayCountByEmployeeIDCompleted += new EventHandler<GetCurLevelDayCountByEmployeeIDCompletedEventArgs>(clientAtt_GetCurLevelDayCountByEmployeeIDCompleted);
            clientAtt.GetRealLeaveDayByEmployeeIdAndDateCompleted += new EventHandler<GetRealLeaveDayByEmployeeIdAndDateCompletedEventArgs>(clientAtt_GetRealLeaveDayByEmployeeIdAndDateCompleted);
            clientAtt.GetCurLevelDaysByEmployeeIDAndLeaveFineTypeCompleted += new EventHandler<GetCurLevelDaysByEmployeeIDAndLeaveFineTypeCompletedEventArgs>(clientAtt_GetCurLevelDaysByEmployeeIDAndLeaveFineTypeCompleted);

            clientAtt.GetLeaveDaysHistoryCompleted += new EventHandler<GetLeaveDaysHistoryCompletedEventArgs>(clientAtt_GetLeaveDaysHistoryCompleted);
            //获取员工名称，并显示所在的公司架构
           // perClient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(perClient_GetEmployeeDetailByIDCompleted);
            perClient.GetEmpOrgInfoByIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs>(perClient_GetEmpOrgInfoByIDCompleted);
            clientAtt.EmployeeLeaveRecordAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_EmployeeLeaveRecordAddCompleted);
            clientAtt.EmployeeLeaveRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(clientAtt_EmployeeLeaveRecordUpdateCompleted);

            clientAtt.AuditLeaveRecordCompleted += new EventHandler<AuditLeaveRecordCompletedEventArgs>(clientAtt_AuditLeaveRecordCompleted);
            clientAtt.GetEmployeeLeaveRdListsByLeaveRecordIDCompleted += new EventHandler<GetEmployeeLeaveRdListsByLeaveRecordIDCompletedEventArgs>(clientAtt_GetEmployeeLeaveRdListsByLeaveRecordIDCompleted);
            toolbar1.btnNew.Content = "添加带薪假(冲减)";
            toolbar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            toolbar1.btnDelete.Click += new RoutedEventHandler(btnDelete_Click);
        }

        /// <summary>
        /// 获取销假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetEmployeeLeaveRdListsByLeaveRecordIDCompleted(object sender, GetEmployeeLeaveRdListsByLeaveRecordIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        txtRemark.Text += "\r\n\r\n";
                        e.Result.ForEach(item =>
                            {
                                txtRemark.Text += "该请假记录已销假，开始时间为：" + item.STARTDATETIME.ToString() + "  结束时间为：" + item.ENDDATETIME.ToString() + "  销假时长为：" + item.TOTALHOURS;
                            });
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("查找员工销假记录错误"));
            }
        }

        /// <summary>
        /// 隐藏当前页不需要使用的GridToolBar按钮
        /// </summary>
        private void UnVisibleGridToolControl()
        {
            toolbar1.btnEdit.Visibility = Visibility.Collapsed;
            toolbar1.btnRefresh.Visibility = Visibility.Collapsed;
            toolbar1.BtnView.Visibility = Visibility.Collapsed;
            //toolbar1.btnSumbitAudit.Visibility = Visibility.Collapsed;
            toolbar1.btnAudit.Visibility = Visibility.Collapsed;
            //toolbar1.btnAduitNoTPass.Visibility = Visibility.Collapsed;
            toolbar1.txtCheckStateName.Visibility = Visibility.Collapsed;
            toolbar1.cbxCheckState.Visibility = Visibility.Collapsed;

            toolbar1.retEdit.Visibility = Visibility.Collapsed;
            toolbar1.retRefresh.Visibility = Visibility.Collapsed;
            toolbar1.retRead.Visibility = Visibility.Collapsed;
            toolbar1.retDelete.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 控制显示的按钮
        /// </summary>
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEELEAVERECORD", LeaveRecord.OWNERID,
                    LeaveRecord.OWNERPOSTID, LeaveRecord.OWNERDEPARTMENTID, LeaveRecord.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 设置当前页面为浏览状态
        /// </summary>
        private void SetOnlyBrowse()
        {
            FormType = FormTypes.Browse;
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
            this.IsEnabled = false;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                LeaveRecord = new T_HR_EMPLOYEELEAVERECORD();

                string strEmployeeState = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeState;

                LeaveRecord.LEAVERECORDID = Guid.NewGuid().ToString();
                LeaveRecord.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                LeaveRecord.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;
                LeaveRecord.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                LeaveRecord.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();

                //权限控制
                LeaveRecord.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                LeaveRecord.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                LeaveRecord.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                LeaveRecord.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


                //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
                LeaveRecord.CREATEDATE = DateTime.Now;
                LeaveRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                LeaveRecord.UPDATEDATE = System.DateTime.Now;
                LeaveRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


                //赋初始值
                tbOrgName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                tbEmpName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                {
                    tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
                }

                tbEmpWorkAge.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.WorkingAge.ToString();
                tbEmpSex.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SexID;
                tbEmpLevel.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();

                this.DataContext = LeaveRecord;

                if (strEmployeeState == Convert.ToInt32(EmployeeState.OnTrial).ToString() || strEmployeeState == Convert.ToInt32(EmployeeState.Regular).ToString() || strEmployeeState == Convert.ToInt32(EmployeeState.OnLeaving).ToString())
                {
                    SetToolBar();
                }
                else
                {
                    this.IsEnabled = false;
                }
            }
            else
            {
                clientAtt.GetLeaveRecordByIDAsync(LeaveRecordID);
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
            if (FormType != FormTypes.Browse)
            {
                //Load事件之后，加载完后获取到父控件
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
                entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            }
        }

        /// <summary>
        ///  新增的保存事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            isSubmit = true;
            needsubmit = true;
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            Save();
        }


        /// <summary>
        /// 切换假期标准后，清空已填写的其他的内容
        /// </summary>
        private void ResetForm()
        {
            ResetDateTimePickerValue(dpStartDate);
            ResetDateTimePickerValue(dpEndDate);
            nudLeaveDay.Value = 0;
            nudLeaveHours.Value = 0;
            nudTotalDays.Value = 0;
            nudTotalAdjustLeaveDays.Value = 0;
            nudTotalVacationDays.Value = 0;
            nudDeductDays.Value = 0;
            txtLeaveReason.Text = string.Empty;
            txtRemark.Text = string.Empty;
            ObservableCollection<V_ADJUSTLEAVE> entList = new ObservableCollection<V_ADJUSTLEAVE>();
            dgLevelDayList.ItemsSource = entList;
        }

        /// <summary>
        /// 置空日期时间选择控件所填内容
        /// </summary>
        /// <param name="dtpDateTime"></param>
        private void ResetDateTimePickerValue(DateTimePicker dtpDateTime)
        {
            dtpDateTime.Value = null;
            DatePicker dpTempDate = Utility.FindChildControl<DatePicker>(dtpDateTime);
            dpTempDate.Text = string.Empty;

            TimePicker tpTempDate = Utility.FindChildControl<TimePicker>(dtpDateTime);
            tpTempDate.Value = null;
        }

        /// <summary>
        /// 计算放假时长
        /// </summary>
        private void CalculateDayCount()
        {
            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();

            nudTotalDays.Value = 0;

            if (dpStartDate.Value == null)
            {
                return;
            }

            if (dpEndDate.Value == null)
            {
                return;
            }

            dtStart = dpStartDate.Value.Value;
            dtEnd = dpEndDate.Value.Value;

            //if (dpStartDate.Value.Value >= dpEndDate.Value)
            //{
            //    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATETIME"), Utility.GetResourceStr("DATECOMPARE", "ENDDATETIME,STARTDATETIME"));
            //    MessageBox.Show("结束时间必须大于开始时间");
            //    return;
            //}

            if (dtEnd.CompareTo(dtStart) <=0)
            {
                return;
            }

            T_HR_LEAVETYPESET ent = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            if (ent == null)
            {
                return;
            }

            decimal dLeaveDay = 0, dLeaveTime = 0, dLeaveTotalTime = 0;
            decimal dLeaveYearDays = 0, dLeaveYearTimes = 0, dLeaveMonthDays = 0, dLeaveMonthTimes = 0,dLeaveSYearTimes=0;
            //第一次请假的时间
            DateTime dLeaveFistDate = DateTime.Parse("1900-1-1");
            //Modified by: Sam
            //Date:2011-09-01
            //For:原来的功能是在服务执行完得到冲减天数之后，判断如果没有冲减天数就设当前页面为只读
            //    但是，选择时间后会调很多服务，导致服务还没执行完，还没把冲减天数读出来，用户还可以点保存
            //    所以此处加了loading
            //带薪假

            RefreshUI(RefreshedTypes.ShowProgressBar);

            clientAtt.GetRealLeaveDayByEmployeeIdAndDateAsync(LeaveRecord.LEAVERECORDID, LeaveRecord.EMPLOYEEID, dtStart, dtEnd, dLeaveDay, dLeaveTime, dLeaveTotalTime);

            clientAtt.GetLeaveDaysHistoryAsync(ent.LEAVETYPESETID, LeaveRecord.LEAVERECORDID, LeaveRecord.EMPLOYEEID, dtStart, dtEnd, dLeaveYearTimes, dLeaveYearDays, dLeaveMonthTimes, dLeaveMonthDays, dLeaveFistDate, dLeaveSYearTimes);
        }

        /// <summary>
        /// 获取当前可用冲减调休天数，冲减天数及扣款天数
        /// </summary>
        private void GetCurLevelDays()
        {
            if (lkLeaveTypeName.DataContext == null)
            {
                return;
            }

            T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            
            if (entLeave.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
            {
                decimal WorkTimePerDay = 0, dLeaveTotalTime = 0;
                decimal.TryParse(tbWorkTimePerDay.Text, out WorkTimePerDay);
                decimal.TryParse(nudTotalDays.Value.ToString(), out dLeaveTotalTime);
                if (WorkTimePerDay > 0)
                {
                    nudDeductDays.Value = decimal.Round(dLeaveTotalTime / WorkTimePerDay, 1).ToDouble() - nudTotalAdjustLeaveDays.Value;
                }

                RefreshUI(RefreshedTypes.HideProgressBar);

            }
            else
            {
                if (dpStartDate.Value == null || dpEndDate.Value == null)
                {
                    return;
                }
                string strLeaveSetId = entLeave.LEAVETYPESETID;
                string strOwnerID = LeaveRecord.OWNERID;

                DateTime dtStartDate = dpStartDate.Value.Value;

                //DateTime startDate = DateTime.Parse("2010-1-1");
                ////如果是调休假期，那么开始时间不失效
                //if (LeaveRecordType == "1")
                //{
                //    dtStartDate = startDate;
                //}
                //DateTime dtStartDate = dpStartDate.Value.Value;
                DateTime dtEndDate = dpEndDate.Value.Value;
                //decimal dCurLevelDays = 0;
                clientAtt.GetCurLevelDaysByEmployeeIDAndLeaveFineTypeAsync(strOwnerID, LeaveRecord.LEAVERECORDID, strLeaveSetId, dtStartDate, dtEndDate, 0);
            }
        }

        /// <summary>
        /// 计算带薪假冲减天数，及当前请假时长应扣款天数
        /// </summary>
        private void CalculateDeductDaysByAdjustLeaveDetails()
        {
            ObservableCollection<V_ADJUSTLEAVE> ents = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;
            decimal dAdjustLeaveDays = 0;

            if (lkLeaveTypeName.DataContext != null)
            {
                T_HR_LEAVETYPESET entLeaveTypeSet = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
                if (entLeaveTypeSet.LEAVETYPEVALUE == (Convert.ToInt32(LeaveTypeValue.SickLeave) + 1).ToString())
                {
                    if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.AdjLevDeduct) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.AdjLevPaidDayDeduct) + 1).ToString())
                    {
                        dAdjustLeaveDays = 1;
                        nudTotalAdjustLeaveDays.Value = dAdjustLeaveDays.ToDouble();
                        if (nudTotalAdjustLeaveDays.Value > nudTotalVacationDays.Value)
                        {
                            nudTotalAdjustLeaveDays.Value = nudTotalVacationDays.Value;
                        }
                        nudDeductDays.Value -= nudTotalAdjustLeaveDays.Value;
                    }
                }
            }

            if (ents == null)
            {
                return;
            }

            if (ents.Count() == 0)
            {
                return;
            }

            foreach (V_ADJUSTLEAVE item in ents)
            {
                if (item.T_HR_ADJUSTLEAVE.ADJUSTLEAVEDAYS != null)
                {
                    dAdjustLeaveDays += item.T_HR_ADJUSTLEAVE.ADJUSTLEAVEDAYS.Value;
                }
            }

            nudTotalAdjustLeaveDays.Value = dAdjustLeaveDays.ToDouble();

            if (nudTotalAdjustLeaveDays.Value > nudTotalVacationDays.Value)
            {
                nudTotalAdjustLeaveDays.Value = nudTotalVacationDays.Value;
            }

            nudDeductDays.Value -= nudTotalAdjustLeaveDays.Value;
        }

        /// <summary>
        /// 四舍五入浮点数
        /// </summary>
        /// <param name="dValue">浮点数</param>
        /// <param name="strNumOfDec">小数位数值比较值</param>
        /// <param name="ilength"></param>
        /// <returns></returns>
        private decimal RoundOff(decimal dValue, int ilength)
        {
            decimal dRes = 0;
            try
            {
                if (dValue == 0)
                {
                    return dRes;
                }

                dRes = decimal.Round(dValue, ilength);

            }
            catch
            {
                tbSelDateErrmsg.Text = "ERROR";
            }

            return dRes;
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entLeaveTypeSet"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            double dMaxDays = 0, dLeaveDays = 0, dWorkTimePerDay = 0;
            flag = false;

            //检查假期类型是否填写
            if (lkLeaveTypeName.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), Utility.GetResourceStr("REQUIRED", "LEAVETYPESET"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            //检查起始日期是否填写
            if (dpStartDate.Value == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("STARTDATETIME"), Utility.GetResourceStr("REQUIRED", "STARTDATETIME"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            //检查截止日期是否填写
            if (dpEndDate.Value == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATETIME"), Utility.GetResourceStr("REQUIRED", "ENDDATETIME"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            //检查前后日期大小
            if (dpStartDate.Value >= dpEndDate.Value)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ENDDATETIME"), Utility.GetResourceStr("DATECOMPARE", "ENDDATETIME,STARTDATETIME"));
                MessageBox.Show("结束时间必须大于开始时间");
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            //检查请假天数是否超过最大允许值
            double.TryParse(txtMaxDays.Text, out dMaxDays);
            double.TryParse(tbWorkTimePerDay.Text, out dWorkTimePerDay);
            dLeaveDays = nudLeaveDay.Value;

            if (dLeaveDays > dMaxDays)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OVERLEAVEMAXDAYS"));
                flag = false;
                return;
            }
            else
            {
                if (LeaveRecord.LEAVEDAYS == null)
                {
                    LeaveRecord.LEAVEDAYS = 0;
                }

                if (LeaveRecord.LEAVEHOURS == null)
                {
                    LeaveRecord.LEAVEHOURS = 0;
                }

                flag = true;
            }


            //检查请假时长是否超过最大允许值
            if (nudTotalDays.Value > dMaxDays * dWorkTimePerDay)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OVERLEAVEMAXDAYS"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
            }

            //检查请假合计时长，不容许为0
            if (nudTotalDays.Value ==0)
            {
                MessageBox.Show("请假合计时长为0，请正确填写请假时间！");
                flag = false;
                return;
            }
            else
	        {
                flag =true;
	        }

            //检查备注 的长度 500字之内

            if (LeaveRecordType != "11")
            {
                //检查带薪假期，当前有无冲减天数
                T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
                if (dCurLevelDays <= 0 && entLeave.FINETYPE == (LeaveFineType.Free.ToInt32() + 1).ToString())
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOUSEDVACATIONDAYS"));
                    flag = false;
                    return;
                }
                else
                {
                    flag = true;
                }

                //检查带薪假期，请假天数是否<=冲减天数，带薪假期不容许有扣款天数
                if (entLeave.FINETYPE == (LeaveFineType.Free.ToInt32() + 1).ToString() && this.nudDeductDays.Value>0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOUSEDVACATIONDAYS"));
                    MessageBox.Show("实际请假天数不能大于当前可用冲减天数！");
                    flag = false;
                    return;
                }
                else
                {
                    flag = true;
                }
            }
            else if (LeaveRecordType == "11")
            {
                //产前检查假，最多只能请假一天
                double timePerDay = Math.Round(dWorkTimePerDay * this.nudTotalVacationDays.Value,0);
                //string PubdWordTimePerDay * decimal.Parse(nudTotalVacationDays.Value.ToString())
                if (timePerDay < this.nudTotalDays.Value)
                //dWorkTimePerDay * this.nudTotalVacationDays.Value =3.75 ,下午请假有问题，下午是 4小时，此处四舍五入
                {
                    //产前检查假，只能最多只能请假一天
                    //Utility.ShowCustomMessage(MessageTypes.Message, "产前检查假，最多只能请假一天","");
                    MessageBox.Show("当前请假类型，只能请假" + this.nudTotalVacationDays.Value + "天");
                    flag = false;
                    return;
                }
                else
                {
                    flag = true;
                }
            }
            ////检查带薪假期，当前有无冲减天数
            //T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            //if (dCurLevelDays <= 0 && entLeave.FINETYPE == (LeaveFineType.Free.ToInt32() + 1).ToString())
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("NOUSEDVACATIONDAYS"));
            //    flag = false;
            //    return;
            //}
            //else
            //{
            //    flag = true;
            //}
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            bool flag = false;
            RefreshUI(RefreshedTypes.ShowProgressBar);

            //if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}

            CheckSubmitForm(out flag);

            if (!flag)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                return flag;
            }
            CheckForm();//跳转
            //ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

            //if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
            //{
            //    clientAtt.EmployeeLeaveRecordUpdateAsync(LeaveRecord, entAdjustleaves);
            //}
            //else
            //{
            //    clientAtt.EmployeeLeaveRecordAddAsync(LeaveRecord, entAdjustleaves);
            //}

            return true;
        }

        /// <summary>
        /// 用于打开两张单同时保持和提交时，没有进行假期冲减的重新判断，现在做法：在CheckSubmitForm验证后
        /// 重新把数据加载进后台再返回，即保存或提交时再重新算一遍冲减天数
        /// </summary>
        private void CheckForm()
        {
            if (lkLeaveTypeName.DataContext == null)
            {
                return;
            }
            T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            if (entLeave.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
            {
                decimal WorkTimePerDay = 0, dLeaveTotalTime = 0;
                decimal.TryParse(tbWorkTimePerDay.Text, out WorkTimePerDay);
                decimal.TryParse(nudTotalDays.Value.ToString(), out dLeaveTotalTime);
                if (WorkTimePerDay > 0)
                {
                    nudDeductDays.Value = decimal.Round(dLeaveTotalTime / WorkTimePerDay, 1).ToDouble() - nudTotalAdjustLeaveDays.Value;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (dpStartDate.Value == null || dpEndDate.Value == null)
                {
                    return;
                }
                string strLeaveSetId = entLeave.LEAVETYPESETID;
                string strOwnerID = LeaveRecord.OWNERID;
                DateTime dtStartDate = dpStartDate.Value.Value;
                DateTime dtEndDate = dpEndDate.Value.Value;
                //decimal dCurLevelDays = 0;
                clientAtt.GetCurLevelDaysByEmployeeIDAndLeaveFineTypeAsync(strOwnerID, LeaveRecord.LEAVERECORDID, strLeaveSetId, dtStartDate, dtEndDate, 0,"submit");
            }
        }

        /// <summary>
        /// 保存提交时经过重新计算后再进行保存等操作
        /// </summary>
        private void CheckFormSave()
        {
            bool flag = false;
            CheckSubmitForm(out flag);

            if (!flag)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                return;
            }
            ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

            if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
            {
                clientAtt.EmployeeLeaveRecordUpdateAsync(LeaveRecord, entAdjustleaves);
            }
            else
            {
                clientAtt.EmployeeLeaveRecordAddAsync(LeaveRecord, entAdjustleaves);
            }
        }

        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            closeFormFlag = true;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void CloseForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        #region 事件
        /// <summary>
        /// 获取员工请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetLeaveRecordByIDCompleted(object sender, GetLeaveRecordByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                LeaveRecord = e.Result;

                lkLeaveTypeName.IsEnabled = false;
                if (LeaveRecord == null)
                {
                    this.IsEnabled = false;
                    return;
                }

                if (FormType == FormTypes.Resubmit)
                {
                    LeaveRecord.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }

                this.DataContext = LeaveRecord;

                T_HR_LEAVETYPESET entLeaveTypeSet = LeaveRecord.T_HR_LEAVETYPESET;
                if (entLeaveTypeSet == null)
                {
                    this.IsEnabled = false;
                    return;
                }

                txtMaxDays.Text = entLeaveTypeSet.MAXDAYS.Value.ToString();

                if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString())
                {
                    nudTotalVacationDays.Value = entLeaveTypeSet.MAXDAYS.Value.ToDouble();
                    nudTotalAdjustLeaveDays.Value = entLeaveTypeSet.MAXDAYS.Value.ToDouble();
                }

                toolbar1.IsEnabled = true;
                toolbar1.Visibility = System.Windows.Visibility.Visible;
                dgLevelDayList.Visibility = System.Windows.Visibility.Visible;
                if (entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString() || entLeaveTypeSet.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
                {
                    toolbar1.IsEnabled = false;
                    toolbar1.Visibility = System.Windows.Visibility.Collapsed;
                    dgLevelDayList.Visibility = System.Windows.Visibility.Collapsed;
                }

                string strLoginUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                if (LeaveRecord.EMPLOYEEID != strLoginUserId || LeaveRecord.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    this.IsEnabled = false;
                }

                //perClient.GetEmployeeDetailByIDAsync(LeaveRecord.EMPLOYEEID);
                perClient.GetEmpOrgInfoByIDAsync(LeaveRecord.EMPLOYEEID, LeaveRecord.OWNERPOSTID, LeaveRecord.OWNERDEPARTMENTID, LeaveRecord.OWNERCOMPANYID);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        //请假类型上年的申请次数
        public string dLeaveSYearTimes { get; set; }
        //请假类型本年的申请次数
        public string dLeaveYearTimes { get; set; }
        //请假类型本月的申请次数
        public string dLeaveMonthTimes { get; set; }
        //第一次请假的时间
        public DateTime dLeaveFistDate { get; set; }
        /// <summary>
        /// 获取员工当前类型假，历史请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetLeaveDaysHistoryCompleted(object sender, GetLeaveDaysHistoryCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtRemark.Text = Utility.GetResourceStr("YEARLEAVETIMES", e.dLeaveYearTimes.ToString()) + Utility.GetResourceStr("YEARLEAVEDAYS", e.dLeaveYearDays.ToString()) + "\r\n"
                            + Utility.GetResourceStr("MONTHLEAVETIMES", e.dLeaveMonthTimes.ToString()) + Utility.GetResourceStr("MONTHLEAVEDAYS", e.dLeaveMonthDays.ToString());
                dLeaveYearTimes = e.dLeaveYearTimes.ToString();
                dLeaveMonthTimes = e.dLeaveMonthTimes.ToString();
                dLeaveFistDate = e.dLeaveFistDate;
                dLeaveSYearTimes = e.dLeaveSYearTimes.ToString();

                clientAtt.GetEmployeeLeaveRdListsByLeaveRecordIDAsync(LeaveRecord.LEAVERECORDID, "2");//根据请假ID去找审核通过的销假信息
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 获取员工调休假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAdjustLeaveDetailListByLeaveRecordIDCompleted(object sender, GetAdjustLeaveDetailListByLeaveRecordIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                IEnumerable<V_ADJUSTLEAVE> ents = e.Result;
                dgLevelDayList.ItemsSource = ents;

                if (dgLevelDayList.SelectedItems == null)
                {
                    CalculateDeductDaysByAdjustLeaveDetails();
                }
                else if (dgLevelDayList.SelectedItems.Count == 0)
                {
                    CalculateDeductDaysByAdjustLeaveDetails();
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.AuditInfo);
            SetToolBar();
        }


        decimal PubdWordTimePerDay = 0;
        /// <summary>
        /// 获取员工上月应用的考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionByEmployeeIDAndDateCompleted(object sender, GetAttendanceSolutionByEmployeeIDAndDateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendanceSolution = e.Result;

                if (entAttendanceSolution == null)
                {
                    return;
                }

                if (entAttendanceSolution.WORKTIMEPERDAY == null)
                {
                    return;
                }

                decimal dWordTimePerDay = 0;

                dWordTimePerDay = entAttendanceSolution.WORKTIMEPERDAY.Value;
                tbWorkTimePerDay.Text = dWordTimePerDay.ToString();
                //
                T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
                LeaveRecordType = entLeave.LEAVETYPEVALUE;

                if (dWordTimePerDay > 0)
                {
                    //一天的工作时长
                    PubdWordTimePerDay = dWordTimePerDay;
                    //获取可冲假天数、冲假天数、扣款天数
                    if (LeaveRecordType != "11")
                    {
                        GetCurLevelDays();
                    }
                    else
                    {
                        //如果本年请假是O次，表示是第一次请假，那么可休的假为0.5天
                        //并且还要判断上年请该类型假也是0次（考虑跨年请假）
                        if (dLeaveYearTimes == "0" && dLeaveSYearTimes=="0")
                        {
                            //当前可用冲减假天数
                            this.nudTotalVacationDays.Value = 0.50;
                            //冲假天数
                            this.nudTotalAdjustLeaveDays.Value = 0.50;
                            //扣款天数
                            this.nudDeductDays.Value = 0.00;
                            //RefreshUI(RefreshedTypes.HideProgressBar);
                        }

                        //如果不是0，那么表示已怀孕，这个时候判断是怀孕几个月，拿第一次的请假时间来判断
                        else
                        {
                            //得到年份
                            DateTime nowDate = dpEndDate.Value.Value;
                            DateTime fistDate = new DateTime();
                            //第一次请假的时间
                            DateTime.TryParse(dLeaveFistDate.Year.ToString() +"-"+ dLeaveFistDate.Month.ToString() +"-"+ dLeaveFistDate.Day.ToString(), out fistDate);
                            TimeSpan d1 = nowDate - fistDate;
                            //得到相隔的月数
                            int Mounth = d1.Days / 30;
                            //孕前1~7月 半天
                            if (Mounth >= 0 && Mounth <= 7)
                            {

                                if (dLeaveMonthTimes=="0")
                                {
                                    //当前可用冲减假天数
                                    this.nudTotalVacationDays.Value = 0.50;
                                    //冲假天数
                                    this.nudTotalAdjustLeaveDays.Value = 0.50;
                                    //扣款天数
                                    this.nudDeductDays.Value = 0.00;
                                    //RefreshUI(RefreshedTypes.HideProgressBar);
                                }
                                else
                                {
                                    //当前可用冲减假天数
                                    this.nudTotalVacationDays.Value = 0.00;
                                    //冲假天数
                                    this.nudTotalAdjustLeaveDays.Value = 0.00;
                                    //扣款天数
                                    this.nudDeductDays.Value = 0.00;
                                    //RefreshUI(RefreshedTypes.HideProgressBar);
                                }
                            }
                            //8~9个月 每月一天
                            else if (Mounth >= 8 && Mounth <= 9)
                            {
                                if (dLeaveMonthTimes=="0")
                                {
                                    //当前可用冲减假天数
                                    this.nudTotalVacationDays.Value = 1.00;
                                    //冲假天数
                                    this.nudTotalAdjustLeaveDays.Value = 1.00;
                                    //扣款天数
                                    this.nudDeductDays.Value = 0.00;
                                    //RefreshUI(RefreshedTypes.HideProgressBar);
                                }
                                else
                                {
                                    //当前可用冲减假天数
                                    this.nudTotalVacationDays.Value = 0.00;
                                    //冲假天数
                                    this.nudTotalAdjustLeaveDays.Value = 0.00;
                                    //扣款天数
                                    this.nudDeductDays.Value = 0.00;
                                    //RefreshUI(RefreshedTypes.HideProgressBar);
                                }
                            }
                        }

                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 获取员工请假实际时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetRealLeaveDayByEmployeeIdAndDateCompleted(object sender, GetRealLeaveDayByEmployeeIdAndDateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.Replace("{", "").Replace("}", "")));
                    return;
                }

                nudLeaveDay.Value = e.dLeaveDay.ToDouble();
                nudLeaveHours.Value = e.dLeaveTime.ToDouble();
                nudTotalDays.Value = e.dLeaveTotalTime.ToDouble();

                clientAtt.GetAttendanceSolutionByEmployeeIDAndDateAsync(LeaveRecord.EMPLOYEEID, LeaveRecord.STARTDATETIME.Value, LeaveRecord.ENDDATETIME.Value);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 获取当前请假时段内，可用冲减调休天数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetCurLevelDaysByEmployeeIDAndLeaveFineTypeCompleted(object sender, GetCurLevelDaysByEmployeeIDAndLeaveFineTypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                dCurLevelDays = RoundOff(e.dCurLevelDays,2);

                T_HR_LEAVETYPESET entLeave = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;

                //Modified by: Sam
                //Date       : 2011-8-25
                //For        : 此处导致当假期标准设置的事假的扣款方式选择“调休+带薪假抵扣+扣款”时，带不出冲减天数;扣款方式为“按日薪扣”时没问题。 

                //如果当前选择的是带薪假期，并且没有冲减天数时，当前页面设为只读
                nudTotalVacationDays.Value = RoundOff(dCurLevelDays, 2).ToDouble();
                if (dCurLevelDays > 0 || entLeave.FINETYPE != (LeaveFineType.Free.ToInt32() + 1).ToString())
                {
                    decimal dWorkTimePerDay = 0, dTotalDays = 0, dMaxDays = 0;
                    decimal.TryParse(txtMaxDays.Text, out dMaxDays);
                    decimal.TryParse(tbWorkTimePerDay.Text, out dWorkTimePerDay);
                    decimal.TryParse(nudTotalDays.Value.ToString(), out dTotalDays);

                    if (entLeave.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString())
                    {
                        if (dMaxDays * dWorkTimePerDay > dTotalDays)
                        {
                            if (dCurLevelDays * dWorkTimePerDay >= dTotalDays)
                            {
                                nudTotalAdjustLeaveDays.Value = RoundOff(dTotalDays / dWorkTimePerDay, 2).ToDouble();
                            }
                            else
                            {
                                nudTotalAdjustLeaveDays.Value = dCurLevelDays.ToDouble();
                            }
                        }
                        else
                        {
                            if (dCurLevelDays * dWorkTimePerDay >= dTotalDays)
                            {
                                nudTotalAdjustLeaveDays.Value = dMaxDays.ToDouble();
                            }
                            else
                            {
                                if (dCurLevelDays >= dMaxDays)
                                {
                                    nudTotalAdjustLeaveDays.Value = dMaxDays.ToDouble();
                                }
                                else
                                {
                                    nudTotalAdjustLeaveDays.Value = dCurLevelDays.ToDouble();
                                }
                            }
                        }
                    }

                    if (dWorkTimePerDay > 0)
                    {
                        nudDeductDays.Value = RoundOff(dTotalDays / dWorkTimePerDay, 2).ToDouble() - nudTotalAdjustLeaveDays.Value;
                    }
                    if (e.UserState != null && e.UserState.ToString() == "submit")//如果为保存提交后的方法所异步调用，则进行相应操作
                    {
                        CheckFormSave();
                    }
                    if (FormType != FormTypes.New)
                    {
                        clientAtt.GetAdjustLeaveDetailListByLeaveRecordIDAsync(LeaveRecordID);
                    }
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 获取DataGrid当前选中项，员工可用假期天数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetCurLevelDayCountByEmployeeIDCompleted(object sender, GetCurLevelDayCountByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (dgLevelDayList.ItemsSource == null)
                {
                    return;
                }

                if (dgLevelDayList.SelectedItem == null)
                {
                    return;
                }

                V_ADJUSTLEAVE entViewAdjustleave = dgLevelDayList.SelectedItem as V_ADJUSTLEAVE;

                T_HR_LEAVETYPESET ent = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
                if (ent == null)
                {
                    return;
                }

                T_HR_EMPLOYEELEVELDAYCOUNT entTemp = new T_HR_EMPLOYEELEVELDAYCOUNT();
                entTemp = e.Result;

                if (entTemp == null)
                {
                    return;
                }

                decimal dAdjustLeaveDays = 0;
                if (entTemp.DAYS != null)
                {
                    dAdjustLeaveDays = entTemp.DAYS.Value;
                }

                entViewAdjustleave.VacationDays = dAdjustLeaveDays;
                entViewAdjustleave.T_HR_ADJUSTLEAVE.LEAVETYPESETID = entTemp.LEAVETYPESETID;

                ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

                foreach (V_ADJUSTLEAVE item in entAdjustleaves)
                {
                    if (item.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID == entViewAdjustleave.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID)
                    {
                        item.VacationDays = entViewAdjustleave.VacationDays;
                        item.T_HR_ADJUSTLEAVE.LEAVETYPESETID = entViewAdjustleave.T_HR_ADJUSTLEAVE.LEAVETYPESETID;
                    }
                }

                dgLevelDayList.ItemsSource = entAdjustleaves;

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        ///// <summary>
        ///// 获取员工个人信息
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void perClient_GetEmployeeDetailByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeePost = e.Result;
        //        if (employeePost == null)
        //        {
        //            this.IsEnabled = false;
        //            return;
        //        }

        //        //赋值
        //        tbEmpName.Text = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME;
        //        tbOrgName.Text = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME + " - " + employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + " - " + employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
        //        if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
        //        {
        //            tbEmpName.Text = tbEmpName.Text + "-" + tbOrgName.Text;
        //        }

        //        tbEmpWorkAge.Text = employeePost.T_HR_EMPLOYEE.WORKINGAGE.ToString();
        //        tbEmpSex.Text = employeePost.T_HR_EMPLOYEE.SEX;
        //        tbEmpLevel.Text = employeePost.EMPLOYEEPOSTS[0].POSTLEVEL.ToString();

        //        LeaveRecord.EMPLOYEEID = employeePost.T_HR_EMPLOYEE.EMPLOYEEID;
        //        LeaveRecord.EMPLOYEECODE = employeePost.T_HR_EMPLOYEE.EMPLOYEECODE;
        //        LeaveRecord.EMPLOYEENAME = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME;

        //        string strEmployeeState = employeePost.T_HR_EMPLOYEE.EMPLOYEESTATE;

        //        if (strEmployeeState != Convert.ToInt32(EmployeeState.OnTrial).ToString() && strEmployeeState != Convert.ToInt32(EmployeeState.Regular).ToString() && strEmployeeState != Convert.ToInt32(EmployeeState.OnLeaving).ToString())
        //        {
        //            SetOnlyBrowse();
        //            return;
        //        }

        //        if (LeaveRecord.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() && LeaveRecord.EMPLOYEEID != SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
        //        {
        //            SetOnlyBrowse();
        //            return;
        //        }

        //        if (FormType != FormTypes.New)
        //        {
        //            RefreshUI(RefreshedTypes.AuditInfo);
        //        }
        //        SetToolBar();
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //}

        /// <summary>
        /// 获取加班申请人员的员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_GetEmpOrgInfoByIDCompleted(object sender, Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs e)
        {
            try
            {
               if (e.Error == null)
               {
                   SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW employeeView = e.Result;
                   if (employeeView == null)
                   {
                       this.IsEnabled = false;
                       return;
                   }

                   //赋值
                   tbEmpName.Text = employeeView.EMPLOYEECNAME;
                   tbOrgName.Text = employeeView.POSTNAME + " - " + employeeView.DEPARTMENTNAME + " - " + employeeView.COMPANYNAME;
                   if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                   {
                       tbEmpName.Text = tbEmpName.Text + "-" + tbOrgName.Text;
                   }

                  
                   tbEmpSex.Text = employeeView.SEX;
                   tbEmpLevel.Text = employeeView.POSTLEVEL.ToString();

                   LeaveRecord.EMPLOYEEID = employeeView.EMPLOYEEID;
                   LeaveRecord.EMPLOYEECODE = employeeView.EMPLOYEECODE;
                   LeaveRecord.EMPLOYEENAME = employeeView.EMPLOYEECNAME;

                   string strEmployeeState = employeeView.EMPLOYEESTATE;

                   if (strEmployeeState == Convert.ToInt32(EmployeeState.Dimission).ToString() && (FormType == FormTypes.New || FormType == FormTypes.Edit || FormType == FormTypes.Resubmit))
                   {
                       SetOnlyBrowse();
                       return;
                   }

                   if (LeaveRecord.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() && LeaveRecord.EMPLOYEEID != SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
                   {
                       SetOnlyBrowse();
                       return;
                   }

                   if (FormType != FormTypes.New)
                   {
                       RefreshUI(RefreshedTypes.AuditInfo);
                   }
                   SetToolBar();
               }
               else
               {
                   Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
               }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message + ex.Message));
            }
        }

        /// <summary>
        /// 新增员工请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_EmployeeLeaveRecordAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", ""));

                if (closeFormFlag)
                {
                    CloseForm();
                    return;
                }

                FormType = FormTypes.Edit;
                LeaveRecordID = LeaveRecord.LEAVERECORDID;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 更新员工请假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_EmployeeLeaveRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (LeaveRecord.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                {
                    if (!isSubmit)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                    }
                }
                else
                {
                    if (!isSubmit)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                    }
                }
                if (needsubmit)
                {
                    try
                    {
                        EntityBrowser entBrowser1 = this.FindParentByType<EntityBrowser>();
                        entBrowser1.ManualSubmit();
                        BackToSubmit();
                    }
                    catch (Exception ex)
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"));
                    }
                }
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                    return;
                }

                FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            else
            {
                needsubmit = false;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

          /// <summary>
        ///     回到提交前的状态
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AuditLeaveRecordCompleted(object sender, AuditLeaveRecordCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 选择当前员工的请假类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkLeaveTypeName_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("LEAVETYPENAME", "LEAVETYPENAME");
            cols.Add("ISFREELEAVEDAY", "ISFREELEAVEDAY,ISCHECKED,DICTIONARYCONVERTER");
            //string filter = "";
            //System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            //filter += " ISFREELEAVEDAY=@" + paras.Count().ToString() + "";
            //paras.Add("1");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.LeaveTypeSet,
                typeof(T_HR_LEAVETYPESET[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_LEAVETYPESET ent = lookup.SelectedObj as T_HR_LEAVETYPESET;

                if (ent != null)
                {
                    if (ent.SEXRESTRICT != "2")
                    {
                        if (ent.SEXRESTRICT != tbEmpSex.Text)
                        {
                            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEELEAVERECORD"), Utility.GetResourceStr("LEAVETYPERESTRICT", "SEXRESTRICT"));
                            MessageBox.Show("该假期标准只限于女性");
                            return;
                        }
                    }

                    if (!string.IsNullOrEmpty(ent.POSTLEVELRESTRICT))
                    {
                        decimal dlevel = 0, dCheckLevel = 0;

                        decimal.TryParse(tbEmpLevel.Text, out dlevel);
                        decimal.TryParse(ent.POSTLEVELRESTRICT, out dCheckLevel);

                        if (dlevel > dCheckLevel)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EMPLOYEELEAVERECORD"), Utility.GetResourceStr("LEAVETYPERESTRICT", "POSTLEVELRESTRICT"));
                            return;
                        }
                    }

                    LeaveRecord.T_HR_LEAVETYPESET = ent;

                    //选择的请假类型 11 字典表 表示产前检查假
                    LeaveRecordType = ent.LEAVETYPEVALUE;
                    txtMaxDays.Text = ent.MAXDAYS.Value.ToString();

                    //nudLeaveDay.Maximum = Convert.ToDouble(ent.MAXDAYS);

                    toolbar1.IsEnabled = true;
                    toolbar1.Visibility = System.Windows.Visibility.Visible;
                    dgLevelDayList.Visibility = System.Windows.Visibility.Visible;
                    if (ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Free) + 1).ToString() || ent.FINETYPE == (Convert.ToInt32(LeaveFineType.Deduct) + 1).ToString())
                    {
                        toolbar1.IsEnabled = false;
                        toolbar1.Visibility = System.Windows.Visibility.Collapsed;
                        dgLevelDayList.Visibility = System.Windows.Visibility.Collapsed;
                    }

                    ResetForm();
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 根据填写的起止时间自动计算请假天数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpStartDate_ValueChanged(object sender, EventArgs e)
        {
            string strMsg = string.Empty;
            if (dpStartDate.Value == null)
            {
                return;
            }

            DateTime dtSelDate = dpStartDate.Value.Value;

            if (dtSelDate.Month < DateTime.Now.AddMonths(-1).Month)
            {
                strMsg = "LEAVEOUTAPPROVED";
                return;
            }

            if (entAttendanceSolution == null)
            {
                return;
            }

            if (dtSelDate.Month == DateTime.Now.AddMonths(-1).Month)
            {
                if (entAttendanceSolution.ISCURRENTMONTH == (Convert.ToInt32(IsChecked.Yes) + 1).ToString())
                {
                    strMsg = "LEAVEOUTAPPROVED";
                    return;
                }
            }

            if (LeaveRecordType != "11")
            {
                CalculateDayCount();
            }

            tbSelDateErrmsg.Text = strMsg;
        }

        /// <summary>
        /// 根据填写的起止时间自动计算请假天数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpEndDate_OnValueChanged(object sender, EventArgs e)
        {
            CalculateDayCount();
        }

        /// <summary>
        /// DataGrid内的抵扣带薪假选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxVacType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLevelDayList.SelectedItem == null)
            {
                return;
            }

            V_ADJUSTLEAVE entViewAdjustleave = dgLevelDayList.SelectedItem as V_ADJUSTLEAVE;

            AppControl.DictionaryComboBox cbxVacType = dgLevelDayList.Columns[1].GetCellContent(dgLevelDayList.SelectedItem) as AppControl.DictionaryComboBox;

            if (cbxVacType.SelectedItem == null)
            {
                return;
            }

            T_HR_LEAVETYPESET entLeaveType = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY entDic = cbxVacType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            if (entDic.DICTIONARYVALUE.Value.ToString() != entLeaveType.LEAVETYPEVALUE && entDic.DICTIONARYVALUE.Value.ToString() != (Convert.ToInt32(LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVETYPESET")));
                return;
            }

            string strVacType = entDic.DICTIONARYVALUE.Value.ToString();
            string strMsg = string.Empty;

            entViewAdjustleave.VacationType = strVacType;
            ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;


            foreach (V_ADJUSTLEAVE item in entAdjustleaves)
            {
                if (string.IsNullOrEmpty(item.VacationType))
                {
                    continue;
                }

                if (item.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID == entViewAdjustleave.T_HR_ADJUSTLEAVE.ADJUSTLEAVEID)
                {
                    continue;
                }

                if (item.VacationType == strVacType)
                {
                    strMsg = Utility.GetResourceStr("ALREADYEXISTED");
                    break;
                }
            }

            if (!string.IsNullOrEmpty(strMsg))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPEVALUE"), strMsg);
                cbxVacType.SelectedIndex = 0;
                return;
            }

            DateTime dtDate = dpStartDate.Value.Value;
            string strOwnerID = string.Empty;

            if (FormType == FormTypes.New)
            {
                strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
            else
            {
                strOwnerID = LeaveRecord.EMPLOYEEID;
            }

            clientAtt.GetCurLevelDayCountByEmployeeIDAsync(strOwnerID, strVacType, dtDate);
        }

        private void dpTerminatedate_OnValueChanged(object sender, EventArgs e)
        {
            if (dgLevelDayList.SelectedItem == null)
            {
                return;
            }

            DateTimePicker dtpStartDate = dgLevelDayList.Columns[3].GetCellContent(dgLevelDayList.SelectedItem) as DateTimePicker;
            DateTimePicker dtpEndDate = dgLevelDayList.Columns[4].GetCellContent(dgLevelDayList.SelectedItem) as DateTimePicker;

            DateTime dtStart = new DateTime();
            DateTime dtEnd = new DateTime();
            int iHour = 0;
            decimal dDays = 0;


            nudTotalDays.Value = 0;

            if (dtpStartDate.Value == null)
            {
                return;
            }

            if (dtpEndDate.Value == null)
            {
                return;
            }

            dtStart = dtpStartDate.Value.Value;
            dtEnd = dtpEndDate.Value.Value;

            if (dtEnd.CompareTo(dtStart) < 0)
            {
                return;
            }

            TimeSpan tsStart = new TimeSpan(dtStart.Ticks);
            TimeSpan tsEnd = new TimeSpan(dtEnd.Ticks);
            TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

            if (ts.Hours != 0)
            {
                dDays = ts.Days;
                iHour = ts.Hours;
            }
            else
            {
                dDays = ts.Days + 1;
            }

            double WorkTimePerDay = 0;
            double.TryParse(tbWorkTimePerDay.Text, out WorkTimePerDay);

            if (iHour > 0)
            {
                if (iHour >= WorkTimePerDay)
                {
                    dDays += 1;
                }
                else
                {
                    dDays += decimal.Parse("0.5");
                }
            }

            NumericUpDown nudLeaveDays = dgLevelDayList.Columns[5].GetCellContent(dgLevelDayList.SelectedItem) as NumericUpDown;
            nudLeaveDays.Value = double.Parse(dDays.ToString());

            AppControl.DictionaryComboBox cbxVacType = dgLevelDayList.Columns[1].GetCellContent(dgLevelDayList.SelectedItem) as AppControl.DictionaryComboBox;
            T_HR_LEAVETYPESET entLeaveType = lkLeaveTypeName.DataContext as T_HR_LEAVETYPESET;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY entDic = cbxVacType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            if (entDic.DICTIONARYVALUE.Value.ToString() != entLeaveType.LEAVETYPEVALUE && entDic.DICTIONARYVALUE.Value.ToString() != (Convert.ToInt32(LeaveTypeValue.AdjustLeave) + 1).ToString())
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVETYPESET")));
                return;
            }

            string strVacType = entDic.DICTIONARYVALUE.Value.ToString();

            ObservableCollection<V_ADJUSTLEAVE> entAdjustleaves = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

            foreach (V_ADJUSTLEAVE item in entAdjustleaves)
            {
                if (item.VacationType == strVacType)
                {
                    item.T_HR_ADJUSTLEAVE.ADJUSTLEAVEDAYS = decimal.Parse(nudLeaveDays.Value.ToString());
                }
            }

            dgLevelDayList.ItemsSource = entAdjustleaves;
            CalculateDeductDaysByAdjustLeaveDetails();
        }

        /// <summary>
        /// 新增(临时，已提交审核的记录将禁止使用此按钮)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (lkLeaveTypeName.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("LEAVETYPESET"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("LEAVETYPESET")));
                return;
            }

            ObservableCollection<V_ADJUSTLEAVE> ents = new ObservableCollection<V_ADJUSTLEAVE>();

            if (dgLevelDayList.ItemsSource != null)
            {
                ents = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;
            }

            V_ADJUSTLEAVE ent = new V_ADJUSTLEAVE();
            T_HR_ADJUSTLEAVE entAdjust = new T_HR_ADJUSTLEAVE();
            entAdjust.ADJUSTLEAVEID = Guid.NewGuid().ToString();
            entAdjust.CREATEDATE = DateTime.Now;
            entAdjust.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entAdjust.T_HR_EMPLOYEELEAVERECORD = LeaveRecord;
            //添加人所属部门ID
            entAdjust.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entAdjust.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entAdjust.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //出差记录只能本人申请请
            entAdjust.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entAdjust.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            entAdjust.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;

            ent.VacationType = string.Empty;
            ent.VacationDays = 0;
            ent.T_HR_ADJUSTLEAVE = entAdjust;

            ents.Add(ent);

            dgLevelDayList.ItemsSource = ents;
        }

        /// <summary>
        /// 删除调休记录(临时，已提交审核的记录将禁止使用此按钮)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgLevelDayList.SelectedItems == null)
            {
                return;
            }

            if (dgLevelDayList.SelectedItems.Count == 0)
            {
                return;
            }


            ObservableCollection<V_ADJUSTLEAVE> entList = dgLevelDayList.ItemsSource as ObservableCollection<V_ADJUSTLEAVE>;

            ObservableCollection<V_ADJUSTLEAVE> entTemps = new ObservableCollection<V_ADJUSTLEAVE>();
            for (int i = 0; i < dgLevelDayList.SelectedItems.Count; i++)
            {
                entTemps.Add(dgLevelDayList.SelectedItems[i] as V_ADJUSTLEAVE);
            }

            int iSel = entTemps.Count;

            for (int i = 0; i < iSel; i++)
            {
                V_ADJUSTLEAVE entTemp = entTemps[i] as V_ADJUSTLEAVE;

                for (int j = 0; j < entList.Count; j++)
                {
                    if (entList[j].VacationType == entTemp.VacationType)
                    {
                        entList.RemoveAt(j);
                    }
                }
            }

            dgLevelDayList.ItemsSource = entList;
        }
        #endregion
    }
}