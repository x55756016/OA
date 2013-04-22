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

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class TravelReimbursementControl : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        private SmtOAPersonOfficeClient OaPersonOfficeClient;
        private PersonnelServiceClient HrPersonnelclient;
        private OaInterfaceClient OaInterfaceClient;//预算费用报销单号使用
        private bool isPageloadCompleted = false;//控制Tab切换时的数据加载 
        private V_Travelmanagement businesstrip = new V_Travelmanagement();
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
        private List<string> citysStartList_Golbal = new List<string>();
        private List<string> citysEndList_Golbal = new List<string>();
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
            HrPersonnelclient = new PersonnelServiceClient();
            areacitys = new List<T_OA_AREACITY>();
            OaInterfaceClient = new OaInterfaceClient();
            HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetAllEmployeePostBriefByEmployeeIDCompleted);
            HrPersonnelclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetEmployeePostBriefByEmployeeIDCompleted);
            OaPersonOfficeClient.UpdateTravelReimbursementCompleted += new EventHandler<UpdateTravelReimbursementCompletedEventArgs>(TrC_UpdateTravelReimbursementCompleted);
            OaPersonOfficeClient.GetTravelReimbursementByIdCompleted += new EventHandler<GetTravelReimbursementByIdCompletedEventArgs>(TrC_GetTravelReimbursementByIdCompleted);
            //OaPersonOfficeClient.GetTravelReimbursementDetailCompleted += new EventHandler<GetTravelReimbursementDetailCompletedEventArgs>(TrC_GetTravelReimbursementDetailCompleted);
            OaPersonOfficeClient.GetTravelSolutionByCompanyIDCompleted += new EventHandler<GetTravelSolutionByCompanyIDCompletedEventArgs>(TrC_GetTravelSolutionByCompanyIDCompleted);
            OaPersonOfficeClient.GetTravleAreaAllowanceByPostValueCompleted += new EventHandler<GetTravleAreaAllowanceByPostValueCompletedEventArgs>(TrC_GetTravleAreaAllowanceByPostValueCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);//保存费用
            fbCtr.InitDataComplete += new EventHandler<FrameworkUI.FBControls.ChargeApplyControl.InitDataCompletedArgs>(fbCtr_InitDataComplete);
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
            InitData();

            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
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

        void TravelReimbursementControl_Loaded(object sender, RoutedEventArgs e)
        {
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);

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
        }

        #endregion


        #region 查询出差报销主表，本页面打开的主入口
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
                    isPageloadCompleted = true;
                    TravelReimbursement_Golbal = e.Result;

                    //ljx  2011-8-29  
                    if (formType == FormTypes.Edit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.Approving)).ToString()
                            || TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.Approved)).ToString()
                            || TravelReimbursement_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.UnApproved)).ToString())
                        {
                            formType = FormTypes.Audit;
                            ShowAudits.Visibility = Visibility.Collapsed;
                            tbShowAudits.Visibility = Visibility.Visible;
                            Utility.InitFileLoad("TravelRequest", TravelReimbursement_Golbal.TRAVELREIMBURSEMENTID, formType, uploadFile);
                        }
                    }
                    if (formType == FormTypes.Resubmit)//重新提交
                    {
                        TravelReimbursement_Golbal.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                    }

                    txtPeopleTravel.Text = TravelReimbursement_Golbal.CLAIMSWERENAME;//报销人
                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.TEL))
                    {
                        txtTELL.Text = TravelReimbursement_Golbal.TEL;//联系电话
                    }
                    ReimbursementTime.Text = TravelReimbursement_Golbal.CREATEDATE.Value.ToShortDateString();//报销时间
                    txtChargeApplyTotal.Text = TravelReimbursement_Golbal.REIMBURSEMENTOFCOSTS.ToString();//本次差旅总费用
                    txtSubTotal.Text = TravelReimbursement_Golbal.THETOTALCOST.ToString();//差旅合计

                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.NOBUDGETCLAIMS))//报销单号
                    {
                        txtNoClaims.Text = string.Empty;
                        txtNoClaims.Text = TravelReimbursement_Golbal.NOBUDGETCLAIMS;
                    }
                    if (!string.IsNullOrEmpty(TravelReimbursement_Golbal.REMARKS))
                    {
                        txtRemark.Text = TravelReimbursement_Golbal.REMARKS;//备注
                    }

                    if (InitFB == false)
                    {
                        InitFBControl();
                    }
                    //HrPersonnelclient.GetEmployeePostBriefByEmployeeIDAsync(TravelReimbursement.OWNERID);
                    postName = TravelReimbursement_Golbal.OWNERPOSTNAME;
                    depName = TravelReimbursement_Golbal.OWNERDEPARTMENTNAME;
                    companyName = TravelReimbursement_Golbal.OWNERCOMPANYNAME;
                    EmployeePostLevel = TravelReimbursement_Golbal.POSTLEVEL;
                    string StrName = TravelReimbursement_Golbal.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
                    txtPeopleTravel.Text = StrName;
                    ToolTipService.SetToolTip(txtPeopleTravel, StrName);
                    EmployeeName = TravelReimbursement_Golbal.OWNERNAME;//出差人



                    if (formType != FormTypes.New || formType != FormTypes.Edit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    else if (formType != FormTypes.Resubmit)
                    {
                        if (TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.Approving).ToString() ||
                            TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.Approved).ToString() ||
                            TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.WaittingApproval).ToString())
                        {
                            BrowseShieldedControl();
                        }
                    }
                    if (formType == FormTypes.New || formType == FormTypes.Edit
                        || formType == FormTypes.Resubmit)
                    {
                        //我的单据中用到(判断出差报告如果在未提交状态,FormType状态改为可编辑)
                        if (TravelReimbursement_Golbal.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
                        {
                            //将Form状态改为编辑
                            formType = FormTypes.Edit;
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            //重新启用Form中的控件
                            txtTELL.IsReadOnly = false;
                            fbCtr.IsEnabled = true;
                            txtRemark.IsReadOnly = false;
                            textStandards.IsReadOnly = false;
                        }
                        StrName = Common.CurrentLoginUserInfo.EmployeeName + "-" 
                            + Common.CurrentLoginUserInfo.UserPosts[0].PostName
                            + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-"
                            + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                        txtPeopleTravel.Text = StrName;
                        EmployeePostLevel = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
                        TravelReimbursement_Golbal.POSTLEVEL = EmployeePostLevel;

                        RefreshUI(RefreshedTypes.ShowProgressBar);
                        OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(TravelReimbursement_Golbal.OWNERCOMPANYID, null, null);
                    }
                    else
                    {
                        if (TravelReimbursement_Golbal.T_OA_REIMBURSEMENTDETAIL.Count > 0)
                        {
                            BindDataGrid(TravelReimbursement_Golbal.T_OA_REIMBURSEMENTDETAIL);
                            RefreshUI(RefreshedTypes.All);
                            if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                            {
                                RefreshUI(RefreshedTypes.AuditInfo);
                            }
                        }
                    }
                    //OaPersonOfficeClient.GetTravelReimbursementDetailAsync(TravelReimbursement.TRAVELREIMBURSEMENTID);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            finally
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
        }
        #endregion

        #region DataGrid BindData 加载显示出差报销数据
        private void BindDataGrid(ObservableCollection<T_OA_REIMBURSEMENTDETAIL> obj)//加载出差报销子表
        {
            TravelDetailList_Golbal = obj;

            citysStartList_Golbal.Clear();
            citysEndList_Golbal.Clear();
            foreach (T_OA_REIMBURSEMENTDETAIL detail in obj)
            {
                citysStartList_Golbal.Add(detail.DEPCITY);
                citysEndList_Golbal.Add(detail.DESTCITY);
            }
            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                DaGrss.ItemsSource = TravelDetailList_Golbal;
            }
            else
            {
                DaGrs.ItemsSource = TravelDetailList_Golbal;
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
                    citysStartList_Golbal.Add(tmp.DEPCITY);
                    citysEndList_Golbal.Add(string.Empty);
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

                            if (formType != FormTypes.New)
                            {
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        //myCity.TxtSelectedCity.Text = GetCityName(obje.DEPCITY);
                                        //注释原因：obje.depcity仍然是中文而不是数字
                                        myCity.TxtSelectedCity.Text = GetCityName(tmp.DEPCITY);
                                        if (TravelDetailList_Golbal.Count() > 1)
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
                                                if (formType == FormTypes.Audit) return;
                                                com.SelectionBox("操作确认", "当前单据没有获取到餐费补贴，是否继续操作？", ComfirmWindow.titlename, "");
                                            }
                                        }
                                    }
                                }

                                #region 查看和审核时隐藏DataGrid模板中的控件
                                if (formType == FormTypes.Browse || formType == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                    ComVechile.IsEnabled = false;
                                    ComLevel.IsEnabled = false;
                                }
                                if (formType != FormTypes.New || formType != FormTypes.Edit)
                                {
                                    if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
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
                    citysStartList_Golbal.Add(tmp.DEPCITY);
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

                            if (formType != FormTypes.New)
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
                                if (formType == FormTypes.Browse || formType == FormTypes.Audit)
                                {
                                    txtASubsidies.IsReadOnly = true;
                                    txtTFSubsidies.IsReadOnly = true;
                                    txtMealSubsidies.IsReadOnly = true;
                                    txtOtherCosts.IsReadOnly = true;
                                    txtTranSportcosts.IsReadOnly = true;
                                }
                                if (formType != FormTypes.New || formType != FormTypes.Edit)
                                {
                                    if (TravelReimbursement_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
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


        #endregion

    }
}
