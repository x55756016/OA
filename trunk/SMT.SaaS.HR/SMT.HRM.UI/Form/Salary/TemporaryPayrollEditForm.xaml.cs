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

namespace SMT.HRM.UI.Form.Salary
{
    public partial class TemporaryPayrollEditForm : BaseForm, IEntityEditor, IAudit,IClient
    {
        SalaryServiceClient client = new SalaryServiceClient();
        private T_HR_CUSTOMGUERDONRECORD customguerdonrecord = new T_HR_CUSTOMGUERDONRECORD();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string customguerdonrecordid = String.Empty;
        private bool flag = false;

        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        public TemporaryPayrollEditForm()
        {
            InitializeComponent();
        }

        public TemporaryPayrollEditForm(string CustomGuerdonRecordID)
        {
            InitializeComponent();
            InitPart();
            FormType = FormTypes.Edit;
            customguerdonrecordid = CustomGuerdonRecordID;
            client.GetCustomGuerdonRecordByIDAsync(customguerdonrecordid);
        }

        public TemporaryPayrollEditForm(FormTypes formtype,string CustomGuerdonRecordID)
        {
            InitializeComponent();
            InitPart();
            FormType = formtype;
            customguerdonrecordid = CustomGuerdonRecordID;
            client.GetCustomGuerdonRecordByIDAsync(customguerdonrecordid);
        }

        public void InitPart() 
        {
            client.GetCustomGuerdonRecordByIDCompleted += new EventHandler<GetCustomGuerdonRecordByIDCompletedEventArgs>(client_GetCustomGuerdonRecordByIDCompleted);
            client.CustomGuerdonRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_CustomGuerdonRecordUpdateCompleted);
        }

        void client_CustomGuerdonRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "T_HR_CUSTOMGUERDONRECORD"));
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

        void client_GetCustomGuerdonRecordByIDCompleted(object sender, GetCustomGuerdonRecordByIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    T_HR_CUSTOMGUERDONRECORD getdata = e.Result;
                    this.DataContext = getdata;
                    customguerdonrecord = getdata;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_CUSTOMGUERDONRECORD", customguerdonrecord.OWNERID,
                    customguerdonrecord.OWNERPOSTID, customguerdonrecord.OWNERDEPARTMENTID, customguerdonrecord.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_CUSTOMGUERDONRECORD", customguerdonrecord.OWNERID,
                    customguerdonrecord.OWNERPOSTID, customguerdonrecord.OWNERDEPARTMENTID, customguerdonrecord.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_CUSTOMGUERDONRECORD>(customguerdonrecord, "HR");
            Utility.SetAuditEntity(entity, "T_HR_EMPLOYEESALARYRECORD", customguerdonrecord.CUSTOMGUERDONRECORDID, strXmlObjectSource);
            //Utility.SetAuditEntity(entity, "T_HR_CUSTOMGUERDONRECORD", customguerdonrecord.CUSTOMGUERDONRECORDID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_CUSTOMGUERDONRECORD", "CUSTOMGUERDONRECORDID", customguerdonrecord.CUSTOMGUERDONRECORDID, args);
        }
        public string GetAuditState() 
        {
            string state = "-1";
            if (customguerdonrecord != null)
                state = customguerdonrecord.CHECKSTATE;
            return state;
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CUSTOMGUERDONRECORD");
        }
        public string GetStatus()
        {
            string state = "-1";
            if (customguerdonrecord != null)
                state = customguerdonrecord.CHECKSTATE;
            return state;
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
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            if (FormType == FormTypes.Browse) items.Clear();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        public void Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            customguerdonrecord = this.DataContext as T_HR_CUSTOMGUERDONRECORD;
            client.CustomGuerdonRecordUpdateAsync(customguerdonrecord);
        }
        public void Cancel() 
        {
            flag = true;
            Save();
        }

        private void lkEmployee_FindClick(object sender, EventArgs e)
        {

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
