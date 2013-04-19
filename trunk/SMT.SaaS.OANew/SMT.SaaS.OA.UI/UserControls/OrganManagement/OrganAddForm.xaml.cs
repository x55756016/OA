using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.FBServiceWS;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class OrganAddForm : BaseForm,IClient, IEntityEditor,IAudit
    {
        private T_OA_ORGANIZATION organ;
        private FormTypes action;
        private SmtOADocumentAdminClient client;
        private FBServiceClient FbClient = new FBServiceClient();//费用控件
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal; 
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dicts= null;
        private ChooseLicenseForm form;
        private string checkstate = "";
        private ObservableCollection<T_OA_LICENSEMASTER> licenseMatserObjList = null;
        private SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER order = new SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER();
        private string organID = "";
        bool IsAudit = true; //判断是否提交了审核
        bool FBControlIsUsed = false; //费用控件是否使用
        //private List<T_OA_LICENSEMASTER> licenseMatserObjOrginList = null;
        //private DateTime dt1;
        //private DateTime dt2;
       

        public T_OA_ORGANIZATION Organ
        {
            get { return organ; }
            set
            {
                organ = value;
                this.DataContext = value;
            }
        }

        #region 初始化
        public OrganAddForm(FormTypes action, string organID, string checkstate)
        {            
            InitializeComponent();
            InitEvent();
            
            licenseMatserObjList = new ObservableCollection<T_OA_LICENSEMASTER>();
            //licenseMatserObjOrginList = new List<T_OA_LICENSEMASTER>();
            this.action = action;
            this.checkstate = checkstate;
            this.organID = organID;
            if (checkstate != ((int)CheckStates.UnSubmit).ToString() && checkstate != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
            {
                //SetReadOnly();
            }
            else
            {
                if (action == FormTypes.Audit || action == FormTypes.Browse)
                {
                    if (action == FormTypes.Browse)
                    {
                        SetReadOnly();
                    }
                    else
                    {
                        SetToolBar();
                    }
                }
            }
            SetFBControl();
            if (action == FormTypes.New)
            {
                Organ = new T_OA_ORGANIZATION();
                organ.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                InitFBControl();
            }
            else
            {
                txtOrganCode.IsEnabled = false;

                if (action == FormTypes.Audit)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                    
                }
                
                InitData();                
                //this.txtOrganCode.IsReadOnly = true;
            }
        }

        //
        public OrganAddForm(FormTypes action, string organID)
        {
            InitializeComponent();
            InitEvent();


            licenseMatserObjList = new ObservableCollection<T_OA_LICENSEMASTER>();
            //licenseMatserObjOrginList = new List<T_OA_LICENSEMASTER>();
            this.action = action;
            this.checkstate = ((int)CheckStates.Approving).ToString();//审核中 
            this.organID = organID;
            if (checkstate != ((int)CheckStates.UnSubmit).ToString() && checkstate != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
            {
                SetReadOnly();
            }
            else
            {
                if (action == FormTypes.Audit || action == FormTypes.Browse)
                {
                    SetToolBar();
                }
            }
            SetFBControl();
            
            txtOrganCode.IsEnabled = false;

            if (action == FormTypes.Audit)
            {
                actionFlag = DataActionFlag.SubmitComplete;                
            }
            InitData();
                
            
        }

        private void InitData()
        {
            if (organ == null)
            {
                organ = new T_OA_ORGANIZATION();
                organ.ORGANIZATIONID = organID;
                organ.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            }
            if (organ != null)
            {
                //dt1 = DateTime.Now;
                client.GetOrganListByOrganIdAsync(organID);                
            }
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.AddOrganCompleted += new EventHandler<AddOrganCompletedEventArgs>(client_AddOrganCompleted);
            client.GetOrganListByOrganIdCompleted += new EventHandler<GetOrganListByOrganIdCompletedEventArgs>(client_GetOrganListByOrganIdCompleted);
            client.UpdateOrganCompleted += new EventHandler<UpdateOrganCompletedEventArgs>(client_UpdateOrganCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            client.GetLicenseMatsterListByOrganIdCompleted += new EventHandler<GetLicenseMatsterListByOrganIdCompletedEventArgs>(client_GetLicenseMatsterListByOrganIdCompleted);
            //client.SubmitFlowCompleted += new EventHandler<SubmitFlowCompletedEventArgs>(client_SubmitFlowCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        

       

       
        private void SetToolBar()
        {
            //if (actions == Action.Add)
            //    ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_OA_ORGANIZATION", organ.OWNERID,
            //        organ.OWNERPOSTID, organ.OWNERDEPARTMENTID, organ.OWNERCOMPANYID);

            //if (actions == Action.Edit)
            //{
               
            //    ToolbarItems = Utility.CreateFormEditButton();
            //}
            //else
            //    ToolbarItems = Utility.CreateFormEditButton("T_OA_ORGANIZATION", organ.OWNERID,
            //        organ.OWNERPOSTID, organ.OWNERDEPARTMENTID, organ.OWNERCOMPANYID);

            //RefreshUI(RefreshedTypes.All);
            //if (actions == Action.Add || actions == Action.Edit)
            //{
                ToolbarItems = CreateFormNewButton();
                RefreshUI(RefreshedTypes.All);
                if (checkstate == ((int)CheckStates.UnSubmit).ToString())
                {
                    HiddenAudit();
                }
            //}
        }

        private void SetFBControl()
        {
            if (organ != null && organ.ISCHARGE=="0")
            {
                scvFB.Visibility = Visibility.Visible;
            }
        }

        private void HiddenFBControl(bool IsChecked)
        {
            if (IsChecked)
            {
                scvFB.Visibility = Visibility.Visible;
            }
            else
            {
                scvFB.Visibility = Visibility.Collapsed;
            }
            RefreshUI(RefreshedTypes.All);
            //if (scvFB.Visibility == Visibility.Visible)
            //{
            //    scvFB.Visibility = Visibility.Visible;
            //    //FrameworkElement element = (FrameworkElement)LayoutRoot.Parent;
            //    //element.Height = element.ActualHeight + 250;
            //    RefreshUI(RefreshedTypes.All);

                
            //}
            //else
            //{
            //    scvFB.Visibility = Visibility.Collapsed;
            //    //FrameworkElement element = (FrameworkElement)LayoutRoot.Parent;
            //    //element.Height = element.ActualHeight - 250;
            //    //RefreshUI(RefreshedTypes.All);
            //}
        }

        private void HiddenAudit()
        {
            //scvAudit.Visibility = Visibility.Collapsed;
            //this.Height -= scvAudit.Height;
        }

        private void SetReadOnly()
        {
            this.txtOrganCode.IsEnabled = false;
            this.txtAddress.IsEnabled = false;
            this.txtLegalPerson.IsEnabled = false;
            this.txtLicenceNo.IsEnabled = false;
            this.txtOrganName.IsEnabled = false;
            this.txtBussinessArea.IsEnabled = false;            
        }

        private void BindLicense()
        {
            if (dicts == null || dicts.Count < 1)
            {
                dgLicense.ItemsSource = null;
                return;
            }
            else
            {
                dgLicense.ItemsSource = null;
                dgLicense.ItemsSource = dicts;
            }
        }
        #endregion

        #region  调用事件完成
        //获取机构完成
        private void client_GetOrganListByOrganIdCompleted(object sender, GetOrganListByOrganIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //dt2 = DateTime.Now;
                        //MessageBox.Show((dt2 - dt1).TotalSeconds.ToString());
                        Organ = e.Result.ToList()[0];
                        txtOrganCode.DataContext = organ;
                        txtOrganCode.DisplayMemberPath = "ORGCODE";
                        if (organ.ISCHARGE == "1")
                        {
                            FBControlIsUsed = true;
                            FeeChkBox.IsChecked = true;
                            HiddenFBControl(true);   
                            
                        }                        
                        client.GetLicenseMatsterListByOrganIdAsync(organ.ORGCODE);
                        
                        InitFBControl();
                        //if (action == FormTypes.Audit)
                        //{
                        //    //audit.XmlObject = DataObjectToXml<T_OA_ORGANIZATION>.ObjListToXml(organ, "OA");
                        //}
                        if (action == FormTypes.Resubmit)
                        {
                            organ.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                        }
                        if (checkstate != ((int)CheckStates.UnSubmit).ToString() && checkstate != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
                        {
                            //BindAduitInfo();                           
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }
                        
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

        public void client_GetLicenseMatsterListByOrganIdCompleted(object sender, GetLicenseMatsterListByOrganIdCompletedEventArgs e)
        {
            try
            {                
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        licenseMatserObjList = e.Result;
                        HandLicenseMaster();
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

        //新增完成
        public void client_AddOrganCompleted(object sender, AddOrganCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (!e.Cancelled)//if (e.Error == null)
                {
                    if (e.Error == null)
                    {
                        if (e.Result != "")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                            if (GlobalFunction.IsSaveAndClose(refreshType))
                            {
                                RefreshUI(refreshType);
                            }
                            else
                            {
                                action = FormTypes.Edit;                                
                                
                                InitData();
                                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                entBrowser.FormType = FormTypes.Edit;
                                RefreshUI(RefreshedTypes.AuditInfo);
                                RefreshUI(RefreshedTypes.All);
                            }



                            //if (actionFlag == DataActionFlag.SubmitFlow)
                            //{
                            //    this.action = FormTypes.Edit;
                            //    SumbitFlow();
                            //}
                            //else
                            //{

                            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "ORGAN"));
                            //    if (GlobalFunction.IsSaveAndClose(refreshType))
                            //    {
                            //        RefreshUI(refreshType);
                            //    }
                            //    else
                            //    {
                            //        this.action = FormTypes.Edit;
                            //        InitData();
                            //    }

                            //}
                        }
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
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

        //更新完成

        private void client_UpdateOrganCompleted(object sender, UpdateOrganCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                
                if (e.Error == null)
                {
                    //HtmlPage.Window.Alert("机构信息修改成功！");  
                    if (e.Result)
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
                                InitData();
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




                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    //actionFlag = DataActionFlag.SubmitComplete;
                        //    SumbitFlow();
                        //}
                        //else
                        //{

                        //    if (actionFlag == DataActionFlag.SubmitComplete)
                        //    {
                        //        if (action == FormTypes.New || action == FormTypes.Edit)
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITORGANREGISTERSUCCESSED"));                                    
                        //        }
                        //        else
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "ORGAN"));                                    
                        //        }
                        //        RefreshUI(RefreshedTypes.CloseAndReloadData);

                        //    }
                        //    else
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "ORGAN"));
                        //        if (GlobalFunction.IsSaveAndClose(refreshType))
                        //        {
                        //            RefreshUI(RefreshedTypes.CloseAndReloadData);
                        //        }
                        //        else
                        //        {
                        //            InitData();
                        //        }
                        //    }
                            
                        //}
                    }
                    else
                    {
                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("UPDATEFAILED"));
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

        #region IEntityEditor
        public string GetTitle()
        {
            if (action == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "ORGAN");
            }
            else if (action == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "ORGAN");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "ORGAN");
            }
        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
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
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            
            if (action != FormTypes.Browse && action != FormTypes.Audit)
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
            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = Utility.GetResourceStr("SUBMITAUDIT"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};
            

            //items.Add(item);

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_SaveClose.png"
            };
            
            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",

                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Save.png"
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

            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            

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
        #endregion

        #region 保存、选择证照、选择机构代码
        private void Save()
        {            
            try
            {
                if (action == FormTypes.Browse)            //查看
                {
                    RefreshUI(refreshType);
                }
                else
                {
                    if (Check())
                    {
                        RefreshUI(RefreshedTypes.ShowProgressBar);
                        //fbCtr.Save(GlobalFunction.GetCheckStateByValue(checkstate));
                        organ.ISCHARGE = (bool)FeeChkBox.IsChecked ? "1" : "0";
                        //fbCtr.Order.ORDERID = organ.ORGANIZATIONID;
                        organ.CHARGEMONEY = fbCtr.Order.TOTALMONEY;
                        //organ.CHARGEMONEY 
                        //organ.ORGCODE = txtOrganCode.DataContext.ToString();
                        AddLicenseMatserObjList();

                        if (action == FormTypes.New)
                        {                            
                            organ.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                            organ.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            organ.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            organ.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            organ.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            organ.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            organ.CREATEDATE = DateTime.Now;
                            organ.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            organ.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            organ.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            organ.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            organ.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            organ.ORGANIZATIONID = Guid.NewGuid().ToString();
                            
                            //fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);
                            //client.AddOrganAsync(organ,licenseMatserObjList);
                        }
                        else
                        {
                            organ.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            organ.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            organ.UPDATEDATE = DateTime.Now;
                            organ.CHECKSTATE = "0";
                            //client.UpdateOrganAsync(organ, licenseMatserObjList);
                        }
                        if (FBControlIsUsed) //使用了费用控件则提交费用信息 先添加费用信息 后提交
                        {
                            fbCtr.Order.ORDERID = organ.ORGANIZATIONID;
                            fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);//提交费用
                        }
                        else
                        {
                            switch (action)
                            { 
                                case FormTypes.New:
                                    client.AddOrganAsync(organ, licenseMatserObjList,"Add");
                                    break;
                                case FormTypes.Edit:
                                    client.UpdateOrganAsync(organ, licenseMatserObjList,"Edit");
                                    break;
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private void Close()
        {
            RefreshUI(refreshType);            
        }

        //验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {                    
                foreach (var h in validators)
                {
                    //HtmlPage.Window.Alert(h.ErrorMessage);
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            else if (dicts==null || dicts.Count == 0)
            {
                //HtmlPage.Window.Alert("请先选择证照信息！");
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "LICENSETAB"));
                return false;
            }
            return true;
        }

        private void ChooseLicense()
        {
            form = new ChooseLicenseForm();
            EntityBrowser browser = new EntityBrowser(form);
            //browser.AuditCtrl.Visibility = Visibility.Collapsed;
            browser.HideLeftMenu();

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            if (form.dict != null && form.dict.Count>0)
            {
                dicts = form.dict;
                BindLicense();
            }           
        }

        private void AddLicenseMatserObjList()
        {
            if (licenseMatserObjList != null)
            {
                licenseMatserObjList.Clear();
                if (dicts != null && dicts.Count > 0)
                {
                    foreach (var h in dicts)
                    {
                        T_OA_LICENSEMASTER licenseMaster = new T_OA_LICENSEMASTER();
                        //licenseMaster.ORGCODE = organ.ORGCODE;
                        
                        licenseMaster.LICENSEMASTERID = Guid.NewGuid().ToString();
                        licenseMaster.LICENSENAME = h.DICTIONARYNAME;
                        licenseMaster.POSITION = " ";
                        licenseMaster.LEGALPERSON = organ.LEGALPERSON;
                        licenseMaster.ADDRESS = organ.ADDRESS;
                        licenseMaster.LICENCENO = organ.LICENCENO;
                        licenseMaster.BUSSINESSAREA = organ.BUSSINESSAREA;
                        licenseMaster.DAY = 0;
                        licenseMaster.LENDFLAG = "0";
                        licenseMaster.ISVALID = "0";
                        licenseMaster.T_OA_ORGANIZATION = organ;

                        licenseMaster.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        licenseMaster.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        licenseMaster.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        licenseMaster.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        licenseMaster.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        licenseMaster.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        licenseMaster.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        licenseMaster.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        licenseMaster.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        licenseMaster.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                        licenseMaster.CREATEDATE = DateTime.Now;

                        licenseMatserObjList.Add(licenseMaster);
                    }
                }
            }
        }

        private void HandLicenseMaster()
        {
            dicts = GetLicenseMaster();
            BindLicense();
        }

        private void SetLicenseMasterValid()
        {
            foreach (var h in licenseMatserObjList)
            {
                h.ISVALID = "1";
            }
        }


        private List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> GetLicenseMaster()
        {
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> tmp = new List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>();
            List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY> dictionary = Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>;
            foreach (var h in licenseMatserObjList)
            {
                var obj = (from a in dictionary
                           where a.DICTIONCATEGORY == "LICENSE" && a.DICTIONARYNAME == h.LICENSENAME
                           select a).FirstOrDefault();
                if (obj!=null)
                {
                    tmp.Add(obj);
                }
            }
            return tmp;
        }

        private void CompanyObject_FindClick(object sender, EventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();

            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY;                
                if (ent != null)
                {
                    organ.ORGCODE = ent.COMPANRYCODE;
                    organ.ORGNAME = ent.CNAME;
                    txtOrganCode.DataContext = organ;
                    txtOrganCode.DisplayMemberPath = "ORGCODE";
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
        #endregion

        #region  流程
        public void SumbitAudit()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
            
        }

        //public void BindAduitInfo()
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "Organization";
        //    entity.FormID = organ.ORGANIZATIONID;
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

        //    if (organ != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "Organization";//"archivesLending";T_HR_COMPANY
        //        entity.FormID = organ.ORGANIZATIONID; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        audit.XmlObject = DataObjectToXml<T_OA_ORGANIZATION>.ObjListToXml(organ, "OA"); 
        //        audit.Submit();
        //    }
        //}

        /// <summary>
        /// 提交审核完成
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

        private void SumbitCompleted()
        {
            fbCtr.Order.ORDERID = organ.ORGANIZATIONID;

            try
            {
                if (organ != null)
                {
                    actionFlag = DataActionFlag.SubmitComplete;//审核动作完成
                    organ.UPDATEDATE = DateTime.Now;
                    organ.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    organ.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    if (action == FormTypes.Audit) //如果是审核状态则将不重复提交
                    {
                        IsAudit = false;
                    }
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            organ.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            if (FBControlIsUsed)
                                fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.Approving); //审核中 add 2010-6-21
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            organ.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            if (FBControlIsUsed)
                                fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.Approved); //审核通过 add 2010-6-21
                            SetLicenseMasterValid();
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            organ.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            if (FBControlIsUsed)
                                fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnApproved); //审核不通过 add 2010-6-21
                            break;
                    }
                    if (!FBControlIsUsed)
                    {
                        client.UpdateOrganAsync(organ, licenseMatserObjList,"Edit");
                    }
                    
                    //fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.);
                    //client.UpdateOrganAsync(organ,licenseMatserObjList);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void Cancel()
        {
            RefreshUI(refreshType);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            RefreshUI(refreshType);
        }

        #endregion

        private void FeeChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FBControlIsUsed = false;
            scvFB.Visibility = Visibility.Collapsed;
            HiddenFBControl(false);
        }

        private void FeeChkBox_Checked(object sender, RoutedEventArgs e)
        {
            FBControlIsUsed = true;
            scvFB.Visibility = Visibility.Visible;
            HiddenFBControl(true);            
            //fbCtr.InitData();   
        }

        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            fbCtr.strExtOrderModelCode = "JGZC";

            if (action == FormTypes.New)
            {
                fbCtr.Order.ORDERID = "";
                fbCtr.Order.ORDERCODE = "JGZC" + string.Format("{0:yyyyMMddHHmmssffff}", System.DateTime.Now);
                //fbCtr.Order.ORDERCODE = "fby-201003030003";

            }
            else
            {
                fbCtr.Order.ORDERID = organ.ORGANIZATIONID;//费用对象
            }
            
            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.Order.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            
            
            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            
            //else
            //{

            //    fbCtr.Order.ORDERID = organ.ORGANIZATIONID;
            //    fbCtr.Order.ORDERCODE = organ.ORGANIZATIONID;
            //    fbCtr.Order.UPDATEDATE = DateTime.Now;
            //    fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            //    fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            //}
            fbCtr.InitDataComplete += (o, e) =>
                {
                    Binding bding = new Binding();
                    bding.Path = new PropertyPath("TOTALMONEY");
                    this.txtFee.SetBinding(TextBox.TextProperty, bding);
                    this.txtFee.DataContext = fbCtr.Order;
                };
            if (action == FormTypes.Audit || action == FormTypes.Browse)
            {
                fbCtr.InitData(false);
            }
            else
            {
                fbCtr.InitData();
            }
            
        }

        void order_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "")
            {
                organ.CHARGEMONEY = order.TOTALMONEY;
            }
        }
        
        private void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }     

        //private void scvFB_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    double _height = ((ScrollViewer)sender).ActualHeight;
            
        //}
               

        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                List<string> aa = e.Message;
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Message[0]);
            }
            else
            {
                switch (action)
                { 
                    case FormTypes.New:
                        client.AddOrganAsync(organ, licenseMatserObjList,"Add");
                        break;
                    case FormTypes.Edit:
                        client.UpdateOrganAsync(organ, licenseMatserObjList,"Edit");
                        break;
                    case FormTypes.Audit:

                        if (organ.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved) || organ.CHECKSTATE == Utility.GetCheckState(CheckStates.UnApproved))
                        {
                            client.UpdateOrganAsync(organ, licenseMatserObjList,"Edit");
                        }//审核通过,审核通过直接修改表单状态
                        else
                        {
                            if (IsAudit) //审核中
                            {
                                //SumbitFlow();
                                IsAudit = false;
                            }
                            else
                            {
                                client.UpdateOrganAsync(organ, licenseMatserObjList,"Edit");
                            }
                        }
                        
                        break;
                }
                
                
            }
        }

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_ORGANIZATION>(organ, "OA");

            Utility.SetAuditEntity(entity, "Organization", organ.ORGANIZATIONID, strXmlObjectSource);
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
            if (organ.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            organ.CHECKSTATE = state;
            client.UpdateOrganAsync(organ, licenseMatserObjList, UserState);
            //client.UpdateArchivesLendingAsync(lendingArchives, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (organ != null)
                state = organ.CHECKSTATE;
            if (action == FormTypes.Browse)
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
