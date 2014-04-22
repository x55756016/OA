/********************************************************************************
//出差申请form，alter by ken 2013/3/27
*********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Data;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.SelectCityControl;
using System.Windows.Media;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.MobileXml;

namespace SMT.SaaS.OA.UI.Views.Travelmanagement
{
    public partial class TravelRequestForm : BaseForm, IClient, IEntityEditor, IAudit
    {
        #region 全局变量
        private bool isPageloadCompleted = false;//控制Tab切换时的数据加载
        private SmtOAPersonOfficeClient OaPersonOfficeClient;
        //private PersonnelServiceClient HrPersonnelclient;
        private SmtOACommonOfficeClient OaCommonOfficeClient;
       
        private FormTypes formType;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;

        private T_OA_AGENTDATESET AgentDateSet;
        private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> TraveDetailList_Golbal = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        //private T_OA_BUSINESSTRIPDETAIL TraveDetailList_Golbal[i] = new T_OA_BUSINESSTRIPDETAIL();
        public List<T_SYS_DICTIONARY> ListVechileLevel = new List<T_SYS_DICTIONARY>();
        public List<T_OA_CANTAKETHEPLANELINE> cantaketheplaneline = new List<T_OA_CANTAKETHEPLANELINE>();//可乘坐飞机线路设置
        public T_OA_TRAVELSOLUTIONS travelsolutions_Golbal;//出差方案
        /// <summary>
        /// 当前方案交通工具标准
        /// </summary>
        public List<T_OA_TAKETHESTANDARDTRANSPORT> transportToolStand = new List<T_OA_TAKETHESTANDARDTRANSPORT>();//
        //bool IsAudit = true;
        //private string Master_Golbal.BUSINESSTRIPID = string.Empty;
        //开始城市集合
        //private List<string> citysStartList_Golbal = new List<string>();
        //目标城市集合
        //private List<string> citysEndList_Golbal = new List<string>();
        private List<string> endTime = new List<string>();
        //private DateTimePicker txtEndTime = new DateTimePicker();
        //员工信息
        //private string strTravelEmployeeName = string.Empty;//出差人
        //public string Master_Golbal.POSTLEVEL = string.Empty;
        //public string Master_Golbal.OWNERDEPARTMENTNAME = string.Empty;//出差人的所属部门
        //public string Master_Golbal.OWNERPOSTNAME = string.Empty;//出差人的所属岗位
        //public string Master_Golbal.OWNERCOMPANYNAME = string.Empty;//出差人所属公司(初始化时用)
        private bool BtnNewButton = false;//单击新建按钮
        public bool needsubmit = false; //为真时可以进行提交
        public bool isSubmit = false;//为真时不作保存成功的提示
        //public string UserState = string.Empty;
        //private bool IsSubmit = false;//是否是点击了提交按钮
        //修改行程
        public bool isAlterTrave = false;

        public EntityBrowser ParentEntityBrowser { get; set; }

        private T_OA_BUSINESSTRIP Master_TravelBussiness;

        public T_OA_BUSINESSTRIP Master_Golbal
        {
            get { return Master_TravelBussiness; }
            set
            {
                this.DataContext = value;
                Master_TravelBussiness = value;
            }
        }
        #endregion

        #region 1构造
        /// <summary>
        /// 平台新建单据
        /// </summary>
        public TravelRequestForm()
        {
            InitializeComponent();
            this.formType = FormTypes.New;
            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                svdgEdit.Visibility = Visibility.Collapsed;
                svdgReadOnly.Visibility = Visibility.Visible;
            }
            else
            {
                svdgEdit.Visibility = Visibility.Visible;
                svdgReadOnly.Visibility = Visibility.Collapsed;
            }
            this.Loaded += new RoutedEventHandler(TravelapplicationPage_Loaded);
        }

        public TravelRequestForm(FormTypes formType, string BUSINESSTRIPID)
        {
            InitializeComponent();
            this.formType = formType;
            if (!string.IsNullOrEmpty(BUSINESSTRIPID))
            {
                Master_Golbal = new T_OA_BUSINESSTRIP();//传递出差id给Load事件
                Master_Golbal.BUSINESSTRIPID = BUSINESSTRIPID;
            }

            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                svdgEdit.Visibility = Visibility.Collapsed;
                svdgReadOnly.Visibility = Visibility.Visible;
                Utility.InitFileLoad(FormTypes.Browse, uploadFile, BUSINESSTRIPID, false);
            }
            else
            {
                svdgEdit.Visibility = Visibility.Visible;
                svdgReadOnly.Visibility = Visibility.Collapsed;                
            }
            this.Loaded += new RoutedEventHandler(TravelapplicationPage_Loaded);
        }

        #endregion      

        #region 2页面Load事件
        void TravelapplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (isPageloadCompleted == true) return;//如果已经加载过，再次切换时就不再加载

            InitWCFSvClinetEvent();
            GetVechileLevelInfos();
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            //entBrowser.BtnDelete.Click += btnDelete_Click;

            fbCtr.GetPayType.Visibility = Visibility.Visible;
            if (formType == FormTypes.Browse || formType == FormTypes.Audit)
            {
                FileLoadedCompleted();
                this.txtSubject.Foreground = new SolidColorBrush(Colors.Black);
                HideControl();
            }

            FormToolBar1.btnNew.Click += new RoutedEventHandler(btnNew_Click);

            FormToolBar1.btnEdit.Visibility = Visibility.Collapsed;//修改

            FormToolBar1.btnDelete.Visibility = Visibility.Collapsed;//删除
            //FormToolBar1.btnDelete.Click += btnDelete_Click;

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

            if (formType == FormTypes.New)
            {
                Master_Golbal = new T_OA_BUSINESSTRIP();
                Master_Golbal.BUSINESSTRIPID = System.Guid.NewGuid().ToString();
                Master_Golbal.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                Master_Golbal.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                Master_Golbal.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                Master_Golbal.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                Master_Golbal.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                Master_Golbal.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                Master_Golbal.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
                Master_Golbal.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                Master_Golbal.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
                Master_Golbal.POSTLEVEL = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
               
                Master_Golbal.TEL = Utility.GetEmployeePhone();
                txtTELL.Text = Utility.GetEmployeePhone();
               

                Master_Golbal.CREATEDATE = DateTime.Now;//创建时间
                Master_Golbal.UPDATEDATE = DateTime.Now;//更新时间
                Master_Golbal.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交
                Master_Golbal.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;//创建公司ID
                Master_Golbal.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;//创建部门ID
                Master_Golbal.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;//创建岗位ID
                Master_Golbal.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//创建人ID
                Master_Golbal.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;//创建人姓名
                Master_Golbal.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;//修改人ID
                Master_Golbal.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                Master_Golbal.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);//未提交

                string StrName = Master_Golbal.OWNERNAME + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
                txtTraveEmployee.Text = StrName;
                //strTravelEmployeeName = Master_Golbal.OWNERNAME;//修改、查看、审核时获取已保存在本地的出差人
                ToolTipService.SetToolTip(txtTraveEmployee, StrName);
                Utility.InitFileLoad("TravelRequest", Master_Golbal.BUSINESSTRIPID, formType, uploadFile);
                OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, null, null);

            }
            else
            {
                if (!string.IsNullOrEmpty(Master_Golbal.BUSINESSTRIPID))
                {
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    OaPersonOfficeClient.GetTravelmanagementByIdAsync(Master_Golbal.BUSINESSTRIPID);
                    Utility.InitFileLoad("TravelRequest", Master_Golbal.BUSINESSTRIPID, formType, uploadFile);
                }
            }
        }


        #region 删除出差申请

        //void btnDelete_Click(object sender, RoutedEventArgs e)
        //{
          
        //}

        void Travelmanagement_DeleteTravelmanagementCompleted(object sender, DeleteTravelmanagementCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
                else
                {
                    if (!e.Result) //返回值为假
                    {
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DALETEFAILED"));
                        return;
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("删除成功！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        this.formType = FormTypes.Browse;
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
            finally
            {
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                this.RefreshUI(RefreshedTypes.HideProgressBar);//读取完数据后，停止动画，隐藏                
                this.RefreshUI(RefreshedTypes.CloseAndReloadData);//重新加载数据
                this.RefreshUI(RefreshedTypes.All);
                //EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                //entBrowser.Close();
            }
        }
        #endregion

        #region InitWCFClientEvent
        private void InitWCFSvClinetEvent()
        {
            OaPersonOfficeClient = new SmtOAPersonOfficeClient();
            OaCommonOfficeClient = new SmtOACommonOfficeClient();
            //HrPersonnelclient = new PersonnelServiceClient();

            OaCommonOfficeClient.IsExistAgentCompleted += new EventHandler<IsExistAgentCompletedEventArgs>(SoaChannel_IsExistAgentCompleted);
            //HrPersonnelclient.GetAllEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetAllEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetAllEmployeePostBriefByEmployeeIDCompleted);
            //HrPersonnelclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(client_GetEmployeePostBriefByEmployeeIDCompleted);
            OaPersonOfficeClient.TravelmanagementAddCompleted += new EventHandler<TravelmanagementAddCompletedEventArgs>(Travelmanagement_TravelmanagementAddCompleted);//添加
            OaPersonOfficeClient.UpdateTravelmanagementCompleted += new EventHandler<UpdateTravelmanagementCompletedEventArgs>(Travelmanagement_UpdateTravelmanagementCompleted);//修改   
            OaPersonOfficeClient.GetTravelmanagementByIdCompleted += new EventHandler<GetTravelmanagementByIdCompletedEventArgs>(Travelmanagement_GetTravelmanagementByIdCompleted);
            //OaPersonOfficeClient.GetBusinesstripDetailCompleted += new EventHandler<GetBusinesstripDetailCompletedEventArgs>(Travelmanagement_GetBusinesstripDetailCompleted);
            OaPersonOfficeClient.GetTravelSolutionByCompanyIDCompleted += new EventHandler<GetTravelSolutionByCompanyIDCompletedEventArgs>(Travelmanagement_GetTravelSolutionByCompanyIDCompleted);
            //fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            //Travelmanagement.GetUnderwayTravelmanagementAsync("6ba49ec8-feb0-4f78-b801-2b8ea5387ab3");
            OaPersonOfficeClient.DeleteTravelmanagementCompleted += new EventHandler<DeleteTravelmanagementCompletedEventArgs>(Travelmanagement_DeleteTravelmanagementCompleted);
            
        }
        #endregion

        #region 获取交通工具的级别字典
        /// <summary>
        /// 获取交通工具的级别字典
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
        #endregion

        #endregion

        #region 3获取出差申请信息

        //void Travelmanagement_GetBusinesstripDetailCompleted(object sender, GetBusinesstripDetailCompletedEventArgs e)//出差申请明细
        //{
        //    isloaded = true;
        //    if (IsSubmit)
        //    {
        //        RefreshUI(RefreshedTypes.HideProgressBar);
        //    }
            
        //    try
        //    {
        //        if (e.Result != null)
        //        {
        //            BindDataGrid(e.Result);
        //        }
        //        else
        //        {
        //            BindDataGrid(null);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        isloaded = false;
        //        Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}

        void Travelmanagement_GetTravelmanagementByIdCompleted(object sender, GetTravelmanagementByIdCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        #region 设置数据已加载属性，绑定数据
                        isPageloadCompleted = true;
                        Master_Golbal = e.Result;
                        if (Master_Golbal.T_OA_BUSINESSTRIPDETAIL.Count > 0)
                        {
                            TraveDetailList_Golbal = Master_Golbal.T_OA_BUSINESSTRIPDETAIL;                         
                            //初始化上传控件
                            Utility.InitFileLoad("TravelRequest", Master_Golbal.BUSINESSTRIPID, formType, uploadFile);
                        }
                        #endregion

                        #region 设置显示数据的grid
                        if (formType == FormTypes.New 
                            || formType == FormTypes.Edit)
                        {
                            if (Master_Golbal.CHECKSTATE != (Convert.ToInt32(CheckStates.UnSubmit)).ToString())
                            {
                                formType = FormTypes.Audit;
                                svdgEdit.Visibility = Visibility.Collapsed;
                                svdgReadOnly.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                svdgEdit.Visibility = Visibility.Visible;
                                svdgReadOnly.Visibility = Visibility.Collapsed;
                            }
                        }
                        if (formType == FormTypes.Resubmit)
                        {
                            Master_Golbal.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                            svdgEdit.Visibility = Visibility.Visible;
                            svdgReadOnly.Visibility = Visibility.Collapsed;
                        }
                        if (Master_Golbal.CHECKSTATE == (Convert.ToInt32(CheckStates.UnSubmit)).ToString())
                        {
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.BtnDelete.Visibility = Visibility.Visible;
                            RefreshUI(RefreshedTypes.ToolBar);
                        }
                        #endregion

                        #region 获取出差人组织架构，岗位级别,联系电话

                        lookupTraveEmployee.DataContext = Master_Golbal;
                        txtTraveEmployee.Text = Master_Golbal.OWNERNAME;//出差人
                        if(!string.IsNullOrEmpty(Master_Golbal.TEL))
                        {
                            txtTELL.Text = Master_Golbal.TEL;
                        }
                        //Master_Golbal.POSTLEVEL = Master_Golbal.POSTLEVEL;
                        //strTravelEmployeeName = Master_Golbal.OWNERNAME;
                        ToolTipService.SetToolTip(txtTraveEmployee, Master_Golbal.OWNERNAME);
                        //Master_Golbal.OWNERPOSTNAME = Master_Golbal.OWNERPOSTNAME;
                        //Master_Golbal.OWNERDEPARTMENTNAME = Master_Golbal.OWNERDEPARTMENTNAME;
                        //Master_Golbal.OWNERCOMPANYNAME = Master_Golbal.OWNERCOMPANYNAME;
                        string StrName = Master_Golbal.OWNERNAME + "-" + Master_Golbal.OWNERPOSTNAME + "-" + Master_Golbal.OWNERDEPARTMENTNAME + "-" + Master_Golbal.OWNERCOMPANYNAME;
                        txtTraveEmployee.Text = StrName;
                        this.txtSubject.Text = Master_Golbal.CONTENT;//出差事由
                        //启用代理
                        if (Master_Golbal.ISAGENT == "1")
                        {
                            this.ckEnabled.IsChecked = true;
                            int i = TraveDetailList_Golbal.Count() - 1;
                            AddAgent(i);
                        }

                        #endregion

                        #region 刷新界面
                        RefreshUI(RefreshedTypes.ToolBar);
                        if (Master_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                        {
                            RefreshUI(RefreshedTypes.AuditInfo);
                        }
                        #endregion

                        #region 获取出差方案，工具标准用来报销，及显示控件颜色
                        //if (formType == FormTypes.Resubmit || formType == FormTypes.New || formType == FormTypes.Edit)
                        //{
                            //重新提交获取出差方案
                            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
                        //}
                        //else
                        //{
                        //    if (formType != FormTypes.New && Master_Golbal.T_OA_BUSINESSTRIPDETAIL.Count > 0)
                        //    {
                        //        BindDataGrid(Master_Golbal.T_OA_BUSINESSTRIPDETAIL);
                        //    }
                        //    RefreshUI(RefreshedTypes.HideProgressBar);
                        //    if (Master_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                        //    {
                        //        HideControl();
                        //    }
                        //}
                        #endregion
                    }
                    else
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        BindDataGrid(null);
                        return;
                    }
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
        }

        #endregion

        #region 4获取出差人方案已判断标准

        /// <summary>
        /// 根据当前用户的岗位级别与方案设置的岗位级别匹配，确认该出差人是否能够申请借款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Travelmanagement_GetTravelSolutionByCompanyIDCompleted(object sender, GetTravelSolutionByCompanyIDCompletedEventArgs e)
        {
            try
            {
                travelsolutions_Golbal = new T_OA_TRAVELSOLUTIONS();
                if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(e.Error.Message), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                }
                if (e.Result != null)
                {
                    travelsolutions_Golbal = e.Result;//出差方案
                }
                if (Master_Golbal.POSTLEVEL.ToInt32() <= travelsolutions_Golbal.RANGEPOSTLEVEL.ToInt32())
                {
                    fbCtr.IsEnabled = false;//如果当前用户的级别与方案设置的"报销范围级别"相同则不能申请费用
                }
                if (e.PlaneObj != null)
                {
                    cantaketheplaneline = e.PlaneObj.ToList();//乘坐飞机线路设置
                }
                if (e.StandardObj != null)
                {
                    //交通工具乘坐标准
                    transportToolStand = e.StandardObj.ToList();//乘坐交通工具标准设置
                }
                if (formType != FormTypes.New && Master_Golbal.T_OA_BUSINESSTRIPDETAIL.Count > 0)
                {
                    BindDataGrid(Master_Golbal.T_OA_BUSINESSTRIPDETAIL);
                }
                else
                {
                    RefreshUI(RefreshedTypes.All);
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

        #region 5新建出差申请单默认的2条出差申请记录
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (DaGrs.ItemsSource == null)
            {
                TraveDetailList_Golbal = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
                T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
                buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                buip.STARTDATE = new DateTime(DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.AddDays(1).Day,8,30,0);
                buip.ENDDATE = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(1).Day, 17, 30, 0); 
                
                TraveDetailList_Golbal.Add(buip);

                T_OA_BUSINESSTRIPDETAIL buipd = new T_OA_BUSINESSTRIPDETAIL();
                buipd.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                buipd.STARTDATE = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(2).Day, 8, 30, 0);
                buipd.ENDDATE = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(2).Day, 17, 30, 0); 
                TraveDetailList_Golbal.Add(buipd);

                DaGrs.ItemsSource = TraveDetailList_Golbal;
                DaGrs.SelectedIndex = 0;
            }
            //if (!string.IsNullOrEmpty(Master_Golbal.BUSINESSTRIPID))
            //{
            //    //ctrFile.Load_fileData(Master_Golbal.BUSINESSTRIPID);
            //}
        }
        #endregion

        #region 6DaGrs_LoadingRow
        /// <summary>
        /// 默认显示出差时间、地点数据的DataGrid
        /// </summary>
        Brush tempcomTypeBorderBrush;//定义combox默认颜色变量
        Brush tempcomTypeForeBrush;
        Brush tempcomLevelBorderBrush;
        Brush tempcomLevelForeBrush;
        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                T_OA_BUSINESSTRIPDETAIL tmp = (T_OA_BUSINESSTRIPDETAIL)e.Row.DataContext;
                ImageButton MyButton_Delbaodao = DaGrs.Columns[10].GetCellContent(e.Row).FindName("myDelete") as ImageButton;
                SearchCity myCity = DaGrs.Columns[1].GetCellContent(e.Row).FindName("txtDEPARTURECITY") as SearchCity;
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(e.Row).FindName("txtTARGETCITIES") as SearchCity;
                CheckBox IsCheck = DaGrs.Columns[7].GetCellContent(e.Row).FindName("myChkBox") as CheckBox;
                CheckBox IsCheckMeet = DaGrs.Columns[8].GetCellContent(e.Row).FindName("myChkBoxMeet") as CheckBox;
                CheckBox IsCheckCar = DaGrs.Columns[9].GetCellContent(e.Row).FindName("myChkBoxCar") as CheckBox;
                TravelDictionaryComboBox ComVechile = DaGrs.Columns[4].GetCellContent(e.Row).FindName("ComVechileType") as TravelDictionaryComboBox;
                TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(e.Row).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                if (BtnNewButton == true)
                {
                    myCitys.TxtSelectedCity.Text = string.Empty;
                    //citysStartList_Golbal.Add(tmp.DEPCITY);
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
                ComVechile.SelectedIndex = 0;
                ComLevel.SelectedIndex = 0;
                //对默认颜色进行赋值
                tempcomTypeBorderBrush = ComVechile.BorderBrush;
                tempcomTypeForeBrush = ComVechile.Foreground;
                tempcomLevelBorderBrush = ComLevel.BorderBrush;
                tempcomLevelForeBrush = ComLevel.Foreground;

                ObservableCollection<T_OA_BUSINESSTRIPDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
                if (formType != FormTypes.New)
                {
                    int j = 0;
                    if (DaGrs.ItemsSource != null && TraveDetailList_Golbal != null)
                    {
                        foreach (var obje in objs)
                        {
                            j++;
                            if (obje.BUSINESSTRIPDETAILID == tmp.BUSINESSTRIPDETAILID)//判断记录的ID是否相同
                            {
                                if (myCity != null)//出发城市
                                {
                                    if (obje.DEPCITY != null)
                                    {
                                        myCity.TxtSelectedCity.Text = GetCityName(obje.DEPCITY);
                                        if (TraveDetailList_Golbal.Count() > 1)
                                        {
                                            if (j > 1)
                                            {
                                                //By  : Sam
                                                //Date: 2011-9-6
                                                //For : 此处之前设置双击的时候还会出现打开状态
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
                            }
                            else
                            {
                                continue;
                            }

                            string dictid = "";
                            //获取交通工具类型、级别
                            if (ComVechile != null)
                            {
                                T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                                var thd = transportToolStand.FirstOrDefault();
                                if (type != null)
                                {
                                    thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                    foreach (T_SYS_DICTIONARY Region in ComVechile.Items)
                                    {
                                        if (thd != null)
                                        {
                                            dictid = Region.DICTIONARYID;
                                            if (Region.DICTIONARYVALUE.ToString() == tmp.TYPEOFTRAVELTOOLS)
                                            {
                                                if (transportToolStand.Count() > 0)
                                                {
                                                    ComVechile.SelectedItem = Region;
                                                    if (thd.TYPEOFTRAVELTOOLS.ToInt32() > Region.DICTIONARYVALUE)
                                                    {
                                                        ComVechile.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComVechile.Foreground = new SolidColorBrush(Colors.Red);
                                                        ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                        ComLevel.Foreground = new SolidColorBrush(Colors.Red);
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
                            }

                            if (ComLevel != null)
                            {
                                var ents = from ent in ListVechileLevel
                                           where ent.T_SYS_DICTIONARY2.DICTIONARYID == dictid
                                           select ent;
                                ComLevel.ItemsSource = ents.ToList();
                                if (ents.Count() > 0)
                                {
                                    T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                                    T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                                    type = ComVechile.SelectedItem as T_SYS_DICTIONARY;
                                    level = ComLevel.SelectedItem as T_SYS_DICTIONARY;

                                    var thd = transportToolStand.FirstOrDefault();
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
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("出差申请DaGrs_LoadingRow异常：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
               
            }
        }
        #endregion

    }
}
