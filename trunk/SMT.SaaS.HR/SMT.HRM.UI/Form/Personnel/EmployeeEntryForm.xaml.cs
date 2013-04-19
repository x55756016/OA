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
using System.Text.RegularExpressions;
using SMT.SAAS.ClientUtility;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeeEntryForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }
        public T_HR_EMPLOYEEPOST EmployeePost { get; set; }
        public string createUserName;
        private T_HR_EMPLOYEEENTRY employeeEntry;

        public T_HR_EMPLOYEEENTRY EmployeeEntry
        {
            get { return employeeEntry; }
            set
            {
                employeeEntry = value;
                this.DataContext = value;
            }
        }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public T_SYS_USER SysUser { set; get; }
        private string EntryId = string.Empty;
        public bool needsubmit = false;//提交审核,用于判断是否需要调用提交方法
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private string OldPassword = string.Empty;
        PersonnelServiceClient client;
        PermissionServiceClient perclient;
        SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orclient;


        public EmployeeEntryForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            EntryId = strID;
            this.Loaded += new RoutedEventHandler(EmployeeEntryForm_Loaded);
            //this.Loaded += (sender, args) =>
            //{
            //    InitParas(strID);
            //};
        }

        void EmployeeEntryForm_Loaded(object sender, RoutedEventArgs e)
        {
            //重载提交按钮-提交先保存
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            
            // 加载字典
            DictionaryManager dicManager = new DictionaryManager();
            List<string> dicCategorys = new List<string>();
            dicCategorys.Add("POSTLEVEL");
            
            dicManager.OnDictionaryLoadCompleted += (o, aregs) =>
            {
                if (aregs.Error == null && aregs.Result)
                {
                    InitParas(EntryId);
                }
                else
                {
                    //SMT.RM.UI.Class.Utility.ShowCustomMessage(Class.MessageTypes.Error,
                    //SMT.RM.UI.Class.Utility.GetResourceStr("ERROR"),
                    //SMT.RM.UI.Class.Utility.GetResourceStr("加载系统字典出错，请管理员检查系统字典服务！"));
                }
            };
            dicManager.LoadDictionary(dicCategorys);
           
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            client.GetEmployeeEntryByIDCompleted += new EventHandler<GetEmployeeEntryByIDCompletedEventArgs>(client_GetEmployeeEntryByIDCompleted);
            client.EmployeeEntryAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeEntryAddCompleted);
            client.EmployeeEntryUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeEntryUpdateCompleted);
            // client.GetEmployeePostByEmployeeIDCompleted += new EventHandler<GetEmployeePostByEmployeeIDCompletedEventArgs>(client_GetEmployeePostByEmployeeIDCompleted);
            //client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(client_GetEmployeeByIDCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            client.GetEmployeePostByEmployeePostIDCompleted += new EventHandler<GetEmployeePostByEmployeePostIDCompletedEventArgs>(client_GetEmployeePostByEmployeePostIDCompleted);
            perclient = new PermissionServiceClient();
            perclient.SysUserInfoAddCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.SysUserInfoAddCompletedEventArgs>(perclient_SysUserInfoAddCompleted);
            perclient.SysUserInfoUpdateCompleted += new EventHandler<SysUserInfoUpdateCompletedEventArgs>(perclient_SysUserInfoUpdateCompleted);
            perclient.GetUserByEmployeeIDCompleted += new EventHandler<GetUserByEmployeeIDCompletedEventArgs>(perclient_GetUserByEmployeeIDCompleted);

            orclient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orclient.GetPostNumberCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs>(orclient_GetPostNumberCompleted);
            if (FormType == FormTypes.Browse)//|| FormType == FormTypes.Audit
            {
                //  this.IsEnabled=false;
                EnablControl();
            }
            if (FormType == FormTypes.New)
            {
                //员工岗位
                EmployeePost = new T_HR_EMPLOYEEPOST();
                EmployeePost.EMPLOYEEPOSTID = Guid.NewGuid().ToString();
                EmployeePost.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeePost.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                EmployeePost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                //EmployeePost.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();  //默认审核成功
                //EmployeePost.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                EmployeePost.CREATEDATE = DateTime.Now;

                //员工入职
                EmployeeEntry = new T_HR_EMPLOYEEENTRY();
                EmployeeEntry.EMPLOYEEENTRYID = Guid.NewGuid().ToString();
                EmployeeEntry.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeEntry.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                EmployeeEntry.CREATEDATE = DateTime.Now;

                //系统用户
                SysUser = new T_SYS_USER();
                SysUser.SYSUSERID = Guid.NewGuid().ToString();
                SysUser.STATE = "0";
                SysUser.CREATEUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SysUser.CREATEDATE = DateTime.Now;

                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetEmployeeEntryByIDAsync(strID);
            }
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

        }
        //void client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //         ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
        //            return;
        //        }
        //        createUserName = e.Result.EMPLOYEECNAME;
        //        RefreshUI(RefreshedTypes.AuditInfo);
        //        SetToolBar();

        //    }
        //}

        void orclient_GetPostNumberCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                lkPost.DataContext = null;
                lkPost.TxtLookUp.Text = string.Empty;
                //txtCompanyName.Text = string.Empty;
                //txtDepartment.Text = string.Empty;
                cbxPostLevel.SelectedItem = null;
            }
            else
            {
                if (e.Result <= 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("POSTNUMBERFULL", "POST"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("POSTNUMBERFULL", "POST"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    lkPost.DataContext = null;
                    lkPost.TxtLookUp.Text = string.Empty;
                    //txtCompanyName.Text = string.Empty;
                    //txtDepartment.Text = string.Empty;
                    cbxPostLevel.SelectedItem = null;
                }

            }
        }

        void client_GetEmployeeEntryByIDCompleted(object sender, GetEmployeeEntryByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                EmployeeEntry = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    EmployeeEntry.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }
                if (EmployeeEntry.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnablControl();
                }
                if (EmployeeEntry.REMARK != null)
                {
                    int index = EmployeeEntry.REMARK.IndexOf("Ё");
                    if (index > 0)
                    {
                        if (EmployeeEntry.EDITSTATE == "0")
                        {
                            tbRemark.Text = Utility.GetResourceStr(EmployeeEntry.REMARK.Substring(0, index));
                        }
                        txtRemarks.Text = EmployeeEntry.REMARK.Substring(index + 1);
                    }
                    else
                    {
                        txtRemarks.Text = EmployeeEntry.REMARK;
                    }
                }
                //client.GetEmployeePostByEmployeeIDAsync(EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID);
                client.GetEmployeePostByEmployeePostIDAsync(employeeEntry.EMPLOYEEPOSTID);
                perclient.GetUserByEmployeeIDAsync(EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID);
                // RefreshUI(RefreshedTypes.AuditInfo);
                //SetToolBar();
            }
            RefreshUI(RefreshedTypes.HideProgressBar);

        }

        void perclient_GetUserByEmployeeIDCompleted(object sender, GetUserByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                SysUser = new T_SYS_USER();
                SysUser = e.Result;
                if (SysUser != null)
                {
                    txtUser.Text = SysUser.USERNAME;
                    txtPwd.Password = SysUser.PASSWORD;
                    OldPassword = SysUser.PASSWORD;
                    txtMark.Text = SysUser.REMARK == null ? string.Empty : SysUser.REMARK;
                }
            }
        }


        void client_GetEmployeePostByEmployeePostIDCompleted(object sender, GetEmployeePostByEmployeePostIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                EmployeePost = e.Result;

                #region 2011-08-17 modified by Sam:解决岗位级别有时能加载有时不能加载的问题
                cbxPostLevel.SelectedValue = EmployeePost.POSTLEVEL.ToString();
                #endregion

                if (FormType == FormTypes.Browse)
                {
                    EnablControl();
                }
                else
                {
                    if (EmployeePost.CHECKSTATE != "0")
                    {
                        EnablControl();
                    }
                    else
                    {
                        this.txtUser.IsEnabled = true;
                        this.txtPwd.IsEnabled = true;
                        FormType = FormTypes.Edit;
                    }
                }

                //RefreshUI(RefreshedTypes.AuditInfo);
                //SetToolBar();
                //client.GetEmployeeByIDAsync(EmployeeEntry.CREATEUSERID);
                if (EmployeeEntry.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || EmployeeEntry.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(EmployeeEntry.CREATEUSERID);
                    client.GetEmployeeToEngineAsync(CreateUserIDs);
                }
                //岗位赋值
                lkPost.DataContext = EmployeePost.T_HR_POST;
                lkPost.TxtLookUp.Text = EmployeePost.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME + GetFullOrgName(EmployeePost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID);
                //txtCompanyName.Text = EmployeePost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                //txtDepartment.Text = EmployeePost.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

                #region 2011-08-17 modified by Sam:解决岗位级别有时能加载有时不能加载的问题
                //foreach (T_SYS_DICTIONARY item in cbxPostLevel.Items)
                //{
                //    //if (item.DICTIONARYVALUE == EmployeePost.T_HR_POST.POSTLEVEL)
                //    if (item.DICTIONARYVALUE == EmployeePost.POSTLEVEL)
                //    {
                //        cbxPostLevel.SelectedItem = item;
                //        break;
                //    }
                //}
                #endregion
            }
        }


        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEEENTRY", EmployeeEntry.OWNERID,
            //        EmployeeEntry.OWNERPOSTID, EmployeeEntry.OWNERDEPARTMENTID, EmployeeEntry.OWNERCOMPANYID);

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
                //重新提交只显示审核
                //ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEENTRY", EmployeeEntry.OWNERID,
                //    EmployeeEntry.OWNERPOSTID, EmployeeEntry.OWNERDEPARTMENTID, EmployeeEntry.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEEENTRY");
        }
        public string GetStatus()
        {
            return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
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
            perclient.DoClose();
            orclient.DoClose();
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
        private string GetXmlString(string StrSource, T_HR_EMPLOYEEENTRY Info)
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


            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (cbxPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY);

            List<T_SYS_USER> sysuserList = new List<T_SYS_USER>();
            sysuserList.Add(SysUser);
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "EMPLOYEEID", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "POSTLEVEL", EmployeePost.POSTLEVEL.ToString(), postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "EMPLOYEECNAME", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "OWNER", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "CREATEUSERNAME", createUserName, createUserName));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "EMPLOYEEPOSTID", Info.EMPLOYEEPOSTID, lkPost.TxtLookUp.Text));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEEENTRY", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            string a = mx.TableToXml(Info, sysuserList, StrSource, AutoList);

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
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("POSTLEVEL", EmployeePost.POSTLEVEL.ToString());

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEENTRY>(EmployeeEntry, para, "HR");
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeEntry);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeEntry.CREATEUSERID);
            paraIDs.Add("CreatePostID", EmployeeEntry.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeEntry.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeEntry.OWNERCOMPANYID);

            if (EmployeeEntry.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                // Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEENTRY", EmployeeEntry.EMPLOYEEENTRYID, strXmlObjectSource, EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEENTRY", EmployeeEntry.EMPLOYEEENTRYID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEENTRY", EmployeeEntry.EMPLOYEEENTRYID, strXmlObjectSource);
            }
        }

        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_EMPLOYEEENTRY", "EMPLOYEEENTRYID", EmployeeEntry.EMPLOYEEENTRYID, args);
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    EmployeePost.CHECKSTATE = "2";
                    EmployeePost.EDITSTATE = "1";
                    EmployeeEntry.T_HR_EMPLOYEE.EDITSTATE = "1";
                    EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEESTATE = "0";
                    EmployeeEntry.EDITSTATE = "1";
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (EmployeeEntry.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            EmployeeEntry.CHECKSTATE = state;
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
            #region
            //EmployeeEntry.CHECKSTATE = state;
            //EmployeeEntry.UPDATEDATE = System.DateTime.Now;
            //EmployeeEntry.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //client.EmployeeEntryUpdateAsync(EmployeeEntry, EmployeePost, UserState);
            //if (args == SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful)
            //{
            //    SysUser.STATE = "1";
            //    perclient.SysUserInfoUpdateAsync(SysUser, "Audit");
            //    T_HR_EMPLOYEEPOSTCHANGE postChange = new T_HR_EMPLOYEEPOSTCHANGE();
            //    postChange = new T_HR_EMPLOYEEPOSTCHANGE();
            //    postChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
            //    postChange.EMPLOYEENAME = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME;
            //    postChange.EMPLOYEECODE = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECODE;
            //    postChange.T_HR_EMPLOYEE.EMPLOYEEID = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID;
            //    postChange.POSTCHANGEID = Guid.NewGuid().ToString();
            //    postChange.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
            //    postChange.TOCOMPANYID = EmployeePost.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            //    postChange.TODEPARTMENTID = EmployeePost.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
            //    postChange.TOPOSTID = EmployeePost.T_HR_POST.POSTID;
            //    postChange.ISAGENCY = "0";
            //    postChange.OWNERID = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID;
            //    postChange.OWNERPOSTID = postChange.TOPOSTID;
            //    postChange.OWNERDEPARTMENTID = postChange.TODEPARTMENTID;
            //    postChange.OWNERCOMPANYID = postChange.TOCOMPANYID;
            //    postChange.POSTCHANGCATEGORY = "2";
            //    postChange.EMPLOYEEPOSTID = EmployeePost.EMPLOYEEPOSTID;
            //    postChange.POSTCHANGREASON = Utility.GetResourceStr("EMPLOYEEENTRY");
            //    postChange.CHANGEDATE = EmployeeEntry.ENTRYDATE.ToString();
            //    postChange.CREATEUSERID = EmployeeEntry.T_HR_EMPLOYEE.CREATEUSERID;
            //    postChange.CREATECOMPANYID = EmployeeEntry.T_HR_EMPLOYEE.CREATECOMPANYID;
            //    postChange.CREATEDEPARTMENTID = EmployeeEntry.T_HR_EMPLOYEE.CREATEDEPARTMENTID;
            //    postChange.CREATEPOSTID = EmployeeEntry.T_HR_EMPLOYEE.CREATEPOSTID;
            //    client.AddEmployeePostChangeForEntryAsync(postChange);
            //    SendEngineXml();
            //}
            #endregion
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeEntry != null)
                state = EmployeeEntry.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        private bool Save()
        {
            // List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            //if (validators.Count > 0)
            //{
            //    //could use the content of the list to show an invalid message summary somehow
            //    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(lkPost.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (cbxPostLevel.SelectedItem == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(dpEntryDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ENTRYDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ENTRYDATE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(dpOnPostDate.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ONPOSTDATE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "ONPOSTDATE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                ///TODO:ADD 所属公司ID，部门ID，岗位ID
                //if (lkEmployeeName.DataContext is SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE)
                //{
                //    SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE employee = lkEmployeeName.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_EMPLOYEE;
                //    T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                //    temp.EMPLOYEEID = employee.EMPLOYEEID;
                //    temp.EMPLOYEECNAME = employee.EMPLOYEECNAME;
                //    EmployeePost.T_HR_EMPLOYEE = temp;
                //    EmployeeEntry.T_HR_EMPLOYEE = temp;
                //}
                string remarks = string.Empty;
                if (!string.IsNullOrEmpty(tbRemark.Text))
                {
                    // EmployeeEntry.REMARK = tbRemark.Text + "Ё";
                    remarks = tbRemark.Text + "Ё";
                }
                if (!string.IsNullOrEmpty(txtRemarks.Text))
                {
                    // EmployeeEntry.REMARK += txtRemarks.Text;
                    remarks += txtRemarks.Text;
                }
                EmployeeEntry.REMARK = remarks;
                if (txtUser.Text == "")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "USERNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "USERNAME"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                if(SysUser==null)
                {
                    SysUser = new T_SYS_USER();
                }

                SysUser.USERNAME = txtUser.Text;
                //改密码的时候作判断
                bool FlagSave = true;
                if (OldPassword == txtPwd.Password)
                {
                    SysUser.PASSWORD = txtPwd.Password;
                }
                else
                {
                    if (CheckPwd(txtPwd.Password))
                    {
                        SysUser.PASSWORD = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(txtPwd.Password.Trim());
                        OldPassword = SysUser.PASSWORD;
                        FlagSave = true;
                    }
                    else
                    {
                        FlagSave = false;
                    }
                }
                SysUser.REMARK = txtMark.Text;
                SysUser.OWNERCOMPANYID = EmployeeEntry.OWNERCOMPANYID;
                SysUser.OWNERDEPARTMENTID = EmployeeEntry.OWNERDEPARTMENTID;
                SysUser.OWNERPOSTID = EmployeeEntry.OWNERPOSTID;
                SysUser.OWNERID = EmployeeEntry.OWNERID;
                //SendEngineXml();
                if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
                {
                    if (SysUser.SYSUSERID==null)
                    {
                        SysUser.SYSUSERID = Guid.NewGuid().ToString();
                        SysUser.EMPLOYEEID = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID;
                        SysUser.EMPLOYEECODE = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECODE;
                        SysUser.EMPLOYEENAME = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME;
                        SysUser.CREATEDATE = DateTime.Now;
                        SysUser.CREATEUSER = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    
                        perclient.SysUserInfoAddAsync(SysUser);
                    }
                    else
                    {
                        if (FlagSave)
                        {
                            perclient.SysUserInfoUpdateAsync(SysUser, "Edit");
                            
                        }
                        else
                        {
                            RefreshUI(RefreshedTypes.HideProgressBar);
                        }
                    }
                    
                }
                else
                {
                    perclient.SysUserInfoAddAsync(SysUser);
                }

                //if (FormType == FormTypes.Edit)
                //{
                //    EmployeeEntry.UPDATEDATE = System.DateTime.Now;
                //    EmployeeEntry.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                //    client.EmployeeEntryUpdateAsync(EmployeeEntry,EmployeePost);
                //}
                //else
                //{
                //    ///TODO:ADD 创建人公司ID，部门ID，岗位ID
                //    client.EmployeeEntryAddAsync(EmployeeEntry, EmployeePost);
                //}
            }
            return true;
        }
        /// <summary>
        /// added by luojie
        /// 用与验证密码的合法性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool CheckPwd(string password)
        {
            bool legalPwd = true;
            string rstMessage = string.Empty;
            if (password != null)
            {
                if (password.Length < 8)
                {
                    legalPwd = false;
                    rstMessage = "密码不能小于8位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                if (password.Length > 15)
                {
                    legalPwd = false;
                    rstMessage = "密码不能大于15位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                string ptnNum = @"\D[0-9]+";
                string ptnWord = @"[0-9][a-z_A-Z]+";
                Match matchNum = Regex.Match(password, ptnNum);
                Match matchWord = Regex.Match(password, ptnWord);
                if (!matchNum.Success && !matchWord.Success)
                {
                    legalPwd = false;
                    rstMessage = "密码必须是8-15位的英文与数字组合";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码必须是中英文结合的");
                }
                if(!string.IsNullOrWhiteSpace(rstMessage)) Utility.ShowCustomMessage(MessageTypes.Error, "错误", rstMessage);
            }
            else
            {
                legalPwd = false;
            }
            return legalPwd;
        }

        /// <summary>
        ///     回到提交前的状态 by luojie
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            //isSubmit = false;

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

        //by luojie 
        //工具栏提交按钮的重载方法
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            needsubmit = true;
            isSubmit = true;

            Save();
        }

        void perclient_SysUserInfoUpdateCompleted(object sender, SysUserInfoUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() != "Audit")
                {
                    EmployeeEntry.UPDATEDATE = System.DateTime.Now;
                    EmployeeEntry.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    //EmployeePost.CHECKSTATE = "2";
                    //EmployeePost.EDITSTATE = "1";
                    EmployeePost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
                    client.EmployeeEntryUpdateAsync(EmployeeEntry, EmployeePost, "Edit");
                }
            }
        }

        void perclient_SysUserInfoAddCompleted(object sender, SMT.Saas.Tools.PermissionWS.SysUserInfoAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.Result),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                //所属
              
                if (FormType == FormTypes.Edit)
                {
                    EmployeeEntry.UPDATEDATE = System.DateTime.Now;
                    EmployeeEntry.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    //EmployeePost.CHECKSTATE = "2";
                    //EmployeePost.EDITSTATE = "1";
                    EmployeePost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
                    client.EmployeeEntryUpdateAsync(EmployeeEntry, EmployeePost, "Edit");
                }
                else
                {
                    EmployeeEntry.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    EmployeeEntry.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    EmployeeEntry.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    //EmployeeEntry.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    //EmployeeEntry.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    //EmployeeEntry.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    EmployeeEntry.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.EmployeeEntryAddAsync(EmployeeEntry, EmployeePost);
                }
               
            }
        }

        void client_EmployeeEntryAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEEENTRY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.All);
            }
        }

        void client_EmployeeEntryUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
        Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                ///by luojie 转至提交按钮原来的方法
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
                canSubmit = true;
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
            RefreshUI(RefreshedTypes.HideProgressBar);
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
            //        //给系统用户赋值
            //        SysUser.EMPLOYEEID = ent.T_HR_EMPLOYEE.EMPLOYEEID;
            //        SysUser.EMPLOYEECODE = ent.T_HR_EMPLOYEE.EMPLOYEECODE;
            //        SysUser.EMPLOYEENAME = ent.T_HR_EMPLOYEE.EMPLOYEECNAME;
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void lkPost_FindClick(object sender, EventArgs e)
        {
            //OrganizationLookupForm lookup = new OrganizationLookupForm();
            //OrganizationLookup lookup = new OrganizationLookup("4afd1d50-b84d-4ea3-8c81-0078c35fea5e","0","T_FB_CHARGEAPPLYMASTER");

            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_POST ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                //T_HR_POST entity = Utility.CloneObject<T_HR_POST>(ent);
                if (ent == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, "员工岗位", "当前选取的岗位为空");
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), "当前选取的岗位为空",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                HandlePostChanged(ent);
                orclient.GetPostNumberAsync(ent.POSTID);

            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        /// <summary>
        ///岗位的全称
        /// </summary>
        /// <param name="dep"></param>
        /// <returns></returns>
        public string GetFullOrgName(string depID)
        {
            List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> allCompanys = Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>;
            List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT> allDepartments = Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>;
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY company = new SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY();
            string orgName = string.Empty;
            string fatherType = "0";
            string fatherID = "";
            bool hasFather = false;

            department = (from c in allDepartments
                          where c.DEPARTMENTID == depID
                          select c).FirstOrDefault();
            if (department != null)
            {
                orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                {
                    fatherType = department.FATHERTYPE;
                    fatherID = department.FATHERID;
                    hasFather = true;
                }
                else
                {
                    hasFather = false;
                }
            }

            while (hasFather)
            {
                if (fatherType == "1" && !string.IsNullOrEmpty(fatherID))
                {
                    department = (from de in allDepartments
                                  where de.DEPARTMENTID == fatherID
                                  select de).FirstOrDefault();
                    if (department != null)
                    {
                        orgName += " - " + department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        if (!string.IsNullOrEmpty(department.FATHERTYPE) && !string.IsNullOrEmpty(department.FATHERID))
                        {
                            fatherID = department.FATHERID;
                            fatherType = department.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }
                }
                else if (fatherType == "0" && !string.IsNullOrEmpty(fatherID))
                {
                    company = (from com in allCompanys
                               where com.COMPANYID == fatherID
                               select com).FirstOrDefault();

                    if (company != null)
                    {
                        orgName += " - " + company.CNAME;
                        if (!string.IsNullOrEmpty(company.FATHERTYPE) && !string.IsNullOrEmpty(company.FATHERID))
                        {
                            fatherID = company.FATHERID;
                            fatherType = company.FATHERTYPE;
                        }
                        else
                        {
                            hasFather = false;
                        }
                    }
                    else
                    {
                        hasFather = false;
                    }

                }
                else
                {
                    hasFather = false;
                }

            }
            return orgName;
        }
        private void HandlePostChanged(SMT.Saas.Tools.OrganizationWS.T_HR_POST ent)
        {
            lkPost.DataContext = ent;
            T_HR_POST temp = new T_HR_POST();
            temp.POSTID = ent.POSTID;
            EmployeePost.T_HR_POST = temp;
            string orgName = ent.T_HR_POSTDICTIONARY.POSTNAME + GetFullOrgName(ent.T_HR_DEPARTMENT.DEPARTMENTID);
            lkPost.TxtLookUp.Text = orgName;
            //txtCompanyName.Text = ent.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
            //txtDepartment.Text = ent.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;

            SysUser.OWNERCOMPANYID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            SysUser.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            SysUser.OWNERPOSTID = ent.POSTID;

            EmployeeEntry.T_HR_EMPLOYEE.OWNERCOMPANYID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            EmployeeEntry.T_HR_EMPLOYEE.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            EmployeeEntry.T_HR_EMPLOYEE.OWNERPOSTID = ent.POSTID;

            EmployeeEntry.OWNERCOMPANYID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            EmployeeEntry.OWNERDEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            EmployeeEntry.OWNERPOSTID = ent.POSTID;

            foreach (T_SYS_DICTIONARY item in cbxPostLevel.Items)
            {
                if (item.DICTIONARYVALUE == ent.POSTLEVEL)
                {
                    cbxPostLevel.SelectedItem = item;
                    break;
                }
            }
        }
        void EnablControl()
        {
            //lkPost.IsEnabled = false;
            lkPost.TxtLookUp.IsReadOnly = true;
            lkPost.SearchButton.IsEnabled = false;
            txtPwd.IsEnabled = false;
            txtUser.IsEnabled = false;
            txtMark.IsEnabled = false;
            numPorbationperiod.IsEnabled = false;
            dpEntryDate.IsEnabled = false;
            dpOnPostDate.IsEnabled = false;
            cbxPostLevel.IsEnabled = false;
        }
        //void SendEngineXml()
        //{
        //    //向引擎发提醒
        //    T_HR_EMPLOYEECHECK EmployeeCheck = new T_HR_EMPLOYEECHECK();
        //    EmployeeCheck.BEREGULARID = Guid.NewGuid().ToString();
        //    T_HR_EMPLOYEE emp = new T_HR_EMPLOYEE();
        //    emp.EMPLOYEEID = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEID;
        //    emp.EMPLOYEECNAME = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEECNAME;
        //    emp.EMPLOYEEENAME = EmployeeEntry.T_HR_EMPLOYEE.EMPLOYEEENAME;
        //    EmployeeCheck.T_HR_EMPLOYEE = emp;
        //    EmployeeCheck.REPORTDATE = Convert.ToDateTime(EmployeeEntry.ENTRYDATE).AddMonths(Convert.ToInt32(EmployeeEntry.PROBATIONPERIOD));
        //    EmployeeCheck.OWNERCOMPANYID = EmployeeEntry.OWNERCOMPANYID;
        //    client.GetEmployeeCheckEngineXmlAsync(EmployeeCheck);
        //}
    }
}
