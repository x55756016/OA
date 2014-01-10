
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
using SMT.Saas.Tools.SalaryWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;

namespace SMT.HRM.UI.Form.Salary
{
    /// <summary>
    /// 员工加扣款批量提交审核
    /// </summary>
    public partial class EmployeeAddSumMassAudit : BaseForm, IEntityEditor, IAudit, IClient
    {
        public T_HR_EMPLOYEEADDSUM EmployeeAddSum { get; set; }
        public FormTypes FormType { get; set; }
        public int ObjectType { get; set; }
        public string ObjectValue { get; set; }
        public string CheckState { get; set; }
        private List<ToolbarItem> toolbarItems = new List<ToolbarItem>();
        private SMT.Saas.Tools.SalaryWS.SalaryServiceClient client = new Saas.Tools.SalaryWS.SalaryServiceClient();
        private SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orgClient = new Saas.Tools.OrganizationWS.OrganizationServiceClient();
        private string userID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
        BasePage basePage = new BasePage();
        private T_HR_EMPLOYEEADDSUMBATCH employeeAddSumBatch;

        public T_HR_EMPLOYEEADDSUMBATCH EmployeeAddSumBatch
        {
            get { return employeeAddSumBatch; }
            set
            {
                employeeAddSumBatch = value;
                this.DataContext = employeeAddSumBatch;
            }
        }
        List<T_HR_EMPLOYEEADDSUM> listDetail;
        private bool needsubmit = false;

        public EmployeeAddSumMassAudit()
        {
            InitializeComponent();
            InitEvents();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        private void InitEvents()
        {       
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
        }

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (employeeAddSumBatch.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                return;
            }

            if (employeeAddSumBatch == null)
            {
                return;
            }

            if (listDetail.Count() == 0)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            needsubmit = true;

            string oldCheckState = EmployeeAddSumBatch.CHECKSTATE;
            System.Collections.ObjectModel.ObservableCollection<string> ids = new System.Collections.ObjectModel.ObservableCollection<string>();
            foreach (var er in dgMassAuditList.ItemsSource)
            {
                ids.Add((er as T_HR_EMPLOYEEADDSUM).ADDSUMID);
            }
            if (ids.Count > 0)
            {
                if (oldCheckState == Utility.GetCheckState(CheckStates.UnSubmit))
                {                    
                    client.EmployeeAddSumBatchAddAsync(EmployeeAddSumBatch, ids);//提交审核
                }
            }

            RefreshUI(RefreshedTypes.ProgressBar);
        }    

        /// <summary>
        /// 从待办任务中打开
        /// </summary>
        /// <param name="formtype"></param>
        /// <param name="monthlyBatchID"></param>
        public EmployeeAddSumMassAudit(FormTypes formtype, string monthlyBatchID)
        {
            InitializeComponent();
            //InitEvents();
            FormType = formtype;
            basePage.GetEntityLogo("T_HR_EMPLOYEEADDSUMBATCH");
            //InitParas();
            client.GetEmployeeAddSumBatchByIDAsync(monthlyBatchID, "NoUnSubmit");
            this.Loaded += new RoutedEventHandler(EmployeeAddSumMassAudit_LoadedDaiBan);
        }


        void EmployeeAddSumMassAudit_LoadedDaiBan(object sender, RoutedEventArgs e)
        {
            this.InitEvents();
            InitParas();
            
        }
        /// <summary>
        /// 从view页面弹出批量审核的页面   还未提交
        /// </summary>
        /// <param name="formtype"></param>
        /// <param name="strType"></param>
        /// <param name="strValue"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="strCheckState"></param>
        public EmployeeAddSumMassAudit(FormTypes formtype, int strType, string strValue, string year, string month, string strCheckState)
        {
            InitializeComponent();
            txtBalanceYear.Text = year;
            nudBalanceMonth.Value = Convert.ToDouble(month);
            FormType = formtype;
            CheckState = strCheckState;
            cbxkAssignedObjectType.SelectedIndex = strType;
            ObjectType = strType;
            ObjectValue = strValue;
            BindAssignObjectLookup();
            basePage.GetEntityLogo("T_HR_EMPLOYEEADDSUMBATCH");
            EmployeeAddSumBatch = new T_HR_EMPLOYEEADDSUMBATCH();
            EmployeeAddSumBatch.MONTHLYBATCHID = Guid.NewGuid().ToString();
            EmployeeAddSumBatch.BALANCEOBJECTNAME = string.Empty;
            EmployeeAddSumBatch.BALANCEOBJECTID = ObjectValue;
            EmployeeAddSumBatch.BALANCEOBJECTTYPE = ObjectType.ToString();
            EmployeeAddSumBatch.BALANCEYEAR = Convert.ToDecimal(txtBalanceYear.Text);
            EmployeeAddSumBatch.BALANCEMONTH = Convert.ToDecimal(nudBalanceMonth.Value);
            EmployeeAddSumBatch.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            EmployeeAddSumBatch.CREATEDATE = System.DateTime.Now;
            EmployeeAddSumBatch.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
            EmployeeAddSumBatch.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            EmployeeAddSumBatch.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            EmployeeAddSumBatch.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            EmployeeAddSumBatch.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            EmployeeAddSumBatch.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            EmployeeAddSumBatch.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            EmployeeAddSumBatch.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            EmployeeAddSumBatch.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            
            this.Loaded += new RoutedEventHandler(EmployeeAddSumMassAudit_Loaded);
        }

        void EmployeeAddSumMassAudit_Loaded(object sender, RoutedEventArgs e)
        {
            this.InitEvents();
            InitParas();
            LoadData();
        }

        public void LoadData()
        {
            string filter = "audit";
            int pageCount = 0;
            DateTime? starttimes = new DateTime(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString()), 1);
            DateTime? endtimes = new DateTime(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString()), DateTime.DaysInMonth(Convert.ToInt32(txtBalanceYear.Text), Convert.ToInt32(nudBalanceMonth.Value.ToString())));
            System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
            client.GetEmployeeAddSumAuditPagingAsync(dataPager.PageIndex, dataPager.PageSize, "ADDSUMID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), userID, CheckState, ObjectType, ObjectValue, "New");
        }

        public void InitParas()
        {
            orgClient.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(orgClient_GetCompanyByIdCompleted);
            orgClient.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(orgClient_GetDepartmentByIdCompleted);
            orgClient.GetPostByIdCompleted += new EventHandler<GetPostByIdCompletedEventArgs>(orgClient_GetPostByIdCompleted);

            client.GetEmployeeAddSumAuditPagingCompleted += new EventHandler<GetEmployeeAddSumAuditPagingCompletedEventArgs>(client_GetEmployeeAddSumAuditPagingCompleted);
            client.GetEmployeeAddSumBatchByIDCompleted += new EventHandler<GetEmployeeAddSumBatchByIDCompletedEventArgs>(client_GetEmployeeAddSumBatchByIDCompleted);
            client.EmployeeAddSumBatchAddCompleted += new EventHandler<EmployeeAddSumBatchAddCompletedEventArgs>(client_EmployeeAddSumBatchAddCompleted);
            client.EmployeeAddSumBatchUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeAddSumBatchUpdateCompleted);
        }

        /// <summary>
        /// 审核完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeAddSumBatchUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 提交审核完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeeAddSumBatchAddCompleted(object sender, EmployeeAddSumBatchAddCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!e.Result)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (needsubmit == true)
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        needsubmit = false;
                    }
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void orgClient_GetPostByIdCompleted(object sender, GetPostByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_POST post = e.Result as OrganizationWS.T_HR_POST;
                lkAssignObject.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                lkAssignObject.DataContext = post;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void orgClient_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_DEPARTMENT depart = e.Result as OrganizationWS.T_HR_DEPARTMENT;
                lkAssignObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                lkAssignObject.DataContext = depart;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void orgClient_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                OrganizationWS.T_HR_COMPANY company = e.Result as OrganizationWS.T_HR_COMPANY;
                lkAssignObject.DisplayMemberPath = "CNAME";
                lkAssignObject.DataContext = company;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void client_GetEmployeeAddSumBatchByIDCompleted(object sender, GetEmployeeAddSumBatchByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                EmployeeAddSumBatch = new T_HR_EMPLOYEEADDSUMBATCH();
                if (e.Result != null)
                {
                    EmployeeAddSumBatch = e.Result as T_HR_EMPLOYEEADDSUMBATCH;
                    txtBalanceYear.Text = EmployeeAddSumBatch.BALANCEYEAR.ToString();
                    nudBalanceMonth.Value = Convert.ToDouble(EmployeeAddSumBatch.BALANCEMONTH.ToString());
                    CheckState = EmployeeAddSumBatch.CHECKSTATE;
                    cbxkAssignedObjectType.SelectedIndex = EmployeeAddSumBatch.BALANCEOBJECTTYPE.ToInt32();
                    ObjectType = EmployeeAddSumBatch.BALANCEOBJECTTYPE.ToInt32();
                    ObjectValue = EmployeeAddSumBatch.BALANCEOBJECTID;
                    BindAssignObjectLookup();

                    string filter = "";
                    int pageCount = 0;
                    DateTime? starttimes = new DateTime(Convert.ToInt32(EmployeeAddSumBatch.BALANCEYEAR), Convert.ToInt32(EmployeeAddSumBatch.BALANCEMONTH), 1);
                    DateTime? endtimes = new DateTime(Convert.ToInt32(EmployeeAddSumBatch.BALANCEYEAR), Convert.ToInt32(EmployeeAddSumBatch.BALANCEMONTH), DateTime.DaysInMonth(Convert.ToInt32(EmployeeAddSumBatch.BALANCEYEAR), Convert.ToInt32(EmployeeAddSumBatch.BALANCEMONTH)));
                    System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

                    filter += " T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID==@" + paras.Count().ToString();
                    paras.Add(EmployeeAddSumBatch.MONTHLYBATCHID);

                    client.GetEmployeeAddSumAuditPagingAsync(dataPager.PageIndex, dataPager.PageSize, "ADDSUMID", filter, paras, pageCount, Convert.ToDateTime(starttimes), Convert.ToDateTime(endtimes), userID, EmployeeAddSumBatch.CHECKSTATE, Convert.ToInt32(EmployeeAddSumBatch.BALANCEOBJECTTYPE), EmployeeAddSumBatch.BALANCEOBJECTID);
                    this.DataContext = EmployeeAddSumBatch;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                }

            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
        }

        void client_GetEmployeeAddSumAuditPagingCompleted(object sender, GetEmployeeAddSumAuditPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                listDetail = e.Result.ToList();
                dgMassAuditList.ItemsSource = e.Result;
                SetProjectMoneySum();
                dataPager.PageCount = e.pageCount;
                if (e.UserState != null)
                {
                    if (e.UserState.ToString() == "New")
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                    }
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        private void BindAssignObjectLookup()
        {
            if (ObjectValue == null || ObjectType == -1)
            {
                lkAssignObject.DataContext = null;
                return;
            }
            switch (ObjectType + 1)
            {
                case 1:
                    orgClient.GetCompanyByIdAsync(ObjectValue);
                    break;

                case 2:
                    orgClient.GetDepartmentByIdAsync(ObjectValue);
                    break;

                case 3:
                    orgClient.GetPostByIdAsync(ObjectValue);
                    break;

                case 4:
                    OrganizationWS.T_HR_EMPLOYEE employee = new OrganizationWS.T_HR_EMPLOYEE();
                    employee.EMPLOYEEID = ObjectValue;
                    //employee.EMPLOYEECNAME = "";
                    lkAssignObject.DisplayMemberPath = "EMPLOYEECNAME";
                    lkAssignObject.DataContext = employee;
                    break;

                default:
                    lkAssignObject.DataContext = null;
                    break;
            }

        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("员工加扣款批量审核 - 注:批量审核只能针对同一个月内的进行处理");
        }

        public string GetStatus()
        {
            return string.Empty;
        }

        public void DoAction(string actionType)
        {
            return;
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
            return toolbarItems;
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

        private void SetToolBar()
        {
            if (FormType == FormTypes.Audit)
                toolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEADDSUMBATCH", EmployeeAddSumBatch.OWNERID,
                               EmployeeAddSumBatch.OWNERPOSTID, EmployeeAddSumBatch.OWNERDEPARTMENTID, EmployeeAddSumBatch.OWNERCOMPANYID);
            RefreshUI(RefreshedTypes.All);
        }

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEEADDSUMBATCH Info)
        {
            Dictionary<string, string> systype = new Dictionary<string, string>();
            systype.Add("0", "员工加扣款");
            systype.Add("1", "员工代扣款");
            //systype.Add("2", "绩效奖金");
            //systype.Add("3", "其他......");

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

            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY AssignType = cbxkAssignedObjectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "BALANCEOBJECTTYPE", Info.BALANCEOBJECTTYPE, AssignType != null ? AssignType.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "BALANCEOBJECTID", Info.BALANCEOBJECTID, lkAssignObject.TxtLookUp.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUMBATCH", "BALANCESHORTDATE", Info.BALANCEYEAR + " - " + Info.BALANCEMONTH, Info.BALANCEYEAR + " - " + Info.BALANCEMONTH));//新加字段发薪年月
            foreach (var v in listDetail)
            {
                AutoList.Add(basedataForChild("T_HR_EMPLOYEEADDSUM", "SYSTEMTYPE", v.SYSTEMTYPE, systype[v.SYSTEMTYPE.ToString()], v.ADDSUMID));
                AutoList.Add(basedataForChild("T_HR_EMPLOYEEADDSUM", "CHECKSTATE", "1", checkState, v.ADDSUMID));
                AutoList.Add(basedataForChild("T_HR_EMPLOYEEADDSUM", "DEALDATE",  v.DEALYEAR + " - " + v.DEALMONTH, v.DEALYEAR + " - " + v.DEALMONTH, v.ADDSUMID));//新加字典加扣款年月
            }
            string a = mx.TableToXml(Info, listDetail, StrSource, AutoList);

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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            #region 批量审核

            Dictionary<string, string> para2 = new Dictionary<string, string>();
            para2.Add("SYSTEMTYPE", (cbxkAssignedObjectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxkAssignedObjectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            // strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEADDSUMBATCH>(EmployeeAddSumBatch, null, "HR", para2, null);
            //try
            //{
            //    //为了手机显示xml文件在这里转化一下，不然传过去的值为数字，手机那面没有判断再显示相应类型 0员工加扣款，1员工代扣款，2绩效奖金，3其他......
            //    string str0 = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PROTECTTYPE" && s.DICTIONARYVALUE == 0).FirstOrDefault().DICTIONARYNAME;
            //    string str1 = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PROTECTTYPE" && s.DICTIONARYVALUE == 1).FirstOrDefault().DICTIONARYNAME;
            //    string str2 = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PROTECTTYPE" && s.DICTIONARYVALUE == 2).FirstOrDefault().DICTIONARYNAME;
            //    string str3 = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "PROTECTTYPE" && s.DICTIONARYVALUE == 3).FirstOrDefault().DICTIONARYNAME;
            //    if (EmployeeAddSumBatch != null && EmployeeAddSumBatch.T_HR_EMPLOYEEADDSUM != null)
            //    {
            //        List<T_HR_EMPLOYEEADDSUM> addSum = EmployeeAddSumBatch.T_HR_EMPLOYEEADDSUM.ToList();
            //        EmployeeAddSumBatch.T_HR_EMPLOYEEADDSUM.ToList().ForEach(
            //            item =>
            //            {
            //                switch (item.SYSTEMTYPE)
            //                {
            //                    case "0":
            //                        {
            //                            if (!string.IsNullOrEmpty(str0))
            //                                item.SYSTEMTYPE = str0;
            //                            else
            //                                item.SYSTEMTYPE = "员工加扣款";
            //                        }; break;
            //                    case "1":
            //                        {
            //                            if (!string.IsNullOrEmpty(str1))
            //                                item.SYSTEMTYPE = str1;
            //                            else
            //                                item.SYSTEMTYPE = "员工代扣款";
            //                        }; break;
            //                    case "2":
            //                        {
            //                            if (!string.IsNullOrEmpty(str2))
            //                                item.SYSTEMTYPE = str2;
            //                            else
            //                                item.SYSTEMTYPE = "绩效奖金";
            //                        }; break;
            //                    case "3":
            //                        {
            //                            if (!string.IsNullOrEmpty(str3))
            //                                item.SYSTEMTYPE = str3;
            //                            else
            //                                item.SYSTEMTYPE = "其他......";
            //                        }; break;
            //                    default: break;
            //                }
            //            }
            //                );
            //    }
            //}
            //catch (Exception ex)
            //{
            //    //无
            //}
            //finally
            //{
            //     if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
            //    strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeAddSumBatch);
            //}
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeAddSumBatch);
            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeAddSumBatch.CREATEUSERID);
            paraIDs.Add("CreatePostID", EmployeeAddSumBatch.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeAddSumBatch.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeAddSumBatch.OWNERCOMPANYID);

            if (EmployeeAddSumBatch.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEADDSUMBATCH", EmployeeAddSumBatch.MONTHLYBATCHID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEADDSUMBATCH", EmployeeAddSumBatch.MONTHLYBATCHID, strXmlObjectSource);
            }
            #endregion
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string strCheckState = "";
            string oldCheckState = EmployeeAddSumBatch.CHECKSTATE;
            string strEditState = Convert.ToInt32(EditStates.UnActived).ToString();
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    strCheckState = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    strCheckState = Utility.GetCheckState(CheckStates.Approved);
                    strEditState = Convert.ToInt32(EditStates.Actived).ToString();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    strCheckState = Utility.GetCheckState(CheckStates.UnApproved);
                    strEditState = Convert.ToInt32(EditStates.Canceled).ToString();
                    break;
            }
            EmployeeAddSumBatch.EDITSTATE = strEditState;
            EmployeeAddSumBatch.CHECKSTATE = strCheckState;
            //System.Collections.ObjectModel.ObservableCollection<string> ids = new System.Collections.ObjectModel.ObservableCollection<string>();
            //foreach (var e in dgMassAuditList.ItemsSource)
            //{
            //    ids.Add((e as T_HR_EMPLOYEEADDSUM).ADDSUMID);
            //}
            //if (ids.Count > 0)
            //{
            //    if (oldCheckState != Utility.GetCheckState(CheckStates.UnSubmit))
            //    {
            //        client.EmployeeAddSumBatchUpdateAsync(EmployeeAddSumBatch);//审核
            //    }
            //    else
            //    {
            //        client.EmployeeAddSumBatchAddAsync(EmployeeAddSumBatch, ids);//提交审核
            //    }
            //}
            //if (e.Error != null)
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            //}
            //else
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            //    RefreshUI(RefreshedTypes.All);
            //}
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            RefreshUI(RefreshedTypes.HideProgressBar);

        }

        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeAddSumBatch != null)
                state = EmployeeAddSumBatch.CHECKSTATE;
            return state;
        }

        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            client.DoClose();
        }
        public bool CheckDataContenxChange()
        {
            return true;
        }
        public void SetOldEntity(object entity)
        {

        }
        #endregion

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dgMassAuditList_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBlock tborder = dgMassAuditList.Columns[1].GetCellContent(e.Row).FindName("tbOrder") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
        }

        private void lkAssignObject_FindClick(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 计算项目金额总和
        /// </summary>
        private void SetProjectMoneySum()
        {
            //计算汇总金额
            decimal sumMoney = 0;
            foreach (var obj in dgMassAuditList.ItemsSource)
            {
                var addsumView = obj as T_HR_EMPLOYEEADDSUM;
                sumMoney += addsumView.PROJECTMONEY == null ? 0 : addsumView.PROJECTMONEY.Value;
            }
            txtProjectMoneySum.Text = sumMoney.ToString();
        }
    }
}
