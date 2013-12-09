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
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
using System.Text.RegularExpressions;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class EmployeeAddSumForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public V_EmployeeAddsumView EmployeeAddSumView { get; set; }
        public T_HR_EMPLOYEEADDSUM EmployeeAddSum { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private ObservableCollection<V_EmployeeAddsumView> EmployeeAddsumInfoList = new ObservableCollection<V_EmployeeAddsumView>();
        private ObservableCollection<V_EmployeeAddsumView> EmployeeAddsumDelList = new ObservableCollection<V_EmployeeAddsumView>();
        private string addSumID;
        private bool isNew;//true 从快捷方式新建单据
        public EmployeeAddSumForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            isNew = true;
            addSumID = string.Empty;
            this.Loaded += new RoutedEventHandler(EmployeeAddSumForm_Loaded);
        }

        public EmployeeAddSumForm(FormTypes type, string AddSumID)
        {
            InitializeComponent();
            FormType = type;
            addSumID = AddSumID;
            this.Loaded += new RoutedEventHandler(EmployeeAddSumForm_Loaded);
        }

        void EmployeeAddSumForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            if (string.IsNullOrEmpty(addSumID))
            {
                lkEmployeeName.IsEnabled = true;
                EmployeeAddSum = new T_HR_EMPLOYEEADDSUM();
                EmployeeAddSum.ADDSUMID = Guid.NewGuid().ToString();
                EmployeeAddSum.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeAddSum.CREATEDATE = System.DateTime.Now;

                EmployeeAddSum.UPDATEDATE = System.DateTime.Now;
                EmployeeAddSum.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeAddSum.DEALYEAR = System.DateTime.Now.Year.ToString();
                EmployeeAddSum.DEALMONTH = System.DateTime.Now.Month.ToString();
                EmployeeAddSum.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                EmployeeAddSum.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                EmployeeAddSum.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                EmployeeAddSum.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                EmployeeAddSum.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                EmployeeAddSum.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                EmployeeAddSum.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                EmployeeAddSum.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                this.DataContext = EmployeeAddSum;
                SetToolBar();
            }
            else
            {
                NotShow(FormType);
                //client.GetEmployeeAddSumByIDAsync(addSumID);
                client.GetEmployeeAddSumViewByIDAsync(addSumID);
            }
        }

        private void NotShow(FormTypes type)
        {
            if (type != FormTypes.Edit && type != FormTypes.Resubmit)
            {
                lkEmployeeName.IsEnabled = false;
                combProtectType.IsEnabled = false;
                numYear.IsEnabled = false;
                numMonth.IsEnabled = false;
                DtGrid.IsReadOnly = true;
            }
        }

        private void InitParas()
        {
            //client.GetEmployeeAddSumByIDCompleted += new EventHandler<GetEmployeeAddSumByIDCompletedEventArgs>(client_GetEmployeeAddSumByIDCompleted);
            client.GetEmployeeAddSumViewByIDCompleted += new EventHandler<GetEmployeeAddSumViewByIDCompletedEventArgs>(client_GetEmployeeAddSumViewByIDCompleted);

            client.EmployeeAddSumUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeAddSumUpdateCompleted);
            client.EmployeeAddSumADDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_EmployeeAddSumADDCompleted);
            client.EmployeeAddSumLotsofADDCompleted += new EventHandler<EmployeeAddSumLotsofADDCompletedEventArgs>(client_EmployeeAddSumLotsofADDCompleted);
        }

        void client_GetEmployeeAddSumViewByIDCompleted(object sender, GetEmployeeAddSumViewByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                EmployeeAddSumView = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    EmployeeAddSumView.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }
                numMonth.Value = Convert.ToDouble(EmployeeAddSumView.DEALMONTH);
                numYear.Value = Convert.ToDouble(EmployeeAddSumView.DEALYEAR);
                foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in combProtectType.ItemsSource)
                {
                    if (item.DICTIONARYVALUE.ToString() == EmployeeAddSumView.SYSTEMTYPE)
                    {
                        combProtectType.SelectedItem = item;
                    }
                }
                //PROJECTNAME传递的是员工全名
                EmployeeAddSumView.PROJECTNAME = EmployeeAddSumView.EMPLOYEENAME + "-" + EmployeeAddSumView.DepartmentName + "-" + EmployeeAddSumView.CompanyName;
                EmployeeAddsumInfoList.Add(EmployeeAddSumView);
                initAddSum();
                DtGrid.ItemsSource = EmployeeAddsumInfoList;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
                SetProjectMoneySum();
            }
        }

        void client_EmployeeAddSumLotsofADDCompleted(object sender, EmployeeAddSumLotsofADDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //if (e.Result)
                //{
                // FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(RefreshedTypes.All);
                //}
                //else
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ADDDATAFAILED", "EMPLOYEEADDSUM"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //}
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_EmployeeAddSumADDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_EmployeeAddSumUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() == "Audit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }


        /// <summary>
        /// 获取加扣款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void client_GetEmployeeAddSumByIDCompleted(object sender, GetEmployeeAddSumByIDCompletedEventArgs e)
        //{
        //    if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result == null)
        //        {
        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //            return;
        //        }
        //        EmployeeAddSum = e.Result;
        //        initAddSum();
        //        RefreshUI(RefreshedTypes.AuditInfo);
        //        SetToolBar();
        //    }
        //}


        private void initAddSum()
        {
            EmployeeAddSum = new T_HR_EMPLOYEEADDSUM();
            EmployeeAddSum.ADDSUMID = EmployeeAddSumView.ADDSUMID;
            EmployeeAddSum.CREATECOMPANYID = EmployeeAddSumView.CREATECOMPANYID;
            EmployeeAddSum.CREATEDEPARTMENTID = EmployeeAddSumView.CREATEDEPARTMENTID;
            EmployeeAddSum.CREATEPOSTID = EmployeeAddSumView.CREATEPOSTID;
            EmployeeAddSum.CREATEUSERID = EmployeeAddSumView.CREATEUSERID;
            EmployeeAddSum.CREATEDATE = EmployeeAddSumView.CREATEDATE;
            EmployeeAddSum.CHECKSTATE = EmployeeAddSumView.CHECKSTATE;
            EmployeeAddSum.DEALMONTH = EmployeeAddSumView.DEALMONTH;
            EmployeeAddSum.DEALYEAR = EmployeeAddSumView.DEALYEAR;
            EmployeeAddSum.EMPLOYEECODE = EmployeeAddSumView.EMPLOYEECODE;
            EmployeeAddSum.EMPLOYEEID = EmployeeAddSumView.EMPLOYEEID;
            EmployeeAddSum.EMPLOYEENAME = EmployeeAddSumView.EMPLOYEENAME;
            EmployeeAddSum.OWNERCOMPANYID = EmployeeAddSumView.OWNERCOMPANYID;
            EmployeeAddSum.OWNERDEPARTMENTID = EmployeeAddSumView.OWNERDEPARTMENTID;
            EmployeeAddSum.OWNERPOSTID = EmployeeAddSumView.OWNERPOSTID;
            EmployeeAddSum.OWNERID = EmployeeAddSumView.OWNERID;
            EmployeeAddSum.PROJECTMONEY = EmployeeAddSumView.PROJECTMONEY;
            //去掉PROJECTNAME，先用该字段显示员工姓名（形式：姓名-部门-公司）
            EmployeeAddSum.PROJECTNAME = EmployeeAddSumView.EMPLOYEENAME + "-" + EmployeeAddSumView.DepartmentName + "-" + EmployeeAddSumView.CompanyName;
            //加扣款类型，this.DataContext绑定的值为EmployeeAddSum不是EmployeeAddSumView，所以EmployeeAddSumView值不会改变
            EmployeeAddSumView.SYSTEMTYPE = Convert.ToString(combProtectType.SelectedIndex);
            EmployeeAddSum.SYSTEMTYPE = EmployeeAddSumView.SYSTEMTYPE;
            EmployeeAddSum.REMARK = EmployeeAddSumView.REMARK;
            EmployeeAddSum.UPDATEDATE = EmployeeAddSumView.UPDATEDATE;
            EmployeeAddSum.UPDATEUSERID = EmployeeAddSumView.UPDATEUSERID;
            EmployeeAddSum.PROJECTCODE = EmployeeAddSumView.PROJECTCODE;
            if (!string.IsNullOrEmpty(EmployeeAddSumView.MONTHLYBATCHID))
            {
                EmployeeAddSum.T_HR_EMPLOYEEADDSUMBATCH = new T_HR_EMPLOYEEADDSUMBATCH();
                EmployeeAddSum.T_HR_EMPLOYEEADDSUMBATCH.MONTHLYBATCHID = EmployeeAddSumView.MONTHLYBATCHID;
            }
            this.DataContext = EmployeeAddSum;
        }
        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEEADDSUM", EmployeeAddSum.OWNERID,
                    EmployeeAddSum.OWNERPOSTID, EmployeeAddSum.OWNERDEPARTMENTID, EmployeeAddSum.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {

                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEADDSUM", EmployeeAddSum.OWNERID,
                    EmployeeAddSum.OWNERPOSTID, EmployeeAddSum.OWNERDEPARTMENTID, EmployeeAddSum.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEEADDSUM");
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
                    Cancel();
                    break;
            }

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
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            //NavigateItem item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("BASEINFO"),
            //    Tooltip = Utility.GetResourceStr("BASEINFO")
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Tooltip = Utility.GetResourceStr("SALARYARCHIVE"),
            //    Url = "/Salary/SalaryArchive"
            //};
            //items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            if (isNew)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEEADDSUM Info)
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

            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY SYSTEMTYPE = combProtectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "SYSTEMTYPE", SYSTEMTYPE != null ? SYSTEMTYPE.DICTIONARYVALUE.ToString() : "0", SYSTEMTYPE != null ? SYSTEMTYPE.DICTIONARYNAME : ""));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "EMPLOYEEID", Info.EMPLOYEEID, Info.EMPLOYEEID));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "EMPLOYEENAME", Info.EMPLOYEENAME, ownerCompanyName + "-" + ownerDepartmentName + "-" + ownerPostName + "-" + Info.EMPLOYEENAME));

            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEEADDSUM", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
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
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEADDSUM>(EmployeeAddSum, "HR");
            //Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEADDSUM", EmployeeAddSum.ADDSUMID, strXmlObjectSource);

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", EmployeeAddSum.EMPLOYEENAME);
            para.Add("EMPLOYEEID", EmployeeAddSum.EMPLOYEEID);


            Dictionary<string, string> para2 = new Dictionary<string, string>();
            para2.Add("SYSTEMTYPE", (combProtectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (combProtectType.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;

            //  strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEADDSUM>(EmployeeAddSum, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, EmployeeAddSum);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", EmployeeAddSum.CREATEUSERID);
            paraIDs.Add("CreatePostID", EmployeeAddSum.CREATEPOSTID);
            paraIDs.Add("CreateDepartmentID", EmployeeAddSum.CREATEDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", EmployeeAddSum.CREATECOMPANYID);

            if (EmployeeAddSum.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEADDSUM", EmployeeAddSum.ADDSUMID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEADDSUM", EmployeeAddSum.ADDSUMID, strXmlObjectSource);
            }

        }
        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            //RefreshUI(RefreshedTypes.ShowProgressBar);
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
            if (EmployeeAddSum.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            EmployeeAddSum.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);//刷新审核控件
            RefreshUI(RefreshedTypes.All);//刷新管理列表界面
            // client.EmployeeAddSumUpdateAsync(EmployeeAddSum);
            //Utility.UpdateCheckState("T_HR_EMPLOYEEADDSUM", "ADDSUMID", EmployeeAddSum.ADDSUMID, args);

        }

        public string GetAuditState()
        {
            string state = "-1";
            if (EmployeeAddSum != null)
                state = EmployeeAddSum.CHECKSTATE;
            return state;
        }
        #endregion
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns>保存结果</returns>
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (EmployeeAddsumInfoList.Count == 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MUSTSEPARATOR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            if (combProtectType.SelectedIndex == -1)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("处理类型不能为空"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }

            if (FormType == FormTypes.New)
            {

                System.Collections.ObjectModel.ObservableCollection<T_HR_EMPLOYEEADDSUM> eaddsum = new System.Collections.ObjectModel.ObservableCollection<T_HR_EMPLOYEEADDSUM>();
                //
                List<V_EmployeeAddsumView> infoList = new List<V_EmployeeAddsumView>();
                infoList.AddRange(EmployeeAddsumInfoList);
                //infoList.AddRange(EmployeeAddsumDelList);

                foreach (var admSum in infoList)
                {
                    if (admSum.PROJECTMONEY == 0 || isZore())
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("输入的金额不能为0"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        RefreshUI(RefreshedTypes.ProgressBar);
                        return false;
                    }
                    if (admSum.PROJECTMONEY == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("请输入正确的金额"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        RefreshUI(RefreshedTypes.ProgressBar);
                        return false;
                    }


                    T_HR_EMPLOYEEADDSUM EmployeeAddSumNew = new T_HR_EMPLOYEEADDSUM();
                    //EmployeeAddSumNew.PROJECTNAME = admSum.PROJECTNAME;
                    EmployeeAddSumNew.PROJECTMONEY = admSum.PROJECTMONEY;
                    EmployeeAddSumNew.SYSTEMTYPE = combProtectType.SelectedIndex.ToString();
                    EmployeeAddSumNew.DEALYEAR = numYear.Value.ToString();
                    EmployeeAddSumNew.DEALMONTH = numMonth.Value.ToString();
                    EmployeeAddSumNew.ADDSUMID = admSum.ADDSUMID;
                    EmployeeAddSumNew.EMPLOYEEID = admSum.EMPLOYEEID;
                    EmployeeAddSumNew.EMPLOYEECODE = admSum.EMPLOYEECODE;
                    EmployeeAddSumNew.EMPLOYEENAME = admSum.EMPLOYEENAME;
                    EmployeeAddSumNew.CREATEDATE = System.DateTime.Now;
                    EmployeeAddSumNew.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    EmployeeAddSumNew.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    EmployeeAddSumNew.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    EmployeeAddSumNew.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    EmployeeAddSumNew.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    EmployeeAddSumNew.OWNERCOMPANYID = admSum.OWNERCOMPANYID;
                    EmployeeAddSumNew.OWNERDEPARTMENTID = admSum.OWNERDEPARTMENTID;
                    EmployeeAddSumNew.OWNERID = admSum.OWNERID;
                    EmployeeAddSumNew.OWNERPOSTID = admSum.OWNERPOSTID;
                    EmployeeAddSumNew.PROJECTCODE = admSum.PROJECTCODE;
                    EmployeeAddSumNew.REMARK = admSum.REMARK;

                    eaddsum.Add(EmployeeAddSumNew);
                }
                //只提交第一个
                EmployeeAddSum = eaddsum.FirstOrDefault();
                client.EmployeeAddSumLotsofADDAsync(eaddsum);


            }
            else
            {

                initAddSum();
                if (EmployeeAddSum.PROJECTMONEY == 0 || EmployeeAddSum.PROJECTMONEY == null || isZore())
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("加扣款金额不能为0"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
                }
                EmployeeAddSum.UPDATEDATE = System.DateTime.Now;
                EmployeeAddSum.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.EmployeeAddSumUpdateAsync(EmployeeAddSum, "Edit");
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

            if (flag)
            {
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }



        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 选择人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick(object sender, EventArgs e)
        {

            #region----多选人员
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.MultiSelected = true;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ents = lookup.SelectedObj as List<ExtOrgObj>;

                foreach (var employeeInfo in ents)
                {
                    V_EmployeeAddsumView addSumInfo = new V_EmployeeAddsumView();
                    //人员重复跳过
                    var tmp = EmployeeAddsumInfoList.Where(s => s.EMPLOYEEID == employeeInfo.ObjectID).FirstOrDefault();
                    if (tmp != null)
                    {
                        continue;
                    }

                    //岗位
                    ExtOrgObj post = (ExtOrgObj)employeeInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;

                    //部门
                    ExtOrgObj dept = (ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string depName = dept.ObjectName;

                    //公司
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string companyName = corp.CNAME;

                    //员工信息
                    var temp = employeeInfo.ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                    addSumInfo.ADDSUMID = Guid.NewGuid().ToString();
                    addSumInfo.CompanyName = companyName;
                    addSumInfo.PostName = postName;
                    addSumInfo.DepartmentName = depName;
                    addSumInfo.EMPLOYEECODE = temp.EMPLOYEECODE;
                    addSumInfo.EMPLOYEEID = temp.EMPLOYEEID;
                    addSumInfo.EMPLOYEENAME = temp.EMPLOYEECNAME;
                    addSumInfo.OWNERID = temp.EMPLOYEEID;
                    addSumInfo.OWNERCOMPANYID = corpid;
                    addSumInfo.OWNERDEPARTMENTID = deptid;
                    addSumInfo.OWNERPOSTID = postid;
                    //PROJECTNAME用于显示员工全名
                    addSumInfo.PROJECTNAME = temp.EMPLOYEECNAME + "-" + depName + "-" + companyName;
                    EmployeeAddsumInfoList.Add(addSumInfo);
                }

                DtGrid.ItemsSource = EmployeeAddsumInfoList;
                SetProjectMoneySum();
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            #endregion

        }

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

        /// <summary>
        /// 选择离职人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btHistory_Click(object sender, RoutedEventArgs e)
        {
            Form.Salary.ResignForm form = new SMT.HRM.UI.Form.Salary.ResignForm();
            EntityBrowser browser = new EntityBrowser(form);
            form.SaveClicked += (obj, ev) =>
            {
                if (form.SelectedEmployees != null)
                {
                    foreach (var ent in form.SelectedEmployees)
                    {
                        var tmp = EmployeeAddsumInfoList.Where(s => s.EMPLOYEEID == ent.EMPLOYEEID).FirstOrDefault();
                        if (tmp != null)
                        {
                            continue;
                        }

                        V_EmployeeAddsumView addSumInfo = new V_EmployeeAddsumView();
                        addSumInfo.ADDSUMID = Guid.NewGuid().ToString();
                        addSumInfo.CompanyName = ent.CompanyName;
                        addSumInfo.PostName = ent.PostName;
                        addSumInfo.DepartmentName = ent.DepartmentName;
                        addSumInfo.EMPLOYEECODE = ent.EMPLOYEECODE;
                        addSumInfo.EMPLOYEEID = ent.EMPLOYEEID;
                        addSumInfo.EMPLOYEENAME = ent.EMPLOYEENAME;
                        addSumInfo.OWNERID = ent.EMPLOYEEID;
                        addSumInfo.OWNERCOMPANYID = ent.OWNERCOMPANYID;
                        addSumInfo.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
                        addSumInfo.OWNERPOSTID = ent.OWNERPOSTID;
                        //去掉PROJECTNAME，先用该字段显示员工姓名（形式：姓名-部门-公司）
                        addSumInfo.PROJECTNAME = ent.EMPLOYEENAME + "-" + ent.DepartmentName + "-" + ent.CompanyName;
                        EmployeeAddsumInfoList.Add(addSumInfo);
                    }
                    DtGrid.ItemsSource = EmployeeAddsumInfoList;
                    SetProjectMoneySum();
                }
            };
            //  form.MinWidth = 450;
            form.Height = 400;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            var ent = DtGrid.SelectedItems[0] as V_EmployeeAddsumView;
            EmployeeAddsumInfoList.Remove(ent);
            //  DtGrid.ItemsSource = EmployeeAddsumInfoList;
            if (!EmployeeAddsumDelList.Contains(ent))
            {
                ent.PROJECTCODE = "del";
                EmployeeAddsumDelList.Add(ent);
            }
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            TextBlock tborder = DtGrid.Columns[0].GetCellContent(e.Row).FindName("tbOrder") as TextBlock;
            if (tborder != null)
            {
                tborder.Text = (e.Row.GetIndex() + 1).ToString();
            }
            Button btnDel = DtGrid.Columns[8].GetCellContent(e.Row).FindName("btnDel") as Button;

            SMT.Saas.Tools.SalaryWS.T_HR_EMPLOYEEADDSUM entTemp = this.DataContext as SMT.Saas.Tools.SalaryWS.T_HR_EMPLOYEEADDSUM;

            if (entTemp != null)
            {
                if (entTemp.CHECKSTATE != "0")
                {
                    if (btnDel != null)
                    {
                        btnDel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// 2012-9-10
        /// 项目金额判断，只有数字才能输入,判断写得不好，可以改正
        /// </summary>
        //string param = "";
        //private void txtProjectMoney_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    string pattern = @"^(-)?(([1-9]+[0-9]*.{1}[0-9]+)|([0].{1}[1-9]+[0-9]*)|([1-9][0-9]*)|([0][.][0-9]+[1-9]*))$";
        //    TextBox txt = sender as TextBox;
        //    Match mText = Regex.Match(txt.Text, pattern);   // 匹配正则表达式

        //    int i = txt.Text.Split('.').Length - 1;//获取有几个小数点

        //    if (txt.Text == "")
        //    {
        //        param = "";
        //    }
        //    else if (txt.Text[0] == '.')
        //    {
        //        param = "";
        //        txt.Text = "";
        //    }
        //    else if (i <= 1)
        //    {
        //        if ((mText.Success || txt.Text[txt.Text.Length - 1] == '.'))
        //        {
        //            param = txt.Text;   // 将现在textBox的值保存下来
        //        }
        //        if (txt.Text[0] == '-')
        //        {

        //            Decimal val;
        //            if (txt.Text.Length >= 2 && Decimal.TryParse(txt.Text, out val))
        //            {

        //                param = txt.Text;   // 将现在textBox的值保存下来
        //            }
        //            else
        //            {
        //                if (txt.Text.Length <= 2)
        //                {
        //                    txt.Text = "-";
        //                    txt.SelectionStart = txt.Text.Length;// 将光标定位到文本框的最后
        //                }
        //                else
        //                {
        //                    txt.Text = param;
        //                    txt.SelectionStart = txt.Text.Length;// 将光标定位到文本框的最后
        //                }

        //            }

        //        }
        //        else
        //        {
        //            txt.Text = param;   // textBox内容不变
        //            txt.SelectionStart = txt.Text.Length;// 将光标定位到文本框的最后
        //        }
        //    }
        //    else if (i > 1)
        //    {
        //        txt.Text = param;
        //        txt.SelectionStart = txt.Text.Length;// 将光标定位到文本框的最后
        //    }
        //}



        /// <summary>
        /// 2012-9-22
        /// 当离开文本框且什么都没输入时，向param存入inputIsNull
        /// 然后在保存时判断,貌似没用..只能判断一次，再想办法
        /// 修改2012-12-6，以前不行，注释掉
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtProjectMoney_LostFocus(object sender, RoutedEventArgs e)
        {
            //TextBox txt = sender as TextBox;
            //if (param == "")
            //{
            //    param = "inputIsNull";
            //}
            TextBox txt = sender as TextBox;
            if (string.IsNullOrEmpty(txt.Text))
            {
                txt.Text = "0";
            }
            else if (!isNumber(txt.Text))
            {
                txt.Text = "0";
            }
            LostFocusProjectMoneySum();
        }



        /// <summary>
        /// 比较传入的值是不是数字
        /// </summary>
        /// <param name="str">输入框的值</param>
        /// <returns>是数字返回true，否则返回false</returns>
        private bool isNumber(string str)
        {
            bool flag = true;
            string pattern = @"^(-)?(([1-9]+[0-9]*.{1}[0-9]+)|([0].{1}[1-9]+[0-9]*)|([1-9][0-9]*)|([0][.][0-9]+[1-9]*))$";
            Match mText = Regex.Match(str, pattern);   // 匹配正则表达式
            if (!mText.Success)
            {
                flag = false;
            }
            return flag;
        }


        /// <summary>
        /// 判断是否为0
        /// </summary>
        /// <returns>判断是否为0</returns>
        private bool isZore()
        {
            bool flag = false;
            foreach (object obj in DtGrid.ItemsSource)
            {
                TextBox txtMark = DtGrid.Columns[6].GetCellContent(obj).FindName("txtProjectMoney") as TextBox;
                if (decimal.Parse(txtMark.Text) == 0)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }



        /// <summary>
        /// 计算项目金额总和
        /// </summary>
        private void SetProjectMoneySum()
        {
            //计算汇总金额
            decimal sumMoney = 0;
            
            foreach (var obj in DtGrid.ItemsSource)
            {
                var addsumView = obj as V_EmployeeAddsumView;
                sumMoney += addsumView.PROJECTMONEY == null ? 0 : addsumView.PROJECTMONEY.Value;
            }
            txtProjectMoneySum.Text = sumMoney.ToString();
        }

        /// <summary>
        /// 计算项目金额总和
        /// </summary>
        private void LostFocusProjectMoneySum()
        {
            //计算汇总金额
            decimal sumMoney = 0;

            foreach (var obj in DtGrid.ItemsSource)
            {
                TextBox txtMoney = DtGrid.Columns[6].GetCellContent(obj).FindName("txtProjectMoney") as TextBox;
                sumMoney += Decimal.Parse(txtMoney.Text);
            }
            txtProjectMoneySum.Text = sumMoney.ToString();
        }
    }

}
