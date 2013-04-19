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
    public partial class SalaryLevelForm : BaseForm, IEntityEditor,IClient
    {
        public T_HR_SALARYLEVEL salaryLevel { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private string salaryLevelID;
        public SalaryLevelForm(FormTypes type, string ID)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SalaryLevelForm_Loaded);         
            FormType = type;
            salaryLevelID = ID;
            //if (postlevelDistinction != null)
            //{
            //    distinction = postlevelDistinction;
            //    this.DataContext = distinction;
            //    BindCombox();
            //}            
            //InitParas();
            //if (string.IsNullOrEmpty(ID))
            //{
            //    salaryLevel = new T_HR_SALARYLEVEL();
            //    salaryLevel.SALARYLEVELID = Guid.NewGuid().ToString();

            //    this.DataContext = salaryLevel;
            //}
            //else
            //{
            //    client.GetSalaryLevelByIDAsync(ID);
            //}
        }

        void SalaryLevelForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            if (string.IsNullOrEmpty(salaryLevelID))
            {
                salaryLevel = new T_HR_SALARYLEVEL();
                salaryLevel.SALARYLEVELID = Guid.NewGuid().ToString();

                this.DataContext = salaryLevel;
            }
            else
            {
                client.GetSalaryLevelByIDAsync(salaryLevelID);
            }
        }
        void BindCombox()
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dic in cbPostLevel.Items)
            {
                if (dic.DICTIONARYVALUE == salaryLevel.T_HR_POSTLEVELDISTINCTION.POSTLEVEL)
                {
                    cbPostLevel.SelectedItem = dic;
                }
            }
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dic in cbSalaryLevel.Items)
            {
                if (dic.DICTIONARYVALUE.ToString() == salaryLevel.SALARYLEVEL)
                {
                    cbSalaryLevel.SelectedItem = dic;
                }
            }
            cbPostLevel.IsEnabled = false;
            cbSalaryLevel.IsEnabled = false;
        }
        private void InitParas()
        {
         //  client.PostLevelDistinctionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PostLevelDistinctionUpdateCompleted);
            client.GetSalaryLevelByIDCompleted += new EventHandler<GetSalaryLevelByIDCompletedEventArgs>(client_GetSalaryLevelByIDCompleted);
            client.SalaryLevelUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryLevelUpdateCompleted);
            client.SalaryLevelADDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalaryLevelADDCompleted);
        }

        void client_SalaryLevelADDCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYLEVEL"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_SalaryLevelUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYLEVEL"));
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_GetSalaryLevelByIDCompleted(object sender, GetSalaryLevelByIDCompletedEventArgs e)
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

                salaryLevel = e.Result;
                this.DataContext = salaryLevel;
                BindCombox();

            }
        }

        //void client_PostLevelDistinctionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
        //    if (e.Error != null)
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "POSTLEVEL"));
        //    }
        //    RefreshUI(RefreshedTypes.ProgressBar);
        //}
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYLEVEL");
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
            //ValidationHelper.ValidateProperty<T_HR_EMPLOYEE>(Employee);
            RefreshUI(RefreshedTypes.ProgressBar);
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

            if (cbPostLevel.Items == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }

            if (cbPostLevel.Items.Count == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }

            T_SYS_DICTIONARY curDic = cbPostLevel.SelectedItem as T_SYS_DICTIONARY;
            if (curDic == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }

            if (curDic.DICTIONARYVALUE == null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
       
            //salaryLevel.T_HR_POSTLEVELDISTINCTION.POSTLEVELID = cbPostLevel.SelectedValue
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
                    salaryLevel.UPDATEDATE = System.DateTime.Now;
                    salaryLevel.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    client.SalaryLevelUpdateAsync(salaryLevel);
                }
                else
                {
                    salaryLevel.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    salaryLevel.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    salaryLevel.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    salaryLevel.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    salaryLevel.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    salaryLevel.SALARYLEVEL = (cbSalaryLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
                    salaryLevel.T_HR_POSTLEVELDISTINCTION = new T_HR_POSTLEVELDISTINCTION();
                    salaryLevel.T_HR_POSTLEVELDISTINCTION.POSTLEVELID = Guid.NewGuid().ToString();
                    salaryLevel.T_HR_POSTLEVELDISTINCTION.POSTLEVEL = (cbPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE;
                    client.SalaryLevelADDAsync(salaryLevel);
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

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
