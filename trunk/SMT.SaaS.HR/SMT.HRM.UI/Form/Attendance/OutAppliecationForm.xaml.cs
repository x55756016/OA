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

using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.AttendanceWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.MobileXml;
using SMT.HRM.UI.OutApplyWS;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class OutAppliecationForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        public FormTypes FormType { get; set; }
        public string OverTimeRecordID { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        public T_HR_EMPLOYEEOUTAPPLIECRECORD OvertimeRecord { get; set; }
        public T_HR_ATTENDANCESOLUTION solution { get; set; }
        OutAppliecrecordServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient perClient;
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region 初始化
        //Modified by:Sam
        //Date:2011-10-9
        //For:增加一个无参数的构造函数来实现待办任务新建单据
        public OutAppliecationForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            OverTimeRecordID = string.Empty;
            this.Loaded += new RoutedEventHandler(OverTimeForm_Loaded);
        }

        public OutAppliecationForm(FormTypes ftType, string strOverTimeRecordID)
        {
            InitializeComponent();
            FormType = ftType;
            OverTimeRecordID = strOverTimeRecordID;
            this.Loaded += new RoutedEventHandler(OverTimeForm_Loaded);
        }

        void OverTimeForm_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            InitParas();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            client = new OutAppliecrecordServiceClient();
            client.AddOverTimeRdCompleted += client_AddOverTimeRdCompleted;
            client.ModifyOverTimeRdCompleted += client_ModifyOverTimeRdCompleted;
            client.AuditOverTimeRdCompleted += client_AuditOverTimeRdCompleted;
            client.GetOverTimeRdByIDCompleted += client_GetOverTimeRdByIDCompleted;
            perClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            perClient.GetEmpOrgInfoByIDCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs>(perClient_GetEmpOrgInfoByIDCompleted);
        }
       
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {//OvertimeRecord.ENDDATE
            return "外出申请";
        }

        public string GetStatus()
        {
            return "";
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);

            return items;
        }

        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }


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

        #region IAudit 成员 审核

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEEOUTAPPLIECRECORD Info)
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

            //decimal? payCatogryValue = Convert.ToDecimal(Info.PAYCATEGORY);
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY PayCatogryDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PAYCATEGORY" && s.DICTIONARYVALUE == payCatogryValue).FirstOrDefault();

            decimal? overTimeValue = Convert.ToDecimal(Info.OVERTIMECATE);
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY overTimeDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "OVERTIMECATE" && s.DICTIONARYVALUE == overTimeValue).FirstOrDefault();

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "OWNERPOSTNAME", tbOrgName.Text.Trim(), tbOrgName.Text.Trim()));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "POSTLEVEL", tbEmpLevel.Text.Trim(), postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "EMPLOYEENAME", Info.EMPLOYEENAME, tbEmpName.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));
            //AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "PAYCATEGORY", Info.PAYCATEGORY, PayCatogryDict == null ? "" : PayCatogryDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEOUTAPPLIECRECORD", "OVERTIMECATE", Info.OVERTIMECATE, overTimeDict == null ? "" : overTimeDict.DICTIONARYNAME));
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

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTLEVEL", tbEmpLevel.Text.Trim());
            para.Add("OWNERPOSTNAME", tbOrgName.Text.Trim());
            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEOUTAPPLIECRECORD>(OvertimeRecord, para, "HR");
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, OvertimeRecord);
            Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEOUTAPPLIECRECORD", OvertimeRecord.OVERTIMERECORDID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = string.Empty;
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

            OvertimeRecord.CHECKSTATE = state;
            if (FormType == FormTypes.Edit )
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "提交成功");
            }
            if (FormType == FormTypes.Audit )            
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), "审核成功");
                                
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //By  : Sam
            //Date: 2011-9-6
            //For : 前台此处调了一次服务，在BLL层的UpdateCheckState方法里面又调了一次，会导致很多问题,所以这里屏蔽掉
            //string state = string.Empty;
            //switch (args)
            //{
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
            //        state = Utility.GetCheckState(CheckStates.Approving);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
            //        state = Utility.GetCheckState(CheckStates.Approved);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
            //        state = Utility.GetCheckState(CheckStates.UnApproved);
            //        break;
            //}
            //OvertimeRecord.CHECKSTATE = state;
            //client.AuditOverTimeRdAsync(OvertimeRecord.OV  ERTIMERECORDID, state);
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (OvertimeRecord != null)
                state = OvertimeRecord.CHECKSTATE;
            return state;
        }

        /// <summary>
        /// 审核外出申请记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AuditOverTimeRdCompleted(object sender, OutApplyWS.AuditOverTimeRdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                FormType = FormTypes.Audit;
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            RefreshUI(RefreshedTypes.All);
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取外出申请申请人员的员工信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void perClient_GetEmpOrgInfoByIDCompleted(object sender, Saas.Tools.PersonnelWS.GetEmpOrgInfoByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null  || e.Result == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW employeeView = e.Result;
                    tbEmpName.Text = employeeView.EMPLOYEECNAME;
                    tbOrgName.Text = employeeView.POSTNAME + " - " + employeeView.DEPARTMENTNAME + " - " + employeeView.COMPANYNAME;
                    if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
                    {
                        tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
                    }
                    tbEmpLevel.Text = employeeView.POSTLEVEL.ToString();
                    this.IsEnabled = true;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();

                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message + ex.Message));
            }
        }       

        /// <summary>
        /// 加载页面信息
        /// </summary>
        private void InitParas()
        {
            string strOwnerID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            this.IsEnabled = false;
            if (FormType == FormTypes.New)
            {
                InitBaseInf();

                OvertimeRecord = new T_HR_EMPLOYEEOUTAPPLIECRECORD();
                OvertimeRecord.OVERTIMERECORDID = Guid.NewGuid().ToString();
                OvertimeRecord.EMPLOYEEID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //OvertimeRecord.EMPLOYEECODE = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeCode;
                OvertimeRecord.EMPLOYEENAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                OvertimeRecord.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();
                OvertimeRecord.STARTDATE = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 18:00:00");
                OvertimeRecord.ENDDATE = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd") + " 22:00:00");

                //权限控制
                OvertimeRecord.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                OvertimeRecord.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                OvertimeRecord.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                OvertimeRecord.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                OvertimeRecord.CREATEDATE = DateTime.Now;
                OvertimeRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                OvertimeRecord.UPDATEDATE = System.DateTime.Now;
                OvertimeRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                this.DataContext = OvertimeRecord;
                //client.GetAttendanceSolutionByEmployeeIDAsync(strOwnerID);
                this.IsEnabled = true;
                SetToolBar();
            }
            else
            {
                client.GetOverTimeRdByIDAsync(OverTimeRecordID);
            }
        }

        /// <summary>
        /// 加载申请单员工信息
        /// </summary>
        public void InitBaseInf()
        {
            tbEmpName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
            tbOrgName.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + " - " + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            tbEmpLevel.Text = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
            if (!string.IsNullOrWhiteSpace(tbOrgName.Text))
            {
                tbEmpName.Text = tbEmpName.Text + " - " + tbOrgName.Text;
            }
        }

        /// <summary>
        /// 设置可用表单按钮
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
                ToolbarItems.Clear();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEOUTAPPLIECRECORD", OvertimeRecord.OWNERID,
                    OvertimeRecord.OWNERPOSTID, OvertimeRecord.OWNERDEPARTMENTID, OvertimeRecord.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 计算申请外出申请的时长
        /// </summary>
        private void CalculateDayCount()
        {
            OvertimeRecord.OVERTIMEHOURS = "0";
            if (dpStartDate.Value == null || dpEndDate.Value == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tbWorkTimePerDay.Text))
            {
                return;
            }

            decimal dWorkTimePerDay = 0;
            decimal.TryParse(tbWorkTimePerDay.Text, out dWorkTimePerDay);

            DateTime dtStart = dpStartDate.Value.Value;
            DateTime dtEnd = dpEndDate.Value.Value;

            if (dtEnd < dtStart)
            {
                return;
            }

            TimeSpan ts = dtEnd.Subtract(dtStart);
            OvertimeRecord.OVERTIMEHOURS = (ts.Days * dWorkTimePerDay + ts.Hours).ToString();
            //OvertimeRecord.STARTDATETIME = dtStart.ToString("hh:mm:ss");
            //OvertimeRecord.ENDDATETIME = dtEnd.ToString("hh:mm:ss");
        }

        /// <summary>
        /// 保存并关闭
        /// </summary>
        private void Cancel()
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

        #region 增删改查

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

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            bool flag = false;
            RefreshUI(RefreshedTypes.ShowProgressBar);

            if (dpStartDate.Value == null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("STARTDATE"), Utility.GetResourceStr("REQUIRED", "STARTDATE"));
                return flag;
            }

            if (dpEndDate.Value == null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("STARTDATE"), Utility.GetResourceStr("REQUIRED", "ENDDATE"));
                return flag;
            }

            if (Convert.ToDateTime(dpEndDate.Value) < Convert.ToDateTime(dpStartDate.Value))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("外出申请结束时间小于开始时间"));
                return flag;
            }
            if (Convert.ToDateTime(dpEndDate.Value) == Convert.ToDateTime(dpStartDate.Value))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("外出申请时长为0"));
                return flag;
            }
            if (FormType == FormTypes.Edit)
            {
                OvertimeRecord.UPDATEDATE = System.DateTime.Now;
                OvertimeRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.ModifyOverTimeRdAsync(OvertimeRecord);
            }
            else if (FormType == FormTypes.New)
            {
                OvertimeRecord.OVERTIMECATE = "1";
                //OvertimeRecord.PAYCATEGORY = "1";
                OvertimeRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                OvertimeRecord.CREATEDATE = DateTime.Now;
                client.AddOverTimeRdAsync(OvertimeRecord);
            }

            return true;
        }

        ///// <summary>
        ///// 获取外出申请记录
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        void client_GetOverTimeRdByIDCompleted(object sender, OutApplyWS.GetOverTimeRdByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                OvertimeRecord = e.Result;
                this.DataContext = OvertimeRecord;

                if (OvertimeRecord == null)
                {
                    this.IsEnabled = false;
                }

                string strLoginUserId = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                if (OvertimeRecord.EMPLOYEEID != strLoginUserId || OvertimeRecord.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    this.IsEnabled = false;
                }
                perClient.GetEmpOrgInfoByIDAsync(OvertimeRecord.OWNERID, OvertimeRecord.OWNERPOSTID, OvertimeRecord.OWNERDEPARTMENTID, OvertimeRecord.OWNERCOMPANYID);
            }
        }


        /// <summary>
        /// 更新外出申请记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_ModifyOverTimeRdCompleted(object sender, OutApplyWS.ModifyOverTimeRdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(e.Result))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr(e.Result.Replace("{", "").Replace("}", "")));
                    if (closeFormFlag)
                    {
                        RefreshUI(RefreshedTypes.Close);
                    }
                }
                else
                {
                    if (OvertimeRecord.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
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
                    RefreshUI(RefreshedTypes.All);
                }
            }
        }

        /// <summary>
        /// 添加外出申请记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_AddOverTimeRdCompleted(object sender, OutApplyWS.AddOverTimeRdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(e.Result))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.Replace("{", "").Replace("}", "")));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    if (closeFormFlag)
                    {
                        RefreshUI(RefreshedTypes.Close);
                    }
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
            RefreshUI(RefreshedTypes.All);
        }

        #endregion

             
    }
}

