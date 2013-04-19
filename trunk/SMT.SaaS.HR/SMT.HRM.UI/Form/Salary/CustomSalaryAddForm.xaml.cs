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
using SMT.Saas.Tools.OrganizationWS;
using SMT.HRM.UI.Form.Salary;
using System.Collections.ObjectModel;


namespace SMT.HRM.UI.Form.Salary
{
    public partial class CustomSalaryAddForm : BaseForm, IEntityEditor ,IClient
    {
        SalaryServiceClient client;
        private string savesid;
        private T_HR_CUSTOMGUERDON customGuerdon;
        private List<V_CUSTOMGUERDON> customGuerdonlist;

        public string SAVEID
        {
            get { return savesid; }
            set { savesid = value; }
        }
        public CustomSalaryAddForm()
        {
            InitializeComponent();
            InitPara();
        }

        public CustomSalaryAddForm(string SalaryStandardid)
        {
            InitializeComponent();
            InitPara();
            this.SAVEID = SalaryStandardid;
            try
            {
                client.GetCustomGuerdonAsync(SalaryStandardid);
                client.GetSalaryStandardByIDAsync(SalaryStandardid);
            }
            catch { }
        }

        public void InitPara()
        {
            customGuerdon = new T_HR_CUSTOMGUERDON();
            customGuerdonlist = new List<V_CUSTOMGUERDON>();
            try
            {
                client = new SalaryServiceClient();
                client.CustomGuerdonAddCompleted += new EventHandler<CustomGuerdonAddCompletedEventArgs>(client_CustomGuerdonAddCompleted);
                client.GetCustomGuerdonCompleted += new EventHandler<GetCustomGuerdonCompletedEventArgs>(client_GetCustomGuerdonCompleted);
                client.GetSalaryStandardByIDCompleted += new EventHandler<GetSalaryStandardByIDCompletedEventArgs>(client_GetSalaryStandardByIDCompleted);
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
        }

        void client_GetSalaryStandardByIDCompleted(object sender, GetSalaryStandardByIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    txtstandardname.Text = e.Result.SALARYSTANDARDNAME;
                }
            }
            catch { }
        }

        void client_GetCustomGuerdonCompleted(object sender, GetCustomGuerdonCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            customGuerdonlist = e.Result.ToList();
        }

        void client_CustomGuerdonAddCompleted(object sender, CustomGuerdonAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
            }
            if (e.Result == "NOSALARYSTANDARDID")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOSALARYSTANDARDID"));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CUSTOMSALARY", "ADD"));
            }
            RefreshUI(RefreshedTypes.ProgressBar);
            RefreshUI(RefreshedTypes.Close);
        }

        public void Save() 
        {
            RefreshUI(RefreshedTypes.Close);
        }
        public void Cancel() 
        {
            RefreshUI(RefreshedTypes.Close);
        }

        #region 添加,删除,查询

        public string GetTitle()
        {
            return Utility.GetResourceStr("ADDBUTTON");
        }
        public string GetStatus()
        {
            return "";//CustomGuerdon != null ? CustomGuerdon.CREATECOMPANYID : "";
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
                Title = Utility.GetResourceStr("save"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("saveclose"),
                ImageUrl = "/SMT.HRM.UI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);
            items.Clear();

            return items;
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


        void browser_ReloadDataEvent()
        {
            //LoadData(SAVEID);
        }


        #endregion

        #region ----------
        private void lkCustomSalary_FindClick(object sender, EventArgs e)
        {
            string filter = "";
            ObservableCollection<object> paras = new ObservableCollection<object>();
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("GUERDONNAME", "GUERDONNAME");
            cols.Add("GUERDONSUM", "GUERDONSUM");
            cols.Add("UPDATEDATE", "UPDATEDATE");
            filter += "OWNERCOMPANYID==@" + paras.Count().ToString();
            paras.Add(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            LookupForm lookup = new LookupForm(EntityNames.CustomGuerdonSet,
                typeof(List<T_HR_CUSTOMGUERDONSET>), cols, filter, paras);
            lookup.TitleContent = Utility.GetResourceStr("CUSTOMSALARY");
            lookup.SelectedClick += (o, ev) =>
            {
                T_HR_CUSTOMGUERDONSET ent = lookup.SelectedObj as T_HR_CUSTOMGUERDONSET;
                if (!GetExit(ent.GUERDONNAME))
                {
                    if (ent != null)
                    {
                        lkCustomSalary.DataContext = ent;
                        T_HR_SALARYSTANDARD entSALARYSTANDARD = new T_HR_SALARYSTANDARD();
                        entSALARYSTANDARD.SALARYSTANDARDID = SAVEID;
                        customGuerdon.CUSTOMGUERDONID = Guid.NewGuid().ToString();
                        customGuerdon.T_HR_SALARYSTANDARD = entSALARYSTANDARD;
                        customGuerdon.T_HR_CUSTOMGUERDONSET = ent;
                        customGuerdon.SUM = ent.GUERDONSUM;
                        customGuerdon.CREATEDATE = System.DateTime.Now;

                        customGuerdon.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        customGuerdon.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        customGuerdon.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        customGuerdon.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        customGuerdon.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        customGuerdon.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        customGuerdon.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        customGuerdon.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                        client.CustomGuerdonAddAsync(customGuerdon);
                        RefreshUI(RefreshedTypes.ProgressBar);
                    }
                }
                else
                {
                    ent.GUERDONNAME = "";// Utility.GetResourceStr("ALREADYEXISTS");
                    lkCustomSalary.DataContext = ent;
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ALREADYEXISTS"));
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });

        }
        private bool GetExit(string names)
        {
            try
            {
                foreach (V_CUSTOMGUERDON cg in customGuerdonlist)
                {
                    if (cg.GUERDONNAME == names)
                        return true;
                }
            }
            catch
            {
                return true;
            }
            return false;
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
