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
using System.Text;

using SMT.SaaS.FrameworkUI;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class TerminateLeaveForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        public FormTypes FormType { set; get; }
        public string CancelLeaveID { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public decimal WordTimePerDay { set; get; }
        //销假记录
        public T_HR_EMPLOYEECANCELLEAVE cancelLeave { get; set; }

        //考勤方案
        public T_HR_ATTENDANCESOLUTION entAttendanceSolution { get; set; }

        AttendanceServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient perClient;
        #endregion

        #region 初始化
        //Modified by:Sam
        //Date:2011-10-9
        //For:增加一个无参数的构造函数来实现待办任务新建单据
        public TerminateLeaveForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            CancelLeaveID = string.Empty;
            this.Loaded += new RoutedEventHandler(TerminateLeaveForm_Loaded);
        }

        public TerminateLeaveForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            CancelLeaveID = strID;
            this.Loaded += new RoutedEventHandler(TerminateLeaveForm_Loaded);
        }

        void TerminateLeaveForm_Loaded(object sender, RoutedEventArgs e)
        {
            client = new AttendanceServiceClient();
            perClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            RegisterEvents();
            InitParas();
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEECANCELLEAVE");
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
                    closeFormFlag = true;
                    Save();
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

        #region IAudit 成员

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEECANCELLEAVE Info)
        {
            //审核状态
            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            //岗位级别
            decimal? postlevelValue = Convert.ToDecimal(tbEmpLevel.Text.Trim());
            string postLevelName = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == postlevelValue).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERPOSTNAME", tbOrgName.Text.Trim(), tbOrgName.Text.Trim()));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "POSTLEVEL", tbEmpLevel.Text.Trim(), postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));
            AutoList.Add(basedata("T_HR_EMPLOYEECANCELLEAVE", "ENTITYKEY", Info.CANCELLEAVEID, string.Empty));
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

        private AutoDictionary basedataForChild(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);//这里需要传递5个参数过去，keyvalue就是该表的主键ID
            return ad;
        }
        #endregion

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTLEVEL", tbEmpLevel.Text.Trim());
            para.Add("OWNERPOSTNAME", tbOrgName.Text.Trim());
            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, cancelLeave);
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEECANCELLEAVE>(cancelLeave, para, "HR");
            Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECANCELLEAVE", CancelLeaveID, strXmlObjectSource);
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

            cancelLeave.CHECKSTATE = state;
            RefreshUI(RefreshedTypes.HideProgressBar);

            if (FormType == FormTypes.Edit)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "提交成功");
            }
            if (FormType == FormTypes.Audit)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "审核成功");

            }
            
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //client.EmployeeCancelLeaveUpdateAsync(cancelLeave);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (cancelLeave != null)
                state = cancelLeave.CHECKSTATE;

            return state;
        }

        #endregion

        #region 私有方法
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
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            client.GetEmployeeLeaveRecordByIDCompleted += new EventHandler<GetEmployeeLeaveRecordByIDCompletedEventArgs>(client_GetEmployeeLeaveRecordByIDCompleted);

            client.GetRealCancelLeaveDayByEmployeeIdAndDateCompleted += new EventHandler<GetRealCancelLeaveDayByEmployeeIdAndDateCompletedEventArgs>(client_GetRealCancelLeaveDayByEmployeeIdAndDateCompleted);
            client.GetEmployeeCancelLeaveByIDCompleted += new EventHandler<GetEmployeeCancelLeaveByIDCompletedEventArgs>(client_GetEmployeeCancelLeaveByIDCompleted);
            //client.EmployeeCancelLeaveAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeCancelLeaveAddCompleted);
            client.EmployeeCancelLeaveAddCompleted += new EventHandler<EmployeeCancelLeaveAddCompletedEventArgs>(client_EmployeeCancelLeaveAddCompleted);
            //client.EmployeeCancelLeaveUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeCancelLeaveUpdateCompleted);
            client.EmployeeCancelLeaveUpdateCompleted += new EventHandler<EmployeeCancelLeaveUpdateCompletedEventArgs>(client_EmployeeCancelLeaveUpdateCompleted);
            //获取员工名称，并显示所在的公司架构
            //perClient.GetEmployeeDetailByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs>(perClient_GetEmployeeDetailByIDCompleted);
            perClient.GetEmpOrgInfoByIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs>(perClient_GetEmpOrgInfoByIDCompleted);
            client.GetEmployeeLeaveRdListsByLeaveRecordIDCompleted += new EventHandler<GetEmployeeLeaveRdListsByLeaveRecordIDCompletedEventArgs>(client_GetEmployeeLeaveRdListsByLeaveRecordIDCompleted);
        }

        void client_GetEmployeeLeaveRdListsByLeaveRecordIDCompleted(object sender, GetEmployeeLeaveRdListsByLeaveRecordIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                ObservableCollection<T_HR_EMPLOYEECANCELLEAVE> list = e.Result;
                this.GetTxtCancelRecord(list);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void InitParas()
        {
            if (FormType == FormTypes.New)
            {
                cancelLeave = new T_HR_EMPLOYEECANCELLEAVE();
                cancelLeave.CANCELLEAVEID = Guid.NewGuid().ToString();
                cancelLeave.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();

                //出差记录只能本人申请
                cancelLeave.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                cancelLeave.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                cancelLeave.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;


                //权限控制
                cancelLeave.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                cancelLeave.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                cancelLeave.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                cancelLeave.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;


                //2010年2月11日, 添加人信息
                cancelLeave.CREATEDATE = DateTime.Now;
                cancelLeave.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                cancelLeave.UPDATEDATE = System.DateTime.Now;
                cancelLeave.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                //添加人所属部门ID
                cancelLeave.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                cancelLeave.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                cancelLeave.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                //赋初始值
                tbOrgName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                tbEmpName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
              
                tbEmpLevel.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
                if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                {
                    tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
                }

                this.DataContext = cancelLeave;

                SetToolBar();
            }
            else
            {
                client.GetEmployeeCancelLeaveByIDAsync(CancelLeaveID);
                if (FormType == FormTypes.Browse)
                {
                    this.IsEnabled = false;
                }
            }
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEECANCELLEAVE", cancelLeave.OWNERID,
                    cancelLeave.OWNERPOSTID, cancelLeave.OWNERDEPARTMENTID, cancelLeave.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 计算放假时长
        /// </summary>
        private void CalculateDayCount()
        {
            DateTime dtTerminateStart = new DateTime();
            DateTime dtTerminateEnd = new DateTime();
            DateTime dtLeaveStart = new DateTime();
            DateTime dtLeaveEnd = new DateTime();


            nudTerminateTotalDays.Value = 0;

            if (dpStartDate.Value == null)
            {
                return;
            }

            if (dpEndDate.Value == null)
            {
                return;
            }

            if (dpTerminateStartDate.Value == null)
            {
                return;
            }

            if (dpTerminateEndDate.Value == null)
            {
                return;
            }

            dtLeaveStart = dpStartDate.Value.Value;
            dtLeaveEnd = dpEndDate.Value.Value;

            dtTerminateStart = dpTerminateStartDate.Value.Value;
            dtTerminateEnd = dpTerminateEndDate.Value.Value;

            if (dtTerminateStart < dtLeaveStart)
            {
                return;
            }

            if (dtLeaveEnd < dtTerminateEnd)
            {
                return;
            }

            if (dtTerminateEnd < dtTerminateStart)
            {
                return;
            }

            if (cancelLeave == null)
            {
                return;
            }

            decimal dCancelLeaveDay = 0, dCancelLeaveTime = 0, dCancelLeaveTotalTime = 0;
            client.GetRealCancelLeaveDayByEmployeeIdAndDateAsync(cancelLeave.CANCELLEAVEID, cancelLeave.EMPLOYEEID, dtTerminateStart, dtTerminateEnd, dCancelLeaveDay, dCancelLeaveTime, dCancelLeaveTotalTime);
        }
        #endregion

        #region 事件
        /// <summary>
        /// 获取销假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeLeaveRecordByIDCompleted(object sender, GetEmployeeLeaveRecordByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                V_EMPLOYEELEAVERECORD temp = e.Result;
                WordTimePerDay = temp.WorkTimePerDay;
                //perClient.GetEmployeeDetailByIDAsync(cancelLeave.EMPLOYEEID, "Employee");
                perClient.GetEmpOrgInfoByIDAsync(cancelLeave.OWNERID, cancelLeave.OWNERPOSTID, cancelLeave.OWNERDEPARTMENTID, cancelLeave.OWNERCOMPANYID);
            }
        }

        /// <summary>
        /// 获取加班申请人员的员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_GetEmpOrgInfoByIDCompleted(object sender, Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs e)
        {
            try
            {
               if (e.Error != null)
               {
                   Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
               }
               else
               {
                   SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW employeeView = e.Result;

                   //赋值
                   tbOrgName.Text = employeeView.POSTNAME + " - " + employeeView.DEPARTMENTNAME + " - " + employeeView.COMPANYNAME;
                   tbEmpName.Text = employeeView.EMPLOYEECNAME;
                   if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                   {
                       tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
                   }

                   //tbEmpWorkAge.Text = employeeView.T_HR_EMPLOYEE.WORKINGAGE.ToString();
                   tbEmpLevel.Text = employeeView.POSTLEVEL.ToString();

                   if (cancelLeave.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() && cancelLeave.EMPLOYEEID != SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
                   {
                       SetOnlyBrowse();
                       return;
                   }

                   RefreshUI(RefreshedTypes.AuditInfo);
                   SetToolBar();
               }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message + ex.Message));
            }
        }
        ///// <summary>
        ///// 获取单据所属员工的员工信息
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void perClient_GetEmployeeDetailByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeePost = e.Result;

        //        //赋值
        //        tbOrgName.Text = employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME + " - " + employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + " - " + employeePost.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
        //        tbEmpName.Text = employeePost.T_HR_EMPLOYEE.EMPLOYEECNAME;
        //        if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
        //        {
        //            tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
        //        }

        //        tbEmpWorkAge.Text = employeePost.T_HR_EMPLOYEE.WORKINGAGE.ToString();
        //        tbEmpLevel.Text = employeePost.EMPLOYEEPOSTS[0].POSTLEVEL.ToString();

        //        if (cancelLeave.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString() && cancelLeave.EMPLOYEEID != SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID)
        //        {
        //            SetOnlyBrowse();
        //            return;
        //        }

        //        RefreshUI(RefreshedTypes.AuditInfo);
        //        SetToolBar();
        //    }
        //}

        /// <summary>
        /// 获取销假记录更新返回的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeCancelLeaveUpdateCompleted(object sender, EmployeeCancelLeaveUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                string strMsg = e.Result;
                if (!string.IsNullOrWhiteSpace(strMsg) && strMsg != "{SAVESUCCESSED}")
                {
                    strMsg = strMsg.Replace('{', ' ').Replace('}', ' ').Trim();
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(strMsg));
                    return;
                }
                if (cancelLeave.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                }
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 获取销假添加记录返回的信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeCancelLeaveAddCompleted(object sender, EmployeeCancelLeaveAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                string strMsg = e.Result;
                if (!string.IsNullOrWhiteSpace(strMsg) && strMsg != "{SAVESUCCESSED}")
                {
                    strMsg=strMsg.Replace('{',' ').Replace('}',' ').Trim();
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(strMsg));
                    return;
                }
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    CancelLeaveID = cancelLeave.CANCELLEAVEID;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        /// 显示获取到的销假记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeCancelLeaveByIDCompleted(object sender, GetEmployeeCancelLeaveByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

               
                cancelLeave = e.Result;

                if (FormType == FormTypes.Resubmit)
                {
                    cancelLeave.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }

                

                cancelLeave.UPDATEDATE = System.DateTime.Now;
                cancelLeave.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = cancelLeave;
                if (cancelLeave.T_HR_EMPLOYEELEAVERECORD != null && cancelLeave.T_HR_EMPLOYEELEAVERECORD.T_HR_LEAVETYPESET != null)
                {
                    lkEmployeeLeave.DataContext = cancelLeave.T_HR_EMPLOYEELEAVERECORD;
                    client.GetEmployeeLeaveRdListsByLeaveRecordIDAsync(cancelLeave.T_HR_EMPLOYEELEAVERECORD.LEAVERECORDID, string.Empty);
                }
                //perClient.GetEmployeeDetailByIDAsync(cancelLeave.EMPLOYEEID);
                perClient.GetEmpOrgInfoByIDAsync(cancelLeave.OWNERID, cancelLeave.OWNERPOSTID, cancelLeave.OWNERDEPARTMENTID, cancelLeave.OWNERCOMPANYID);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 返回实际请假时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetRealCancelLeaveDayByEmployeeIdAndDateCompleted(object sender, GetRealCancelLeaveDayByEmployeeIdAndDateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.Replace("{", "").Replace("}", "")));
                    return;
                }

                nudTerminateDay.Value = e.dCancelLeaveDay.ToDouble();
                nudTerminateHours.Value = e.dCancelLeaveTime.ToDouble();
                nudTerminateTotalDays.Value = e.dCancelLeaveTotalTime.ToDouble();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                return false;
            }


            if (!string.IsNullOrEmpty(txtTerminateRemark.Text))
            {
                cancelLeave.REMARK = txtTerminateRemark.Text;
            }

            if (cancelLeave.STARTDATETIME == null || cancelLeave.ENDDATETIME == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANCELLEAVEDATEREQUIRED"));
                return false;
            }

            if (cancelLeave.LEAVEDAYS == null && cancelLeave.LEAVEHOURS == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANCELLEAVEDATEREQUIRED"));
                return false;
            }

            if (FormType == FormTypes.New)
            {
                client.EmployeeCancelLeaveAddAsync(cancelLeave);
            }
            else
            {
                cancelLeave.UPDATEDATE = System.DateTime.Now;
                cancelLeave.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.EmployeeCancelLeaveUpdateAsync(cancelLeave);
            }

            return true;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        /// <summary>
        /// 选择已审核通过的请假单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkEmployeeLeave_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEENAME", "EMPLOYEENAME");
            cols.Add("EMPLOYEECODE", "EMPLOYEECODE");
            cols.Add("STARTDATETIME", "STARTDATETIME");
            cols.Add("ENDDATETIME", "ENDDATETIME");
            cols.Add("LEAVEDAYS", "LEAVEDAYS");

            StringBuilder strfilter = new StringBuilder();
            ObservableCollection<object> objArgs = new ObservableCollection<object>();

            strfilter.Append(" EMPLOYEEID == @0");
            strfilter.Append(" && CHECKSTATE == @1");
            objArgs.Add(cancelLeave.EMPLOYEEID);
            objArgs.Add(Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.Approved).ToString());

            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.EmployeeLeaveRecord,
                typeof(List<T_HR_EMPLOYEELEAVERECORD>), cols, strfilter.ToString(), objArgs);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_EMPLOYEELEAVERECORD ent = lookup.SelectedObj as T_HR_EMPLOYEELEAVERECORD;

                if (ent == null)
                {
                    return;
                }

                lkEmployeeLeave.DataContext = ent;
                cancelLeave.T_HR_EMPLOYEELEAVERECORD = ent;
                this.GetTxtCancelRecord(ent.T_HR_EMPLOYEECANCELLEAVE);
                dpStartDate.Value = ent.STARTDATETIME;
                dpEndDate.Value = ent.ENDDATETIME;
                
                if (ent.LEAVEDAYS != null)
                {
                    nudLeaveDay.Value = ent.LEAVEDAYS.Value.ToDouble();
                }
                
                if (ent.LEAVEHOURS != null)
                {
                    nudLeaveHours.Value = ent.LEAVEHOURS.Value.ToDouble();
                }
                
                if (ent.TOTALHOURS != null)
                {
                    nudTotalDays.Value = ent.TOTALHOURS.Value.ToDouble();
                }

            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 把对应请假的销假记录显示到销假记录框中
        /// </summary>
        /// <param name="ent"></param>
        private void GetTxtCancelRecord(ObservableCollection<T_HR_EMPLOYEECANCELLEAVE> empCancels)
        {
            string strMsg = string.Empty;
            try
            {
                List<T_HR_EMPLOYEECANCELLEAVE> emps = empCancels.OrderBy(t => t.STARTDATETIME).ToList();
                emps.ForEach(it =>
                {
                    if (it.CHECKSTATE == "1" || it.CHECKSTATE == "2")
                    {
                        string strCheck = string.Empty;
                        switch (it.CHECKSTATE)
                        {
                            case "0": strCheck = "  未提交"; break;
                            case "1": strCheck = "  审核中"; break;
                            case "2": strCheck = "  审核通过"; break;
                            default: strCheck = "这是什么状态";
                                break;
                        }
                        strMsg += "销假起止时间：";
                        strMsg += Convert.ToString(it.STARTDATETIME) + " — " + Convert.ToString(it.ENDDATETIME) + strCheck  + "\n";
                    }
                });
                this.txtCancelRecord.Text = strMsg;
            }
            catch (Exception)
            {
              
            }
        }

        /// <summary>
        /// 根据起始时间，计算销假时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpTerminateStartDate_OnValueChanged(object sender, EventArgs e)
        {
            CalculateDayCount();
        }

        /// <summary>
        /// 根据截止时间，计算销假时长
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dpTerminateEndDate_OnValueChanged(object sender, EventArgs e)
        {
            CalculateDayCount();
        }
        #endregion        
    }
}
