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
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
using SMT.Saas.Tools.DailyManagementWS;
using System.Collections.ObjectModel;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class LeftOfficeForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public FormTypes FormType { get; set; }
        private T_HR_LEFTOFFICE leftOffice;
        public string createUserName;
        SMT.Saas.Tools.FBServiceWS.FBServiceClient fbClient = new SMT.Saas.Tools.FBServiceWS.FBServiceClient();
        DailyManagementServicesClient DMClient = new DailyManagementServicesClient();
        public bool needsubmit = false;//提交审核,
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private bool isViewEmpolyeeID = true;

        public T_HR_LEFTOFFICE LeftOffice
        {
            get { return leftOffice; }
            set
            {
                leftOffice = value;
                this.DataContext = value;
            }
        }
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private T_HR_EMPLOYEEPOST employeePostSelcected { get; set; }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string leftofficeID;
        PersonnelServiceClient client;
        SMT.Saas.Tools.FlowWFService.ServiceClient flowClient;
        private int postCount;
        public LeftOfficeForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            leftofficeID = strID;
            InitParas(strID);
            // fbClient.GetLeavingUserAsync(LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
        }
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public LeftOfficeForm()
        {
            if (Application.Current.Resources["CustomDateConverter"] == null)
            {
                Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            }
            InitializeComponent();
            FormType = FormTypes.New;
            leftofficeID = "";
            InitParas(leftofficeID);
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            flowClient = new Saas.Tools.FlowWFService.ServiceClient();
            client.GetLeftOfficeByIDCompleted += new EventHandler<GetLeftOfficeByIDCompletedEventArgs>(client_GetLeftOfficeByIDCompleted);
            client.LeftOfficeUpdateCompleted += new EventHandler<LeftOfficeUpdateCompletedEventArgs>(client_LeftOfficeUpdateCompleted);
            client.LeftOfficeAddCompleted += new EventHandler<LeftOfficeAddCompletedEventArgs>(client_LeftOfficeAddCompleted);
            client.GetPostsActivedByEmployeeIDCompleted += new EventHandler<GetPostsActivedByEmployeeIDCompletedEventArgs>(client_GetPostsActivedByEmployeeIDCompleted);
            client.GetEmployeePostByEmployeePostIDCompleted += new EventHandler<GetEmployeePostByEmployeePostIDCompletedEventArgs>(client_GetEmployeePostByEmployeePostIDCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            DMClient.GetPersonAccountListByMultSearchCompleted += new EventHandler<GetPersonAccountListByMultSearchCompletedEventArgs>(DMClient_GetPersonAccountListByMultSearchCompleted);
            flowClient.IsExistFlowDataByUserIDCompleted += new EventHandler<Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs>(flowClient_IsExistFlowDataByUserIDCompleted);
            fbClient.GetLeavingUserCompleted += new EventHandler<Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs>(fbClient_GetLeavingUserCompleted);

            client.GetEmployeeViewsPagingCompleted += new EventHandler<GetEmployeeViewsPagingCompletedEventArgs>(client_GetEmployeeViewsPagingCompleted);

            this.Loaded += new RoutedEventHandler(LeftOfficeForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            */
            #endregion
        }
        /// <summary>
        /// 得到目前登录人所能看到的人事档案的员工id，为了不新加新的接口，用这种办法，
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeViewsPagingCompleted(object sender, GetEmployeeViewsPagingCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                List<V_EMPLOYEEVIEW> list = e.Result.Where(s => s.EMPLOYEEID == LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID).ToList();
                if (list == null || list.Count() <= 0)
                {
                    isViewEmpolyeeID = false;
                }
                else
                {
                    isViewEmpolyeeID = true;
                }
            }
        }



        //获取相应人员的借款
        public void GetPersonAccountData()
        {
            if (leftOffice == null)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(LeftOffice.OWNERCOMPANYID))
            {
                return;
            }
            if (leftOffice.T_HR_EMPLOYEE == null)
            {
                return;
            }
            T_FB_PERSONACCOUNT temp = new T_FB_PERSONACCOUNT();
            string filter = "";    //查询过滤条件
            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
            if (!string.IsNullOrEmpty(LeftOffice.OWNERCOMPANYID))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(OWNERCOMPANYID) ";
                paras.Add(LeftOffice.OWNERCOMPANYID);
            }
            if (!string.IsNullOrEmpty(leftOffice.T_HR_EMPLOYEE.EMPLOYEEID))
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    filter += " and ";
                }
                filter += "@" + paras.Count().ToString() + ".Contains(OWNERID) ";
                paras.Add(leftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
            }
            DMClient.GetPersonAccountListByMultSearchAsync(filter, paras, "OWNERID");
        }

        void DMClient_GetPersonAccountListByMultSearchCompleted(object sender, GetPersonAccountListByMultSearchCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                //DtBorrowMoney.ClearValue
                if (e.Error != null)
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.Error == null)
                    {
                        if (e.Result != null && e.Result.Count > 0)
                        {
                            DtBorrowMoney.ItemsSource = e.Result.Where(c => c.BORROWMONEY > 0).ToList();
                            return;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未知错误"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 未还款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fbClient_GetLeavingUserCompleted(object sender, Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                DtBorrowMoney.ItemsSource = e.Result;

            }
        }

        void LeftOfficeForm_Loaded(object sender, RoutedEventArgs e)
        {
            //重载提交按钮-提交先保存
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            #region 原来的
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                LeftOffice = new T_HR_LEFTOFFICE();
                LeftOffice.DIMISSIONID = Guid.NewGuid().ToString();
                LeftOffice.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                LeftOffice.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                LeftOffice.CREATEDATE = DateTime.Now;
                leftOffice.APPLYDATE = DateTime.Now;
                LeftOffice.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                LeftOffice.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                LeftOffice.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                LeftOffice.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                T_HR_EMPLOYEE employeeTmp = new T_HR_EMPLOYEE();
                employeeTmp.EMPLOYEEID = Common.CurrentLoginUserInfo.EmployeeID;
                employeeTmp.EMPLOYEECNAME = Common.CurrentLoginUserInfo.EmployeeName;
                LeftOffice.T_HR_EMPLOYEE = employeeTmp;

                dpApplyDate.Text = System.DateTime.Now.ToShortDateString();
                lkEmployeeName.DataContext = employeeTmp;
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                client.GetPostsActivedByEmployeeIDAsync(employeeTmp.EMPLOYEEID);
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                lkEmployeeName.IsEnabled = false;
                GetEmployeeViewsPaging();
                client.GetLeftOfficeByIDAsync(leftofficeID);
            }
            //设置默认值：新建时，离职类型只有辞职，其他的都不要
            //cbxEmployeeType.SelectedValue = "1";
            //cbxEmployeeType.IsEnabled = false;
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

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_LEFTOFFICE", LeftOffice.OWNERID,
            //        LeftOffice.OWNERPOSTID, LeftOffice.OWNERDEPARTMENTID, LeftOffice.OWNERCOMPANYID);

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
                //ToolbarItems = Utility.CreateFormEditButton("T_HR_LEFTOFFICE", LeftOffice.OWNERID,
                //    LeftOffice.OWNERPOSTID, LeftOffice.OWNERDEPARTMENTID, LeftOffice.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("LEFTOFFICE");
        }
        public string GetStatus()
        {
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
            flowClient.DoClose();
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
        private string GetXmlString(string StrSource, T_HR_LEFTOFFICE Info)
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
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevelDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "POSTLEVEL" && s.DICTIONARYVALUE == employeePostSelcected.POSTLEVEL).FirstOrDefault();
            postLevelName = postLevelDict == null ? "" : postLevelDict.DICTIONARYNAME;

           // SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY LEFTOFFICECATEGORY = cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            string postname = employeePostSelcected.T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME + " - " + employeePostSelcected.T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME
                              + " - " + employeePostSelcected.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "CHECKSTATE", "1", checkState));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "LEFTOFFICECATEGORY", "1", "辞职"));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "EMPLOYEEID", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "POSTLEVEL", employeePostSelcected.POSTLEVEL.ToString(), postLevelName));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "EMPLOYEECNAME", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "OWNER", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "CREATEUSERNAME", createUserName, createUserName));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "POST", "", postname));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "EMPLOYEEPOSTID", Info.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID, Info.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID));

            AutoList.Add(basedata("T_HR_LEFTOFFICE", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_LEFTOFFICE", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
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
            if (LeftOffice != null)
            {
                if (leftOffice.T_HR_EMPLOYEE != null)
                {
                    para.Add("EMPLOYEECNAME", LeftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME);
                    para.Add("EMPLOYEEID", LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                    para.Add("OWNER", LeftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME);
                }
                if (employeePostSelcected != null)
                {
                    para.Add("POSTLEVEL", employeePostSelcected.POSTLEVEL.ToString());
                }
                para.Add("CREATEUSERNAME", createUserName);

            }


            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("LEFTOFFICECATEGORY", (cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxEmployeeType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("T_HR_EMPLOYEEReference", LeftOffice.T_HR_EMPLOYEE == null ? "" : LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID + "#" + LeftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            // strXmlObjectSource = Utility.ObjListToXml<T_HR_LEFTOFFICE>(LeftOffice, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, LeftOffice);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            if (leftOffice != null)
            {
                if (leftOffice.T_HR_EMPLOYEE != null)
                {
                    paraIDs.Add("CreateUserID", LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                    paraIDs.Add("CreateUserName", LeftOffice.T_HR_EMPLOYEE.EMPLOYEECNAME);
                }
                paraIDs.Add("CreatePostID", LeftOffice.OWNERPOSTID);
                paraIDs.Add("CreateDepartmentID", LeftOffice.OWNERDEPARTMENTID);
                paraIDs.Add("CreateCompanyID", LeftOffice.OWNERCOMPANYID);
            }

            if (LeftOffice.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICE", LeftOffice.DIMISSIONID, strXmlObjectSource, LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICE", LeftOffice.DIMISSIONID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICE", LeftOffice.DIMISSIONID, strXmlObjectSource);
            }

            //  Utility.SetAuditEntity(entity, "T_HR_LEFTOFFICE", LeftOffice.DIMISSIONID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //Utility.UpdateCheckState("T_HR_LEFTOFFICE", "DIMISSIONID", LeftOffice.DIMISSIONID, args);
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
                    //更新岗位
                    // client.updateAllpostByemployeeIDAsync(leftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (LeftOffice.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            LeftOffice.CHECKSTATE = state;
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
            // client.LeftOfficeUpdateAsync(LeftOffice, strMsg, UserState);

        }
        public string GetAuditState()
        {
            string state = "-1";
            if (LeftOffice != null)
                state = LeftOffice.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            // List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee = lkEmployeeName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
            //是否选择员工
            if (employee != null)
            {
                T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                temp.EMPLOYEEID = employee.EMPLOYEEID;
                temp.EMPLOYEECNAME = employee.EMPLOYEECNAME;
                temp.OWNERID = employee.EMPLOYEEID;
                temp.OWNERPOSTID = employee.OWNERPOSTID;
                temp.OWNERDEPARTMENTID = employee.OWNERDEPARTMENTID;
                temp.OWNERCOMPANYID = employee.OWNERCOMPANYID;
                LeftOffice.T_HR_EMPLOYEE = temp;

                // flowClient.IsExistFlowDataByUserIDAsync(employee.EMPLOYEEID);
                //  LeftOffice.T_HR_EMPLOYEE = employee;


            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }

            SaveLeftOffice();


            return true;
        }
        void SaveLeftOffice()
        {
            try
            {


                ////判断有没有借款(需求：有借款也能提交)
                //if (DtBorrowMoney.ItemsSource != null)
                //{
                //    List<T_FB_PERSONACCOUNT> bors = DtBorrowMoney.ItemsSource as List<T_FB_PERSONACCOUNT>;
                //    if (bors != null)
                //    {
                //        //xiedx
                //        //2012-8-27
                //        //foreach (var temp in bors)
                //        //{
                //        //    if (temp.BORROWMONEY > 0)
                //        //    {
                //        //        RefreshUI(RefreshedTypes.HideProgressBar);
                //        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未处理借款"),
                //        //            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //        //        //return;2013/3/29 按集团要求暂时屏蔽
                //        //    }
                //        //}
                //        //if (bors.Count() > 0)
                //        //{
                //        //    RefreshUI(RefreshedTypes.HideProgressBar);
                //        //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未处理借款"),
                //        //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //        //    return;
                //        //}

                //    }
                //}


                //是否选择离职类型
                //if (cbxEmployeeType.SelectedItem == null)
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "LEFTOFFICECATEGORY"),
                //      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    RefreshUI(RefreshedTypes.HideProgressBar);
                //    return;
                //}
                //是否选择离职时间
                if (string.IsNullOrEmpty(dpLeftDate.Text))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "LEFTOFFICEDATE"),
                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                //是否选择申请时间
                if (string.IsNullOrEmpty(dpApplyDate.Text))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "APPLYDATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                //离职时间要大于申请时间
                if (DateTime.Parse(dpLeftDate.Text) <= DateTime.Parse(dpApplyDate.Text))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("离职时间要大于申请时间"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                // 是否选择岗位
                if (employeePostSelcected != null)
                {
                    //判断主岗位离职
                    if (LeftOffice.ISAGENCY == "0" && postCount > 1)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请先做兼职离职"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        return;
                    }

                    //if (LeftOffice.ISAGENCY == "1" && postCount > 1)
                    //{

                    //}

                    LeftOffice.T_HR_EMPLOYEEPOST = new T_HR_EMPLOYEEPOST();
                    LeftOffice.T_HR_EMPLOYEEPOST.EMPLOYEEPOSTID = employeePostSelcected.EMPLOYEEPOSTID;
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POST"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }


                if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //权限比较，看当前用户有没有权限进行提单操作
                if (!isViewEmpolyeeID)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                string strMsg = "";
                LeftOffice.LEFTOFFICECATEGORY = "1";
                if (FormType == FormTypes.New)
                {
                    //所属
                    LeftOffice.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    LeftOffice.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    LeftOffice.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    LeftOffice.OWNERID = LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID;

                    client.LeftOfficeAddAsync(LeftOffice, strMsg);
                }
                else
                {
                    LeftOffice.UPDATEDATE = System.DateTime.Now;
                    LeftOffice.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; ;

                    client.LeftOfficeUpdateAsync(LeftOffice, strMsg, "Edit");


                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.ToString()),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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

        #region 完成事件

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
        /// <summary>
        /// 获取员工有效岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostsActivedByEmployeeIDCompleted(object sender, GetPostsActivedByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
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
                postCount = e.Result.Count();
                foreach (var p in e.Result)
                {
                    if (p.ISAGENCY == "0")  //0:主岗位； 1：兼职岗位
                    {
                        if (FormType == FormTypes.New)
                        {
                            employeePostSelcected = p;
                            LeftOffice.ISAGENCY = employeePostSelcected.ISAGENCY;
                        }
                        p.ISAGENCY = "主岗位";
                    }
                    else
                    {
                        p.ISAGENCY = "兼职岗位";
                    }
                }

                DtGrid.ItemsSource = e.Result;
                GetPersonAccountData();

            }
        }
        /// <summary>
        /// 获取员工岗位(GetPostsActivedByEmployeeIDAsync服务调用完成)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeePostByEmployeePostIDCompleted(object sender, GetEmployeePostByEmployeePostIDCompletedEventArgs e)
        {
            List<T_HR_EMPLOYEEPOST> post = new List<T_HR_EMPLOYEEPOST>();
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
                post.Add(e.Result);
                foreach (var p in post)
                {
                    if (p.ISAGENCY == "0")  //0:主岗位； 1：兼职岗位
                    {
                        if (FormType == FormTypes.New)
                        {
                            employeePostSelcected = p;
                            LeftOffice.ISAGENCY = employeePostSelcected.ISAGENCY;
                        }
                        p.ISAGENCY = "主岗位";
                    }
                    else
                    {
                        p.ISAGENCY = "兼职岗位";
                    }
                }
                DtGrid.ItemsSource = post;
            }
        }
        /// <summary>
        /// 新增离职申请
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_LeftOfficeAddCompleted(object sender, LeftOfficeAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
        /// 修改离职申请
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_LeftOfficeUpdateCompleted(object sender, LeftOfficeUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
                if (e.UserState.ToString() == "Edit" && !isSubmit)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    isSubmit = false;
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "LEFTOFFICE"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
      Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "LEFTOFFICE"));
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
                    catch (Exception ex)
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
            RefreshUI(RefreshedTypes.HideProgressBar);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetLeftOfficeByIDCompleted(object sender, GetLeftOfficeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                LeftOffice = e.Result;

                employeePostSelcected = LeftOffice.T_HR_EMPLOYEEPOST;
                if (FormType == FormTypes.Resubmit)
                {
                    LeftOffice.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                }
                if (LeftOffice.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    client.GetPostsActivedByEmployeeIDAsync(LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                }
                else
                {
                    if (employeePostSelcected != null)
                    {
                        client.GetEmployeePostByEmployeePostIDAsync(employeePostSelcected.EMPLOYEEPOSTID);
                    }
                }
                // client.GetEmployeeByIDAsync(LeftOffice.CREATEUSERID);
                //RefreshUI(RefreshedTypes.AuditInfo);
                //SetToolBar();

                if (LeftOffice.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                                   || LeftOffice.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(LeftOffice.CREATEUSERID);
                    client.GetEmployeeToEngineAsync(CreateUserIDs);
                    fbClient.GetLeavingUserAsync(LeftOffice.CREATEUSERID);
                }
            }
        }
        /// <summary>
        /// 获取流程中是否有未处理完的单据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void flowClient_IsExistFlowDataByUserIDCompleted(object sender, Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.Result),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    List<T_HR_POST> dictp = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
                    if (dictp != null)
                    {
                        var ent = from c in dictp
                                  where c.FATHERPOSTID == LeftOffice.OWNERPOSTID
                                  select c;
                        if (ent.Count() > 0)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("注意，存在下级岗位"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                    SaveLeftOffice();
                }
                //else
                //{
                //    orClient.IsFatherPostCompleted+=(o,ex)=>
                //    {
                //        if(e.Error==null)
                //        {
                //            if(e.Result ==true)
                //            {
                //                string Result = "";
                //                ComfirmWindow com = new ComfirmWindow();
                //                com.OnSelectionBoxClosed += (obj, result) =>
                //                {
                //                    lkEmployeeName.DataContext = null;
                //                    DtGrid.ItemsSource = null;
                //                };
                //                com.SelectionBox(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("是岗位的直接上级，是否异动"), ComfirmWindow.confirmation, Result);
                //            }
                //        }
                //    };
                //}

            }
        }
        #endregion

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            try
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
                //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST;

                //    if (ent != null)
                //    {
                //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
                //    }
                //};

                //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
                #endregion
                OrganizationLookup lookup = new OrganizationLookup();

                lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
                lookup.SelectedClick += (obj, ev) =>
                {
                    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    DtBorrowMoney.ItemsSource = null;
                    if (ent != null)
                    {
                        employeePostSelcected = null;
                        lkEmployeeName.DataContext = ent;
                        client.GetPostsActivedByEmployeeIDAsync(ent.EMPLOYEEID);
                        //fbClient.GetLeavingUserAsync(LeftOffice.T_HR_EMPLOYEE.EMPLOYEEID);
                        GetEmployeeViewsPaging();
                        RefreshUI(RefreshedTypes.HideProgressBar);
                    }

                };

                lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("请正确选择员工"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBtn_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;


            foreach (var item in DtGrid.ItemsSource)
            {

                CheckBox cbx = DtGrid.Columns[0].GetCellContent(item).FindName("myChkBtn") as CheckBox;
                if (cbx != cb)
                {
                    cbx.IsChecked = false;
                }
                else
                {
                    if (cb.IsChecked == false)
                    {
                        employeePostSelcected = null;
                    }

                    else
                    {
                        employeePostSelcected = cb.Tag as T_HR_EMPLOYEEPOST;
                        LeftOffice.OWNERPOSTID = employeePostSelcected.T_HR_POST.POSTID;
                        LeftOffice.OWNERDEPARTMENTID = employeePostSelcected.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                        LeftOffice.OWNERCOMPANYID = employeePostSelcected.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;

                        if (employeePostSelcected.ISAGENCY == "主岗位" || employeePostSelcected.ISAGENCY == "0")
                        {
                            LeftOffice.ISAGENCY = "0";
                        }
                        else
                        {
                            LeftOffice.ISAGENCY = "1";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 加载Grid行，设置默认主岗位选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

            T_HR_EMPLOYEEPOST ep = (T_HR_EMPLOYEEPOST)e.Row.DataContext;
            CheckBox mychkBox = DtGrid.Columns[0].GetCellContent(e.Row).FindName("myChkBtn") as CheckBox;
            if (employeePostSelcected != null)
            {
                if (ep.EMPLOYEEPOSTID == employeePostSelcected.EMPLOYEEPOSTID)
                {
                    mychkBox.IsChecked = true;
                    employeePostSelcected = ep;
                }
            }
            mychkBox.Tag = ep;
        }

        //private void GridPagerMoney_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void GridPagerThing_Click(object sender, RoutedEventArgs e)
        //{

        //}
        /// <summary>
        /// 获取该人所能看到的所有人事档案，数据量大，是目前能想到的解决办法
        /// </summary>
        private void GetEmployeeViewsPaging()
        {
            try
            {
                int pageSize = 0, pageIndex = 0, pageCount = 0;
                string filter = string.Empty, strMsg = string.Empty;
                System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
                string sType = "", sValue = "";
                //不分页
                pageIndex = 1;
                pageSize = 999999;
                client.GetEmployeeViewsPagingAsync(pageIndex, pageSize, "EMPLOYEECNAME",
                filter, paras, pageCount, sType, sValue, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
            //catch捕捉但没做什么，为了不阻断程序执行
            catch { }
        }
    }
}
