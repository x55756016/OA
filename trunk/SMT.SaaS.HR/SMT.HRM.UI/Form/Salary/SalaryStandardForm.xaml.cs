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

using System.Reflection;
using SMT.Saas.Tools.SalaryWS;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalaryStandardForm : BaseForm, IEntityEditor, IAudit,IClient
    {
        public T_HR_SALARYSTANDARD SalaryStandard { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private string StandardID;
        public SalaryStandardForm(FormTypes type, string standardID)
        {
            InitializeComponent();
            FormType = type;
            StandardID = standardID;
            this.Loaded += new RoutedEventHandler(SalaryStandardForm_Loaded);          
        }

        void SalaryStandardForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            //FormType = type;
            //if (type != FormTypes.Edit)
            //{
            //  bt_AddCustomSalary.Visibility = Visibility.Collapsed;
            //    txtbsign.Visibility = Visibility.Collapsed;
            txtRemark.Height = 60;
            //}
            if (string.IsNullOrEmpty(StandardID))
            {
                SalaryStandard = new T_HR_SALARYSTANDARD();
                SalaryStandard.SALARYSTANDARDID = Guid.NewGuid().ToString();
                SalaryStandard.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalaryStandard.CREATEDATE = System.DateTime.Now;

                SalaryStandard.UPDATEDATE = System.DateTime.Now;
                SalaryStandard.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                SalaryStandard.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                SalaryStandard.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                SalaryStandard.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                SalaryStandard.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                SalaryStandard.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                this.DataContext = SalaryStandard;
                StandardItemWinForm.SAVEID = SalaryStandard.SALARYSTANDARDID;
                SetToolBar();
            }
            else
            {
                client.GetSalaryStandardByIDAsync(StandardID);
                // StandardItemWinForm.SAVEID = standardID;
                StandardItemWinForm.LoadData(StandardID);
            }
        }
        private void InitParas()
        {
            client.GetSalaryStandardByIDCompleted += new EventHandler<GetSalaryStandardByIDCompletedEventArgs>(client_GetSalaryStandardByIDCompleted);

            client.SalaryStandardUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryStandardUpdateCompleted);
            client.SalaryStandardAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryStandardAddCompleted);
        }

        void client_SalaryStandardAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                FormType = FormTypes.Edit;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSTANDARD"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_SalaryStandardUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYSTANDARD"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }



        void client_GetSalaryStandardByIDCompleted(object sender, GetSalaryStandardByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
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
                SalaryStandard = e.Result;
                this.DataContext = SalaryStandard;
                StandardItemWinForm.SAVEID = SalaryStandard.SALARYSTANDARDID;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        } 

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse)
            {
                CustomGuerdonWinForm.IsEnabled = false;
                lkSalaryLevel.IsEnabled = false;
                Nodisplay();
                return;
            }
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_SALARYSTANDARD", SalaryStandard.OWNERID,
                    SalaryStandard.OWNERPOSTID, SalaryStandard.OWNERDEPARTMENTID, SalaryStandard.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_SALARYSTANDARD", SalaryStandard.OWNERID,
                    SalaryStandard.OWNERPOSTID, SalaryStandard.OWNERDEPARTMENTID, SalaryStandard.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }

        public void Nodisplay() 
        {
            try
            {
                foreach (UIElement element in TabStandard.Children)
                {
                    if (element is TextBlock)
                    {
                        continue;
                    }
                    if (element is TextBox)
                    {
                        TextBox current = ((TextBox)element);
                        current.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }


        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYSTANDARD");
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
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("BASEINFO"),
                Tooltip = Utility.GetResourceStr("BASEINFO")
            };
            items.Add(item);

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
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            if (FormType == FormTypes.Browse) items.Clear();
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Utility.SetAuditEntity(entity, "T_HR_SALARYSTANDARD", SalaryStandard.SALARYSTANDARDID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
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
            SalaryStandard.CHECKSTATE = state;
            SalaryStandard.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.SalaryStandardUpdateAsync(SalaryStandard);
            //Utility.UpdateCheckState("T_HR_SALARYSTANDARD", "SALARYSTANDARDID", SalaryStandard.SALARYSTANDARDID, args);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (SalaryStandard != null)
                state = SalaryStandard.CHECKSTATE;
            return state;
        }
        public bool Save()
        {
            //ValidationHelper.ValidateProperty<T_HR_EMPLOYEE>(Employee);
            RefreshUI(RefreshedTypes.ProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if(string.IsNullOrEmpty(lkSalaryLevel.TxtLookUp.Text.Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSTANDARD"),
Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSTANDARD"));
                RefreshUI(RefreshedTypes.ProgressBar);
                    return false;
            }
            SalaryStandard.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
               // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            else
            {

                if (FormType == FormTypes.Edit)
                {
                    SalaryStandard.UPDATEDATE = System.DateTime.Now;
                    SalaryStandard.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.SalaryStandardUpdateAsync(SalaryStandard);
                }
                else
                {
                    client.SalaryStandardAddAsync(SalaryStandard);
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
        //private void lkSalarySolution_FindClick(object sender, EventArgs e)
        //{

        //    Dictionary<string, string> cols = new Dictionary<string, string>();
        //    cols.Add("SALARYSOLUTIONNAME", "SALARYSOLUTIONNAME");
        //    cols.Add("BANKNAME", "BANKNAME");
        //    cols.Add("BANKACCOUNTNO", "BANKACCOUNTNO");

        //    LookupForm lookup = new LookupForm(EntityNames.SalarySolution,
        //        typeof(List<T_HR_SALARYSOLUTION>), cols);

        //    lookup.SelectedClick += (o, ev) =>
        //    {
        //        T_HR_SALARYSOLUTION ent = lookup.SelectedObj as T_HR_SALARYSOLUTION;
        //        if (ent != null)
        //        {
        //            lkSalarySolution.DataContext = ent;
        //            SetBasicSalary(ent);

        //        }
        //    };

        //    lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        //}
      
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void bt_AddCustomSalary_Click(object sender, RoutedEventArgs e)
        {
            //Form.Salary.CustomGuerdonForm form = new SMT.HRM.UI.Form.Salary.CustomGuerdonForm(SalaryStandard.SALARYSTANDARDID, SalaryStandard.SALARYSTANDARDNAME);
            //EntityBrowser browser = new EntityBrowser(form);
            //browser.Show();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string GETMODELTYPE = tabcall.SelectedIndex.ToString();
                switch (Convert.ToInt32(GETMODELTYPE))
                {
                    case 1:
                        CustomGuerdonWinForm.LoadData(SalaryStandard.SALARYSTANDARDID, SalaryStandard.SALARYSTANDARDNAME);
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void LookUp_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("POSTLEVEL", "T_HR_POSTLEVELDISTINCTION.POSTLEVEL");
            cols.Add("SALARYLEVEL", "SALARYLEVEL");


            LookupForm lookup = new LookupForm(EntityNames.SalaryLevel,
               typeof(List<T_HR_SALARYLEVEL>), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_SALARYLEVEL ent = lookup.SelectedObj as T_HR_SALARYLEVEL;
                if (ent != null)
                {
                    lkSalaryLevel.DataContext = ent;
                    SalaryStandard.T_HR_SALARYLEVEL = new T_HR_SALARYLEVEL();
                    SalaryStandard.T_HR_SALARYLEVEL.SALARYLEVELID = ent.SALARYLEVELID;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
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
