/********************************************************************************
//出差报销form，alter by ken 2013/3/27
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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using System.Collections.ObjectModel;
using SMT.SaaS.OA.UI.Views.Travelmanagement;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using System.Windows.Data;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PermissionWS;
using System.Text.RegularExpressions;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.TravelExpApplyMaster;
using SMT.SaaS.OA.UI.UserControls.Travelmanagement;
using SMT.Saas.Tools.FBServiceWS;
using SMT.SAAS.Application;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;
using System.IO;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        private SmtOAPersonOfficeClient OaPersonOfficeClient;
        private PersonnelServiceClient HrPersonnelclient;
        private OaInterfaceClient OaInterfaceClient;//预算费用报销单号使用

        private bool isloaded = false;//控制Tab切换时的数据加载 
        private V_Travelmanagement businesstrip = new V_Travelmanagement();
        private T_OA_TRAVELREIMBURSEMENT travelReimbursement;
       
        private FormTypes actions;
        private V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        //private SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER order = new SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER();
        private bool IsAudit = true;
        private bool Resubmit = true;
        private string travelReimbursementID = "";
        //private SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient AttendanceClient = new SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient();
        private ObservableCollection<T_OA_REIMBURSEMENTDETAIL> TrList = new ObservableCollection<T_OA_REIMBURSEMENTDETAIL>();
        public List<T_SYS_DICTIONARY> ListVechileLevel = new List<T_SYS_DICTIONARY>();
        public T_OA_REIMBURSEMENTDETAIL TrDetail;
        public List<T_OA_CANTAKETHEPLANELINE> cantaketheplaneline = new List<T_OA_CANTAKETHEPLANELINE>();//可乘坐飞机线路设置
        public List<T_OA_TAKETHESTANDARDTRANSPORT> takethestandardtransport = new List<T_OA_TAKETHESTANDARDTRANSPORT>();//交通工具标准设置
        //private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> buipList = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> buipList = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        public T_OA_TRAVELSOLUTIONS travelsolutions = new T_OA_TRAVELSOLUTIONS();
        public List<T_OA_AREAALLOWANCE> areaallowance = new List<T_OA_AREAALLOWANCE>();
        private SMTLoading loadbar = new SMTLoading();
        private List<string> citycode = new List<string>();
        private List<string> cityscode = new List<string>();
        public double Fees = 0;
        private string EmployeeName = string.Empty;//出差人
        public string EmployeePostLevel = string.Empty;//出差人的岗位级别
        public string depName = string.Empty;//出差人的所属部门
        public string postName = string.Empty;//出差人的所属岗位
        public string companyName = string.Empty;//出差人所属公司(初始化时用)
        private string businesstrID = string.Empty;
        private List<T_OA_AREACITY> areacitys;
        public string UserState = "Audit";
        private string state = string.Empty;
        private string bxbz = string.Empty;//报销标准
        private string UsableMoney = string.Empty;//用来存储可用额度
        public bool needsubmit = false;//提交审核
        private bool isSubmit = false;//是提交的话不弹出保存成功提示
        public bool clickSubmit = false;//单击了提交按钮
        private bool BtnNewButton = false;//单击新建按钮
        private bool SaveBtn = false;//保存数据
        private bool InitFB = false;//初始化预算数据
        private string StrPayInfo = "";//支付信息,用了传递给手机源数据

        private bool canSubmit = false;//能否提交审核

        public EntityBrowser ParentEntityBrowser { get; set; }//关闭窗口用

        public T_OA_TRAVELREIMBURSEMENT TravelReimbursement
        {
            get { return travelReimbursement; }
            set
            {
                this.DataContext = value;
                travelReimbursement = value;
            }
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
            this.actions = action;
            this.travelReimbursementID = travelReimbursementID;
            this.businesstrID = StrBusinesstrID;
            InitEvent();
            InitData();

            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "TravelReimbursement";
            //ctrFile.InitBtn(Visibility.Visible, Visibility.Collapsed);
            //ctrFile.Event_AllFilesFinished += new EventHandler<SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs>(ctrFile_Event_AllFilesFinished);

            if (actions != FormTypes.New && actions != FormTypes.Edit && actions != FormTypes.Resubmit)
            {
                ShowAudits.Visibility = Visibility.Collapsed;
                tbShowAudits.Visibility = Visibility.Visible;
                
            }
            else
            {
                
                ShowAudits.Visibility = Visibility.Visible;
                tbShowAudits.Visibility = Visibility.Collapsed;
            }
            this.Loaded += new RoutedEventHandler(TravelReimbursementControl_Loaded);
        }
        #endregion

        #region 上传附件
        private void UploadFiles()
        {
            //System.Windows.Controls.OpenFileDialog openFileWindow = new OpenFileDialog();
            //openFileWindow.Multiselect = true;
            //if (openFileWindow.ShowDialog() == true)
            //    foreach (FileInfo file in openFileWindow.Files)
            //        ctrFile.InitFiles(file.Name, file.OpenRead());
        }
        //void ctrFile_Event_AllFilesFinished(object sender, SMT.SaaS.FrameworkUI.FileUpload.FileCountEventArgs e)
        //{
        //    //RefreshUI(RefreshedTypes.HideProgressBar);
        //}
        #endregion

        #region InitData
        private void InitData()
        {
            if (actions == FormTypes.New)
            {
                TravelReimbursement = new T_OA_TRAVELREIMBURSEMENT();
                TravelReimbursement.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                ReimbursementTime.Text = System.DateTime.Now.ToShortDateString();
            }
        }
        #endregion

        #region 查看时屏蔽控件(不需要调整时间时查看&审核时调用)
        private void BrowseShieldedControl()
        {
            txtTELL.IsReadOnly = true;
            fbChkBox.IsEnabled = false;
            fbCtr.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            textStandards.IsReadOnly = true;
            FormToolBar1.Visibility = Visibility.Collapsed;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }
        #endregion

        #region 审核时屏蔽控件(需要调整出差时间时调用)
        private void AuditShieldedControl()
        {
            txtTELL.IsReadOnly = true;
            fbChkBox.IsEnabled = false;
            fbCtr.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            textStandards.IsReadOnly = true;
            FormToolBar1.Visibility = Visibility.Collapsed;
            //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }
        #endregion

        #region 初始化
        private void InitEvent()
        {
            OaPersonOfficeClient = new SmtOAPersonOfficeClient();
            HrPersonnelclient = new PersonnelServiceClient();
            areacitys = new List<T_OA_AREACITY>();
            OaInterfaceClient = new OaInterfaceClient();
            HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetAllEmployeePostBriefByEmployeeIDCompleted);
            HrPersonnelclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetEmployeePostBriefByEmployeeIDCompleted);
            OaPersonOfficeClient.UpdateTravelReimbursementCompleted += new EventHandler<UpdateTravelReimbursementCompletedEventArgs>(TrC_UpdateTravelReimbursementCompleted);
            OaPersonOfficeClient.GetTravelReimbursementByIdCompleted += new EventHandler<GetTravelReimbursementByIdCompletedEventArgs>(TrC_GetTravelReimbursementByIdCompleted);
            OaPersonOfficeClient.GetTravelReimbursementDetailCompleted += new EventHandler<GetTravelReimbursementDetailCompletedEventArgs>(TrC_GetTravelReimbursementDetailCompleted);
            OaPersonOfficeClient.GetTravelSolutionByCompanyIDCompleted += new EventHandler<GetTravelSolutionByCompanyIDCompletedEventArgs>(TrC_GetTravelSolutionByCompanyIDCompleted);
            OaPersonOfficeClient.GetTravleAreaAllowanceByPostValueCompleted += new EventHandler<GetTravleAreaAllowanceByPostValueCompletedEventArgs>(TrC_GetTravleAreaAllowanceByPostValueCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);//保存费用
            fbCtr.InitDataComplete += new EventHandler<FrameworkUI.FBControls.ChargeApplyControl.InitDataCompletedArgs>(fbCtr_InitDataComplete);
        }

        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            isSubmit = true;
            needsubmit = true;
            clickSubmit = true;
            
            Save();
        }

        void TravelReimbursementControl_Loaded(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);

            GetVechileLevelInfos();
            if (isloaded == true) return;//如果已经加载过，再次切换时就不再加载
            if (actions == FormTypes.Browse || actions == FormTypes.Audit)
            {
                //ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
                Utility.InitFileLoad("TravelReimbursement", travelReimbursementID, actions, uploadFile);
                BrowseShieldedControl();
            }
            if (actions != FormTypes.New)
            {
                if (!string.IsNullOrEmpty(travelReimbursementID))
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    OaPersonOfficeClient.GetTravelReimbursementByIdAsync(travelReimbursementID);
                    Utility.InitFileLoad("TravelReimbursement", travelReimbursementID, actions, uploadFile);
                }
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
        }

        #region 新建
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsFinishedCitys())
            {
                return;
            }

            BtnNewButton = true;
            int i = 0;
            T_OA_REIMBURSEMENTDETAIL buip = new T_OA_REIMBURSEMENTDETAIL();
            buip.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();

            if (actions != FormTypes.New)
            {
                if (TrList.Count() > 0)
                {
                    //foreach (T_OA_REIMBURSEMENTDETAIL tailList in TrList)
                    //{
                    //    buip.STARTDATE = tailList.ENDDATE;
                    //    if (cityscode.Count() == 0)
                    //    {
                    //        return;
                    //    }
                    //    buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityscode[i].Replace(",", ""));
                        
                    //    i++;
                    //}
                    //将原有记录的最后一条记录的目的城市作为出发城市。
                    //buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY);
                    if (TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>() != null)
                    {
                        buip.DEPCITY = TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().DESTCITY;
                        buip.STARTDATE = TrList.LastOrDefault<T_OA_REIMBURSEMENTDETAIL>().ENDDATE;
                        
                    }
                }
                buip.ENDDATE = DateTime.Now;
                TrList.Add(buip);
                DaGrs.ItemsSource = TrList;

                foreach (Object obje in DaGrs.ItemsSource)
                {
                    if (DaGrs.Columns[1].GetCellContent(obje) != null)
                    {
                        SearchCity myCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;

                        if (myCity != null)
                        {
                            myCity.IsEnabled = false;
                            ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region 检查是否选择了目标城市否则不给添加
        private bool CheckIsFinishedCitys()
        {
            bool IsResult = true;
            string StrStartDt = "";
            string EndDt = "";
            string StrStartTime = "";
            string StrEndTime = "";
            foreach (object obje in DaGrs.ItemsSource)
            {
                TrDetail = new T_OA_REIMBURSEMENTDETAIL();
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;

                TrDetail.T_OA_TRAVELREIMBURSEMENT = TravelReimbursement;
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;

                TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                EndDt = EndDate.Value.Value.ToString("d");//结束日期
                StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                if (string.IsNullOrEmpty(StrStartDt) || string.IsNullOrEmpty(StrStartTime))//开始日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "开始时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(EndDt) || string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "结束时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }
                DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);
                if (DtStart >= DtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "开始时间不能大于等于结束时间", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }


                if (ToolType.SelectedIndex <= 0)//交通工具类型
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }
            }
            return IsResult;
        }
        #endregion


        void TrC_GetTravleAreaAllowanceByPostValueCompleted(object sender, GetTravleAreaAllowanceByPostValueCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    if (e.Result != null)
                    {
                        areaallowance = e.Result.ToList();
                        areacitys = e.citys.ToList();
                    }
                    if (e.Result.Count() == 0)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司的出差方案没有对应的出差补贴", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                    if (actions != FormTypes.New)
                    {
                        if (needsubmit == false)
                        {
                            if (TravelReimbursement != null)
                            {
                                RefreshUI(RefreshedTypes.ShowProgressBar);//获取明细时加载进度
                                OaPersonOfficeClient.GetTravelReimbursementDetailAsync(TravelReimbursement.TRAVELREIMBURSEMENTID);
                            }
                            else
                            {
                                RefreshUI(RefreshedTypes.HideProgressBar);
                            }
                        }
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 根据城市值  获取相应的出差补贴
        /// </summary>
        /// <param name="CityValue"></param>
        private T_OA_AREAALLOWANCE GetAllowanceByCityValue(string CityValue)
        {
            var q = from ent in areaallowance
                    join ac in areacitys on ent.T_OA_AREADIFFERENCE.AREADIFFERENCEID equals ac.T_OA_AREADIFFERENCE.AREADIFFERENCEID
                    where ac.CITY == CityValue && ent.T_OA_TRAVELSOLUTIONS.TRAVELSOLUTIONSID == travelsolutions.TRAVELSOLUTIONSID
                    select ent;

            if (q.Count() > 0)
            {
                return q.FirstOrDefault();
            }
            return null;
        }

        void TrC_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)//判断能否乘坐哪种类型的交通工具及级别
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
                if (e.Result != null)
                {
                    
                    travelsolutions = e.Result;//出差方案
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "您公司没有关联出差方案，请关联一套出差方案以便报销", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null )
                {
                    if (e.StandardObj.Count() > 0)
                    {
                        takethestandardtransport = e.StandardObj.ToList();//乘坐交通工具设置
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差方案中没有关联对应的交通工具设置", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差方案中没有关联对应的交通工具设置", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                OaPersonOfficeClient.GetTravleAreaAllowanceByPostValueAsync(EmployeePostLevel, travelsolutions.TRAVELSOLUTIONSID, null);
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #region 查询报销明细
        void TrC_GetTravelReimbursementDetailCompleted(object sender, GetTravelReimbursementDetailCompletedEventArgs e)//查询报销明细
        {
            isloaded = true;
            if (clickSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            try
            {
                if (e.Result != null)
                {
                    BindDataGrid(e.Result);
                }
                else
                {
                    BindDataGrid(null);
                }
            }
            catch (Exception ex)
            {
                isloaded = false;
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 根据报销ID查询数据
        void TrC_GetTravelReimbursementByIdCompleted(object sender, GetTravelReimbursementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.Result == null)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return;
                    }
                    TravelReimbursement = e.Result;
                    if (TravelReimbursement.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || TravelReimbursement.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.All);
                    }
                    //ljx  2011-8-29  
                    if (actions == FormTypes.Edit)
                    {
                        if (TravelReimbursement.CHECKSTATE == (Convert.ToInt32(CheckStates.Approving)).ToString() || TravelReimbursement.CHECKSTATE == (Convert.ToInt32(CheckStates.Approved)).ToString() || TravelReimbursement.CHECKSTATE == (Convert.ToInt32(CheckStates.UnApproved)).ToString())
                        {
                            actions = FormTypes.Audit;
                            ShowAudits.Visibility = Visibility.Collapsed;
                            tbShowAudits.Visibility = Visibility.Visible;
                            Utility.InitFileLoad("TravelRequest", TravelReimbursement.TRAVELREIMBURSEMENTID, actions, uploadFile);
                        }
                    }
                    if (actions == FormTypes.Resubmit)//重新提交
                    {
                        TravelReimbursement.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }

                    txtPeopleTravel.Text = TravelReimbursement.CLAIMSWERENAME;//报销人
                    if (!string.IsNullOrEmpty(TravelReimbursement.TEL))
                    {
                        txtTELL.Text = TravelReimbursement.TEL;//联系电话
                    }
                    ReimbursementTime.Text = DateTime.Now.ToShortDateString();//报销时间
                    txtFee.Text = TravelReimbursement.REIMBURSEMENTOFCOSTS.ToString();//本次差旅总费用
                    txtFees.Text = TravelReimbursement.THETOTALCOST.ToString();//差旅合计

                    if (!string.IsNullOrEmpty(TravelReimbursement.NOBUDGETCLAIMS))//报销单号
                    {
                        txtNoClaims.Text = string.Empty;
                        txtNoClaims.Text = TravelReimbursement.NOBUDGETCLAIMS;
                    }
                    if (!string.IsNullOrEmpty(TravelReimbursement.REMARKS))
                    {
                        txtRemark.Text = TravelReimbursement.REMARKS;//备注
                    }
                    depName = Utility.GetDepartmentName(TravelReimbursement.OWNERDEPARTMENTID);//所属部门ID

                    //若是可保存的操作 则查找可用岗位，没有则是已离职 
                    //if (actions == FormTypes.Edit || actions == FormTypes.New || actions == FormTypes.Resubmit)
                    //    HrPersonnelclient.GetEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
                    //else
                    //    HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
                    if (InitFB == false)
                    {
                        InitFBControl();
                    }
                    OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement.OWNERCOMPANYID, null, null);
                
                    //HrPersonnelclient.GetEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
                    postName = TravelReimbursement.OWNERPOSTNAME;
                    depName = TravelReimbursement.OWNERDEPARTMENTNAME;
                    companyName = TravelReimbursement.OWNERCOMPANYNAME;
                    EmployeePostLevel = TravelReimbursement.POSTLEVEL;
                    string StrName = TravelReimbursement.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
                    txtPeopleTravel.Text = StrName;
                    ToolTipService.SetToolTip(txtPeopleTravel, StrName);
                    EmployeeName = TravelReimbursement.OWNERNAME;//出差人

                    if (actions != FormTypes.New || actions != FormTypes.Edit)
                    {
                        if (TravelReimbursement.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    else if (actions != FormTypes.Resubmit)
                    {
                        if (TravelReimbursement.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                            TravelReimbursement.CHECKSTATE == ((int)CheckStates.Approved).ToString() ||
                            TravelReimbursement.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    if (actions == FormTypes.New || actions == FormTypes.Edit)
                    {
                        //我的单据中用到(判断出差报告如果在未提交状态,FormType状态改为可编辑)
                        if (TravelReimbursement.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            //将Form状态改为编辑
                            actions = FormTypes.Edit;
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            //重新启用Form中的控件
                            txtTELL.IsReadOnly = false;
                            fbCtr.IsEnabled = true;
                            txtRemark.IsReadOnly = false;
                            textStandards.IsReadOnly = false;
                            //ctrFile.InitBtn(Visibility.Visible, Visibility.Collapsed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #endregion

        #region DataGrid 数据加载
        private void BindDataGrid(ObservableCollection<T_OA_REIMBURSEMENTDETAIL> obj)//加载出差报销子表
        {
            TrList = obj;
            foreach (T_OA_REIMBURSEMENTDETAIL detail in TrList)
            {
                citycode.Add(detail.DEPCITY);
                cityscode.Add(detail.DESTCITY);
            }
            if (actions != FormTypes.New && actions != FormTypes.Edit && actions != FormTypes.Resubmit)
            {
                DaGrss.ItemsSource = TrList;
            }
            else
            {
                DaGrs.ItemsSource = TrList;
            }
        }


        /// <summary>
        /// 操作子表数据
        /// </summary>
        private void NewDetail()
        {
            ObservableCollection<T_OA_REIMBURSEMENTDETAIL> ListDetail = new ObservableCollection<T_OA_REIMBURSEMENTDETAIL>();
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string EndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            int i = 0;
            if (DaGrs.ItemsSource != null)
            {
                foreach (Object obje in DaGrs.ItemsSource)
                {
                    TrDetail = new T_OA_REIMBURSEMENTDETAIL();
                    TrDetail.REIMBURSEMENTDETAILID = (obje as T_OA_REIMBURSEMENTDETAIL).REIMBURSEMENTDETAILID;
                    TrDetail.T_OA_TRAVELREIMBURSEMENT = travelReimbursement;

                    DateTimePicker StartDate = ((DateTimePicker)((StackPanel)DaGrs.Columns[0].GetCellContent(obje)).Children.FirstOrDefault()) as DateTimePicker;
                    DateTimePicker EndDate = ((DateTimePicker)((StackPanel)DaGrs.Columns[2].GetCellContent(obje)).Children.FirstOrDefault()) as DateTimePicker;
                    TextBox datys = ((TextBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TextBox Newdatys = ((TextBox)((StackPanel)DaGrs.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                    TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                    TextBox txtToolubsidies = ((TextBox)((StackPanel)DaGrs.Columns[8].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TextBox txtASubsidies = ((TextBox)((StackPanel)DaGrs.Columns[9].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TextBox txtTFSubsidies = ((TextBox)((StackPanel)DaGrs.Columns[10].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TextBox txtMealSubsidies = ((TextBox)((StackPanel)DaGrs.Columns[11].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    TextBox txtOthercosts = ((TextBox)((StackPanel)DaGrs.Columns[12].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                    CheckBox IsCheck = ((CheckBox)((StackPanel)DaGrs.Columns[13].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;
                    CheckBox IsCheckMeet = ((CheckBox)((StackPanel)DaGrs.Columns[14].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;
                    CheckBox IsCheckCar = ((CheckBox)((StackPanel)DaGrs.Columns[15].GetCellContent(obje)).Children.FirstOrDefault()) as CheckBox;

                    StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                    EndDt = EndDate.Value.Value.ToString("d");//结束日期
                    StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                    StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                    DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime); ;
                    DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);

                    TrDetail.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                    TrDetail.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                    TrDetail.CREATEDATE = DateTime.Now;
                    TrDetail.UPDATEDATE = DateTime.Now;
                    if (DtStart != null)//开始时间
                    {
                        TrDetail.STARTDATE = DtStart;
                    }
                    if (DtEnd != null)//结束时间
                    {
                        TrDetail.ENDDATE = DtEnd;
                    }
                    if (datys != null)//出差天数
                    {
                        TrDetail.BUSINESSDAYS = datys.Text;
                    }
                    if (Newdatys != null)//出差天数
                    {
                        TrDetail.THENUMBEROFNIGHTS = Newdatys.Text;
                    }
                    if (citycode.Count() > 0)
                    {
                        TrDetail.DEPCITY = citycode[i].Replace(",", "");//出发城市
                    }
                    if (cityscode.Count() > 0)
                    {
                        TrDetail.DESTCITY = cityscode[i].Replace(",", "");//目标城市
                    }
                    if (ToolType != null)//乘坐交通工具类型
                    {
                        T_SYS_DICTIONARY ComVechileObj = ToolType.SelectedItem as T_SYS_DICTIONARY;
                        TrDetail.TYPEOFTRAVELTOOLS = ComVechileObj.DICTIONARYVALUE.ToString();
                    }
                    if (ToolLevel != null)//乘坐交通工具级别
                    {
                        T_SYS_DICTIONARY ComLevelObj = ToolLevel.SelectedItem as T_SYS_DICTIONARY;
                        TrDetail.TAKETHETOOLLEVEL = ComLevelObj.DICTIONARYVALUE.ToString();
                    }
                    if (txtToolubsidies != null)//乘坐交通工具费用
                    {
                        if (!string.IsNullOrEmpty(txtToolubsidies.Text))
                            TrDetail.TRANSPORTCOSTS = Convert.ToDecimal(txtToolubsidies.Text);
                    }
                    if (txtASubsidies != null)//住宿标准费用
                    {
                        if (!string.IsNullOrEmpty(txtASubsidies.Text))
                            TrDetail.ACCOMMODATION = Convert.ToDecimal(txtASubsidies.Text);
                    }
                    if (txtTFSubsidies != null)//交通费用补贴
                    {
                        if (!string.IsNullOrEmpty(txtTFSubsidies.Text))
                            TrDetail.TRANSPORTATIONSUBSIDIES = Convert.ToDecimal(txtTFSubsidies.Text);
                    }
                    if (txtMealSubsidies != null)//餐费补贴
                    {
                        if (!string.IsNullOrEmpty(txtMealSubsidies.Text))
                            TrDetail.MEALSUBSIDIES = Convert.ToDecimal(txtMealSubsidies.Text);
                    }
                    if (txtOthercosts != null)//其他费用
                    {
                        if (!string.IsNullOrEmpty(txtOthercosts.Text))
                            TrDetail.OTHERCOSTS = Convert.ToDecimal(txtOthercosts.Text);
                    }
                    if (IsCheck != null)//是否是私事
                    {
                        TrDetail.PRIVATEAFFAIR = (bool)IsCheck.IsChecked ? "1" : "0";
                    }
                    if (IsCheckMeet != null)//是否是开会
                    {
                        TrDetail.GOOUTTOMEET = (bool)IsCheckMeet.IsChecked ? "1" : "0";
                    }
                    if (IsCheckCar != null)//公司派车
                    {
                        TrDetail.COMPANYCAR = (bool)IsCheckCar.IsChecked ? "1" : "0";
                    }
                    ListDetail.Add(TrDetail);
                    i++;
                }
                TrList = ListDetail;
            }
        }
        #endregion

        #region 获取当前用户信息已废除

        void client_GetAllEmployeePostBriefByEmployeeIDCompleted(object sender, GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            string StrName = "";
            if (e.Result != null)
            {
                employeepost = e.Result;
                if (TravelReimbursement != null)
                {
                    if (TravelReimbursement.OWNERPOSTID != null && TravelReimbursement.OWNERCOMPANYID != null && TravelReimbursement.OWNERDEPARTMENTID != null)
                    {
                        if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault() != null)
                        {
                            EmployeePostLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
                        }
                        else //存在岗位异动的情况下
                        {
                            var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
                            EmployeePostLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
                        }

                        //2013/3/27 alter by ken 修改出差加载员工岗位信息方式
                        postName = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().PostName;
                        depName = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
                        companyName = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CompanyName;
                        //postName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                        //depName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        //companyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CNAME;
                        StrName = TravelReimbursement.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
                        txtPeopleTravel.Text = StrName;
                        ToolTipService.SetToolTip(txtPeopleTravel, StrName);
                        EmployeeName = TravelReimbursement.OWNERNAME;//出差人
                    }
                    if (InitFB == false)
                    {
                        InitFBControl();
                    }
                    OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement.OWNERCOMPANYID, null, null);
                }
            }
        }

        void client_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            string StrName = "";
            if (e.Result != null)
            {
                employeepost = e.Result;
                if (TravelReimbursement != null)
                {
                    if (TravelReimbursement.OWNERPOSTID != null && TravelReimbursement.OWNERCOMPANYID != null && TravelReimbursement.OWNERDEPARTMENTID != null)
                    {
                        if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault() != null)
                        {
                            EmployeePostLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();
                        }
                        else //存在岗位异动的情况下
                        {
                            var ent = employeepost.EMPLOYEEPOSTS.Where(s => s.ISAGENCY == "0").FirstOrDefault();
                            EmployeePostLevel = ent != null ? ent.POSTLEVEL.ToString() : "0 ";
                        }

                        //2013/3/27 alter by ken 修改加载员工岗位信息方式
                        postName = employeepost.EMPLOYEEPOSTS.Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().PostName;
                        depName = employeepost.EMPLOYEEPOSTS.Where(c => c.DepartmentID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().DepartmentName;
                        companyName = employeepost.EMPLOYEEPOSTS.Where(c => c.CompanyID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CompanyName;
                        //postName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == TravelReimbursement.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                        //depName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == TravelReimbursement.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                        //companyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == TravelReimbursement.OWNERCOMPANYID).FirstOrDefault().CNAME;
                        StrName = TravelReimbursement.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
                        txtPeopleTravel.Text = StrName;
                        ToolTipService.SetToolTip(txtPeopleTravel, StrName);
                        EmployeeName = TravelReimbursement.OWNERNAME;//出差人
                    }
                    if (InitFB == false)
                    {
                        InitFBControl();
                    }
                    OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement.OWNERCOMPANYID, null, null);
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("对不起，该员工已离职，不能进行该操作"));
                HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
            }
        }
        #endregion

        #region 费用保存Completed
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), e.Message[0], Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }
            else
            {
                switch (actions)
                {
                    case FormTypes.Edit://修改
                        if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            if (fbCtr.Order.PAYMENTINFO != null)
                            {
                                txtPAYMENTINFO.Text = fbCtr.Order.PAYMENTINFO;//支付信息
                                StrPayInfo = txtPAYMENTINFO.Text;
                            }
                            travelReimbursement.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                            }
                        }
                        if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.Approving))
                        {
                            RefreshUI(RefreshedTypes.AuditInfo);
                            IsAudit = false;
                        }
                        break;
                    case FormTypes.Resubmit://重新提交
                        if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.UnApproved))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            travelReimbursement.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                            }
                        }
                        else if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.UnSubmit))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            if (fbCtr.Order.INNERORDERCODE != null)
                            {
                                txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                                travelReimbursement.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号
                            }
                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                            }
                        }
                        break;
                    case FormTypes.Audit:
                        if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.Approved) || TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.UnApproved))
                        {
                            if (e.Message != null && e.Message.Count() > 0)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                            txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                            travelReimbursement.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                            if (needsubmit == true)
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Submit");
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                            }
                        }//审核通过直接修改表单状态
                        else
                        {
                            if (IsAudit) //审核中
                            {
                                RefreshUI(RefreshedTypes.AuditInfo);
                                IsAudit = false;
                            }
                            else if (Resubmit)//重新提交
                            {
                                if (e.Message != null && e.Message.Count() > 0)
                                {
                                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("无报销单号,请重试！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                    return;
                                }
                                txtNoClaims.Text = fbCtr.Order.INNERORDERCODE;//保存后将单号显示出来
                                travelReimbursement.NOBUDGETCLAIMS = fbCtr.Order.INNERORDERCODE;//预算返回的报销单号

                                if (needsubmit == true)
                                {
                                    OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Submit");
                                }
                                else
                                {
                                    OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                                }
                                RefreshUI(RefreshedTypes.AuditInfo);
                                Resubmit = false;
                            }
                            else
                            {
                                OaPersonOfficeClient.UpdateTravelReimbursementAsync(TravelReimbursement, TrList, actions.ToString(), "Edit");
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region 修改Completed
        void TrC_UpdateTravelReimbursementCompleted(object sender, UpdateTravelReimbursementCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    if (e.Result != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Result), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        return;
                    }
                    if (e.UserState.ToString() == "Edit" && !isSubmit)
                    {
                        isSubmit = false;
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "TRAVELREIMBURSEMENTPAGE"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                            ParentEntityBrowser.ParentWindow.Close();
                        }
                    }
                    if (e.UserState.ToString() == "Audit")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    }
                    canSubmit = true;
                    //if (e.UserState.ToString() == "Submit")
                    //{
                    //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    //}

                    RefreshUI(RefreshedTypes.AuditInfo);
                    if (TravelReimbursement.REIMBURSEMENTOFCOSTS > 0 || fbCtr.Order.TOTALMONEY > 0)
                    {
                        if (needsubmit == true)
                        {

                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.ManualSubmit();
                            HideButtons();
                            OaPersonOfficeClient.GetTravelReimbursementByIdAsync(travelReimbursementID);
                        }
                        else
                        {
                            needsubmit = false;
                            RefreshUI(RefreshedTypes.HideProgressBar);
                        }
                    }
                    else
                    {
                        needsubmit = false;
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        if (clickSubmit == true)
                        {       
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            RefreshUI(RefreshedTypes.All);
                            //clickSubmit = false;
                        }
                    }

                    //RefreshUI(RefreshedTypes.All);
                }
                //TrC.GetTravelReimbursementByIdAsync(travelReimbursementID);
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        /// <summary>
        /// 点提交后隐藏按钮
        /// </summary>
        public void HideButtons()
        {
            needsubmit = false;
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
            RefreshUI(RefreshedTypes.AuditInfo);
        }
        #endregion

        #region 保存函数
        private void Save()
        {
            SaveBtn = true;
            try
            {
                if (Check())
                {
                    textStandards.Text = string.Empty;//清空报销标准说明
                    //字段赋值
                    SetTraveReimbursementValue();
                }
                else
                {
                    needsubmit = false;
                    isSubmit = false;
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        /// <summary>
        /// 字段赋值
        /// </summary>
        private void SetTraveReimbursementValue()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);

            //计算出差时间
            TravelTime();

            //计算补贴
            TravelAllowance();

            //添加子表数据
            NewDetail();

            //CountMoney();

            if (!string.IsNullOrEmpty(this.txtFees.Text) && this.txtFees.Text.Trim() != "0")
            {
                if (fbCtr.TravelSubject != null)
                {
                    fbCtr.TravelSubject.ApplyMoney = Convert.ToDecimal(this.txtFees.Text);//将本次出差总费用给预算
                }
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出差报销费用不可为零，请重新填写单据", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtPAYMENTINFO.Text))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "支付信息不能为空，请重新填写", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            else
            {
                fbCtr.Order.PAYMENTINFO = txtPAYMENTINFO.Text;//支付信息
                StrPayInfo = txtPAYMENTINFO.Text;
            }


            travelReimbursement.TEL = this.txtTELL.Text;//联系电话;
            travelReimbursement.CONTENT = this.txtReport.Text;//报告内容    
            travelReimbursement.THETOTALCOST = Convert.ToDecimal(txtFees.Text);//本次差旅总费用
            //travelReimbursement.REIMBURSEMENTOFCOSTS = fbCtr.Order.TOTALMONEY;//总费用;
            travelReimbursement.REIMBURSEMENTTIME = Convert.ToDateTime(ReimbursementTime.Text);
            travelReimbursement.REMARKS = this.txtRemark.Text;
            if (actions == FormTypes.New)
            {
                //travelReimbursement.TRAVELREIMBURSEMENTID = System.Guid.NewGuid().ToString();
                //travelReimbursement.T_OA_BUSINESSTRIP = businesstrip.Travelmanagement;
                //travelReimbursement.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                //travelReimbursement.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                //travelReimbursement.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                //travelReimbursement.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                //travelReimbursement.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                //travelReimbursement.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                //travelReimbursement.OWNERCOMPANYID = businesstrip.Travelmanagement.OWNERCOMPANYID;//所属公司ID
                //travelReimbursement.OWNERDEPARTMENTID = businesstrip.Travelmanagement.OWNERDEPARTMENTID;//所属部门ID
                //travelReimbursement.OWNERPOSTID = businesstrip.Travelmanagement.OWNERPOSTID;//所属岗位ID
                //travelReimbursement.OWNERID = businesstrip.Travelmanagement.OWNERID;//所属人ID
                //travelReimbursement.OWNERNAME = businesstrip.Travelmanagement.OWNERNAME;//报销人ID(出差人)
                //travelReimbursement.CLAIMSWERE = businesstrip.Travelmanagement.OWNERID;//报销人ID(出差人)
                //travelReimbursement.CLAIMSWERENAME = businesstrip.Travelmanagement.OWNERNAME;//报销人姓名
                //travelReimbursement.CREATEDATE = DateTime.Now;//创建时间
                //ctrFile.FormID = travelReimbursement.TRAVELREIMBURSEMENTID;
                //ctrFile.Save();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
            }
            if (travelReimbursement.REIMBURSEMENTOFCOSTS > 0)
            {
                fbCtr.Order.ORDERID = TravelReimbursement.TRAVELREIMBURSEMENTID;
                fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);//提交费用 
            }
            else if (travelReimbursement.REIMBURSEMENTOFCOSTS == 0)
            {
                if (actions != FormTypes.New)
                {
                    OaPersonOfficeClient.UpdateTravelReimbursementAsync(travelReimbursement, TrList, actions.ToString(), "Edit");
                }
            }

        }

        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(travelReimbursementID))
            {
                //ctrFile.Load_fileData(travelReimbursementID);
            }
            fbCtr.GetPayType.Visibility = Visibility.Visible;
        }
        #endregion

        #region IEntityEditor 成员

        public string GetTitle()
        {
            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "TRAVELREIMBURSEMENTPAGE");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "TRAVELREIMBURSEMENTPAGE");
            }
            else if (actions == FormTypes.Audit)
            {
                return Utility.GetResourceStr("AUDIT1", "TRAVELREIMBURSEMENTPAGE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "TRAVELREIMBURSEMENTPAGE");
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
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                //case "1":
                //    refreshType = RefreshedTypes.CloseAndReloadData;
                //    Save();
                //    break;
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
            if (actions != FormTypes.Browse && actions != FormTypes.Audit)
            {
                //ToolbarItem item = new ToolbarItem
                //{
                //    DisplayType = ToolbarItemDisplayTypes.Image,
                //    Key = "1",
                //    Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                //};
                //items.Add(item);

                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("SAVE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
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

        #region 验证
        private bool Check()
        {
            string StrStartDt = "";
            string EndDt = "";
            string StrStartTime = "";
            string StrEndTime = "";
            bool checkPrivate=false;
            bool checkMeet=false;
            foreach (object obje in DaGrs.ItemsSource)
            {
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                CheckBox chbPrivate=DaGrs.Columns[13].GetCellContent(obje).FindName("myChkBox") as CheckBox;
                CheckBox chbMeet=DaGrs.Columns[14].GetCellContent(obje).FindName("myChkBoxMeet") as CheckBox;

                if(chbPrivate.IsChecked==true) checkPrivate=true;
                if(chbMeet.IsChecked==true) checkMeet=true;

                if (StartDate.Value != null)
                    StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                if (EndDate.Value != null)
                    EndDt = EndDate.Value.Value.ToString("d");//结束日期
                if (StartDate.Value != null)
                    StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                if (EndDate.Value != null)
                    StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间

                if (string.IsNullOrEmpty(StrStartDt) || string.IsNullOrEmpty(StrStartTime))//开始日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                if (string.IsNullOrEmpty(EndDt) || string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "到达时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);
                if (DtStart >= DtEnd)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发时间不能大于等于到达时间", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            //add by luojie
            //当没有报销金额时弹出提醒
            decimal totalFee = Convert.ToDecimal(txtFee.Text);
            if ( totalFee<=0 && (!checkMeet &&!checkPrivate))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }

            List<string> checkCity = new List<string>();

            ObservableCollection<T_OA_REIMBURSEMENTDETAIL> entBusinessTripDetails = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
            int foreachCount = 0;
            foreach (Object obje in DaGrs.ItemsSource)//判断所选的出发城市是否与目标城市相同
            {
                foreachCount++;
                string cityCheck = string.Empty;

                DateTimePicker dpStartTime = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker dpEndTime = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                SearchCity myCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
                TravelDictionaryComboBox ComVechile = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[7].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                if (dpStartTime.Value != null)
                {
                    TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Hour);
                    if (tsStart == null)//开始时间不能为空
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                    if (dpStartTime.Value.Value.Date == null)//开始日期不能为空
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }

                    if (dpEndTime.Value != null)
                    {
                        TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Hour);
                        if (tsEnd == null)//结束时间不能为空
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                        if (dpEndTime.Value.Value.Date == null)//结束日期不能为空
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETITLE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                        if (dpStartTime.Value >= dpEndTime.Value)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORSTARTDATEGTENDDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return false;
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ENDDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "STARTDATETIME"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                T_OA_REIMBURSEMENTDETAIL entDetail = obje as T_OA_REIMBURSEMENTDETAIL;

                var queryData = from c in entBusinessTripDetails
                                where c.STARTDATE > dpStartTime.Value && c.ENDDATE > dpEndTime.Value && c.REIMBURSEMENTDETAILID != entDetail.REIMBURSEMENTDETAILID
                                orderby c.STARTDATE
                                select c;

                if (queryData.Count() > 0)
                {
                    if (queryData.FirstOrDefault().STARTDATE < entDetail.ENDDATE)
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANNOTBEREPEATEDTOADD", "KPIRECEIVEDATE"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }

                if (string.IsNullOrEmpty(myCity.TxtSelectedCity.Text))//判断出发城市
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
                else
                {
                    cityCheck = myCity.TxtSelectedCity.Text.Trim();
                }
                checkCity.Add(cityCheck);
                if (string.IsNullOrEmpty(myCitys.TxtSelectedCity.Text))//判断目标城市
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }

                //检测出发城市是否重复
                if (foreachCount > 1)
                {
                    if (cityCheck == checkCity[0])
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("THESAMECANNOTAPPEAR", "INITIALDEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
                if (myCity.TxtSelectedCity.Text != null)
                {
                    if (myCity.TxtSelectedCity.Text == myCitys.TxtSelectedCity.Text)//出发城市不能与目标城市相同
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTTHESAMEASTHETARGECITY", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }
                if (myCitys.TxtSelectedCity.Text != null)
                {
                    if (myCitys.TxtSelectedCity.Text == myCity.TxtSelectedCity.Text)//目标城市不能与出发城市相同
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("NOTTHESAMEASTHETARGECITY", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        return false;
                    }
                }

                //if (ComVechile.SelectedIndex <= 0)//交通工具类型
                //{
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "TYPEOFTRAVELTOOLS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return false;
                //}
            }

            if (string.IsNullOrEmpty(this.txtReport.Text.Trim()))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("STRINGNOTNULL", "REPORT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                this.txtRemark.Focus();
                return false;
            }

            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            return true;
        }
        #endregion

        #region InitFBControl
        private void InitFBControl()
        {
            InitFB = true;
            fbCtr.submitFBFormTypes = actions;//将FormType赋给FB
            //fbCtr.SetRemarkVisiblity(Visibility.Collapsed);//隐藏预算控件中的备注
            fbCtr.SetApplyTypeVisiblity(Visibility.Collapsed);//隐藏支付类型
            fbCtr.TravelSubject = new FrameworkUI.FBControls.TravelSubject();
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.ChargeApply;//费用报销

            if (actions == FormTypes.New)
            {
                fbCtr.Order.ORDERID = "";
                fbCtr.strExtOrderModelCode = "CCBX";
            }
            else
            {
                fbCtr.Order.ORDERID = TravelReimbursement.TRAVELREIMBURSEMENTID;//费用对象
                fbCtr.strExtOrderModelCode = "CCBX";
            }
            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            if (businesstrip.Travelmanagement != null)//新增获取出差报告的时候用的是报告人的信息
            {
                fbCtr.Order.OWNERCOMPANYID = businesstrip.Travelmanagement.OWNERCOMPANYID;//出差人所属公司ID
                fbCtr.Order.OWNERCOMPANYNAME = companyName;//出差人所属公司名称
                fbCtr.Order.OWNERDEPARTMENTID = businesstrip.Travelmanagement.OWNERDEPARTMENTID;//出差人所属部门ID
                fbCtr.Order.OWNERDEPARTMENTNAME = depName;//出差人所属部门名称
                fbCtr.Order.OWNERPOSTID = businesstrip.Travelmanagement.OWNERPOSTID;//出差人所属工岗位ID
                fbCtr.Order.OWNERPOSTNAME = postName;//出差人所属岗位名称
                fbCtr.Order.OWNERID = businesstrip.Travelmanagement.OWNERID;//出差人ID
                fbCtr.Order.OWNERNAME = businesstrip.Travelmanagement.OWNERNAME;//出差人姓名
            }
            else//修改、查看、审核的时候获取的是报销人的信息
            {
                fbCtr.Order.OWNERCOMPANYID = TravelReimbursement.OWNERCOMPANYID;//出差人所属公司ID
                fbCtr.Order.OWNERCOMPANYNAME = companyName;//出差人所属公司名称
                fbCtr.Order.OWNERDEPARTMENTID = TravelReimbursement.OWNERDEPARTMENTID;//出差人所属部门ID
                fbCtr.Order.OWNERDEPARTMENTNAME = depName;//出差人所属部门名称
                fbCtr.Order.OWNERPOSTID = TravelReimbursement.OWNERPOSTID;//出差人所属工岗位ID
                fbCtr.Order.OWNERPOSTNAME = postName;//出差人所属岗位名称
                fbCtr.Order.OWNERID = TravelReimbursement.OWNERID;//出差人ID
                fbCtr.Order.OWNERNAME = TravelReimbursement.OWNERNAME;//出差人姓名
            }
            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            if (actions == FormTypes.Audit || actions == FormTypes.Browse)
            {
                fbCtr.strExtOrderModelCode = "CCBX";
                fbCtr.InitData(false);
            }
            else
            {
                if (actions == FormTypes.New && businesstrip.Travelmanagement != null)
                {
                    fbCtr.InitData();
                }
                else if (actions == FormTypes.Edit || actions == FormTypes.Resubmit && TravelReimbursement != null)
                {
                    fbCtr.InitData();
                }
            }
        }

        void fbCtr_InitDataComplete(object sender, FrameworkUI.FBControls.ChargeApplyControl.InitDataCompletedArgs e)
        {
            if (e.Message != null && e.Message.Count() > 0)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Message[0]);
                DaGrs.IsEnabled = false;
                fbCtr.IsEnabled = false;
                if (needsubmit == false)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
            }
            Binding bding = new Binding();
            bding.Path = new PropertyPath("TOTALMONEY");
            if (fbCtr.ListDetail.Count() > 0)
            {
                this.txtFee.SetBinding(TextBox.TextProperty, bding);//报销费用总额
                this.txtFee.DataContext = fbCtr.Order;
            }
            this.txtAvailableCredit.Text = fbCtr.TravelSubject.UsableMoney.ToString();//当前用户可用额度
            if (fbCtr.Order.PAYMENTINFO != null && !string.IsNullOrEmpty(fbCtr.Order.PAYMENTINFO))
            {
                this.txtPAYMENTINFO.Text = fbCtr.Order.PAYMENTINFO;//支付信息
                StrPayInfo = this.txtPAYMENTINFO.Text;
            }
            UsableMoney = txtAvailableCredit.Text;
            if (actions == FormTypes.Browse || actions == FormTypes.Audit)
            {
                fbCtr.Visibility = Visibility.Collapsed;
                lblFees.Visibility = Visibility.Collapsed;
                fbChkBox.Visibility = Visibility.Collapsed;

                fbCtr.strExtOrderModelCode = "CCBX";
                //费用报销
                if (fbCtr.ListDetail.Count() > 0 )
                {
                    fbCtr.Visibility = Visibility.Visible;
                    scvFB.Visibility = Visibility.Visible;
                    fbChkBox.IsChecked = true;
                }
                //冲借款
                if (fbCtr.ListBorrowDetail.Count() > 0)
                {
                    var q = (from ent in fbCtr.ListBorrowDetail
                             select ent.REPAYMONEY).Sum();
                    if (q > 0)
                    {
                        fbCtr.Visibility = Visibility.Visible;
                        scvFB.Visibility = Visibility.Visible;
                        fbChkBox.IsChecked = true;
                    }
                }
            }
            if (actions == FormTypes.Edit)
            {
                scvFB.Visibility = Visibility.Visible;
                fbChkBox.IsChecked = true;
            }
        }
        #endregion

        #region 字典转换
        /// <summary>
        /// 审核状态转换
        /// </summary>
        /// <param name="checkStateValue"></param>
        /// <returns></returns>
        private string GetCheckState(string checkStateValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CHECKSTATE" && a.DICTIONARYVALUE == Convert.ToDecimal(checkStateValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 交通工具类型值转换
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetTypeName(string typeValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILESTANDARD" && a.DICTIONARYVALUE == Convert.ToDecimal(typeValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 获取级别对应的类型ID
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetTypeId(string typeValue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILESTANDARD" && a.DICTIONARYVALUE == Convert.ToDecimal(typeValue)
                           select new
                           {
                               DICTIONARYGUID = a.DICTIONARYID
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYGUID : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 交通工具级别值转换
        /// </summary>
        /// <param name="typevalue"></param>
        /// <returns></returns>
        private string GetLevelName(string levelValue, string typeId)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "VICHILELEVEL" && (a.T_SYS_DICTIONARY2 != null && a.T_SYS_DICTIONARY2.DICTIONARYID == typeId) && a.DICTIONARYVALUE == Convert.ToDecimal(levelValue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 城市值转换
        /// </summary>
        /// <param name="cityvalue"></param>
        /// <returns></returns>
        private string GetCityName(string cityvalue)
        {
            try
            {
                var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                           where a.DICTIONCATEGORY == "CITY" && a.DICTIONARYVALUE == Convert.ToDecimal(cityvalue)
                           select new
                           {
                               DICTIONARYNAME = a.DICTIONARYNAME,
                               DICTIONARYVALUE = a.DICTIONARYVALUE
                           };
                return ents.Count() > 0 ? ents.FirstOrDefault().DICTIONARYNAME : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region MobileXml
        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text, string keyValue)
        {
            string[] strlist = new string[5];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            strlist[4] = keyValue;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryChiledList(strlist);
            return ad;
        }

        private string GetXmlString(T_OA_TRAVELREIMBURSEMENT Info, string StrSource)
        {
            string goouttomeet = string.Empty;
            string privateaffair = string.Empty;
            string companycar = string.Empty;
            string isagent = string.Empty;
            string path = string.Empty;
            string chargetype = string.Empty;
            string ExtTotal = "";
            if (fbCtr.ListDetail.Count() > 0)
            {
                decimal totalMoney = this.fbCtr.ListDetail.Sum(item =>
            {
                return (item.Entity as T_FB_EXTENSIONORDERDETAIL).APPLIEDMONEY;
            });
                ExtTotal = totalMoney.ToString();
            }

            SMT.SaaS.MobileXml.MobileXml mx = new MobileXml.MobileXml();
            SMT.SaaS.MobileXml.AutoDictionary ad = new MobileXml.AutoDictionary();

            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CHECKSTATE", TravelReimbursement.CHECKSTATE, GetCheckState(TravelReimbursement.CHECKSTATE)));//审核状态
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "BUSINESSTRIPID", businesstrID, string.Empty));//出差申请ID
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "AVAILABLECREDIT", UsableMoney, string.Empty));//可用额度
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REIMBURSEMENTSTANDARDS", bxbz, string.Empty));//报销标准
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REIMBURSEMENTOFCOSTS", fbCtr.Order.TOTALMONEY.ToString(), string.Empty));//报销总计
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "POSTLEVEL", EmployeePostLevel, string.Empty));//出差人的岗位级别
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CONTENT", TravelReimbursement.CONTENT, TravelReimbursement.CONTENT));//报告内容
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "REMARKS", TravelReimbursement.REMARKS, string.Empty));//备注
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYMENTINFO", fbCtr.Order.PAYMENTINFO, string.Empty));//支付信息
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYTARGET", ExtTotal, string.Empty));//小计
            StrPayInfo = txtPAYMENTINFO.Text.ToString();
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "PAYMENTINFO", StrPayInfo, string.Empty));//支付信息
            AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "AttachMent", TravelReimbursement.TRAVELREIMBURSEMENTID, TravelReimbursement.TRAVELREIMBURSEMENTID));//附件

            if (!string.IsNullOrEmpty(TravelReimbursement.NOBUDGETCLAIMS))//报销单号
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "NOBUDGETCLAIMS", TravelReimbursement.NOBUDGETCLAIMS, string.Empty));//报销单号
            }
            if (TravelReimbursement.CLAIMSWERE != null && !string.IsNullOrEmpty(EmployeeName))//报销人
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "CLAIMSWERE", TravelReimbursement.CLAIMSWERE, EmployeeName + "-" + postName + "-" + depName + "-" + companyName));
            }
            if (TravelReimbursement.OWNERID != null && !string.IsNullOrEmpty(EmployeeName))//所属人
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement.OWNERID, EmployeeName));
            }
            if (TravelReimbursement.OWNERCOMPANYID != null && !string.IsNullOrEmpty(companyName))//所属公司
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERCOMPANYID", TravelReimbursement.OWNERCOMPANYID, companyName));
            }
            if (TravelReimbursement.OWNERDEPARTMENTID != null && !string.IsNullOrEmpty(depName))//所属部门
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERDEPARTMENTID", TravelReimbursement.OWNERDEPARTMENTID, depName));
            }
            if (TravelReimbursement.OWNERPOSTID != null && !string.IsNullOrEmpty(postName))//所属岗位
            {
                AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERPOSTID", TravelReimbursement.OWNERPOSTID, postName));
            }
            foreach (T_OA_REIMBURSEMENTDETAIL objDetail in TrList)//填充子表
            {
                if (objDetail.BUSINESSDAYS != null)//出差天数
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "BUSINESSDAYS", objDetail.BUSINESSDAYS, objDetail.BUSINESSDAYS, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.THENUMBEROFNIGHTS != null)//住宿天数
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "THENUMBEROFNIGHTS", objDetail.THENUMBEROFNIGHTS, objDetail.THENUMBEROFNIGHTS, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.DEPCITY != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "DEPCITY", objDetail.DEPCITY, SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(objDetail.DEPCITY), objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.DESTCITY != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "DESTCITY", objDetail.DESTCITY, SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(objDetail.DESTCITY), objDetail.REIMBURSEMENTDETAILID));
                }
                if (TravelReimbursement.CREATEUSERID != null)//创建人
                {
                    AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement.CREATEUSERID, Common.CurrentLoginUserInfo.EmployeeName, objDetail.REIMBURSEMENTDETAILID));
                }
                if (TravelReimbursement.UPDATEUSERID != null)//修改人
                {
                    AutoList.Add(basedata("T_OA_TRAVELREIMBURSEMENT", "OWNERID", TravelReimbursement.UPDATEUSERID, Common.CurrentLoginUserInfo.EmployeeName, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.OTHERCOSTS != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "OTHERCOSTS", objDetail.OTHERCOSTS.ToString(), string.Empty, objDetail.REIMBURSEMENTDETAILID));
                }
                else //如果没有其他费用就传空值给Xml
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "OTHERCOSTS", string.Empty, string.Empty, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.PRIVATEAFFAIR != null)
                {
                    if (objDetail.PRIVATEAFFAIR == "0")//是否私事
                    {
                        privateaffair = "否";
                    }
                    else
                    {
                        privateaffair = "是";
                    }
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "PRIVATEAFFAIR", objDetail.PRIVATEAFFAIR, privateaffair, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.GOOUTTOMEET != null)
                {
                    if (objDetail.GOOUTTOMEET == "0")//内部会议\培训
                    {
                        goouttomeet = "否";
                    }
                    else
                    {
                        goouttomeet = "是";
                    }
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "GOOUTTOMEET", objDetail.GOOUTTOMEET, goouttomeet, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.COMPANYCAR != null)
                {
                    if (objDetail.COMPANYCAR == "0")//是否是公司派车
                    {
                        companycar = "否";
                    }
                    else
                    {
                        companycar = "是";
                    }
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "COMPANYCAR", objDetail.COMPANYCAR, companycar, objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.TYPEOFTRAVELTOOLS != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "TYPEOFTRAVELTOOLS", objDetail.TYPEOFTRAVELTOOLS, GetTypeName(objDetail.TYPEOFTRAVELTOOLS), objDetail.REIMBURSEMENTDETAILID));
                }
                if (objDetail.TAKETHETOOLLEVEL != null)
                {
                    AutoList.Add(basedata("T_OA_REIMBURSEMENTDETAIL", "TAKETHETOOLLEVEL", objDetail.TAKETHETOOLLEVEL, GetLevelName(objDetail.TAKETHETOOLLEVEL, GetTypeId(objDetail.TYPEOFTRAVELTOOLS)), objDetail.REIMBURSEMENTDETAILID));
                }
            }
            ObservableCollection<Object> TrListObj = new ObservableCollection<Object>();
            foreach (var item in TrList)
            {
                TrListObj.Add(item);
            }

            if (fbCtr.ListDetail.Count > 0)//获取算控件中的数据
            {
                //SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONALORDER entext = fbCtr
                //fbCtr.Order.REMARK
                string StrType = "";
                if (fbCtr.Order.REMARK != null)
                {
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "FBREMARK", fbCtr.Order.REMARK, fbCtr.Order.REMARK, fbCtr.Order.EXTENSIONALORDERID));//科目报销备注
                }
                if (fbCtr.Order.APPLYTYPE == 1)
                {
                    StrType = "个人费用报销";
                }
                if (fbCtr.Order.APPLYTYPE == 2)
                {
                    StrType = "冲借款";
                }
                if (fbCtr.Order.REMARK != null)
                {
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "EXTENSIONTYPE", StrType, StrType, fbCtr.Order.EXTENSIONALORDERID));//科目报销备注
                }
                foreach (FBEntity item in fbCtr.ListDetail)//预算费用报销明细
                {
                    SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONORDERDETAIL entTemp = item.Entity as SMT.Saas.Tools.FBServiceWS.T_FB_EXTENSIONORDERDETAIL;

                    TrListObj.Add(entTemp);

                    if (entTemp.CHARGETYPE != null)
                    {
                        if (entTemp.CHARGETYPE.ToString() == "1")
                        {
                            chargetype = "个人预算费用";
                        }
                        else
                        {
                            chargetype = "部门预算费用";
                        }
                        AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "CHARGETYPE", entTemp.CHARGETYPE.ToString(), chargetype, entTemp.EXTENSIONORDERDETAILID));//费用类型
                    }
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "SUBJECTCODE", entTemp.T_FB_SUBJECT.SUBJECTCODE, entTemp.T_FB_SUBJECT.SUBJECTCODE, entTemp.EXTENSIONORDERDETAILID));//科目编号
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "SUBJECTNAME", entTemp.T_FB_SUBJECT.SUBJECTNAME, entTemp.T_FB_SUBJECT.SUBJECTNAME, entTemp.EXTENSIONORDERDETAILID));//科目名称
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "USABLEMONEY", entTemp.USABLEMONEY.ToString(), entTemp.USABLEMONEY.ToString(), entTemp.EXTENSIONORDERDETAILID));//可用金额
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "REMARK", entTemp.REMARK, entTemp.REMARK, entTemp.EXTENSIONORDERDETAILID));//摘要
                    AutoList.Add(basedata("T_FB_EXTENSIONORDERDETAIL", "APPLIEDMONEY", entTemp.APPLIEDMONEY.ToString(), entTemp.APPLIEDMONEY.ToString(), entTemp.EXTENSIONORDERDETAILID));//申领金额
                }
            }
            //冲借款明细
            
            if (fbCtr.ListBorrowDetail.Count() > 0)
            {
                
                foreach (T_FB_CHARGEAPPLYREPAYDETAIL item in fbCtr.ListBorrowDetail)//预算费用报销明细
                {
                    TrListObj.Add(item);

                    if (item.REPAYTYPE != null)
                    {
                        switch (item.REPAYTYPE.ToString())
                        { 
                            case "1":
                                chargetype = "现金还普通借款";
                                break;
                            case "2":
                                chargetype = "现金还备用金借款";
                                break;
                            case "3":
                                chargetype = "现金还专项借款";
                                break;
                            
                        }
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "CHARGETYPE", item.REPAYTYPE.ToString(), chargetype, item.CHARGEAPPLYREPAYDETAILID));//费用类型
                    }
                    if (item.BORROWMONEY != null)
                    {
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "BORROWMONEY", item.BORROWMONEY.ToString(), item.BORROWMONEY.ToString(), item.CHARGEAPPLYREPAYDETAILID));//科目编号
                    }

                    AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "REMARK", item.REMARK, item.REMARK, item.CHARGEAPPLYREPAYDETAILID));//摘要
                    if (item.REPAYMONEY != null)
                    {
                        AutoList.Add(basedata("T_FB_CHARGEAPPLYREPAYDETAIL", "REPAYMONEY", item.REPAYMONEY.ToString(), item.REPAYMONEY.ToString(), item.CHARGEAPPLYREPAYDETAILID));//申领金额
                    }
                }
            }
            //string a = mx.TableToXml(Info, TrListObj, StrSource, AutoList);
            string a = mx.TableToXmlForTravel(Info, TrListObj, StrSource, AutoList);

            return a;
        }
        #endregion

        #region IAudit
        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (actions == FormTypes.Edit && actions == FormTypes.Resubmit)
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            }
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("REIMBURSEMENTOFCOSTS", fbCtr.Order.TOTALMONEY.ToString());
            parameters.Add("POSTLEVEL", EmployeePostLevel);
            parameters.Add("DEPARTMENTNAME", depName);
            parameters.Add("BUSINESSTRIPID", businesstrID);

            if (TravelReimbursement != null && TravelReimbursement.T_OA_BUSINESSTRIP != null)
            {
                entity.SystemCode = "OA";
                if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                {
                    strXmlObjectSource = GetXmlString(TravelReimbursement, entity.BusinessObjectDefineXML);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销表单数据不能为空!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                if (clickSubmit == true)
                {
                    RefreshUI(RefreshedTypes.All);
                    clickSubmit = false;
                }
                return;
            }
            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", TravelReimbursement.OWNERID);
            paraIDs.Add("CreatePostID", TravelReimbursement.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", TravelReimbursement.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", TravelReimbursement.OWNERCOMPANYID);

            if (TravelReimbursement.REIMBURSEMENTOFCOSTS > 0 || fbCtr.Order.TOTALMONEY >0)
            {
                if (TravelReimbursement.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
                {
                    Utility.SetAuditEntity(entity, "T_OA_TRAVELREIMBURSEMENT", TravelReimbursement.TRAVELREIMBURSEMENTID, strXmlObjectSource, paraIDs);
                }
                else
                {
                    Utility.SetAuditEntity(entity, "T_OA_TRAVELREIMBURSEMENT", TravelReimbursement.TRAVELREIMBURSEMENTID, strXmlObjectSource);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出差报销费用不能为零，请填写报销费用!"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                if (clickSubmit == true)
                {
                    RefreshUI(RefreshedTypes.All);
                    clickSubmit = false;
                }
                return;
            }
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != TravelReimbursement.OWNERID)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if ((actions == FormTypes.Resubmit || actions == FormTypes.Edit) && canSubmit == false)
            {
                //RefreshUI(RefreshedTypes.ShowProgressBar);
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            Utility.InitFileLoad(FormTypes.Audit, uploadFile, TravelReimbursement.TRAVELREIMBURSEMENTID, false);
            RefreshUI(RefreshedTypes.HideProgressBar);

            if (actions == FormTypes.Audit)
            {
                IsAudit = false;
            }
            if (actions == FormTypes.Resubmit)
            {
                Resubmit = false;
            }

            if (TravelReimbursement.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }

            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中
                    state = Utility.GetCheckState(CheckStates.Approving);//提示提交成功
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    textStandards.Text = string.Empty;//清空报销标准说明
                    OaPersonOfficeClient.GetTravelReimbursementByIdAsync(travelReimbursementID);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                    state = Utility.GetCheckState(CheckStates.Approved);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSAUDIT"));//提示审核成功
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"),
                        Utility.GetResourceStr("SUCCESSAUDIT"));//提示审核成功
                    break;
            }
            TravelReimbursement.CHECKSTATE = state;
            clickSubmit = false;
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            if (TravelReimbursement.CHECKSTATE == Utility.GetCheckState(CheckStates.Approving))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
            }
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (TravelReimbursement != null)
                state = TravelReimbursement.CHECKSTATE;
            if (actions == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            OaPersonOfficeClient.DoClose();
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

        #region 出发城市lookup选择
        private void txtDEPARTURECITY_SelectClick(object sender, EventArgs e)
        {
            SearchCity txt = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                txt.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                if (DaGrs.SelectedItem != null)
                {
                    if (DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem) != null)
                    {
                        //T_OA_BUSINESSTRIPDETAIL list = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                        T_OA_REIMBURSEMENTDETAIL list = DaGrs.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                        SearchCity myCitys = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;//出发城市
                        SearchCity mystartCity = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;//目标城市
                        int k = citycode.IndexOf(list.DEPCITY);

                        if (k > -1)
                        {
                            citycode[k] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                            list.DEPCITY = citycode[k];
                        }
                        else
                        {
                            citycode.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                        }
                        if (citycode.Count > 1)
                        {
                            if (myCitys.TxtSelectedCity.Text.ToString().Trim() == mystartCity.TxtSelectedCity.Text.ToString().Trim())
                            {
                                myCitys.TxtSelectedCity.Text = string.Empty;
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                    }
                }
                if (citycode.Last().Split(',').Count() > 2)
                {
                    txt.TxtSelectedCity.Text = string.Empty;
                    citycode.RemoveAt(citycode.Count - 1);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCity)
            {
                (SelectCity as AreaSortCity).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
            SelectCity.GetSelectedCities.Visibility = Visibility.Collapsed;//隐藏已选中城市的Border
        }
        #endregion

        #region 目标城市LookUp选择事件
        private void txtTARGETCITIES_SelectClick(object sender, EventArgs e)
        {
            SearchCity txt = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                txt.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                if (DaGrs.SelectedItem != null)
                {
                    int SelectIndex = DaGrs.SelectedIndex;//选择的行数，选择的行数也就是目的城市的位置
                    if (DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem) != null)
                    {
                        T_OA_REIMBURSEMENTDETAIL list = DaGrs.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                        
                        //T_OA_BUSINESSTRIPDETAIL list = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                        SearchCity myCitys = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;
                        SearchCity mystartCity = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;
                        if (citycode.Count() > 0)
                        {
                            mystartCity.Tag = citycode[SelectIndex];//将旧的传递起来
                        }
                        if (string.IsNullOrEmpty(mystartCity.TxtSelectedCity.Text))
                        {
                            txt.TxtSelectedCity.Text = string.Empty;
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("请先选择出发城市"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                        if (cityscode.Count > 1)
                        {
                            if (mystartCity.TxtSelectedCity.Text.ToString().Trim() == myCitys.TxtSelectedCity.Text.ToString().Trim())
                            {
                                myCitys.TxtSelectedCity.Text = string.Empty;
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                        if (cityscode.Count >= (SelectIndex + 1))
                        {
                            cityscode[SelectIndex] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                        }
                        else
                        {
                            cityscode.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                        }
                        if (citycode.Count == (SelectIndex + 1))
                        {
                            citycode.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                            SetNextDepartureCity(SelectIndex);
                        }
                        else
                        {
                            citycode[SelectIndex] = mystartCity.Tag.ToString();
                            citycode[SelectIndex + 1] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();//出发城市中下一条记录
                            SetNextDepartureCity(SelectIndex);
                        }
                        //将选择的城市值赋给对应的集合
                        if (TrList.Count() > 0)
                        {
                            var ents = from ent in TrList
                                       where ent.REIMBURSEMENTDETAILID == list.REIMBURSEMENTDETAILID
                                       select ent;
                            TrList.ForEach(item => {
                                if (item.REIMBURSEMENTDETAILID == list.REIMBURSEMENTDETAILID)
                                {
                                    item.DESTCITY = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                                }
                            });

                        }
                    }
                }
                if (cityscode.Last().Split(',').Count() > 2)
                {
                    txt.TxtSelectedCity.Text = string.Empty;
                    cityscode.RemoveAt(cityscode.Count - 1);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("CITY"), "", "123", SelectCity, false, false, null);
            if (SelectCity is AreaSortCity)
            {
                (SelectCity as AreaSortCity).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
            SelectCity.GetSelectedCities.Visibility = Visibility.Collapsed;//隐藏已选中城市的Border
        }
        /// <summary>
        /// 设置选中的下一个出发城市的值
        /// </summary>
        /// <param name="SelectIndex"></param>
        private void SetNextDepartureCity(int SelectIndex)
        {
            int EachCount = 0;
            foreach (Object obje in DaGrs.ItemsSource)//将下一个出发城市的值修改
            {
                EachCount++;
                if (DaGrs.Columns[1].GetCellContent(obje) != null)
                {
                    SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                    if ((SelectIndex + 2) == EachCount)
                    {
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citycode[SelectIndex + 1]);
                    }
                }
            }
        }
        #endregion

        #region 键盘事件
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    if (DaGrs.SelectedIndex == TrList.Count - 1)
            //    {
            //        T_OA_REIMBURSEMENTDETAIL buport = new T_OA_REIMBURSEMENTDETAIL();
            //        buport.REIMBURSEMENTDETAILID = Guid.NewGuid().ToString();
            //        buport.STARTDATE = DateTime.Now;
            //        buport.ENDDATE = DateTime.Now;
            //        TrList.Add(buport);
            //    }
            //}
        }
        #endregion

        #region 删除
        private void myDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DaGrs.SelectedItems == null)
            {
                return;
            }

            if (DaGrs.SelectedItems.Count == 0)
            {
                return;
            }
            if (DaGrs.SelectedItems.Count > 0)//判断是否有选中数据,否则提醒用户
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();//提醒用户是否真的删除该数据,以免操作失误。
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    TrList = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    if (TrList.Count() > 1)
                    {
                        for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
                        {
                            T_OA_REIMBURSEMENTDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_REIMBURSEMENTDETAIL;

                            if (TrList.Contains(entDel))
                            {
                                TrList.Remove(entDel);
                            }
                        }
                        DaGrs.ItemsSource = TrList;
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, Result);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("SELECTERROR", "DELETE"), Utility.GetResourceStr("CONFIRMBUTTON"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region LoadingRow事件

        //定义combox默认颜色变量
        Brush tempcomTypeBorderBrush;
        Brush tempcomTypeForeBrush;
        Brush tempcomLevelBorderBrush;
        Brush tempcomLevelForeBrush;
        Brush txtASubsidiesForeBrush;
        Brush txtASubsidiesBorderBrush;

        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                T_OA_REIMBURSEMENTDETAIL tmp = (T_OA_REIMBURSEMENTDETAIL)e.Row.DataContext;

                DateTimePicker dpStartTime = DaGrs.Columns[0].GetCellContent(e.Row).FindName("StartTime") as DateTimePicker;
                DateTimePicker dpEndTime = DaGrs.Columns[2].GetCellContent(e.Row).FindName("EndTime") as DateTimePicker;
                SearchCity myCity = DaGrs.Columns[1].GetCellContent(e.Row).FindName("txtDEPARTURECITY") as SearchCity;
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(e.Row).FindName("txtTARGETCITIES") as SearchCity;
                TextBox txtTranSportcosts = DaGrs.Columns[8].GetCellContent(e.Row).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
                TextBox txtASubsidies = DaGrs.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;//住宿标准
                TextBox txtTFSubsidies = DaGrs.Columns[10].GetCellContent(e.Row).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                TextBox txtMealSubsidies = DaGrs.Columns[11].GetCellContent(e.Row).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
                TravelDictionaryComboBox ComVechile = DaGrs.Columns[6].GetCellContent(e.Row).FindName("ComVechileType") as TravelDictionaryComboBox;
                TravelDictionaryComboBox ComLevel = DaGrs.Columns[7].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                TextBox txtOtherCosts = DaGrs.Columns[12].GetCellContent(e.Row).FindName("txtOtherCosts") as TextBox;//其他费用
                CheckBox IsCheck = DaGrs.Columns[13].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                CheckBox IsCheckMeet = DaGrs.Columns[14].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
                CheckBox IsCheckCar = DaGrs.Columns[15].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;
                ImageButton MyButton_Delbaodao = DaGrs.Columns[16].GetCellContent(e.Row).FindName("myDelete") as ImageButton;

                //对默认控件的颜色进行赋值
                tempcomTypeBorderBrush = ComVechile.BorderBrush;
                tempcomTypeForeBrush = ComVechile.Foreground;
                tempcomLevelBorderBrush = ComLevel.BorderBrush;
                tempcomLevelForeBrush = ComLevel.Foreground;
                txtASubsidiesForeBrush = txtASubsidies.Foreground;
                txtASubsidiesBorderBrush = txtASubsidies.BorderBrush;
                T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

                if (BtnNewButton == true)
                {
                    myCitys.TxtSelectedCity.Text = string.Empty;
                    citycode.Add(tmp.DEPCITY);
                    cityscode.Add(string.Empty);
                }
                else
                {
                    BtnNewButton = false;
                }

                MyButton_Delbaodao.Margin = new Thickness(0);
                MyButton_Delbaodao.AddButtonAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_delete.png", Utility.GetResourceStr("DELETE"));
                MyButton_Delbaodao.Tag = tmp;
                myCity.Tag = tmp;
                myCitys.Tag = tmp;

                //查询出发城市&目标城市&&将ID转换为Name
                if (DaGrs.ItemsSource != null)
                {
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    int i = 0;
                    foreach (var obje in objs)
                    {
                        i++;
                        if (obje.REIMBURSEMENTDETAILID == tmp.REIMBURSEMENTDETAILID)//判断记录的ID是否相同
                        {
                            string dictid = "";
                            ComVechile.SelectedIndex = 0;
                            ComLevel.SelectedIndex = 0;
                            DaGrs.SelectedItem = e.Row;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();

                            entareaallowance = StandardsMethod(i);

                            if (actions != FormTypes.New)
                            {
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        //myCity.TxtSelectedCity.Text = GetCityName(obje.DEPCITY);
                                        //注释原因：obje.depcity仍然是中文而不是数字
                                        myCity.TxtSelectedCity.Text = GetCityName(tmp.DEPCITY);
                                        if (TrList.Count() > 1)
                                        {
                                            if (i > 1)
                                            {
                                                myCity.IsEnabled = false;
                                                ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                                            }
                                        }
                                    }
                                }
                                if (myCitys != null)//目标城市
                                {
                                    if (obje.DESTCITY != null)
                                    {
                                        myCitys.TxtSelectedCity.Text = GetCityName(obje.DESTCITY);
                                    }
                                }
                                if (obje.PRIVATEAFFAIR == "1")//私事
                                {
                                    IsCheck.IsChecked = true;
                                }
                                if (obje.GOOUTTOMEET == "1")//外出开会
                                {
                                    IsCheckMeet.IsChecked = true;
                                }
                                if (obje.COMPANYCAR == "1")//公司派车
                                {
                                    IsCheckCar.IsChecked = true;
                                }
                                if (txtASubsidies != null)//住宿标准
                                {
                                    txtASubsidies.Text = obje.ACCOMMODATION.ToString();
                                }
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    txtTFSubsidies.Text = obje.TRANSPORTATIONSUBSIDIES.ToString();
                                    ((DataGridCell)((StackPanel)txtTFSubsidies.Parent).Parent).IsEnabled = false;
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到交通补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    txtMealSubsidies.Text = obje.MEALSUBSIDIES.ToString();
                                    ((DataGridCell)((StackPanel)txtMealSubsidies.Parent).Parent).IsEnabled = false;
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }

                                #region 查看和审核时隐藏DataGrid模板中的控件
                                if (actions == FormTypes.Browse || actions == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                    ComVechile.IsEnabled = false;
                                    ComLevel.IsEnabled = false;
                                }
                                if (actions != FormTypes.New || actions != FormTypes.Edit)
                                {
                                    if (TravelReimbursement.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                        txtTFSubsidies.IsReadOnly = true;
                                        txtMealSubsidies.IsReadOnly = true;
                                        txtOtherCosts.IsReadOnly = true;
                                        txtTranSportcosts.IsReadOnly = true;
                                        ComVechile.IsEnabled = false;
                                        ComLevel.IsEnabled = false;
                                    }
                                }
                                if (entareaallowance != null)
                                {
                                    if (txtASubsidies.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())//判断住宿费超标
                                    {
                                        txtASubsidies.BorderBrush = new SolidColorBrush(Colors.Red);
                                        txtASubsidies.Foreground = new SolidColorBrush(Colors.Red);
                                        txtAccommodation.Visibility = Visibility.Visible;
                                        this.txtAccommodation.Text = "住宿费超标";
                                    }
                                    if (txtASubsidies.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())
                                    {
                                        if (txtASubsidiesForeBrush != null)
                                        {
                                            txtASubsidies.Foreground = txtASubsidiesForeBrush;
                                        }
                                        if (txtASubsidiesBorderBrush != null)
                                        {
                                            txtASubsidies.BorderBrush = txtASubsidiesBorderBrush;
                                        }
                                    }
                                }
                                #endregion

                                #region 获取交通工具类型和级别
                                if (ComVechile != null)//交通工具类型
                                {
                                    type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                    level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                    var thd = takethestandardtransport.FirstOrDefault();
                                    thd = this.GetVehicleTypeValue("");

                                    foreach (T_SYS_DICTIONARY Region in ComVechile.Items)
                                    {
                                        if (thd != null)
                                        {
                                            dictid = Region.DICTIONARYID;
                                            if (Region.DICTIONARYVALUE.ToString() == tmp.TYPEOFTRAVELTOOLS)
                                            {
                                                if (takethestandardtransport.Count() > 0)
                                                {
                                                    ComVechile.SelectedItem = Region;
                                                    if (thd.TYPEOFTRAVELTOOLS.ToInt32() > Region.DICTIONARYVALUE)
                                                    {
                                                        ComVechile.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                        ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                        this.txtTips.Visibility = Visibility.Visible;
                                                        this.txtTips.Text = "交通工具超标";
                                                    }
                                                    if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= Region.DICTIONARYVALUE)
                                                    {
                                                        if (tempcomTypeBorderBrush != null)
                                                        {
                                                            ComVechile.BorderBrush = tempcomTypeBorderBrush;
                                                        }
                                                        if (tempcomTypeForeBrush != null)
                                                        {
                                                            ComVechile.Foreground = tempcomTypeForeBrush;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (ComLevel != null)//交通工具级别
                                {
                                    var ents = from ent in ListVechileLevel
                                               where ent.T_SYS_DICTIONARY2.DICTIONARYID == dictid
                                               select ent;
                                    ComLevel.ItemsSource = ents.ToList();
                                    if (ents.Count() > 0)
                                    {
                                        type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                        level = ComLevel.SelectedItem as T_SYS_DICTIONARY;

                                        var thd = takethestandardtransport.FirstOrDefault();
                                        if (type != null)
                                        {
                                            thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                        }
                                        if (thd != null)
                                        {
                                            foreach (T_SYS_DICTIONARY RegionLevel in ComLevel.Items)
                                            {
                                                if (RegionLevel.DICTIONARYVALUE.ToString() == tmp.TAKETHETOOLLEVEL)
                                                {
                                                    ComLevel.SelectedItem = RegionLevel;
                                                    if (thd.TAKETHETOOLLEVEL.ToInt32() <= RegionLevel.DICTIONARYVALUE)
                                                    {
                                                        if (tempcomLevelForeBrush != null)
                                                        {
                                                            ComLevel.Foreground = tempcomLevelForeBrush;
                                                        }
                                                        if (tempcomLevelBorderBrush != null)
                                                        {
                                                            ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (thd.TAKETHETOOLLEVEL.ToInt32() > RegionLevel.DICTIONARYVALUE)
                                                        {
                                                            ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                            this.txtTips.Visibility = Visibility.Visible;
                                                            this.txtTips.Text = "交通工具超标";
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            if (tempcomLevelForeBrush != null)
                                                            {
                                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                                            }
                                                            if (tempcomLevelBorderBrush != null)
                                                            {
                                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }// ComLevel != null
                                }
                                #endregion
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #region 计算出差补贴补贴
        /// <summary>
        /// 计算补贴
        /// </summary>
        private void TravelAllowance()
        {
            if (DaGrs.ItemsSource != null)
            {
                T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();

                ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                double total = 0;
                int i = 0;
                foreach (var obje in objs)
                {
                    i++;
                    double toodays = 0;
                    TextBox txtTFSubsidies = new TextBox();//初始化交通补贴控件
                    TextBox txtMealSubsidies = new TextBox();//初始化餐费补贴控件
                    TextBox txtASubsidies = new TextBox();//初始化住宿费控件

                    //GetTFSubsidiesTextBox(txtTFSubsidies, i).Text = string.Empty;//再次计算的时候先清空已存在的交通补贴
                    //GetMealSubsidiesTextBox(txtMealSubsidies, i).Text = string.Empty;//再次计算的时候先清空已存在的餐费补贴
                    if (i >= 1) txtTFSubsidies = DaGrs.Columns[10].GetCellContent(obje).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                    if (i >= 1) txtMealSubsidies = DaGrs.Columns[11].GetCellContent(obje).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴

                    List<string> list = new List<string>
                                {
                                     obje.BUSINESSDAYS
                                };

                    if (obje.BUSINESSDAYS != null && !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                    {
                        double totalHours = System.Convert.ToDouble(list[0]);
                        toodays = totalHours;
                    }
                    double tresult = toodays;//计算本次出差的总天数

                    string cityValue = cityscode[i - 1].Replace(",", "");//目标城市值
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);

                    #region 根据本次出差的总天数,根据天数获取相应的补贴
                    if (travelsolutions != null && employeepost != null)
                    {
                        TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
                        TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

                        txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i);//交通补贴控件赋值
                        txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i);//交通费控件赋值
                        txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i);//住宿费控件赋值
                        txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i);//其他费用控件赋值
                        txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i);//餐费补贴控件赋值

                        if (tresult <= travelsolutions.MINIMUMINTERVALDAYS.ToInt32())//本次出差总时间小于等于设定天数的报销标准
                        {
                            if (entareaallowance != null)
                            {
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    if (obje.BUSINESSDAYS != null)
                                    {
                                        if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                        {
                                            txtTFSubsidies.Text = "0";
                                            txtTFSubsidies.IsReadOnly = true;
                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                        }
                                        else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                        {
                                            txtTFSubsidies.Text = "0";
                                        }
                                        else
                                        {
                                            if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                            {
                                                txtTFSubsidies.Text = (entareaallowance.TRANSPORTATIONSUBSIDIES.ToDouble() * toodays).ToString();
                                                //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                                {
                                                    ComfirmWindow com = new ComfirmWindow();
                                                    com.OnSelectionBoxClosed += (obj, result) =>
                                                    {
                                                        txtTranSportcosts.IsReadOnly = true;//交通费
                                                        txtASubsidies.IsReadOnly = true;//住宿标准
                                                        txtOtherCosts.IsReadOnly = true;//其他费用
                                                    };
                                                    if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                    {
                                                        com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                txtTFSubsidies.Text = "0";
                                                txtTFSubsidies.IsReadOnly = false;
                                            }
                                        }
                                    }
                                    else//如果天数为null的禁用住宿费控件
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    if (obje.BUSINESSDAYS != null)
                                    {
                                        if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                        {
                                            txtMealSubsidies.Text = "0";
                                            txtMealSubsidies.IsReadOnly = true;
                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                        }
                                        else if (obje.GOOUTTOMEET == "1")//如果是开会
                                        {
                                            txtMealSubsidies.Text = "0";
                                        }
                                        else
                                        {
                                            if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                            {
                                                txtMealSubsidies.Text = (entareaallowance.MEALSUBSIDIES.ToDouble() * toodays).ToString();
                                                //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                                {
                                                    ComfirmWindow com = new ComfirmWindow();
                                                    com.OnSelectionBoxClosed += (obj, result) =>
                                                    {
                                                        txtTranSportcosts.IsReadOnly = true;//交通费
                                                        txtASubsidies.IsReadOnly = true;//住宿标准
                                                        txtOtherCosts.IsReadOnly = true;//其他费用
                                                    };
                                                    if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                    {
                                                        com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                txtMealSubsidies.Text = "0";
                                                txtMealSubsidies.IsReadOnly = false;
                                            }
                                        }
                                    }
                                    else//如果天数为null的禁用住宿费控件
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                txtTFSubsidies.Text = "0";
                                txtMealSubsidies.Text = "0";
                            }
                        }
                    }
                    #endregion

                    #region 如果出差天数大于设定的最大天数,按驻外标准获取补贴
                    if (travelsolutions != null && employeepost != null)
                    {
                        TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
                        TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

                        txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i);//交通补贴控件赋值
                        txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i);//交通费控件赋值
                        txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i);//住宿费控件赋值
                        txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i);//其他费用控件赋值
                        txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i);//餐费补贴控件赋值

                        if (tresult > travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                        {
                            if (entareaallowance != null)
                            {
                                double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                if (entareaallowance != null)
                                {
                                    if (txtTFSubsidies != null)//交通补贴
                                    {
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                txtTFSubsidies.Text = "0";
                                                txtTFSubsidies.IsReadOnly = true;
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            }
                                            else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                            {
                                                txtTFSubsidies.Text = "0";
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                    double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                    //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble() / 2;
                                                    //除以2是因为驻外标准不分餐费和交通补贴，2者合2为一，否则会多加
                                                    double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble()/2;
                                                    txtTFSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

                                                    //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                    if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                                    {
                                                        ComfirmWindow com = new ComfirmWindow();
                                                        com.OnSelectionBoxClosed += (obj, result) =>
                                                        {
                                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                                        };
                                                        if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                        {
                                                            com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    txtTFSubsidies.Text = "0";
                                                    txtTFSubsidies.IsReadOnly = false;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                    if (txtMealSubsidies != null)//餐费补贴
                                    {
                                        if (obje.BUSINESSDAYS != null)
                                        {
                                            if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                            {
                                                txtMealSubsidies.Text = "0";
                                                txtMealSubsidies.IsReadOnly = true;
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            }
                                            else if (obje.GOOUTTOMEET == "1")//如果是开会
                                            {
                                                txtMealSubsidies.Text = "0";
                                            }
                                            else
                                            {
                                                if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                                {
                                                    double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                    //double middlemoney = (travelsolutions.MAXIMUMRANGEDAYS.ToDouble() - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                    double IntMaxDays = travelsolutions.MAXIMUMRANGEDAYS.ToDouble();
                                                    double IntMinDAys = travelsolutions.MINIMUMINTERVALDAYS.ToDouble();
                                                    double middlemoney = (IntMaxDays - IntMinDAys) * mealSubsidies;
                                                    //double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble();
                                                    //驻外标准：交通费和餐费补贴为一起的，所以除以2
                                                    double lastmoney = (tresult - travelsolutions.MAXIMUMRANGEDAYS.ToDouble()) * entareaallowance.OVERSEASSUBSIDIES.ToDouble()/2;
                                                    txtMealSubsidies.Text = (minmoney + middlemoney + lastmoney).ToString();

                                                    //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                    if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                                    {
                                                        ComfirmWindow com = new ComfirmWindow();
                                                        com.OnSelectionBoxClosed += (obj, result) =>
                                                        {
                                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                                        };
                                                        if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                        {
                                                            com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    txtMealSubsidies.Text = "0";
                                                    txtMealSubsidies.IsReadOnly = false;
                                                }
                                            }
                                        }
                                        else//如果天数为null的禁用住宿费控件
                                        {
                                            txtASubsidies.IsReadOnly = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                txtTFSubsidies.Text = "0";
                                txtMealSubsidies.Text = "0";
                            }
                        }
                    }
                    #endregion

                    #region 如果出差时间大于设定的最小天数并且小于设定的最大天数的报销标准
                    if (travelsolutions != null && employeepost != null)
                    {
                        TextBox txtTranSportcosts = new TextBox();//初始化交通费控件
                        TextBox txtOtherCosts = new TextBox();//初始化其他费用控件

                        txtTFSubsidies = GetTFSubsidiesTextBox(txtTFSubsidies, i);//交通补贴控件赋值
                        txtTranSportcosts = GetTranSportcostsTextBox(txtTranSportcosts, i);//交通费控件赋值
                        txtASubsidies = GetASubsidiesTextBox(txtASubsidies, i);//住宿费控件赋值
                        txtOtherCosts = GetOtherCostsTextBox(txtOtherCosts, i);//其他费用控件赋值
                        txtMealSubsidies = GetMealSubsidiesTextBox(txtMealSubsidies, i);//餐费补贴控件赋值

                        if (tresult >= travelsolutions.MINIMUMINTERVALDAYS.ToInt32() && tresult <= travelsolutions.MAXIMUMRANGEDAYS.ToInt32())
                        {
                            if (entareaallowance != null)
                            {
                                double DbTranceport = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble();
                                double DbMeal = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble();
                                double tfSubsidies = Convert.ToDecimal(entareaallowance.TRANSPORTATIONSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                double mealSubsidies = Convert.ToDecimal(entareaallowance.MEALSUBSIDIES).ToDouble() * (Convert.ToDecimal(travelsolutions.INTERVALRATIO).ToDouble() / 100);
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    if (obje.BUSINESSDAYS != null)
                                    {
                                        if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                        {
                                            txtTFSubsidies.Text = "0";
                                            txtTFSubsidies.IsReadOnly = true;
                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                        }
                                        else if (obje.GOOUTTOMEET == "1" || obje.COMPANYCAR == "1")//如果是开会或者是公司派车，交通费没有
                                        {
                                            txtTFSubsidies.Text = "0";
                                        }
                                        else
                                        {
                                            if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                            {
                                                double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbTranceport;
                                                double middlemoney = (tresult - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * tfSubsidies;
                                                txtTFSubsidies.Text = (minmoney + middlemoney).ToString();

                                                //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                                {
                                                    ComfirmWindow com = new ComfirmWindow();
                                                    com.OnSelectionBoxClosed += (obj, result) =>
                                                    {
                                                        txtTranSportcosts.IsReadOnly = true;//交通费
                                                        txtASubsidies.IsReadOnly = true;//住宿标准
                                                        txtOtherCosts.IsReadOnly = true;//其他费用
                                                    };
                                                    if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                    {
                                                        com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                txtTFSubsidies.Text = "0";
                                                txtTFSubsidies.IsReadOnly = false;
                                            }
                                        }
                                    }
                                    else//如果天数为null的禁用住宿费控件
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    if (obje.BUSINESSDAYS != null)
                                    {
                                        if (obje.PRIVATEAFFAIR == "1")//如果是私事不予报销
                                        {
                                            txtMealSubsidies.Text = "0";
                                            txtMealSubsidies.IsReadOnly = true;
                                            txtTranSportcosts.IsReadOnly = true;//交通费
                                            txtASubsidies.IsReadOnly = true;//住宿标准
                                            txtOtherCosts.IsReadOnly = true;//其他费用
                                        }
                                        else if (obje.GOOUTTOMEET == "1")//如果是开会
                                        {
                                            txtMealSubsidies.Text = "0";
                                        }
                                        else
                                        {
                                            if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                            {
                                                //最小区间段金额
                                                double minmoney = travelsolutions.MINIMUMINTERVALDAYS.ToDouble() * DbMeal;
                                                //中间区间段金额
                                                double middlemoney = (tresult - travelsolutions.MINIMUMINTERVALDAYS.ToDouble()) * mealSubsidies;
                                                txtMealSubsidies.Text = (minmoney + middlemoney).ToString();

                                                //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                                if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                                {
                                                    ComfirmWindow com = new ComfirmWindow();
                                                    com.OnSelectionBoxClosed += (obj, result) =>
                                                    {
                                                        txtTranSportcosts.IsReadOnly = true;//交通费
                                                        txtASubsidies.IsReadOnly = true;//住宿标准
                                                        txtOtherCosts.IsReadOnly = true;//其他费用
                                                    };
                                                    if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                                    {
                                                        com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                txtMealSubsidies.Text = "0";
                                                txtMealSubsidies.IsReadOnly = false;
                                            }
                                        }
                                    }
                                    else//如果天数为null的禁用住宿费控件
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                            {
                                txtTFSubsidies.Text = "0";
                                txtMealSubsidies.Text = "0";
                            }
                        }
                    }
                    #endregion

                    total += txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
                    this.txtFees.Text = total.ToString();//总费用
                    this.txtFee.Text = total.ToString();

                    Fees = total;
                }

                CountMoney();
            }
        }
        #endregion

        #region 查询DataGrid中的各项费用控件
        /// <summary>
        /// 交通费
        /// </summary>
        /// <param name="txtTranSportcosts"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetTranSportcostsTextBox(TextBox txtTranSportcosts, int i)
        {
            if (DaGrs.Columns[8].GetCellContent(TrList[i - 1]) != null)
            {
                txtTranSportcosts = DaGrs.Columns[8].GetCellContent(TrList[i - 1]).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
            }
            return txtTranSportcosts;
        }
        /// <summary>
        /// 住宿费
        /// </summary>
        /// <param name="txtASubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetASubsidiesTextBox(TextBox txtASubsidies, int i)
        {
            if (DaGrs.Columns[9].GetCellContent(TrList[i - 1]) != null)
            {
                txtASubsidies = DaGrs.Columns[9].GetCellContent(TrList[i - 1]).FindName("txtACCOMMODATION") as TextBox;//住宿费
            }
            return txtASubsidies;
        }
        /// <summary>
        /// 交通补贴
        /// </summary>
        /// <param name="txtTFSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetTFSubsidiesTextBox(TextBox txtTFSubsidies, int i)
        {
            if (DaGrs.Columns[10].GetCellContent(TrList[i - 1]) != null)
            {
                txtTFSubsidies = DaGrs.Columns[10].GetCellContent(TrList[i - 1]).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
            }
            
            return txtTFSubsidies;
        }
        /// <summary>
        /// 餐费补贴
        /// </summary>
        /// <param name="txtMealSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetMealSubsidiesTextBox(TextBox txtMealSubsidies, int i)
        {
            if (DaGrs.Columns[11].GetCellContent(TrList[i - 1]) != null)
            {
                txtMealSubsidies = DaGrs.Columns[11].GetCellContent(TrList[i - 1]).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
            }
            return txtMealSubsidies;
        }
        /// <summary>
        /// 其他费用
        /// </summary>
        /// <param name="txtMealSubsidies"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private TextBox GetOtherCostsTextBox(TextBox txtOtherCosts, int i)
        {
            if (DaGrs.Columns[12].GetCellContent(TrList[i - 1]) != null)
            {
                txtOtherCosts = DaGrs.Columns[12].GetCellContent(TrList[i - 1]).FindName("txtOtherCosts") as TextBox;//其他费用
            }
            return txtOtherCosts;
        }
        #endregion

        /// <summary>
        /// 查看、审核时用(将DataGr模版中的控件全部替换为TextBlock,以便在新平台中节约空间)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DaGrss_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                T_OA_REIMBURSEMENTDETAIL tmp = (T_OA_REIMBURSEMENTDETAIL)e.Row.DataContext;

                TextBlock dpStartTimelook = DaGrss.Columns[0].GetCellContent(e.Row).FindName("tbStartTime") as TextBlock;
                TextBlock dpEndTime = DaGrss.Columns[2].GetCellContent(e.Row).FindName("tbEndTime") as TextBlock;
                TextBlock myCity = DaGrss.Columns[1].GetCellContent(e.Row).FindName("tbDEPARTURECITY") as TextBlock;
                TextBlock myCitys = DaGrss.Columns[3].GetCellContent(e.Row).FindName("tbTARGETCITIES") as TextBlock;
                TextBox txtTranSportcosts = DaGrss.Columns[8].GetCellContent(e.Row).FindName("txtTRANSPORTCOSTS") as TextBox;//交通费
                TextBox txtASubsidies = DaGrss.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;//住宿标准
                TextBox txtTFSubsidies = DaGrss.Columns[10].GetCellContent(e.Row).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                TextBox txtMealSubsidies = DaGrss.Columns[11].GetCellContent(e.Row).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴
                TextBlock ComVechile = DaGrss.Columns[6].GetCellContent(e.Row).FindName("tbComVechileType") as TextBlock;
                TextBlock ComLevel = DaGrss.Columns[7].GetCellContent(e.Row).FindName("tbComVechileTypeLeve") as TextBlock;
                TextBox txtOtherCosts = DaGrss.Columns[12].GetCellContent(e.Row).FindName("txtOtherCosts") as TextBox;//其他费用
                CheckBox IsCheck = DaGrss.Columns[13].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                CheckBox IsCheckMeet = DaGrss.Columns[14].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
                CheckBox IsCheckCar = DaGrss.Columns[15].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;

                //对默认控件的颜色进行赋值
                tempcomTypeForeBrush = ComVechile.Foreground;
                tempcomLevelForeBrush = ComLevel.Foreground;
                txtASubsidiesForeBrush = txtASubsidies.Foreground;
                txtASubsidiesBorderBrush = txtASubsidies.BorderBrush;
                //DaGrss.Columns[5].Visibility = Visibility.Collapsed;

                if (BtnNewButton == true)
                {
                    myCitys.Text = string.Empty;
                    citycode.Add(tmp.DEPCITY);
                }
                else
                {
                    BtnNewButton = false;
                }

                //查询出发城市&目标城市&&将ID转换为Name
                if (DaGrss.ItemsSource != null)
                {
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrss.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    int i = 0;
                    foreach (var obje in objs)
                    {
                        i++;
                        if (obje.REIMBURSEMENTDETAILID == tmp.REIMBURSEMENTDETAILID)//判断记录的ID是否相同
                        {
                            DaGrss.SelectedItem = e.Row;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();

                            T_OA_AREAALLOWANCE entareaallowance = StandardsMethod(i);

                            #region 修改、查看、审核

                            if (actions != FormTypes.New)
                            {
                                #region 获取目标城市、各项补贴数据
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        myCity.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(obje.DEPCITY);
                                    }
                                }
                                if (myCitys != null)//目标城市
                                {
                                    if (obje.DESTCITY != null)
                                    {
                                        myCitys.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(obje.DESTCITY);
                                    }
                                }
                                if (obje.TYPEOFTRAVELTOOLS != null)//交通工具类型
                                {
                                    ComVechile.Text = GetTypeName(obje.TYPEOFTRAVELTOOLS);
                                }
                                if (obje.TAKETHETOOLLEVEL != null)//交通工具级别
                                {
                                    ComLevel.Text = GetLevelName(obje.TAKETHETOOLLEVEL, GetTypeId(obje.TYPEOFTRAVELTOOLS));
                                }
                                if (obje.PRIVATEAFFAIR == "1")//私事
                                {
                                    IsCheck.IsChecked = true;
                                }
                                if (obje.GOOUTTOMEET == "1")//外出开会
                                {
                                    IsCheckMeet.IsChecked = true;
                                }
                                if (obje.COMPANYCAR == "1")//公司派车
                                {
                                    IsCheckCar.IsChecked = true;
                                }
                                if (txtASubsidies != null)//住宿标准
                                {
                                    txtASubsidies.Text = obje.ACCOMMODATION.ToString();
                                }
                                if (txtTFSubsidies != null)//交通补贴
                                {
                                    txtTFSubsidies.Text = obje.TRANSPORTATIONSUBSIDIES.ToString();
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtTFSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                if (txtMealSubsidies != null)//餐费补贴
                                {
                                    txtMealSubsidies.Text = obje.MEALSUBSIDIES.ToString();
                                    if (EmployeePostLevel.ToInt32() > 8)//当前用户的岗位级别小于副部长及以上级别的补贴标准
                                    {
                                        //在正常状态下如果没有获取到补贴(没有对应的城市补贴或其他导致的问题)提示用户是否继续操作
                                        if (string.IsNullOrWhiteSpace(txtMealSubsidies.Text))
                                        {
                                            ComfirmWindow com = new ComfirmWindow();
                                            com.OnSelectionBoxClosed += (obj, result) =>
                                            {
                                                txtTranSportcosts.IsReadOnly = true;//交通费
                                                txtASubsidies.IsReadOnly = true;//住宿标准
                                                txtOtherCosts.IsReadOnly = true;//其他费用
                                            };
                                            if (obje.BUSINESSDAYS != null || !string.IsNullOrEmpty(obje.BUSINESSDAYS))
                                            {
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }
                                #endregion

                                #region 查看和审核时隐藏DataGrid模板中的控件
                                if (actions == FormTypes.Browse || actions == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                }
                                if (actions != FormTypes.New || actions != FormTypes.Edit)
                                {
                                    if (TravelReimbursement.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                                    {
                                        txtASubsidies.IsReadOnly = true;
                                        txtTFSubsidies.IsReadOnly = true;
                                        txtMealSubsidies.IsReadOnly = true;
                                        txtOtherCosts.IsReadOnly = true;
                                        txtTranSportcosts.IsReadOnly = true;
                                    }
                                }
                                if (entareaallowance != null)
                                {
                                    if (txtASubsidies.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())//判断住宿费超标
                                    {
                                        txtASubsidies.BorderBrush = new SolidColorBrush(Colors.Red);
                                        txtASubsidies.Foreground = new SolidColorBrush(Colors.Red);
                                        txtAccommodation.Visibility = Visibility.Visible;
                                        this.txtAccommodation.Text = "住宿费超标";
                                    }
                                    if (txtASubsidies.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * obje.THENUMBEROFNIGHTS.ToDouble())
                                    {
                                        if (txtASubsidiesForeBrush != null && txtASubsidies.Foreground==null)
                                        {
                                            txtASubsidies.Foreground = txtASubsidiesForeBrush;
                                        }
                                        if (txtASubsidiesBorderBrush != null && txtASubsidies.BorderBrush==null)
                                        {
                                            txtASubsidies.BorderBrush = txtASubsidiesBorderBrush;
                                        }
                                    }
                                }
                                #endregion

                                #region 获取交通工具类型、级别
                                if (ComVechile != null)
                                {
                                    var thd = takethestandardtransport.FirstOrDefault();
                                    thd = this.GetVehicleTypeValue("");

                                    if (thd != null)
                                    {
                                        if (takethestandardtransport.Count() > 0)
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > obje.TYPEOFTRAVELTOOLS.ToInt32())
                                            {
                                                ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                this.txtTips.Visibility = Visibility.Visible;
                                                this.txtTips.Text = "交通工具超标";
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= obje.TYPEOFTRAVELTOOLS.ToInt32())
                                            {
                                                if (tempcomTypeForeBrush != null)
                                                {
                                                    ComVechile.Foreground = tempcomTypeForeBrush;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (ComLevel != null)//交通工具级别
                                {
                                    //获取交通工具类型
                                    int ii = CheckTraveToolStand(obje.TYPEOFTRAVELTOOLS.ToString(), obje.TAKETHETOOLLEVEL.ToString(), EmployeePostLevel);
                                    switch (ii)
                                    {
                                        case 0://类型超标
                                            ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtTips.Visibility = Visibility.Visible;
                                            this.txtTips.Text = "交通工具超标";
                                            break;
                                        case 1://级别超标
                                            ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtTips.Visibility = Visibility.Visible;
                                            this.txtTips.Text = "交通工具超标";
                                            break;
                                        case 2://没超标，则隐藏
                                            this.txtTips.Visibility = Visibility.Collapsed;
                                            this.txtTips.Text = "";
                                            break;
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                    //CountMoneyA();
                    CountTravelDays(tmp,e);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }


        #region 计算出差天数 ljx
        private void CountTravelDays(T_OA_REIMBURSEMENTDETAIL detail,DataGridRowEventArgs e)
        {
            try
            {
                int i = 0;
                if (DaGrss.ItemsSource == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用

                TextBox myDaysTime = DaGrss.Columns[5].GetCellContent(e.Row).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
                TextBox textAccommodation = DaGrss.Columns[9].GetCellContent(e.Row).FindName("txtACCOMMODATION") as TextBox;

                foreach (object obj in DaGrss.ItemsSource)
                {
                    i++;

                    //if (DaGrss.Columns[9].GetCellContent(obj) == null)
                    //{
                    //    break;
                    //}
                    if (((T_OA_REIMBURSEMENTDETAIL)obj).REIMBURSEMENTDETAILID == detail.REIMBURSEMENTDETAILID)
                    {
                        
                        T_OA_REIMBURSEMENTDETAIL obje = obj as T_OA_REIMBURSEMENTDETAIL;
                        ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrss.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                        //出差天数
                        double toodays = 0;
                        //获取出差补贴
                        T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                        string cityValue = cityscode[i - 1].Replace(",", "");//目标城市值
                        //根据城市查出差标准补贴（已根据岗位级别过滤）
                        entareaallowance = this.GetAllowanceByCityValue(cityValue);

                        //循环出差报告的天数
                        int k = 0;
                        if (actions == FormTypes.New)
                        {
                            foreach (T_OA_BUSINESSTRIPDETAIL objDetail in buipList)
                            {
                                k++;
                                if (k == i)
                                {
                                    if (!string.IsNullOrEmpty(objDetail.BUSINESSDAYS))
                                    {
                                        double totalHours = System.Convert.ToDouble(objDetail.BUSINESSDAYS);
                                        //出差天数
                                        toodays = totalHours;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (detail.BUSINESSDAYS != null && detail.BUSINESSDAYS != "")
                            {
                                toodays = System.Convert.ToDouble(detail.BUSINESSDAYS);
                            }
                            
                        }
                        if (entareaallowance != null)
                        {
                            if (toodays > 0)
                            {
                                if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * Convert.ToDouble(detail.THENUMBEROFNIGHTS))//判断住宿费超标
                                {
                                    //文本框标红
                                    textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                    textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                    this.txtAccommodation.Visibility = Visibility.Visible;
                                    this.txtAccommodation.Text = "住宿费超标";
                                }
                            }
                            if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * Convert.ToDouble(detail.THENUMBEROFNIGHTS))
                            {
                                if (txtASubsidiesForeBrush != null)
                                {
                                    textAccommodation.Foreground = txtASubsidiesForeBrush;
                                }
                                if (txtASubsidiesBorderBrush != null)
                                {
                                    textAccommodation.BorderBrush = txtASubsidiesBorderBrush;
                                }
                                string StrMessage = "";
                                StrMessage = this.txtAccommodation.Text;
                                if (string.IsNullOrEmpty(StrMessage))
                                {
                                    this.txtAccommodation.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    
                }

                DaGrss.Columns[5].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion
        private T_OA_AREAALLOWANCE StandardsMethod(int i)
        {
            T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
            string cityValue = cityscode[i - 1].Replace(",", "");//目标城市值
            entareaallowance = this.GetAllowanceByCityValue(cityValue);

            if (entareaallowance != null)//根据出差的城市及出差人的级别，将当前出差人的标准信息显示在备注中
            {
                if (i <= TrList.Count() && TrList.Count() > 1)
                {
                    if (TrList[i - 1].PRIVATEAFFAIR == "1")//如果是私事
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + "无" + ",餐费补贴：" + "无" + ",住宿标准：" + "无" + "。\n";
                    }
                    else if (TrList[i - 1].GOOUTTOMEET == "1")//如果是内部会议及培训
                    {
                        //textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。\n";
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。<br/>";
                    }
                    else if (TrList[i - 1].COMPANYCAR == "1")//如果是公司派车
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() + "元,住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                    {
                        textStandards.Text = "您的岗位级别≥'I'级,无各项差旅补贴。";
                        textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                }
                if (TrList.Count() == 1)   //只有一条记录的情况
                {
                    if (TrList[i - 1].PRIVATEAFFAIR == "1")//如果是私事
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + "无" + "，餐费补贴：" + "无" + "，住宿标准：" + "无" + "。\n";
                    }
                    else if (TrList[i - 1].GOOUTTOMEET == "1")//如果是内部会议及培训
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差为《内部会议、培训》，无各项差旅补贴。\n";
                    }
                    else if (TrList[i - 1].COMPANYCAR == "1")//如果是公司派车
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + "无" + "餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else if (EmployeePostLevel.ToInt32() <= 8)//当前用户的岗位级别小于副部长及以上级别的无各项补贴
                    {
                        textStandards.Text = "您的岗位级别≥'I'级，无各项差旅补贴。";
                        textStandards.Text = textStandards.Text + "住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                    else
                    {
                        textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "的出差报销标准是：交通补贴：" + entareaallowance.TRANSPORTATIONSUBSIDIES + "元，餐费补贴：" + entareaallowance.MEALSUBSIDIES.ToString() + "元，住宿标准：" + entareaallowance.ACCOMMODATION + "元。\n";
                        //textStandards.Text += "(以上为员工现岗位级别的补贴，仅供参考)";
                    }
                }
                bxbz = textStandards.Text;
            }
            else
            {
                textStandards.Text = textStandards.Text + SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(cityValue) + "没有相应的出差标准。\n";
            }
            return entareaallowance;
        }
        #endregion

        #region 根据当前用户的级别过滤出该级别能乘坐的交通工具类型

        /// <summary>
        /// 根据当前用户的级别过滤出该级别能乘坐的交通工具类型
        /// </summary>
        /// <param name="TraveToolType">交通工具类型</param>
        /// <param name="postLevel">岗位级别</param>
        /// <returns>0：类型超标，1：类型不超标，2：级别不超标</returns>
        private int CheckTraveToolStand(string TraveToolType, string TraveToolLevel, string postLevel)
        {
            int i = 0;
            var q = from ent in takethestandardtransport
                    where ent.ENDPOSTLEVEL.Contains(postLevel) && ent.TYPEOFTRAVELTOOLS == TraveToolType
                    orderby ent.TAKETHETOOLLEVEL ascending
                    select ent;
            if (q.Count() > 0)
            {
                i = 1;
            }
            var qLevel = from ent in q
                         where ent.TAKETHETOOLLEVEL.Contains(TraveToolLevel)
                         select ent;
            if (qLevel.Count() > 0)
            {
                i = 2;
            }
            return i;
        }

        private T_OA_TAKETHESTANDARDTRANSPORT GetVehicleTypeValue(string ToolType)
        {
            try
            {
                if (string.IsNullOrEmpty(ToolType))
                {
                    var q = from ent in takethestandardtransport
                            where ent.ENDPOSTLEVEL.Contains(EmployeePostLevel)
                            orderby ent.TAKETHETOOLLEVEL ascending
                            select ent;
                    q = q.OrderBy(n => n.TYPEOFTRAVELTOOLS);
                    if (q.Count() > 0)
                    {
                        return q.FirstOrDefault();
                    }
                }
                else
                {
                    var q = from ent in takethestandardtransport
                            where ent.ENDPOSTLEVEL.Contains(EmployeePostLevel) && ent.TYPEOFTRAVELTOOLS == ToolType
                            orderby ent.TAKETHETOOLLEVEL ascending
                            select ent;

                    if (q.Count() > 0)
                    {
                        return q.FirstOrDefault();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            return null;
        }
        #endregion

        #region 交通工具类型、级别
        /// <summary>
        /// 获取交通工具的级别
        /// </summary>
        void GetVechileLevelInfos()
        {
            List<T_SYS_DICTIONARY> dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;
            var objs = from d in dicts
                       where d.DICTIONCATEGORY == "VICHILELEVEL"
                       orderby d.DICTIONARYVALUE
                       select d;
            ListVechileLevel = objs.ToList();
        }

        private void ComVechileType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
                if (vechiletype.SelectedIndex >= 0)
                {
                    var thd = takethestandardtransport.FirstOrDefault();
                    thd = this.GetVehicleTypeValue("");
                    T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                    if (DaGrs.SelectedItem != null)
                    {
                        if (DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;

                            var ListObj = from ent in ListVechileLevel
                                          where ent.T_SYS_DICTIONARY2.DICTIONARYID == VechileTypeObj.DICTIONARYID
                                          orderby ent.DICTIONARYVALUE descending
                                          select ent;
                            if (ListObj.Count() > 0)
                            {
                                ComLevel.ItemsSource = ListObj;
                                ComLevel.SelectedIndex = 0;
                            }
                        }
                    }
                    if (employeepost != null)
                    {

                        if (DaGrs.SelectedItem != null)
                        {
                            if (DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem) != null)
                            {
                                TravelDictionaryComboBox ComLevel = DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                                TravelDictionaryComboBox ComType = DaGrs.Columns[6].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                type = ComType.SelectedItem as T_SYS_DICTIONARY;

                                if (thd != null)
                                {
                                    if (type != null)
                                    {
                                        if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= type.DICTIONARYVALUE)
                                        {
                                            if (tempcomTypeBorderBrush != null)
                                            {
                                                ComType.BorderBrush = tempcomTypeBorderBrush;
                                            }
                                            if (tempcomTypeForeBrush != null)
                                            {
                                                ComType.Foreground = tempcomTypeForeBrush;
                                            }
                                            if (tempcomLevelForeBrush != null)
                                            {
                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                            }
                                            if (tempcomLevelBorderBrush != null)
                                            {
                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                            }
                                            this.txtTips.Visibility = Visibility.Collapsed;
                                        }
                                        else
                                        {
                                            if (level != null)
                                            {
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                    this.txtTips.Visibility = Visibility.Visible;
                                                    this.txtTips.Text = "交通工具超标";
                                                    return;
                                                }
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                    this.txtTips.Visibility = Visibility.Visible;
                                                    this.txtTips.Text = "交通工具超标";
                                                    return;
                                                }
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() <= type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() <= level.DICTIONARYVALUE)
                                            {
                                                this.txtTips.Visibility = Visibility.Collapsed;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void ComVechileTypeLeve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
                if (vechiletype.SelectedIndex >= 0)
                {
                    if (employeepost != null)
                    {

                        var thd = takethestandardtransport.FirstOrDefault();

                        T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                        if (DaGrs.SelectedItem != null)
                        {
                            if (DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem) != null)
                            {
                                TravelDictionaryComboBox ComLevel = DaGrs.Columns[7].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                                TravelDictionaryComboBox ComType = DaGrs.Columns[6].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                type = ComType.SelectedItem as T_SYS_DICTIONARY;
                                if (type != null)
                                {
                                    thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                    if (thd == null)
                                    {
                                        ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                        ComType.Foreground = new SolidColorBrush(Colors.Red);
                                        return;
                                    }
                                    if (level != null)
                                    {
                                        if (thd.TAKETHETOOLLEVEL.ToInt32() < level.DICTIONARYVALUE)
                                        {
                                            if (tempcomLevelForeBrush != null)
                                            {
                                                ComLevel.Foreground = tempcomLevelForeBrush;
                                            }
                                            if (tempcomLevelBorderBrush != null)
                                            {
                                                ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                            }
                                            if (tempcomTypeBorderBrush != null)
                                            {
                                                ComType.BorderBrush = tempcomTypeBorderBrush;
                                            }
                                            if (tempcomTypeForeBrush != null)
                                            {
                                                ComType.Foreground = tempcomTypeForeBrush;
                                            }
                                        }
                                        else
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                this.txtTips.Visibility = Visibility.Visible;
                                                this.txtTips.Text = "交通工具超标";
                                                return;
                                            }
                                            else if (thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                            {
                                                ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                this.txtTips.Visibility = Visibility.Visible;
                                                this.txtTips.Text = "交通工具超标";
                                                return;
                                            }
                                            else
                                            {
                                                if (tempcomLevelForeBrush != null)
                                                {
                                                    ComLevel.Foreground = tempcomLevelForeBrush;
                                                }
                                                if (tempcomLevelBorderBrush != null)
                                                {
                                                    ComLevel.BorderBrush = tempcomLevelBorderBrush;
                                                }
                                                this.txtTips.Visibility = Visibility.Collapsed;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 开始时间
        private void StartTime_OnValueChanged(object sender, EventArgs e)
        {
            object obj = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;

            if (obj == null)
            {
                return;
            }

            DateTimePicker dpStartTime = DaGrs.Columns[0].GetCellContent(obj).FindName("StartTime") as DateTimePicker;
            DateTimePicker dpEndTime = DaGrs.Columns[2].GetCellContent(obj).FindName("EndTime") as DateTimePicker;
            TextBox myDaysTime = DaGrs.Columns[4].GetCellContent(obj).FindName("txtTOTALDAYS") as TextBox;

            if (dpStartTime == null || dpEndTime == null || myDaysTime == null)
            {
                return;
            }

            //如果出发时间与到达时间相等,视为当天往返，出差时间算一天
            if (dpStartTime.Value.Value.Date == dpEndTime.Value.Value.Date && travelsolutions.ANDFROMTHATDAY == "1")
            {
                myDaysTime.Text = "1天";
            }
            else
            {
                double TotalDays = 0;//出差天数
                int TotalHours = 0;//出差小时
                TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                TotalDays = ts.Days;
                TotalHours = ts.Hours;

                int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();

                if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
                {
                    TotalDays += 1;
                }
                else
                {
                    if (TotalHours > 0)
                        TotalDays += 0.5;
                }
                myDaysTime.Text = TotalDays.ToString() + "天";
            }
        }
        #endregion

        #region 结束时间
        private void EndTime_OnValueChanged(object sender, EventArgs e)
        {
            object obj = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;

            if (obj == null)
            {
                return;
            }

            DateTimePicker dpStartTime = DaGrs.Columns[0].GetCellContent(obj).FindName("StartTime") as DateTimePicker;
            DateTimePicker dpEndTime = DaGrs.Columns[2].GetCellContent(obj).FindName("EndTime") as DateTimePicker;
            TextBox myDaysTime = DaGrs.Columns[4].GetCellContent(obj).FindName("txtTOTALDAYS") as TextBox;

            if (dpStartTime == null || dpEndTime == null || myDaysTime == null)
            {
                return;
            }

            //如果出发时间与到达时间相等,视为当天往返，出差时间算一天
            if (dpStartTime.Value.Value.Date == dpEndTime.Value.Value.Date && travelsolutions.ANDFROMTHATDAY == "1")
            {
                myDaysTime.Text = "1天";
            }
            else
            {
                double TotalDays = 0;//出差天数
                int TotalHours = 0;//出差小时
                TimeSpan tsStart = new TimeSpan(dpStartTime.Value.Value.Ticks);
                TimeSpan tsEnd = new TimeSpan(dpEndTime.Value.Value.Ticks);
                TimeSpan ts = tsEnd.Subtract(tsStart).Duration();

                TotalDays = ts.Days;
                TotalHours = ts.Hours;

                int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();

                if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
                {
                    TotalDays += 1;
                }
                else
                {
                    if (TotalHours > 0)
                        TotalDays += 0.5;
                }
                myDaysTime.Text = TotalDays.ToString() + "天";
            }
        }
        #endregion

        #region 交通工具费用
        private void txtTRANSPORTCOSTS_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 其他费用
        private void txtOtherCosts_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 住宿费
        private void txtACCOMMODATION_LostFocus(object sender, RoutedEventArgs e)
        {
            CountMoney();
        }
        #endregion

        #region 住宿时间计算
        public void TravelTimeCalculation()
        {
            if (TrList == null || DaGrs.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TrList.Count; i++)
            {
                GetTraveTimeCalculationTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TrList[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TrList[i].ENDDATE);
                string FirstTraveFrom = TrList[i].DEPCITY;
                string FirstTraveTo = TrList[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TrList.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TrList[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TrList[j].ENDDATE);
                    string NextTraveFrom = TrList[j].DEPCITY;
                    string NextTraveTo = TrList[j].DESTCITY;
                    GetTraveTimeCalculationTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom)
                        {
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            i = j - 1;
                            OneDayTrave = true;
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (OneDayTrave == true) continue;
                //非当天往返
                decimal TotalDays = 0;
                switch (TrList.Count())
                {
                    case 1:
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TrList[i + 1].ENDDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TrList.Count() - 1) break;//最后一条记录不处理
                        if (i == TrList.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TrList[i + 1].ENDDATE);
                            TotalDays = CaculateTravCalculationDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TrList[i + 1].STARTDATE);
                        TotalDays = CaculateTravCalculationDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveTimeCalculationTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }
        private TextBox GetTraveTimeCalculationTextBox(TextBox myDaysTime, int i)
        {
            if (DaGrs.Columns[5].GetCellContent(TrList[i]) != null)
            {
                myDaysTime = DaGrs.Columns[5].GetCellContent(TrList[i]).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
            }
            return myDaysTime;
        }

        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="FirstStartTime">开始时间</param>
        /// <param name="NextStartTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravCalculationDays(DateTime FirstStartTime, DateTime NextStartTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = NextStartTime.Subtract(FirstStartTime.Date);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;

            return TotalDays;
        }
        #endregion

        #region 住宿费，交通费，其他费用
        private void CountMoney()
        {
            try
            {
                TravelTimeCalculation();

                double totall = 0;
                int i = 0;
                if (DaGrs.ItemsSource == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用
                bool IsPassEd = false;//住宿费是否超标
                foreach (object obj in DaGrs.ItemsSource)
                {
                    i++;
                    if (DaGrs.Columns[8].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrs.Columns[9].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    if (DaGrs.Columns[12].GetCellContent(obj) == null)
                    {
                        return;
                    }
                    TextBox myDaysTime = DaGrs.Columns[5].GetCellContent(obj).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
                    TextBox textTransportcosts = DaGrs.Columns[8].GetCellContent(obj).FindName("txtTRANSPORTCOSTS") as TextBox;
                    TextBox textAccommodation = DaGrs.Columns[9].GetCellContent(obj).FindName("txtACCOMMODATION") as TextBox;
                    TextBox textOthercosts = DaGrs.Columns[12].GetCellContent(obj).FindName("txtOtherCosts") as TextBox;
                    TextBox txtTFSubsidies = DaGrs.Columns[10].GetCellContent(obj).FindName("txtTRANSPORTATIONSUBSIDIES") as TextBox;//交通补贴
                    TextBox txtMealSubsidies = DaGrs.Columns[11].GetCellContent(obj).FindName("txtMEALSUBSIDIES") as TextBox;//餐费补贴

                    T_OA_REIMBURSEMENTDETAIL obje = obj as T_OA_REIMBURSEMENTDETAIL;
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    //出差天数
                    double toodays = 0;
                    //获取出差补贴
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = cityscode[i - 1].Replace(",", "");//目标城市值
                    //根据城市查出差标准补贴（已根据岗位级别过滤）
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);

                    //循环出差报告的天数
                    int k = 0;
                    if (actions == FormTypes.New)
                    {
                        foreach (T_OA_BUSINESSTRIPDETAIL objDetail in buipList)
                        {
                            k++;
                            if (k == i)
                            {
                                if (!string.IsNullOrEmpty(objDetail.BUSINESSDAYS))
                                {
                                    double totalHours = System.Convert.ToDouble(objDetail.BUSINESSDAYS);
                                    //出差天数
                                    toodays = totalHours;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var objDetail in objs)
                        {
                            k++;
                            if (k == i)
                            {
                                if (myDaysTime != null && !string.IsNullOrEmpty(myDaysTime.Text) && myDaysTime.Text != "0")
                                {
                                    double totalHours = System.Convert.ToDouble(myDaysTime.Text);
                                    //住宿天数
                                    toodays = totalHours;

                                    if (entareaallowance != null)
                                    {
                                        
                                        if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * toodays)//判断住宿费超标
                                        {
                                            //文本框标红
                                            textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                            textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                            this.txtAccommodation.Visibility = Visibility.Visible;
                                            IsPassEd = true;
                                            //this.txtAccommodation.Text = "住宿费超标";
                                        }

                                        if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * toodays)
                                        {
                                            if (txtASubsidiesForeBrush != null)
                                            {
                                                textAccommodation.Foreground = txtASubsidiesForeBrush;
                                            }
                                            if (txtASubsidiesBorderBrush != null)
                                            {
                                                textAccommodation.BorderBrush = txtASubsidiesBorderBrush;
                                            }
                                            string StrMessage = "";
                                            StrMessage = this.txtAccommodation.Text;
                                            if (string.IsNullOrEmpty(StrMessage))
                                            {
                                                this.txtAccommodation.Visibility = Visibility.Collapsed;
                                            }
                                        }

                                    }
                                    
                                }
                                break;
                            }
                        }
                    }
                    
                    double ta = textTransportcosts.Text.ToDouble() + textAccommodation.Text.ToDouble() + textOthercosts.Text.ToDouble();
                    totall = totall + ta;

                    Fees = txtTFSubsidies.Text.ToDouble() + txtMealSubsidies.Text.ToDouble();
                    totall += Fees;
                }
                if (IsPassEd)
                {
                    this.txtAccommodation.Text = "住宿费超标";
                }
                else
                {
                    this.txtAccommodation.Text = string.Empty;
                    this.txtAccommodation.Visibility = Visibility.Collapsed;
                }
                txtFees.Text = totall.ToString();
                txtFee.Text = totall.ToString();
                travelReimbursement.REIMBURSEMENTOFCOSTS =decimal.Parse(totall.ToString());
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        /// <summary>
        /// 住宿天数
        /// </summary>
        private void CountMoneyA()
        {
            try
            {
                int i = 0;
                if (DaGrss.ItemsSource == null)
                {
                    return;
                }
                //住宿费，交通费，其他费用
                foreach (object obj in DaGrss.ItemsSource)
                {
                    i++;
                    if (DaGrss.Columns[6].GetCellContent(obj) == null)
                    {
                        break;
                    }
                    TextBox myDaysTime = DaGrss.Columns[5].GetCellContent(obj).FindName("txtTHENUMBEROFNIGHTS") as TextBox;
                    TextBox textAccommodation = DaGrss.Columns[9].GetCellContent(obj).FindName("txtACCOMMODATION") as TextBox;

                    T_OA_REIMBURSEMENTDETAIL obje = obj as T_OA_REIMBURSEMENTDETAIL;
                    ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrss.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                    //出差天数
                    double toodays = 0;
                    //获取出差补贴
                    T_OA_AREAALLOWANCE entareaallowance = new T_OA_AREAALLOWANCE();
                    string cityValue = cityscode[i - 1].Replace(",", "");//目标城市值
                    //根据城市查出差标准补贴（已根据岗位级别过滤）
                    entareaallowance = this.GetAllowanceByCityValue(cityValue);

                    //循环出差报告的天数
                    int k = 0;
                    if (actions == FormTypes.New)
                    {
                        foreach (T_OA_BUSINESSTRIPDETAIL objDetail in buipList)
                        {
                            k++;
                            if (k == i)
                            {
                                if (!string.IsNullOrEmpty(objDetail.BUSINESSDAYS))
                                {
                                    double totalHours = System.Convert.ToDouble(objDetail.BUSINESSDAYS);
                                    //出差天数
                                    toodays = totalHours;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var objDetail in objs)
                        {
                            k++;
                            if (k == i)
                            {
                                if (myDaysTime != null && !string.IsNullOrEmpty(myDaysTime.Text) && myDaysTime.Text != "0")
                                {
                                    double totalHours = System.Convert.ToDouble(myDaysTime.Text);
                                    //住宿天数
                                    toodays = totalHours;                                   
                                    break;
                                }
                            }
                        }
                    }
                    if (entareaallowance != null)
                    {
                        if (toodays > 0)
                        {
                            if (textAccommodation.Text.ToDouble() > entareaallowance.ACCOMMODATION.ToDouble() * toodays)//判断住宿费超标
                            {
                                //文本框标红
                                textAccommodation.BorderBrush = new SolidColorBrush(Colors.Red);
                                textAccommodation.Foreground = new SolidColorBrush(Colors.Red);
                                this.txtAccommodation.Visibility = Visibility.Visible;
                                
                                this.txtAccommodation.Text = "住宿费超标";
                            }
                        }
                        if (textAccommodation.Text.ToDouble() <= entareaallowance.ACCOMMODATION.ToDouble() * toodays)
                        {
                            if (txtASubsidiesForeBrush != null)
                            {
                                textAccommodation.Foreground = txtASubsidiesForeBrush;
                            }
                            if (txtASubsidiesBorderBrush != null)
                            {
                                textAccommodation.BorderBrush = txtASubsidiesBorderBrush;
                            }
                            string StrMessage = "";
                            StrMessage = this.txtAccommodation.Text;
                            if (string.IsNullOrEmpty(StrMessage))
                            {
                                this.txtAccommodation.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }

                DaGrss.Columns[5].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #region 私事myChkBox_Checked事件
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        btlist.PRIVATEAFFAIR = "1";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (!chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        btlist.PRIVATEAFFAIR = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
        #endregion

        #region 外出开会
        private void myChkBoxMeet_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        var ents = from ent in TrList
                                   where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            int k = TrList.IndexOf(ents.FirstOrDefault());
                            TrList[k].GOOUTTOMEET = "1";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选内部会议或培训，无各项补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBoxMeet_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TrList
                               where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TrList.IndexOf(ents.FirstOrDefault());
                        TrList[k].GOOUTTOMEET = "0";
                    }
                }
            }
        }
        #endregion

        #region 公司派车
        private void myChkBoxCar_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox chk = sender as CheckBox;
                if (chk.IsChecked.Value)
                {
                    T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                    if (btlist != null)
                    {
                        var ents = from ent in TrList
                                   where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                                   select ent;
                        if (ents.Count() > 0)
                        {
                            int k = TrList.IndexOf(ents.FirstOrDefault());
                            TrList[k].COMPANYCAR = "1";
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选公司派车，无交通补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBoxCar_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_REIMBURSEMENTDETAIL btlist = (T_OA_REIMBURSEMENTDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TrList
                               where ent.REIMBURSEMENTDETAILID == btlist.REIMBURSEMENTDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TrList.IndexOf(ents.FirstOrDefault());
                        TrList[k].COMPANYCAR = "0";
                    }
                }
            }
        }
        #endregion

        #region 隐藏和显示FB控件
        private void fbChkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (fbChkBox.IsChecked == true)
            {
                scvFB.Visibility = Visibility.Visible;
            }
        }

        private void fbChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (fbChkBox.IsChecked == false)
            {
                scvFB.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 隐藏附件控件
        public void FileLoadedCompleted()
        {
            //if (!ctrFile._files.HasAccessory)
            //{
            //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayoutRoot, 6);
            //    this.lblFile.Visibility = Visibility.Collapsed;
            //}
        }
        #endregion

        #region 出差时间计算
        /// <summary>
        /// 计算出差天数
        /// </summary>
        public void TravelTime()
        {
            if (TrList == null || DaGrs.ItemsSource == null)
            {
                return;
            }
            #region 存在多条的处理
            TextBox myDaysTime = new TextBox();
            bool OneDayTrave = false;
            for (int i = 0; i < TrList.Count; i++)
            {
                GetTraveDayTextBox(myDaysTime, i).Text = string.Empty;
                OneDayTrave = false;
                //记录本条记录以便处理
                DateTime FirstStartTime = Convert.ToDateTime(TrList[i].STARTDATE);
                DateTime FirstEndTime = Convert.ToDateTime(TrList[i].ENDDATE);
                string FirstTraveFrom = TrList[i].DEPCITY;
                string FirstTraveTo = TrList[i].DESTCITY;
                //遍历剩余的记录
                for (int j = i + 1; j < TrList.Count; j++)
                {
                    DateTime NextStartTime = Convert.ToDateTime(TrList[j].STARTDATE);
                    DateTime NextEndTime = Convert.ToDateTime(TrList[j].ENDDATE);
                    string NextTraveFrom = TrList[j].DEPCITY;
                    string NextTraveTo = TrList[j].DESTCITY;
                    GetTraveDayTextBox(myDaysTime, j).Text = string.Empty;
                    if (NextEndTime.Date == FirstStartTime.Date)
                    {
                        if (NextTraveTo == FirstTraveFrom && (TrList.Count == 2 || TrList.Count==1))
                        {
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = "1";
                            //i = j - 1;
                            OneDayTrave = true;
                            break;
                        }
                        else continue;
                    }
                    else
                    {
                        break;
                    }
                }
                if (OneDayTrave == true) continue;
                //非当天往返
                decimal TotalDays = 0;
                switch (TrList.Count())
                {
                    case 1:
                        TotalDays = CaculateTravDays(FirstStartTime, FirstEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    case 2:
                        if (i == 1) break;
                        DateTime NextEndTime = Convert.ToDateTime(TrList[i + 1].ENDDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextEndTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                    default:
                        if (i == TrList.Count() - 1) break;//最后一条记录不处理
                        if (i == TrList.Count() - 2)//倒数第二条记录=最后一条结束时间-上一条开始时间
                        {
                            DateTime NextENDDATETime = Convert.ToDateTime(TrList[i + 1].ENDDATE);
                            TotalDays = CaculateTravDays(FirstStartTime, NextENDDATETime);
                            myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                            myDaysTime.Text = TotalDays.ToString();
                            break;
                        }
                        //否则出差时间=下一条开始时间-上一条开始时间
                        DateTime NextStartTime = Convert.ToDateTime(TrList[i + 1].STARTDATE);
                        TotalDays = CaculateTravDays(FirstStartTime, NextStartTime);
                        myDaysTime = GetTraveDayTextBox(myDaysTime, i);
                        myDaysTime.Text = TotalDays.ToString();
                        break;
                }
            }
            #endregion
        }

        private TextBox GetTraveDayTextBox(TextBox myDaysTime, int i)
        {
            if (DaGrs.Columns[4].GetCellContent(TrList[i]) != null)
            {
                myDaysTime = DaGrs.Columns[4].GetCellContent(TrList[i]).FindName("txtTOTALDAYS") as TextBox;
            }
            return myDaysTime;
        }
        /// <summary>
        /// 计算出差时长结算-开始时间NextStartTime-FirstStartTime
        /// </summary>
        /// <param name="FirstStartTime">开始时间</param>
        /// <param name="NextStartTime">结束时间</param>
        /// <returns></returns>
        private decimal CaculateTravDays(DateTime FirstStartTime, DateTime NextStartTime)
        {
            //计算出差时间（天数）
            TimeSpan TraveDays = NextStartTime.Subtract(FirstStartTime);
            decimal TotalDays = 0;//出差天数
            decimal TotalHours = 0;//出差小时
            TotalDays = TraveDays.Days;
            TotalHours = TraveDays.Hours;
            int customhalfday = travelsolutions.CUSTOMHALFDAY.ToInt32();
            if (TotalHours >= customhalfday)//如果出差时间大于等于方案设置的时间，按方案标准时间计算
            {
                TotalDays += 1;
            }
            else
            {
                if (TotalHours > 0)
                    TotalDays += Convert.ToDecimal("0.5");//TotalDays += decimal.Round(TotalHours / 24,1);
            }
            return TotalDays;
        }
        #endregion

        #region 行删除事件
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (DaGrs.SelectedItems == null)
            {
                return;
            }

            if (DaGrs.SelectedItems.Count == 0)
            {
                return;
            }

            TrList = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
            if (TrList.Count() > 1)
            {
                for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
                {
                    int k = DaGrs.SelectedIndex;//当前选中行
                    T_OA_REIMBURSEMENTDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_REIMBURSEMENTDETAIL;

                    if (TrList.Contains(entDel))
                    {

                        TrList.Remove(entDel);
                        if (cityscode.Count > k)
                        {

                            int EachCount = 0;
                            foreach (Object obje in DaGrs.ItemsSource)//将下一个出发城市的值修改
                            {
                                EachCount++;
                                if (DaGrs.Columns[1].GetCellContent(obje) != null)
                                {
                                    SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                                    if ((k + 1) == EachCount)
                                    {
                                        if (k > 0)
                                        {
                                            mystarteachCity.TxtSelectedCity.Text = GetCityName(cityscode[k - 1]);
                                            citycode[k + 1] = cityscode[k - 1];//上一城市的城市值
                                        }
                                    }
                                }
                            }
                            cityscode.RemoveAt(k);//清除目标城市的值
                            citycode.RemoveAt(k);//清除出发城市的值
                        }
                    }
                }
                DaGrs.ItemsSource = TrList;
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "必须保留一条出差时间及地点!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        private void myChkBoxMeet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                var temp = DaGrs.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                CheckBox chbMeet = DaGrs.Columns[14].GetCellContent(temp).FindName("myChkBoxMeet") as CheckBox;
                if (chbMeet.IsChecked == true)
                {
                    temp.GOOUTTOMEET = "1";
                }
                else
                {
                    temp.GOOUTTOMEET = "0";
                }

                TravelAllowance();
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        private void myChkBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<T_OA_REIMBURSEMENTDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_REIMBURSEMENTDETAIL>;
                var temp = DaGrs.SelectedItem as T_OA_REIMBURSEMENTDETAIL;
                CheckBox chbMeet = DaGrs.Columns[13].GetCellContent(temp).FindName("myChkBox") as CheckBox;
                if (chbMeet.IsChecked == true)
                {
                    temp.PRIVATEAFFAIR = "1";
                }
                else
                {
                    temp.PRIVATEAFFAIR = "0";
                }

                TravelAllowance();
            }
            catch
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }
    }
}
