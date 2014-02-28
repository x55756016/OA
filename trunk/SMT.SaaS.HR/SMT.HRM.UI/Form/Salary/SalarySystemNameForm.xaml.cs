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
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.HRM.UI.Form.Salary
{
    public partial class SalarySystemNameForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        SalaryServiceClient client = new SalaryServiceClient();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private PermissionServiceClient permissionClient = new PermissionServiceClient();
        private ObservableCollection<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> postLevelDicts;
        private ObservableCollection<T_HR_POSTLEVELDISTINCTION> postLevels;
        private bool auditsign = false;
        FormTypes FormType { get; set; }
        T_HR_SALARYSYSTEM salarySystem { get; set; }
        string salarySystemID { get; set; }
        private bool isNew;//true 从快捷方式新建单据
        public SalarySystemNameForm()
        {
            if (!Application.Current.Resources.Contains("DictionaryConverter"))
            {
                Application.Current.Resources.Add("DictionaryConverter", new SMT.HRM.UI.DictionaryConverter());
            }

            InitializeComponent();
            InitParas();
            isNew = true;
            FormType = FormTypes.New;
            salarySystemID = string.Empty;
            initEvent();

        }
        public SalarySystemNameForm(FormTypes type, string ID)
        {
            InitializeComponent();
            FormType = type;
            salarySystemID = ID;
            initEvent();

        }
        public void initEvent()
        {
            InitParas();
            if (string.IsNullOrEmpty(salarySystemID))
            {
                salarySystem = new T_HR_SALARYSYSTEM();
                salarySystem.SALARYSYSTEMID = Guid.NewGuid().ToString();
                salarySystem.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                salarySystem.OWNERCOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                salarySystem.OWNERDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                salarySystem.OWNERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                salarySystem.OWNERPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                this.DataContext = salarySystem;
                SetToolBar();
            }
            else
            {
                NotShow(FormType);
                client.GetSalarySystemByIDAsync(salarySystemID);
            }
            permissionClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
        }
        private void NotShow(FormTypes type)
        {
            if (type != FormTypes.Edit && type != FormTypes.Resubmit)
            {
                txtSalarySystemName.IsEnabled = false;
                txtRemark.IsEnabled = false;
                DtGridPostDis.IsEnabled = false;
            }
        }

        void InitParas()
        {
            client.SalarySystemAddCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySystemAddCompleted);
            client.SalarySystemUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_SalarySystemUpdateCompleted);
            client.GetSalarySystemByIDCompleted += new EventHandler<GetSalarySystemByIDCompletedEventArgs>(client_GetSalarySystemByIDCompleted);

            client.GetPostLevelDistinctionBySystemIDCompleted += new EventHandler<GetPostLevelDistinctionBySystemIDCompletedEventArgs>(client_GetPostLevelDistinctionBySystemIDCompleted);
            client.PostLevelDistinctionUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PostLevelDistinctionUpdateCompleted);
            permissionClient.GetSysDictionaryByCategoryCompleted += new EventHandler<GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            client.SalarySystemDeleteCompleted += new EventHandler<SalarySystemDeleteCompletedEventArgs>(client_SalarySystemDeleteCompleted);
        }

        void client_SalarySystemDeleteCompleted(object sender, SalarySystemDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESSED", "SALARYSYSTEM"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                CloseForm();
            }
            FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void CloseForm()
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }

        #region 岗位工资和级差额
        /// <summary>
        /// 生成岗位工资和级差额完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_PostLevelDistinctionUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (auditsign)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "SALARYSYSTEM"));
                    auditsign = false;
                }
                else
                {
                    if (e.UserState.ToString() == "ADD")
                    {
                        FormType = FormTypes.Edit;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.FormType = FormTypes.Edit;
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("ADDDATASUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSYSTEM"));
                        RefreshUI(RefreshedTypes.AuditInfo);
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYSYSTEM"));
                    }
                    //添加删除按钮
                    ToolbarItems = Utility.CreateFormEditButton();
                    ToolbarItems.Add(ToolBarItems.Delete);
                    RefreshUI(RefreshedTypes.All);
                }
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 获取所有岗位级别
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, GetSysDictionaryByCategoryCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            else
            {
                postLevelDicts = e.Result;
                //获取已设置的岗位
                client.GetPostLevelDistinctionBySystemIDAsync(salarySystemID);
            }
        }
        /// <summary>
        /// 获取所有的岗位薪资 和级差额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetPostLevelDistinctionBySystemIDCompleted(object sender, GetPostLevelDistinctionBySystemIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            else
            {
                postLevels = e.Result;
                if (postLevels != null)
                {
                    foreach (var dict in postLevelDicts)
                    {
                        if (!IsLevelAdded(dict))
                        {
                            T_HR_POSTLEVELDISTINCTION level = new T_HR_POSTLEVELDISTINCTION();
                            level.POSTLEVEL = Convert.ToDecimal(dict.DICTIONARYVALUE);
                            level.POSTLEVELID = Guid.NewGuid().ToString();
                            level.T_HR_SALARYSYSTEM = new T_HR_SALARYSYSTEM();
                            level.T_HR_SALARYSYSTEM.SALARYSYSTEMID = salarySystem.SALARYSYSTEMID;
                            postLevels.Add(level);
                        }
                    }
                }
                else
                {
                    postLevels = new ObservableCollection<T_HR_POSTLEVELDISTINCTION>();
                    foreach (var dict in postLevelDicts)
                    {

                        T_HR_POSTLEVELDISTINCTION level = new T_HR_POSTLEVELDISTINCTION();
                        level.POSTLEVEL = Convert.ToDecimal(dict.DICTIONARYVALUE);
                        level.POSTLEVELID = Guid.NewGuid().ToString();
                        level.T_HR_SALARYSYSTEM = new T_HR_SALARYSYSTEM();
                        level.T_HR_SALARYSYSTEM.SALARYSYSTEMID = salarySystem.SALARYSYSTEMID;
                        postLevels.Add(level);

                    }
                }


                DtGridPostDis.ItemsSource = postLevels.OrderBy(c => c.POSTLEVEL);
            }

            //client.GetAllSalaryLevelAsync();

        }
        private bool IsLevelAdded(SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY dict)
        {
            bool rslt = false;

            var ents = from p in postLevels
                       where p.POSTLEVEL == Convert.ToDecimal(dict.DICTIONARYVALUE)
                       select p;
            rslt = ents.Count() > 0;
            return rslt;
        }
        #endregion
        #region 薪资体系增删改
        void client_GetSalarySystemByIDCompleted(object sender, GetSalarySystemByIDCompletedEventArgs e)
        {
            if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //if (e.Result == null)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                //    return;
                //}
                salarySystem = e.Result;
                if (FormType == FormTypes.Resubmit)
                {
                    salarySystem.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                }
                this.DataContext = salarySystem;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
            }
        }

        void client_SalarySystemUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SALARYSYSTEM"));
                client.PostLevelDistinctionUpdateAsync(postLevels, "UPDATE");
            }

            RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_SalarySystemAddCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //if (e.Result == "SUCCESSED")
                //{
                // Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "SALARYSYSTEM"));
                //}
                //else
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                //}
                client.PostLevelDistinctionUpdateAsync(postLevels, "ADD");
                
            }
            //  RefreshUI(RefreshedTypes.All);
            RefreshUI(RefreshedTypes.ProgressBar);
        }
        #endregion
        bool Save()
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (string.IsNullOrEmpty(txtSalarySystemName.Text.Trim()))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "SALARYSYSTEMNAME"));
                RefreshUI(RefreshedTypes.ProgressBar);
                return false;
            }

            if (FormType == FormTypes.Edit || FormType == FormTypes.Resubmit)
            {
                salarySystem.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                salarySystem.UPDATEDATE = System.DateTime.Now;
                client.SalarySystemUpdateAsync(salarySystem);
            }
            else
            {
                salarySystem.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                salarySystem.CREATEDATE = System.DateTime.Now;
                salarySystemID = salarySystem.SALARYSYSTEMID;
                client.SalarySystemAddAsync(salarySystem);
            }
            return true;
        }
        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_SALARYSYSTEM", salarySystem.OWNERID,
                    salarySystem.OWNERPOSTID, salarySystem.OWNERDEPARTMENTID, salarySystem.OWNERCOMPANYID);

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
                if (salarySystem != null)
                {
                    if (salarySystem.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
                //ToolbarItems.Add(ToolBarItems.Delete);
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_SALARYSYSTEM", salarySystem.OWNERID,
                    salarySystem.OWNERPOSTID, salarySystem.OWNERDEPARTMENTID, salarySystem.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
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

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("SALARYSYSTEM");
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
                case "Delete":
                    //删除薪资体系设置
                    delete(salarySystemID);
                    break;
            }

        }

        //删除薪资体系设置
        public void delete(string strID)
        {
            string Result = "";
            //提示是否删除
            ComfirmWindow com = new ComfirmWindow();
            ObservableCollection<string> ids = new ObservableCollection<string>();
            ids.Add(strID);
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                client.SalarySystemDeleteAsync(ids);
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
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
            if (isNew)
            {
                ToolbarItems = Utility.CreateFormSaveButton();
            }
            if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                if (salarySystem != null)
                {
                    if (salarySystem.CHECKSTATE == "1" && FormType == FormTypes.Edit)
                    {
                        ToolbarItems = new List<ToolbarItem>();
                    }
                }
            }
            return ToolbarItems;
        }

        public event UIRefreshedHandler OnUIRefreshed;
        #endregion
        #region IAudit
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            //string strXmlObjectSource = string.Empty;
            //strXmlObjectSource = Utility.ObjListToXml<T_HR_SALARYSYSTEM>(salarySystem, "HR");
            //Utility.SetAuditEntity(entity, "T_HR_SALARYSYSTEM", salarySystem.SALARYSYSTEMID, strXmlObjectSource);

            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("SALARYSYSTEMNAME", salarySystem.SALARYSYSTEMNAME);
            para.Add("EntityKey", salarySystem.SALARYSYSTEMID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();

            string strXmlObjectSource = string.Empty;
            string strKeyName = "SALARYSYSTEMID";
            string strKeyValue = salarySystem.SALARYSYSTEMID;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_SALARYSYSTEM>(salarySystem, para, "HR", para2, strKeyName, strKeyValue);



            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", salarySystem.CREATEUSERID);
            paraIDs.Add("CreatePostID", salarySystem.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", salarySystem.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", salarySystem.OWNERCOMPANYID);

            if (salarySystem.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_SALARYSYSTEM", salarySystem.SALARYSYSTEMID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_SALARYSYSTEM", salarySystem.SALARYSYSTEMID, strXmlObjectSource);
            }

        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            // RefreshUI(RefreshedTypes.ProgressBar);
            auditsign = true;
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
            salarySystem.CHECKSTATE = state;
            salarySystem.EDITSTATE = Convert.ToInt32(EditStates.Actived).ToString();
            client.SalarySystemUpdateAsync(salarySystem);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (salarySystem != null)
                state = salarySystem.CHECKSTATE;

            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {

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
    }
}
