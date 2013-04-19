using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.CommForm;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class LicenseBorrowForm : BaseForm,IClient, IEntityEditor,IAudit
    {
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private Action action;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private SmtOADocumentAdminClient client;
        //private V_LicenseBorrow licenseViewObj;
        private string licenseID = "";
        private string checkState = "";
        FormTypes formTypeAction;
        private T_OA_LICENSEUSER licenseObj;

        public T_OA_LICENSEUSER LicenseObj
        {
            get { return licenseObj; }
            set
            {
                licenseObj = value;
                this.DataContext = value;
            }
        }

        //public V_LicenseBorrow LicenseViewObj
        //{
        //    get { return licenseViewObj; }
        //    set
        //    {
        //        //this.DataContext = value;
        //        licenseViewObj = value;
        //    }
        //}

        #region 初始化
        public LicenseBorrowForm(Action action, string licenseID, string checkState)
        {
            

            InitializeComponent();
            this.action = action;
            this.licenseID = licenseID;
            this.checkState = checkState;
            InitEvent();
            switch (action)
            { 
                case Action.Add:
                    formTypeAction = FormTypes.New;
                    break;
                case Action.Edit:
                    formTypeAction = FormTypes.Edit;
                    break;
                case Action.AUDIT:
                    formTypeAction = FormTypes.Audit;
                    break;
                case Action.Read:
                    formTypeAction = FormTypes.Browse;
                    break;       
                case Action.ReSubmit:
                    formTypeAction = FormTypes.Resubmit;
                    break;       
            }
            if (action != Action.Add)
            {
                if (action == Action.AUDIT)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                client.GetLicenseBorrowListByIdAsync(licenseID);
            }     
            if (checkState != ((int)CheckStates.UnSubmit).ToString() && checkState != ((int)CheckStates.UnApproved).ToString() && checkState !="5")   //只有未提交和未通过才能修改
            {
                if (action != Action.Add)
                {
                    SetReadOnly();
                }
            }
            else
            {
                if (action != Action.AUDIT && action != Action.Read)
                {
                    SetToolBar();
                }
                if (action == Action.Read)
                {
                    SetReadOnly();
                }
            }
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        //引擎使用
        public LicenseBorrowForm(FormTypes actionFormtype, string licenseID)
        {
            InitializeComponent();
            this.action = Action.AUDIT;
            formTypeAction = FormTypes.Audit;
            this.licenseID = licenseID;
            this.checkState = ((int)CheckStates.Approving).ToString();
            InitEvent();
            if (action != Action.Add)
            {
                if (action == Action.AUDIT)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                client.GetLicenseBorrowListByIdAsync(licenseID);
            }
            if (checkState != ((int)CheckStates.UnSubmit).ToString() && checkState != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
            {
                SetReadOnly();
            }
            else
            {
                SetToolBar();
            }
        }

        private void InitEvent()
        {

            client = new SmtOADocumentAdminClient();
            client.AddLicenseBorrowCompleted += new EventHandler<AddLicenseBorrowCompletedEventArgs>(client_AddLicenseBorrowCompleted);
            client.GetLicenseBorrowListByIdCompleted += new EventHandler<GetLicenseBorrowListByIdCompletedEventArgs>(client_GetLicenseBorrowListByIdCompleted);
            client.UpdateLicenseBorrowCompleted += new EventHandler<UpdateLicenseBorrowCompletedEventArgs>(client_UpdateLicenseBorrowCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            if (action == Action.Add)
            {
                //LicenseViewObj = new V_LicenseBorrow();
                //LicenseViewObj.licenseUser = new T_OA_LICENSEUSER();
                LicenseObj = new T_OA_LICENSEUSER();
                licenseObj.STARTDATE = DateTime.Now;
                licenseObj.ENDDATE = DateTime.Now.AddDays(7);
                //this.SumbitButton.Visibility = Visibility.Collapsed;
                //licenseObj = new T_OA_LICENSEUSER();
            }
            else
            {
                if (action == Action.AUDIT)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                client.GetLicenseBorrowListByIdAsync(licenseID);
            }
        }

        private void SetReadOnly()
        {
            this.sDate.IsEnabled = false;
            this.eDate.IsEnabled = false;
            this.txtContent.IsReadOnly = true;
            this.txtLicenseName.IsReadOnly = true;
        }

        private void SetToolBar()
        {
            //ToolbarItems = CreateFormNewButton();
            if (checkState == ((int)CheckStates.UnSubmit).ToString())
            {
                //scvAudit.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 完成事件        

        /// <summary>
        /// 更新完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UpdateLicenseBorrowCompleted(object sender, UpdateLicenseBorrowCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (!string.IsNullOrEmpty(e.Result))
                    {   
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (e.UserState.ToString() == "Edit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                            if (GlobalFunction.IsSaveAndClose(refreshType))
                            {
                                RefreshUI(refreshType);
                            }
                            else
                            {
                                client.GetLicenseBorrowListByIdAsync(licenseID);
                            }

                        }
                        else if (e.UserState.ToString() == "Audit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                        }
                        RefreshUI(RefreshedTypes.All);


                        //HtmlPage.Window.Alert("更新外借记录成功！");
                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    actionFlag = DataActionFlag.SubmitComplete;
                        //    SumbitFlow();
                        //}
                        //else
                        //{
                        //    RefreshUI(RefreshedTypes.ProgressBar);
                        //    if (actionFlag == DataActionFlag.SubmitComplete)
                        //    {
                        //        if (action == Action.Add || action == Action.Edit)
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "LICENSELENDING"));
                        //        }
                        //        else
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "LICENSELENDING"));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "LICENSELENDING"));
                        //        if (GlobalFunction.IsSaveAndClose(refreshType))
                        //        {                                    
                        //            RefreshUI(RefreshedTypes.CloseAndReloadData);
                        //        }
                        //        else
                        //        {                                    
                        //            client.GetLicenseBorrowListByIdAsync(licenseID);
                        //        }
                        //    }                            
                        //    //todo: close and refresh
                        //    RefreshUI(refreshType);
                        //}
                    }
                }
                else
                {   
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetLicenseBorrowListByIdCompleted(object sender, GetLicenseBorrowListByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //licenseViewObj = e.Result;
                        LicenseObj = e.Result;
                        if (action == Action.AUDIT)
                        {
                            //audit.XmlObject = DataObjectToXml<T_OA_LICENSEUSER>.ObjListToXml(LicenseObj, "OA");
                        }
                        if (actionFlag == DataActionFlag.SubmitFlow)
                        {
                            actionFlag = DataActionFlag.SubmitComplete;
                            //SumbitFlow();
                            return;
                        }
                        //T_OA_LICENSEMASTER licenseMaster = new T_OA_LICENSEMASTER();
                        //licenseMaster.LICENSENAME = licenseViewObj.LicenseName;
                        this.txtLicenseName.Text = LicenseObj.T_OA_LICENSEMASTER.LICENSENAME;
                        //BindAduitInfo();
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    //RefreshUI(RefreshedTypes.ProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }


        /// <summary>
        ///  新增完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_AddLicenseBorrowCompleted(object sender, AddLicenseBorrowCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (!string.IsNullOrEmpty(e.Result))
                    {                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        //HtmlPage.Window.Alert("新增外借记录成功！");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                        else
                        {
                            formTypeAction = FormTypes.Edit;
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;                            
                            this.action = Action.Edit;
                            this.licenseID = licenseObj.LICENSEUSERID;
                            client.GetLicenseBorrowListByIdAsync(licenseID);
                        }


                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    client.GetLicenseBorrowListByIdAsync(licenseObj.LICENSEUSERID);
                        //}
                        //else
                        //{
                        //    RefreshUI(RefreshedTypes.ProgressBar);
                        //    if (GlobalFunction.IsSaveAndClose(refreshType))
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "LICENSELENDING"));
                        //        //todo: close and refresh
                        //        RefreshUI(refreshType);
                        //    }
                        //    else
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "LICENSELENDING"));
                        //        this.action = Action.Edit;
                        //        this.licenseID = licenseObj.LICENSEUSERID;
                        //        client.GetLicenseBorrowListByIdAsync(licenseID);
                        //    }
                        //}

                    }
                }
                else
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {   
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }
        #endregion

        #region 自定义事件

        private void Save()
        {
            try
            {                
                if (licenseObj.T_OA_LICENSEMASTER == null)
                {
                    this.txtLicenseName.Text = "";
                    //HtmlPage.Window.Alert("请先选择证照！");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "LICENSE"));
                    return;
                }
                if (licenseObj.STARTDATE == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "LENDTIME"));
                    return;
                }
                if (licenseObj.ENDDATE == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EXPECTEDRETURNTIME"));
                    return;
                }
                if (Convert.ToDateTime(licenseObj.STARTDATE) > Convert.ToDateTime(licenseObj.ENDDATE))
                {
                    //HtmlPage.Window.Alert("预订归还时间不能早于借阅时间!");
                    //this.txtTitle.Focus();
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "LENDTIME,EXPECTEDRETURNTIME"));
                    return;
                }
                if (string.IsNullOrEmpty(licenseObj.CONTENT))
                {
                    //HtmlPage.Window.Alert("请先填写借阅用途！");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "PURPOSE"));
                    return;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (action == Action.Add)  //新增
                {
                    licenseObj.LICENSEUSERID = Guid.NewGuid().ToString();
                    licenseObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();

                    licenseObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    licenseObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    licenseObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    licenseObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    licenseObj.CREATEDATE = DateTime.Now;

                    licenseObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    licenseObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    licenseObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    licenseObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                    licenseObj.HASRETURN = "0";
                    client.AddLicenseBorrowAsync(licenseObj,"Add");
                }
                else    //修改
                {
                    //licenseObj.LENDERID = "admin";
                    licenseObj.UPDATEDATE = DateTime.Now;
                    licenseObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    licenseObj.CHECKSTATE = "0";
                    client.UpdateLicenseBorrowAsync(licenseObj,"Edit");
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        /// <summary>
        /// 提交流程按钮
        /// </summary>
        private void SumbitAudit()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }

        private void ChooseLicense()
        {
            Dictionary<string, string> cols = new Dictionary<string, string>();
            cols.Add("LICENSENAME", "LICENSENAME");
            cols.Add("LEGALPERSON", "LEGALPERSON");
            cols.Add("LICENCENO", "LICENCENO");
            cols.Add("DAY", "DAY");
            LookupForm lookup = new LookupForm(SMT.SaaS.OA.UI.SmtOADocumentAdminService.EntityNames.LicenseBorrow,
                typeof(T_OA_LICENSEMASTER[]), cols);

            lookup.SelectedClick += (o, ev) =>
            {
                T_OA_LICENSEMASTER ent = lookup.SelectedObj as T_OA_LICENSEMASTER;
                if (ent != null)
                {                    
                    LicenseObj.T_OA_LICENSEMASTER = ent;
                    this.txtLicenseName.Text = ent.LICENSENAME;
                }
            };
            lookup.Show();
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            string StrReurn = "";
            switch (action)
            { 
                case Action.Add:
                    StrReurn = Utility.GetResourceStr("ADDTITLE", "LICENSELENDING");
                    break;
                case Action.Edit:
                    StrReurn = Utility.GetResourceStr("EDITTITLE", "LICENSELENDING");
                    break;
                case Action.AUDIT:
                    StrReurn = Utility.GetResourceStr("VIEWTITLE", "LICENSELENDING");
                    break;
                case Action.Read:
                    StrReurn = Utility.GetResourceStr("VIEWTITLE", "LICENSELENDING");
                    break;
            }
            return StrReurn;
            
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
                    refreshType = RefreshedTypes.All;
                    Save();                    
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "2":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    SumbitAudit();
                    break;
                case "3":
                    ChooseLicense();
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            if (action == Action.Add || action == Action.Edit )
            {
                return CreateFormNewButton();
            }
            else
            {
                return ToolbarItems;
            }
        }

        private List<ToolbarItem> CreateFormNewButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //ToolbarItem item1 = new ToolbarItem
            // {
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = Utility.GetResourceStr("SUBMITAUDIT"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};
            
            //item1.ItemClick += new EventHandler<RoutedEventArgs>(item_ItemClick);

            //items.Add(item1);
            if (formTypeAction != FormTypes.Browse && formTypeAction != FormTypes.Audit)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    //Title = Utility.GetResourceStr("SAVEANDCLOSE"),
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

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "3",
                    Title = Utility.GetResourceStr("CHOOSELICENSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };

                items.Add(item);
            }

            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };
            
            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png",
                

            };
            item.ItemClick += new EventHandler<RoutedEventArgs>(item1_ItemClick);

            items.Add(item);



            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "5",
            //    Title = "提交审核",
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "6",
            //    Title = "审核",//"审核"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "7",
            //    Title = "审核不通过",//"审核不通过"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            //};

            //items.Add(item);

            return items;
        }

        void item_ItemClick(object sender, RoutedEventArgs e)
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), "IT IS sUBMITAUDIT");
        }

        void item1_ItemClick(object sender, RoutedEventArgs e)
        {
            Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),"IT IS ME");
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

        #region 流程
        /// <summary>
        /// 提交流程结果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            auditResult = e.Result;
            switch (auditResult)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    //todo 审核中
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
                    //todo 取消
                    Cancel();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //todo 终审通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    //todo 审核不通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
                    //todo 审核异常
                    HandError();
                    break;
            }
        }

        //public void BindAduitInfo()
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "licenseBorrow";
        //    entity.FormID = licenseObj.LICENSEUSERID;
        //    entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //    entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //    entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //    entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    audit.BindingData();
        //}

        //public void SumbitFlow()
        //{

        //    if (licenseObj != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "licenseBorrow";//"archivesLending";T_HR_COMPANY
        //        entity.FormID = licenseObj.LICENSEUSERID; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        audit.XmlObject = DataObjectToXml<T_OA_LICENSEUSER>.ObjListToXml(LicenseObj, "OA");
        //        audit.Submit();
        //    }
        //}

        private void SumbitCompleted()
        {
            try
            {
                if (licenseObj != null)
                {
                    licenseObj.UPDATEDATE = DateTime.Now;
                    licenseObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    licenseObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            licenseObj.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            licenseObj.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            licenseObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    client.UpdateLicenseBorrowAsync(licenseObj,"Edit");
                }
            }
            catch (Exception ex)
            {
                //RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void Cancel()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_LICENSEUSER>(licenseObj, "OA");

            Utility.SetAuditEntity(entity, "licenseBorrow", licenseObj.LICENSEUSERID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
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
            if (licenseObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            licenseObj.CHECKSTATE = state;
            client.UpdateLicenseBorrowAsync(licenseObj, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (licenseObj != null)
                state = licenseObj.CHECKSTATE;
            if (formTypeAction == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion



        #region IForm 成员

        public void ClosedWCFClient()
        {
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
    }
}
