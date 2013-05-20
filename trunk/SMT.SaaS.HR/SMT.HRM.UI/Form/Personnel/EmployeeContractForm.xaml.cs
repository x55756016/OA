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
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.IO;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeContractForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public OpenFileDialog openFileDialog = null;
        public string createUserName;
        public FormTypes FormType { get; set; }
        private T_HR_EMPLOYEECONTRACT employeeContract;
        private string contractID;
        private bool employeeContractIsSet;
        public bool needsubmit = false;//提交审核,
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        public T_HR_EMPLOYEECONTRACT EmployeeContract
        {
            get { return employeeContract; }
            set
            {
                employeeContract = value;
                this.DataContext = value;
            }
        }

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        PersonnelServiceClient client;
        private bool canSubmit = false;//能否提交审核
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public EmployeeContractForm()
        {
            InitializeComponent();
            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            }
            FormType = FormTypes.New;
            contractID = "";
            this.Loaded += (sender, args) =>
            {
                //InitParas(strID);
            };
            InitParas(contractID);
        }

        public EmployeeContractForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            contractID = strID;
            this.Loaded += (sender, args) =>
            {
                //InitParas(strID);
                //by luojie 重载提交
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
                entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            };
            InitParas(strID);
            
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            client.GetEmployeeContractByIDCompleted += new EventHandler<GetEmployeeContractByIDCompletedEventArgs>(client_GetEmployeeContractByIDCompleted);
            client.EmployeeContractAddCompleted += new EventHandler<EmployeeContractAddCompletedEventArgs>(client_EmployeeContractAddCompleted);
            client.EmployeeContractUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeContractUpdateCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            //ctrFile.SystemName = "HR";
            //ctrFile.ModelName = "EMPLOYEECONTRACT";
            this.Loaded += new RoutedEventHandler(EmployeeContractForm_Loaded);
            //ctrFile.InitBtn(Visibility.Visible, Visibility.Collapsed);

            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                EnabledControl();

            }
            
        }

        void EmployeeContractForm_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (FormType == FormTypes.New)
            {
                falseRabtn.IsChecked = true;
                EmployeeContract = new T_HR_EMPLOYEECONTRACT();
                employeeContractIsSet = false;
                EmployeeContract.EMPLOYEECONTACTID = Guid.NewGuid().ToString();
                EmployeeContract.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeContract.CREATEDATE = DateTime.Now;
                employeeContract.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                //ctrFile.Load_fileData(EmployeeContract.EMPLOYEECONTACTID);
                SetToolBar();
                Utility.InitFileLoad("EmployeeContract", EmployeeContract.EMPLOYEECONTACTID, FormType, uploadFile);
            }
            else
            {
                if (falseRabtn.IsChecked == true)
                {
                    nudDay.IsEnabled = true;
                }
               // nudDay.Value = 0;
                //nudDay.IsEnabled = false;
                employeeContractIsSet = false;
                lkEmployeeName.IsEnabled = false;
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetEmployeeContractByIDAsync(contractID);
                Utility.InitFileLoad("EmployeeContract", contractID, FormType, uploadFile);
            }
            //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), FormType.ToString(),
            //   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        }


        private void EnabledControl()
        {
            falseRabtn.IsEnabled = false;
            trueRabtn.IsEnabled = false;
            txtCode.IsReadOnly = true;
            txtPeason.IsReadOnly = true;
            lkEmployeeName.IsEnabled = false;
            dpEndDate.IsEnabled = false;
            dpFromDate.IsEnabled = false;
            txtEndDate.IsReadOnly = true;
            txtRemark.IsReadOnly = true;
            nudDay.IsEnabled = false;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
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


        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

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
        void client_GetEmployeeContractByIDCompleted(object sender, GetEmployeeContractByIDCompletedEventArgs e)
        {
          
            
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                EmployeeContract = e.Result;
                if (e.Result.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnabledControl();
                }
                //ctrFile.Load_fileData(EmployeeContract.EMPLOYEECONTACTID);
                //if(employeeContract.CHECKSTATE )
                if (!string.IsNullOrEmpty(EmployeeContract.CREATEUSERID))
                {
                    if (EmployeeContract.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || EmployeeContract.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                        SetToolBar();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }
                    else
                    {
                        System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                        CreateUserIDs.Add(EmployeeContract.CREATEUSERID);
                        client.GetEmployeeToEngineAsync(CreateUserIDs);
                    }
                }
                else
                {
                    EmployeeContract.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }

            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEECHECK", EmployeeContract.OWNERID,
            //        EmployeeContract.OWNERPOSTID, EmployeeContract.OWNERDEPARTMENTID, EmployeeContract.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEECHECK", EmployeeContract.OWNERID,
                    EmployeeContract.OWNERPOSTID, EmployeeContract.OWNERDEPARTMENTID, EmployeeContract.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEECONTRACT");
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
        private string GetXmlString(string StrSource, T_HR_EMPLOYEECONTRACT Info)
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


            //string postLevelName = string.Empty;
            //SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVLE" && s.DICTIONARYVALUE == employeepost.POSTLEVEL).FirstOrDefault();
            //postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "CHECKSTATE", "1", checkState));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "EMPLOYEEID", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            //  AutoList.Add(basedata("T_HR_EMPLOYEECHECK", "POSTLEVEL", employeepost.POSTLEVEL.ToString(), employeepost.POSTLEVEL.ToString()));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "EMPLOYEECNAME", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "OWNER", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "CREATEUSERNAME", createUserName, createUserName));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "AttachMent", Info.EMPLOYEECONTACTID, Info.EMPLOYEECONTACTID));

            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEECONTRACT", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
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
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != EmployeeContract.OWNERID)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            //if ((FormType == FormTypes.Resubmit || FormType == FormTypes.Edit) && canSubmit == false)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("请先保存修改的记录"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //  Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECONTRACT", EmployeeContract.EMPLOYEECONTACTID);
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", EmployeeContract.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", EmployeeContract.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", EmployeeContract.T_HR_EMPLOYEE.EMPLOYEECNAME);
            if (EmployeeContract.NOENDDATE == "1")
            {
                nudDay.IsEnabled = false;
                trueRabtn.IsChecked = true;
                txtbk.Opacity = 0;
                dpEndDate.Opacity = 0;
                employeeContractIsSet = false;
            }
            else
            {
                falseRabtn.IsChecked = true;
            }
         
            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("T_HR_EMPLOYEEReference", EmployeeContract.T_HR_EMPLOYEE == null ? "" : EmployeeContract.T_HR_EMPLOYEE.EMPLOYEEID + "#" + EmployeeContract.T_HR_EMPLOYEE.EMPLOYEECNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            // strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEECONTRACT>(EmployeeContract, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeContract);


            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeContract.T_HR_EMPLOYEE.EMPLOYEEID);
            paraIDs.Add("CreatePostID", EmployeeContract.T_HR_EMPLOYEE.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeContract.T_HR_EMPLOYEE.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeContract.T_HR_EMPLOYEE.OWNERCOMPANYID);

            if (EmployeeContract.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECONTRACT", EmployeeContract.EMPLOYEECONTACTID, strXmlObjectSource, EmployeeContract.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECONTRACT", EmployeeContract.EMPLOYEECONTACTID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEECONTRACT", EmployeeContract.EMPLOYEECONTACTID, strXmlObjectSource);
               
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.InitFileLoad(FormTypes.Audit, uploadFile, EmployeeContract.EMPLOYEECONTACTID,false);
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    //向引擎发数据
                    client.GetEmployeeContractEngineXmlAsync(EmployeeContract);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (EmployeeContract.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            EmployeeContract.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            // client.EmployeeContractUpdateAsync(EmployeeContract, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeContract != null)
                state = EmployeeContract.CHECKSTATE;
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
            string strMsg = "";
            //  List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (!CheckData())
            {
                needsubmit = false;
                isSubmit = false;
                return false;
            }

            if (lkEmployeeName.DataContext is SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = lkEmployeeName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                T_HR_EMPLOYEE ent = new T_HR_EMPLOYEE();
                ent.EMPLOYEEID = employee.EMPLOYEEID;
                ent.EMPLOYEECNAME = employee.EMPLOYEECNAME;
                ent.OWNERPOSTID = employee.OWNERPOSTID;
                ent.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                ent.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                
                EmployeeContract.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                EmployeeContract.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                EmployeeContract.OWNERPOSTID = employee.OWNERPOSTID;
                EmployeeContract.OWNERID = employee.OWNERID;
                EmployeeContract.T_HR_EMPLOYEE = ent;
              
            }


            //上传的文件
            //ctrFile.FormID = EmployeeContract.EMPLOYEECONTACTID;
            //ctrFile.Save();
            //日期转换
            if (falseRabtn.IsChecked == true)
            {
                EmployeeContract.TODATE = Convert.ToDateTime(dpEndDate.Text).ToShortDateString();
                EmployeeContract.NOENDDATE = "0";
                
            }
            else
            {
                EmployeeContract.NOENDDATE = "1";
                dpEndDate.Text = "";
                nudDay.Value = 0;
                nudDay.IsEnabled = false;
                employeeContractIsSet = false;
            }
               
            if (FormType == FormTypes.New)
            {
                //所属
                EmployeeContract.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                EmployeeContract.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                EmployeeContract.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                //EmployeeContract.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                client.EmployeeContractAddAsync(EmployeeContract, strMsg);
            }
            else
            {
                EmployeeContract.UPDATEDATE = System.DateTime.Now;
                EmployeeContract.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; ;
                //client.GetEmployeeContractEngineXmlAsync(EmployeeContract);
                client.EmployeeContractUpdateAsync(EmployeeContract, "Edit");
             
              
            }
            return true;
        }

        private bool CheckData()
        {
            if (string.IsNullOrEmpty(lkEmployeeName.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(dpFromDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "FROMDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "FROMDATE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            //employeeContractIsSet值为false是为永久合同，则没有到期日期
            if (falseRabtn.IsChecked == true && string.IsNullOrEmpty(dpEndDate.Text))
            {

                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATE"));
                ComfirmWindow.ConfirmationBoxs(
                    Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("STRINGNOTNULL", "ENDDATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            //employeeContractIsSet值为false是为永久合同，则没有到期日期，不用判断日期先后
            if (falseRabtn.IsChecked == true && Convert.ToDateTime(dpFromDate.Text) > Convert.ToDateTime(dpEndDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "FROMDATE,ENDDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("DATEERROR", "FROMDATE,ENDDATE"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            // if (ctrFile._files.Count <= 0)
            // {
            //     ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ATTACHMENT"),
            //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //     RefreshUI(RefreshedTypes.HideProgressBar);
            //     return false;
            // }
            if (!uploadFile.HasAccessory)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ATTACHMENT"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            return true;
        }

        void client_EmployeeContractAddCompleted(object sender, EmployeeContractAddCompletedEventArgs e)
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
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                if (!isSubmit)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    isSubmit = false;
                }
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
                canSubmit = true;
                if (needsubmit)
                {
                    try
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();
                    }
                    catch (Exception )
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                    }

                }
                RefreshUI(RefreshedTypes.All);
            }
        }

        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            //isSubmit = false;

            //隐藏工具栏 不允许二次提交
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            #region 隐藏entitybrowser中的toolbar按钮
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
            RefreshUI(RefreshedTypes.HideProgressBar);

        }

        void client_EmployeeContractUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                    isSubmit = false;
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "EMPLOYEECONTRACT"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "EMPLOYEECONTRACT"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                if (needsubmit)
                {
                    try
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();
                    }
                    catch (Exception )
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
                RefreshUI(RefreshedTypes.All);
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
        #endregion

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
            //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

            //    if (ent != null)
            //    {
            //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
            //        client.GetEmployeeEntryByEmployeeIDAsync(ent.T_HR_EMPLOYEE.EMPLOYEEID);
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                
                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent;
                    client.GetEmployeeEntryByEmployeeIDAsync(ent.EMPLOYEEID);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        #region 上传文件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();

            openFileDialog.Multiselect = false;
            //OpenFileDialog.Filter = "Excel Files (*.xls)|*.xls;";

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            // UpFile.Text = openFileDialog.File.Name;
            //txtUploadResMsg.Text = string.Empty;
        }
        /// <summary>
        ///  读取社保卡的Excel文件，并导入数据库，返回导入后的结果
        /// </summary>
        private UploadFileModel ImportClockInRd()
        {
            string strMsg = string.Empty;
            Stream stream = null;
            try
            {
                if (openFileDialog == null)
                    return null;

                if (openFileDialog.File == null)
                    return null;

                RefreshUI(RefreshedTypes.ProgressBar);

                stream = (System.IO.Stream)openFileDialog.File.OpenRead();

                byte[] Buffer = new byte[stream.Length];
                stream.Read(Buffer, 0, (int)stream.Length);

                stream.Dispose();
                stream.Close();

                UploadFileModel UploadFile = new UploadFileModel();
                UploadFile.FileName = openFileDialog.File.Name;
                UploadFile.File = Buffer;

                strMsg = string.Empty;
                return UploadFile;
                //clientAtt.ImportClockInRdListFromExcelAsync(UploadFile, dtStart, dtEnd, strMsg);
            }
            catch (Exception )
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return null;
            }
            finally
            {
                stream.Dispose();
                stream.Close();
            }
        }
        #endregion

        //private void Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        //{

            
        //}

        //RadioButton判断是否为永久合同
        //RadioButton值为“否”是触发事件，employeeContractIsSet返回false
        private void  false_Click(object sender, RoutedEventArgs e)
        {
            txtbk.Opacity = 1;
            dpEndDate.Opacity = 1;
            employeeContractIsSet = false;
            if (falseRabtn.IsChecked == true)
            {
                trueRabtn.IsChecked = false;
                falseRabtn.IsChecked = true;
            }
            else
            {
                trueRabtn.IsChecked = true;
                falseRabtn.IsChecked = false;
            }
            dpEndDate.Text = DateTime.Now.ToString();
            nudDay.IsEnabled = true;
        }
        //RadioButton值为“是”是触发事件，employeeContractIsSet返回true
        private void  true_Click(object sender, RoutedEventArgs e)
        {
            txtbk.Opacity = 0;
            dpEndDate.Opacity = 0;
            employeeContractIsSet = true;
            dpEndDate.Text = "";
            nudDay.Value = 0;
            nudDay.IsEnabled = false;
            if (trueRabtn.IsChecked == true)
            {
                falseRabtn.IsChecked = false;
                trueRabtn.IsChecked = true;

            }
            else
            {
                falseRabtn.IsChecked = true;
                trueRabtn.IsChecked = false;
            }
        }


    }
}
