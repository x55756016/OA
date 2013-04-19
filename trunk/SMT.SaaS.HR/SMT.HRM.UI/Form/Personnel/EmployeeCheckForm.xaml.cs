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
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeCheckForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }
        private T_HR_EMPLOYEECHECK employeeCheck;
        public string createUserName;
        public T_HR_EMPLOYEECHECK EmployeeCheck
        {
            get { return employeeCheck; }
            set
            {
                employeeCheck = value;
                this.DataContext = value;
            }
        }
        private T_HR_EMPLOYEEPOST employeepost { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private bool canSubmit = false;//能否提交审核
        public bool needsubmit = false;//提交审核,用于判断是否需要调用提交方法
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        PersonnelServiceClient client;
        private string checkid;
        public EmployeeCheckForm(FormTypes type, string strID)
        {
            InitializeComponent();

            FormType = type;
            checkid = strID;
            InitParas(strID);
        }
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public EmployeeCheckForm()
        {
            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            }
            InitializeComponent();

            FormType = FormTypes.New;
            checkid = "";
            InitParas(checkid);
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            client.EmployeeCheckAddCompleted += new EventHandler<EmployeeCheckAddCompletedEventArgs>(client_EmployeeCheckAddCompleted);
            client.EmployeeCheckUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeCheckUpdateCompleted);
            client.GetEmployeeCheckByIDCompleted += new EventHandler<GetEmployeeCheckByIDCompletedEventArgs>(client_GetEmployeeCheckByIDCompleted);
            //weirui 2012/8/24 修改 加公司查询
            //client.GetEmployeeEntryByEmployeeIDCompleted += new EventHandler<GetEmployeeEntryByEmployeeIDCompletedEventArgs>(client_GetEmployeeEntryByEmployeeIDCompleted);
            client.GetEmployeeEntryByEmployeeIDAndCOMPANYIDCompleted += new EventHandler<GetEmployeeEntryByEmployeeIDAndCOMPANYIDCompletedEventArgs>(client_GetEmployeeEntryByEmployeeIDAndCOMPANYIDCompleted); 
            //client.GetEmployeeEntryByEmployeeIDAsync
            client.GetEmployeePostByEmployeeIDCompleted += new EventHandler<GetEmployeePostByEmployeeIDCompletedEventArgs>(client_GetEmployeePostByEmployeeIDCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            client.GetEmpOrgInfoByIDCompleted += new EventHandler<GetEmpOrgInfoByIDCompletedEventArgs>(client_GetEmpOrgInfoByIDCompleted);
            this.Loaded += new RoutedEventHandler(EmployeeCheckForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;

            }
            */
            #endregion
        }

        void client_GetEmpOrgInfoByIDCompleted(object sender, GetEmpOrgInfoByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null || e.Result == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                }
                else
                {
                    SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEVIEW employeeView = e.Result;
                    string strOrgName = employeeView.POSTNAME + " - " + employeeView.DEPARTMENTNAME + " - " + employeeView.COMPANYNAME;
                    if (!string.IsNullOrWhiteSpace(strOrgName))
                    {
                        strOrgName = EmployeeCheck.EMPLOYEENAME + " - " + strOrgName;
                    }

                    //T_HR_EMPLOYEE ent = new T_HR_EMPLOYEE();
                    //ent.EMPLOYEEID = employeeView.EMPLOYEEID;
                    //ent.EMPLOYEECODE = employeeView.EMPLOYEECODE;
                    //ent.OWNERID = employeeView.OWNERID;
                    //ent.OWNERPOSTID = employeeView.OWNERPOSTID;
                    //ent.OWNERDEPARTMENTID = employeeView.OWNERDEPARTMENTID;
                    //ent.OWNERCOMPANYID = employeeView.OWNERCOMPANYID;

                    lkEmployeeName.TxtLookUp.Text = strOrgName;
                    //lkEmployeeName.DataContext = ent;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message + ex.Message));
            }
        }

        void client_GetEmployeeEntryByEmployeeIDAndCOMPANYIDCompleted(object sender, GetEmployeeEntryByEmployeeIDAndCOMPANYIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                var temp = e.Result.FirstOrDefault();

                if (temp != null)
                {
                    txtPtr.Text = temp.PROBATIONPERIOD.GetValueOrDefault().ToString();
                    //入职时间
                    if (temp.ENTRYDATE != null)
                        txtReportDate.Text = temp.ENTRYDATE.Value.ToString("yyyy-MM-dd");
                    //到岗日期
                    if (temp.ONPOSTDATE != null)
                        txtOndutyDate.Text = temp.ONPOSTDATE.Value.ToString("yyyy-MM-dd");
                }
            }
        }

        void EmployeeCheckForm_Loaded(object sender, RoutedEventArgs e)
        {
            //重载提交按钮-提交先保存
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;

            }
            #endregion
            if (FormType == FormTypes.New)
            {
                EmployeeCheck = new T_HR_EMPLOYEECHECK();
                EmployeeCheck.BEREGULARID = Guid.NewGuid().ToString();
                EmployeeCheck.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeCheck.CREATEDATE = DateTime.Now;
                EmployeeCheck.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                EmployeeCheck.BEREGULARDATE = DateTime.Now;
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                lkEmployeeName.IsEnabled = false;
                client.GetEmployeeCheckByIDAsync(checkid);
            }
        }

        //by luojie 
        //工具栏提交按钮的重载方法
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            needsubmit = true;
            isSubmit = true;

            Save();
        }

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


        void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                createUserName = e.Result.EMPLOYEECNAME;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
        }

        void client_GetEmployeePostByEmployeeIDCompleted(object sender, GetEmployeePostByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                employeepost = e.Result;
                //client.GetEmployeeByIDAsync(EmployeeCheck.CREATEUSERID);
                if (EmployeeCheck.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                   || EmployeeCheck.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(EmployeeCheck.CREATEUSERID);
                    client.GetEmployeeToEngineAsync(CreateUserIDs);
                }
                //RefreshUI(RefreshedTypes.AuditInfo);
                //SetToolBar();
            }
        }

        //void client_GetEmployeeEntryByEmployeeIDCompleted(object sender, GetEmployeeEntryByEmployeeIDCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
        //       Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        T_HR_EMPLOYEEENTRY temp = e.Result;
        //        if (temp != null)
        //        {
        //            txtPtr.Text = temp.PROBATIONPERIOD.GetValueOrDefault().ToString();
        //            //入职时间
        //            if (temp.ENTRYDATE != null)
        //                txtReportDate.Text = temp.ENTRYDATE.GetValueOrDefault().ToString("yyyy-MM-dd");
        //            //到岗日期
        //            if (temp.ONPOSTDATE != null)
        //                txtOndutyDate.Text = temp.ONPOSTDATE.GetValueOrDefault().ToString("yyyy-MM-dd");
        //        }
        //    }
        //}

        void client_GetEmployeeCheckByIDCompleted(object sender, GetEmployeeCheckByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }

                EmployeeCheck = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    EmployeeCheck.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                }
                client.GetEmployeePostByEmployeeIDAsync(EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
                client.GetEmpOrgInfoByIDAsync(EmployeeCheck.OWNERID, EmployeeCheck.OWNERPOSTID, EmployeeCheck.OWNERDEPARTMENTID, EmployeeCheck.OWNERCOMPANYID);
                //RefreshUI(RefreshedTypes.AuditInfo);
                //SetToolBar();
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEECHECK", EmployeeCheck.OWNERID,
            //        EmployeeCheck.OWNERPOSTID, EmployeeCheck.OWNERDEPARTMENTID, EmployeeCheck.OWNERCOMPANYID);

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
                //ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEECHECK", EmployeeCheck.OWNERID,
                  //  EmployeeCheck.OWNERPOSTID, EmployeeCheck.OWNERDEPARTMENTID, EmployeeCheck.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEECHECK");
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
                    // Cancel();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
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
        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEECHECK Info)
        {
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ownerCompany = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(s => s.COMPANYID == Info.OWNERCOMPANYID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ownerDepartment = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(s => s.DEPARTMENTID == Info.OWNERDEPARTMENTID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_POST ownerPost = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(s => s.POSTID == Info.OWNERPOSTID).FirstOrDefault();
            string ownerCompanyName = string.Empty;
            string ownerDepartmentName = string.Empty;
            string ownerPostName = string.Empty;
            if (ownerCompany != null)
            {
                ownerCompanyName = ownerCompany.CNAME;
            }
            if (ownerDepartment != null)
            {
                ownerDepartmentName = ownerDepartment.T_HR_DEPARTMENTDICTIONARY == null ? "" : ownerDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }
            if (ownerPost != null)
            {
                ownerPostName = ownerPost.T_HR_POSTDICTIONARY == null ? "" : ownerPost.T_HR_POSTDICTIONARY.POSTNAME;
            }

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;


            string postLevelName = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == employeepost.POSTLEVEL).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "EMPLOYEEID", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "POSTLEVEL", employeepost.POSTLEVEL.ToString(), postLevelName));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "EMPLOYEECNAME", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "OWNER", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "CREATEUSERNAME", createUserName, createUserName));

            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));

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
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
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
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("POSTLEVEL", employeepost.POSTLEVEL.ToString());
            para.Add("CREATEUSERNAME", createUserName);

            if (!string.IsNullOrWhiteSpace(lkEmployeeName.TxtLookUp.Text))
            {
                para.Add("OWNER", lkEmployeeName.TxtLookUp.Text);
            }
            else
            {
                para.Add("OWNER", EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEECNAME); 
            }


            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("T_HR_EMPLOYEEReference", EmployeeCheck.T_HR_EMPLOYEE == null ? "" : EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEEID + "#" + EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEECNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEECHECK>(EmployeeCheck, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeCheck);
            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
            paraIDs.Add("CreatePostID", EmployeeCheck.T_HR_EMPLOYEE.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeCheck.T_HR_EMPLOYEE.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeCheck.T_HR_EMPLOYEE.OWNERCOMPANYID);

            if (EmployeeCheck.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                // Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECHECK", EmployeeCheck.BEREGULARID, strXmlObjectSource, EmployeeCheck.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECHECK", EmployeeCheck.BEREGULARID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECHECK", EmployeeCheck.BEREGULARID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //Utility.UpdateCheckState("T_HR_EMPLOYEECHECK", "BEREGULARID", EmployeeCheck.BEREGULARID, args);
            string state = "";
            string UserState = "Audit";
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
            if (EmployeeCheck.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            EmployeeCheck.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //EmployeeCheck.CHECKSTATE = state;
            //client.EmployeeCheckUpdateAsync(EmployeeCheck, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeCheck != null)
                state = EmployeeCheck.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        private bool Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (lkEmployeeName.DataContext is SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = lkEmployeeName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                T_HR_EMPLOYEE ent = new T_HR_EMPLOYEE();
                ent.EMPLOYEEID = employee.EMPLOYEEID;
                ent.EMPLOYEECNAME = employee.EMPLOYEECNAME;
                ent.OWNERPOSTID = employee.OWNERPOSTID;
                ent.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                ent.OWNERCOMPANYID = employee.OWNERCOMPANYID;

                EmployeeCheck.EMPLOYEECODE = employee.EMPLOYEECODE;
                EmployeeCheck.EMPLOYEENAME = employee.EMPLOYEECNAME;
                EmployeeCheck.OWNERID = employee.EMPLOYEEID;
                EmployeeCheck.OWNERPOSTID = employee.OWNERPOSTID;
                EmployeeCheck.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                EmployeeCheck.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                EmployeeCheck.T_HR_EMPLOYEE = ent;
                //EmployeeCheck.T_HR_EMPLOYEE = employee;
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(dpBeregulardate.Text.Trim()))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "BEREGULARDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "BEREGULARDATE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                //对转正日期作校验
                bool isRightTime = true;
                string strBeregularDate = dpBeregulardate.Text;
                string strReportDate = txtReportDate.Text;
                string strOndutyDate = txtOndutyDate.Text;
                //报道时间
                DateTime beregularDate = Convert.ToDateTime(strBeregularDate);
                if(!string.IsNullOrEmpty(strReportDate))
                {
                    DateTime reportDate = Convert.ToDateTime(strReportDate);
                    if (reportDate != null && reportDate > beregularDate)
                    {
                        isRightTime = false;
                    }
                }
                //到岗时间
                if (!string.IsNullOrEmpty(strOndutyDate))
                {
                    DateTime ondutyDate = Convert.ToDateTime(strOndutyDate);
                    if (ondutyDate != null && ondutyDate > beregularDate)
                    {
                        isRightTime = false;
                    }
                }
                //弹出警告窗
                if (!isRightTime)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "抱歉,转正时间须晚于入职时间和到岗时间.",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
            }
            
            if (FormType == FormTypes.New)
            {
                EmployeeCheck.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                EmployeeCheck.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                EmployeeCheck.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                EmployeeCheck.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                string strMsg = "";
                client.EmployeeCheckAddAsync(EmployeeCheck, strMsg);

            }
            else
            {
                EmployeeCheck.UPDATEDATE = System.DateTime.Now;
                EmployeeCheck.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; ;

                client.EmployeeCheckUpdateAsync(EmployeeCheck, "Edit");
            }

            return true;
        }

        void client_EmployeeCheckAddCompleted(object sender, EmployeeCheckAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
        }


        void client_EmployeeCheckUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() == "Edit" && !isSubmit)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    isSubmit = false;
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "EMPLOYEECHECK"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "EMPLOYEECHECK"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                ///by luojie 转至提交按钮原来的方法
                if (needsubmit)
                {
                    try
                    {
                        
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();
                    }
                    catch
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                    }

                }
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                RefreshUI(RefreshedTypes.All);
            }
        }

        /// <summary>
        ///     回到提交前的状态 by luojie
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            isSubmit = false;

            //隐藏工具栏 不允许二次提交
            #region 隐藏entitybrowser中的toolbar按钮
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }
            #endregion
            //if (refreshType == RefreshedTypes.CloseAndReloadData)
            //{
            //    //refreshType = RefreshedTypes.AuditInfo;
            //    refreshType = RefreshedTypes.HideProgressBar;
            //}

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

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            //cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            //cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            //LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
            //    typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            //lookup.SelectedClick += (o, ev) =>
            //{
            //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST;;

            //    if (ent != null)
            //    {
            //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
            //        client.GetEmployeeEntryByEmployeeIDAsync(ent.T_HR_EMPLOYEE.EMPLOYEEID);
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门


                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司

                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE emp = userInfo.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                    T_HR_EMPLOYEEPOST empPost = emp.T_HR_EMPLOYEEPOST.Where(t => t.T_HR_POST.POSTID == postid).FirstOrDefault();
                    if (empPost == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "检测员工当前岗位异常，请重试",
                                                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }

                    if (empPost.ISAGENCY == "1")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "员工兼职岗位不能转正",
                                                                         Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }

                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    lkEmployeeName.TxtLookUp.Text = StrEmployee;
                    lkEmployeeName.DataContext = emp;
                    ToolTipService.SetToolTip(lkEmployeeName.TxtLookUp, StrEmployee);

                    if (ent != null)
                    {
                        //client.GetEmployeeEntryByEmployeeIDAsync(ent.EMPLOYEEID);
                        client.GetEmployeeEntryByEmployeeIDAndCOMPANYIDAsync(emp.EMPLOYEEID, emp.OWNERCOMPANYID);
                        client.GetEmployeePostByEmployeeIDAsync(emp.EMPLOYEEID);
                    }
                }


                //SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                //if (ent != null)
                //{
                //    lkEmployeeName.DataContext = ent;
                //    //client.GetEmployeeEntryByEmployeeIDAsync(ent.EMPLOYEEID);
                //    client.GetEmployeeEntryByEmployeeIDAndCOMPANYIDAsync(ent.EMPLOYEEID, ent.OWNERCOMPANYID);
                //    client.GetEmployeePostByEmployeeIDAsync(ent.EMPLOYEEID);
                //}
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}
