using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.HRM.UI.Views.Organization;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
using System.Windows.Browser;
namespace SMT.HRM.UI.Form
{
    public partial class CompanyForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        private FormTypes formType;
        public string createUserName;
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        private T_HR_COMPANY company;

        public T_HR_COMPANY Company
        {
            get { return company; }
            set
            {
                company = value;
                this.DataContext = company;
            }
        }
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool canSubmit = false;//能否提交审核
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        OrganizationServiceClient client;
        SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient personClient;
        private string companyid;
        public delegate void refreshGridView();
        public event refreshGridView ReloadDataEvent;
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建  zwp 2011.10.09
        /// </summary>
        public CompanyForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            companyid = "";
            //InitControlEvent();
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
            };
        }

        /// <summary>
        /// 显示公司详细信息
        /// </summary>
        /// <param name="org">公司实例</param>
        public CompanyForm(T_HR_COMPANY org)
        {

            InitializeComponent();
            Company = org;
            // InitControlEvent();
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();
            };


            //绑定国家
            //  Utility.CbxItemBinder(cbxCountry, "COUNTYTYPE", Company.COUNTYTYPE);
        }
        /// <summary>
        /// 新增公司
        /// </summary>
        /// <param name="formType">操作类型</param>
        /// <param name="org">选择树的实例</param>
        public CompanyForm(FormTypes formType, string strID)
        {
            InitializeComponent();

            FormType = formType;
            companyid = strID;
            //InitControlEvent();
            this.Loaded += (sender, args) =>
            {
                InitControlEvent();

            };

            #region 原来的
            /*
            if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            {
                EnableControl();
            }
            */
            #endregion
            //if (FormType == FormTypes.Resubmit)
            //{
            //    EnableControl();
            //}
        }
        void EnableControl()
        {
            lkParentCompany.IsEnabled = false;
            lkParentDepartment.IsEnabled = false;
            cbxCheckState.IsEnabled = false;
            cbxCity.IsEnabled = false;
            cbxCompanyLevel.IsEnabled = false;
            cbxCompanyType.IsEnabled = false;
            cbxCountry.IsEnabled = false;
            cbxProvince.IsEnabled = false;
            txtSortNumber.IsEditable = false;
            txtSortNumber.IsHitTestVisible = false;
            btnEditIndex.IsEnabled = false;
            txtAccountCode.IsReadOnly = true;
            txtAddress.IsReadOnly = true;
            txtBankID.IsReadOnly = true;
            txtBussinessArea.IsReadOnly = true;
            txtBussinessLicenceNO.IsReadOnly = true;
            txtCName.IsReadOnly = true;
            txtCompanyCode.IsReadOnly = true;
            txtEmail.IsReadOnly = true;
            txtEName.IsReadOnly = true;
            txtFaxNumber.IsReadOnly = true;
            txtLeagalPersonID.IsReadOnly = true;
            txtLegalPerson.IsReadOnly = true;
            txtLinkMan.IsReadOnly = true;
            txtTelNumber.IsReadOnly = true;
            txtZipCode.IsReadOnly = true;
            txtRemark.IsReadOnly = true;
            txtBriefName.IsReadOnly = true;
            if (SMT.SaaS.FrameworkUI.Common.PermissionHelper.GetPermissionValue("T_HR_COMPANY", SMT.SaaS.FrameworkUI.Common.Permissions.Edit) < 0)
            {
                btnEditIndex.Visibility = Visibility.Collapsed;
                txtSortNumber.IsEnabled = false;
            }

        }

        private void InitControlEvent()
        {
            client = new OrganizationServiceClient();
            personClient = new Saas.Tools.PersonnelWS.PersonnelServiceClient();
            client.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(client_GetCompanyByIdCompleted);
            client.CompanyAddCompleted += new EventHandler<CompanyAddCompletedEventArgs>(client_CompanyAddCompleted);
            client.CompanyUpdateCompleted += new EventHandler<CompanyUpdateCompletedEventArgs>(client_CompanyUpdateCompleted);
            client.IsChildCompanyCompleted += new EventHandler<IsChildCompanyCompletedEventArgs>(client_IsChildCompanyCompleted);
            client.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(client_GetDepartmentByIdCompleted);
            client.CompanyIndexUpdateCompleted += new EventHandler<CompanyIndexUpdateCompletedEventArgs>(client_CompanyIndexUpdateCompleted);
            client.CompanyDeleteCompleted += new EventHandler<CompanyDeleteCompletedEventArgs>(client_CompanyDeleteCompletedEventArgs);
            personClient.GetEmployeeToEngineCompleted += new EventHandler<Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs>(personClient_GetEmployeeToEngineCompleted);
            //this.Loaded += new RoutedEventHandler(CompanyForm_Loaded);
            CompanForm_Load();
            
        }

       
        void CompanForm_Load()
        {
            #region 新增
            if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            {
                EnableControl();
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                Company = new T_HR_COMPANY();
                Company.COMPANYID = Guid.NewGuid().ToString();
                Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetCompanyByIdAsync(companyid);
            }

            if (FormType != FormTypes.Browse)
            {
                //Load事件之后，加载完后获取到父控件
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
                entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            }
        }
        void CompanyForm_Loaded(object sender, RoutedEventArgs e)
        {
            #region 新增
            if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            {
                EnableControl();
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                Company = new T_HR_COMPANY();
                Company.COMPANYID = Guid.NewGuid().ToString();
                Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.GetCompanyByIdAsync(companyid);
            }

            //Load事件之后，加载完后获取到父控件
            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            //entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
        }

        /// <summary>
        /// 新增的保存事件
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
        ///     回到提交前的状态
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;


            //隐藏工具栏 不允许二次提交
            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //#region 隐藏entitybrowser中的toolbar按钮
            //entBrowser.BtnSaveSubmit.IsEnabled = false;
            //if (entBrowser.EntityEditor is IEntityEditor)
            //{
            //    List<ToolbarItem> bars = GetToolBarItems();
            //    if (bars != null)
            //    {
            //        ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
            //        if (bar != null)
            //        {
            //            bar.Visibility = Visibility.Collapsed;
            //        }
            //    }
            //}
            //#endregion
            if (refreshType == RefreshedTypes.CloseAndReloadData)
            {
                //refreshType = RefreshedTypes.AuditInfo;
                refreshType = RefreshedTypes.HideProgressBar;
            }

        }
        void client_CompanyIndexUpdateCompleted(object sender, CompanyIndexUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void personClient_GetEmployeeToEngineCompleted(object sender, Saas.Tools.PersonnelWS.GetEmployeeToEngineCompletedEventArgs e)
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

        void client_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
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
                Company = e.Result;

                CheckFatherObject();
                if (!string.IsNullOrEmpty(company.COMPANYTYPE))
                {
                    foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxCompanyType.ItemsSource)
                    {
                        if (item.DICTIONARYVALUE.ToString() == company.COMPANYTYPE)
                        {
                            cbxCompanyType.SelectedItem = item;
                            break;
                        }
                    }
                }
                //绑定国家
                // Utility.CbxItemBinder(cbxCountry, "COUNTYTYPE", Company.COUNTYTYPE);

                //BindAduitInfo();
                if (FormType == FormTypes.Resubmit)
                {
                    //Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    //if (Company.EDITSTATE == Convert.ToInt32(EditStates.Actived).ToString())
                    //{
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                    //}
                    //else
                    //{
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //}
                    Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    if (Company.EDITSTATE != Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    }

                }
                if (Company.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    EnableControl();
                }
                //绑定城市
                BindCity();
                if (Company.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || Company.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(Company.CREATEUSERID);
                    personClient.GetEmployeeToEngineAsync(CreateUserIDs);
                }


            }

        }

        /// <summary>
        /// 为上级部门LookUp加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                T_HR_DEPARTMENT ent = e.Result;
                if (ent == null)
                {
                    return;
                }

                lkParentDepartment.DataContext = ent;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        /// <summary>
        /// 检查是否存在上级机构
        /// </summary>
        private void CheckFatherObject()
        {
            if (string.IsNullOrWhiteSpace(Company.FATHERTYPE) || string.IsNullOrWhiteSpace(Company.FATHERID))
            {
                return;
            }

            lkParentCompany.DataContext = null;
            lkParentDepartment.DataContext = null;

            if (Company.FATHERTYPE == "0")
            {
                lkParentCompany.DataContext = Company.T_HR_COMPANY2;
            }
            else if (Company.FATHERTYPE == "1")
            {
                client.GetDepartmentByIdAsync(Company.FATHERID);
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_COMPANY", Company.OWNERID,
            //        Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (Company != null)
                {
                    if (Company.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }

            else if (FormType == FormTypes.Resubmit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.RemoveAt(0);
                ToolbarItems.RemoveAt(0);
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_COMPANY", Company.OWNERID,
                    Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("COMPANYINFO");
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
            return "编辑中";
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
                    Cancel();
                    break;
                case "Delete":
                    delete(company.COMPANYID);
                    break;
                //case "2":
                //    SubmitAduit();
                //    break;
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
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_COMPANY", Company.OWNERID,
            //        Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (Company != null)
                {
                    if (Company.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }

            else if (FormType == FormTypes.Resubmit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.RemoveAt(0);
                ToolbarItems.RemoveAt(0);
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_COMPANY", Company.OWNERID,
                    Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID);
            }
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
        private string GetXmlString(string StrSource, T_HR_COMPANY Info)
        {
            T_SYS_DICTIONARY COMPANYTYPE = cbxCompanyType.SelectedItem as T_SYS_DICTIONARY;
            T_SYS_DICTIONARY COUNTYTYPE = cbxCountry.SelectedItem as T_SYS_DICTIONARY;
            T_SYS_DICTIONARY COMPANYLEVEL = cbxCompanyLevel.SelectedItem as T_SYS_DICTIONARY;
            T_SYS_DICTIONARY CITY = cbxCity.SelectedItem as T_SYS_DICTIONARY;
            T_SYS_DICTIONARY Province = cbxProvince.SelectedItem as T_SYS_DICTIONARY;

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_COMPANY", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_COMPANY", "COMPANYTYPE", COMPANYTYPE != null ? COMPANYTYPE.DICTIONARYVALUE.ToString() : "0", COMPANYTYPE != null ? COMPANYTYPE.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_COMPANY", "COUNTYTYPE", COUNTYTYPE != null ? COUNTYTYPE.DICTIONARYVALUE.ToString() : "0", COUNTYTYPE != null ? COUNTYTYPE.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_COMPANY", "COMPANYLEVEL", COMPANYLEVEL != null ? COMPANYLEVEL.DICTIONARYVALUE.ToString() : "0", COMPANYLEVEL != null ? COMPANYLEVEL.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_COMPANY", "CITY", CITY != null ? CITY.DICTIONARYVALUE.ToString() : "0", CITY != null ? CITY.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_COMPANY", "OWNERPROVINCE", Province != null ? Province.DICTIONARYVALUE.ToString() : "0", Province != null ? Province.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_COMPANY", "SORTINDEX", txtSortNumber.Value.ToString(), ""));
            if (Info.FATHERTYPE == "0")
            {
                AutoList.Add(basedata("T_HR_COMPANY", "SUPERIORORG", Info.FATHERID, lkParentCompany.TxtLookUp.Text));
            }
            else
            {
                AutoList.Add(basedata("T_HR_COMPANY", "SUPERIORDEPARTMENT", Info.FATHERID, lkParentDepartment.TxtLookUp.Text));
            }
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
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Company.OWNERID && Company.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            //RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                //RefreshUI(RefreshedTypes.ShowProgressBar);
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
            //  Utility.SetAuditEntity(entity, "T_HR_COMPANY", Company.COMPANYID);
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", createUserName);

            //Dictionary<string, string> para2 = new Dictionary<string, string>();
            //para2.Add("COMPANYTYPE", (cbxCompanyType.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCompanyType.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("COUNTYTYPE", (cbxCountry.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCountry.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("COMPANYLEVEL", (cbxCompanyLevel.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCompanyLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("CITY", (cbxCity.SelectedItem as T_SYS_DICTIONARY) == null ? "" : (cbxCity.SelectedItem as T_SYS_DICTIONARY).DICTIONARYNAME);
            //para2.Add("T_HR_COMPANY2Reference", Company.T_HR_COMPANY2 == null ? "" : Company.T_HR_COMPANY2.COMPANYID + "#" + Company.T_HR_COMPANY2.CNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, Company);
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_COMPANY>(Company, para, "HR");
            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_COMPANY>(Company, para, "HR", para2, null);

            Utility.SetAuditEntity(entity, "T_HR_COMPANY", Company.COMPANYID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            // RefreshUI(RefreshedTypes.ProgressBar);
            string state = "";
            string UserState = "Audit";
            string strMsg = "";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //if (FormType == FormTypes.Audit && Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    //{
                    //    state = Utility.GetCheckState(CheckStates.UnApproved);
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //}
                    //else
                    //{
                    //    state = Utility.GetCheckState(CheckStates.Approved);
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    //}
                    state = Utility.GetCheckState(CheckStates.Approved);
                    if (Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        Company.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                    }
                    else
                    {
                        Company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    //if (FormType == FormTypes.Audit && Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    //{
                    //    state = Utility.GetCheckState(CheckStates.Approved);
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    //}
                    //else
                    //{
                    //    state = Utility.GetCheckState(CheckStates.UnApproved);
                    //    Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    //}
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    if (Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                    {
                        Company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                    }
                    else
                    {
                        Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    }
                    break;
            }
            if (Company.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            Company.CHECKSTATE = state;
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
            // client.CompanyUpdateAsync(Company, strMsg, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (Company != null)
                state = Company.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            //List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            //if (validators.Count > 0)
            //{
            //    //  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIREDFIELDS"));
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    return false;
            //}
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                return false;
            }
            else
            {
                //国别赋值
                if (cbxCountry.SelectedItem == null)
                {
                    // Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COUNTYTYPEORAREA"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "COUNTYTYPEORAREA"),
                      Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                else
                {
                    T_SYS_DICTIONARY dic = cbxCountry.SelectedItem as T_SYS_DICTIONARY;
                    Company.COUNTYTYPE = dic.DICTIONARYVALUE.ToString();
                }
                //城市赋值
                if (cbxCity.SelectedItem == null)
                {
                    // Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "CITY"),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                else
                {
                    T_SYS_DICTIONARY dic = cbxCity.SelectedItem as T_SYS_DICTIONARY;
                    Company.CITY = dic.DICTIONARYVALUE.ToString();
                }
                if (cbxCompanyType.SelectedItem != null)
                {
                    Company.COMPANYTYPE = (cbxCompanyType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANYTYPE"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return false;
                }
                string strMsg = "";
                if (FormType == FormTypes.New)
                {
                    //所属
                    Company.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    Company.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Company.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Company.OWNERCOMPANYID = Company.COMPANYID;
                    Company.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    Company.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    Company.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    Company.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    company.CREATEDATE = DateTime.Now;
                    //添加公司
                    client.CompanyAddAsync(Company, strMsg);
                }
                else
                {
                    //如果状态为审核通过，修改时，则修改状态为审核中
                    if (Company.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        Company.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                        if (Company.EDITSTATE != Convert.ToInt32(EditStates.PendingCanceled).ToString())
                        {
                            Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                        }

                    }
                    Company.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    Company.UPDATEDATE = DateTime.Now;
                    //变更公司
                    client.CompanyUpdateAsync(Company, strMsg, "Edit");
                }
            }
            return true;
        }


        void client_CompanyAddCompleted(object sender, CompanyAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "COMPANRYCODE,CNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "COMPANRYCODE,CNAME"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }

                //  Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    closeForm();
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.Add(ToolBarItems.Delete);
                RefreshUI(RefreshedTypes.All);


            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 修改公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_CompanyUpdateCompleted(object sender, CompanyUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                needsubmit = false;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg == "Repetition")
                {
                    needsubmit = false;
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "COMPANRYCODE,CNAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REPETITIONTWOPARAS", "COMPANRYCODE,CNAME"),
                          Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (e.UserState.ToString() == "Edit")
                {
                    // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    if (!isSubmit)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    }
                }

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
                else if (e.UserState.ToString() == "Audit")
                {
                    // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "COMPANY"));
                    // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                     Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "COMPANY"));
                    // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                if (closeFormFlag)
                {
                    closeForm();
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }

        void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
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

            //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            //entBrowser.Close();
        }
        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void closeForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        public void Audit()
        {
            T_HR_COMPANYHISTORY companyHis = new T_HR_COMPANYHISTORY();
            companyHis.RECORDSID = Guid.NewGuid().ToString();
            companyHis = Utility.CloneObject<T_HR_COMPANYHISTORY>(Company);
            //companyHis.COMPANYCATEGORY = Company.COMPANYCATEGORY;
            //companyHis.COMPANRYCODE = Company.COMPANRYCODE;
            //companyHis.COMPANYLEVEL = Company.COMPANYLEVEL;
            //companyHis.COMPANYID = Company.COMPANYID;
            //companyHis.CNAME = Company.CNAME;
            //companyHis.ENAME = Company.ENAME;
            //companyHis.T_HR_COMPANY = Company.T_HR_COMPANY2;
            //companyHis.LEGALPERSON = Company.LEGALPERSON;
            //companyHis.LINKMAN = Company.LINKMAN;
            //companyHis.TELNUMBER = Company.TELNUMBER;
            //companyHis.ADDRESS = Company.ADDRESS;
            //companyHis.LEGALPERSONID = Company.LEGALPERSONID;
            //companyHis.BUSSINESSLICENCENO = Company.BUSSINESSLICENCENO;
            //companyHis.BUSSINESSAREA = Company.BUSSINESSAREA;
            //companyHis.ACCOUNTCODE = Company.ACCOUNTCODE;
            //companyHis.BANKID = Company.BANKID;
            //companyHis.EMAIL = Company.EMAIL;
            //companyHis.ZIPCODE = Company.ZIPCODE;
            //companyHis.FAXNUMBER = Company.FAXNUMBER;
            if (Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
            {
                companyHis.CANCELDATE = DateTime.Now;
            }
            companyHis.REUSEDATE = DateTime.Now;
            //companyHis.CREATEDATE = Company.CREATEDATE;
            //companyHis.CREATEUSERID = Company.CREATEUSERID;
            //companyHis.UPDATEDATE = DateTime.Now;
            //companyHis.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            client.CompanyHistoryAddAsync(companyHis);
        }

        void client_CompanyHistoryAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (Company.EDITSTATE == Convert.ToInt32(EditStates.PendingCanceled).ToString())
                {
                    Company.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    Company.EDITSTATE = Convert.ToInt32(EditStates.Canceled).ToString();
                }
                else
                {
                    Company.CHECKSTATE = Convert.ToInt32(CheckStates.Approved).ToString();
                    Company.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
                }
                Company.UPDATEDATE = DateTime.Now;
                Company.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.CompanyUpdateAsync(Company, "Audit");
            }
        }

        /// <summary>
        /// 效验LookUp所选的公司是否为合法的上级公司
        /// </summary>
        /// <param name="ent"></param>
        private void HandleParentComapnyChanged(T_HR_COMPANY ent)
        {
            if (Company.COMPANYID != ent.COMPANYID)
            {
                client.IsChildCompanyAsync(Company.COMPANYID, ent.COMPANYID, ent);
            }
            else
            {
                //MessageBox.Show("不能选自己作为父公司!");
                //  Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBESELFFATHERCOMPANY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("CANNOTBESELFFATHERCOMPANY"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 效验LookUp所选的部门是否为合法的上级部门
        /// </summary>
        /// <param name="ent"></param>
        private void HandleParentDepartmentChanged(T_HR_DEPARTMENT ent)
        {
            if (Company.COMPANYID != ent.T_HR_COMPANY.COMPANYID)
            {
                client.IsChildCompanyAsync(Company.COMPANYID, ent.T_HR_COMPANY.COMPANYID, ent);
            }
            else
            {
                // Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBESELFFATHERCOMPANY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("CANNOTBESELFFATHERCOMPANY"),
         Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        public void delete(string strid)
        {
            string Result = "";
            string strMsg = string.Empty;
            //提示是否删除
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                client.CompanyDeleteAsync(strid, strMsg);
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("确定要删除该公司信息？"), ComfirmWindow.titlename, Result);
        }

        public void client_CompanyDeleteCompletedEventArgs(object sender, CompanyDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.strMsg != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    closeForm();
                    //ReloadData();
                    //HtmlPage.Window.Invoke("SLCloseCurrentPage");
                }
            }
            FormType = FormTypes.Browse;
            this.RefreshUI(RefreshedTypes.All);
        }

        public void SubmitAudit()
        {
            if (Company != null)
            {
                //if (FormType == FormTypes.Resubmit)
                //{
                //    Company.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                //    Company.EDITSTATE = Convert.ToInt32(EditStates.PendingCanceled).ToString();
                //}
                //else
                //{
                Company.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                Company.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                //}
                Company.UPDATEDATE = DateTime.Now;
                Company.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.CompanyUpdateAsync(Company, "SubmitAudit");
            }
        }

        void client_IsChildCompanyCompleted(object sender, IsChildCompanyCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // MessageBox.Show(e.Error.Message);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }

            if (!e.Result)
            {
                // MessageBox.Show("您选择的父公司已是当前公司的子公司,不能为父公司！");
                // Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTBEFATHERCOMPANY"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("CANNOTBEFATHERCOMPANY"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

            lkParentCompany.DataContext = null;
            lkParentDepartment.DataContext = null;

            T_HR_COMPANY entCompany = e.UserState as T_HR_COMPANY;
            if (entCompany != null)
            {
                lkParentCompany.DataContext = entCompany;
                Company.FATHERID = entCompany.COMPANYID;
                Company.FATHERTYPE = "0";
                Company.T_HR_COMPANY2 = new T_HR_COMPANY();
                Company.T_HR_COMPANY2.COMPANYID = entCompany.COMPANYID;
                Company.T_HR_COMPANY2.CNAME = entCompany.CNAME;
                Company.T_HR_COMPANY2.COMPANYTYPE = entCompany.COMPANYTYPE;
                return;
            }

            T_HR_DEPARTMENT entDepartment = e.UserState as T_HR_DEPARTMENT;
            if (entDepartment != null)
            {
                lkParentDepartment.DataContext = entDepartment;
                Company.FATHERID = entDepartment.DEPARTMENTID;
                Company.FATHERTYPE = "1";
                Company.T_HR_COMPANY2 = new T_HR_COMPANY();
                Company.T_HR_COMPANY2.COMPANYID = entDepartment.T_HR_COMPANY.COMPANYID;
                Company.T_HR_COMPANY2.CNAME = entDepartment.T_HR_COMPANY.CNAME;
                Company.T_HR_COMPANY2.COMPANYTYPE = entDepartment.T_HR_COMPANY.COMPANYTYPE;
                return;
            }
        }

        /// <summary>
        /// 选择上级公司
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkParentCompany_FindClick(object sender, EventArgs e)
        {
            #region
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("COMPANRYCODE", "COMPANRYCODE");
            //cols.Add("CNAME", "CNAME");
            //cols.Add("ENAME", "ENAME");
            //LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Company,
            //    typeof(T_HR_COMPANY[]), cols);

            //lookup.SelectedClick += (o, ev) =>
            //{
            //    T_HR_COMPANY ent = lookup.SelectedObj as T_HR_COMPANY;

            //    if (ent != null)
            //    {
            //        HandleParentComapnyChanged(ent);
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            #endregion
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;
                if (ent != null)
                {
                    HandleParentComapnyChanged(ent);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 选择上级部门
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkParentDepartment_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                if (ent != null)
                {
                    HandleParentDepartmentChanged(ent);
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 选择国家和地区 绑定相关省
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbxCountry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY entity = cbxCountry.SelectedItem as T_SYS_DICTIONARY;
            Utility.CbxItemBinder(cbxProvince, "PROVINCE", "", entity.DICTIONARYID);
            BindCity();
        }
        /// <summary>
        /// 选择省 绑定相关城市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            T_SYS_DICTIONARY entity = cbxProvince.SelectedItem as T_SYS_DICTIONARY;
            cbxCity.SelectedItem = null;
            if (entity != null)
            {
                Utility.CbxItemBinder(cbxCity, "CITY", Company.CITY, entity.DICTIONARYID);
            }
        }

        //#region 审核功能
        //private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果

        //public void BindAduitInfo()
        //{

        //    //this.DialogResult = true;
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "T_HR_COMPANY";//"archivesLending";
        //    entity.FormID = Company.COMPANYID;//"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //    entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID; //"7cd6c0a4-9735-476a-9184-103b962d3383"; //
        //    entity.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //    entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //    entity.CreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

        //    entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
        //    entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;

        //    audit.BindingData();

        //}
        //public void SubmitAduit()
        //{

        //    if (Company != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "T_HR_COMPANY";//"archivesLending";T_HR_COMPANY
        //        entity.FormID = Company.COMPANYID; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //        entity.CreateCompanyID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;// "7cd6c0a4-9735-476a-9184-103b962d3383";
        //        entity.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

        //        entity.CreateUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;

        //        audit.Submit();
        //    }
        //}

        ///// <summary>
        ///// 提交审核完成
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        //{

        //    auditResult = e.Result;
        //    switch (auditResult)
        //    {
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
        //            //todo 审核中
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
        //            //todo 取消
        //            Cancel();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
        //            //todo 终审通过
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
        //            //todo 审核不通过
        //            SumbitCompleted();
        //            break;
        //        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
        //            //todo 审核异常
        //            HandError();
        //            break;
        //    }


        //}


        //private void SumbitCompleted()
        //{
        //    try
        //    {
        //        if (Company != null)
        //        {
        //            //isApprove = false;
        //            //lendingArchives.UPDATEDATE = DateTime.Now;
        //            //lendingArchives.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        //            //lendingArchives.UPDATEUSERNAME = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;

        //           Company.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

        //            switch (auditResult)
        //            {
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
        //                    Company.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
        //                    break;
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
        //                    Company.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
        //                    break;
        //                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
        //                    Company.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
        //                    break;
        //            }
        //            client.CompanyUpdateAsync(Company);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.ToString()));
        //    }
        //}



        //private void HandError()
        //{
        //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));

        //    //this.ReloadData();
        //}
        //#endregion

        void BindCity()
        {
            if (Company != null && !string.IsNullOrEmpty(Company.CITY))
            {
                decimal? cityValue = Convert.ToDecimal(Company.CITY);
                List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
                var ent = dicts.Where(s => s.DICTIONCATEGORY == "CITY" && s.DICTIONARYVALUE == cityValue).FirstOrDefault();
                if (ent == null)
                    return;
                if (cbxProvince.ItemsSource != null)
                {
                    foreach (var item in cbxProvince.Items)
                    {
                        T_SYS_DICTIONARY dict = item as T_SYS_DICTIONARY;
                        if (dict != null && ent.T_SYS_DICTIONARY2 != null)
                        {
                            if (dict.DICTIONARYID == ent.T_SYS_DICTIONARY2.DICTIONARYID)
                            {
                                cbxProvince.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }

            }
        }
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            personClient.DoClose();
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

        private void cbxCompanyType_Loaded(object sender, RoutedEventArgs e)
        {
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
            dicts = dicts.Where(s => s.DICTIONCATEGORY == "COMPANYTYPE" && s.DICTIONARYVALUE != -1).OrderBy(s => s.DICTIONARYVALUE).ToList();
            cbxCompanyType.ItemsSource = dicts.ToList();
            cbxCompanyType.DisplayMemberPath = "DICTIONARYNAME";
        }

        private void btnEditIndex_Click(object sender, RoutedEventArgs e)
        {
            T_HR_COMPANY temp = new T_HR_COMPANY();
            temp.COMPANYID = Company.COMPANYID;
            temp.SORTINDEX = Company.SORTINDEX;
            string strMsg = string.Empty;
            RefreshUI(RefreshedTypes.ShowProgressBar);
            client.CompanyIndexUpdateAsync(temp, strMsg);
        }
    }
}
