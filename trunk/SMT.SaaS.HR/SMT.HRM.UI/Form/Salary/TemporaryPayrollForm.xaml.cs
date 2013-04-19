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

namespace SMT.HRM.UI.Form.Salary
{
    public partial class TemporaryPayrollForm : BaseForm, IEntityEditor,IClient
    {
        private bool flag = false;
        private System.Windows.Threading.DispatcherTimer timer;
        SalaryServiceClient client = new SalaryServiceClient ();
        public TemporaryPayrollForm()
        {
            InitializeComponent();
            client.CustomGuerdonRecordCompleted += new EventHandler<CustomGuerdonRecordCompletedEventArgs>(client_CustomGuerdonRecordCompleted);
            client.CustomGuerdonRecordAccountCompleted += new EventHandler<CustomGuerdonRecordAccountCompletedEventArgs>(client_CustomGuerdonRecordAccountCompleted);
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(10000);
            timer.Tick += new EventHandler(timer_Tick);
            progressGenerate.Maximum = 100;

            numMonth.Value = System.DateTime.Now.Month;
            numYear.Value = System.DateTime.Now.Year;
            if (System.DateTime.Now.Month <= 1)
            {
                numMonth.Value = 12;
                numYear.Value = (System.DateTime.Now.Year - 1).ToDouble();
            }
            else
            {
                numMonth.Value = System.DateTime.Now.Month - 1;
            }
        }

        void client_CustomGuerdonRecordAccountCompleted(object sender, CustomGuerdonRecordAccountCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == string.Empty)
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CUSTOMGUERDONRECORD"));
                else
                    if (e.Result == "NODATA")
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NODATA"));
                    else
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BUDGETERROR") + ": " + Utility.GetResourceStr(e.Result));
            }
            timer.Stop();
            progressGenerate.Value = progressGenerate.Maximum;
            if (flag)
            {
                timer.Stop();
                progressGenerate.Value = progressGenerate.Minimum;
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
            isSaving = false;
            RefreshUI(RefreshedTypes.ToolBar);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_CustomGuerdonRecordCompleted(object sender, CustomGuerdonRecordCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "CUSTOMGUERDONRECORD"));
            }
            timer.Stop();
            progressGenerate.Value = progressGenerate.Maximum;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            progressGenerate.Value += 1;
            if (progressGenerate.Value >= 100)
                progressGenerate.Value = 1;
        }


        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("CUSTOMGUERDONRECORD");
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

            return items;
        }
        public bool isSaving = false;

        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            if (isSaving == false)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("CREATE"),  //"生成", 
                    ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
                };

                items.Add(item);

            //    item = new ToolbarItem
            //    {
            //        DisplayType = ToolbarItemDisplayTypes.Image,
            //        Key = "1",
            //        Title = Utility.GetResourceStr("SAVEANDCLOSE"),
            //        ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            //    };

            //    items.Add(item);
            }
            return items;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion

        private void Save()
        {
            //this.DialogResult = true;
            timer.Start();
            
            object obj = lkSelectObj.DataContext;
            if (lkSelectObj.TxtLookUp.Text == string.Empty)
            {
                timer.Stop();
                progressGenerate.Value = progressGenerate.Minimum;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("RANGEOFCHOICE"));
                return;
            }
            if (obj == null) 
            {
                return;
            }
            //client.CustomGuerdonRecordAccountAsync(0, "7cd6c0a4-9735-476a-9184-103b962d3383", 2010, 3);
            isSaving = true;
            RefreshUI(RefreshedTypes.ToolBar);
            RefreshUI(RefreshedTypes.ProgressBar);
            if (obj is T_HR_COMPANY)
            {
                client.CustomGuerdonRecordAccountAsync(0, ((T_HR_COMPANY)obj).COMPANYID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());
            }
            else if (obj is T_HR_DEPARTMENT)
            {
                client.CustomGuerdonRecordAccountAsync(1, ((T_HR_DEPARTMENT)obj).DEPARTMENTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());
            }
            else if (obj is T_HR_POST)
            {
                client.CustomGuerdonRecordAccountAsync(2, ((T_HR_POST)obj).POSTID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());
            }
            else if (obj is T_HR_EMPLOYEE)
            {
                client.CustomGuerdonRecordAccountAsync(3, ((T_HR_EMPLOYEE)obj).EMPLOYEEID, (int)numYear.Value, (int)numMonth.Value, GetCreateInfor());
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

        private void Cancel()
        {
            ////this.DialogResult = false;
            flag = true;
            Save();
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
            
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) =>{});
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
