/// <summary>
/// Log No.： 1
/// Modify Desc： XAML进行资源加载
/// Modifier： 冉龙军
/// Modify Date： 2010-09-09
/// </summary>
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

using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.HRM.UI.Form.Personnel
{
    public partial class PensionMasterForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        #region 初始化
        public string createUserName;
        private T_HR_PENSIONMASTER pensionMaster;

        public T_HR_PENSIONMASTER PensionMaster
        {
            get { return pensionMaster; }
            set
            {
                pensionMaster = value;
                this.DataContext = value;
            }
        }
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private FormTypes formType;

        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }
        private string pensionMasterID;
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        public bool needsubmit = false;//提交审核,用于判断是否需要调用提交方法
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        PersonnelServiceClient client;

        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建  zwp 2011.10.09
        /// </summary>
        public PensionMasterForm()
        {
            InitializeComponent();
            this.pensionMasterID = "";
            FormType = FormTypes.New;
            InitParas(pensionMasterID);

            //pensionMaster.SOCIALSERVICE;
        }

        public PensionMasterForm(FormTypes type, string pensionMasterID)
        {
            InitializeComponent();
            this.pensionMasterID = pensionMasterID;
            FormType = type;
            InitParas(pensionMasterID);
        }

        private void InitParas(string pensionMasterID)
        {
            client = new PersonnelServiceClient();
            client.GetPensionMasterByIDCompleted += new EventHandler<GetPensionMasterByIDCompletedEventArgs>(client_GetPensionMasterByIDCompleted);
            client.PensionMasterAddCompleted += new EventHandler<PensionMasterAddCompletedEventArgs>(client_PensionMasterAddCompleted);
            client.PensionMasterUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(client_PensionMasterUpdateCompleted);
            client.PensionMasterDeleteCompleted += new EventHandler<PensionMasterDeleteCompletedEventArgs>(client_PensionMasterDeleteCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            this.Loaded += new RoutedEventHandler(PensionMasterForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            */
            #endregion
        }

        void client_PensionMasterDeleteCompleted(object sender, PensionMasterDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
            FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
        }

        void PensionMasterForm_Loaded(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            #region 新增
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                PensionMaster = new T_HR_PENSIONMASTER();
                pensionMaster.PENSIONMASTERID = Guid.NewGuid().ToString();
                PensionMaster.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                lkEmployeeName.IsEnabled = false;
                client.GetPensionMasterByIDAsync(pensionMasterID);
            }
        }

        //by luojie 
        //工具栏提交按钮的重载方法
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            needsubmit = true;
            isSubmit = true;

            Save();
        }


        #endregion

        private void SetToolBar()
        {
            if (FormType == FormTypes.Browse) return;
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_PENSIONMASTER", PensionMaster.OWNERID,
                    PensionMaster.OWNERPOSTID, PensionMaster.OWNERDEPARTMENTID, PensionMaster.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (PensionMaster != null)
                {
                    if (PensionMaster.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else
                ToolbarItems = Utility.CreateFormEditButton("T_HR_PENSIONMASTER", PensionMaster.OWNERID,
                    PensionMaster.OWNERPOSTID, PensionMaster.OWNERDEPARTMENTID, PensionMaster.OWNERCOMPANYID);

            RefreshUI(RefreshedTypes.All);
        }
        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("PENSIONMASTER");
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
                    closeFormFlag = true;
                    Save();
                    // Cancel();
                    break;
                case "Delete":
                    Delete(pensionMaster.PENSIONMASTERID);
                    break;
            }
        }

        private void Delete(string id)
        {
            string Result = "";
            string strMsg = string.Empty;
            //提示是否删除
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                client.PensionMasterDeleteAsync(new System.Collections.ObjectModel.ObservableCollection<string>(new List<string>() { id }));
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("确定要删除吗？"), ComfirmWindow.titlename, Result);
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

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            //List<ToolbarItem> items = new List<ToolbarItem>();
            //if (FormType != FormTypes.Browse)
            //{
            //    items = Utility.CreateFormSaveButton();
            //}

            //return items;
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            else
                ToolbarItems = Utility.CreateFormSaveButton("T_HR_PENSIONMASTER", PensionMaster.OWNERID,
                    PensionMaster.OWNERPOSTID, PensionMaster.OWNERDEPARTMENTID, PensionMaster.OWNERCOMPANYID);

            if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (PensionMaster != null)
                {
                    if (PensionMaster.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else
            {
                if (PensionMaster != null)
                {
                    ToolbarItems = Utility.CreateFormEditButton("T_HR_PENSIONMASTER", PensionMaster.OWNERID,
                        PensionMaster.OWNERPOSTID, PensionMaster.OWNERDEPARTMENTID, PensionMaster.OWNERCOMPANYID);
                }
            }
            if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            return ToolbarItems;
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
        #endregion

        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IAudit
        
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Post.OWNERID)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            // Utility.SetAuditEntity(entity, "T_HR_PENSIONMASTER", PensionMaster.PENSIONMASTERID);
            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", PensionMaster.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", PensionMaster.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", PensionMaster.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EntityKey", PensionMaster.PENSIONMASTERID);

            Dictionary<string, string> para2 = new Dictionary<string, string>();
            para2.Add("ISVALID", (cbxIsValid.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY) == null ? "" : (cbxIsValid.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY).DICTIONARYNAME);
            para2.Add("T_HR_EMPLOYEEReference", PensionMaster.T_HR_EMPLOYEE == null ? "" : PensionMaster.T_HR_EMPLOYEE.EMPLOYEEID + "#" + PensionMaster.T_HR_EMPLOYEE.EMPLOYEECNAME);


            string strXmlObjectSource = string.Empty;
            string strKeyName = "PENSIONMASTERID";
            string strKeyValue = PensionMaster.PENSIONMASTERID;
            strXmlObjectSource = Utility.ObjListToXml<T_HR_PENSIONMASTER>(PensionMaster, para, "HR", para2, strKeyName, strKeyValue);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", PensionMaster.OWNERID);
            paraIDs.Add("CreatePostID", PensionMaster.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", PensionMaster.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", PensionMaster.OWNERCOMPANYID);

            if (PensionMaster.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                //Utility.SetAuditEntity(entity, "T_HR_PENSIONMASTER", PensionMaster.PENSIONMASTERID, strXmlObjectSource, PensionMaster.T_HR_EMPLOYEE.EMPLOYEEID);
                Utility.SetAuditEntity(entity, "T_HR_PENSIONMASTER", PensionMaster.PENSIONMASTERID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_PENSIONMASTER", PensionMaster.PENSIONMASTERID, strXmlObjectSource);
            }
        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            // "SUBMITSUCCESSED"
            //  Utility.UpdateCheckState("T_HR_PENSIONMASTER", "PENSIONMASTERID", PensionMaster.PENSIONMASTERID, args);
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    pensionMaster.ISVALID = "1";
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (pensionMaster.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            PensionMaster.CHECKSTATE = state;

            if (FormType == FormTypes.Edit)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            else if (FormType == FormTypes.Audit)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
                
               
            //client.PensionMasterUpdateAsync(PensionMaster, UserState);
        }
        public string GetAuditState()
        {
            string state = "-1";
            if (PensionMaster != null)
                state = PensionMaster.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region 保存
        private bool Save()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (string.IsNullOrEmpty(lkEmployeeName.TxtLookUp.Text.Trim()))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                isSubmit = false;
                return false;
            }
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                isSubmit = false;
                return false;
            }
            else
            {
                string strMsg = "";
                if (FormType == FormTypes.New)
                {
                    PensionMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    PensionMaster.CREATEDATE = DateTime.Now;
                    client.PensionMasterAddAsync(PensionMaster, strMsg);
                }
                else
                {
                    PensionMaster.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    PensionMaster.UPDATEDATE = DateTime.Now;

                    client.PensionMasterUpdateAsync(PensionMaster, "Edit");
                }
            }
            return true;
        }
        #endregion

        #region 完成事件
        /// <summary>
        /// 获取创建人信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeToEngineCompleted(object sender, GetEmployeeToEngineCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    createUserName = e.Result[0].EMPLOYEENAME;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        void client_GetPensionMasterByIDCompleted(object sender, GetPensionMasterByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != null)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            PensionMaster = e.Result;
            if (FormType == FormTypes.Resubmit)
            {
                PensionMaster.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                PensionMaster.ISVALID = string.Empty;
            }
            if (e.Result.CHECKSTATE != Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                this.IsEnabled = false;
            }
            if (!string.IsNullOrEmpty(PensionMaster.CREATEUSERID))
            {
                // client.GetEmployeeByIDAsync(PensionMaster.CREATEUSERID);
                if (PensionMaster.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || PensionMaster.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    SetToolBar();
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                    CreateUserIDs.Add(PensionMaster.CREATEUSERID);
                    client.GetEmployeeToEngineAsync(CreateUserIDs);
                }
            }
            else
            {
                PensionMaster.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();
                RefreshUI(RefreshedTypes.HideProgressBar);
            }

        }
        /// <summary>
        /// 新增社保
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_PensionMasterAddCompleted(object sender, PensionMasterAddCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ADDERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                if (!isSubmit)
                {
                    isSubmit = false;
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                    ToolbarItems = Utility.CreateFormEditButton();
                    ToolbarItems.Add(ToolBarItems.Delete);
                }
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                toSubmit();
                RefreshUI(RefreshedTypes.All);
            }
            //this.Close();
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        /// 提交
        /// </summary>
        private void toSubmit()
        {
            ///by luojie 转至提交按钮原来的方法
            if (needsubmit)
            {
                try
                {

                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.ManualSubmit();
                    BackToSubmit();
                }
                catch (Exception ex)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                }

            }
            if (closeFormFlag)
            {
                RefreshUI(RefreshedTypes.Close);
            }
            RefreshUI(RefreshedTypes.All);
        }
        /// <summary>
        ///     回到提交前的状态 by luojie
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            //isSubmit = false;

            //隐藏工具栏 不允许二次提交
            #region 隐藏entitybrowser中的toolbar按钮
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }
            #endregion
            //if (refreshType == RefreshedTypes.CloseAndReloadData)
            //{
            //    //refreshType = RefreshedTypes.AuditInfo;
            //    refreshType = RefreshedTypes.HideProgressBar;
            //}

        }
        /// <summary>
        /// 修改社保
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_PensionMasterUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("EDITERROR"), Utility.GetResourceStr(e.Error.Message));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.UserState.ToString() == "Edit" && !isSubmit)
                {
                    isSubmit = false;
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "PENSIONMASTER"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
          Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "PENSIONMASTER"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                canSubmit = true;
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                toSubmit();
                RefreshUI(RefreshedTypes.All);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        #endregion
        /// <summary>
        /// 选择员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            //cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            //cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            //LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
            //    typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            //lookup.SelectedClick += (o, ev) =>
            //{
            //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST; ;

            //    if (ent != null)
            //    {
            //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;

                if (ent != null)
                {
                    lkEmployeeName.DataContext = ent;
                    //  SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE temp = lkEmployeeName.DataContext as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    T_HR_EMPLOYEE entity = new T_HR_EMPLOYEE();
                    entity.EMPLOYEEID = ent.EMPLOYEEID;
                    entity.EMPLOYEECNAME = ent.EMPLOYEECNAME;
                    entity.OWNERPOSTID = ent.OWNERPOSTID;
                    entity.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
                    entity.OWNERCOMPANYID = ent.OWNERCOMPANYID;

                    PensionMaster.OWNERCOMPANYID = ent.OWNERCOMPANYID;
                    PensionMaster.OWNERDEPARTMENTID = ent.OWNERDEPARTMENTID;
                    PensionMaster.OWNERPOSTID = ent.OWNERPOSTID;
                    PensionMaster.OWNERID = ent.OWNERID;
                    PensionMaster.T_HR_EMPLOYEE = entity;
                }
            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}

