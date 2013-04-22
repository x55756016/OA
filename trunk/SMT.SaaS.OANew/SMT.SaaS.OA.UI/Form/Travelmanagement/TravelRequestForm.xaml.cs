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
        private PersonnelServiceClient HrPersonnelclient;
        private SmtOACommonOfficeClient OaCommonOfficeClient;
       
        private FormTypes formType;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;

        private T_OA_AGENTDATESET AgentDateSet;
        private ObservableCollection<T_OA_BUSINESSTRIPDETAIL> TraveDetailList_Golbal = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
        private T_OA_BUSINESSTRIPDETAIL TraveDetailOne_Golbal = new T_OA_BUSINESSTRIPDETAIL();
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
        private List<string> citysStartList_Golbal = new List<string>();
        //目标城市集合
        private List<string> citysEndList_Golbal = new List<string>();
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

        #region 构造
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

        public TravelRequestForm(FormTypes action, string BUSINESSTRIPID)
        {
            InitializeComponent();
            this.formType = action;
            if (!string.IsNullOrEmpty(BUSINESSTRIPID))
            {
                Master_Golbal = new T_OA_BUSINESSTRIP();
                Master_Golbal.BUSINESSTRIPID = BUSINESSTRIPID;
            }
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

        #region InitEvent

        private void InitWCFSvClinetEvent()
        {
            OaPersonOfficeClient = new SmtOAPersonOfficeClient();
            OaCommonOfficeClient = new SmtOACommonOfficeClient();
            HrPersonnelclient = new PersonnelServiceClient();

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

        }


        #endregion

        #endregion      

        #region 页面事件

        #region 页面Load事件
        void TravelapplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (isPageloadCompleted == true) return;//如果已经加载过，再次切换时就不再加载

            InitWCFSvClinetEvent();
            GetVechileLevelInfos();
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);

            fbCtr.GetPayType.Visibility = Visibility.Visible;
            if (formType == FormTypes.Browse || formType == FormTypes.Audit)
            {
                FileLoadedCompleted();
                this.txtSubject.Foreground = new SolidColorBrush(Colors.Black);
                HideControl();
            }

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
                Master_Golbal.TEL = Common.CurrentLoginUserInfo.Telphone;
                txtTELL.Text = Common.CurrentLoginUserInfo.Telphone;
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
        #endregion


        #region 屏蔽控件
        private void HideControl()
        {
            lookupTraveEmployee.IsEnabled = false;
            txtSubject.IsReadOnly = true;
            txtTELL.IsReadOnly = true;
            ckEnabled.IsEnabled = false;

            svdgEdit.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            DaGrs.IsEnabled = false;
            DaGrs.Columns[9].Visibility = Visibility.Collapsed;
            svdgEdit.IsEnabled = true;
            fbCtr.IsEnabled = false;
            FormToolBar1.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region 新建按钮事件
        void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckIsFinishedCitys())
            {
                return;
            }
            BtnNewButton = true;

            T_OA_BUSINESSTRIPDETAIL NewBussnessTripDetail = new T_OA_BUSINESSTRIPDETAIL();
            NewBussnessTripDetail.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();

            if (TraveDetailList_Golbal.Count() > 0)
            {
                //新的赋值方式
                NewBussnessTripDetail.STARTDATE = TraveDetailList_Golbal[TraveDetailList_Golbal.Count() - 1].ENDDATE;
                NewBussnessTripDetail.DEPCITY = citysEndList_Golbal[TraveDetailList_Golbal.Count() - 1];
                NewBussnessTripDetail.ENDDATE = DateTime.Now;
                //citysStartList_Golbal.Add(citysEndList_Golbal[TraveDetailList_Golbal.Count() - 1]);绑定后会增加一条
                TraveDetailList_Golbal.Add(NewBussnessTripDetail);
                DaGrs.ItemsSource = TraveDetailList_Golbal;
                //禁用此记录开始城市选择控件
                SearchCity myCity = DaGrs.Columns[1].GetCellContent(NewBussnessTripDetail).FindName("txtDEPARTURECITY") as SearchCity;
                if (myCity != null)
                {
                    myCity.TxtSelectedCity.Text = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(citysStartList_Golbal[TraveDetailList_Golbal.Count() - 1]);
                    myCity.IsEnabled = false;
                    ((DataGridCell)((StackPanel)myCity.Parent).Parent).IsEnabled = false;
                }
                //旧的赋值方式
            }
        }

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
                SearchCity myCitys = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;

                TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;
                DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;

                TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                SearchCity DepartCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                SearchCity TargetCity = DaGrs.Columns[3].GetCellContent(obje).FindName("txtTARGETCITIES") as SearchCity;
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

                if (string.IsNullOrEmpty(StrEndTime))//结束日期不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "结束时间的年月日或时分不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(DepartCity.TxtSelectedCity.Text))//出发城市不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    IsResult = false;
                }

                if (string.IsNullOrEmpty(TargetCity.TxtSelectedCity.Text))//到达城市不能为空
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "到达城市不能为空", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
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

            try
            {
                TraveDetailList_Golbal = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
                if (TraveDetailList_Golbal.Count() > 1)
                {
                    int selectGridRowIndex = DaGrs.SelectedIndex;
                    T_OA_BUSINESSTRIPDETAIL entDel = DaGrs.SelectedItems[selectGridRowIndex] as T_OA_BUSINESSTRIPDETAIL;
                    if (TraveDetailList_Golbal.Contains(entDel))
                    {
                        TraveDetailList_Golbal.Remove(entDel);
                        if (citysEndList_Golbal.Count >= selectGridRowIndex)
                        {
                            citysEndList_Golbal.RemoveAt(selectGridRowIndex - 1);
                        }
                        if (citysStartList_Golbal.Count >= selectGridRowIndex)
                        {
                            citysStartList_Golbal.RemoveAt(selectGridRowIndex - 1);
                        }
                    }
                    //如果选中的不是第一条也不是最后一行，那么修改选中行的下一行的开始城市
                    if (1 < selectGridRowIndex && selectGridRowIndex < TraveDetailList_Golbal.Count - 1)
                    {
                        Object obje = DaGrs.SelectedItems[selectGridRowIndex + 1];
                        SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citysEndList_Golbal[selectGridRowIndex - 1]);
                        citysStartList_Golbal[selectGridRowIndex + 1] = citysEndList_Golbal[selectGridRowIndex - 1];//上一城市的城市值
                    }
                    DaGrs.ItemsSource = TraveDetailList_Golbal;
                    //for (int i = 0; i < DaGrs.SelectedItems.Count; i++)
                    //{
                    //    int k = DaGrs.SelectedIndex;//当前选中行
                    //    T_OA_BUSINESSTRIPDETAIL entDel = DaGrs.SelectedItems[i] as T_OA_BUSINESSTRIPDETAIL;

                    //    if (TraveDetailList_Golbal.Contains(entDel))
                    //    {

                    //        TraveDetailList_Golbal.Remove(entDel);
                    //        if (citysEndList_Golbal.Count > k)
                    //        {

                    //            int EachCount = 0;
                    //            foreach (Object obje in DaGrs.ItemsSource)//将下一个出发城市的值修改
                    //            {
                    //                EachCount++;
                    //                if (DaGrs.Columns[1].GetCellContent(obje) != null)
                    //                {
                    //                    SearchCity mystarteachCity = DaGrs.Columns[1].GetCellContent(obje).FindName("txtDEPARTURECITY") as SearchCity;
                    //                    if ((k + 1) == EachCount)
                    //                    {
                    //                        if (k > 0)
                    //                        {
                    //                            mystarteachCity.TxtSelectedCity.Text = GetCityName(citysEndList_Golbal[k - 1]);
                    //                            citysStartList_Golbal[k + 1] = citysEndList_Golbal[k - 1];//上一城市的城市值
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //            citysEndList_Golbal.RemoveAt(k);//清除目标城市的值
                    //            citysStartList_Golbal.RemoveAt(k);//清除出发城市的值
                    //        }
                    //        //buipList[k].PRIVATEAFFAIR = "0";//清除私事的勾选
                    //        //buipList[k].GOOUTTOMEET = "0";//清除内部开会\培训的勾选
                    //        //buipList[k].COMPANYCAR = "0";//清除公司派车的勾选
                    //    }
                    //}
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "必须保留一条出差时间及地点!", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
            }
        }
        #endregion

        #region LayoutRoot_Loaded
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (DaGrs.ItemsSource == null)
            {
                TraveDetailList_Golbal = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
                T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
                buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                buip.STARTDATE = DateTime.Now;
                buip.ENDDATE = DateTime.Now;
                TraveDetailList_Golbal.Add(buip);

                T_OA_BUSINESSTRIPDETAIL buipd = new T_OA_BUSINESSTRIPDETAIL();
                buipd.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
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

        #region 选择出差人LookUP
        private void lookupTraveEmployee_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    //Master_Golbal.OWNERPOSTNAME = post.ObjectName;//岗位
                    Master_Golbal.POSTLEVEL = (ent.FirstOrDefault().ObjectInstance 
                        as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID 
                            == postid).FirstOrDefault().POSTLEVEL.ToString();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    Master_Golbal.OWNERDEPARTMENTNAME = dept.ObjectName;//部门

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string selectCompanyId = corp.COMPANYID;
                    Master_Golbal.OWNERCOMPANYNAME = corp.CNAME;//公司

                    string StrEmployee = userInfo.ObjectName + "[" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME + "]";
                    txtTraveEmployee.Text = StrEmployee;//出差人
                    //strTravelEmployeeName = userInfo.ObjectName;
                    ToolTipService.SetToolTip(txtTraveEmployee, StrEmployee);
                    Master_Golbal.OWNERID = userInfo.ObjectID;//出差人ID
                    Master_Golbal.OWNERNAME = userInfo.ObjectName;//出差人
                    Master_Golbal.OWNERCOMPANYID = selectCompanyId;
                    Master_Golbal.OWNERDEPARTMENTID = deptid;
                    Master_Golbal.OWNERPOSTID = postid;

                    Master_Golbal.OWNERPOSTNAME=Master_Golbal.OWNERPOSTNAME;
                    Master_Golbal.OWNERDEPARTMENTNAME=Master_Golbal.OWNERDEPARTMENTNAME;
                    Master_Golbal.OWNERCOMPANYNAME=Master_Golbal.OWNERCOMPANYNAME;

                    fbCtr.Order.OWNERCOMPANYID = selectCompanyId;
                    fbCtr.Order.OWNERCOMPANYNAME = Master_Golbal.OWNERCOMPANYNAME;
                    fbCtr.Order.OWNERDEPARTMENTID = deptid;
                    fbCtr.Order.OWNERDEPARTMENTNAME = Master_Golbal.OWNERDEPARTMENTNAME;
                    fbCtr.Order.OWNERPOSTID = postid;
                    fbCtr.Order.OWNERPOSTNAME = post.ObjectName;
                    fbCtr.Order.OWNERID = userInfo.ObjectID;
                    fbCtr.Order.OWNERNAME = userInfo.ObjectName;
                    fbCtr.RefreshData();

                    if (!string.IsNullOrEmpty(selectCompanyId))//如果是选出差人的情况下(获取所选用户公司)
                    {
                        OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
                    }
                    else //默认是当前用户(当前用户公司)
                    {
                        MessageBox.Show("请选择出差员工");
                    }
                    
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region DaGrs_LoadingRow
        /// <summary>
        /// 默认显示出差时间、地点数据的DataGrid
        /// </summary>
        Brush tempcomTypeBorderBrush;//定义combox默认颜色变量
        Brush tempcomTypeForeBrush;
        Brush tempcomLevelBorderBrush;
        Brush tempcomLevelForeBrush;
        private void DaGrs_LoadingRow(object sender, DataGridRowEventArgs e)
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
                citysStartList_Golbal.Add(tmp.DEPCITY);
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
        #endregion


        #region DataGrid 数据加载、字典数据转换
        private void BindDataGrid(ObservableCollection<T_OA_BUSINESSTRIPDETAIL> obj)
        {
            if (obj == null) return;
            citysStartList_Golbal.Clear();
            citysEndList_Golbal.Clear();
            foreach (T_OA_BUSINESSTRIPDETAIL detail in obj)
            {
                citysStartList_Golbal.Add(detail.DEPCITY);
                citysEndList_Golbal.Add(detail.DESTCITY);
            }
            if (formType != FormTypes.New && formType != FormTypes.Edit && formType != FormTypes.Resubmit)
            {
                DaGridReadOnly.ItemsSource = obj;
            }
            else
            {
                DaGrs.ItemsSource = obj;
            }
        }
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
        /// 添加子表数据
        /// </summary>
        private void NewDetail()
        {
            try
            {

                ObservableCollection<T_OA_BUSINESSTRIPDETAIL> ListDetail = new ObservableCollection<T_OA_BUSINESSTRIPDETAIL>();
                string StrStartDt = "";   //开始时间
                string StrStartTime = ""; //开始时：分
                string EndDt = "";    //结束时间
                string StrEndTime = ""; //结束时：分
                int i = 0;
                if (DaGrs.ItemsSource != null)
                {
                    foreach (Object obje in DaGrs.ItemsSource)//判断所选的出发城市是否与目标城市相同
                    {
                        TraveDetailOne_Golbal = new T_OA_BUSINESSTRIPDETAIL();
                        TraveDetailOne_Golbal.BUSINESSTRIPDETAILID = (obje as T_OA_BUSINESSTRIPDETAIL).BUSINESSTRIPDETAILID;
                        TraveDetailOne_Golbal.T_OA_BUSINESSTRIP = Master_Golbal;
                        DateTimePicker StartDate = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                        DateTimePicker EndDate = DaGrs.Columns[2].GetCellContent(obje).FindName("EndTime") as DateTimePicker;
                        TextBox datys = ((TextBox)((StackPanel)DaGrs.Columns[6].GetCellContent(obje)).Children.FirstOrDefault()) as TextBox;
                        CheckBox IsCheck = DaGrs.Columns[7].GetCellContent(obje).FindName("myChkBox") as CheckBox;
                        CheckBox IsCheckMeet = DaGrs.Columns[8].GetCellContent(obje).FindName("myChkBoxMeet") as CheckBox;
                        CheckBox IsCheckCar = DaGrs.Columns[9].GetCellContent(obje).FindName("myChkBoxCar") as CheckBox;
                        TravelDictionaryComboBox ToolType = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[4].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;
                        TravelDictionaryComboBox ToolLevel = ((TravelDictionaryComboBox)((StackPanel)DaGrs.Columns[5].GetCellContent(obje)).Children.FirstOrDefault()) as TravelDictionaryComboBox;

                        if (StartDate.Value != null)
                        {
                            StrStartDt = StartDate.Value.Value.ToString("d");//开始日期
                            StrStartTime = StartDate.Value.Value.ToString("HH:mm");//开始时间
                            if (EndDate.Value != null)
                            {
                                EndDt = EndDate.Value.Value.ToString("d");//结束日期
                                StrEndTime = EndDate.Value.Value.ToString("HH:mm");//结束时间
                            }
                        }

                        DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                        DateTime DtEnd = System.Convert.ToDateTime(EndDt + " " + StrEndTime);

                        if (citysStartList_Golbal != null)//出发城市
                        {
                            TraveDetailOne_Golbal.DEPCITY = citysStartList_Golbal[i].Replace(",", "");
                        }
                        if (citysEndList_Golbal != null)//目标城市
                        {
                            TraveDetailOne_Golbal.DESTCITY = citysEndList_Golbal[i].Replace(",", "");
                        }
                        if (DtStart != null)
                        {
                            TraveDetailOne_Golbal.STARTDATE = DtStart;
                        }
                        if (datys != null)//出差天数
                        {
                            TraveDetailOne_Golbal.BUSINESSDAYS = datys.Text;
                        }
                        if (DtEnd != null)
                        {
                            TraveDetailOne_Golbal.ENDDATE = DtEnd;
                        }
                        if (IsCheck != null)//是否是私事
                        {
                            TraveDetailOne_Golbal.PRIVATEAFFAIR = (bool)IsCheck.IsChecked ? "1" : "0";
                        }
                        if (IsCheckMeet != null)//是否是开会
                        {
                            TraveDetailOne_Golbal.GOOUTTOMEET = (bool)IsCheckMeet.IsChecked ? "1" : "0";
                        }
                        if (IsCheckCar != null)//公司派车
                        {
                            TraveDetailOne_Golbal.COMPANYCAR = (bool)IsCheckCar.IsChecked ? "1" : "0";
                        }
                        if (ToolType != null)//乘坐交通工具类型
                        {
                            T_SYS_DICTIONARY ComVechileObj = ToolType.SelectedItem as T_SYS_DICTIONARY;
                            if (ComVechileObj != null)
                                TraveDetailOne_Golbal.TYPEOFTRAVELTOOLS = ComVechileObj.DICTIONARYVALUE.ToString();
                        }
                        if (ToolLevel != null)//乘坐交通工具级别
                        {
                            T_SYS_DICTIONARY ComLevelObj = ToolLevel.SelectedItem as T_SYS_DICTIONARY;
                            if (ComLevelObj != null)
                                TraveDetailOne_Golbal.TAKETHETOOLLEVEL = ComLevelObj.DICTIONARYVALUE.ToString();
                        }
                        ListDetail.Add(TraveDetailOne_Golbal);
                        i++;
                    }
                    TraveDetailList_Golbal = ListDetail;
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

        #region 获取出差申请明细
     
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

                        #region 判断是否需要修改，并判断是否获取出差方案
                        if (formType == FormTypes.Resubmit || formType == FormTypes.New || formType == FormTypes.Edit)
                        {
                            //重新提交获取出差方案
                            OaPersonOfficeClient.GetTravelSolutionByCompanyIDAsync(Master_Golbal.OWNERCOMPANYID, null, null);
                        }
                        else
                        {
                            RefreshUI(RefreshedTypes.HideProgressBar);
                            if (Master_Golbal.CHECKSTATE != ((int)CheckStates.UnSubmit).ToString())
                            {
                                HideControl();
                            }
                        }
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


        #region 出差申请Grid中控件事件

        #region 出发城市 目标城市 lookup选择
        /// <summary>
        /// 出发城市lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtDEPARTURECITY_SelectClick(object sender, EventArgs e)
        {
            SearchCity senderCity = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                string selectCityName = SelectCity.Result.Keys.FirstOrDefault();
                string selectCityValue = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                selectCityValue = selectCityValue.Replace(',', ' ').Trim();

                try
                { 
                    int selectGridRowIndex = DaGrs.SelectedIndex;


                    if (selectGridRowIndex > 0 && citysEndList_Golbal.Count >= selectGridRowIndex+1)
                    {
                        if (selectCityValue == citysEndList_Golbal[selectGridRowIndex])
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }
                    for (int i = 0; i < citysStartList_Golbal.Count; i++)
                    {
                        if (citysStartList_Golbal.Count > 1)
                        {
                            //如果上下两条出差记录城市一样
                            if (i < citysStartList_Golbal.Count - 1 && citysStartList_Golbal[i] == citysStartList_Golbal[i + 1])
                            {
                                //出发城市与开始城市不能相同
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), "出发城市重复！", Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                                return;
                            }
                        }
                    }

                    senderCity.TxtSelectedCity.Text = selectCityName;
                    if (citysStartList_Golbal.Count >= selectGridRowIndex+1)
                    {
                        citysStartList_Golbal[selectGridRowIndex] = selectCityValue;
                    }
                    else
                    {
                        citysStartList_Golbal.Add(selectCityValue);
                    }
                  
                    
                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                }
                //if (DaGrs.SelectedItem != null)
                //{
                //    int selectGridRowIndex = DaGrs.SelectedIndex;
                //    if (DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem) != null)
                //    {
                //        //T_OA_BUSINESSTRIPDETAIL travDetaillist = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                //        SearchCity thisEndCity = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;//出发城市
                //        SearchCity thisStartCity = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;//目标城市
                //        //int k = citysStartList.IndexOf(travDetaillist.DEPCITY);

                //        if (citysStartList.Count() >= selectGridRowIndex + 1)
                //        {
                //            citysStartList[selectGridRowIndex] = selectCityValue;
                //        }
                //        else
                //        {
                //            citysStartList.Add(selectCityValue);
                //        }
                //        if (citysStartList.Count > 1)
                //        {
                //            if (thisEndCity.TxtSelectedCity.Text.ToString().Trim() == thisStartCity.TxtSelectedCity.Text.ToString().Trim())
                //            {
                //                thisEndCity.TxtSelectedCity.Text = string.Empty;
                //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //                return;
                //            }
                //        }
                //    }
                //    else
                //    {

                //    }
                //}
                //if (citysStartList.Last().Split(',').Count() > 2)
                //{
                //    senderCity.TxtSelectedCity.Text = string.Empty;
                //    citysStartList.RemoveAt(citysStartList.Count - 1);
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "DEPARTURECITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
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
        /// 到达城市lookup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtTARGETCITIES_SelectClick(object sender, EventArgs e)
        {
            SearchCity serchCitySender = (SearchCity)sender;
            AreaSortCity SelectCity = new AreaSortCity();

            SelectCity.SelectedClicked += (obj, ea) =>
            {
                string selectCityName = SelectCity.Result.Keys.FirstOrDefault();
                string selectCityValue = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                selectCityValue = selectCityValue.Replace(',', ' ').Trim();

                try
                {
                    int selectGridRowIndex = DaGrs.SelectedIndex;

                    if (selectGridRowIndex >= 0 && citysStartList_Golbal.Count >= selectGridRowIndex+1)
                    {
                        if (selectCityValue == citysStartList_Golbal[selectGridRowIndex])
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                            return;
                        }
                    }

                    if (citysEndList_Golbal.Count >= selectGridRowIndex+1)
                    {
                        citysEndList_Golbal[selectGridRowIndex] = selectCityValue;
                    }
                    else
                    {
                        citysEndList_Golbal.Add(selectCityValue);
                    }
                    serchCitySender.TxtSelectedCity.Text = selectCityName;
                    //设置下一行的起始城市 如果为最后一行数据，且grid有下一行，增加下一行
                    if (citysStartList_Golbal.Count == (selectGridRowIndex + 1))
                     {
                         ObservableCollection<T_OA_BUSINESSTRIPDETAIL> objs = DaGrs.ItemsSource as ObservableCollection<T_OA_BUSINESSTRIPDETAIL>;
                         if (objs.Count > selectGridRowIndex + 1)
                         {
                             citysStartList_Golbal.Add(selectCityValue);
                             SetNextDepartureCity(selectGridRowIndex);
                         }
                     }
                     else
                     {
                         //citysStartList[selectGridRowIndex] = thisSelectStartCity.Tag.ToString();
                         citysStartList_Golbal[selectGridRowIndex + 1] = selectCityValue;//出发城市中下一条记录
                         SetNextDepartureCity(selectGridRowIndex);
                     }
                }
                catch (Exception ex)
                {
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                    SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                }
                //serchCitySender.TxtSelectedCity.Text = SelectCity.Result.Keys.FirstOrDefault();
                //if (DaGrs.SelectedItem != null)
                //{
                //    int selectGridRowIndex = DaGrs.SelectedIndex;//选择的行数，选择的行数也就是目的城市的位置
                //    if (DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem) != null)
                //    {
                //        //T_OA_BUSINESSTRIPDETAIL travDetaillist = DaGrs.SelectedItem as T_OA_BUSINESSTRIPDETAIL;
                //        SearchCity thisSelectEndCity = DaGrs.Columns[3].GetCellContent(DaGrs.SelectedItem).FindName("txtTARGETCITIES") as SearchCity;
                //        SearchCity thisSelectStartCity = DaGrs.Columns[1].GetCellContent(DaGrs.SelectedItem).FindName("txtDEPARTURECITY") as SearchCity;
                       
                //        if (citysEndList.Count > 1)
                //        {
                //            if (thisSelectStartCity.TxtSelectedCity.Text.ToString().Trim() == thisSelectEndCity.TxtSelectedCity.Text.ToString().Trim())
                //            {
                //                thisSelectEndCity.TxtSelectedCity.Text = string.Empty;
                //                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("出发城市和目标城市不能相同"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //                return;
                //            }
                //        }
                //        if (citysEndList.Count >= (selectGridRowIndex + 1))
                //        {
                //            citysEndList[selectGridRowIndex] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();
                //        }
                //        else
                //        {
                //            citysEndList.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                //        }
                //        //设置下一行的起始城市
                //        if (citysStartList.Count == (selectGridRowIndex + 1))
                //        {
                //            citysStartList.Add(SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString());
                //            SetNextDepartureCity(selectGridRowIndex);
                //        }
                //        else
                //        {
                //            citysStartList[selectGridRowIndex] = thisSelectStartCity.Tag.ToString();
                //            citysStartList[selectGridRowIndex + 1] = SelectCity.Result[SelectCity.Result.Keys.FirstOrDefault()].ToString();//出发城市中下一条记录
                //            SetNextDepartureCity(selectGridRowIndex);
                //        }
                //    }
                //}
                //if (citysEndList.Last().Split(',').Count() > 2)
                //{
                //    serchCitySender.TxtSelectedCity.Text = string.Empty;
                //    citysEndList.RemoveAt(citysEndList.Count - 1);
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("CANONLYCHOOSEONE", "ARRIVALCITY"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                //    return;
                //}
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
                    //DateTimePicker myDaysTime = DaGrs.Columns[0].GetCellContent(obje).FindName("StartTime") as DateTimePicker;
                    if ((SelectIndex + 2) == EachCount)
                    {
                        mystarteachCity.TxtSelectedCity.Text = GetCityName(citysStartList_Golbal[SelectIndex + 1]);
                        //myDaysTime.Value = Convert.ToDateTime(endTime[SelectIndex+1]);
                    }
                }
            }
        }
        #endregion

        #region 私事myChkBox_Checked事件
        private void myChkBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].PRIVATEAFFAIR = "1";
                    }
                }
            }
        }

        private void myChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].PRIVATEAFFAIR = "0";
                    }
                }
            }
        }
        #endregion

        #region 交通工具类型、级别控件
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
            TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
            if (vechiletype.SelectedIndex >= 0)
            {
                var thd = transportToolStand.FirstOrDefault();
                thd = this.GetVehicleTypeValue("");
                T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;
                if (DaGrs.SelectedItem != null)
                {
                    if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                    {
                        TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;

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
                //if (employeepost != null)
                //{
                if (!string.IsNullOrEmpty(Master_Golbal.POSTLEVEL))
                {
                    if (DaGrs.SelectedItem != null)
                    {
                        if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                            TravelDictionaryComboBox ComType = DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;
                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                            level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                            type = ComType.SelectedItem as T_SYS_DICTIONARY;

                            if (transportToolStand.Count() > 0)
                            {
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
                                        }
                                        else
                                        {
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE && thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                                return;
                                            }
                                            if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                            {
                                                ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                ComLevel.BorderBrush = new SolidColorBrush(Colors.Red);
                                                ComLevel.Foreground = new SolidColorBrush(Colors.Red);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //}
            }
        }


        private void ComVechileTypeLeve_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TravelDictionaryComboBox vechiletype = sender as TravelDictionaryComboBox;
            if (vechiletype.SelectedIndex >= 0)
            {
                //if (employeepost != null)
                //{
                if (!string.IsNullOrEmpty(Master_Golbal.POSTLEVEL))
                {
                    if (DaGrs.SelectedItem != null)
                    {
                        var thd = transportToolStand.FirstOrDefault();

                        T_SYS_DICTIONARY VechileTypeObj = vechiletype.SelectedItem as T_SYS_DICTIONARY;

                        if (DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem) != null)
                        {
                            TravelDictionaryComboBox ComLevel = DaGrs.Columns[5].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileTypeLeve") as TravelDictionaryComboBox;
                            TravelDictionaryComboBox ComType = DaGrs.Columns[4].GetCellContent(DaGrs.SelectedItem).FindName("ComVechileType") as TravelDictionaryComboBox;

                            T_SYS_DICTIONARY type = new T_SYS_DICTIONARY();
                            T_SYS_DICTIONARY level = new T_SYS_DICTIONARY();
                            level = ComLevel.SelectedItem as T_SYS_DICTIONARY;
                            type = ComType.SelectedItem as T_SYS_DICTIONARY;
                            if (type != null)
                            {
                                thd = this.GetVehicleTypeValue(type.DICTIONARYVALUE.ToString());
                                if (transportToolStand.Count() > 0)
                                {
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
                                            if (type != null)
                                            {
                                                if (thd.TYPEOFTRAVELTOOLS.ToInt32() > type.DICTIONARYVALUE)
                                                {
                                                    ComType.BorderBrush = new SolidColorBrush(Colors.Red);
                                                    ComType.Foreground = new SolidColorBrush(Colors.Red);
                                                    return;
                                                }
                                                if (thd.TAKETHETOOLLEVEL.ToInt32() > level.DICTIONARYVALUE)
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
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region 外出开会控件
        private void myChkBoxMeet_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].GOOUTTOMEET = "1";
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选内部会议或培训，无各项补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
        }

        private void myChkBoxMeet_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].GOOUTTOMEET = "0";
                    }
                }
            }
        }
        #endregion

        #region 公司派车控件
        private void myChkBoxCar_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].COMPANYCAR = "1";
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr("您已勾选公司派车，无交通补贴！"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
        }

        private void myChkBoxCar_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (!chk.IsChecked.Value)
            {
                T_OA_BUSINESSTRIPDETAIL btlist = (T_OA_BUSINESSTRIPDETAIL)chk.DataContext;
                if (btlist != null)
                {
                    var ents = from ent in TraveDetailList_Golbal
                               where ent.BUSINESSTRIPDETAILID == btlist.BUSINESSTRIPDETAILID
                               select ent;
                    if (ents.Count() > 0)
                    {
                        int k = TraveDetailList_Golbal.IndexOf(ents.FirstOrDefault());
                        TraveDetailList_Golbal[k].COMPANYCAR = "0";
                    }
                }
            }
        }
        #endregion

        #region KeyDown事件
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            int i = 0;
            if (e.Key == Key.Enter)
            {
                if (TraveDetailList_Golbal.Count() > 0)
                {
                    if (DaGrs.SelectedIndex == TraveDetailList_Golbal.Count - 1)
                    {
                        T_OA_BUSINESSTRIPDETAIL buip = new T_OA_BUSINESSTRIPDETAIL();
                        buip.BUSINESSTRIPDETAILID = Guid.NewGuid().ToString();
                        if (TraveDetailList_Golbal.Count() > 0)
                        {
                            foreach (T_OA_BUSINESSTRIPDETAIL tailList in TraveDetailList_Golbal)
                            {
                                tailList.DESTCITY = citysEndList_Golbal[i].Replace(",", "");
                                buip.DEPCITY = SMT.SaaS.FrameworkUI.Common.Utility.GetCityName(tailList.DESTCITY);
                                buip.STARTDATE = tailList.ENDDATE;
                            }
                            i++;
                        }
                        buip.ENDDATE = DateTime.Now;
                        TraveDetailList_Golbal.Add(buip);
                    }
                }
            }
        }
        #endregion

        #endregion

    }
}
