using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Collections.ObjectModel;


//using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.OA.UI.CommForm;
//using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.SAAS.Main.CurrentContext;


namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class AddMeeting : ChildWindow
    {

        #region
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        //private MeetingInfoManagementServiceClient MeetingInfoClient = new MeetingInfoManagementServiceClient();
        private ObservableCollection<T_OA_MEETINGCONTENT> MeetingContentList = new ObservableCollection<T_OA_MEETINGCONTENT>();
        private ObservableCollection<T_OA_MEETINGSTAFF> MeetingStaffList = new ObservableCollection<T_OA_MEETINGSTAFF>();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        public delegate void refreshGridView();
        private string strMeetingType = ""; //会议类型
        private string strStartTime = ""; //开始时间 时：分
        private string strEndTime = ""; //结束时间
        private string strMeetingRoom = ""; //会议室
        private string tmpTemplateContent = ""; //模板内容
        //private string tmpInsertContent = "";  //插入会议材料标记，如果为空则成功 非空则失败
        private T_OA_MEETINGINFO tmpMeetingInfo = new T_OA_MEETINGINFO();
        private string StrDepartment = "";  //部门
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        private bool AuditType = false; //是否提交了审核按钮
        private bool SaveAuditType = false; //保存提交审核标志
        //private bool AuditAddFlag = false;//添加时提交审核标志
        //private bool AuditEditFlag = false; //审核修改标志
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpMeetingMember = new List<ExtOrgObj>();
        public event refreshGridView ReloadDataEvent;
        //private bool AuditFlag = false; //审核标志
        private int SelectIndex = 0; //记录开始时间选择的位置


        private Action ActionType ;//动作标记

        private string[] lstring = new string[100];

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        #endregion 
        
        #region 构造函数

        public AddMeeting(Action action, T_OA_MEETINGINFO MeetingT)
        {
            InitializeComponent();
            combox_MeetingRoomSelectSource();
            combox_SelectSource();
            this.GetTimeHour();
            ActionType = action;
            
            
            if (action == Action.Add)
            {
                
                this.dpStartDate.Text = System.DateTime.Now.ToShortDateString();
                this.dpEndDate.Text = System.DateTime.Now.ToShortDateString();
                this.tbltitle.Text = "添加会议信息";
                SVAudit.Visibility = Visibility.Collapsed;
            }
            if (action == Action.Edit)
            {                
                GetMeetingInfoByInfo(MeetingT);
                GetMeetingStaffInfo(MeetingT);
                this.tbltitle.Text = "修改会议信息";
            }
            if (action == Action.AUDIT)
            {
                this.btnAdd.Visibility = Visibility.Collapsed;
                this.btnAddToConfirm.Visibility = Visibility.Collapsed;                
                this.btnCancel.Visibility = Visibility.Collapsed;
                GetMeetingInfoByInfo(MeetingT);
                InitAudit(MeetingT);
                GetMeetingStaffInfo(MeetingT);
                this.tbltitle.Text = "审核会议信息";
            }

            audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            MeetingClient.MeetingInfoAddCompleted += new EventHandler<MeetingInfoAddCompletedEventArgs>(MeetingInfoClient_MeetingInfoAddCompleted);
            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
        }

        /// <summary>
        /// 获取参会人员
        /// </summary>
        /// <param name="MeetingT"></param>
        private void GetMeetingStaffInfo(T_OA_MEETINGINFO MeetingT)
        {
            if (MeetingT != null)
            {
                MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
                MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDAsync(MeetingT.MEETINGINFOID);
            }
        }

        void MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted(object sender, GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs e)
        {
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

            }
            
        }

        /// <summary>
        /// 获取参会人员并填充
        /// </summary>
        private void GetMeetingStaff(ObservableCollection<string> staffs)
        {
            PersonnelServiceClient personclient = new PersonnelServiceClient();
            //personclient.getem
            personclient.GetEmployeeByIDsCompleted += new EventHandler<GetEmployeeByIDsCompletedEventArgs>(personclient_GetEmployeeByIDsCompleted);
            personclient.GetEmployeeByIDsAsync(staffs);
            
        }

        void personclient_GetEmployeeByIDsCompleted(object sender, GetEmployeeByIDsCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (e.Result != null)
            {
                List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> employee = e.Result.ToList();
                cbxMeetingMembers.ItemsSource = employee;
                cbxMeetingMembers.DisplayMemberPath = "EMPLOYEECNAME";
                cbxMeetingMembers.SelectedIndex = 0;
            }
        }


        private void InitAudit(T_OA_MEETINGINFO MeetingInfo)
        {
            SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
            entity.ModelCode = "MeetingInfo";
            entity.FormID = MeetingInfo.MEETINGINFOID;
            entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
            entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
            entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
            entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
            audit.BindingData();
        }

        
        
        #region 设置时间
        // 设置开始时间和结束时间的时间段 
        // 设置半小时为一时间段 
        private void GetTimeHour()
        {
            string[] strTime = new string[48];
            int intk = 0;
            for (int i = 0; i < 48; i++)
            {


                if (i % 2 == 0)
                {
                    intk = intk + 1;
                    if (intk == 24)
                    {
                        intk = 0;
                    }
                    strTime[i] = intk.ToString() + ":00";

                }
                else
                {
                    if (intk == 24)
                    {
                        intk = 0;
                    }
                    strTime[i] = intk.ToString() + ":30";
                }

            }
            
            string StrHour = System.DateTime.Now.Hour.ToString();
            int IntMinute = System.DateTime.Now.Minute;
            string StrMinute = "";
            string StrInitStart = "";
            if (IntMinute < 30)
            {
                StrMinute = ":00";
            }
            else
            {
                StrMinute = ":30";
            }
            StrInitStart = StrHour + StrMinute;
            this.tpStartTime.ItemsSource = strTime;
            this.tpEndTime.ItemsSource = strTime;
            
            if (strStartTime == "")
            {

                
                int StartIndex = 0;
                
                foreach (var time in strTime)
                {
                    StartIndex++;
                    if (time == StrInitStart)
                    {
                        this.tpStartTime.SelectedIndex = StartIndex;
                        SelectIndex = StartIndex + 1;
                        break;
                    }
                }
                
            }
            else
            {
                this.tpStartTime.SelectedItem = strStartTime;
            }
            if (strEndTime == "")
            {
                this.tpEndTime.SelectedIndex = SelectIndex;

            }
            else
            {
                this.tpEndTime.SelectedItem = strEndTime;
            }
        }

        #endregion

        

        private void GetMeetingInfoByInfo(T_OA_MEETINGINFO MeetingInfoT)
        {
            if (MeetingInfoT != null)
            {
                tmpMeetingInfo = MeetingInfoT;
                //cbxMeetingType.SelectedItem = MeetingInfoT.MEETINGTYPE;
                //cbMeetingRoom.SelectedItem = MeetingInfoT.MEETINGROOMNAME;
                
                tbxCheckState.Text = MeetingInfoT.CHECKSTATE;
                tbxCreateDate.Text = MeetingInfoT.CHECKSTATE.ToString();
                tbxCreatUserID.Text = MeetingInfoT.CREATEUSERID;
                tbxIsCancel.Text = MeetingInfoT.ISCANCEL;
                tbxMeetingTitle.Text = MeetingInfoT.MEETINGTITLE;
                tbxMeetingContent.RichTextBoxContext = MeetingInfoT.CONTENT;
                dpStartDate.Text = Convert.ToDateTime(MeetingInfoT.STARTTIME).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(MeetingInfoT.ENDTIME).ToShortDateString();

                strStartTime = Convert.ToDateTime(MeetingInfoT.STARTTIME).ToShortTimeString();
                strEndTime = Convert.ToDateTime(MeetingInfoT.ENDTIME).ToShortTimeString();
                //strMeetingType = MeetingInfoT.MEETINGTYPE;
                //strMeetingRoom = MeetingInfoT.MEETINGROOMNAME;
                this.tbxMeetingInfoID.Text = MeetingInfoT.MEETINGINFOID;
                GetTimeHour();
                //tpEndTime.SelectedItem = MeetingInfoT.ENDTIME.ToShortTimeString();
            }
            
        }
        #endregion

        #region COMBOX 设置数据源

        private void combox_SelectSource()
        {
            //MeetingTypeManagementServiceClient typeClient = new MeetingTypeManagementServiceClient();
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);

        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbxMeetingType.Items.Clear();
                this.cbxMeetingType.ItemsSource = e.Result;
                if (strMeetingType == "")
                {
                    this.cbxMeetingType.SelectedIndex = 0;//默认选中第一项
                }
                else
                {
                    this.cbxMeetingType.SelectedItem = strMeetingType;
                }
            }
        }

        private void combox_MeetingRoomSelectSource()
        {
            //MeetingManagementServiceClient MeetingRoomClient = new MeetingManagementServiceClient();
            MeetingClient.GetMeetingRoomNameInfosToComboxCompleted += new EventHandler<GetMeetingRoomNameInfosToComboxCompletedEventArgs>(MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted);
            MeetingClient.GetMeetingRoomNameInfosToComboxAsync();
            
        }

        void MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted(object sender, GetMeetingRoomNameInfosToComboxCompletedEventArgs e)
        { 
            if (e.Result != null)
            {
                this.cbMeetingRoom.Items.Clear();
                
                
                
                this.cbMeetingRoom.ItemsSource = e.Result;
                if (strMeetingRoom == "")
                {
                    this.cbMeetingRoom.SelectedIndex = 0;
                }
                else
                {
                    this.cbMeetingRoom.SelectedItem = strMeetingRoom;
                }
            }

        }

       

        #endregion

        #region 取消按钮事件
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
            string StrContent = ""; //会议类容
            int StrCount = 0; //会议人数，由选择与会人员数量确定
            string StrMeetingRoom = ""; //会议室名

            string StrError = "";  //错误提示信息
            bool blError = true;  //是否正确

            if (this.cbMeetingRoom.SelectedIndex > 0)
            {
                StrMeetingRoom = this.cbMeetingRoom.SelectedItem.ToString();
            }
            else
            {
                StrError += "必须选择会议室\n";
                blError = false;
            }

            if (string.IsNullOrEmpty(StrDepartment))
            {
                StrError += "部门不能为空\n";
                blError = false;
            }

            StrMeetingTypeName = this.cbxMeetingType.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(this.dpStartDate.Text.ToString()))
            {
                StrStartDt = this.dpStartDate.Text.ToString();
            }
            else
            {
                StrError += "会议开始时间不能为空\n";
                blError = false;


            }
            if (!string.IsNullOrEmpty(this.dpEndDate.Text.ToString()))
            {
                StrEndDt = this.dpEndDate.Text.ToString();

            }
            else
            {
                StrError += "会议结束时间不能为空\n";
                blError = false;
                //return;
            }
            if (!string.IsNullOrEmpty(this.tbxMeetingTitle.Text.ToString()))
            {
                StrMeetingTitle = this.tbxMeetingTitle.Text.ToString();
            }
            else
            {
                StrError += "会议标题不能为空\n";
                blError = false;
            }
            //if (!string.IsNullOrEmpty(this.tbxMeetingContent.Text.ToString()))
            //{
            //    StrContent = HttpUtility.HtmlEncode(this.tbxMeetingContent.Text.ToString());
            //}
            //else
            //{
            //    StrError += "会议内容不能为空！\n";
            //    blError = false;
            //    //MessageBox.Show("会议内容不能为空！");
            //}
            if (!string.IsNullOrEmpty(this.tpStartTime.SelectedItem.ToString()))
            {
                StrStartTime = this.tpStartTime.SelectedItem.ToString();
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.SelectedItem.ToString()))
            {
                StrEndTime = this.tpEndTime.SelectedItem.ToString();
            }

            StrCount = cbxMeetingMembers.Items.Count;
            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart >= DtEnd)
            {
                StrError += "会议开始时间必须小于结束时间！\n";
                blError = false;
            }
            if (!(StrCount > 0))
            {
                StrError += "请选择与会人员！\n";
                blError = false;
            }
            if (blError)
            {
                if (ActionType == Action.Add)
                {
                    tmpMeetingInfo.MEETINGINFOID = System.Guid.NewGuid().ToString();
                    //tmpMeetingInfo.MEETINGROOMNAME = StrMeetingRoom;
                    tmpMeetingInfo.MEETINGTITLE = StrMeetingTitle;
                    //tmpMeetingInfo.MEETINGTYPE = StrMeetingTypeName;
                    tmpMeetingInfo.DEPARTNAME = StrDepartment;
                    //tmpMeetingInfo.CONTENT = StrContent;
                    tmpMeetingInfo.COUNT = System.Convert.ToInt16(StrCount);
                    tmpMeetingInfo.STARTTIME = DtStart;
                    tmpMeetingInfo.ENDTIME = DtEnd;
                    tmpMeetingInfo.ISCANCEL = "1";

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
                    
                    try
                    {

                        
                        //MeetingClient.MeetingInfoAddAsync(tmpMeetingInfo);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                if (ActionType == Action.Edit)
                {
                    tmpMeetingInfo.MEETINGINFOID = tbxMeetingInfoID.Text.ToString();
                    //tmpMeetingInfo.MEETINGROOMNAME = StrMeetingRoom;
                    tmpMeetingInfo.MEETINGTITLE = StrMeetingTitle;
                    //tmpMeetingInfo.MEETINGTYPE = StrMeetingTypeName;
                    tmpMeetingInfo.DEPARTNAME = StrDepartment;
                    //tmpMeetingInfo.CONTENT = StrContent;
                    tmpMeetingInfo.COUNT = System.Convert.ToInt16(StrCount);
                    tmpMeetingInfo.STARTTIME = DtStart;
                    tmpMeetingInfo.ENDTIME = DtEnd;
                    tmpMeetingInfo.ISCANCEL = "0";
                    tmpMeetingInfo.CHECKSTATE = "0"; //申请状态
                    tmpMeetingInfo.CREATEDATE = System.Convert.ToDateTime(dpStartDate.Text.ToString());
                    tmpMeetingInfo.CREATEUSERID = tbxCreatUserID.Text.ToString();
                    tmpMeetingInfo.UPDATEDATE = System.DateTime.Now;
                    tmpMeetingInfo.UPDATEUSERID = "1";

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

                    try
                    {                        
                        
                        //MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show(StrError);
            }
        }

        #endregion

        #region 添加按钮事件
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //ActionType
            //ActionType = Action.Add;
            this.SaveMeetingInfo();

        }


        void MeetingInfoClient_MeetingInfoAddCompleted(object sender, MeetingInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    
                    AddMeetingContentInfo();
                    //与会人员是在知道自己纳入会员后上传附件
                    AddMeetingStaffInfo();
                    checkRoomIsUsing();
                    
                    AddMeetingStaffInfo();
                    if (AuditType) //添加并提交审核
                    {
                        SumbitFlow();
                        SaveAuditType = true;
                        AuditType = false;
                    }
                    else
                    {
                        MessageBox.Show("添加会议信息成功！");
                        this.Close();
                        this.ReloadDataEvent();
                    }
                    
                    
                }
                else
                {
                    MessageBox.Show("添加会议信息"+e.Result.ToString());

                }
            }
            else
            {
                MessageBox.Show("输入添加时遇到错误，请与管理员联系");
            }
        }


        void MeetingInfoClient_SubmitFlowCompleted(object sender, SubmitFlowCompletedEventArgs e)
        { 

        }

        #endregion

        #region 修改按钮事件

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            
            this.SaveMeetingInfo();
        }

        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            string bbb = "";
            if (!e.Cancelled)
            {
                if (e.Result == 1)
                {
                    if (ActionType == Action.AUDIT)
                    {
                        MessageBox.Show("审核成功");
                        this.ReloadData();
                        this.Close();
                    }
                    if (AuditType)
                    {
                        SumbitFlow();
                        AuditType = false;
                    }
                    else
                    {
                        if (SaveAuditType)
                        {
                            MessageBox.Show("操作成功");
                        }
                        else
                        {
                            MessageBox.Show("会议信息修改成功！");
                        }

                        this.ReloadData();
                        this.Close();
                    }
                }
                //需要将修改记录添加到会议修改表中

            }
            else
            {
                MessageBox.Show(e.ToString());
            }

        }

        #endregion 

        #region 添加与会人员信息
        void AddMeetingStaffInfo()
        {
            
            //MeetingStaffManagementServiceClient MemberClient = new MeetingStaffManagementServiceClient();
            if (tmpMeetingMember != null)
            {
                foreach (var MeetingMember in tmpMeetingMember)
                {
                    T_OA_MEETINGSTAFF StaffT = new T_OA_MEETINGSTAFF();
                    StaffT.MEETINGSTAFFID = System.Guid.NewGuid().ToString();
                    StaffT.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
                    StaffT.CREATEUSERID = tmpMeetingInfo.CREATEUSERID;
                    StaffT.CONFIRMFLAG = "0";
                    StaffT.FILENAME = "";
                    StaffT.MEETINGUSERID = MeetingMember.ObjectID.ToString();// ArrUsers[i].ToString();
                    StaffT.CREATEDATE = Convert.ToDateTime(tmpMeetingInfo.CREATEDATE);
                    StaffT.UPDATEDATE = null;
                    StaffT.UPDATEUSERID = "";
                    StaffT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    StaffT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    StaffT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    StaffT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    StaffT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    StaffT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    StaffT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    StaffT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    StaffT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    StaffT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    
                    MeetingStaffList.Add(StaffT);
                }
            }            

            MeetingClient.BatchAddMeetingStaffInfosCompleted += new EventHandler<BatchAddMeetingStaffInfosCompletedEventArgs>(MemberClient_BatchAddMeetingStaffInfosCompleted);
            MeetingClient.BatchAddMeetingStaffInfosAsync(MeetingStaffList);
            
            
        }

        void MemberClient_BatchAddMeetingStaffInfosCompleted(object sender, BatchAddMeetingStaffInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    MessageBox.Show(e.Result.ToString());
                }
            }
        }

        void MemberClient_MeetingStaffInfoAddCompleted(object sender,MeetingStaffInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    MessageBox.Show("与会人员"+e.Result.ToString());
                }
            }
        }
        #endregion

        #region 会议室是否被占用

        private void checkRoomIsUsing()
        {
            
            MeetingClient.IsExistMeetingRoomJustUsingCompleted += new EventHandler<IsExistMeetingRoomJustUsingCompletedEventArgs>(RoomApp_IsExistMeetingRoomJustUsingCompleted);
            //MeetingClient.IsExistMeetingRoomJustUsingAsync(tmpMeetingInfo.MEETINGROOMNAME, tmpMeetingInfo.STARTTIME, tmpMeetingInfo.ENDTIME);
            
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
            
            if (cbxMeetingTypeTemplate.SelectedIndex > 0)
            {
                //StrTemplateName = this.cbxMeetingTypeTemplate.SelectedItem.ToString();
                T_OA_MEETINGTEMPLATE TemplateT = this.cbxMeetingTypeTemplate.SelectedItem as T_OA_MEETINGTEMPLATE;
                StrTemplateName = TemplateT.TEMPLATENAME;
                //tmpTemplateContent = TemplateT.CONTENT;                
            }

            if (tmpMeetingMember != null)
            {
                foreach (var MeetingMember in tmpMeetingMember)
                {
                    T_OA_MEETINGCONTENT ContentT = new T_OA_MEETINGCONTENT();
                    ContentT.MEETINGCONTENTID = System.Guid.NewGuid().ToString();
                    ContentT.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;
                    ContentT.MEETINGUSERID = MeetingMember.ObjectID;
                    ContentT.CREATEDATE = Convert.ToDateTime(tmpMeetingInfo.CREATEDATE);
                    ContentT.CREATEUSERID = tmpMeetingInfo.CREATEUSERID;
                    ContentT.UPDATEDATE = tmpMeetingInfo.UPDATEDATE;
                    ContentT.UPDATEUSERID = tmpMeetingInfo.UPDATEUSERID;
                    ContentT.CONTENT = tmpTemplateContent;

                    ContentT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ContentT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    ContentT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    ContentT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    ContentT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    ContentT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    ContentT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    ContentT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    ContentT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    ContentT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;

                    MeetingContentList.Add(ContentT);
                }
            }      

            MeetingClient.BatchAddMeetingContentInfosCompleted += new EventHandler<BatchAddMeetingContentInfosCompletedEventArgs>(MeetingContentClient_BatchAddMeetingContentInfosCompleted);
            MeetingClient.BatchAddMeetingContentInfosAsync(MeetingContentList);
            
        }

        void MeetingContentClient_BatchAddMeetingContentInfosCompleted(object sender, BatchAddMeetingContentInfosCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    MessageBox.Show("会议内容"+e.Result.ToString());
                }
            }
        }
        

        //void MeetingContentClient_MeetingContentAddCompleted(object sender, MeetingContentAddCompletedEventArgs e)
        //{
        //    if (!e.Cancelled)
        //    {
        //        if (e.Result != "")
        //        {
        //            tmpInsertContent = e.Result.ToString();
        //        }
        //    }

        //}
        #endregion

        #region 会议类型改变
        private void cbxMeetingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string StrSelMeetingType = "";
            StrSelMeetingType = this.cbxMeetingType.SelectedItem.ToString();
            MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted += new EventHandler<GetMeetingTypeTemplateNameInfosByMeetingTypeCompletedEventArgs>(TemplateClient_GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted);
            MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeAsync(StrSelMeetingType);

        }

        void TemplateClient_GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted(object sender, GetMeetingTypeTemplateNameInfosByMeetingTypeCompletedEventArgs e)
        {

            if (e.Result != null)
            {
                List<T_OA_MEETINGTEMPLATE> tmpTemplate = e.Result.ToList();
                T_OA_MEETINGTEMPLATE TemplateT = new T_OA_MEETINGTEMPLATE();
                TemplateT.MEETINGTEMPLATEID = "";
                TemplateT.TEMPLATENAME = "请选择";
                tmpTemplate.Insert(0,TemplateT);
                this.cbxMeetingTypeTemplate.ItemsSource = tmpTemplate;
                
                this.cbxMeetingTypeTemplate.DisplayMemberPath = "TEMPLATENAME";                
                this.cbxMeetingTypeTemplate.SelectedIndex = 0;              
                
            }

        }
        #endregion       

        #region 选择部门

        private void PostsObject_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Department;


            lookup.SelectedClick += (obj, ev) =>
            {
                PostsObject.DataContext = lookup.SelectedObj;
                if (lookup.SelectedObj is SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT tmp = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT;
                    //MeetingInfoT.POSTID = tmp.T_HR_POSTDICTIONARY.POSTLEVEL;
                    //MeetingInfoT.CREATEDEPARTMENTID = tmp.DEPARTMENTID;
                    StrDepartment = tmp.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                }
            };
            lookup.Show();
        }


        private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);

        }
        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                    department = e.Result;
                    //StrCompanyId = department.DEPARTMENTID;
                    PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    PostsObject.DataContext = department;
                }
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
                    //this.tbxMeetingContent.Text = template.CONTENT;
                }
            }
        }
        
        #endregion

        #region 人员选择


        private void btnFindMember_Click(object sender, RoutedEventArgs e)
        {
            OrganizationLookup lookupmember = new OrganizationLookup();
            
            
            lookupmember.MultiSelected = true;
            lookupmember.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookupmember.SelectedClick += (o, ev) =>
            {
                //SMT.SaaS.FrameworkUI.Common.OrganizationWS.T_HR_POST ent = lookupmember.SelectedObj[0].ObjectInstance as SMT.SaaS.FrameworkUI.Common.OrganizationWS.T_HR_POST;
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookupmember.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null)
                {
                    tmpMeetingMember = ent;
                    HandlePostChanged(ent);
                }
                else
                {
                    tmpMeetingMember = null;
                }

            };
           
            lookupmember.ShowDialog();
            //lookupmember.Show();
        }

        private void HandlePostChanged(List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent)
        { 
            
            if (ent != null)
            {
                this.cbxMeetingMembers.ItemsSource = ent;
                this.cbxMeetingMembers.DisplayMemberPath = "ObjectName";
                this.cbxMeetingMembers.SelectedIndex = 0;
            }
        }

        private void GetAllPost(SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST ent)
        {
            if (ent != null && ent.EMPLOYEEPOSTS[0] != null)
            {
                //MembersObject.TxtLookUp.Text = ent.EMPLOYEEPOSTS[0].T_HR_EMPLOYEE.EMPLOYEECNAME; //员工姓名
                //txtPostsId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;//岗位
                //txtCompanyId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;//公司
                //txtDepartmentId.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;//部门
            }
        }
        #endregion

        #region 提交流程

        private void SumbitFlow()
        {
            if (tmpMeetingInfo != null)
            {
                SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
                entity.ModelCode = "MeetingInfo"; //会议申请模块
                entity.FormID = tmpMeetingInfo.MEETINGINFOID;
                entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
                entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
                entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
                entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
                entity.Content = "aaaaa";
                //audit.AuditEntity = entity;  
                audit.Submit();
            }
        }
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
                    AuditType = false;
                    tmpMeetingInfo.UPDATEDATE = DateTime.Now;
                    tmpMeetingInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpMeetingInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
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
                    SaveAuditType = true;
                    MeetingClient.MeetingInfoUpdateAsync(tmpMeetingInfo);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void Cancel()
        {
            //this.DialogResult = true;
            this.Close();
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            this.Close();
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
    }
}

