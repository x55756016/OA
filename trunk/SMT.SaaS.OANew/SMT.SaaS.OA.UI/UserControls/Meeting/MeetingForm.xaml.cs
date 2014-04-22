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
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using System.Collections.ObjectModel;
using SMT.Saas.Tools.FlowWFService;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.CommForm;

using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using System.Text;
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MeetingForm : BaseForm,IClient,IEntityEditor,IAudit
    {
        #region 修改说明
        /// <summary>
        /// 修改时将添加与会人员、会议内容、会议通知全都放置后台处理2010-5-28
        /// </summary>
        #endregion

        #region
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        //private MeetingInfoManagementServiceClient MeetingInfoClient = new MeetingInfoManagementServiceClient();
        private ObservableCollection<T_OA_MEETINGCONTENT> MeetingContentList = new ObservableCollection<T_OA_MEETINGCONTENT>();
        private ObservableCollection<T_OA_MEETINGSTAFF> MeetingStaffList = new ObservableCollection<T_OA_MEETINGSTAFF>();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private ObservableCollection<string> StrAddStaffList = new ObservableCollection<string>();  //获取员工时的ID数组

        private ObservableCollection<string> StrHostStaffList = new ObservableCollection<string>();  //主持人ID
        private ObservableCollection<string> StrRecordStaffList = new ObservableCollection<string>();  //记录人ID
        public delegate void refreshGridView();
        private string strMeetingType = ""; //会议类型
        private T_OA_MEETINGTYPE SelectMeetingType = new T_OA_MEETINGTYPE();
        private T_OA_MEETINGROOM SelectMeetingRoom = new T_OA_MEETINGROOM();
        private string strStartTime = ""; //开始时间 时：分
        private string strEndTime = ""; //结束时间
        private string strMeetingRoom = ""; //会议室
        private byte[] tmpTemplateContent ; //模板内容
        //private string tmpInsertContent = "";  //插入会议材料标记，如果为空则成功 非空则失败
        private T_OA_MEETINGINFO tmpMeetingInfo = new T_OA_MEETINGINFO();
        private string StrDepartment = "";  //部门
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        private bool AuditType = false; //是否提交了审核按钮
        private bool SaveAuditType = false; //保存提交审核标志
        
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpMeetingMember = new List<ExtOrgObj>();
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpHostMember = new List<ExtOrgObj>();
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpRecordMember = new List<ExtOrgObj>();
        public event refreshGridView ReloadDataEvent;
        
        private int SelectIndex = 0; //记录开始时间选择的位置
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;

        private FormTypes ActionType ;//动作标记
        
        
        private string first = "";//获取参会人员
        private string second = "";//主持人
        private string third = "";//记录人

        private string StrRecordID = "";//会议记录人ID
        private string StrHostID = "";//主持人ID

        bool SelectChange = false;//下拉框动作

        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID

        private T_OA_MEETINGMESSAGE MessageObj; //会议通知

        private List<ExtOrgObj> issuanceExtOrgObj;

        private SelectMeetingRoom addFrm;
        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        private V_MeetingInfo meetingT;
        private string strMeetingInfoID;

        #endregion 
        
        #region 构造函数

        public MeetingForm(FormTypes action, V_MeetingInfo MeetingT)
        {
            InitializeComponent();
            this.meetingT = MeetingT;
            ActionType = action;
            this.Loaded+=new RoutedEventHandler(MeetingForm_Loaded);
          
           
        }
        /// <summary>
        /// 门户审核调用
        /// </summary>
        /// <param name="action"></param>
        /// <param name="MeetingT"></param>
        public MeetingForm(FormTypes action, string StrMeetingInfoID)
        {
            InitializeComponent();

            ActionType = action;
            this.strMeetingInfoID = StrMeetingInfoID;
            this.Loaded += new RoutedEventHandler(MeetingForm_Loaded);
        }

        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            MeetingClient.MeetingInfoAddCompleted += new EventHandler<MeetingInfoAddCompletedEventArgs>(MeetingInfoClient_MeetingInfoAddCompleted);
            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
            MeetingClient.MeetingInfoUpdateByFormCompleted += new EventHandler<MeetingInfoUpdateByFormCompletedEventArgs>(MeetingClient_MeetingInfoUpdateByFormCompleted);
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);
            
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDCompleted += new EventHandler<GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs>(MeetingClient_GetMeetingRoomTreeInfosByCompanyIDCompleted);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            MeetingClient.GetMeetingMessageByIDCompleted += new EventHandler<GetMeetingMessageByIDCompletedEventArgs>(MeetingClient_GetMeetingMessageByIDCompleted);
            personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
            MeetingClient.GetMeetingInfoSingleInfoByIdCompleted += new EventHandler<GetMeetingInfoSingleInfoByIdCompletedEventArgs>(MeetingClient_GetMeetingInfoSingleInfoByIdCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
           // this.Loaded += new RoutedEventHandler(MeetingForm_Loaded);


            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (ActionType == FormTypes.New)
            {
                this.dpStartDate.Text = System.DateTime.Now.ToShortDateString();
                this.dpEndDate.Text = System.DateTime.Now.ToShortDateString();
                //SVAudit.Visibility = Visibility.Collapsed;
                //senddoctab.Items.IndexOf
                tmpMeetingInfo.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                //tabitemaudit.Visibility = Visibility.Collapsed;
                this.tpStartTime.Value = System.DateTime.Now.AddMinutes(60);
                this.tpEndTime.Value = System.DateTime.Now.AddMinutes(90);

                combox_SelectSource();

            }
            if (ActionType == FormTypes.Edit || ActionType == FormTypes.Resubmit)
            {
                SelectChange = false;

                GetMeetingInfoByInfo(this.meetingT);
                GetMeetingStaffInfo(this.meetingT.meetinginfo);
                GetMeetingMessageInfo(this.meetingT.meetinginfo);
                tmpMeetingInfo = this.meetingT.meetinginfo;
                if (ActionType == FormTypes.Resubmit)
                    tmpMeetingInfo.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                //tabitemaudit.Visibility = Visibility.Collapsed;
            }
            if (ActionType == FormTypes.Audit || ActionType == FormTypes.Browse)
            {

                SetEnabled();
                //audit.XmlObject = DataObjectToXml<T_OA_MEETINGINFO>.ObjListToXml(tmpMeetingInfo, "OA"); 
                GetMeetingInfoByInfo(this.meetingT);
                //RefreshUI(RefreshedTypes.AuditInfo);
                //RefreshUI(RefreshedTypes.All);
                //InitAudit(MeetingT.meetinginfo);
                GetMeetingStaffInfo(this.meetingT.meetinginfo);
                GetMeetingMessageInfo(this.meetingT.meetinginfo);

            }

            if (ActionType == FormTypes.Audit)
            {
                SetEnabled();

                MeetingClient.GetMeetingInfoSingleInfoByIdAsync(this.strMeetingInfoID);

            }

        }

        void MeetingForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();

            if (ActionType == FormTypes.New)
            {
                PostsObject.Text = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
                StrDepartment = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
                StrRecordID = Common.CurrentLoginUserInfo.EmployeeID;
                tbxRecordMembers.Text = Common.CurrentLoginUserInfo.EmployeeName;
                txtTel.Text = Utility.GetEmployeePhone();
            }
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void MeetingClient_GetMeetingInfoSingleInfoByIdCompleted(object sender, GetMeetingInfoSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        //GetMeetingInfoByInfo(e.Result);
                        tmpMeetingInfo = e.Result;
                        GetMeetingInfoByInfoAudit(e.Result);
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                        //InitAudit(e.Result);
                        GetMeetingStaffInfo(e.Result);
                        GetMeetingMessageInfo(e.Result);
                        //audit.XmlObject = DataObjectToXml<T_OA_MEETINGINFO>.ObjListToXml(tmpMeetingInfo, "OA"); 
                    }
                }
            }
            //throw new NotImplementedException();
        }

        void personclient_GetEmployeeDetailByParasCompleted(object sender, GetEmployeeDetailByParasCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    StrDepartmentIDsList.Clear();
                    StrCompanyIDsList.Clear();
                    StrPositionIDsList.Clear();
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();
                        //vemployeeObj.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void MeetingClient_GetMeetingMessageByIDCompleted(object sender, GetMeetingMessageByIDCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    this.txtMessageTitle.Text = e.Result.TITLE;                    
                    txtMessageContent.RichTextBoxContext = e.Result.CONTENT;
                    this.txtMessageTel.Text = e.Result.TEL;
                }
            }
        }

        

        void MeetingClient_GetMeetingRoomTreeInfosByCompanyIDCompleted(object sender, GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    //this.txtMeetingRoom.ItemsSource = e.Result.ToList();
                    //this.txtMeetingRoom.ValueMemberPath = "MEETINGROOMNAME";
                    //this.txtMeetingRoom.dis
                }
            }
        }

        /// <summary>
        /// 获取 主持人和记录人
        /// </summary>
        /// <param name="MeetingT"></param>
        private void GetHostAndRecordStaffInfo(T_OA_MEETINGINFO MeetingT)
        {
            if (MeetingT != null)
            {
                StrHostStaffList.Add(MeetingT.HOSTID);
                StrRecordStaffList.Add(MeetingT.HOSTID);
            }

        }

        private void SetEnabled()
        {
            this.txtTel.IsEnabled = false;
            this.tbxMeetingContent.IsEnabled = false;
            this.tbxMeetingTitle.IsEnabled = false;
            //this.cbMeetingRoom.IsEnabled = false;
            //this.cbxHostMembers.IsEnabled = false;
            
            this.btnFindHostMember.IsEnabled = false;
            //this.btnFindMember.IsEnabled = false;
            this.btnFindRecordMember.IsEnabled = false;
            this.cbxMeetingType.IsEnabled = false;
            
            this.dpStartDate.IsEnabled = false;
            this.dpEndDate.IsEnabled = false;
            this.tpStartTime.IsEnabled = false;
            this.tpEndTime.IsEnabled = false;
            this.PostsObject.IsEnabled = false;
        }

        

        /// <summary>
        /// 获取参会人员
        /// </summary>
        /// <param name="MeetingT"></param>
        private void GetMeetingStaffInfo(T_OA_MEETINGINFO MeetingT)
        {
            if (MeetingT != null)
            {                
                MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDAsync(MeetingT.MEETINGINFOID);
            }
        }
        private void GetMeetingMessageInfo(T_OA_MEETINGINFO MeetingT)
        {
            if (MeetingT != null)
            {
                MeetingClient.GetMeetingMessageByIDAsync(MeetingT.MEETINGINFOID);
            }
        }
        void MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted(object sender, GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result != null)
            {
                List<T_OA_MEETINGSTAFF> stafflist = new List<T_OA_MEETINGSTAFF>();
                stafflist = e.Result.ToList();
                foreach (T_OA_MEETINGSTAFF staff in stafflist)
                {
                    StrStaffList.Add(staff.MEETINGUSERID);
                }
                if (StrStaffList.Count >0)
                {
                    GetMeetingStaff(StrStaffList);
                }
                if (ActionType == FormTypes.Audit)
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }

            }
            
        }

        /// <summary>
        /// 获取参会人员并填充
        /// </summary>
        private void GetMeetingStaff(ObservableCollection<string> staffs)
        {   
            personclient.GetEmployeeDetailByIDsAsync(staffs);
            
        }

        void personclient_GetEmployeeDetailByIDsCompleted(object sender, GetEmployeeDetailByIDsCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    vemployeeObj.Clear();
                    StrStaffList.Clear();//清空员工ID集合 否则会逐条记录添加
                    if (e.Result != null)
                    {
                        vemployeeObj = e.Result.ToList();
                        //vemployeeObj.FirstOrDefault().T_HR_EMPLOYEE.EMPLOYEECNAME
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

        void personclient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {

            try
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> employee = e.Result.ToList();

                        //distributeList = e.Result.ToList();
                        //foreach (var h in employee)
                        //{
                        //    object obj = new object();
                        //    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj extOrgObj = new SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj();
                        //    SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE tmp = new SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE();
                        //    tmp.EMPLOYEEID = h.EMPLOYEEID;
                        //    tmp.EMPLOYEECNAME = h.EMPLOYEECNAME;
                        //    //tmp.
                        //    obj = tmp;
                            
                        //    extOrgObj.ObjectInstance = obj;
                        //    issuanceExtOrgObj.Add(extOrgObj);
                        //}
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
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }


            
        }


       
        private void GetMeetingInfoByInfo(V_MeetingInfo MeetingInfoT)
        {
            
            if (MeetingInfoT != null)
            {
                tmpMeetingInfo = MeetingInfoT.meetinginfo;
                //cbxMeetingType.SelectedItem = MeetingInfoT.MEETINGTYPE;
                //cbMeetingRoom.SelectedItem = MeetingInfoT.MEETINGROOMNAME;
                txtMeetingRoom.Text = MeetingInfoT.meetingroom.MEETINGROOMNAME;
                //txtMeetingRoom.SelectedItem = MeetingInfoT.meetingroom;
                SelectMeetingRoom = MeetingInfoT.meetingroom;
                SelectMeetingType = MeetingInfoT.meetingtype;
                
                tbxMeetingTitle.Text = MeetingInfoT.meetinginfo.MEETINGTITLE;                
                tbxMeetingContent.RichTextBoxContext = MeetingInfoT.meetinginfo.CONTENT;
                dpStartDate.Text = Convert.ToDateTime(MeetingInfoT.meetinginfo.STARTTIME).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(MeetingInfoT.meetinginfo.ENDTIME).ToShortDateString();
                txtTel.Text = MeetingInfoT.meetinginfo.TEL;
                PostsObject.Text = MeetingInfoT.meetinginfo.DEPARTNAME;
                strStartTime = Convert.ToDateTime(MeetingInfoT.meetinginfo.STARTTIME).ToShortTimeString();
                tpStartTime.Value = MeetingInfoT.meetinginfo.STARTTIME;
                tpEndTime.Value = MeetingInfoT.meetinginfo.ENDTIME;
                strEndTime = Convert.ToDateTime(MeetingInfoT.meetinginfo.ENDTIME).ToShortTimeString();
                tbxHostMembers.Text = MeetingInfoT.meetinginfo.HOSTNAME;
                StrHostID = MeetingInfoT.meetinginfo.HOSTID;
                tbxRecordMembers.Text = MeetingInfoT.meetinginfo.RECORDUSERNAME;
                StrRecordID = MeetingInfoT.meetinginfo.RECORDUSERID;
                
                combox_SelectSource();
                
            }
            
        }

        private void GetMeetingInfoByInfoAudit(T_OA_MEETINGINFO MeetingInfoT)
        {
            
            if (MeetingInfoT != null)
            {
                tmpMeetingInfo = MeetingInfoT;
                //cbxMeetingType.SelectedItem = MeetingInfoT.MEETINGTYPE;
                //cbMeetingRoom.SelectedItem = MeetingInfoT.MEETINGROOMNAME;
                txtMeetingRoom.Text = MeetingInfoT.T_OA_MEETINGROOM.MEETINGROOMNAME;
                //txtMeetingRoom.SelectedItem = MeetingInfoT.meetingroom;
                SelectMeetingRoom = MeetingInfoT.T_OA_MEETINGROOM;
                SelectMeetingType = MeetingInfoT.T_OA_MEETINGTYPE;

                tbxMeetingTitle.Text = MeetingInfoT.MEETINGTITLE;                                
                tbxMeetingContent.RichTextBoxContext = MeetingInfoT.CONTENT;
                dpStartDate.Text = Convert.ToDateTime(MeetingInfoT.STARTTIME).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(MeetingInfoT.ENDTIME).ToShortDateString();
                txtTel.Text = MeetingInfoT.TEL;
                PostsObject.Text = MeetingInfoT.DEPARTNAME;
                strStartTime = Convert.ToDateTime(MeetingInfoT.STARTTIME).ToShortTimeString();
                tpStartTime.Value = MeetingInfoT.STARTTIME;
                tpEndTime.Value = MeetingInfoT.ENDTIME;
                strEndTime = Convert.ToDateTime(MeetingInfoT.ENDTIME).ToShortTimeString();
                tbxHostMembers.Text = MeetingInfoT.HOSTNAME;
                StrHostID = MeetingInfoT.HOSTID;
                tbxRecordMembers.Text = MeetingInfoT.RECORDUSERNAME;
                StrRecordID = MeetingInfoT.RECORDUSERID;
                
                combox_SelectSource();

            }

        }
        #endregion

        #region COMBOX 设置数据源

        private void combox_SelectSource()
        {
            
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            

        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (ActionType == FormTypes.New)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            if (e.Result != null)
            {
                this.cbxMeetingType.Items.Clear();
                this.cbxMeetingType.ItemsSource = e.Result;
                this.cbxMeetingType.DisplayMemberPath = "MEETINGTYPE";

                if (SelectMeetingType.MEETINGTYPEID != null)
                {
                    foreach (var item in cbxMeetingType.Items)
                    {
                        T_OA_MEETINGTYPE dict = item as T_OA_MEETINGTYPE;
                        if (dict != null)
                        {
                            if (dict.MEETINGTYPE == SelectMeetingType.MEETINGTYPE)
                            {
                                cbxMeetingType.SelectedItem = item;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    cbxMeetingType.SelectedIndex = 0;
                }

            }
            else
            {
                this.cbxMeetingType.IsEnabled = false;// 无会议类型,设成不可编辑
            }
            
        }

        

       

        #endregion

        #region 增加修改

        private void SaveMeetingInfo()
        {
            
            string StrMeetingTitle = ""; //会议标题

            string StrMeetingTypeName = ""; //会议类型
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string StrEndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            //string StrContent = ""; //会议类容
            int StrCount = 0; //会议人数，由选择与会人员数量确定
            string StrMeetingRoom = ""; //会议室名
            string StrTel = ""; //电话
            
            string StrHostName = "";//主持人姓名
            
            string StrRecordName = ""; //会议记录人名

            string StrError = "";  //错误提示信息
            bool blError = true;  //是否正确
            string StrNoticeTitle = ""; //通知标题
            //string StrNoticeContent = "";//通知内容
            string StrNoticeTel = "";//通知联系人电话
            StrHostName = tbxHostMembers.Text.ToString();
            StrTel = this.txtTel.Text.Trim().ToString();
            StrRecordName = tbxRecordMembers.Text.ToString();
            StrNoticeTitle = this.txtMessageTitle.Text.ToString();
            //StrNoticeContent = this.txtMessageContent.Text.ToString();
            StrNoticeTel = this.txtMessageTel.Text.ToString();
            StrDepartment = PostsObject.Text.ToString();
            
            if (string.IsNullOrEmpty(StrDepartment))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDEPART"));
                return;
            }
            if (string.IsNullOrEmpty(SelectMeetingRoom.MEETINGROOMID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTMEETINGROOM"));
                
                return;
            }

            if (!string.IsNullOrEmpty(this.dpStartDate.Text.ToString()))
            {
                StrStartDt = this.dpStartDate.Text.ToString();
            }
            else
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                return;


            }
            if (!string.IsNullOrEmpty(this.dpEndDate.Text.ToString()))
            {
                StrEndDt = this.dpEndDate.Text.ToString();

            }
            else
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                return;

            }
            if (this.cbxMeetingType.SelectedIndex > -1)
            {
                SelectMeetingType = this.cbxMeetingType.SelectedItem as T_OA_MEETINGTYPE;
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDMEETINGTYPE"));
                this.cbxMeetingType.Focus();
                return;
            }
            //StrMeetingTypeName = this.cbxMeetingType.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(tpStartTime.Value.Value.ToString()))
            {
                StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.Value.Value.ToString()))
            {
                StrEndTime = this.tpEndTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGENDTIMENOTNULL"));
                return;
            }
            if (!string.IsNullOrEmpty(this.tbxMeetingTitle.Text.ToString()))
            {
                StrMeetingTitle = this.tbxMeetingTitle.Text.ToString();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGTITLE"));
                return;
                
            }
            if (this.tbxMeetingContent.GetRichTextbox().Xaml == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGCONTENT"));
                return;
                //StrContent = HttpUtility.HtmlEncode(this.tbxMeetingContent.Text.ToString());
            }
            
            
            if (vemployeeObj.Count() > 0) //使用 vemployeeObj 而不用tmpmember ，tmpmember是从数据处取出来
            {
                StrCount = vemployeeObj.Count();
            }
            
            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart <= System.DateTime.Now)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "STARTTIME"));
                return;
            }
            if (DtEnd <= System.DateTime.Now)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "ENDTIME"));
                return;
            }
            if (DtStart >= DtEnd)
            {
                //StrError += "会议开始时间必须小于结束时间！\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME1"));
                return;
                //MessageBox.Show("会议开始时间必须小于结束时间");
            }
            if (!(StrCount > 0))
            {
                //StrError += "请选择与会人员！\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PLEASESELECTMEETINGMEMBER"));
                return;
            }
            //foreach (var MeetingMember in tmpMeetingMember)
            //{
            //    T_OA_MEETINGSTAFF StaffT = new T_OA_MEETINGSTAFF();
            //    StaffT.MEETINGSTAFFID = System.Guid.NewGuid().ToString();
            //    StaffT.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
            //    StaffT.CREATEUSERID = tmpMeetingInfo.CREATEUSERID;
            //    StaffT.CONFIRMFLAG = "0";
            //    StaffT.FILENAME = "";
            //    StaffT.MEETINGUSERID = MeetingMember.ObjectID.ToString();// ArrUsers[i].ToString();
            //}
            if (StrHostName == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTHOSTMEMBER"));
                
                return;
            }
            else
            {
                StrHostName = tbxHostMembers.Text.ToString();
                
            }
            if (StrRecordName == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTRECORDMEMBER"));
                //cbxRecordMembers.Focus();
                return;
            }
            else
            {
                StrRecordName = tbxRecordMembers.Text.ToString();
                
            }

            if (string.IsNullOrEmpty(StrNoticeTitle))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL","NOTICETITLE"));                
                return;
            }
            if (this.txtMessageContent.GetRichTextbox().Xaml == "")
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL","NOTICECONTENT"));
                return;
            }
            if (string.IsNullOrEmpty(StrNoticeTel))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "MEETINGMESSATETEL"));
                return;
            }

            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (ActionType == FormTypes.New)
            {
                tmpMeetingInfo.MEETINGINFOID = System.Guid.NewGuid().ToString();
                tmpMeetingInfo.T_OA_MEETINGROOM = SelectMeetingRoom;
                tmpMeetingInfo.MEETINGTITLE = StrMeetingTitle;
                tmpMeetingInfo.TEL = StrTel;
                
                tmpMeetingInfo.T_OA_MEETINGTYPE = SelectMeetingType;
                tmpMeetingInfo.DEPARTNAME = StrDepartment;
                                
                tmpMeetingInfo.CONTENT = tbxMeetingContent.RichTextBoxContext;
                tmpMeetingInfo.COUNT = System.Convert.ToInt16(StrCount);
                tmpMeetingInfo.STARTTIME = DtStart;
                tmpMeetingInfo.ENDTIME = DtEnd;
                tmpMeetingInfo.ISCANCEL = "1";
                tmpMeetingInfo.ISAUTO = "0";
                tmpMeetingInfo.HOSTID = StrHostID;
                tmpMeetingInfo.HOSTNAME = StrHostName;
                tmpMeetingInfo.RECORDUSERID = StrRecordID;
                tmpMeetingInfo.RECORDUSERNAME = StrRecordName;

                tmpMeetingInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                tmpMeetingInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                tmpMeetingInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                tmpMeetingInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpMeetingInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpMeetingInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                tmpMeetingInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpMeetingInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpMeetingInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpMeetingInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                

                tmpMeetingInfo.UPDATEUSERNAME = "";

                tmpMeetingInfo.UPDATEDATE = null;
                tmpMeetingInfo.UPDATEUSERID = "";
                tmpMeetingInfo.CHECKSTATE = "0";  //未提交
                tmpMeetingInfo.CREATEDATE = System.DateTime.Now;

                AddMeetingStaffInfo();//添加与会人员
                AddStaffMessage();//添加会议通知
                AddMeetingContentInfo();//添加会议内容
                try
                {
                    MeetingClient.MeetingInfoAddAsync(tmpMeetingInfo, MeetingStaffList, MeetingContentList, MessageObj,"Add");
                                        
                }
                catch (Exception ex)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    //MessageBox.Show(ex.ToString());
                }
            }
            if (ActionType == FormTypes.Edit)
            {
                
                tmpMeetingInfo.T_OA_MEETINGROOM = SelectMeetingRoom;
                tmpMeetingInfo.MEETINGTITLE = StrMeetingTitle;
                tmpMeetingInfo.TEL = StrTel;
                tmpMeetingInfo.T_OA_MEETINGTYPE = SelectMeetingType;
                tmpMeetingInfo.DEPARTNAME = StrDepartment;
                                
                tmpMeetingInfo.CONTENT = tbxMeetingContent.RichTextBoxContext;
                tmpMeetingInfo.COUNT = System.Convert.ToInt16(StrCount);
                tmpMeetingInfo.STARTTIME = DtStart;
                tmpMeetingInfo.ENDTIME = DtEnd;
                
                tmpMeetingInfo.CHECKSTATE = "0"; //申请状态
                tmpMeetingInfo.CREATEDATE = System.Convert.ToDateTime(dpStartDate.Text.ToString());
                
                tmpMeetingInfo.UPDATEDATE = System.DateTime.Now;
                tmpMeetingInfo.UPDATEUSERID = "1";
                tmpMeetingInfo.ISAUTO = "0";

                tmpMeetingInfo.HOSTID = StrHostID;
                tmpMeetingInfo.HOSTNAME = StrHostName;
                tmpMeetingInfo.RECORDUSERID = StrRecordID;
                tmpMeetingInfo.RECORDUSERNAME = StrRecordName;
                
                tmpMeetingInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpMeetingInfo.UPDATEDATE = DateTime.Now;
                tmpMeetingInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;


                AddMeetingStaffInfo();//添加与会人员
                AddStaffMessage();//添加会议通知
                AddMeetingContentInfo();//添加会议内容

                try
                {
                    MeetingClient.MeetingInfoUpdateByFormAsync(tmpMeetingInfo, MeetingStaffList, MeetingContentList, MessageObj,"Edit");

                    
                }
                catch (Exception ex)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),ex.ToString());
                    RefreshUI(RefreshedTypes.HideProgressBar);//结束动画
                }
              }
            
            
        }

        #endregion

        #region 添加按钮事件
        
        void MeetingInfoClient_MeetingInfoAddCompleted(object sender, MeetingInfoAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result == "")
                    {

                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                        else
                        {
                            ActionType = FormTypes.Edit;
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }
                                                
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }
        
        

        #endregion

        #region 修改按钮事件

        /// <summary>
        /// 针对审核的动作  审核时 只修改会议主表 其它不操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Error == null)
                    {
                        if (e.Result > 0)
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
                                    ActionType = FormTypes.Edit;
                                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                    entBrowser.FormType = FormTypes.Edit;
                                    RefreshUI(RefreshedTypes.AuditInfo);
                                    RefreshUI(RefreshedTypes.All);
                                }

                                //if (GlobalFunction.IsSaveAndClose(refreshType))
                                //{
                                //    RefreshUI(refreshType);
                                //}
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

                            //if (ActionType == FormTypes.Audit)
                            //{
                            //    //MessageBox.Show("审核成功");
                            //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "MEETINGADD"));
                            //    RefreshUI(refreshType);

                            //}
                            //else
                            //{

                            //    if (SaveAuditType)
                            //    {
                            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSEDFORMEETINGINFO"));
                            //        RefreshUI(RefreshedTypes.CloseAndReloadData);

                            //    }
                            //    else
                            //    {
                            //        //MessageBox.Show("会议信息修改成功！");
                            //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGADD"));
                            //    }

                            //    RefreshUI(refreshType);
                            //    //this.Close();

                            //}
                        }
                    }//需要将修改记录添加到会议修改表中
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), e.Error.Message.ToString());
                    }
                }
                else
                {
                    //MessageBox.Show(e.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.ToString());
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }

        }

        /// <summary>
        /// 是针对修改操作的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MeetingClient_MeetingInfoUpdateByFormCompleted(object sender, MeetingInfoUpdateByFormCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result == 0)
                {
                    if (ActionType == FormTypes.Audit)
                    {
                        //MessageBox.Show("审核成功");
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "MEETINGADD"));
                        RefreshUI(refreshType);

                    }
                    else
                    {

                        if (e.UserState.ToString() == "Edit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGADD"));
                            if (GlobalFunction.IsSaveAndClose(refreshType))
                            {
                                RefreshUI(refreshType);
                            }
                            else
                            {
                                ActionType = FormTypes.Edit;
                                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                entBrowser.FormType = FormTypes.Edit;
                                RefreshUI(RefreshedTypes.AuditInfo);
                                RefreshUI(RefreshedTypes.All);
                            }
                        }
                        else if (e.UserState.ToString() == "Audit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "MEETINGADD"));
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITAUDITSUCCESSEDFORMEETINGINFO"));
                            //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITCOMPANYDOCAUDIT"));
                        }
                        RefreshUI(RefreshedTypes.All);


                        
                    }
                }
                //需要将修改记录添加到会议修改表中

            }
            else
            {
                //MessageBox.Show(e.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.ToString());
            }
        }


        #endregion 

        #region 添加与会人员信息
        void AddMeetingStaffInfo()
        {
            
            //MeetingStaffManagementServiceClient MemberClient = new MeetingStaffManagementServiceClient();
            if (tmpMeetingMember != null)
            {
                MeetingStaffList.Clear();
                foreach (var MeetingMember in vemployeeObj)
                {
                    T_OA_MEETINGSTAFF StaffT = new T_OA_MEETINGSTAFF();

                    StaffT.MEETINGSTAFFID = System.Guid.NewGuid().ToString();
                    StaffT.T_OA_MEETINGINFO = tmpMeetingInfo;// new T_OA_MEETINGINFO();
                        //tmpMeetingInfo;
                    StaffT.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
                    StaffT.CREATEUSERID = tmpMeetingInfo.CREATEUSERID;
                    StaffT.CONFIRMFLAG = "0";
                    StaffT.ISOK = "0";
                    StaffT.FILENAME = "";
                    //StaffT.MEETINGUSERID = MeetingMember.ObjectID.ToString();// ArrUsers[i].ToString();
                    StaffT.MEETINGUSERID = MeetingMember.T_HR_EMPLOYEE.EMPLOYEEID;
                    StaffT.CREATEDATE = Convert.ToDateTime(tmpMeetingInfo.CREATEDATE);
                    StaffT.UPDATEDATE = null;
                    StaffT.UPDATEUSERID = "";
                    StaffT.UPDATEUSERNAME = "";
                    StaffT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    StaffT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    StaffT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    StaffT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    StaffT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    StaffT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    StaffT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    //StaffT.OWNERNAME = MeetingMember.ObjectName;// Common.CurrentLoginUserInfo.EmployeeName;
                    StaffT.OWNERNAME = MeetingMember.T_HR_EMPLOYEE.EMPLOYEECNAME;// Common.CurrentLoginUserInfo.EmployeeName;
                    StaffT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    StaffT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    
                    MeetingStaffList.Add(StaffT);
                }
            }            

            //MeetingClient.BatchAddMeetingStaffInfosCompleted += new EventHandler<BatchAddMeetingStaffInfosCompletedEventArgs>(MemberClient_BatchAddMeetingStaffInfosCompleted);
            //MeetingClient.BatchAddMeetingStaffInfosAsync(MeetingStaffList);
            
            
        }
        /// <summary>
        /// 添加与会人员通知
        /// </summary>
        void AddStaffMessage()
        {
            string StrTitle= txtMessageTitle.Text.ToString();
            //string StrContent=txtMessageContent.Text.ToString();
            string StrTel = txtMessageTel.Text.ToString();
            MessageObj = new T_OA_MEETINGMESSAGE();
            MessageObj.MEETINGMESSAGEID = System.Guid.NewGuid().ToString();

            MessageObj.T_OA_MEETINGINFO = tmpMeetingInfo;// new T_OA_MEETINGINFO();// tmpMeetingInfo;
            //MessageObj.T_OA_MEETINGINFO.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
            
            MessageObj.TITLE = StrTitle;
            //MessageObj.CONTENT = StrContent;
            
            MessageObj.CONTENT = txtMessageContent.RichTextBoxContext;
            MessageObj.TEL = StrTel;

            MessageObj.CREATEDATE = System.DateTime.Now;
            MessageObj.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            MessageObj.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            MessageObj.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            MessageObj.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            MessageObj.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            MessageObj.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            MessageObj.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;            
            MessageObj.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            MessageObj.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            MessageObj.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;

        }

        void MemberClient_BatchAddMeetingStaffInfosCompleted(object sender, BatchAddMeetingStaffInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    //MessageBox.Show(e.Result.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),e.Result.ToString());
                }
            }
        }

        void MemberClient_MeetingStaffInfoAddCompleted(object sender,MeetingStaffInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Result.ToString());
                    //MessageBox.Show("与会人员"+e.Result.ToString());
                }
            }
        }
        #endregion

        #region 会议室是否被占用

        private void checkRoomIsUsing()
        {
            
            //MeetingClient.IsExistMeetingRoomJustUsingCompleted += new EventHandler<IsExistMeetingRoomJustUsingCompletedEventArgs>(RoomApp_IsExistMeetingRoomJustUsingCompleted);
            //MeetingClient.IsExistMeetingRoomJustUsingAsync(tmpMeetingInfo.T_OA_MEETINGROOM.MEETINGROOMNAME, tmpMeetingInfo.STARTTIME, tmpMeetingInfo.ENDTIME);
            
        }


        void RoomApp_IsExistMeetingRoomJustUsingCompleted(object sender, IsExistMeetingRoomJustUsingCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //MessageBox.Show(tmpMeetingInfo.MEETINGROOMNAME+"被占用");
                }
                else
                {
                    //MeetingRoomAppManagementServiceClient RoomApp = new MeetingRoomAppManagementServiceClient();                
                    T_OA_MEETINGROOMAPP RoomAppT = new T_OA_MEETINGROOMAPP();
                    RoomAppT.MEETINGROOMAPPID = System.Guid.NewGuid().ToString();
                    //RoomAppT.MEETINGROOMNAME = tmpMeetingInfo.MEETINGROOMNAME;
                    RoomAppT.DEPARTNAME = tmpMeetingInfo.DEPARTNAME;
                    RoomAppT.STARTTIME = tmpMeetingInfo.STARTTIME;
                    RoomAppT.ENDTIME = tmpMeetingInfo.ENDTIME;
                    RoomAppT.CHECKSTATE = "0";  //申请状态
                    RoomAppT.CREATEDATE = System.DateTime.Now;
                    RoomAppT.ISCANCEL = "1";

                    RoomAppT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    RoomAppT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    RoomAppT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    RoomAppT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    RoomAppT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    RoomAppT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    RoomAppT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    RoomAppT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    RoomAppT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    RoomAppT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                    RoomAppT.UPDATEUSERNAME = "";
                    RoomAppT.UPDATEDATE = null;
                    RoomAppT.UPDATEUSERID = "";

                    

                    MeetingClient.MeetingRoomAppInfoAddCompleted += new EventHandler<MeetingRoomAppInfoAddCompletedEventArgs>(RoomApp_MeetingRoomAppInfoAddCompleted);
                    MeetingClient.MeetingRoomAppInfoAddAsync(RoomAppT);


                }
            }

        }

        void RoomApp_MeetingRoomAppInfoAddCompleted(object sender, MeetingRoomAppInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                //MessageBox.Show("会议申请成功");
                //this.ReloadDataEvent();
                //this.Close();

            }
        }

        #endregion

        #region 添加会议上传材料
        void AddMeetingContentInfo()
        {
            
            //string StrContent = "";//会议材料内容
            string StrTemplateName = ""; //模板名称
            
            //if (cbxMeetingTypeTemplate.SelectedIndex > 0)
            //{
            //    //StrTemplateName = this.cbxMeetingTypeTemplate.SelectedItem.ToString();
            //    T_OA_MEETINGTEMPLATE TemplateT = this.cbxMeetingTypeTemplate.SelectedItem as T_OA_MEETINGTEMPLATE;
            //    StrTemplateName = TemplateT.TEMPLATENAME;
            //    tmpTemplateContent = TemplateT.CONTENT;                
            //}
            tmpTemplateContent = System.Text.Encoding.Unicode.GetBytes(this.tbxMeetingContent.GetRichTextbox().Xaml);

            if (tmpMeetingMember != null)
            {
                MeetingContentList.Clear();
                foreach (var MeetingMember in vemployeeObj)
                {
                    T_OA_MEETINGCONTENT ContentT = new T_OA_MEETINGCONTENT();
                    ContentT.MEETINGCONTENTID = System.Guid.NewGuid().ToString();
                    ContentT.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
                    //ContentT.MEETINGUSERID = MeetingMember.ObjectID;
                    ContentT.MEETINGUSERID = MeetingMember.T_HR_EMPLOYEE.EMPLOYEEID;
                    ContentT.CREATEDATE = Convert.ToDateTime(tmpMeetingInfo.CREATEDATE);
                    ContentT.CREATEUSERID = tmpMeetingInfo.CREATEUSERID;
                    ContentT.UPDATEDATE = null;
                    ContentT.UPDATEUSERID = "";
                    ContentT.UPDATEUSERNAME = "";
                    ContentT.CONTENT = System.Convert.ToString(tmpTemplateContent);

                    ContentT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ContentT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    ContentT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ContentT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    ContentT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    ContentT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ContentT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    //ContentT.OWNERNAME = MeetingMember.ObjectName;//Common.CurrentLoginUserInfo.EmployeeName;
                    ContentT.OWNERNAME = MeetingMember.T_HR_EMPLOYEE.EMPLOYEECNAME;
                    ContentT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    ContentT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;

                    MeetingContentList.Add(ContentT);
                }
            }      

            //MeetingClient.BatchAddMeetingContentInfosCompleted += new EventHandler<BatchAddMeetingContentInfosCompletedEventArgs>(MeetingContentClient_BatchAddMeetingContentInfosCompleted);
            //MeetingClient.BatchAddMeetingContentInfosAsync(MeetingContentList);
            
        }

        void MeetingContentClient_BatchAddMeetingContentInfosCompleted(object sender, BatchAddMeetingContentInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    //MessageBox.Show("会议内容"+e.Result.ToString());
                }
            }
        }
        

        
        #endregion

        #region 会议类型改变
        private void cbxMeetingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string StrSelMeetingType = "";
            if (SelectChange)
            {
                if (cbxMeetingType.Items.Count > 0)
                {
                    T_OA_MEETINGTYPE MeetingType = this.cbxMeetingType.SelectedItem as T_OA_MEETINGTYPE;
                    StrSelMeetingType = MeetingType.MEETINGTYPE.ToString();
                    tbxMeetingContent.RichTextBoxContext = MeetingType.CONTENT;
                    //MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted += new EventHandler<GetMeetingTypeTemplateNameInfosByMeetingTypeCompletedEventArgs>(TemplateClient_GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted);
                    //MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeAsync(StrSelMeetingType);
                }
            }
            else
            {
                SelectChange = true;
            }

        }

        
        #endregion       
        
        #region 模板选择改变
        private void cbxMeetingTypeTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxName = sender as ComboBox;
            T_OA_MEETINGTEMPLATE docTemplate = cbxName.SelectedItem as T_OA_MEETINGTEMPLATE;
            if (docTemplate != null)
            {
                string TemplateID = docTemplate.MEETINGTEMPLATEID;                
                MeetingClient.GetMeetingTypeTemplateSingleInfoByIdCompleted += new EventHandler<GetMeetingTypeTemplateSingleInfoByIdCompletedEventArgs>(MeetingClient_GetMeetingTypeTemplateSingleInfoByIdCompleted);
                MeetingClient.GetMeetingTypeTemplateSingleInfoByIdAsync(TemplateID);
            }
        }

        void  MeetingClient_GetMeetingTypeTemplateSingleInfoByIdCompleted(object sender, GetMeetingTypeTemplateSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                if (e.Result != null)
                {
                    T_OA_MEETINGTEMPLATE template = e.Result as T_OA_MEETINGTEMPLATE;
                    tbxMeetingContent.RichTextBoxContext = template.CONTENT;

                }
            }
        }
        
        #endregion
        
        #region 提交流程

        
        #endregion
        
        #region 审核动作

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
            try
            {
                if (tmpMeetingInfo != null)
                {                    
                    tmpMeetingInfo.UPDATEDATE = DateTime.Now;
                    tmpMeetingInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpMeetingInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    SaveAuditType = true;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            tmpMeetingInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            tmpMeetingInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            tmpMeetingInfo.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    tmpMeetingInfo.T_OA_MEETINGROOM = SelectMeetingRoom;
                    tmpMeetingInfo.T_OA_MEETINGTYPE = SelectMeetingType;
                    MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo);

                    //if (ActionType == FormTypes.New)
                    //{
                    //    if (AuditType)
                    //    {
                    //        MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo);                            
                    //    }

                        
                        
                    //}
                    //if (ActionType == FormTypes.Edit || ActionType == FormTypes.Audit)
                    //{
                    //    if (AuditType)
                    //    {
                    //        MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo);
                    //    }                        
                        
                    //}
                    
                    
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                return;
            }
        }

        private void Cancel()
        {
            //this.DialogResult = true;
            //this.Close();
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            //this.Close();
            this.ReloadData();
        }
        #endregion

        #region 审核按钮
        private void btnAddToConfirm_Click_1(object sender, RoutedEventArgs e)
        {
            AuditType = true;
            this.SaveMeetingInfo();

        }
        #endregion

        #region 开始时间改变
        private void tpStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.tpEndTime.SelectedIndex = tpStartTime.SelectedIndex + 1;
        }
        #endregion

        #region 绑定参会人员
        private void AddMembersObj()
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.All;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    
                    foreach (var h in ent)
                    {
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company)//公司
                        {
                            StrCompanyIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department)//部门
                        {
                            
                            StrDepartmentIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Post)//岗位
                        {
                            StrPositionIDsList.Add(h.ObjectID);
                        }
                        if (h.ObjectType == SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel)
                        {
                            StrStaffList.Add(h.ObjectID);
                        }
                    }
                    //issuanceExtOrgObj = ent;
                    tmpMeetingMember = ent;                    
                    personclient.GetEmployeeDetailByParasAsync(StrCompanyIDsList,StrDepartmentIDsList,StrPositionIDsList,StrStaffList);
                    //personclient.GetEmployeeDetailByIDsAsync(StrStaffList);
                    //BindData();
                }
            };
            lookup.MultiSelected = true;
            lookup.SelectSameGradeOnly = true;
            lookup.Show();
        }

        private void BindData()
        {

            if (vemployeeObj == null || vemployeeObj.Count < 1)
            {
                dgmember.ItemsSource = null;

                return;
            }
            else
            {
                dgmember.ItemsSource = vemployeeObj;
            }

        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            string StrReturn = "";
            switch (ActionType)
            { 
                case FormTypes.New:
                    StrReturn = Utility.GetResourceStr("ADDTITLE", "MEETINGADD");
                    break;
                case FormTypes.Edit:
                    StrReturn = Utility.GetResourceStr("EDITTITLE", "MEETINGADD");
                    break;
                case FormTypes.Audit:
                    StrReturn = Utility.GetResourceStr("AUDITTITLE", "MEETINGADD");
                    break;
                case FormTypes.Browse:
                    StrReturn = Utility.GetResourceStr("VIEWTITLE", "MEETINGADD");
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
                case "0"://保存
                    refreshType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1"://保存并关闭
                    refreshType = RefreshedTypes.CloseAndReloadData;                    
                    Save();
                    break;
                case "2"://提交审核
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    AuditType = true;
                    this.SaveMeetingInfo();
                    break;
                case "3"://选择参会人员
                    AddMembersObj();
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

            if (ActionType != FormTypes.Browse && ActionType != FormTypes.Audit)
            {

                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "3",
                    Title = Utility.GetResourceStr("MEETINGMEMBER"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addTeam.png"

                };
                items.Add(item);
                //item = new ToolbarItem
                //{
                //    DisplayType = ToolbarItemDisplayTypes.Image,
                //    Key = "2",
                //    Title = Utility.GetResourceStr("SUBMITAUDIT"),
                //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
                //};
                //items.Add(item);
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
            }

            return items;
        }

        private void Close()
        {
            RefreshUI(refreshType);
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

        #region 确定、取消
        private void Save()
        {

            if (ActionType == FormTypes.New)
            { this.SaveMeetingInfo(); }
            if (ActionType == FormTypes.Edit)
            {
                this.SaveMeetingInfo();
            }
            if (ActionType == FormTypes.Audit)
            {
                AuditType = true;
                this.SaveMeetingInfo();
            }
        }

        private void SaveAndClose()
        {
            Save();
            //RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        #region 主持人
        private void btnFindHostMember_Click(object sender, RoutedEventArgs e)
        {

            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //tmpHostMember = companyInfo;
                    StrHostID = companyInfo.ObjectID;
                    personclient.GetEmployeeByIDAsync(StrHostID);//获取主持人的电话
                    personclient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(personclient_GetEmployeeByIDCompleted);
                    tbxHostMembers.Text = companyInfo.ObjectName;
                    //CompanyObject.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();

            
        }

        


        #endregion

        #region 获取主持人的联系电话
        void personclient_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    //如果办公电话为空 则取手机号码
                    if (!string.IsNullOrEmpty(e.Result.OFFICEPHONE))
                    {

                        this.txtTel.Text = e.Result.OFFICEPHONE + " ";
                        this.txtMessageTel.Text = e.Result.OFFICEPHONE+ " ";
                    }
                    if(!string.IsNullOrEmpty(e.Result.MOBILE))
                    {
                        this.txtTel.Text += e.Result.MOBILE;
                        this.txtMessageTel.Text += e.Result.MOBILE;
                    }
                }
            }

        }
        #endregion

        #region 会议记录人

        private void btnFindRecordMember_Click(object sender, RoutedEventArgs e)
        {

            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //tmpHostMember = companyInfo;
                    StrRecordID = companyInfo.ObjectID;
                    tbxRecordMembers.Text = companyInfo.ObjectName;                    
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();


        }

        

        #endregion

        #region 选择部门
        
        
        private void btnFindDepartment_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();                    
                     
                    PostsObject.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion


        #region 加载会议室信息        
        
        private void browser_ReloadDataEvent()
        {
            if (addFrm.SelectedMeetingRoom != null )
            {
                SelectMeetingRoom = addFrm.SelectedMeetingRoom;
                this.txtMeetingRoom.Text = addFrm.SelectedMeetingRoom.MEETINGROOMNAME;
                
            }

        }
        #endregion


        #region tabcontrol选择事件
        
        
        private void senddoctab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl TabC = sender as TabControl;
            if (TabC.SelectedIndex == 2)
            {
                if (!string.IsNullOrEmpty(tbxMeetingTitle.Text.ToString()))
                {
                    this.txtMessageTitle.Text = Utility.GetResourceStr("NOTICEABOUT") + tbxMeetingTitle.Text.ToString() + Utility.GetResourceStr("NOTICES"); ;                    
                    String MeetingRoom = this.txtMeetingRoom.Text.ToString();
                    string StrStartDt = "";
                    string StrStartTime = "";
                    string StrEndDt = "";
                    string StrEndTime = "";
                    StrStartDt = this.dpStartDate.Text.ToString();
                    StrEndDt = this.dpEndDate.Text.ToString();
                    StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
                    StrEndTime = tpEndTime.Value.Value.ToString("HH:mm");
                    DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                    DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
                    
                        
                    if (!string.IsNullOrEmpty(MeetingRoom) && (DtStart < DtEnd))
                    {
                        if (this.txtMessageContent.RichTextBoxContext == null)
                        {
                            SMT.SaaS.FrameworkUI.Common.Utility.SetRichTextBoxDataByString(this.txtMessageContent.GetRichTextbox(), this.StringTransDate(DtStart, DtEnd, MeetingRoom, tbxMeetingTitle.Text.ToString()));
                                
                        }
                    }                        
                    
                }                
            }                        
        }

        #endregion

        #region 事件格式转换
        
        
        private string StringTransDate(DateTime StartDate, DateTime EndDate, string MeetingRoom, string StrTitle)
        {
            //XX年XX月XX日，X点X分至X点X分，在XXX会议室举行XX会议，忘各位准时参加
            string StrReturn = "";
            //StrReturn = StartDate.Year.ToString() + Utility.GetResourceStr("YEAR") + StartDate.Month.ToString() + Utility.GetResourceStr("MONTH") +
            //    StartDate.Day.ToString() + Utility.GetResourceStr("DAY");
            //StrReturn = StartDate.ToShortDateString();
            StrReturn = Utility.GetResourceStr("NOWDECIDE") + StartDate.ToLongDateString().ToString() + ",";
            StrReturn += StartDate.ToShortTimeString() + Utility.GetResourceStr("TO") + EndDate.ToShortTimeString() + ".";
            StrReturn += Utility.GetResourceStr("IN") + MeetingRoom + Utility.GetResourceStr("MEETINGROOM") + Utility.GetResourceStr("CELEBRATE") + StrTitle + Utility.GetResourceStr("NOTICES") + ",";
            StrReturn += Utility.GetResourceStr("HOPEONTIMEVISIST");
            return StrReturn;
        }
        private byte[] TransDate(DateTime StartDate,DateTime EndDate,string MeetingRoom,string StrTitle)
        {
            //XX年XX月XX日，X点X分至X点X分，在XXX会议室举行XX会议，忘各位准时参加
            string StrReturn = "";
            //StrReturn = StartDate.Year.ToString() + Utility.GetResourceStr("YEAR") + StartDate.Month.ToString() + Utility.GetResourceStr("MONTH") +
            //    StartDate.Day.ToString() + Utility.GetResourceStr("DAY");
            //StrReturn = StartDate.ToShortDateString();
            StrReturn = Utility.GetResourceStr("NOWDECIDE") + StartDate.ToLongDateString().ToString() + ",";
            StrReturn += StartDate.ToShortTimeString() +Utility.GetResourceStr("TO")+ EndDate.ToShortTimeString()+".";
            StrReturn += Utility.GetResourceStr("IN") + MeetingRoom + Utility.GetResourceStr("MEETINGROOM") + Utility.GetResourceStr("CELEBRATE") + StrTitle + Utility.GetResourceStr("NOTICES") + ",";
            StrReturn += Utility.GetResourceStr("HOPEONTIMEVISIST");
            return System.Text.Encoding.Unicode.GetBytes(StrReturn); 
        }
        #endregion

        #region 删除按钮
        
        

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            Button delBtn = sender as Button;
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST MeetingV = delBtn.Tag as SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST;
            vemployeeObj.Remove(MeetingV);
            dgmember.ItemsSource = null;
            BindData();
        }
        #endregion

        #region dgmember_loadingrow事件
        
        
        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST StaffV = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST)e.Row.DataContext;

            Button DelBtn = dgmember.Columns[4].GetCellContent(e.Row).FindName("BtnDel") as Button;
            DelBtn.Tag = StaffV;
            int index = e.Row.GetIndex();
            var cell = dgmember.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();　
            

        }
        #endregion

        private void btnFindRoom_Click(object sender, RoutedEventArgs e)
        {
            string StrMessage = CheckMeetingDate();
            if (!string.IsNullOrEmpty(StrMessage))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("TIPS"), Utility.GetResourceStr(StrMessage), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            else
            {
                       
                string StrStartDt = "";   //开始时间
                string StrStartTime = ""; //开始时：分
                string StrEndDt = "";    //结束时间
                string StrEndTime = ""; //结束时：分


                StrStartDt = this.dpStartDate.Text.ToString();
                StrEndDt = this.dpEndDate.Text.ToString();
                StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
                StrEndTime = this.tpEndTime.Value.Value.ToString("HH:mm");


                DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
                addFrm = new SelectMeetingRoom(DtStart,DtEnd);

                EntityBrowser browser = new EntityBrowser(addFrm);
                browser.MinWidth = 500;
                browser.MinHeight = 400;
                browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
                browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
            }
        }

        #region 时间大小的判断
        private string CheckMeetingDate()
        {
            string StrReturn = "";//返回字符串            
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string StrEndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            

            if (!string.IsNullOrEmpty(this.dpStartDate.Text.ToString()))
            {
                StrStartDt = this.dpStartDate.Text.ToString();
            }
            else
            {
                StrReturn = "STARTTIMENOTNULL";

            }
            if (!string.IsNullOrEmpty(this.dpEndDate.Text.ToString()))
            {
                StrEndDt = this.dpEndDate.Text.ToString();

            }
            else
            {
                StrReturn = "ENDTIMENOTNULL";
            }
            
            if (!string.IsNullOrEmpty(tpStartTime.Value.Value.ToString()))
            {
                StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
            }
            else
            {                
                StrReturn = "MEETINGSTARTTIMENOTNULL";
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.Value.Value.ToString()))
            {
                StrEndTime = this.tpEndTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                StrReturn = "MEETINGENDTIMENOTNULL";
            }
           

            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart <= System.DateTime.Now)
            {
                StrReturn = "STARTTIMENOTLESSCURRENTTIME";
            }
            if (DtEnd <= System.DateTime.Now)
            {
                StrReturn = "ENDTIMENOTLESSCURRENTTIME";
            }
            if (DtStart >= DtEnd)
            {
                StrReturn = "STARTTIMENOTGREATENDTIME1";
                //MessageBox.Show("会议开始时间必须小于结束时间");
            }

            return StrReturn;
        }
        #endregion



        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_MEETINGINFO>(tmpMeetingInfo, "OA");

            Utility.SetAuditEntity(entity, "MeetingInfo", tmpMeetingInfo.MEETINGINFOID, strXmlObjectSource);
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
            if (tmpMeetingInfo.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpMeetingInfo.CHECKSTATE = state;
            MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo, UserState);
            
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (tmpMeetingInfo != null)
                state = tmpMeetingInfo.CHECKSTATE;
            if (ActionType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            MeetingClient.DoClose();
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
