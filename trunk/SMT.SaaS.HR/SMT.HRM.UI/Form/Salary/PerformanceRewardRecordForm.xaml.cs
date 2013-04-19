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
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class PerformanceRewardRecordForm : BaseForm, IEntityEditor, IAudit,IClient
    {
        private System.Windows.Threading.DispatcherTimer timer;
        private T_HR_PERFORMANCEREWARDRECORD PerformanceRewardRecord { get; set; }
        SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        public FormTypes FormType { get; set; }
        public PerformanceRewardRecordForm(FormTypes type, string ID)
        {
            InitializeComponent();
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(10000);
            timer.Tick += new EventHandler(timer_Tick);
            progressGenerate.Maximum = 100;

          FormType = type;
          if (string.IsNullOrEmpty(ID))
          {
              PerformanceRewardRecord = new T_HR_PERFORMANCEREWARDRECORD();
              client.PerformanceRewardRecordAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PerformanceRewardRecordAddCompleted);
              PerformanceRewardRecord.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
              PerformanceRewardRecord.PERFORMANCEREWARDRECORDID = Guid.NewGuid().ToString();
              PerformanceRewardRecord.SALARYMONTH = System.DateTime.Now.Month>1?(System.DateTime.Now.Month-1).ToString():"12";
              PerformanceRewardRecord.SALARYYEAR = System.DateTime.Now.Month > 1 ? System.DateTime.Now.Year.ToString() : (System.DateTime.Now.Year-1).ToString();
              PerformanceRewardRecord.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
              PerformanceRewardRecord.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
              PerformanceRewardRecord.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
              PerformanceRewardRecord.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
              SetToolBar();
              this.DataContext = PerformanceRewardRecord;
          }
        }

        void client_PerformanceRewardRecordAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "PERFORMANCEREWARDRECORD"));
            }
            timer.Stop();
            progressGenerate.Value = progressGenerate.Maximum;
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        private void SetToolBar()
        {
            if(FormType == FormTypes.Browse) return ;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_PERFORMANCEREWARDRECORD", PerformanceRewardRecord.OWNERID,
                    PerformanceRewardRecord.OWNERPOSTID, PerformanceRewardRecord.OWNERDEPARTMENTID, PerformanceRewardRecord.OWNERCOMPANYID);

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
                ToolbarItems = Utility.CreateFormEditButton("T_HR_PERFORMANCEREWARDRECORD", PerformanceRewardRecord.OWNERID,
                    PerformanceRewardRecord.OWNERPOSTID, PerformanceRewardRecord.OWNERDEPARTMENTID, PerformanceRewardRecord.OWNERCOMPANYID);

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
                Title = Utility.GetResourceStr("CREATE"),  //"生成",
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "1",
            //    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
            //    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            //};

            //items.Add(item);

            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            Utility.SetAuditEntity(entity, "T_HR_PERFORMANCEREWARDRECORD", PerformanceRewardRecord.PERFORMANCEREWARDRECORDID);
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            Utility.UpdateCheckState("T_HR_PERFORMANCEREWARDRECORD", "PERFORMANCEREWARDRECORDID", PerformanceRewardRecord.PERFORMANCEREWARDRECORDID, args);
        }
        public string GetAuditState()
        {
            return "";
        }
        #endregion

        void timer_Tick(object sender, EventArgs e)
        {
            progressGenerate.Value += 1;
            if (progressGenerate.Value >= 100)
                progressGenerate.Value = 1;
        }
        private void Cancel()
        {
            //this.DialogResult = false;
            timer.Stop();
            progressGenerate.Value = progressGenerate.Minimum;
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        private void lkSelectObj_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.All;
            lookup.TitleContent = Utility.GetResourceStr("ORGANNAME");
            lookup.SelectedClick += (obj, ev) =>
            {
                lkSelectObj.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is T_HR_COMPANY)
                {
                    lkSelectObj.DisplayMemberPath = "CNAME";
                }
                else if (lookup.SelectedObj is T_HR_DEPARTMENT)
                {
                    lkSelectObj.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
                else if (lookup.SelectedObj is T_HR_POST)
                {
                    lkSelectObj.DisplayMemberPath = "T_HR_POSTDICTIONARY.POSTNAME";
                }
                else if (lookup.SelectedObj is T_HR_EMPLOYEE)
                {
                    lkSelectObj.DisplayMemberPath = "EMPLOYEECNAME";
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }


        private void Save()
        {
            //this.DialogResult = true;
            timer.Start();
            object obj = lkSelectObj.DataContext;
            if (obj == null)
            {
                return;
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            if (obj is T_HR_COMPANY)
            {
                client.PerformanceRewardRecordAddAsync(0, ((T_HR_COMPANY)obj).COMPANYID, numYear.Value.ToString(), numMonth.Value.ToString(), Convert.ToDateTime(dpstarttime.Text), Convert.ToDateTime(dpendtime.Text), GetCreateInfor());
            }
            else if (obj is T_HR_DEPARTMENT)
            {
                client.PerformanceRewardRecordAddAsync(1, ((T_HR_DEPARTMENT)obj).DEPARTMENTID, numYear.Value.ToString(), numMonth.Value.ToString(), Convert.ToDateTime(dpstarttime.Text), Convert.ToDateTime(dpendtime.Text), GetCreateInfor());
            }
            else if (obj is T_HR_POST)
            {
                client.PerformanceRewardRecordAddAsync(2, ((T_HR_POST)obj).POSTID, numYear.Value.ToString(), numMonth.Value.ToString(), Convert.ToDateTime(dpstarttime.Text), Convert.ToDateTime(dpendtime.Text), GetCreateInfor());
            }
            else if (obj is T_HR_EMPLOYEE)
            {
                client.PerformanceRewardRecordAddAsync(3, ((T_HR_EMPLOYEE)obj).EMPLOYEEID, numYear.Value.ToString(), numMonth.Value.ToString(), Convert.ToDateTime(dpstarttime.Text), Convert.ToDateTime(dpendtime.Text), GetCreateInfor());
            }
        }

        private string GetCreateInfor()
        {
            string sbstr = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID + ";";

            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID + ";";
            sbstr += SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            return sbstr;
        }

        private void lkSelectObj2_FindClick(object sender, EventArgs e)
        {

        }

        private void cbxPayState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void dpstarttime_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            dp.Text = DateTime.Now.Date.ToString();
        }

        private void dpendtime_Loaded(object sender, RoutedEventArgs e)
        {
            DatePicker dp = (DatePicker)sender;
            int MaxDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            dp.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, MaxDate).ToShortDateString();
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
