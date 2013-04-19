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
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class AreaAllowanceForm : BaseForm, IEntityEditor,IClient
    {
        public T_HR_AREAALLOWANCE allowance { get; set; }
        public FormTypes FormType { get; set; }
        public string PostLevel { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private string areaID { get; set; }
        public string AllowancelID { get; set; }
        public AreaAllowanceForm(FormTypes type, string allowanceID, string areaID, string postlevel)
        {
            InitializeComponent();
            InitParas();
            this.areaID = areaID;
            FormType = type;
            PostLevel = postlevel;
            AllowancelID = allowanceID;
            this.Loaded += new RoutedEventHandler(AreaAllowanceForm_Loaded);
            //if (string.IsNullOrEmpty(allowanceID))
            //{
            //    allowance = new T_HR_AREAALLOWANCE();
            //    allowance.AREAALLOWANCEID = Guid.NewGuid().ToString();
            //    allowance.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
            //    allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID = areaID;
            //    allowance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    allowance.CREATEDATE = System.DateTime.Now;

            //    //areacity.UPDATEDATE = System.DateTime.Now;
            //    //areacity.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    cbArea.IsEnabled = false;
            //    this.DataContext = allowance;
            //}
            //else
            //{
            //    cbArea.IsEnabled = false;
            //    cbPostLevel.IsEnabled = false;
            //    client.GetAreaAllowanceByIDAsync(allowanceID);
            //}
        }

        void AreaAllowanceForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AllowancelID))
            {
                allowance = new T_HR_AREAALLOWANCE();
                allowance.AREAALLOWANCEID = Guid.NewGuid().ToString();
                allowance.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
                allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID = areaID;
                allowance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                allowance.CREATEDATE = System.DateTime.Now;

                //areacity.UPDATEDATE = System.DateTime.Now;
                //areacity.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                cbArea.IsEnabled = false;
                this.DataContext = allowance;
            }
            else
            {
                cbArea.IsEnabled = false;
                cbPostLevel.IsEnabled = false;
                client.GetAreaAllowanceByIDAsync(AllowancelID);
            }
        }

        void InitParas()
        {
            client.GetAreaAllowanceByIDCompleted += new EventHandler<GetAreaAllowanceByIDCompletedEventArgs>(client_GetAreaAllowanceByIDCompleted);
            client.AreaAllowanceADDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaAllowanceADDCompleted);
            client.AreaAllowanceUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaAllowanceUpdateCompleted);
            client.GetAreaCategoryCompleted += new EventHandler<GetAreaCategoryCompletedEventArgs>(client_GetAreaCategoryCompleted);
        }
        void BindCommbox()
        {
            if (allowance != null)
            {
                if (allowance.T_HR_AREADIFFERENCE != null && !string.IsNullOrEmpty(allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID))
                {
                    foreach (T_HR_AREADIFFERENCE area in cbArea.Items)
                    {

                        if (area.AREADIFFERENCEID == allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID)
                        {
                            cbArea.SelectedItem = area;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(PostLevel))
            {
                foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postlevel in cbPostLevel.Items)
                {
                    if (postlevel.DICTIONARYNAME == PostLevel)
                    {
                        cbPostLevel.SelectedItem = postlevel;
                        break;
                    }
                }
            }

        }
        void client_GetAreaCategoryCompleted(object sender, GetAreaCategoryCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result != null)
                {
                    cbArea.ItemsSource = e.Result.ToList();
                    BindCommbox();
                }

            }
        }

        void client_AreaAllowanceUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "AREAALLOWANCE"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_AreaAllowanceADDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "AREAALLOWANCE"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetAreaAllowanceByIDCompleted(object sender, GetAreaAllowanceByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == null)
                {
                    allowance = new T_HR_AREAALLOWANCE();
                    allowance.AREAALLOWANCEID = Guid.NewGuid().ToString();
                    allowance.T_HR_AREADIFFERENCE = new T_HR_AREADIFFERENCE();
                    allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID = areaID;
                    allowance.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    allowance.CREATEDATE = System.DateTime.Now;
                    FormType = FormTypes.New;
                }
                else
                {
                    allowance = e.Result;
                }
                this.DataContext = allowance;
                BindCommbox();
            }
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("AREAALLOWANCE");
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

        public bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY postLevel = cbPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

            //if (validators.Count > 0)
            //{
            //    //could use the content of the list to show an invalid message summary somehow
            //    //MessageBox.Show(validators.Count.ToString() + " invalid validators");
            //}
            //else
            //{
            if (postLevel == null || postLevel.DICTIONARYNAME == "空")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            allowance.T_HR_AREADIFFERENCE.AREADIFFERENCEID = (cbArea.SelectedItem as T_HR_AREADIFFERENCE).AREADIFFERENCEID;
            allowance.POSTLEVEL = (cbPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
            if (FormType == FormTypes.Edit)
            {
                allowance.UPDATEDATE = System.DateTime.Now;
                allowance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.AreaAllowanceUpdateAsync(allowance);
            }
            else
            {
                client.AreaAllowanceADDAsync(allowance);

            }
            return true;
            //}
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

        private void cbArea_Loaded(object sender, RoutedEventArgs e)
        {
            client.GetAreaCategoryAsync();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
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
