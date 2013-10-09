using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseInfoIssuranceForm : BaseForm,IClient, IEntityEditor, IAudit
    {
        private HouseInfoChooseForm addFrm;
        private List<T_OA_HOUSEINFO> houseInfoList;
        //private List<T_OA_HOUSEINFO> originHouseInfoList;
        private List<string> houseID;
        private T_OA_HOUSEINFO houseObj;
        private T_OA_HOUSEINFOISSUANCE issuanceObj;
        //private List<T_OA_HOUSELIST> originHouseList;
        private List<T_OA_HOUSELIST> houseList;
        private List<T_OA_DISTRIBUTEUSER> distributeList;
        private string issuanceID = "";
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private List<ExtOrgObj> issuanceExtOrgObj;
        private ObservableCollection<T_OA_DISTRIBUTEUSER> distributeLists;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private string checkstate = "";
        private bool submitflag=false;

        public T_OA_HOUSEINFOISSUANCE IssuanceObj
        {
            get { return issuanceObj; }
            set
            {
                this.DataContext = value;
                issuanceObj = value;
            }
        }
        private Action action;
        private FormTypes formTypeAction;
        private SmtOACommonAdminClient client;
        private ObservableCollection<T_OA_HOUSELIST> houseLists;


        #region 初始化
        public HouseInfoIssuranceForm(Action action, string issuanceID, string checkstate)
        {
            this.action = action;
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
            this.issuanceID = issuanceID;
            this.checkstate = checkstate;
            InitializeComponent();
            InitEvent();            
            InitData();
            
        }

        //门户调用审核使用
        public HouseInfoIssuranceForm(FormTypes typeaction, string issuanceID)
        {
            this.action = Action.AUDIT;
            formTypeAction = FormTypes.Audit;
            
            this.issuanceID = issuanceID;
            //this.checkstate = checkstate;
            InitializeComponent();
            InitEvent();
            InitData();

        }


        /// <summary>
        /// 构造函数方便平台调用
        /// </summary>
        /// <param name="Straction"></param>
        /// <param name="issuanceID"></param>
        /// <param name="checkstate"></param>
        public HouseInfoIssuranceForm(string Straction, string issuanceID, string checkstate)
        {
            switch (Straction)
            { 
                case "0"://增加
                    this.action = Action.Add;
                    break;
                case "1"://修改
                    this.action = Action.Edit;
                    break;
                case "3"://查看
                    this.action = Action.Read;
                    break;
                case "4"://打印
                    this.action = Action.Print;
                    break;
                case "5"://借出
                    this.action = Action.Lend;
                    break;
                case "6"://6归还
                    this.action = Action.Return;
                    break;
                case "7":
                    this.action = Action.AUDIT;
                    break;
            }
            
            this.issuanceID = issuanceID;
            this.checkstate = checkstate;
            InitializeComponent();
            InitEvent();
            InitData();
        }

        private void InitEvent()
        {            
            houseInfoList = new List<T_OA_HOUSEINFO>();
            houseID = new List<string>();
            houseObj = new T_OA_HOUSEINFO();
            houseLists = new ObservableCollection<T_OA_HOUSELIST>();
            distributeLists = new ObservableCollection<T_OA_DISTRIBUTEUSER>();
            distributeList = new List<T_OA_DISTRIBUTEUSER>();
            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
            client = new SmtOACommonAdminClient();
            //SetToolBar();
            client.AddIssuanceCompleted += new EventHandler<AddIssuanceCompletedEventArgs>(client_AddIssuanceCompleted);
            client.UpdateIssuanceCompleted += new EventHandler<UpdateIssuanceCompletedEventArgs>(client_UpdateIssuanceCompleted);
            client.GetIssuanceListByIdCompleted += new EventHandler<GetIssuanceListByIdCompletedEventArgs>(client_GetIssuanceListByIdCompleted);
            client.GetIssuanceHouseInfoListCompleted += new EventHandler<GetIssuanceHouseInfoListCompletedEventArgs>(client_GetIssuanceHouseInfoListCompleted);
            client.GetIssuanceHouseListCompleted += new EventHandler<GetIssuanceHouseListCompletedEventArgs>(client_GetIssuanceHouseListCompleted);
            client.GetDistributeUserListCompleted += new EventHandler<GetDistributeUserListCompletedEventArgs>(client_GetDistributeUserListCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }        

        private void InitData()
        {
            if (action == Action.Add)
            {
                IssuanceObj = new T_OA_HOUSEINFOISSUANCE();
                IssuanceObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            }
            else
            {
                if (action == Action.AUDIT)
                {
                    txtContent.HideControls();
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                if(action == Action.ReSubmit)
                    IssuanceObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                client.GetIssuanceListByIdAsync(issuanceID);
            }
            if (action == Action.Read || action == Action.AUDIT)
            {
                SetReadOnly();
            }
                        
        }

        private void SetReadOnly()
        {
            this.txtTitle.IsEnabled = false;
            
            txtContent.HideControls();
        }


        private void SetToolBar()
        {           
            //ToolbarItems = CreateFormNewButton();
            //if (checkstate == ((int)CheckStates.UnSubmit).ToString())
            //{
            //    //scvAudit.Visibility = Visibility.Collapsed;
            //}
        }
        #endregion

        #region 完成事件
        private void client_GetDistributeUserListCompleted(object sender, GetDistributeUserListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        distributeList.Clear();
                        issuanceExtOrgObj.Clear();
                        distributeList = e.Result.ToList();
                        foreach (var h in distributeList)
                        {
                            object obj = new object();
                            SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                            if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company).ToString())
                            {
                                T_HR_COMPANY tmp = new T_HR_COMPANY();
                                tmp.COMPANYID = h.VIEWER;
                                tmp.CNAME = Utility.GetCompanyName(tmp.COMPANYID);
                                obj = tmp;                                
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department).ToString())
                            {
                                T_HR_DEPARTMENT tmp = new T_HR_DEPARTMENT();
                                tmp.DEPARTMENTID = h.VIEWER;
                                T_HR_DEPARTMENTDICTIONARY tmpdict = new T_HR_DEPARTMENTDICTIONARY();                                
                                tmpdict.DEPARTMENTNAME = Utility.GetDepartmentName(h.VIEWER);
                                tmp.T_HR_DEPARTMENTDICTIONARY = tmpdict;
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post).ToString())
                            {
                                T_HR_POST tmp = new T_HR_POST();
                                tmp.POSTLEVEL = System.Convert.ToDecimal(h.VIEWER);
                                T_HR_POSTDICTIONARY tmpdict = new T_HR_POSTDICTIONARY();
                                //tmpdict.POSTNAME = "";
                                tmpdict.POSTNAME = Utility.GetPostName(h.VIEWER);
                                tmp.T_HR_POSTDICTIONARY = tmpdict;
                                obj = tmp;
                            }
                            else if (h.VIEWTYPE == ((int)SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel).ToString())
                            {
                                T_HR_EMPLOYEE tmp = new T_HR_EMPLOYEE();
                                tmp.EMPLOYEEID = h.VIEWER;
                                tmp.EMPLOYEECNAME = "";
                                obj = tmp;
                            }
                            extOrgObj.ObjectInstance = obj;
                            issuanceExtOrgObj.Add(extOrgObj);
                        }
                        BindData();
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }   

        private void client_GetIssuanceListByIdCompleted(object sender, GetIssuanceListByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        IssuanceObj = e.Result.ToList()[0];
                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    actionFlag = DataActionFlag.SubmitComplete;
                        //    SumbitFlow();                           
                        //    return;
                        //}
                        if (formTypeAction == FormTypes.Resubmit)
                        {
                            IssuanceObj.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                            
                        }                        
                        txtContent.RichTextBoxContext = issuanceObj.CONTENT;
                        //txtWorkYear.Text = issuanceObj
                        client.GetIssuanceHouseInfoListAsync(issuanceObj.ISSUANCEID);
                        client.GetIssuanceHouseListAsync(issuanceObj.ISSUANCEID);
                        client.GetDistributeUserListAsync(issuanceObj.ISSUANCEID);
                        //BindAduitInfo();
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.Error.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }


        private void client_UpdateIssuanceCompleted(object sender, UpdateIssuanceCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        //HtmlPage.Window.Alert(e.Result);                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Result);
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
                        
                                     
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.ToString());
                    
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_AddIssuanceCompleted(object sender, AddIssuanceCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {   
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Result);
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
                            formTypeAction = FormTypes.Edit;
                            action = Action.Edit;
                            this.issuanceID = issuanceObj.ISSUANCEID;
                            InitData();
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }


                        
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


        private void client_GetIssuanceHouseInfoListCompleted(object sender, GetIssuanceHouseInfoListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        houseInfoList = e.Result.ToList();
                        BindData();
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

        private void client_GetIssuanceHouseListCompleted(object sender, GetIssuanceHouseListCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        houseList = e.Result.ToList();
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
        #endregion

        #region 绑定数据
        private void BindData()
        {
            if (houseInfoList == null || houseInfoList.Count < 1)
            {
                dgHouse.ItemsSource = null;
            }
            else
            {
                dgHouse.ItemsSource = null;
                dgHouse.ItemsSource = houseInfoList;
            }
            if (issuanceExtOrgObj == null || issuanceExtOrgObj.Count < 1)
            {
                dgIssunanceObj.ItemsSource = null;
                return;
            }
            else
            {
                dgIssunanceObj.ItemsSource = null;
                dgIssunanceObj.ItemsSource = issuanceExtOrgObj;
            }
            
        }
        #endregion

        #region 按钮事件

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //MainGrid.Height = ((Grid)sender).ActualHeight * 0.90;
            //dgHouse.Height = ((Grid)sender).ActualHeight * 0.90;
            //dgIssunanceObj.Height = ((Grid)sender).ActualHeight * 0.90;
            //scvAudit.Height = ((Grid)sender).ActualHeight * 0.90;

        }

        #region 模板列选择
        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (!chkbox.IsChecked.Value)
            {
                houseObj = chkbox.DataContext as T_OA_HOUSEINFO;
                houseID.Remove(houseObj.HOUSEID);
            }
        }

        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;
            if (chkbox.IsChecked.Value)
            {
                houseObj = chkbox.DataContext as T_OA_HOUSEINFO;
                houseID.Add(houseObj.HOUSEID);
            }
        }
        #endregion   

        private void addFrm_Closed(object sender, EventArgs e)
        {
            if (addFrm.houseInfoList != null && addFrm.houseInfoList.Count > 0)
            {
                foreach (var h in addFrm.houseInfoList)
                {
                    var entity = from q in houseInfoList
                                 where h.HOUSEID == q.HOUSEID
                                 select q;
                    if (entity.Count() == 0)
                    {
                        houseInfoList.Add(h);
                    }
                }
                BindData();
            }
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "HOUSEINFOISSUANCE");
            }
            else
            {
                if (action == Action.Read)
                    return Utility.GetResourceStr("VIEWTITLE", "HOUSEINFOISSUANCE");
                else
                    return Utility.GetResourceStr("EDITTITLE", "HOUSEINFOISSUANCE");
            }
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
                    submitflag = false;
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    submitflag = false;
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "2":
                    Add();
                    break;
                case "3":
                    Delete();
                    break;
                case "4":
                    AddIssuanObj();
                    break;
                case "5":
                    submitflag = true;
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    SumbitAudit();
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
            if (formTypeAction == FormTypes.New || formTypeAction == FormTypes.Edit || formTypeAction == FormTypes.Resubmit)
            {
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



                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "3",
                    Title = Utility.GetResourceStr("DELETEHOUSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png"
                };

                items.Add(item);

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "2",
                    Title = Utility.GetResourceStr("CHOOSEHOUSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };

                items.Add(item);

                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "4",
                    Title = Utility.GetResourceStr("CHOOSEDISTRBUTEOBJECT"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
                };

                items.Add(item);
            }
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

        #region 保存、添加、删除
        private void Save()
        {
            try
            {
                List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();

                if (validators.Count > 0)
                {
                    if (string.IsNullOrEmpty(issuanceObj.ISSUANCETITLE))
                    {
                        //HtmlPage.Window.Alert("请输入发布标题！");
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ISSUANCETITLE"));
                        return;
                    }
                    if (issuanceObj.CONTENT.Length ==0)
                    {
                        //HtmlPage.Window.Alert("请输入发布公告内容！");
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "ISSUANCECONTENT"));
                        return;
                    }
                }
                else if (houseInfoList.Count == 0)
                {
                    //HtmlPage.Window.Alert("请先选择要发布的房源信息！");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "HOUSEINFO"));
                    return;
                }
                else if (issuanceExtOrgObj.Count == 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "DISTRBUTEOBJECT"));
                    return;
                }
                else
                {
                    //if(txtContent.)
                    if (txtContent.RichTextBoxContext == null)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REQUIRED", "ISSUANCECONTENT"));
                        return;
                    }
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    houseLists.Clear();
                    distributeLists.Clear();
                    issuanceObj.CONTENT = txtContent.RichTextBoxContext;
                    if (action == Action.Add)  //新增
                    {
                        //string issuanceID = Guid.NewGuid().ToString();
                        //issuanceObj.ISSUANCEID = issuanceID;
                        issuanceObj.ISSUANCEID = Guid.NewGuid().ToString();
                        issuanceObj.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                        //issuanceObj.CONTENT = txtContent.RichTextBoxContext;
                        //issuanceObj.T_OA_HOUSELIST = houseLists;
                        //issuanceObj.POSTLEVEL = "0";
                        
                        
                        issuanceObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        issuanceObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        issuanceObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        issuanceObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        issuanceObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        issuanceObj.CREATEDATE = DateTime.Now;

                        issuanceObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        issuanceObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        issuanceObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        issuanceObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        issuanceObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

                        foreach (var h in houseInfoList)
                        {
                            AddHouseList(h, issuanceObj);
                        }
                        foreach (var h in issuanceExtOrgObj)
                        {
                            AddDistributeObjList(h, issuanceObj.ISSUANCEID);
                        } 
                        client.AddIssuanceAsync(issuanceObj, houseLists,distributeLists,"Add");
                    }
                    else                       //更新
                    {
                        foreach (var h in houseInfoList)
                        {
                            //是更新还是新增
                            var entity = houseList.Where(s => s.T_OA_HOUSEINFO.HOUSEID == h.HOUSEID).FirstOrDefault();
                            if (entity != null)
                            {
                                entity.UPDATEDATE = DateTime.Now;
                                entity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                                entity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                                //entity.EntityKey = null;
                                houseLists.Add(entity);
                            }
                            else
                            {
                                //AddHouseList(h, issuanceObj.ISSUANCEID);
                                AddHouseList(h, issuanceObj);
                            }
                        }
                        foreach (var h in issuanceExtOrgObj)
                        {
                            //是更新还是新增
                            var entity = distributeList.Where(s => s.FORMID == h.ObjectID).FirstOrDefault();
                            if (entity != null)
                            {
                                entity.UPDATEDATE = DateTime.Now;
                                entity.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                                entity.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                                //entity.EntityKey = null;
                                distributeLists.Add(entity);
                            }
                            else
                            {
                                AddDistributeObjList(h, issuanceObj.ISSUANCEID);
                            }
                        }
                        issuanceObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        
                        issuanceObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        issuanceObj.CHECKSTATE = "0";
                        client.UpdateIssuanceAsync(issuanceObj, houseLists, distributeLists, submitflag,"Edit");
                    }
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private void Close()
        {
            RefreshUI(refreshType);
        }

        private void Add()
        {
            addFrm = new HouseInfoChooseForm();
            EntityBrowser browser = new EntityBrowser(addFrm);
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) =>{});
        }

        private void AddIssuanObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count>0)
                {
                    issuanceExtOrgObj = ent;       
                    BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }

        private void browser_ReloadDataEvent()
        {
            if (addFrm.houseInfoList != null && addFrm.houseInfoList.Count > 0)
            {
                foreach (var h in addFrm.houseInfoList)
                {
                    var entity = from q in houseInfoList
                                 where h.HOUSEID == q.HOUSEID
                                 select q;
                    if (entity.Count() == 0)
                    {
                        houseInfoList.Add(h);
                    }
                }
                BindData();
            }
        }  

        private void Delete()
        {
            if (houseID.Count == 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTERROR", "DELETE"));
            }
            else
            {
                foreach (var h in houseID)
                {
                    for (int i = houseInfoList.Count - 1; i >= 0; i--)
                    {
                        if (houseInfoList[i].HOUSEID == h)
                        {
                            houseInfoList.RemoveAt(i);
                        }
                    }
                    if (houseList != null)
                    {
                        for (int i = houseList.Count - 1; i >= 0; i--)
                        {
                            if (houseList[i].T_OA_HOUSEINFO.HOUSEID == h)
                            {
                                houseList.RemoveAt(i);
                            }
                        }
                    }
                }
                houseID.Clear();
                BindData();
            }
        }

        //新增房源信息清单
        private void AddHouseList(T_OA_HOUSEINFO houseObj, T_OA_HOUSEINFOISSUANCE issuanceObj)
        {
            T_OA_HOUSELIST houseListTmp = new T_OA_HOUSELIST();
            
            houseListTmp.T_OA_HOUSEINFO = houseObj;
            houseListTmp.T_OA_HOUSEINFO.HOUSEID = houseObj.HOUSEID;
            houseListTmp.HOUSELISTID = Guid.NewGuid().ToString();
            houseListTmp.T_OA_HOUSEINFOISSUANCE = issuanceObj;

            houseListTmp.CONTENT = System.Convert.ToString(issuanceObj.CONTENT);//租房协议
            houseListTmp.SHAREDDEPOSIT = houseObj.SHAREDDEPOSIT;//合租租金
            houseListTmp.SHAREDRENTCOST = houseObj.SHAREDRENTCOST;//合租押金
            houseListTmp.RENTCOST = houseObj.RENTCOST;//整套押金
            houseListTmp.DEPOSIT = houseObj.DEPOSIT;//整套租金
            houseListTmp.MANAGECOST = (int)(houseObj.MANAGECOST/houseObj.Number); //管理费

            houseListTmp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            houseListTmp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            houseListTmp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            houseListTmp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            houseListTmp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            houseListTmp.CREATEDATE = DateTime.Now;

            

            houseListTmp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            houseListTmp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            houseListTmp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            houseListTmp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            houseListTmp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;

            houseLists.Add(houseListTmp);
        }

        private void AddDistributeObjList(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj, string issuanceID)
        {
            T_OA_DISTRIBUTEUSER distributeTmp = new T_OA_DISTRIBUTEUSER();
            distributeTmp.DISTRIBUTEUSERID = Guid.NewGuid().ToString();
            distributeTmp.MODELNAME = "HousingIssuance";
            distributeTmp.FORMID = issuanceID;
            distributeTmp.VIEWTYPE = ((int)GetObjectType(issuanceExtOrgObj)).ToString();
            if (distributeTmp.VIEWTYPE == ((int)IssuanceObjectType.Post).ToString())    //如果是选择岗位，则保存岗位级别
            {
                //if (!string.IsNullOrEmpty((issuanceExtOrgObj.ObjectInstance as T_HR_POST).POSTLEVEL.ToString()))
                //{
                //    distributeTmp.VIEWER = (issuanceExtOrgObj.ObjectInstance as T_HR_POST).POSTLEVEL.ToString();
                //}
                //else
                //{
                //    distributeTmp.VIEWER = (issuanceExtOrgObj.ObjectInstance as T_HR_POST).T_HR_POSTDICTIONARY.POSTLEVEL.ToString();
                //}
            }
            else
            {
                if (!string.IsNullOrEmpty(issuanceExtOrgObj.ObjectID))
                {
                    distributeTmp.VIEWER = issuanceExtOrgObj.ObjectID;
                }
                else
                {                    
                    distributeTmp.VIEWER = Utility.ReturnIssuranceObjID(issuanceExtOrgObj);
                    
                }
                
            }
            distributeTmp.CREATEDATE = DateTime.Now;
            distributeTmp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            distributeTmp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            distributeTmp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID; ;
            distributeTmp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            distributeTmp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            distributeTmp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            distributeTmp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            if (distributeTmp.VIEWTYPE != ((int)IssuanceObjectType.Post).ToString()) //不提供按部门下的岗位发布
            {
                distributeLists.Add(distributeTmp);
            }
        }

        private IssuanceObjectType GetObjectType(SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj issuanceExtOrgObj)
        {
            if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)
            {
                return IssuanceObjectType.Company;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)
            {
                return IssuanceObjectType.Department;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)
            {
                return IssuanceObjectType.Post;
            }
            else if (issuanceExtOrgObj.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
            {
                return IssuanceObjectType.Employee;
            }
            return IssuanceObjectType.Company;
        }
        #endregion

        #region 流程
        public void SumbitAudit()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();
        }

        //public void BindAduitInfo()
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "housingIssuance";
        //    entity.FormID = IssuanceObj.ISSUANCEID;
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

        //    if (IssuanceObj != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "housingIssuance";//"archivesLending";T_HR_COMPANY
        //        entity.FormID = IssuanceObj.ISSUANCEID; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        audit.XmlObject = DataObjectToXml<T_OA_HOUSEINFOISSUANCE>.ObjListToXml(IssuanceObj, "OA"); 
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
            
            actionFlag = DataActionFlag.SubmitComplete;
        }

        private void SumbitCompleted()
        {
            try
            {
                if (issuanceObj != null)
                {
                    submitflag = true;
                    issuanceObj.UPDATEDATE = DateTime.Now;
                    issuanceObj.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    issuanceObj.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            issuanceObj.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            issuanceObj.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            issuanceObj.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    client.UpdateIssuanceAsync(issuanceObj, houseLists, distributeLists, submitflag);
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_HOUSEINFOISSUANCE>(IssuanceObj, "OA");

            Utility.SetAuditEntity(entity, "housingIssuance", IssuanceObj.ISSUANCEID, strXmlObjectSource);
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
            if (issuanceObj.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            issuanceObj.CHECKSTATE = state;
            submitflag = true;
            client.UpdateIssuanceAsync(issuanceObj, houseLists, distributeLists, submitflag, UserState);
            //SendDocClient.SendDocInfoUpdateAsync(issuanceObj, StrUpdateReturn, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (issuanceObj != null)
                state = issuanceObj.CHECKSTATE;
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
