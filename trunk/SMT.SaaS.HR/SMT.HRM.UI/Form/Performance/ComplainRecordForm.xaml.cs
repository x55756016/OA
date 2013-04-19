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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;

using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PerformanceWS;
using SMT.HRM.UI.Form.Performance;
using SMT.Saas.Tools.PermissionWS;

namespace SMT.HRM.UI.Form.Performance
{
    public partial class ComplainRecordForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        private bool flag = false;
        PerformanceServiceClient client;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool auditsign = false;
        public T_HR_KPIRECORD kpirecord { get; set; }
        private T_HR_KPIRECORDCOMPLAIN kpirecordComplain;

        public T_HR_KPIRECORDCOMPLAIN KPIRecordComplain
        {
            get { return kpirecordComplain; }
            set
            {
                kpirecordComplain = value;
                //this.DataContext = kpirecordComplain;
            }
        }

        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }


        public ComplainRecordForm()
        {
            InitializeComponent();
        }

        public ComplainRecordForm(FormTypes type, V_COMPLAINRECORD entity)
        {
            InitializeComponent();
            FormType = type;
            kpirecord = new T_HR_KPIRECORD();
            kpirecord = entity.T_HR_KPIRECORD;
            KPIRecordComplain = entity.T_HR_KPIRECORDCOMPLAIN;
            try
            {
                txtComplainName.Text = entity.EMPLOYEECNAME;
                txtflowid.Text = entity.FLOWID;
                txtInitScroe.Text = entity.T_HR_KPIRECORDCOMPLAIN.INITIALSCORE.ToString();
                txtScroe.Text = entity.T_HR_KPIRECORDCOMPLAIN.REVIEWSCORE.ToString();
                txtMark.Text = entity.T_HR_KPIRECORDCOMPLAIN.COMPLAINREMARK.ToString();
            }
            catch { }
            //InitParas();
        }

        public ComplainRecordForm(FormTypes type, string kpirecordComplainID)
        {
            InitializeComponent();
            FormType = type;
            kpirecord = new T_HR_KPIRECORD();
            KPIRecordComplain = new T_HR_KPIRECORDCOMPLAIN();
            KPIRecordComplain.COMPLAINID = kpirecordComplainID;
            InitParas();
        }

        private void InitParas()
        {
            client = new PerformanceServiceClient();
            client.KPIRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_KPIRecordUpdateCompleted);
            client.GetCompainRecordByIDCompleted += new EventHandler<GetCompainRecordByIDCompletedEventArgs>(client_GetCompainRecordByIDCompleted);
            client.CompainRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CompainRecordUpdateCompleted);
            client.UpdateKPIRecordComplainCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_UpdateKPIRecordComplainCompleted);
            client.GetVcomplainRecordByIDCompleted += new EventHandler<GetVcomplainRecordByIDCompletedEventArgs>(client_GetVcomplainRecordByIDCompleted);
            client.CompainRecordUpdateOverCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CompainRecordUpdateOverCompleted);
            if (FormType == FormTypes.New)
            {
                KPIRecordComplain = new T_HR_KPIRECORDCOMPLAIN();
                KPIRecordComplain.COMPLAINID = Guid.NewGuid().ToString();
                KPIRecordComplain.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                SetToolBar();
            }
            else
            {
                client.GetVcomplainRecordByIDAsync(KPIRecordComplain.COMPLAINID);
                //client.GetCompainRecordByIDAsync(KPIRecordComplain.COMPLAINID);
            }
        }

        void client_CompainRecordUpdateOverCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_GetVcomplainRecordByIDCompleted(object sender, GetVcomplainRecordByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                V_COMPLAINRECORD v = e.Result as V_COMPLAINRECORD;
                if (v != null)
                {
                    kpirecord = v.T_HR_KPIRECORD;
                    KPIRecordComplain = v.T_HR_KPIRECORDCOMPLAIN;
                    try
                    {
                        txtComplainName.Text = v.EMPLOYEECNAME;
                        txtflowid.Text = string.IsNullOrEmpty(v.FLOWID) ? string.Empty : v.FLOWID;
                        txtInitScroe.Text = v.T_HR_KPIRECORDCOMPLAIN.INITIALSCORE != null ? v.T_HR_KPIRECORDCOMPLAIN.INITIALSCORE.ToString() : string.Empty;
                        txtScroe.Text = v.T_HR_KPIRECORDCOMPLAIN.REVIEWSCORE != null ? v.T_HR_KPIRECORDCOMPLAIN.REVIEWSCORE.ToString() : string.Empty;
                        txtMark.Text = string.IsNullOrEmpty(v.T_HR_KPIRECORDCOMPLAIN.COMPLAINREMARK) ? string.Empty : v.T_HR_KPIRECORDCOMPLAIN.COMPLAINREMARK;
                    }
                    catch { }
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        void client_KPIRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_UpdateKPIRecordComplainCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        void client_CompainRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (!auditsign) Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    auditsign = false;
                }
            }
            else
            {
                if (!auditsign)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "KPIRECORDCOMPLAIN"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("审核通过"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.GetResourceStr("SUSSESSEDSUBMIT", "KPIRECORDCOMPLAIN"));
                    //修改申诉状态
                    kpirecord.SUMSCORE = KPIRecordComplain.REVIEWSCORE;
                    client.KPIRecordUpdateAsync(kpirecord);
                    if (KPIRecordComplain.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved))
                    {
                        client.CompainRecordUpdateOverAsync(kpirecord.KPIRECORDID);
                    }
                    auditsign = false;
                }
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.All);
            if (flag)
            {
                flag = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_GetCompainRecordByIDCompleted(object sender, GetCompainRecordByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                KPIRecordComplain = e.Result;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_KPIRECORDCOMPLAIN", KPIRecordComplain.OWNERID,
                    KPIRecordComplain.OWNERPOSTID, KPIRecordComplain.OWNERDEPARTMENTID, KPIRecordComplain.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        private void Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (FormType == FormTypes.Edit)
            {
                KPIRecordComplain.UPDATEDATE = System.DateTime.Now;
                //client.CompainRecordUpdateAsync(KPIRecordComplain);
            }
            else
            {
                RefreshUI(RefreshedTypes.ProgressBar);


                //KPIRecordComplain.CHECKSTATE = "0";
                //KPIRecordComplain.CREATEDATE = System.DateTime.Now;
                //KPIRecordComplain.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //KPIRecordComplain.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                //KPIRecordComplain.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //KPIRecordComplain.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                //KPIRecordComplain.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                //KPIRecordComplain.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                //KPIRecordComplain.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                //KPIRecordComplain.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

            }

        }
        private void Cancel()
        {
            flag = true;
            Save();
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("KPIRECORDCOMPLAIN");
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
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = "详细信息",
                Tooltip = "详细信息"
            };
            items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = "薪资标准",
            //    Tooltip = "薪资标准",
            //    Url = "/Salary/SalaryStandard.xaml"
            //};
            //items.Add(item); 
            return items;
        }
        //public List<ToolbarItem> GetToolBarItem()
        //{
        //    List<ToolbarItem> items = new List<ToolbarItem>();

        //    ToolbarItem item = new ToolbarItem
        //    {
        //        DisplayType = ToolbarItemDisplayTypes.Image,
        //        Key = "0",
        //        Title = Utility.GetResourceStr("SAVE"),
        //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
        //    };

        //    items.Add(item);

        //    item = new ToolbarItem
        //    {
        //        DisplayType = ToolbarItemDisplayTypes.Image,
        //        Key = "1",
        //        Title = Utility.GetResourceStr("SAVEANDCLOSE"),
        //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
        //    };

        //    items.Add(item);

        //    return items;
        //}

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

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EntityKey", KPIRecordComplain.COMPLAINID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();

            string strXmlObjectSource = string.Empty;
            string strKeyName = "COMPLAINID";
            string strKeyValue = KPIRecordComplain.COMPLAINID;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_KPIRECORDCOMPLAIN>(KPIRecordComplain, para, "HR", para2, strKeyName, strKeyValue);
            Utility.SetAuditEntity(entity, "T_HR_KPIRECORDCOMPLAIN", KPIRecordComplain.COMPLAINID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            auditsign = true;
            string state = "";
            //Utility.UpdateCheckState("T_HR_KPIRECORDCOMPLAIN", "COMPLAINID", KPIRecordComplain.COMPLAINID, args);
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    kpirecord.COMPLAINSTATUS = "1";
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    kpirecord.COMPLAINSTATUS = "2";
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    kpirecord.COMPLAINSTATUS = "2";
                    break;
            }
            KPIRecordComplain.CHECKSTATE = state;
            client.CompainRecordUpdateAsync(KPIRecordComplain, "Audit");
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (KPIRecordComplain != null)
                state = KPIRecordComplain.CHECKSTATE;
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
    }
}
