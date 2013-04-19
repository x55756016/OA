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
namespace SMT.HRM.UI.Form.Salary
{
    public partial class PerformanceRewardForm : BaseForm, IEntityEditor, IAudit,IClient
    {
        public T_HR_PERFORMANCEREWARDRECORD performanceRewardRecord { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();

        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        public PerformanceRewardForm(FormTypes type, string ID)
        {
            InitializeComponent();
            InitParas();
            FormType = type;
            if (string.IsNullOrEmpty(ID))
            {
                performanceRewardRecord = new T_HR_PERFORMANCEREWARDRECORD();
                performanceRewardRecord.PERFORMANCEREWARDRECORDID = Guid.NewGuid().ToString();
                performanceRewardRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                performanceRewardRecord.CREATEDATE = System.DateTime.Now;

                //performanceRewardRecord.UPDATEDATE = System.DateTime.Now;
                //performanceRewardRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                performanceRewardRecord.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                performanceRewardRecord.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                performanceRewardRecord.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                performanceRewardRecord.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                this.DataContext = performanceRewardRecord;
                SetToolBar();
            }
            else
            {
                client.GetPerformaceRewardByIDAsync(ID);
            }
        }


        void InitParas()
        {
            client.PerformanceRewardRecordUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PerformanceRewardRecordUpdateCompleted);
            client.GetPerformaceRewardByIDCompleted += new EventHandler<GetPerformaceRewardByIDCompletedEventArgs>(client_GetPerformaceRewardByIDCompleted);
        }



        void client_PerformanceRewardRecordUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.UserState != null && e.UserState.ToString() == "Audit")
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSFUL"));
                else
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "PERFORMANCEREWARDRECORD"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetPerformaceRewardByIDCompleted(object sender, GetPerformaceRewardByIDCompletedEventArgs e)
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
                performanceRewardRecord = e.Result;
                this.DataContext = performanceRewardRecord;
                T_HR_EMPLOYEE employee = new T_HR_EMPLOYEE();
                employee.EMPLOYEECNAME = performanceRewardRecord.EMPLOYEENAME;
                lkEmployeeName.DataContext = employee;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            else
            {
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_PERFORMANCEREWARDRECORD", performanceRewardRecord.OWNERID,
                    performanceRewardRecord.OWNERPOSTID, performanceRewardRecord.OWNERDEPARTMENTID, performanceRewardRecord.OWNERCOMPANYID);
            }

            if (FormType == FormTypes.Edit)
            {

                ToolbarItems = Utility.CreateFormEditButton();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_PERFORMANCEREWARDRECORD", performanceRewardRecord.OWNERID,
                    performanceRewardRecord.OWNERPOSTID, performanceRewardRecord.OWNERDEPARTMENTID, performanceRewardRecord.OWNERCOMPANYID);
            }

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("PERFORMANCEREWARDRECORD");
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
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            // Utility.SetAuditEntity(entity, "T_HR_PERFORMANCEREWARDRECORD", performanceRewardRecord.PERFORMANCEREWARDRECORDID);
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_PERFORMANCEREWARDRECORD>(performanceRewardRecord, "HR");
            Utility.SetAuditEntity(entity, "T_HR_PERFORMANCEREWARDRECORD", performanceRewardRecord.PERFORMANCEREWARDRECORDID, strXmlObjectSource);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
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
            performanceRewardRecord.CHECKSTATE = state;
            client.PerformanceRewardRecordUpdateAsync(performanceRewardRecord, "Audit");
            // Utility.UpdateCheckState("T_HR_PERFORMANCEREWARDRECORD", "PERFORMANCEREWARDRECORDID", performanceRewardRecord.PERFORMANCEREWARDRECORDID, args);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (performanceRewardRecord != null)
                state = performanceRewardRecord.CHECKSTATE;
            return state;
        }
        #endregion
        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (validators.Count > 0)
            {
                //could use the content of the list to show an invalid message summary somehow
                //MessageBox.Show(validators.Count.ToString() + " invalid validators");
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            else
            {

                if (FormType == FormTypes.Edit)
                {
                    performanceRewardRecord.UPDATEDATE = System.DateTime.Now;
                    performanceRewardRecord.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                    client.PerformanceRewardRecordUpdateAsync(performanceRewardRecord);
                }
                else
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
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
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");

            LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
                typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
                    performanceRewardRecord.EMPLOYEENAME = ent.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    performanceRewardRecord.EMPLOYEECODE = ent.T_HR_EMPLOYEE.EMPLOYEECODE;
                    performanceRewardRecord.EMPLOYEEID = ent.T_HR_EMPLOYEE.EMPLOYEEID;

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
