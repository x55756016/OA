/********************************************************************************
//出差报销form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;
using System.Windows.Browser;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        public string OpenFrom = string.Empty;
        private SmtOAPersonOfficeClient OaPersonOfficeClient;
        //private PersonnelServiceClient HrPersonnelclient;
        private OaInterfaceClient OaInterfaceClient;//预算费用报销单号使用
        private bool isPageloadCompleted = false;//控制Tab切换时的数据加载 
        //private V_Travelmanagement businesstrip = new V_Travelmanagement();
        private T_OA_TRAVELREIMBURSEMENT TravelReimbursement;
       
        private FormTypes formType;
        private V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        //private SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER order = new SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER();
        private bool IsAudit = true;
        private bool Resubmit = true;
        private string travelReimbursementID = "";
        //private SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient AttendanceClient = new SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient();
        private ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TravelDetailList_Golbal = new ObservableCollection<T_OA_REIMBURSEMENTDETAIL>();
        public List<T_SYS_DICTIONARY> ListVechileLevel = new List<T_SYS_DICTIONARY>();
        public T_OA_REIMBURSEMENTDETAIL TrDetail;
        public List<T_OA_CANTAKETHEPLANELINE> cantaketheplaneline = new List<T_OA_CANTAKETHEPLANELINE>();//可乘坐飞机线路设置
        public List<T_OA_TAKETHESTANDARDTRANSPORT> takethestandardtransport = new List<T_OA_TAKETHESTANDARDTRANSPORT>();//交通工具标准设置
        //private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> buipList = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> buipList = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        public T_OA_TRAVELSOLUTIONS travelsolutions = new T_OA_TRAVELSOLUTIONS();
        public List<T_OA_AREAALLOWANCE> areaallowance = new List<T_OA_AREAALLOWANCE>();
        private SMTLoading loadbar = new SMTLoading();
        //private List<string> citysStartList_Golbal = new List<string>();
        //private List<string> citysEndList_Golbal = new List<string>();
        //public double Fees = 0;
        private string EmployeeName = string.Empty;//出差人
        public string EmployeePostLevel = string.Empty;//出差人的岗位级别
        public string depName = string.Empty;//出差人的所属部门
        public string postName = string.Empty;//出差人的所属岗位
        public string companyName = string.Empty;//出差人所属公司(初始化时用)
        private string businesstrID = string.Empty;
        private List<T_OA_AREACITY> areacitys;
        public string UserState = "Audit";
        private string state = string.Empty;
        private string UsableMoney = string.Empty;//用来存储可用额度
        public bool needsubmit = false;//提交审核
        //private bool isSubmit = false;//是提交的话不弹出保存成功提示
        public bool clickSubmit = false;//单击了提交按钮
        private bool BtnNewButton = false;//单击新建按钮
        private bool SaveBtn = false;//保存数据
        private bool InitFB = false;//初始化预算数据
        private string StrPayInfo = "";//支付信息,用了传递给手机源数据

        private bool canSubmit = false;//能否提交审核

        public EntityBrowser ParentEntityBrowser { get; set; }//关闭窗口用

        public T_OA_TRAVELREIMBURSEMENT TravelReimbursement_Golbal
        {
            get { return TravelReimbursement; }
            set
            {
                this.DataContext = value;
                TravelReimbursement = value;
            }
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            OaPersonOfficeClient = new SmtOAPersonOfficeClient();
            //HrPersonnelclient = new PersonnelServiceClient();
            areacitys = new List<T_OA_AREACITY>();
            OaInterfaceClient = new OaInterfaceClient();
            //HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetAllEmployeePostBriefByEmployeeIDCompleted);
            //HrPersonnelclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetEmployeePostBriefByEmployeeIDCompleted);
            OaPersonOfficeClient.UpdateTravelReimbursementCompleted += new EventHandler<UpdateTravelReimbursementCompletedEventArgs>(TrC_UpdateTravelReimbursementCompleted);
            OaPersonOfficeClient.GetTravelReimbursementByIdCompleted += new EventHandler<GetTravelReimbursementByIdCompletedEventArgs>(TrC_GetTravelReimbursementByIdCompleted);
            //OaPersonOfficeClient.GetTravelReimbursementDetailCompleted += new EventHandler<GetTravelReimbursementDetailCompletedEventArgs>(TrC_GetTravelReimbursementDetailCompleted);
            OaPersonOfficeClient.GetTravelSolutionByCompanyIDCompleted += new EventHandler<GetTravelSolutionByCompanyIDCompletedEventArgs>(TrC_GetTravelSolutionByCompanyIDCompleted);
            OaPersonOfficeClient.GetTravleAreaAllowanceByPostValueCompleted += new EventHandler<GetTravleAreaAllowanceByPostValueCompletedEventArgs>(TrC_GetTravleAreaAllowanceByPostValueCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);//保存费用
            fbCtr.InitDataComplete += new EventHandler<FrameworkUI.FBControls.ChargeApplyControl.InitDataCompletedArgs>(fbCtr_InitDataComplete);
            fbCtr.ItemSelectChange+=fbCtr_ItemSelectChange;
            OaPersonOfficeClient.DeleteTravelReimbursementByBusinesstripIdCompleted += new EventHandler<DeleteTravelReimbursementByBusinesstripIdCompletedEventArgs>(Travelmanagement_DeleteTravelReimbursementByBusinesstripIdCompleted);
        }

        #endregion

        #region 出差报销构造
        public TravelReimbursementControl()
        {
            InitializeComponent();
        }

        public TravelReimbursementControl(FormTypes action, string travelReimbursementID, string StrBusinesstrID)
        {
            InitializeComponent();
            this.formType = action;
            this.travelReimbursementID = travelReimbursementID;
            this.businesstrID = StrBusinesstrID;
            InitEvent();
            if (formType == FormTypes.New)
            {
                NewMaster_Golbal();
            }

            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                DaGrEditScrollView.Visibility = Visibility.Collapsed;
                DaGrReadOnlyScrollView.Visibility = Visibility.Visible;                
            }
            else
            {                
                DaGrEditScrollView.Visibility = Visibility.Visible;
                DaGrReadOnlyScrollView.Visibility = Visibility.Collapsed;
            }
            this.Loaded += new RoutedEventHandler(TravelReimbursementControl_Loaded);
            //this.DaGrReadOnly.Loaded += DaGrReadOnly_Loaded;
        }

        void TravelReimbursementControl_Loaded(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            //entBrowser.BtnDelete.Click += BtnDelete_Click;


            GetVechileLevelInfos();
            if (isPageloadCompleted == true) return;//如果已经加载过，再次切换时就不再加载
            if (formType == FormTypes.Browse || formType == FormTypes.Audit)
            {
                Utility.InitFileLoad("TravelReimbursement", travelReimbursementID, formType, uploadFile);
                BrowseShieldedControl();
            }
            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);
            FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;//修改
            FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;//删除
            FormToolBar1.BtnView.Visibility = Visibility.Collapsed;//查看
            FormToolBar1.btnRefresh.Visibility = Visibility.Collapsed;//刷新
            FormToolBar1.btnReSubmit.Visibility = Visibility.Collapsed;//重新提交
            FormToolBar1.btnAudit.Visibility = Visibility.Collapsed;//审核
            FormToolBar1.cbxCheckState.Visibility = Visibility.Collapsed;
            FormToolBar1.btnPrint.Visibility = Visibility.Collapsed;//打印
            FormToolBar1.btnOutPDF.Visibility = Visibility.Collapsed;//导出pdf
            FormToolBar1.btnOutExcel.Visibility = Visibility.Collapsed;//导出excel
            FormToolBar1.stpCheckState.Visibility = Visibility.Collapsed;//检查审核状态
            FormToolBar1.retEdit.Visibility = Visibility.Collapsed;
            FormToolBar1.retRead.Visibility = Visibility.Collapsed;
            FormToolBar1.retRefresh.Visibility = Visibility.Collapsed;
            FormToolBar1.retAudit.Visibility = Visibility.Collapsed;
            FormToolBar1.retDelete.Visibility = Visibility.Collapsed;
            FormToolBar1.retPDF.Visibility = Visibility.Collapsed;
            FormToolBar1.retAuditNoPass.Visibility = Visibility.Collapsed;

            if (formType != FormTypes.New)
            {
                if (!string.IsNullOrEmpty(travelReimbursementID))
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    OaPersonOfficeClient.GetTravelReimbursementByIdAsync(travelReimbursementID);
                    Utility.InitFileLoad("TravelReimbursement", travelReimbursementID, formType, uploadFile);
                }
            }
            if (OpenFrom == "FromMVC")
            {
                fbCtr.Visibility = Visibility.Collapsed;
                lblFees.Visibility = Visibility.Collapsed;
                fbChkBox.Visibility = Visibility.Collapsed;
                this.FormToolBar1.Visibility = Visibility.Collapsed;
                hide();
                //RegisterOnBeforeUnload();
                //HtmlPage.RegisterScriptableObject 方法 
                //注册托管对象以便用于通过 JavaScript 代码的可脚本化访问。
                HtmlPage.RegisterScriptableObject("TravelReimbursement", this);
            }
        }

        #endregion
    }
}
