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

using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeInsuranceForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        private T_HR_EMPLOYEEINSURANCE employInsurance;
        public string createUserName;
        public T_HR_EMPLOYEEINSURANCE EmployeeInsurance
        {
            get { return employInsurance; }
            set
            {
                employInsurance = value;
                this.DataContext = value;
            }
        }
        private string employInsuranceID;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        PersonnelServiceClient client;
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭

        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public EmployeeInsuranceForm()
        {
            InitializeComponent();

            FormType =  FormTypes.New;
            this.employInsuranceID = "";
            InitParas(employInsuranceID);
        }

        public EmployeeInsuranceForm(FormTypes type, string employInsuranceID)
        {
            InitializeComponent();

            FormType = type;
            this.employInsuranceID = employInsuranceID;
            InitParas(employInsuranceID);
        }

        private void InitParas(string employInsuranceID)
        {
            client = new PersonnelServiceClient();
            client.GetEmployeeInsuranceByIDCompleted += new EventHandler<GetEmployeeInsuranceByIDCompletedEventArgs>(client_GetEmployeeInsuranceByIDCompleted);
            client.EmployeeInsuranceAddCompleted += new EventHandler<EmployeeInsuranceAddCompletedEventArgs>(client_EmployeeInsuranceAddCompleted);
            client.EmployeeInsuranceUpdateCompleted += new EventHandler<EmployeeInsuranceUpdateCompletedEventArgs>(client_EmployeeInsuranceUpdateCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(EmployeeInsuranceForm_Loaded);
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }

        }

        void EmployeeInsuranceForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (FormType == FormTypes.New)
            {
                EmployeeInsurance = new T_HR_EMPLOYEEINSURANCE();
                EmployeeInsurance.EMPLOYINSURANCEID = Guid.NewGuid().ToString();
                EmployeeInsurance.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                EmployeeInsurance.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                lkEmployeeName.IsEnabled = false;
                client.GetEmployeeInsuranceByIDAsync(employInsuranceID);
            }
        }










        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.OWNERID,
            //        EmployeeInsurance.OWNERPOSTID, EmployeeInsurance.OWNERDEPARTMENTID, EmployeeInsurance.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.OWNERID,
                    EmployeeInsurance.OWNERPOSTID, EmployeeInsurance.OWNERDEPARTMENTID, EmployeeInsurance.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEEINSURANCE");
        }
        public string GetStatus()
        {
            return EmployeeInsurance != null ? EmployeeInsurance.CHECKSTATE : "";
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
                    //  Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
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

        #region IClient
        public void ClosedWCFClient()
        {
            //throw new NotImplementedException();
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
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Post.OWNERID)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            // Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.EMPLOYINSURANCEID);
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EntityKey", EmployeeInsurance.EMPLOYINSURANCEID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();
            para2.Add("INSURANCECATEGORY", (cbInsuranceType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbInsuranceType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            para2.Add("T_HR_EMPLOYEEReference", EmployeeInsurance.T_HR_EMPLOYEE == null ? "" : EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEEID + "#" + EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEECNAME);

            string strXmlObjectSource = string.Empty;
            string strKeyName = "EMPLOYINSURANCEID";
            string strKeyValue = EmployeeInsurance.EMPLOYINSURANCEID;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEINSURANCE>(EmployeeInsurance, para, "HR", para2, strKeyName, strKeyValue);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEEID);
            paraIDs.Add("CreatePostID", EmployeeInsurance.T_HR_EMPLOYEE.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeInsurance.T_HR_EMPLOYEE.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeInsurance.T_HR_EMPLOYEE.OWNERCOMPANYID);

            if (EmployeeInsurance.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.EMPLOYINSURANCEID, strXmlObjectSource, EmployeeInsurance.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.EMPLOYINSURANCEID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEINSURANCE", EmployeeInsurance.EMPLOYINSURANCEID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //  Utility.UpdateCheckState("T_HR_EMPLOYEEINSURANCE", "EMPLOYINSURANCEID", EmployeeInsurance.EMPLOYINSURANCEID, args);
            string state = "";
            string UserState = "Audit";
            string strMsg = "";
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
            if (EmployeeInsurance.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            EmployeeInsurance.CHECKSTATE = state;
            client.EmployeeInsuranceUpdateAsync(EmployeeInsurance, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeInsurance != null)
                state = EmployeeInsurance.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region 保存
        private bool Save()
        {
            //  List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            string strMsg = "";
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (string.IsNullOrEmpty(lkEmployeeName.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            //if (validators.Count > 0)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIREDFIELDS"));
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (cbInsuranceType.SelectedIndex < 0)
            {

                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "INSURANCECATEGORY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "INSURANCECATEGORY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;


            }
            if (!string.IsNullOrEmpty(dpStartDate.Text) && !string.IsNullOrEmpty(dpLastDate.Text))
            {
                DateTime startDate = Convert.ToDateTime(dpStartDate.Text);
                DateTime lastDate = Convert.ToDateTime(dpLastDate.Text);
                if (startDate > lastDate)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "STARTDATE,LASTDATE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("DATEERROR", "STARTDATE,LASTDATE"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }

            }
            if (lkEmployeeName.DataContext is SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = lkEmployeeName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                temp.EMPLOYEEID = employee.EMPLOYEEID;
                temp.EMPLOYEECNAME = employee.EMPLOYEECNAME;
                temp.EMPLOYEECODE = employee.EMPLOYEECODE;
                temp.EMPLOYEEENAME = employee.EMPLOYEEENAME;
                temp.OWNERPOSTID = employee.OWNERPOSTID;
                temp.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                temp.OWNERCOMPANYID = employee.OWNERCOMPANYID;

                EmployeeInsurance.OWNERID = employee.EMPLOYEEID;
                EmployeeInsurance.OWNERPOSTID = employee.OWNERPOSTID;
                EmployeeInsurance.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                EmployeeInsurance.OWNERCOMPANYID = employee.OWNERCOMPANYID;

                EmployeeInsurance.T_HR_EMPLOYEE = temp;
            }
            if (FormType == FormTypes.New)
            {
                //所属
                EmployeeInsurance.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                EmployeeInsurance.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                EmployeeInsurance.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                EmployeeInsurance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeInsurance.CREATEDATE = DateTime.Now;


                client.EmployeeInsuranceAddAsync(EmployeeInsurance, strMsg);
            }
            else
            {
                EmployeeInsurance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeInsurance.UPDATEDATE = DateTime.Now;
                client.EmployeeInsuranceUpdateAsync(EmployeeInsurance, strMsg, "Edit");
            }

            return true;
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

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion
        #region 完成事件
        /// <summary>
        ///  新增保险
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeInsuranceAddCompleted(object sender, EmployeeInsuranceAddCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 修改保险
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeInsuranceUpdateCompleted(object sender, EmployeeInsuranceUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (e.UserState.ToString() == "Edit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "EMPLOYEEINSURANCE"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "EMPLOYEEINSURANCE"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 根据id获取保险
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeInsuranceByIDCompleted(object sender, GetEmployeeInsuranceByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (e.Result != null)
                {
                    EmployeeInsurance = e.Result;
                    if (EmployeeInsurance.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                   || EmployeeInsurance.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                        SetToolBar();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                    else
                    {
                        System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                        CreateUserIDs.Add(EmployeeInsurance.CREATEUSERID);
                        client.GetEmployeeToEngineAsync(CreateUserIDs);
                    }
                }
            }
        }
        /// <summary>
        /// 获取创建人信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeToEngineCompleted(object sender, GetEmployeeToEngineCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    createUserName = e.Result[0].EMPLOYEENAME;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        #endregion
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            #region
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            //cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            //cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            //LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
            //    typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            //lookup.SelectedClick += (o, ev) =>
            //{
            //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

            //    if (ent != null)
            //    {
            //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            #endregion
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}

