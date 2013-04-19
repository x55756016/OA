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

using SMT.Saas.Tools.AttendanceWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using System.Collections.ObjectModel;

using System.Windows.Data;
using System.Windows.Controls.Primitives;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.OrganizationWS;
using OrganizationWS = SMT.Saas.Tools.OrganizationWS;


namespace SMT.HRM.UI.Form.Attendance
{
    public partial class AttendanceMachineSetForm : BaseForm, IEntityEditor
    {
        private  AttendanceServiceClient client;
        private OrganizationServiceClient orgclient;
        //private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private T_HR_ATTENDMACHINESET attendMachineSet;
        private FormTypes formType;
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        public T_HR_ATTENDMACHINESET AttendMachineSet
        {
            get 
            {
                return attendMachineSet;
            }
            set 
            {
                attendMachineSet = value;
                this.DataContext = attendMachineSet;
            }
        }

        public AttendanceMachineSetForm()
        {
            InitializeComponent();
        }

        public AttendanceMachineSetForm(FormTypes type, string AttendanceMachineSetID)
        {
            InitializeComponent();
            FormType = type;
            InitParas(AttendanceMachineSetID);
            if (string.IsNullOrEmpty(AttendanceMachineSetID))
            {
                AttendMachineSet = new T_HR_ATTENDMACHINESET();
                AttendMachineSet.ATTENDMACHINESETID = Guid.NewGuid().ToString();
            }
            else
            {
                client.GetAttendMachineSetByIDAsync(AttendanceMachineSetID);
            }
        }

        private void InitParas(string AttendanceMachineSetID)
        {
            client = new AttendanceServiceClient();
            orgclient = new OrganizationServiceClient();
            client.GetAttendMachineSetByIDCompleted += new EventHandler<GetAttendMachineSetByIDCompletedEventArgs>(client_GetAttendMachineSetByIDCompleted);
            client.AttendMachineSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AttendMachineSetAddCompleted);
            client.AttendMachineSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AttendMachineSetUpdateCompleted);
            orgclient.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(orgclient_GetCompanyByIdCompleted);
        }

        void orgclient_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if(e.Result!=null)
                lkCustomSalary.TxtLookUp.Text = e.Result.CNAME;
            }
            else
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        }

        void client_AttendMachineSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "ATTENDMACHINESET"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_AttendMachineSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "ATTENDMACHINESET"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetAttendMachineSetByIDCompleted(object sender, GetAttendMachineSetByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    this.AttendMachineSet = e.Result as T_HR_ATTENDMACHINESET;
                    orgclient.GetCompanyByIdAsync(this.AttendMachineSet.COMPANYID);
                }
                else
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ATTENDMACHINESET"));
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("ATTENDMACHINESET");
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

            return items;
        }

        //public List<ToolbarItem> GetToolBarItems()
        //{
        //    return ToolbarItems;
        //}

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

        void Save()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ProgressBar);
            if (validators.Count > 0)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else if (lkCustomSalary.DataContext==null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "COMPANY"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return;
            }
            else
            {
                if (FormType == FormTypes.Edit)
                {
                    AttendMachineSet.UPDATEDATE = System.DateTime.Now;
                    AttendMachineSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.AttendMachineSetUpdateAsync(AttendMachineSet);
                }
                else
                {
                    AttendMachineSet.CREATEDATE = System.DateTime.Now;
                    AttendMachineSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    AttendMachineSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    AttendMachineSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    AttendMachineSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    AttendMachineSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    AttendMachineSet.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    AttendMachineSet.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    AttendMachineSet.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    client.AttendMachineSetAddAsync(AttendMachineSet);
                }
            }
        }
        void Cancel()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #region ----------
        private void lkCustomSalary_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("COMPANRYCODE", "COMPANRYCODE");
            cols.Add("CNAME", "CNAME");
            cols.Add("ADDRESS", "ADDRESS");

            LookupForm lookup = new LookupForm(EntityNames.Company,
                typeof(List<T_HR_COMPANY>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_COMPANY ent = lookup.SelectedObj as T_HR_COMPANY;
                if (ent != null)
                {
                    lkCustomSalary.DataContext = ent;
                    lkCustomSalary.TxtLookUp.Text = ent.CNAME;
                    AttendMachineSet.COMPANYID = ent.COMPANYID;
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }

        #endregion
    }
}
