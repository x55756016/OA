/********************************************************************************
//出差主页面，alter by ken 2013/3/27
*********************************************************************************/
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
using SMT.SaaS.OA.UI.Views.Travelmanagement;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class BusinessApplicationsForm : BaseForm, IEntityEditor
    {
        #region 全局变量
        public string OpenFrom = string.Empty;
        private string businesstrID = string.Empty;
        //public string missionReportsID = string.Empty;
        public string travelReimbursementID = string.Empty;
        //public string reportCheckState = string.Empty;
        public string trCheckState = string.Empty;
        public string traverlCheck = string.Empty;
        public string FormId = string.Empty;//窗体id
        public V_Travelmanagement TraveView = new V_Travelmanagement();
        public TravelType tlType;//类型
        private FormTypes actions;
        private SmtOAPersonOfficeClient Travelmanagement;
        public EntityBrowser ParentEntityBrowser { get; set; }
        //private List<T_HR_POST> entlist;
        //private List<T_HR_DEPARTMENT> allDepartments;
        //private List<T_HR_COMPANY> allCompanys;
        //private OrganizationServiceClient organClient = new OrganizationServiceClient();
        //修改行程
        public bool isAlterTrave = false;
        #endregion

        public BusinessApplicationsForm(FormTypes action, string businesstrID)
        {

            InitializeComponent();

            this.actions = action;
            this.businesstrID = businesstrID;
            InitEvent();
            this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        }
        /// <summary>
        /// for工作计划，从mvc平台打开
        /// </summary>
        /// <param name="action"></param>
        /// <param name="businesstrID"></param>
        public BusinessApplicationsForm(FormTypes action, string businesstrID,string OpenType)
        {
            if (OpenType == "FromMVC")
            {
                OpenFrom = OpenType;
               
            }
            InitializeComponent();

            this.actions = action;
            this.businesstrID = businesstrID;
            InitEvent();
            this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        }

        public BusinessApplicationsForm(FormTypes action, string businesstrID, bool WhetherReimbursement)
        {
            InitializeComponent();
            if (WhetherReimbursement == true)
            {
                TabTravel.SelectedIndex = 1;
            }
            else
            {
                TabTravel.SelectedIndex = 2;
            }
            this.actions = action;
            this.businesstrID = businesstrID;
            InitEvent();
            this.Loaded += new RoutedEventHandler(BusinessApplicationsForm_Loaded);
        }

        private void InitEvent()
        {
            Travelmanagement = new SmtOAPersonOfficeClient();
            Travelmanagement.GetAccordingToBusinesstripIdCheckCompleted += new EventHandler<GetAccordingToBusinesstripIdCheckCompletedEventArgs>(Travelmanagement_GetAccordingToBusinesstripIdCheckCompleted);
           
        }

        /// <summary>
        /// 通过出差申请ID查询报告、报销的ID及状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BusinessApplicationsForm_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);//打开进度圈
            //organClient.GetALLCompanyViewAsync("");
            //2013/3/27停止加载所有组织架构，直接加载出差业务数据
            Travelmanagement.GetAccordingToBusinesstripIdCheckAsync(businesstrID);
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("获取出差报销，出差申请id：" + businesstrID);
        }

        void Travelmanagement_GetAccordingToBusinesstripIdCheckCompleted(object sender, GetAccordingToBusinesstripIdCheckCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    TraveView = e.Result;
                    //missionReportsID = TraveView.ReportId;
                    travelReimbursementID = TraveView.TrId;
                    traverlCheck = TraveView.TraveAppCheckState;
                    //reportCheckState = TraveView.ReportCheckState;
                    trCheckState = TraveView.TrCheckState;
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("打开出差form获取到的出差报销id：" + TraveView.TrId);

                }
                else
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("GetAccordingToBusinesstripId返回结果为空");
                }
                //判断出差报销是否存在
                bool BoolReimID = !string.IsNullOrEmpty(travelReimbursementID) && travelReimbursementID != "空";
                if (false == BoolReimID) traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab

                if (traverlCheck != Convert.ToInt32(CheckStates.Approved).ToString())
                {
                    traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab
                }
                else
                {
                    if ((trCheckState == "0" || trCheckState == "1" || trCheckState == "3") && !isAlterTrave)
                    {
                        TabTravel.SelectedIndex = 2;
                        if (actions != FormTypes.Browse && actions != FormTypes.Edit)
                        {
                            tbbTravelapplication.Visibility = Visibility.Collapsed;//隐藏出差申请Tab
                            //TabReport.Visibility = Visibility.Collapsed;//隐藏出差报告Tab
                        }
                    }
                }

                if (actions == FormTypes.New)//新增
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                    TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.New, "");
                    TravelapplicationForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                    TravelapplicationForm.ParentEntityBrowser = this.ParentEntityBrowser;
                    EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                    TravelBrowser.FormType = FormTypes.New;                    
                    TravelBrowser.MinWidth = 980;
                    TravelBrowser.MinHeight = 445;
                    TravelapplicationGd.Children.Add(TravelBrowser);
                }
                else if (actions == FormTypes.Edit)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                    //TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Edit, businesstrID);
                    //2012-9-21 ljx 
                    TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Edit, TraveView.Travelmanagement.BUSINESSTRIPID);
                    TravelapplicationForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                    TravelapplicationForm.ParentEntityBrowser = this.ParentEntityBrowser;
                    EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                    TravelBrowser.FormType = FormTypes.Edit;
                    //TravelBrowser.MinWidth = 980;
                    TravelBrowser.MinHeight = 445;
                    TravelapplicationGd.Children.Add(TravelBrowser);


                    if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //出差报销
                        TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Edit, travelReimbursementID, businesstrID);
                        TravelReimbursementForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                        TravelReimbursementForm.ParentEntityBrowser = this.ParentEntityBrowser;
                        TravelReimbursementForm.OpenFrom = OpenFrom;
                        EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                        TravelReimbursementBrowser.FormType = FormTypes.Edit;
                        //Canvas can = new Canvas();
                        //TravelReimbursementBrowser.MinWidth = 980;
                        TravelReimbursementBrowser.MinHeight = 445;
                        TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                    }
                }
                else if (actions == FormTypes.Browse)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                    TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Browse, businesstrID);
                    TravelapplicationForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                    EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                    TravelBrowser.FormType = FormTypes.Browse;
                    TravelBrowser.MinWidth = 728;
                    TravelBrowser.MinHeight = 445;
                    TravelapplicationGd.Children.Add(TravelBrowser);


                    if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //出差报销
                        TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Browse, travelReimbursementID, businesstrID);
                        TravelReimbursementForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                        EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                        TravelReimbursementBrowser.FormType = FormTypes.Browse;
                        //Canvas can = new Canvas();
                        TravelReimbursementBrowser.MinWidth = 728;
                        TravelReimbursementBrowser.MinHeight = 445;
                        TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                    }
                }
                else if (actions == FormTypes.Audit)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                    TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Audit, businesstrID);
                    TravelapplicationForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                    EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                    TravelBrowser.FormType = FormTypes.Audit;
                    TravelBrowser.MinWidth = 728;
                    TravelBrowser.MinHeight = 445;
                    TravelapplicationGd.Children.Add(TravelBrowser);


                    if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                    {
                        //出差报销
                        TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Audit, travelReimbursementID, businesstrID);
                        TravelReimbursementForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                        EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                        TravelReimbursementBrowser.FormType = FormTypes.Audit;
                        //Canvas can = new Canvas();
                        TravelReimbursementBrowser.MinWidth = 728;
                        TravelReimbursementBrowser.MinHeight = 445;
                        TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                    }
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);//停止进度圈

                    if (traverlCheck != "3"&& !isAlterTrave)
                    {
                        tbbTravelapplication.Visibility = Visibility.Collapsed;//出差申请Tab
                    }
                    else
                    {   //重新提交（或修改行程）
                        TravelRequestForm TravelapplicationForm = new TravelRequestForm(FormTypes.Resubmit, businesstrID);
                        TravelapplicationForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                        EntityBrowser TravelBrowser = new EntityBrowser(TravelapplicationForm);
                        TravelBrowser.FormType = FormTypes.Resubmit;
                        TravelBrowser.MinWidth = 980;
                        TravelBrowser.MinHeight = 445;
                        TravelapplicationGd.Children.Add(TravelBrowser);
                        if (isAlterTrave)
                        {
                            //traveformFather.Visibility = Visibility.Collapsed;
                            //TravelapplicationGd.Visibility = Visibility.Visible;
                            //tbbTravelapplication.Visibility = Visibility.Visible;
                            //TabTravel.SelectedIndex = 1;
                            TravelapplicationForm.isAlterTrave = true;
                        }
                    }

                    if (trCheckState != "3")
                    {
                        traveformFather.Visibility = Visibility.Collapsed;//出差报销Tab
                    }
                    else
                    {
                        if (BoolReimID && traverlCheck == Convert.ToInt32(CheckStates.Approved).ToString())
                        {
                            //出差报销
                            TravelReimbursementControl TravelReimbursementForm = new TravelReimbursementControl(FrameworkUI.FormTypes.Resubmit, travelReimbursementID, businesstrID);
                            TravelReimbursementForm.OnUIRefreshed += TravelapplicationForm_OnUIRefreshed;
                            EntityBrowser TravelReimbursementBrowser = new EntityBrowser(TravelReimbursementForm);
                            TravelReimbursementBrowser.FormType = FormTypes.Resubmit;
                            TravelReimbursementBrowser.MinWidth = 980;
                            TravelReimbursementBrowser.MinHeight = 445;
                            TravelReimbursementGd.Children.Add(TravelReimbursementBrowser);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("打开出差form，获取出差异常：" + ex.ToString());
            }
        }

        void TravelapplicationForm_OnUIRefreshed(object sender, UIRefreshedEventArgs args)
        {
           this.RefreshUI(args.RefreshedType);
        }

      
        #region IEntityEditor 成员
        public string GetTitle()
        {
            return "出差管理";
        }

        public string GetStatus()
        {
            return "";
        }

        public void DoAction(string actionType)
        {

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
        private void Close()
        {
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }
        #endregion
    }
}
