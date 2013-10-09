using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.OA.UI.SmtOACommonAdminService;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class HouseHireAppForm : BaseForm, IClient, IEntityEditor, IAudit
    {
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private string checkState = "";
        private Action action;
        private FormTypes FormTypeAction;
        private SmtOACommonAdminClient client;
        private string hireAppID = "";
        private MemberHireApp addFrm;
        private T_OA_HOUSEINFO houseInfo;
        private V_HouseHireList houseList;
        private string FromFormFlag = ""; //窗体来源  0 申请管理   1 我的租房管理中 入住    2 我的租房管理中 退房
        private T_OA_HIREAPP hireApp;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private string StrRentType = "0";//合租类型  0 整租  1 合租
        private string StrSettlementType = "0"; //结算方式 0 工资 1 现金
        //public T_OA_HOUSEINFO HouseInfo
        //{
        //    get { return houseInfo; }
        //    set 
        //    { 
        //        houseInfo = value;
        //        this.DataContext = value;
        //    }
        //}

        public V_HouseHireList HouseListInfo
        {
            get { return houseList; }
            set
            {
                houseList = value;
                this.DataContext = value;
            }
        }

        #region 初始化
        /// <summary>
        /// 2010-5-20 by liujx
        /// </summary>
        /// <param name="action">动作 增、删、改、查、审核</param>
        /// <param name="hireAppID">申请ID</param>
        /// <param name="checkState">状态</param>
        /// <param name="FromFlag">来源标记</param>
        public HouseHireAppForm(Action action, string hireAppID, string checkState, string FromFlag)
        {
            InitializeComponent();
            FromFormFlag = FromFlag;
            this.action = action;
            switch (action)
            {
                case Action.Add:
                    FormTypeAction = FormTypes.New;
                    break;
                case Action.Edit:
                    FormTypeAction = FormTypes.Edit;
                    break;
                case Action.AUDIT:
                    FormTypeAction = FormTypes.Audit;
                    break;
                case Action.Read:
                    FormTypeAction = FormTypes.Browse;
                    break;
            }

            this.hireAppID = hireAppID;
            this.checkState = checkState;
            InitEvent();
            InitData();
        }

        //引擎调用 2010-7-10
        public HouseHireAppForm(FormTypes actionformtype, string hireAppID)
        {
            InitializeComponent();
            FromFormFlag = "0";
            this.action = Action.AUDIT;
            this.hireAppID = hireAppID;
            this.checkState = "1";
            InitEvent();
            InitData();
        }



        private void InitEvent()
        {
            client = new SmtOACommonAdminClient();
            client.GetHireAppByIDCompleted += new EventHandler<GetHireAppByIDCompletedEventArgs>(client_GetHireAppByIDCompleted);
            client.AddHireAppCompleted += new EventHandler<AddHireAppCompletedEventArgs>(client_AddHireAppCompleted);
            client.UpdateHireAppCompleted += new EventHandler<UpdateHireAppCompletedEventArgs>(client_UpdateHireAppCompleted);
            client.GetHireAppByHouseListIDCompleted += new EventHandler<GetHireAppByHouseListIDCompletedEventArgs>(client_GetHireAppByHouseListIDCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            client.AddHireRecordCompleted += new EventHandler<AddHireRecordCompletedEventArgs>(client_AddHireRecordCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void client_AddHireRecordCompleted(object sender, AddHireRecordCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("Message"), Utility.GetResourceStr("COMEINISSUCCESSED"));
                }
            }
        }

        void client_GetHireAppByHouseListIDCompleted(object sender, GetHireAppByHouseListIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        HouseListInfo = e.Result.ToList()[0];
                        //hireApp.T_OA_HOUSELIST = HouseListInfo.houselistObj;
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.ProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }



        private void InitData()
        {
            if (action == Action.Add)
            {
                this.sDate.SelectedDate = DateTime.Now;
                this.eDate.SelectedDate = DateTime.Now.AddYears(1);
                houseInfo = new T_OA_HOUSEINFO();
                houseList = new V_HouseHireList();
                hireApp = new T_OA_HIREAPP();
                hireApp.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            }
            else
            {
                if (action == Action.AUDIT)
                {
                    actionFlag = DataActionFlag.SubmitComplete;

                }
                client.GetHireAppByIDAsync(hireAppID);
            }
            if (checkState != ((int)CheckStates.UnSubmit).ToString() && checkState != ((int)CheckStates.UnApproved).ToString() && FromFormFlag == "0")   //只有未提交和未通过才能修改
            {
                if (action != Action.Add)
                {
                    SetReadOnly();
                }
            }
            else
            {
                SetToolBar();
            }

            //if (action == Action.Return)
            //{
            //    //SetReturnBar();
            //    this.sDate.IsEnabled = false;
            //    this.eDate.IsEnabled = false;
            //    this.rbtPay.IsEnabled = false;
            //    this.RbtShared.IsEnabled = false;
            //    this.rbtWhole.IsEnabled = false;
            //    this.RbtCash.IsEnabled = false;
            //}
        }

        #endregion

        #region 完成事件
        private void client_GetHireAppByIDCompleted(object sender, GetHireAppByIDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {


                        if (actionFlag == DataActionFlag.SubmitFlow)
                        {

                            //SumbitFlow();
                            return;
                        }
                        else
                        {
                            hireApp = e.Result.ToList()[0];
                            client.GetHireAppByHouseListIDAsync(hireApp.T_OA_HOUSELIST.HOUSELISTID);
                        }
                        this.sDate.SelectedDate = hireApp.STARTDATE;
                        this.eDate.SelectedDate = hireApp.ENDDATE;
                        this.bDate.SelectedDate = hireApp.BACKDATE;
                        if (action == Action.AUDIT)
                        {
                            //audit.XmlObject = DataObjectToXml<T_OA_HIREAPP>.ObjListToXml(hireApp, "OA");
                        }
                        //BindAduitInfo();
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
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

        private void client_AddHireAppCompleted(object sender, AddHireAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
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
                            FormTypeAction = FormTypes.Edit;
                            action = Action.Edit;
                            this.hireAppID = hireApp.HIREAPPID;
                            InitData();
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }


                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    client.GetHireAppByIDAsync(hireApp.HIREAPPID);
                        //}
                        //else
                        //{
                        //    if (GlobalFunction.IsSaveAndClose(refreshType))
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "HOUSEHIREAPP"));
                        //        RefreshUI(refreshType);
                        //    }
                        //    else
                        //    {

                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "HOUSEHIREAPP"));
                        //        this.action = Action.Edit;
                        //        this.hireAppID = hireApp.HIREAPPID;
                        //        InitData();
                        //    }
                        //}
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

                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void client_UpdateHireAppCompleted(object sender, UpdateHireAppCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {

                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {

                        if (e.UserState.ToString() == "Edit")
                        {
                            if (FromFormFlag == "1") //入住操作  修改完ISOK之后 需要添加一条出租记录
                            {
                                AddHireRecord();
                            }
                            else
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



                        ////HtmlPage.Window.Alert("修改房源记录成功！");                        
                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    //actionFlag = DataActionFlag.SubmitComplete;
                        //    SumbitFlow();
                        //}
                        //else
                        //{
                        //    RefreshUI(RefreshedTypes.ProgressBar);
                        //    if (actionFlag == DataActionFlag.SubmitComplete)
                        //    {
                        //        if (action == Action.Add || action == Action.Edit)
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITHOUSEHIREAPPSUCCESSED"));
                        //        }
                        //        else
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "HOUSEHIREAPP"));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (FromFormFlag == "1") //入住操作  修改完ISOK之后 需要添加一条出租记录
                        //        {
                        //            AddHireRecord();
                        //        }
                        //        else
                        //        {
                        //            if (GlobalFunction.IsSaveAndClose(refreshType))
                        //            {
                        //                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "HOUSEHIREAPP"));
                        //            }
                        //            else
                        //            {
                        //                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "HOUSEHIREAPP"));
                        //                InitData();
                        //            }
                        //        }
                        //    }
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
        #endregion

        #region 添加入住记录
        private void AddHireRecord()
        {
            //添加租赁记录并将，出租申请的标志改为确定入住
            T_OA_HIRERECORD hirerecord = new T_OA_HIRERECORD();
            hirerecord.HIRERECORD = System.Guid.NewGuid().ToString();
            hirerecord.RENTER = hireApp.RENTTYPE;
            hirerecord.T_OA_HIREAPP = hireApp;
            hirerecord.MANAGECOST = Convert.ToDecimal(hireApp.MANAGECOST);
            hirerecord.RENTCOST = Convert.ToDecimal(hireApp.RENTCOST);
            hirerecord.WATER = 0;
            hirerecord.ELECTRICITY = 0;
            hirerecord.OTHERCOST = 0;
            hirerecord.WATERNUM = 0;
            hirerecord.ELECTRICITYNUM = 0;
            hirerecord.SETTLEMENTDATE = System.DateTime.Now;
            hirerecord.SETTLEMENTTYPE = hireApp.SETTLEMENTTYPE;//付款方式
            hirerecord.ISSETTLEMENT = "0"; //是否结算

            hirerecord.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            hirerecord.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            hirerecord.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            hirerecord.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            hirerecord.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            hirerecord.CREATEDATE = DateTime.Now;

            hirerecord.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            hirerecord.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            hirerecord.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            hirerecord.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            hirerecord.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            client.AddHireRecordAsync(hirerecord);
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {

            string StrReturn = "";
            switch (FromFormFlag)
            {
                case "0":
                    if (action == Action.Add)
                    {
                        StrReturn = Utility.GetResourceStr("ADDTITLE", "HOUSEHIREAPP");
                    }
                    else
                    {
                        StrReturn = Utility.GetResourceStr("EDITTITLE", "HOUSEHIREAPP");
                    }
                    break;
                case "1"://入住
                    StrReturn = Utility.GetResourceStr("COMEINHIREHOUSE");
                    this.sDate.IsEnabled = false;
                    this.eDate.IsEnabled = false;
                    this.bDate.IsEnabled = false;
                    this.rbtPay.IsEnabled = false;
                    this.RbtShared.IsEnabled = false;
                    this.RbtCash.IsEnabled = false;
                    this.rbtWhole.IsEnabled = false;
                    //this.tabaudit.Visibility = Visibility.Collapsed;
                    break;
                case "2"://退房
                    StrReturn = Utility.GetResourceStr("CHECKOUTHOUSE");
                    this.sDate.IsEnabled = false;
                    this.eDate.IsEnabled = false;
                    this.bDate.IsEnabled = true;
                    this.rbtPay.IsEnabled = false;
                    this.RbtShared.IsEnabled = false;
                    this.RbtCash.IsEnabled = false;
                    this.rbtWhole.IsEnabled = false;
                    //this.tabaudit.Visibility = Visibility.Collapsed;
                    break;
            }
            return StrReturn;
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
                    Select();
                    break;
                case "3":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    SumbitAudit();
                    break;
                case "4"://入住操作
                    Save();
                    break;
                case "5"://退房
                    Save();
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
            if (FormTypeAction != FormTypes.Browse && FormTypeAction != FormTypes.Audit)
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

            switch (FromFormFlag)
            {
                case "0":
                    //ToolbarItem item = new ToolbarItem
                    //{
                    //    DisplayType = ToolbarItemDisplayTypes.Image,
                    //    Key = "3",
                    //    Title = Utility.GetResourceStr("SUBMITAUDIT"),
                    //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
                    //};
                    //items.Add(item);

                    ToolbarItem item = new ToolbarItem
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
                    break;
                case "1":
                    ToolbarItem item1 = new ToolbarItem
                    {

                        DisplayType = ToolbarItemDisplayTypes.Image,
                        Key = "4",
                        Title = Utility.GetResourceStr("OKCOMEIN"),
                        ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
                    };
                    items.Add(item1);
                    break;
                case "2":
                    ToolbarItem item2 = new ToolbarItem
                    {

                        DisplayType = ToolbarItemDisplayTypes.Image,
                        Key = "5",
                        Title = Utility.GetResourceStr("CHECKOUTHOUSE"),
                        ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
                    };
                    items.Add(item2);
                    break;
            }
            return items;
        }

        private List<ToolbarItem> CreateFormReturnButton()
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

        #region 自定义函数

        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void SetReadOnly()
        {
            this.sDate.IsEnabled = false;
            this.eDate.IsEnabled = false;
            this.bDate.IsEnabled = false;
        }

        private void SetToolBar()
        {
            ToolbarItems = CreateFormNewButton();
            if (checkState == ((int)CheckStates.UnSubmit).ToString() && FromFormFlag == "0")
            {
                //scvAudit.Visibility = Visibility.Collapsed;
            }
        }
        //private void SetReturnBar()
        //{
        //    ToolbarItems = CreateFormReturnButton();
        //    this.sDate.IsEnabled = false;
        //    this.eDate.IsEnabled = false;
        //    this.bDate.IsEnabled = true; 
        //}

        private void Save()
        {
            try
            {
                string StrStartDt = "";   //开始时间

                string StrEndDt = "";    //结束时间
                string StrOutDt = "";//退房时间
                if (houseList == null || houseList.houseInfoObj == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECT", "HOUSEMANAGERINFO"));
                }
                if (rbtWhole.IsChecked == true)
                {
                    StrRentType = "0";
                }
                if (RbtShared.IsChecked == true)
                {
                    StrRentType = "1";
                }
                if (rbtPay.IsChecked == true)
                {
                    StrSettlementType = "0";
                }
                if (RbtCash.IsChecked == true)
                {
                    StrSettlementType = "1";
                }
                if (!string.IsNullOrEmpty(this.sDate.Text.ToString()))
                {
                    StrStartDt = this.sDate.Text.ToString();
                }
                else
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                    return;


                }
                if (!string.IsNullOrEmpty(this.eDate.Text.ToString()))
                {
                    StrEndDt = this.eDate.Text.ToString();
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                    return;
                }

                DateTime DtStart = System.Convert.ToDateTime(StrStartDt);
                DateTime DtEnd = System.Convert.ToDateTime(StrEndDt);

                if (DtStart >= DtEnd)
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                    return;

                }
                StrOutDt = this.bDate.Text.ToString();
                if (FromFormFlag == "2") //退房
                {
                    if (string.IsNullOrEmpty(StrOutDt))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("BACKDATENOTNULL"));
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(StrOutDt))
                {
                    DateTime DtOut = System.Convert.ToDateTime(StrOutDt);
                    if (DtStart >= DtOut)
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATOUTTIME"));
                        return;
                    }
                    if (DtOut <= System.DateTime.Now)//退房时间不能小于当前时间
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CHECKOUTDATENOLESSTHANNOW"));
                        return;
                    }
                }

                if (Check())
                {

                    if (action != Action.Return)
                    {
                        hireApp.STARTDATE = Convert.ToDateTime(sDate.SelectedDate);
                        hireApp.ENDDATE = Convert.ToDateTime(eDate.SelectedDate);
                    }
                    else
                    {
                        //hireApp.T_OA_HOUSEINFO.ISRENT = "0";
                        hireApp.BACKDATE = Convert.ToDateTime(bDate.SelectedDate);
                    }
                    houseInfo = houseList.houseInfoObj;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    if (action == Action.Add)
                    {
                        hireApp.HIREAPPID = Guid.NewGuid().ToString();
                        hireApp.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                        hireApp.T_OA_HOUSELIST = houseList.houselistObj;

                        //hireApp.T_OA_HOUSELIST.HOUSELISTID = houseInfo.HOUSEID;
                        hireApp.MANAGECOST = System.Convert.ToInt32(txtManageCost.Text.ToString()); //houseInfo.MANAGECOST;//管理费


                        hireApp.DEPOSIT = houseInfo.DEPOSIT;
                        hireApp.RENTCOST = houseInfo.RENTCOST;
                        hireApp.RENTTYPE = StrRentType;//出租类型
                        hireApp.SETTLEMENTTYPE = StrSettlementType; //结算方式
                        hireApp.ISBACK = "0";  //是否退房
                        hireApp.ISOK = "0";//是否确认

                        hireApp.STARTDATE = DtStart;
                        hireApp.ENDDATE = DtEnd;

                        hireApp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        hireApp.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        hireApp.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        hireApp.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        hireApp.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        hireApp.CREATEDATE = DateTime.Now;

                        hireApp.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        hireApp.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        hireApp.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        hireApp.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        hireApp.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;


                        client.AddHireAppAsync(hireApp, "Add");

                    }
                    else
                    {
                        hireApp.UPDATEDATE = DateTime.Now;
                        hireApp.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        hireApp.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        //hireApp.T_OA_HOUSELIST.HOUSELISTID = houseList.houselistObj.HOUSELISTID;
                        hireApp.T_OA_HOUSELIST = houseList.houselistObj;
                        hireApp.DEPOSIT = houseInfo.DEPOSIT;
                        hireApp.RENTCOST = houseInfo.RENTCOST;
                        if (FromFormFlag == "1")//入住
                        {
                            hireApp.ISOK = "1";
                        }
                        if (FromFormFlag == "2")//退房
                        {
                            hireApp.ISBACK = "1";
                            hireApp.BACKDATE = bDate.SelectedDate;
                            if (bDate.SelectedDate <= sDate.SelectedDate)
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("RETURNDATENOTLESSSTARTDATE"));
                            }
                        }
                        hireApp.RENTTYPE = StrRentType;//出租类型
                        hireApp.SETTLEMENTTYPE = StrSettlementType; //结算方式

                        client.UpdateHireAppAsync(hireApp, "Edit");
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        private void Close()
        {
            RefreshUI(refreshType);
        }

        private bool Check()
        {
            //if (hireApp.T_OA_HOUSEINFO == null)
            //{
            //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "HOUSEINFO"));
            //    return false;
            //}
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
            if (Convert.ToDateTime(sDate.SelectedDate) > Convert.ToDateTime(eDate.SelectedDate))
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "HIREDATE,EXPECTEDBACKDATE"));
                return false;
            }
            if (action == Action.Return)
            {
                try
                {
                    Convert.ToDateTime(bDate.SelectedDate);
                }
                catch (Exception ex)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "HIREDATE,EXPECTEDBACKDATE"));
                    return false;
                }
                if (Convert.ToDateTime(sDate.SelectedDate) > Convert.ToDateTime(bDate.SelectedDate))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEERROR", "HIREDATE,BACKDATE"));
                    return false;
                }
            }
            return true;
        }

        private void Select()
        {
            //addFrm = new HouseHireAppChooseHouseForm();
            //addFrm = new memberh
            addFrm = new MemberHireApp();
            EntityBrowser browser = new EntityBrowser(addFrm);
            browser.FormType = FormTypes.Browse;
            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        {
            if (addFrm.selectHouseInfoObj != null)
            {
                HouseListInfo = addFrm.selectHouseInfoObj;
                this.txtManageCost.Text = ((int)(HouseListInfo.houseInfoObj.MANAGECOST / HouseListInfo.houseInfoObj.Number)).ToString();//获取管理费的平均值
            }
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
        //    entity.ModelCode = "houseHireApp";
        //    entity.FormID = hireApp.HIREAPPID;
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

        //    if (hireApp != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "houseHireApp";//"archivesLending";T_HR_COMPANY
        //        entity.FormID = hireApp.HIREAPPID; //"0b6c8e80-69fa-4f54-810a-1f0d339c6603";//Company.COMPANYID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        //audit.XmlObject = DataObjectToXml<T_OA_HIREAPP>.ObjListToXml(hireApp, "OA");
        //        //audit.Submit();
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
            actionFlag = DataActionFlag.SubmitComplete;
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
            try
            {
                if (hireApp != null)
                {
                    hireApp.UPDATEDATE = DateTime.Now;
                    hireApp.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    hireApp.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    hireApp.T_OA_HOUSELIST = houseList.houselistObj;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            hireApp.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            //hireApp.T_OA_HOUSEINFO.ISRENT = "1";
                            hireApp.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            hireApp.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }

                    client.UpdateHireAppAsync(hireApp);
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

        #region radion按钮事件

        //整租
        private void rbtWhole_Click(object sender, RoutedEventArgs e)
        {
            rbtWhole.IsChecked = true;
            RbtShared.IsChecked = false;

        }
        //合租
        private void RbtShared_Click(object sender, RoutedEventArgs e)
        {
            RbtShared.IsChecked = true;
            rbtWhole.IsChecked = false;

        }
        //工资
        private void rbtPay_Click(object sender, RoutedEventArgs e)
        {
            rbtPay.IsChecked = true;
            RbtCash.IsChecked = false;
        }
        //现金
        private void RbtCash_Click(object sender, RoutedEventArgs e)
        {
            rbtPay.IsChecked = false;
            RbtCash.IsChecked = true;

        }
        #endregion

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_HIREAPP>(hireApp, "OA");

            Utility.SetAuditEntity(entity, "houseHireApp", hireApp.HIREAPPID, strXmlObjectSource);
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
            if (hireApp.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            hireApp.CHECKSTATE = state;
            client.UpdateHireAppAsync(hireApp, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (hireApp != null)
                state = hireApp.CHECKSTATE;
            if (FormTypeAction == FormTypes.Browse)
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
