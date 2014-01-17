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
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalarySolutionForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        public T_HR_SALARYSOLUTION SalarySolution { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool auditsign = false;
        public string SalarySolutionID { get; set; }
        private bool isNew;//true 从快捷方式新建单据

        public SalarySolutionForm()
        {
            InitializeComponent();
            FormType = FormTypes.New;
            isNew = true;
            SalarySolutionID = string.Empty;
            this.Loaded += new RoutedEventHandler(SalarySolutionForm_Loaded);
        }
        public SalarySolutionForm(FormTypes type, string solutionID)
        {
            InitializeComponent();
            //InitParas();
            FormType = type;
            SalarySolutionID = solutionID;
            this.Loaded += new RoutedEventHandler(SalarySolutionForm_Loaded);
            //if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            //{
            //    EnableControl();
            //}
            //if (string.IsNullOrEmpty(solutionID))
            //{
            //    SalarySolution = new T_HR_SALARYSOLUTION();
            //    SalarySolution.SALARYSOLUTIONID = Guid.NewGuid().ToString();
            //    SalarySolution.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    SalarySolution.CREATEDATE = System.DateTime.Now;

            //    SalarySolution.UPDATEDATE = System.DateTime.Now;
            //    SalarySolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //    SalarySolution.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            //    SalarySolution.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            //    SalarySolution.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    SalarySolution.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            //    SalarySolution.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
            //    this.DataContext = SalarySolution;
            //    SolutionItemWinForm.SAVEID = SalarySolution.SALARYSOLUTIONID;
            //    SolutionItemWinForm.FormType = FormType;
            //    SolutionItemWinForm.LoadData(SalarySolution.SALARYSOLUTIONID);
            //    SetToolBar();
            //}
            //else
            //{
            //    client.GetSalarySolutionByIDAsync(solutionID);
            //    SolutionItemWinForm.FormType = FormType;
            //    SolutionItemWinForm.LoadData(solutionID);
            //}

        }

        void SalarySolutionForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            if (FormType == FormTypes.Audit || FormType == FormTypes.Browse)
            {
                EnableControl();
            }
            if (FormType == FormTypes.New)
            {
                SolutionItemWinForm.IsEnabled = false;
            }
            if (string.IsNullOrEmpty(SalarySolutionID))
            {
                SalarySolution = new T_HR_SALARYSOLUTION();
                SalarySolution.SALARYSOLUTIONID = Guid.NewGuid().ToString();
                SalarySolution.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalarySolution.CREATEDATE = System.DateTime.Now;

                SalarySolution.UPDATEDATE = System.DateTime.Now;
                SalarySolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SalarySolution.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SalarySolution.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SalarySolution.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalarySolution.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                SalarySolution.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                this.DataContext = SalarySolution;
                SolutionItemWinForm.SAVEID = SalarySolution.SALARYSOLUTIONID;
                SolutionItemWinForm.FormType = FormType;
                SolutionItemWinForm.LoadData(SalarySolution.SALARYSOLUTIONID);
                SetToolBar();
            }
            else
            {
                client.GetSalarySolutionByIDAsync(SalarySolutionID);
                SolutionItemWinForm.FormType = FormType;
                SolutionItemWinForm.LoadData(SalarySolutionID);
            }
        }

        private void EnableControl()
        {

            txtSalarySolutionName.IsEnabled = false;
            txtBankName.IsEnabled = false;
            txtBankAccountNo.IsEnabled = false;
            numPayDay.IsEnabled = false;
            numPayAlertDay.IsEnabled = false;
            lkSalarySystem.IsEnabled = false;
            lkArea.IsEnabled = false;
            txtTaxesBasic.IsEnabled = false;
            txtTaxesRate.IsEnabled = false;
            txtRemark.IsEnabled = false;
            SolutionItemWinForm.ToolBar.IsEnabled = false;
            SalaryTaxesWinForm.ToolBar.IsEnabled = false;

        }

        private void InitParas()
        {
            client.GetSalarySolutionByIDCompleted += new EventHandler<GetSalarySolutionByIDCompletedEventArgs>(client_GetSalarySolutionByIDCompleted);

            client.SalarySolutionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySolutionUpdateCompleted);
            client.SalarySolutionAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySolutionAddCompleted);
            client.GetSalarySolutionEngineXmlCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_GetSalarySolutionEngineXmlCompleted);
            client.SalarySolutionSameSearchCompleted += new EventHandler<SalarySolutionSameSearchCompletedEventArgs>(client_SalarySolutionSameSearchCompleted);
            //client.GetSalarySolutionStandardWithPagingCompleted += new EventHandler<GetSalarySolutionStandardWithPagingCompletedEventArgs>(client_GetSalarySolutionStandardWithPagingCompleted);
            //client.SalarySolutionStandardAddCompleted += new EventHandler<SalarySolutionStandardAddCompletedEventArgs>(client_SalarySolutionStandardAddCompleted);
            //client.SalarySolutionStandardDeleteCompleted += new EventHandler<SalarySolutionStandardDeleteCompletedEventArgs>(client_SalarySolutionStandardDeleteCompleted);
        }

        void client_SalarySolutionSameSearchCompleted(object sender, SalarySolutionSameSearchCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (e.Result)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXIST"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("EXIST"));
                }
                else
                {
                    //if (SolutionItemWinForm.salarySolutionItemsList != null)
                    //{
                    //    if (SolutionItemWinForm.salarySolutionItemsList.Count() > 0)
                    //    {
                    //        SalarySolution.T_HR_SALARYSOLUTIONITEM = SolutionItemWinForm.salarySolutionItemsList;
                    //    }
                    //}

                    client.SalarySolutionAddAsync(SalarySolution);
                }
            }
        }

        void client_GetSalarySolutionEngineXmlCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                switch (rb.Name)
                {
                    case "bankpay":
                        SalarySolution.PAYTYPE = "0";
                        break;
                    case "moneypay":
                        SalarySolution.PAYTYPE = "1";
                        break;
                    case "yuan":
                        SalarySolution.SALARYPRECISION = "0";
                        break;
                    case "angle":
                        SalarySolution.SALARYPRECISION = "1";
                        break;
                    case "cent":
                        SalarySolution.SALARYPRECISION = "2";
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }
        #region
        //void client_SalarySolutionStandardAddCompleted(object sender, SalarySolutionStandardAddCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result == "SUCCESSED")
        //        {
        //            ;// Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSOLUTIONSTANDARD"));
        //        }
        //        else
        //        {
        //            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
        //        }
        //    }
        //    LoadData();
        //    RefreshUI(RefreshedTypes.All);
        //}

        //void client_GetSalarySolutionStandardWithPagingCompleted(object sender, GetSalarySolutionStandardWithPagingCompletedEventArgs e)
        //{
        //    List<T_HR_SALARYSOLUTIONSTANDARD> list = new List<T_HR_SALARYSOLUTIONSTANDARD>();
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        return;
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //        }
        //       // DtGrid.ItemsSource = list;

        //      //  dataPager.PageCount = e.pageCount;
        //    }
        //}

        //void client_SalarySolutionStandardDeleteCompleted(object sender, SalarySolutionStandardDeleteCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSOLUTIONSTANDARD"));
        //    }
        //    LoadData();
        //    RefreshUI(RefreshedTypes.All);
        //}
        #endregion

        void client_SalarySolutionAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSOLUTION"));
                SolutionItemWinForm.Save();
                FormType = FormTypes.Edit;
                SolutionItemWinForm.IsEnabled = true;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_SalarySolutionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (auditsign)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "SALARYSOLUTION"));
                    auditsign = false;
                    if (SalarySolution.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved))
                    {
                        //激活引擎
                        client.GetSalarySolutionEngineXmlAsync(SalarySolution);
                    }
                }
                else
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYSOLUTION"));
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
        }



        void client_GetSalarySolutionByIDCompleted(object sender, GetSalarySolutionByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                SalarySolution = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    SalarySolution.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }
                this.DataContext = SalarySolution;
                switch (SalarySolution.PAYTYPE)
                {
                    case "0":
                        bankpay.IsChecked = true;
                        break;
                    case "1":
                        moneypay.IsChecked = true;
                        break;
                }
                switch (SalarySolution.SALARYPRECISION)
                {
                    case "0":
                        yuan.IsChecked = true;
                        break;
                    case "1":
                        angle.IsChecked = true;
                        break;
                    case "2":
                        cent.IsChecked = true;
                        break;
                }
                if (SalarySolution.T_HR_SALARYSYSTEM != null)
                {
                    lkSalarySystem.DataContext = SalarySolution.T_HR_SALARYSYSTEM;
                }
                if (SalarySolution.T_HR_AREADIFFERENCE != null)
                {
                    lkArea.DataContext = SalarySolution.T_HR_AREADIFFERENCE;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
                //   LoadData();
                if (SalarySolution.CHECKSTATE != Convert.ToInt16(CheckStates.UnSubmit).ToString())
                {
                    //  SPToolBar.Visibility = Visibility.Collapsed;
                    EnableControl();
                }
            }
        }
        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse)
            {
                //   SPToolBar.Visibility = Visibility.Collapsed;
                return;
            }
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_SALARYSOLUTION", SalarySolution.OWNERID,
                    SalarySolution.OWNERPOSTID, SalarySolution.OWNERDEPARTMENTID, SalarySolution.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ////如果有修改权限才允许保存
                //if (PermissionHelper.GetPermissionValue("T_HR_COMPANY", Permissions.Audit, Company.OWNERID, Company.OWNERPOSTID, Company.OWNERDEPARTMENTID, Company.OWNERCOMPANYID) >= 0)
                //{
                //    ToolbarItem item = new ToolbarItem
                //    {
                //        DisplayType = ToolbarItemDisplayTypes.Image,
                //        Key = "2",
                //        Title = Utility.GetResourceStr("AUDIT"),// "审核",
                //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/18_audit.png"
                //    };

                //    ToolbarItems.Add(item);
                //}
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_SALARYSOLUTION", SalarySolution.OWNERID,
                    SalarySolution.OWNERPOSTID, SalarySolution.OWNERDEPARTMENTID, SalarySolution.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("SALARYSOLUTION") + ":" + SalarySolution.SALARYSOLUTIONNAME;
            return Utility.GetResourceStr("SALARYSOLUTION");
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
            //    Title = "详细信息",
            //    Tooltip = "详细信息"
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    //Url = "/Salary/SalaryStandard"
            //    //Url = "/Views/Salary/SalaryStandard.xaml?ID=1"
            //    Url = "/Salary/SalaryStandard.xaml"
            //};
            //items.Add(item);

            //item = new NavigateItem
            //{
            //    Title = "方案应用",
            //    Tooltip = "方案应用",
            //    Url = "/Personnel/EmployeeEntry.xaml"
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
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            // Utility.SetAuditEntity(entity, "T_HR_SALARYSOLUTION", SalarySolution.SALARYSOLUTIONID);
            string strKeyName = "SALARYSOLUTIONID";
            string strKeyValue = SalarySolution.SALARYSOLUTIONID;
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EntityKey", SalarySolution.SALARYSOLUTIONID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();
            para2.Add("T_HR_AREADIFFERENCEReference", SalarySolution.T_HR_AREADIFFERENCE == null ? "" : SalarySolution.T_HR_AREADIFFERENCE.AREADIFFERENCEID + "#" + SalarySolution.T_HR_AREADIFFERENCE.AREACATEGORY);
            para2.Add("T_HR_SALARYSYSTEMReference", SalarySolution.T_HR_SALARYSYSTEM == null ? "" : SalarySolution.T_HR_SALARYSYSTEM.SALARYSYSTEMID + "#" + SalarySolution.T_HR_SALARYSYSTEM.SALARYSYSTEMNAME);

            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_SALARYSOLUTION>(SalarySolution, para, "HR", para2, strKeyName, strKeyValue);
            Utility.SetAuditEntity(entity, "T_HR_SALARYSOLUTION", SalarySolution.SALARYSOLUTIONID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            string state = "";
            auditsign = true;
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
            SalarySolution.CHECKSTATE = state;
            SalarySolution.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.SalarySolutionUpdateAsync(SalarySolution);
            //Utility.UpdateCheckState("T_HR_SALARYSOLUTION", "SALARYSOLUTIONID", SalarySolution.SALARYSOLUTIONID, args);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (SalarySolution != null)
                state = SalarySolution.CHECKSTATE;
            return state;
        }
        #endregion
        public bool Save()
        {
            //ValidationHelper.ValidateProperty<T_HR_EMPLOYEE>(Employee);
            RefreshUI(RefreshedTypes.ShowProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                //  Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                if (SalarySolution.T_HR_SALARYSYSTEM == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("SALARYSOLUTION"), Utility.GetResourceStr("SELECTORGISNULL", "SALARYSYSTEM"));
                    return false;
                }


                SalarySolution.BANKNAME = (txtBankName.SelectedIndex + 1).ToString();
                SalarySolution.CHECKSTATE = Convert.ToInt16(CheckStates.UnSubmit).ToString();
                if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
                {
                    SalarySolution.UPDATEDATE = System.DateTime.Now;
                    SalarySolution.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.SalarySolutionUpdateAsync(SalarySolution);
                }
                else
                {
                    SalarySolution.PAYTYPE = (SalarySolution.PAYTYPE == null) ? "0" : SalarySolution.PAYTYPE;
                    SalarySolution.SALARYPRECISION = (SalarySolution.SALARYPRECISION == null) ? "0" : SalarySolution.SALARYPRECISION;
                    client.SalarySolutionSameSearchAsync(SalarySolution.SALARYSOLUTIONNAME);
                    //client.SalarySolutionAddAsync(SalarySolution);
                }

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
        //private void LookUp_FindClick(object sender, EventArgs e)
        //{
        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("POSTLEVEL", "T_HR_POSTLEVELDISTINCTION.POSTLEVEL");
        //    cols.Add("SALARYLEVEL", "SALARYLEVEL");


        //    LookupForm lookup = new LookupForm(EntityNames.SalaryLevel,
        //       typeof(List<T_HR_SALARYLEVEL>), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_SALARYLEVEL ent = lookup.SelectedObj as T_HR_SALARYLEVEL;
        //        if (ent != null)
        //        {
        //            lkSalaryLevel.DataContext = ent;
        //            txtSalaryLevele.Text = ent.SALARYLEVEL;
        //            SalarySolution.T_HR_SALARYLEVEL = new T_HR_SALARYLEVEL();
        //            SalarySolution.T_HR_SALARYLEVEL.SALARYLEVELID = ent.SALARYLEVELID;
        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //}

        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void DtGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {

        }

        private void btnStandardAdd_Click(object sender, RoutedEventArgs e)
        {
            T_HR_SALARYSOLUTIONSTANDARD solutionSatandard = new T_HR_SALARYSOLUTIONSTANDARD();
            if (SalarySolution != null && !string.IsNullOrEmpty(SalarySolution.SALARYSOLUTIONID))
            {
                solutionSatandard.T_HR_SALARYSOLUTION = new T_HR_SALARYSOLUTION();
                solutionSatandard.T_HR_SALARYSOLUTION.SALARYSOLUTIONID = SalarySolution.SALARYSOLUTIONID;
                Dictionary<string, string> cols = new Dictionary<string, string>();
                cols.Add("SALARYSTANDARDNAME", "SALARYSTANDARDNAME");
                cols.Add("POSTSALARY", "POSTSALARY");
                cols.Add("SECURITYALLOWANCE", "SECURITYALLOWANCE");
                cols.Add("HOUSINGALLOWANCE", "HOUSINGALLOWANCE");
                cols.Add("AREADIFALLOWANCE", "AREADIFALLOWANCE");

                System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();
                string filter = "";
                filter = "CHECKSTATE==@" + paras.Count;
                paras.Add(Convert.ToInt16(CheckStates.Approved).ToString());

                LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
                    typeof(List<T_HR_SALARYSTANDARD>), cols);
                //LookupForm lookup = new LookupForm(EntityNames.SalaryStandard,
                //  typeof(List<T_HR_SALARYSTANDARD>), cols, filter, paras);
                lookup.SelectedClick += (o, ev) =>
                {
                    T_HR_SALARYSTANDARD ent = lookup.SelectedObj as T_HR_SALARYSTANDARD;
                    if (ent != null)
                    {
                        solutionSatandard.SOLUTIONSTANDARDID = Guid.NewGuid().ToString();
                        solutionSatandard.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        solutionSatandard.CREATEDATE = System.DateTime.Now;
                        solutionSatandard.T_HR_SALARYSTANDARD = new T_HR_SALARYSTANDARD();
                        solutionSatandard.T_HR_SALARYSTANDARD.SALARYSTANDARDID = ent.SALARYSTANDARDID;
                        client.SalarySolutionStandardAddAsync(solutionSatandard);
                    }
                };

                lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            }

        }

        private void btnStandardDel_Click(object sender, RoutedEventArgs e)
        {
            //string Result = "";
            //if (DtGrid.SelectedItems.Count > 0)
            //{
            //    ObservableCollection<string> ids = new ObservableCollection<string>();

            //    foreach (T_HR_SALARYSOLUTIONSTANDARD tmp in DtGrid.SelectedItems)
            //    {
            //        ids.Add(tmp.SOLUTIONSTANDARDID);
            //    }

            //    ComfirmWindow com = new ComfirmWindow();
            //    com.OnSelectionBoxClosed += (obj, result) =>
            //    {
            //        client.SalarySolutionStandardDeleteAsync(ids);
            //    };
            //    com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);

            //}
            //else
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            //    return;
            //}
        }
        void LoadData()
        {
            if (SalarySolution != null)
            {
                int pageCount = 0;
                string filter = "";
                System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();

                //TextBox txtName = Utility.FindChildControl<TextBox>(expander, "txtName");
                if (!string.IsNullOrEmpty(SalarySolution.SALARYSOLUTIONID))
                {
                    filter += "  T_HR_SALARYSOLUTION.SALARYSOLUTIONID==@" + paras.Count().ToString();
                    paras.Add(SalarySolution.SALARYSOLUTIONID);
                }
                //   client.GetSalarySolutionStandardWithPagingAsync(dataPager.PageIndex, dataPager.PageSize, "SOLUTIONSTANDARDID", filter, paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
            }
        }

        private void lkArea_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("AREACATEGORY", "AREACATEGORY");
            // cols.Add("SALARYLEVEL", "SALARYLEVEL");


            LookupForm lookup = new LookupForm(EntityNames.AreaCategory,
               typeof(List<T_HR_AREADIFFERENCE>), cols);

            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("SALARYSYSTEMNAME", "SALARYSYSTEMNAME");
            //// cols.Add("SALARYLEVEL", "SALARYLEVEL");


            //LookupForm lookup = new LookupForm(EntityNames.SalarySystem,
            //   typeof(List<T_HR_SALARYSYSTEM>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_AREADIFFERENCE ent = lookup.SelectedObj as T_HR_AREADIFFERENCE;
                if (ent != null)
                {
                    lkArea.DataContext = ent;
                    SalarySolution.T_HR_AREADIFFERENCE = ent;
                    //SalarySolution.T_HR_SALARYLEVEL.SALARYLEVELID = ent.SALARYLEVELID;

                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void lkSalarySystem_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("SALARYSYSTEMNAME", "SALARYSYSTEMNAME");
            // cols.Add("SALARYLEVEL", "SALARYLEVEL");


            LookupForm lookup = new LookupForm(EntityNames.SalarySystem,
               typeof(List<T_HR_SALARYSYSTEM>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYSYSTEM ent = lookup.SelectedObj as T_HR_SALARYSYSTEM;
                if (ent != null)
                {
                    lkSalarySystem.DataContext = ent;
                    SalarySolution.T_HR_SALARYSYSTEM = new T_HR_SALARYSYSTEM();
                    SalarySolution.T_HR_SALARYSYSTEM.SALARYSYSTEMID = ent.SALARYSYSTEMID;

                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (tabcall.SelectedIndex == 1)
                {
                    SalaryTaxesWinForm.LoadData(SalarySolution.SALARYSOLUTIONID);
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
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
    }
}
