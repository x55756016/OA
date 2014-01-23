/*
 * 文件名：AttSolRdForm.xaml.cs
 * 作  用：考勤方案应用表单，实现如下功能：新增，编辑，审核
 * 创建人：吴鹏
 * 创建时间：2010年1月19日, 16:31:45
 * 修改人：
 * 修改时间：
 */

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
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.AttendanceWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using System.Text;
using SMT.SaaS.LocalData;
using SMT.HRM.UI.OutApplyWS;
using SMT.SaaS.MobileXml;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class NoAttendCardEmployeesForm : BaseForm, IEntityEditor, IAudit
    {
        #region 全局变量
        //类型
        /// <summary>
        /// 
        /// </summary>
        public FormTypes FormType { get; set; }
        /// <summary>
        /// 考勤方案应用主键
        /// </summary>
        public string AttendanceSolutionAsignID { get; set; }
        /// <summary>
        /// 免打卡人员表单
        /// </summary>
        public T_HR_NOATTENDCARDEMPLOYEES entity { get; set; }

        OutAppliecrecordServiceClient clientAtt = new OutAppliecrecordServiceClient();
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient clientOrg = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient clientPer = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        private string strResMsg = string.Empty;
        /// <summary>
        /// 是否关闭窗体 false 表示不关闭
        /// </summary>
        private bool closeFormFlag = false;
        #endregion

        #region 初始化
        public NoAttendCardEmployeesForm(FormTypes formtype, string strAttendanceSolutionAsignID)
        {
            InitializeComponent();
            //传过来的类型
            FormType = formtype;
            //考勤方案应用主键
            AttendanceSolutionAsignID = strAttendanceSolutionAsignID;
            //注册事件
            RegisterEvents();
            //页面初始化
            InitParas();
        }


        /// <summary>
        /// 注册事件
        /// </summary>
        private void RegisterEvents()
        {
            clientAtt.GetNoAttendCardEmployeesByIDCompleted += clientAtt_GetNoAttendCardEmployeesByIDCompleted;
            clientAtt.AddNoAttendCardEmployeesRdCompleted += clientAtt_AddNoAttendCardEmployeesRdCompleted;
            //clientAtt.ModifyOverTimeRdCompleted += clientAtt_ModifyOverTimeRdCompleted;
            clientAtt.UpdateNoAttendCardEmployeesRdCompleted += clientAtt_UpdateNoAttendCardEmployeesRdCompleted;
            //clientAtt.AuditOverTimeRdCompleted += clientAtt_AuditOverTimeRdCompleted;


            clientOrg.GetPostByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs>(clientOrg_GetPostByIdCompleted);
            clientOrg.GetDepartmentByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs>(clientOrg_GetDepartmentByIdCompleted);
            clientOrg.GetCompanyByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs>(clientOrg_GetCompanyByIdCompleted);            
            clientPer.GetEmployeeByIDsCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeByIDsCompletedEventArgs>(clientPer_GetEmployeeByIDsCompleted);

            this.Loaded += new RoutedEventHandler(AttendanceSolutionAsignForm_Loaded);
        }

       

        /// <summary>
        /// 加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AttendanceSolutionAsignForm_Loaded(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            
        }        

        /// <summary>
        /// 页面初始化
        /// </summary>
        private void InitParas()
        {
            //如果登录人=null
            if (Common.CurrentLoginUserInfo.UserPosts.Count() == 0)
            {
                ToolbarItems = new List<ToolbarItem>();
                RefreshUI(RefreshedTypes.All);
                this.IsEnabled = false;
                return;
            }
            SetDateTime(null,null);
            if (FormType == FormTypes.New)
            {
                InitForm();
                SetToolBar();
            }
            else
            {
                LoadData();
                if (FormType == FormTypes.Browse)
                {
                    //dpStarDate.IsEnabled = false;
                   // dpEndDate.IsEnabled = false;
                    txtRemark.IsReadOnly = true;
                    //this.IsEnabled = false;//这里this.IsEnabled会定义该窗口所有控件的IsEnabled属性，有好处，有坏处

                }
            }
        }


        /// <summary>
        /// 表单初始化
        /// </summary>
        private void InitForm()
        {
            entity = new T_HR_NOATTENDCARDEMPLOYEES();
            entity.NOATTENDCARDEMPLOYEESID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entity.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entity.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;            

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entity.CREATEDATE = DateTime.Now;
            entity.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entity.UPDATEDATE = System.DateTime.Now;
            entity.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            //entity.ASSIGNEDOBJECTTYPE = "0";
            entity.EMPLOYEEID = string.Empty;
            //entity.T_HR_ATTENDANCESOLUTION = new T_HR_ATTENDANCESOLUTION();

            entity.STARTDATE = DateTime.Parse(DateTime.Now.AddMonths(1).ToString("yyyy-MM") + "-01");
            entity.ENDDATE = DateTime.Parse(DateTime.Now.Year + "-12-31");

            //审核
            entity.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();

            this.DataContext = entity;
        }

        /// <summary>
        /// 编辑状态下，加载表单数据
        /// </summary>
        private void LoadData()
        {
            if (string.IsNullOrEmpty(AttendanceSolutionAsignID))
            {
                return;
            }

            RefreshUI(RefreshedTypes.ShowProgressBar);

            clientAtt.GetNoAttendCardEmployeesByIDAsync(AttendanceSolutionAsignID);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("免打卡人员名单");
        }

        public string GetStatus()
        {
            string strTemp = string.Empty;
            if (!string.IsNullOrEmpty(AttendanceSolutionAsignID))
                strTemp = "编辑中";

            return strTemp;
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
                case "3":
                    Submit();
                    break;
            }
        }

        private void Submit()
        {
            string state =string.Empty;
            state = Utility.GetCheckState(CheckStates.Approved);
            //Save();
            clientAtt.AuditOutApplyAsync(AttendanceSolutionAsignID, state);
        }

        private void RefreshUI(RefreshedTypes type)
        {
            if (OnUIRefreshed != null)
            {
                UIRefreshedEventArgs args = new UIRefreshedEventArgs();
                args.RefreshedType = type;
                OnUIRefreshed(this, args);
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

        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T FlowEntity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("POSTLEVEL",Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString());
            para.Add("OWNERPOSTNAME", Common.CurrentLoginUserInfo.UserPosts[0].PostName.ToString());
            FlowEntity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_NOATTENDCARDEMPLOYEES>(OvertimeRecord, para, "HR");
            if (!string.IsNullOrEmpty(FlowEntity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(FlowEntity.BusinessObjectDefineXML, entity);
            Utility.SetAuditEntity(FlowEntity, "T_HR_NOATTENDCARDEMPLOYEES", entity.NOATTENDCARDEMPLOYEESID, strXmlObjectSource);
                        
        }

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_NOATTENDCARDEMPLOYEES Info)
        {
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;
            
            decimal? postlevelValue = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel;
            string postLevelName = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == postlevelValue).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;
                        
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "OWNERPOSTNAME", Common.CurrentLoginUserInfo.UserPosts[0].PostName, Common.CurrentLoginUserInfo.UserPosts[0].PostName));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "POSTLEVEL", postlevelValue.ToString(), postlevelValue.ToString()));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "EMPLOYEENAME", Info.EMPLOYEENAME, Info.EMPLOYEENAME));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "OWNERCOMPANYID", Info.OWNERCOMPANYID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyName));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName));
            AutoList.Add(basedata("T_HR_NOATTENDCARDEMPLOYEES", "OWNERPOSTID", Info.OWNERPOSTID, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostName));
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
            entity.CHECKSTATE = state;
            //entAttendanceSolutionAsign.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            bool bl= Save();

            if (bl)
            {
                //clientAtt.AuditAttSolAsignAsync(AttendanceSolutionAsignID, state);
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (entity != null)
                state = entity.CHECKSTATE;
            return state;
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_NOATTENDCARDEMPLOYEES", entity.OWNERID,
                    entity.OWNERPOSTID, entity.OWNERDEPARTMENTID, entity.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 分配对象为员工时，检查选择的多个员工是否为同一公司
        /// </summary>
        /// <param name="ents"></param>
        private void CheckSelectedObj(List<ExtOrgObj> ents)
        {
            string strIds = string.Empty, strSelEmpNames = string.Empty, strMsg = string.Empty;

            List<string> entCompanyIds = new List<string>();
            List<string> entPostIds = new List<string>();
            List<string> entDepartmentIds = new List<string>();
           
            foreach (ExtOrgObj item in ents)
            {
              
                ExtOrgObj objPost = (ExtOrgObj)item.ParentObject;
                if (objPost == null)
                {
                    continue;
                }
              
                SMT.Saas.Tools.OrganizationWS.T_HR_POST entPost = objPost.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                if (entPost == null)
                {
                    continue;
                }
                
                //这句阻断，不知道为什么
                //if (entPost.T_HR_DEPARTMENT == null)
                //{
                //    continue;
                //}
                
                ExtOrgObj objDepartment = (ExtOrgObj)objPost.ParentObject;
                if (objDepartment == null)
                {
                    continue;
                }
                
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT entDepartment = objDepartment.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                if (entDepartment.T_HR_COMPANY == null)
                {
                    continue;
                }
                
                if (entPostIds.Contains(entPost.POSTID) == false)
                {
                    
                    entPostIds.Add(entPost.POSTID);
                }
                
                if (entDepartmentIds.Contains(entDepartment.DEPARTMENTID) == false)
                {
                    
                    entDepartmentIds.Add(entDepartment.DEPARTMENTID);
                }
                
                if (entCompanyIds.Contains(entDepartment.T_HR_COMPANY.COMPANYID) == false)
                {
                   
                    entCompanyIds.Add(entDepartment.T_HR_COMPANY.COMPANYID);
                }
                
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE entEmp = item.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                strIds += entEmp.EMPLOYEEID + ",";
                strSelEmpNames += entDepartment.T_HR_COMPANY.CNAME + "-"
                    + entDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME + "-"
                    + entPost.T_HR_POSTDICTIONARY.POSTNAME +"-"+ entEmp.EMPLOYEECNAME + ";";
               
            }

            lkAssignObject.TxtLookUp.Text = strSelEmpNames;

            //entity.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString();
            entity.EMPLOYEEID = strIds;
            entity.EMPLOYEENAME = strSelEmpNames;

          

            SetRecordOwner(entCompanyIds[0]);
        }

        /// <summary>
        /// 根据分配对象，更新考勤方案归属(OwnerPostId, OwnerDepartmentId, OwnerCompanyId)
        /// </summary>
        /// <param name="strCheckCompanyId"></param>
        private void SetRecordOwner(string strCheckCompanyId)
        {
            var q = from p in Common.CurrentLoginUserInfo.UserPosts
                    where p.CompanyID == strCheckCompanyId
                    select p;

            if (q.Count() == 0)
            {
                txtErrorMsg.Text = "NOPERMISSIONASIGNATTSOLOBJ";
                return;
            }

            UserPost entCurrOwnerUserPost = q.FirstOrDefault();
            if (entCurrOwnerUserPost == null)
            {
                txtErrorMsg.Text = "NOPERMISSIONASIGNATTSOLOBJ";
                return;
            }

            if (string.IsNullOrWhiteSpace(entCurrOwnerUserPost.PostID) || string.IsNullOrWhiteSpace(entCurrOwnerUserPost.DepartmentID) || string.IsNullOrWhiteSpace(entCurrOwnerUserPost.CompanyID))
            {
                txtErrorMsg.Text = "NOPERMISSIONASIGNATTSOLOBJ";
                return;
            }

            entity.OWNERPOSTID = entCurrOwnerUserPost.PostID;
            entity.OWNERDEPARTMENTID = entCurrOwnerUserPost.DepartmentID;
            entity.OWNERCOMPANYID = entCurrOwnerUserPost.CompanyID;
        }



        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = true;
            RefreshUI(RefreshedTypes.ShowProgressBar);

            try
            {
                string strStartTime = inputStartYear.Value + "-" + inputStartMonth.Value + "-1";//月初日期字符串
                DateTime startTime = Convert.ToDateTime(strStartTime);
                string strEndTime = inputEndYear.Value + "-" + inputEndMonth.Value + "-" + DateTime.DaysInMonth(Convert.ToInt32(inputEndYear.Value), Convert.ToInt32(inputEndMonth.Value)) + " 23:59:58";//月底日期字符串
                DateTime endTime = Convert.ToDateTime(strEndTime);
                if (startTime >= endTime)
                {
                    flag = false;
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "起始时间要大于结束时间");
                }
                if (string.IsNullOrEmpty(lkAssignObject.TxtLookUp.Text))
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "请先选择免打卡员工！");
                    flag = false;
                }
                if (txtRemark.Text.Length >= 2000 || lkAssignObject.TxtLookUp.Text.Length >= 2000)
                {
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "备注框或选择的员工框字符长度数已超过数据库字段的最大长度！");
                    flag = false;
                }
                entity.STARTDATE = startTime;
                entity.ENDDATE = endTime;
                entity.REMARK = this.txtRemark.Text;
                if (FormType == FormTypes.Edit)
                {
                    entity.UPDATEDATE = DateTime.Now;
                    entity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                }

                if (!flag)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddNoAttendCardEmployeesRdAsync(entity);
                }
                else
                {
                    clientAtt.UpdateNoAttendCardEmployeesRdAsync(entity);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }

            return flag;
        }

        /// <summary>
        /// 关闭当前窗口
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

        #region 事件

        void clientAtt_AddNoAttendCardEmployeesRdCompleted(object sender, AddNoAttendCardEmployeesRdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "Sucess")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "提示", "保存成功");
                    FormType = FormTypes.Edit;
                    AttendanceSolutionAsignID = entity.NOATTENDCARDEMPLOYEESID;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);

                    if (closeFormFlag)
                    {
                        CloseForm();
                        return;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        void clientAtt_UpdateNoAttendCardEmployeesRdCompleted(object sender, UpdateNoAttendCardEmployeesRdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "Sucess")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "提示", "保存成功");

                    if (closeFormFlag)
                    {
                        CloseForm();
                        return;
                    }

                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' })));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.All);
        }

        void clientAtt_GetNoAttendCardEmployeesByIDCompleted(object sender, GetNoAttendCardEmployeesByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entity = e.Result;
                SetDateTime(entity.STARTDATE, entity.ENDDATE);
                if (FormType == FormTypes.Resubmit)
                {
                    entity.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }

                this.DataContext = entity;


                string[] sArray = entity.EMPLOYEEID.Split(',');
                ObservableCollection<string> strIds = new ObservableCollection<string>();
                foreach (string strId in sArray)
                {
                    strIds.Add(strId);
                }
                clientPer.GetEmployeeByIDsAsync(strIds);
                lkAssignObject.TxtLookUp.Text = entity.EMPLOYEENAME;
                lkAssignObject.IsEnabled = false;
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }

            RefreshUI(RefreshedTypes.AuditInfo);
            SetToolBar();
        }


        /// <summary>
        /// 设置时间
        /// </summary>
        private void SetDateTime(DateTime? start, DateTime? end)
        {
            int startYear = DateTime.Now.Year, startMonth = DateTime.Now.Month, endYear = DateTime.Now.Year, endMonth = DateTime.Now.Month;//分别是开始年月和结束年月
            if (start != null && end != null)
            {
                startYear = start.Value.Year;
                startMonth = start.Value.Month;
                endYear = end.Value.Year;
                endMonth = end.Value.Month;//分别是开始年月和结束年月
            }
          //  inputStartYear.
            inputStartYear.Value = Convert.ToDouble(startYear);
            inputStartMonth.Value = Convert.ToDouble(startMonth);
            inputEndYear.Value = Convert.ToDouble(endYear);
            inputEndMonth.Value = Convert.ToDouble(endMonth);
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为公司）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetCompanyByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetCompanyByIdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                OrganizationWS.T_HR_COMPANY entCompany = e.Result as OrganizationWS.T_HR_COMPANY;
                lkAssignObject.DataContext = entCompany;
                lkAssignObject.DisplayMemberPath = "CNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为部门）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetDepartmentByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetDepartmentByIdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                OrganizationWS.T_HR_DEPARTMENT entDepartment = e.Result as OrganizationWS.T_HR_DEPARTMENT;
                lkAssignObject.DataContext = entDepartment;
                lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为岗位）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientOrg_GetPostByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                OrganizationWS.T_HR_POST entPost = e.Result as OrganizationWS.T_HR_POST;
                lkAssignObject.DataContext = entPost;
                lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        /// <summary>
        /// 根据应用对象类型，对应用对象赋值（当前类型为员工）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientPer_GetEmployeeByIDsCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeByIDsCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == null)
                {
                    return;
                }

                List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> entList = e.Result.ToList();

                string strIds = string.Empty, strNames = string.Empty;
                
                foreach (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE item in entList)
                {
                    strIds += item.EMPLOYEEID + ",";
                    strNames += item.EMPLOYEECNAME + ";";
                }

                tbAssignedObjectID.Text = strIds;
                
                lkAssignObject.IsEnabled = true;
                lkAssignObject.SearchButton.IsEnabled = false;
                lkAssignObject.TipTextValue = strNames;//设置Tip
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }


        /// <summary>
        /// 选择考勤方案应用对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAssignObject_FindClick(object sender, EventArgs e)
        { 
            OrganizationLookup lookup = new OrganizationLookup();           
            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.MultiSelected = true;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ents = lookup.SelectedObj as List<ExtOrgObj>;
                if (ents == null)
                {
                    return;
                }

                if (ents.Count() == 0)
                {
                    return;
                }
               
                txtErrorMsg.Text = string.Empty;
                string strCheckCompanyId = string.Empty;


                if (lookup.SelectedObjType == OrgTreeItemTypes.Personnel)
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    if (ent == null)
                    {
                        return;
                    }
                    CheckSelectedObj(ents);

                }
                else
                {
                    MessageBox.Show("只能选择员工！");
                }
                
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        #endregion

    }
}
