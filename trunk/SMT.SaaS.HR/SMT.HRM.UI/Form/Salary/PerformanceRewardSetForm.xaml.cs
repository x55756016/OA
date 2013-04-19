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
using SMT.Saas.Tools.PermissionWS;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class PerformanceRewardSetForm : BaseForm, IEntityEditor, IAudit
    {
        public T_HR_PERFORMANCEREWARDSET PerformanceRewardSet { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        public PerformanceRewardSetForm(FormTypes type, string PerformanceRewardSetID)
        {
            InitializeComponent();

            InitParas();

            FormType = type;
            if (string.IsNullOrEmpty(PerformanceRewardSetID))
            {
                PerformanceRewardSet = new T_HR_PERFORMANCEREWARDSET();
                PerformanceRewardSet.PERFORMANCEREWARDSETID = Guid.NewGuid().ToString();
                PerformanceRewardSet.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                PerformanceRewardSet.CREATEDATE = System.DateTime.Now;

                PerformanceRewardSet.UPDATEDATE = System.DateTime.Now;
                PerformanceRewardSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                PerformanceRewardSet.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                PerformanceRewardSet.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                PerformanceRewardSet.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                PerformanceRewardSet.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                PerformanceRewardSet.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                PerformanceRewardSet.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                PerformanceRewardSet.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                PerformanceRewardSet.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                this.DataContext = PerformanceRewardSet;
                SetToolBar();
            }
            else
            {
                client.GetPerformanceRewardSetByIDAsync(PerformanceRewardSetID);
            }
        }

        private void InitParas()
        {
            client.GetPerformanceRewardSetByIDCompleted += new EventHandler<GetPerformanceRewardSetByIDCompletedEventArgs>(client_GetPerformanceRewardSetByIDCompleted);

            client.PerformanceRewardSetUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PerformanceRewardSetUpdateCompleted);
            client.PerformanceRewardSetAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PerformanceRewardSetAddCompleted);
        }

        void client_PerformanceRewardSetAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "PERFORMANCEREWARDSET"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_PerformanceRewardSetUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PERFORMANCEREWARDSET"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }



        void client_GetPerformanceRewardSetByIDCompleted(object sender, GetPerformanceRewardSetByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    return;
                }
                PerformanceRewardSet = e.Result;
                this.DataContext = PerformanceRewardSet;
                BindCommbox();
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
        }
        void BindCommbox()
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbPerformanceCategory.Items)
            {
                if (item.DICTIONARYVALUE.ToString() == PerformanceRewardSet.PERFORMANCECATEGORY)
                {
                    cbPerformanceCategory.SelectedItem = item;
                }

            }
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbCalculateType.Items)
            {
                if (item.DICTIONARYVALUE.ToString() == PerformanceRewardSet.CALCULATETYPE)
                {
                    cbCalculateType.SelectedItem = item;
                }
            }
        }
        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_PERFORMANCEREWARDSET", PerformanceRewardSet.OWNERID,
                    PerformanceRewardSet.OWNERPOSTID, PerformanceRewardSet.OWNERDEPARTMENTID, PerformanceRewardSet.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_PERFORMANCEREWARDSET", PerformanceRewardSet.OWNERID,
                    PerformanceRewardSet.OWNERPOSTID, PerformanceRewardSet.OWNERDEPARTMENTID, PerformanceRewardSet.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("PERFORMANCEREWARDSET");
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

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
           // Utility.SetAuditEntity(entity, "T_HR_PERFORMANCEREWARDSET", PerformanceRewardSet.PERFORMANCEREWARDSETID);
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_PERFORMANCEREWARDSET>(PerformanceRewardSet, "HR");
            Utility.SetAuditEntity(entity, "T_HR_PERFORMANCEREWARDSET", PerformanceRewardSet.PERFORMANCEREWARDSETID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            //string state = "";
            //switch (args)
            //{
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
            //        state = Utility.GetCheckState(CheckStates.Approving);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
            //        state = Utility.GetCheckState(CheckStates.Approved);
            //        break;
            //    case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
            //        state = Utility.GetCheckState(CheckStates.UnApproved);
            //        break;
            //}
            //PerformanceRewardSet.CHECKSTATE = state;
            //PerformanceRewardSet.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            //client.PerformanceRewardSetUpdateAsync(PerformanceRewardSet);
            Utility.UpdateCheckState("T_HR_PERFORMANCEREWARDSET", "PERFORMANCEREWARDSETID", PerformanceRewardSet.PERFORMANCEREWARDSETID, args);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (PerformanceRewardSet != null)
                state = PerformanceRewardSet.CHECKSTATE;
            return state;
        }
        #endregion
        public void Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            //ValidationHelper.ValidateProperty<T_HR_EMPLOYEE>(Employee);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            //T_SYS_DICTIONARY performanceCategory = cbPerformanceCategory.SelectedItem as T_SYS_DICTIONARY;
            //T_SYS_DICTIONARY calculateType = cbCalculateType.SelectedItem as T_SYS_DICTIONARY;

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.ProgressBar);
            }
            else
            {
                if (cbPerformanceCategory.SelectedIndex <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "PERFORMANCECATEGORY"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return;
                }
                if (cbCalculateType.SelectedIndex <= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "CALCULATETYPE"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return;
                }
                if (string.IsNullOrEmpty(txtPerformanceCapital.Text.Trim()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "PERFORMANCECAPITAL"));
                    RefreshUI(RefreshedTypes.ProgressBar);
                    return;
                }
                if (FormType == FormTypes.Edit)
                {
                    PerformanceRewardSet.UPDATEDATE = System.DateTime.Now;
                    PerformanceRewardSet.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.PerformanceRewardSetUpdateAsync(PerformanceRewardSet);
                }
                else
                {
                    client.PerformanceRewardSetAddAsync(PerformanceRewardSet);
                }

            }
        }
        /// <summary>
        /// 关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            if (!flag)
            {
                return;
            }
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }



        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
