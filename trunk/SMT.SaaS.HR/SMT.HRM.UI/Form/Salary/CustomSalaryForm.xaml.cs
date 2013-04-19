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

namespace SMT.HRM.UI.Form.Salary
{
    public partial class CustomSalaryForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        SalaryServiceClient client;
        private bool flag = false;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private bool auditsign = false;
        bool onlyones = false;

        public string customguerdonsetid { get; set; }

        public bool Onlyone
        {
            get { return onlyones; }
            set { onlyones = value; }
        }

        public T_HR_CUSTOMGUERDONSET CustomGuerdonSetPost { get; set; }

        private T_HR_CUSTOMGUERDONSET customGuerdonSet;

        public T_HR_CUSTOMGUERDONSET CustomGuerdonSet
        {
            get { return customGuerdonSet; }
            set
            {
                customGuerdonSet = value;
                this.DataContext = customGuerdonSet;
            }
        }

        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public CustomSalaryForm()
        {
            InitializeComponent();
        }

        public CustomSalaryForm(FormTypes type, string customGuerdonSetID)
        {
            InitializeComponent();
            FormType = type;
            customguerdonsetid = customGuerdonSetID;
            this.Loaded += new RoutedEventHandler(CustomSalaryForm_Loaded);
            //InitParas(customGuerdonSetID);
        }

        void CustomSalaryForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas(customguerdonsetid);
        }

        private void InitParas(string customGuerdonSetID)
        {
            client = new SalaryServiceClient();
            client.GetCustomGuerdonSetNameCompleted += new EventHandler<GetCustomGuerdonSetNameCompletedEventArgs>(client_GetCustomGuerdonSetNameCompleted);
            client.GetCustomGuerdonSetByIDCompleted += new EventHandler<GetCustomGuerdonSetByIDCompletedEventArgs>(client_GetCustomGuerdonSetByIDCompleted);
            client.CustomGuerdonSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CustomGuerdonSetAddCompleted);
            client.CustomGuerdonSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CustomGuerdonSetUpdateCompleted);

            if (FormType == FormTypes.New)
            {
                CustomGuerdonSet = new T_HR_CUSTOMGUERDONSET();
                CustomGuerdonSet.CUSTOMGUERDONSETID = Guid.NewGuid().ToString();
                CustomGuerdonSet.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                SetToolBar();
            }
            else
            {
                client.GetCustomGuerdonSetByIDAsync(customGuerdonSetID);
            }

        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;
                switch (rb.Name)
                {
                    case "rbadd":
                        CustomGuerdonSet.GUERDONCATEGORY = "1";
                        break;
                    case "rbsubtract":
                        CustomGuerdonSet.GUERDONCATEGORY = "2";
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        void client_GetCustomGuerdonSetNameCompleted(object sender, GetCustomGuerdonSetNameCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            Onlyone = e.Result;
        }

        void client_GetCustomGuerdonSetByIDCompleted(object sender, GetCustomGuerdonSetByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("FINDERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                CustomGuerdonSet = e.Result;
                switch (CustomGuerdonSet.GUERDONCATEGORY)
                {
                    case "1":
                        rbadd.IsChecked = true;
                        break;
                    case "2":
                        rbsubtract.IsChecked = true;
                        break;
                }
                if (FormType == FormTypes.Resubmit)
                {
                    CustomGuerdonSet.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    CustomGuerdonSet.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        void client_CustomGuerdonSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                FormType = FormTypes.Edit;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "CUSTOMSALARY"));
                RefreshUI(RefreshedTypes.AuditInfo);
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
            if (flag)
            {
                flag = false;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
        }

        void client_CustomGuerdonSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (!auditsign) Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
                    auditsign = false;
                }
            }
            else
            {
                if (!auditsign)
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CUSTOMSALARY"));
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "CUSTOMSALARY"));
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

        private void SetToolBar()
        {
            //if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.OWNERID,
            //        CustomGuerdonSet.OWNERPOSTID, CustomGuerdonSet.OWNERDEPARTMENTID, CustomGuerdonSet.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.OWNERID,
                    CustomGuerdonSet.OWNERPOSTID, CustomGuerdonSet.OWNERDEPARTMENTID, CustomGuerdonSet.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }


        public void SubmitAudit()
        {
            if (CustomGuerdonSet != null)
            {
                CustomGuerdonSet.CHECKSTATE = Convert.ToInt32(CheckStates.Approving).ToString();
                CustomGuerdonSet.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                CustomGuerdonSet.UPDATEDATE = DateTime.Now;
                CustomGuerdonSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.CustomGuerdonSetUpdateAsync(CustomGuerdonSet, "SubmitAudit");
            }
        }
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //string strXmlObjectSource = string.Empty; 
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_CUSTOMGUERDONSET>(CustomGuerdonSet, "HR");
            //Utility.SetAuditEntity(entity, "T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.CUSTOMGUERDONSETID, strXmlObjectSource);

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("GUERDONNAME", CustomGuerdonSet.GUERDONNAME);
            para.Add("EMPLOYEEID", CustomGuerdonSet.CREATEUSERID);
            string strXmlObjectSource = string.Empty;

            strXmlObjectSource = Utility.ObjListToXml<T_HR_CUSTOMGUERDONSET>(CustomGuerdonSet, para, "HR");

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", CustomGuerdonSet.CREATEUSERID);
            paraIDs.Add("CreatePostID", CustomGuerdonSet.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", CustomGuerdonSet.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", CustomGuerdonSet.OWNERCOMPANYID);

            if (CustomGuerdonSet.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.CUSTOMGUERDONSETID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_CUSTOMGUERDONSET", CustomGuerdonSet.CUSTOMGUERDONSETID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            auditsign = true;
            //Utility.UpdateCheckState("T_HR_CUSTOMGUERDONSET", "CUSTOMGUERDONSETID", customguerdonsetid, args);
            string state = "";
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
            CustomGuerdonSet.CHECKSTATE = state;
            CustomGuerdonSet.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.CustomGuerdonSetUpdateAsync(CustomGuerdonSet, "Audit");
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (CustomGuerdonSet != null)
                state = CustomGuerdonSet.CHECKSTATE;
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CUSTOMSALARY");
        }
        public string GetStatus()
        {
            return "";// CustomGuerdonSet != null ? CustomGuerdonSet.CREATECOMPANYID : "";
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

        private void Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ProgressBar);
            if (validators.Count > 0)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                // MessageBox.Show(validators.Count.ToString() + " invalid validators");
            }
            else if (txtSalaryName.Text.Trim() == string.Empty)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CUSTOMSALARYNAME"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else if (txtMoney.Text.Trim() == string.Empty)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "GUERDONSUM"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            //else if (combproperty == null || combproperty.SelectedIndex < 1)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "GUERDONCATEGORY"));
            //    RefreshUI(RefreshedTypes.ProgressBar);
            //    return;
            //}
            else if (combcal == null || combcal.SelectedIndex < 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "CALCULATETYPE"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else
            {
                if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
                {
                    CustomGuerdonSet.GUERDONCATEGORY = combproperty.SelectedIndex.ToString();
                    CustomGuerdonSet.CALCULATORTYPE = (combcal.SelectedIndex + 1).ToString();
                    CustomGuerdonSet.UPDATEDATE = System.DateTime.Now;
                    client.CustomGuerdonSetUpdateAsync(CustomGuerdonSet, CustomGuerdonSetPost);
                }
                else
                {
                    if (Onlyone)
                    {
                        RefreshUI(RefreshedTypes.ProgressBar);
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ALREADYEXISTS"));
                        //txtSalaryName.BorderBrush = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        //txtSalaryName.BorderBrush = new SolidColorBrush(Colors.White);
                        //CustomGuerdonSet.GUERDONCATEGORY = combproperty.SelectedIndex.ToString();
                        CustomGuerdonSet.CALCULATORTYPE = (combcal.SelectedIndex + 1).ToString();
                        CustomGuerdonSet.CHECKSTATE = "0";
                        CustomGuerdonSet.CREATEDATE = System.DateTime.Now;
                        CustomGuerdonSet.GUERDONCATEGORY = (CustomGuerdonSet.GUERDONCATEGORY == null) ? "1" : CustomGuerdonSet.GUERDONCATEGORY;
                        CustomGuerdonSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        CustomGuerdonSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        CustomGuerdonSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        CustomGuerdonSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        CustomGuerdonSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        CustomGuerdonSet.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        CustomGuerdonSet.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        CustomGuerdonSet.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                        client.CustomGuerdonSetAddAsync(CustomGuerdonSet, CustomGuerdonSetPost);
                    }
                }
            }
        }


        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        private void Cancel()
        {
            flag = true;
            Save();
        }

        private void txtSalaryName_LostFocus(object sender, RoutedEventArgs e)
        {
            client.GetCustomGuerdonSetNameAsync(txtSalaryName.Text);
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
