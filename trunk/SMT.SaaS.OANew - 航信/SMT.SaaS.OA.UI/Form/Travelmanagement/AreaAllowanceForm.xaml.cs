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
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class AreaAllowanceForm : BaseForm, IEntityEditor, IClient
    {
        public AreaAllowanceForm()
        {
            InitializeComponent();
        }

        public T_OA_AREAALLOWANCE allowance { get; set; }
        public FormTypes FormType { get; set; }
        private RefreshedTypes actiontype = RefreshedTypes.CloseAndReloadData;
        public string PostLevel { get; set; }
        private SmtOAPersonOfficeClient client = new SmtOAPersonOfficeClient();
        private string areaID { get; set; }
        private string travelSId { get; set; }
        private T_OA_TRAVELSOLUTIONS solutionsObj = new T_OA_TRAVELSOLUTIONS();
        public AreaAllowanceForm(FormTypes type, string allowanceID, string areaID, string travelSolutionsId, string postlevel)
        {
            InitializeComponent();
            travelSId = travelSolutionsId;
            this.Loaded += (sender, agrs) =>
            {
                #region 原来的
                InitParas();
                this.areaID = areaID;
                FormType = type;
                PostLevel = postlevel;
                LoadSolutionInfos();
                if (string.IsNullOrEmpty(allowanceID))
                {
                    allowance = new T_OA_AREAALLOWANCE();
                    allowance.AREAALLOWANCEID = Guid.NewGuid().ToString();
                    allowance.T_OA_AREADIFFERENCE = new T_OA_AREADIFFERENCE();
                    allowance.T_OA_AREADIFFERENCE.AREADIFFERENCEID = areaID;
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
                    client.GetAreaAllowanceByIDAsync(allowanceID);
                }
                #endregion
            };
        }

        void InitParas()
        {
            client.GetAreaAllowanceByIDCompleted += new EventHandler<GetAreaAllowanceByIDCompletedEventArgs>(client_GetAreaAllowanceByIDCompleted);
            client.AreaAllowanceADDCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaAllowanceADDCompleted);
            client.AreaAllowanceUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_AreaAllowanceUpdateCompleted);
            client.GetAreaCategoryCompleted += new EventHandler<GetAreaCategoryCompletedEventArgs>(client_GetAreaCategoryCompleted);
            client.GetTravelSolutionFlowCompleted += new EventHandler<GetTravelSolutionFlowCompletedEventArgs>(client_GetTravelSolutionFlowCompleted);
        }

        void client_GetTravelSolutionFlowCompleted(object sender, GetTravelSolutionFlowCompletedEventArgs e)
        {

            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result.ToList(), e.pageCount);
                }
                else
                {
                    BindDataGrid(null, 0);
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        private void BindDataGrid(List<T_OA_TRAVELSOLUTIONS> obj, int pageCount)
        {

            if (obj == null || obj.Count < 1)
            {
                this.cmbSolution.ItemsSource = null;
                return;
            }
            cmbSolution.ItemsSource = obj;
            cmbSolution.DisplayMemberPath = "PROGRAMMENAME";
            foreach (T_OA_TRAVELSOLUTIONS Region in cmbSolution.Items)
            {
                if (Region.OWNERCOMPANYID == Common.CurrentLoginUserInfo.UserPosts[0].CompanyID)
                {
                    cmbSolution.SelectedItem = Region;
                }
                else
                {
                    cmbSolution.SelectedIndex = 0;
                }
                solutionsObj = Region;
            }
        }

        void BindCommbox()
        {
            if (allowance != null)
            {
                if (allowance.T_OA_AREADIFFERENCE != null && !string.IsNullOrEmpty(allowance.T_OA_AREADIFFERENCE.AREADIFFERENCEID))
                {
                    foreach (T_OA_AREADIFFERENCE area in cbArea.Items)
                    {

                        if (area.AREADIFFERENCEID == allowance.T_OA_AREADIFFERENCE.AREADIFFERENCEID)
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
                if (GlobalFunction.IsSaveAndClose(actiontype))
                {
                    RefreshUI(actiontype);
                }
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
                if (GlobalFunction.IsSaveAndClose(actiontype))
                {
                    RefreshUI(actiontype);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
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
                    allowance = new T_OA_AREAALLOWANCE();
                    allowance.AREAALLOWANCEID = Guid.NewGuid().ToString();
                    allowance.T_OA_AREADIFFERENCE = new T_OA_AREADIFFERENCE();
                    allowance.T_OA_AREADIFFERENCE.AREADIFFERENCEID = areaID;
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
            return Utility.GetResourceStr("TRAVELAREAALLOWANCE");
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
                    actiontype = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    actiontype = RefreshedTypes.CloseAndReloadData;
                    Save();
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


            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

            item = new ToolbarItem
           {
               DisplayType = ToolbarItemDisplayTypes.Image,
               Key = "0",
               Title = Utility.GetResourceStr("SAVE"),
               ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
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

            if (postLevel == null || postLevel.DICTIONARYNAME == "空")
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }
            allowance.T_OA_AREADIFFERENCE = cbArea.SelectedItem as T_OA_AREADIFFERENCE;
            allowance.T_OA_TRAVELSOLUTIONS = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
            allowance.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID = travelSId;
            allowance.T_OA_AREADIFFERENCE.AREADIFFERENCEID = (cbArea.SelectedItem as T_OA_AREADIFFERENCE).AREADIFFERENCEID;
            allowance.POSTLEVEL = (cbPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYVALUE.ToString();
            allowance.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            allowance.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            allowance.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            allowance.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            if (FormType == FormTypes.Edit)
            {
                allowance.UPDATEDATE = System.DateTime.Now;
                allowance.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.AreaAllowanceUpdateAsync(allowance);
            }
            else
            {
                client.AreaAllowanceADDAsync(allowance, travelSId);

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

        #region 加载数据
        void LoadSolutionInfos()
        {
            int pageCount = 0;
            string filter = "";    //查询过滤条件

            ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值

            SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo loginUserInfo = new SMT.SaaS.OA.UI.SmtOAPersonOfficeService.LoginUserInfo();
            loginUserInfo.companyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            loginUserInfo.userID = Common.CurrentLoginUserInfo.EmployeeID;
            client.GetTravelSolutionFlowAsync(0, 100, "CREATEDATE descending", filter, paras, pageCount, loginUserInfo);
        }
        #endregion

        #region 出差方案选择
        private void cmbSolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            solutionsObj = cmbSolution.SelectedItem as T_OA_TRAVELSOLUTIONS;
            if (solutionsObj != null)
            {
                client.GetQueryProgramSubsidiesAsync(solutionsObj.TRAVELSOLUTIONSID);
            }
        }
        #endregion
    }
}
