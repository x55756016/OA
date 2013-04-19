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
    public partial class PostSalaryForm : BaseForm, IEntityEditor,IClient
    {
        public T_HR_POSTLEVELDISTINCTION distinction { get; set; }
        public FormTypes FormType { get; set; }
        private SalaryServiceClient client = new SalaryServiceClient();
        private ObservableCollection<T_HR_POSTLEVELDISTINCTION> postLevels = new ObservableCollection<T_HR_POSTLEVELDISTINCTION>();
        private string postSalaryID;
        public PostSalaryForm(FormTypes type, string id)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PostSalaryForm_Loaded);         
            FormType = type;
            postSalaryID = id;
            //InitParas();
            //if (!string.IsNullOrEmpty(id))
            //{
                
            //    client.GetPostLevelDistinctionByIDAsync(id);
            //}
            //else
            //{
            //    distinction = new T_HR_POSTLEVELDISTINCTION();
            //    distinction.POSTLEVELID = Guid.NewGuid().ToString();
            //    this.DataContext = distinction;
            //}

            //if (string.IsNullOrEmpty(ID))
            //{
            //    area = new T_HR_AREADIFFERENCE();
            //    area.AREADIFFERENCEID = Guid.NewGuid().ToString();
            //    area.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
            //    area.CREATEDATE = System.DateTime.Now;

            //    area.UPDATEDATE = System.DateTime.Now;
            //    area.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            //    this.DataContext = area;
            //}
            //else
            //{
            //    client.GetAreaCategoryByIDAsync(areaID);
            //}
        }

        void PostSalaryForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitParas();
            if (!string.IsNullOrEmpty(postSalaryID))
            {

                client.GetPostLevelDistinctionByIDAsync(postSalaryID);
            }
            else
            {
                distinction = new T_HR_POSTLEVELDISTINCTION();
                distinction.POSTLEVELID = Guid.NewGuid().ToString();
                this.DataContext = distinction;
            }
        }
        void BindCombox()
        {
            foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dic in cbPostLevel.Items)
            {
                if (dic.DICTIONARYVALUE == distinction.POSTLEVEL)
                {
                    cbPostLevel.SelectedItem = dic;
                }
            }
        }
        private void InitParas()
        {
            client.PostLevelDistinctionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PostLevelDistinctionUpdateCompleted);
            client.PostLevelDistinctionADDCompleted += new EventHandler<PostLevelDistinctionADDCompletedEventArgs>(client_PostLevelDistinctionADDCompleted);
            client.GetPostLevelDistinctionByIDCompleted += new EventHandler<GetPostLevelDistinctionByIDCompletedEventArgs>(client_GetPostLevelDistinctionByIDCompleted);
        }

        void client_GetPostLevelDistinctionByIDCompleted(object sender, GetPostLevelDistinctionByIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                distinction = e.Result;
                this.DataContext = distinction;
                BindCombox();
                cbPostLevel.IsEnabled = false;
            }
        }

        void client_PostLevelDistinctionADDCompleted(object sender, PostLevelDistinctionADDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                if (e.Result == "SUCCESSED")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "POSTLEVEL"));
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                }

            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }

        void client_PostLevelDistinctionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "POSTLEVEL"));
            }
            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("POSTSALARY");
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
            if (cbPostLevel.SelectedIndex <= 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
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
                    distinction.UPDATEDATE = System.DateTime.Now;
                    distinction.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    postLevels.Add(distinction);
                    client.PostLevelDistinctionUpdateAsync(postLevels);
                }
                else
                {
                    distinction.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    distinction.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    distinction.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    distinction.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    distinction.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    distinction.POSTLEVEL = (cbPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE;
                    client.PostLevelDistinctionADDAsync(distinction);
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
