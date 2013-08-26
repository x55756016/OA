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

namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendanceSolutionAsignForm : BaseForm, IEntityEditor, IAudit
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
        /// 考勤方案应用实体
        /// </summary>
        public T_HR_ATTENDANCESOLUTIONASIGN entAttendanceSolutionAsign { get; set; }

        private AttendanceServiceClient clientAtt = new AttendanceServiceClient();
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient clientOrg = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient clientPer = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private List<UserPost> entUserPosts = new List<UserPost>();

        private string strResMsg = string.Empty;
        /// <summary>
        /// 是否关闭窗体 false 表示不关闭
        /// </summary>
        private bool closeFormFlag = false;
        #endregion

        #region 初始化
        public AttendanceSolutionAsignForm(FormTypes formtype, string strAttendanceSolutionAsignID)
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
            clientAtt.GetAttendanceSolutionAsignByIDCompleted += new EventHandler<GetAttendanceSolutionAsignByIDCompletedEventArgs>(clientAtt_GetAttendanceSolutionAsignByIDCompleted);
            clientAtt.AddAttendanceSolutionAsignCompleted += new EventHandler<AddAttendanceSolutionAsignCompletedEventArgs>(clientAtt_AddAttendanceSolutionAsignCompleted);
            clientAtt.ModifyAttendanceSolutionAsignCompleted += new EventHandler<ModifyAttendanceSolutionAsignCompletedEventArgs>(clientAtt_ModifyAttendanceSolutionAsignCompleted);
            clientAtt.AuditAttSolAsignCompleted += new EventHandler<AuditAttSolAsignCompletedEventArgs>(clientAtt_AuditAttSolAsignCompleted);

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
            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts != null)
            {
                //登录人
                entUserPosts = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts;
            }
            //如果登录人=null
            if (entUserPosts.Count() == 0)
            {
                ToolbarItems = new List<ToolbarItem>();
                RefreshUI(RefreshedTypes.All);
                this.IsEnabled = false;
                return;
            }

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
                    dpStarDate.IsEnabled = false;
                    dpEndDate.IsEnabled = false;
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
            entAttendanceSolutionAsign = new T_HR_ATTENDANCESOLUTIONASIGN();
            entAttendanceSolutionAsign.ATTENDANCESOLUTIONASIGNID = System.Guid.NewGuid().ToString().ToUpper();

            //权限控制
            entAttendanceSolutionAsign.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entAttendanceSolutionAsign.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entAttendanceSolutionAsign.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entAttendanceSolutionAsign.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;            

            //2010年2月11日, 11:37:35,目前暂未实现登录部分，人员相关数据为假定值
            entAttendanceSolutionAsign.CREATEDATE = DateTime.Now;
            entAttendanceSolutionAsign.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            entAttendanceSolutionAsign.UPDATEDATE = System.DateTime.Now;
            entAttendanceSolutionAsign.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //页面初始化加载的临时数据
            entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE = "0";
            entAttendanceSolutionAsign.ASSIGNEDOBJECTID = string.Empty;
            entAttendanceSolutionAsign.T_HR_ATTENDANCESOLUTION = new T_HR_ATTENDANCESOLUTION();

            entAttendanceSolutionAsign.STARTDATE = DateTime.Parse(DateTime.Now.AddMonths(1).ToString("yyyy-MM") + "-01");
            entAttendanceSolutionAsign.ENDDATE = DateTime.Parse(DateTime.Now.Year + "-12-31");

            //审核
            entAttendanceSolutionAsign.CHECKSTATE = Convert.ToInt32(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit).ToString();

            this.DataContext = entAttendanceSolutionAsign;
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

            clientAtt.GetAttendanceSolutionAsignByIDAsync(AttendanceSolutionAsignID);
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDANCESOLUTIONASIGNFORM");
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
            clientAtt.AuditAttSolAsignAsync(AttendanceSolutionAsignID, state);
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

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            string strKeyName = "ATTENDANCESOLUTIONASIGNID";
            string strKeyValue = entAttendanceSolutionAsign.ATTENDANCESOLUTIONASIGNID;

            Dictionary<string, string> paraValue = new Dictionary<string, string>();
            paraValue.Add("EntityKey", entAttendanceSolutionAsign.ATTENDANCESOLUTIONASIGNID);

            Dictionary<string, string> paraText = new Dictionary<string, string>();
            paraText.Add("T_HR_ATTENDANCESOLUTIONReference", entAttendanceSolutionAsign.T_HR_ATTENDANCESOLUTION == null ? "" : entAttendanceSolutionAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONID + "#" + entAttendanceSolutionAsign.T_HR_ATTENDANCESOLUTION.ATTENDANCESOLUTIONNAME);
            strXmlObjectSource = Utility.ObjListToXml<T_HR_ATTENDANCESOLUTIONASIGN>(entAttendanceSolutionAsign, paraValue, "HR", paraText, strKeyName, strKeyValue);

            Dictionary<string, string> paras = new Dictionary<string, string>();
            paras["CreateCompanyID"] = entAttendanceSolutionAsign.OWNERCOMPANYID;
            paras["CreateDepartmentID"] = entAttendanceSolutionAsign.OWNERDEPARTMENTID;
            paras["CreatePostID"] = entAttendanceSolutionAsign.OWNERPOSTID;
            paras["CreateUserID"] = entAttendanceSolutionAsign.OWNERID;

            AddXmlInfo(ref strXmlObjectSource);
            Utility.SetAuditEntity(entity, "T_HR_ATTENDANCESOLUTIONASIGN", AttendanceSolutionAsignID, strXmlObjectSource, paras);
           
            
        }
        /// <summary>
        /// 2012-09-25
        /// Utility.ObjListToXml这个方法里面有一些参数没写进去，生成传入流程的xml文件不完整
        /// 使得我的单据中没有考取方案分配，缺的是
        ///  <System>
        ///  <Function Description="考勤方案应用" Address="EngineService.svc"   FuncName="CallApplicationService"  Binding="customBinding" SplitChar="Г">
        ///    <ParaStr>
        ///      <Para TableName="T_HR_ATTENDANCESOLUTIONASIGN" Name="ATTENDANCESOLUTIONASIGNID" Description="员工考勤方案ID" Value=""></Para>
        ///    </ParaStr>
        ///  </Function>
        ///</System>
        ///<MsgOpen>
        ///  <AssemblyName>SMT.HRM.UI</AssemblyName>
        ///  <PublicClass>SMT.HRM.UI.Utility</PublicClass>
        ///  <ProcessName>CreateFormFromEngine</ProcessName>
        ///  <PageParameter>SMT.HRM.UI.Form.Attendance.AttendanceSolutionAsignForm</PageParameter>
        ///  <ApplicationOrder>{ATTENDANCESOLUTIONASIGNID}</ApplicationOrder>
        ///  <FormTypes>Audit</FormTypes>
        ///</MsgOpen>
        /// </summary>
        /// <param name="strXmlObjectSource"></param>
        private void AddXmlInfo(ref string strXmlObjectSource)
        {
           
            StringBuilder strAddXml = new StringBuilder();
            strAddXml.AppendLine("<Name>HR</Name>");
            strAddXml.AppendLine("<System>");
            strAddXml.AppendLine(" <Function Description=\"考勤方案应用\" Address=\"EngineService.svc\"   FuncName=\"CallApplicationService\"");
            strAddXml.AppendLine("Binding=\"customBinding\" SplitChar=\"Г\">");
            strAddXml.AppendLine("<ParaStr>");
            strAddXml.AppendLine("<Para TableName=\"T_HR_ATTENDANCESOLUTIONASIGN\" Name=\"ATTENDANCESOLUTIONASIGNID\" Description=\"员工考勤方案ID\"");
            strAddXml.AppendLine("Value=\"\"></Para>");
            strAddXml.AppendLine("</ParaStr>");
            strAddXml.AppendLine("</Function>");
            strAddXml.AppendLine("</System>");
            strAddXml.AppendLine("<MsgOpen>");
            strAddXml.AppendLine("<AssemblyName>SMT.HRM.UI</AssemblyName>");
            strAddXml.AppendLine("<PublicClass>SMT.HRM.UI.Utility</PublicClass>");
            strAddXml.AppendLine("<ProcessName>CreateFormFromEngine</ProcessName>");
            strAddXml.AppendLine("<PageParameter>SMT.HRM.UI.Form.Attendance.AttendanceSolutionAsignForm</PageParameter>");
            strAddXml.AppendLine("<ApplicationOrder>{ATTENDANCESOLUTIONASIGNID}</ApplicationOrder>");
            strAddXml.AppendLine("<FormTypes>Audit</FormTypes>");
            strAddXml.AppendLine("</MsgOpen>");
            strXmlObjectSource = strXmlObjectSource.Replace("<Name>HR</Name>", strAddXml.ToString());
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
            entAttendanceSolutionAsign.CHECKSTATE = state;
            //entAttendanceSolutionAsign.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            bool bl= Save();

            if (bl)
            {
                clientAtt.AuditAttSolAsignAsync(AttendanceSolutionAsignID, state);
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (entAttendanceSolutionAsign != null)
                state = entAttendanceSolutionAsign.CHECKSTATE;
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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_ATTENDANCESOLUTIONASIGN", entAttendanceSolutionAsign.OWNERID,
                    entAttendanceSolutionAsign.OWNERPOSTID, entAttendanceSolutionAsign.OWNERDEPARTMENTID, entAttendanceSolutionAsign.OWNERCOMPANYID);
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
                strSelEmpNames += entEmp.EMPLOYEECNAME + ";";
               
            }

            if (entCompanyIds.Count() > 1)
            {                
                txtErrorMsg.Text = "只能选取一个公司进行分配";
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), "只能选取一个公司进行分配");
                return;
            }

            lkAssignObject.TxtLookUp.Text = strSelEmpNames;

            entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString();
            entAttendanceSolutionAsign.ASSIGNEDOBJECTID = strIds;

          

            SetRecordOwner(entCompanyIds[0]);
        }

        /// <summary>
        /// 根据分配对象，更新考勤方案归属(OwnerPostId, OwnerDepartmentId, OwnerCompanyId)
        /// </summary>
        /// <param name="strCheckCompanyId"></param>
        private void SetRecordOwner(string strCheckCompanyId)
        {
            var q = from p in entUserPosts
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

            entAttendanceSolutionAsign.OWNERPOSTID = entCurrOwnerUserPost.PostID;
            entAttendanceSolutionAsign.OWNERDEPARTMENTID = entCurrOwnerUserPost.DepartmentID;
            entAttendanceSolutionAsign.OWNERCOMPANYID = entCurrOwnerUserPost.CompanyID;
        }

        /// <summary>
        /// 效验提交的表单
        /// </summary>
        /// <param name="entAttendanceSolutionAsign"></param>
        /// <returns></returns>
        private void CheckSubmitForm(out bool flag)
        {
            flag = false;

            string strVacName = string.Empty, strFineType = string.Empty, strIsFactor = string.Empty, strRemark = string.Empty;

            if (lkAttSol.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCESOLUTION"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCESOLUTION")));
                flag = false;
                return;
            }
            else
            {
                flag = true;
                T_HR_ATTENDANCESOLUTION entAttSol = lkAttSol.DataContext as T_HR_ATTENDANCESOLUTION;

                if (string.IsNullOrEmpty(entAttSol.ATTENDANCESOLUTIONID))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ATTENDANCESOLUTION"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ATTENDANCESOLUTION")));
                    flag = false;
                    return;
                }

                entAttendanceSolutionAsign.T_HR_ATTENDANCESOLUTION = entAttSol;
            }

            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTTYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("ASSIGNEDOBJECTTYPE")));
                flag = false;
                return;
            }
            else
            {
                T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
                if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINETYPE"), string.Format(Utility.GetResourceStr("REQUIRED"), Utility.GetResourceStr("FINETYPE")));
                    flag = false;
                    return;
                }

                flag = true;
            }

            if (lkAssignObject.DataContext == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("REQUIRED", "ASSIGNEDOBJECT"));
                flag = false;
                return;
            }
            else
            {
                flag = true;
                if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    OrganizationWS.T_HR_COMPANY entCompany = lkAssignObject.DataContext as OrganizationWS.T_HR_COMPANY;

                    if (entCompany == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entCompany.COMPANYID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                {
                    OrganizationWS.T_HR_DEPARTMENT entDepartment = lkAssignObject.DataContext as OrganizationWS.T_HR_DEPARTMENT;

                    if (entDepartment == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entDepartment.DEPARTMENTID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECTID"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                {
                    OrganizationWS.T_HR_POST entPost = lkAssignObject.DataContext as OrganizationWS.T_HR_POST;

                    if (entPost == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }

                    if (string.IsNullOrEmpty(entPost.POSTID))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
                {
                    string strIds = tbAssignedObjectID.Text;                    

                    if (string.IsNullOrWhiteSpace(strIds))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr("UNAVAILABLEASSIGNEDOBJECT"));
                        flag = false;
                        return;
                    }
                }
            }

            if (!flag)
            {
                return;
            }

            flag = CheckDate();

            if (!flag)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtErrorMsg.Text))
            {
                flag = false;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ASSIGNEDOBJECT"), Utility.GetResourceStr(txtErrorMsg.Text));
                return;
            }

            if (FormType == FormTypes.Edit)
            {
                entAttendanceSolutionAsign.UPDATEDATE = DateTime.Now;
                entAttendanceSolutionAsign.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            }
        }

        /// <summary>
        /// 校验输入的时间
        /// </summary>
        /// <returns></returns>
        private bool CheckDate()
        {
            DateTime dtStart = new DateTime(), dtEnd = new DateTime();
            bool flag = false;

            
            flag = DateTime.TryParse(dpStarDate.Text, out dtStart);

            //判断时间不小于当前时间

            if (DateTime.Parse(dpStarDate.ToString()) == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "时间不能为空");
                
             
                return false;
            }
            //if (DateTime.Parse(dpStarDate.ToString()) < DateTime.Parse(DateTime.Now.ToString()))
            //{
            //    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("AVAILABLEDATE"), Utility.GetResourceStr("DATENOTNULL", "AVAILABLEDATE"));
            //    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), "起始时间要大于当前时间");
            //    dpStarDate.Text = DateTime.Now.ToString();
            //    //dpStarDate.Focus();
                
            //    return false;
            //}
            if (!flag)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("AVAILABLEDATE"), Utility.GetResourceStr("DATENOTNULL", "AVAILABLEDATE"));
                dpStarDate.Text = string.Empty;
                dpStarDate.Focus();
                return false;
            }

            flag = DateTime.TryParse(dpEndDate.Text, out dtEnd);
            if (!flag)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("UNAVAILABLEDATE"), Utility.GetResourceStr("DATENOTNULL", "UNAVAILABLEDATE"));
                dpEndDate.Text = string.Empty;
                dpEndDate.Focus();
                return false;
            }

            if (dtEnd.CompareTo(dtStart) < 0)
            {
                flag = false;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("UNAVAILABLEDATE"), string.Format(Utility.GetResourceStr("DATECOMPARE"), Utility.GetResourceStr("UNAVAILABLEDATE"), Utility.GetResourceStr("AVAILABLEDATE")));
                dpEndDate.Focus();
                return false;
            }

            string strMsg = string.Empty;
            //if (dtStart.Year == DateTime.Now.Year)
            //{
            //    if (dtStart.Month <= DateTime.Now.Month)
            //    {
            //        strMsg = Utility.GetResourceStr("EARLYEFFECTIVEDATE");
            //    }
            //}

            if (!string.IsNullOrEmpty(strMsg))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("AVAILABLEDATE"), strMsg);
                return false;
            }

            return flag;
        }

        /// <summary>
        /// 保存
        /// </summary>
        private bool Save()
        {
            bool flag = false;
            RefreshUI(RefreshedTypes.ShowProgressBar);

            try
            {
                CheckSubmitForm(out flag);

                if (!flag)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }

                if (FormType == FormTypes.New)
                {
                    clientAtt.AddAttendanceSolutionAsignAsync(entAttendanceSolutionAsign);
                }
                else
                {
                    clientAtt.ModifyAttendanceSolutionAsignAsync(entAttendanceSolutionAsign);
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
        /// <summary>
        /// 根据主键索引，获得指定的考勤方案应用以便查看编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_GetAttendanceSolutionAsignByIDCompleted(object sender, GetAttendanceSolutionAsignByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                entAttendanceSolutionAsign = e.Result;

                if (FormType == FormTypes.Resubmit)
                {
                    entAttendanceSolutionAsign.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }

                this.DataContext = entAttendanceSolutionAsign;

                if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
                {
                    clientOrg.GetCompanyByIdAsync(entAttendanceSolutionAsign.ASSIGNEDOBJECTID);
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
                {
                    clientOrg.GetDepartmentByIdAsync(entAttendanceSolutionAsign.ASSIGNEDOBJECTID);
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
                {
                    clientOrg.GetPostByIdAsync(entAttendanceSolutionAsign.ASSIGNEDOBJECTID);
                }
                else if (entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
                {
                    string[] sArray = entAttendanceSolutionAsign.ASSIGNEDOBJECTID.Split(',');
                    ObservableCollection<string> strIds = new ObservableCollection<string>();
                    foreach(string strId in sArray)
                    {
                        strIds.Add(strId);
                    }
                    clientPer.GetEmployeeByIDsAsync(strIds);
                }

                lkAttSol.IsEnabled = false;
                cbxkAssignedObjectType.IsEnabled = false;
                lkAssignObject.IsEnabled = false;
                //if (entAttendanceSolutionAsign.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                //{
                //    //this.IsEnabled = false;
                //}
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
        /// 新增考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AddAttendanceSolutionAsignCompleted(object sender, AddAttendanceSolutionAsignCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SAVESUCCESSED"));

                    if (closeFormFlag)
                    {
                        CloseForm();
                        return;
                    }

                    FormType = FormTypes.Edit;
                    AttendanceSolutionAsignID = entAttendanceSolutionAsign.ATTENDANCESOLUTIONASIGNID;
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

        /// <summary>
        /// 更新考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_ModifyAttendanceSolutionAsignCompleted(object sender, ModifyAttendanceSolutionAsignCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error == null)
            {
                if (e.Result == "{SAVESUCCESSED}")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", Utility.GetResourceStr("CURRENTRECORD", "")));

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

        /// <summary>
        /// 审批考勤方案应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void clientAtt_AuditAttSolAsignCompleted(object sender, AuditAttSolAsignCompletedEventArgs e)
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
                lkAssignObject.TxtLookUp.Text = strNames;
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
        /// 选择考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAttSol_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("ATTENDANCESOLUTIONNAME", "ATTENDANCESOLUTIONNAME");
            cols.Add("ATTENDANCETYPE", "ATTENDANCETYPE,ATTENDANCETYPE,DICTIONARYCONVERTER");
            cols.Add("CARDTYPE", "CARDTYPE,CARDTYPE,DICTIONARYCONVERTER");
            cols.Add("WORKDAYTYPE", "WORKDAYTYPE,WORKDAYTYPE,DICTIONARYCONVERTER");
            cols.Add("WORKMODE", "WORKMODE");
            string filter = "";
            System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
            filter += " CHECKSTATE=@" + paras.Count().ToString() + "";
            paras.Add("2");
            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.AttendanceSolution,
                typeof(T_HR_ATTENDANCESOLUTION[]), cols, filter, paras);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_ATTENDANCESOLUTION ent = lookup.SelectedObj as T_HR_ATTENDANCESOLUTION;

                if (ent != null)
                {
                    lkAttSol.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 选择考勤方案应用对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {            
            if (cbxkAssignedObjectType.SelectedItem == null)
            {
                return;
            }

            T_SYS_DICTIONARY entDic = cbxkAssignedObjectType.SelectedItem as T_SYS_DICTIONARY;
            if (string.IsNullOrEmpty(entDic.DICTIONARYID) || string.IsNullOrEmpty(entDic.DICTIONCATEGORY) || string.IsNullOrEmpty(entDic.DICTIONARYVALUE.ToString()))
            {
                return;
            }

            OrganizationLookup lookup = new OrganizationLookup();
            if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Company;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Department;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Post;
            }
            else if (entDic.DICTIONARYVALUE.ToString() == (Convert.ToInt32(AssignedObjectType.Personnel) + 1).ToString())
            {
                lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
                lookup.MultiSelected = true;
            }

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

                if (lookup.SelectedObjType == OrgTreeItemTypes.Company)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "CNAME";

                        tbAssignedObjectID.Text = ent.COMPANYID;
                        entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Company) + 1).ToString();
                        entAttendanceSolutionAsign.ASSIGNEDOBJECTID = ent.COMPANYID;

                        SetRecordOwner(ent.COMPANYID);
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Department)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                        tbAssignedObjectID.Text = ent.DEPARTMENTID;

                        entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Department) + 1).ToString();
                        entAttendanceSolutionAsign.ASSIGNEDOBJECTID = ent.DEPARTMENTID;

                        SetRecordOwner(ent.T_HR_COMPANY.COMPANYID);
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Post)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                    if (ent != null)
                    {
                        lkAssignObject.DataContext = ent;
                        lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                        tbAssignedObjectID.Text = ent.POSTID;

                        entAttendanceSolutionAsign.ASSIGNEDOBJECTTYPE = (Convert.ToInt32(AssignedObjectType.Post) + 1).ToString();
                        entAttendanceSolutionAsign.ASSIGNEDOBJECTID = ent.POSTID;

                        SetRecordOwner(ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID);
                    }
                }
                else if (lookup.SelectedObjType == OrgTreeItemTypes.Personnel)
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    if (ent == null)
                    {
                        return;
                    }
                    CheckSelectedObj(ents);
                    
                }
                
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }        
        #endregion

    }
}
