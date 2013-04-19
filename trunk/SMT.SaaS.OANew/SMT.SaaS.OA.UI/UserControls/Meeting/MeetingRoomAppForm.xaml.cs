using System;
using System.Collections.Generic;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
//using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;

using SMT.SaaS.FrameworkUI;
//using SMT.SaaS.OA.UI.MeetingWs;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.OA.UI.Class;
namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MeetingRoomAppForm : BaseForm,IClient, IEntityEditor,IAudit
    {
        public MeetingRoomAppForm()
        {
            InitializeComponent();
        }

        #region  初始化数据和变量

        
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        public delegate void refreshGridView();
        //private string strMeetingType = ""; //会议类型        
        private string strStartTime = ""; //开始时间 时：分
        private string strEndTime = ""; //结束时间
        private T_OA_MEETINGROOM SelectedMeetingRoom = new T_OA_MEETINGROOM(); //会议室
        private string StrDepartmentName = ""; //部门名
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭
        private bool IsSuccess = false; //成功操作开关， 添加成功  修改成功
        private string ActionType = ""; //动作状态  Add 添加 Edit 修改
        private bool AuditFlag = false; //审核标志 默认不提交审核 
        private bool AuditAddFlag = false;//添加时提交审核标志
        private bool AuditEditFlag = false; //审核修改标志
        private FormTypes actions;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private int SelectIndex = 0; //记录开始时间选择的位置
        private T_OA_MEETINGROOMAPP tmpRoomTimeT = new T_OA_MEETINGROOMAPP();
        //private T_OA_MEETINGROOMAPP AuditRoomapp = new T_OA_MEETINGROOMAPP();
        public event refreshGridView ReloadDataEvent;
        private string StrDepartmentID = ""; //所属部门
        
        

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        private void InitEvent()
        {
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            MeetingClient.MeetingRoomAppInfoAddCompleted += new EventHandler<MeetingRoomAppInfoAddCompletedEventArgs>(RoomAppClient_MeetingRoomAppInfoAddCompleted);
            MeetingClient.MeetingRoomAppUpdateCompleted += new EventHandler<MeetingRoomAppUpdateCompletedEventArgs>(MeetingClient_MeetingRoomAppUpdateCompleted);
            MeetingClient.GetMeetingRoomNameInfosToComboxCompleted += new EventHandler<GetMeetingRoomNameInfosToComboxCompletedEventArgs>(MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted);
            MeetingClient.MeetingRoomTimeChangeAddCompleted += new EventHandler<MeetingRoomTimeChangeAddCompletedEventArgs>(RoomTimeClient_MeetingRoomTimeChangeAddCompleted);
            MeetingClient.GetMeetingRoomAppSingleInfoByAppIdCompleted += new EventHandler<GetMeetingRoomAppSingleInfoByAppIdCompletedEventArgs>(MeetingClient_GetMeetingRoomAppSingleInfoByAppIdCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void MeetingClient_GetMeetingRoomAppSingleInfoByAppIdCompleted(object sender, GetMeetingRoomAppSingleInfoByAppIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        GetMeetingRoomAppInfo(e.Result,e.Result.T_OA_MEETINGROOM);
                        tmpRoomTimeT = e.Result;
                        StrDepartmentID = tmpRoomTimeT.OWNERDEPARTMENTID;
                                                
                        //this.audititem.Visibility = Visibility.Visible;
                        if (actions == FormTypes.Audit)
                        {
                            //audit.XmlObject = DataObjectToXml<T_OA_MEETINGROOMAPP>.ObjListToXml(tmpRoomTimeT, "OA"); 
                        }
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),e.Error.ToString());
                    return;
                }
            }
            //throw new NotImplementedException();
        }

        #endregion

        #region 构造函数

        public MeetingRoomAppForm(FormTypes actionenum, T_OA_MEETINGROOMAPP MeetingRoomAppT,T_OA_MEETINGROOM Room)
        {
            InitializeComponent();
            InitEvent();
            actions = actionenum;
            switch (actionenum)
            { 
                case FormTypes.New:
                    tmpRoomTimeT.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                    this.RowEditResult.Height = new GridLength(0);                    
                    this.dpStartDate.Text = System.DateTime.Now.ToShortDateString();
                    this.dpEndDate.Text = System.DateTime.Now.ToShortDateString();
                    this.tpStartTime.Value = System.DateTime.Now;
                    this.tpEndTime.Value = System.DateTime.Now.AddMinutes(30);
                    combox_MeetingRoomSelectSource();
                    StrDepartmentName = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                    StrDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    CompanyObject.Text = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                    break;
                case FormTypes.Edit:                    
                    tmpRoomTimeT = MeetingRoomAppT;
                    MeetingClient.GetMeetingRoomAppSingleInfoByAppIdAsync(MeetingRoomAppT.MEETINGROOMAPPID);                    
                    break;
                case FormTypes.Resubmit:
                    tmpRoomTimeT = MeetingRoomAppT;
                    MeetingClient.GetMeetingRoomAppSingleInfoByAppIdAsync(MeetingRoomAppT.MEETINGROOMAPPID);                    
                    break;
                case FormTypes.Browse:
                    GetMeetingRoomAppInfo(MeetingRoomAppT, Room);
                    tmpRoomTimeT = MeetingRoomAppT;                    
                    this.RowEditResult.MaxHeight = 0;
                    if (MeetingRoomAppT.CHECKSTATE == "1")
                    {
                        //this.audititem.Visibility = Visibility.Visible;
                    }
                    SetReadOnly();                    
                    break;
                case FormTypes.Audit:
                    GetMeetingRoomAppInfo(MeetingRoomAppT, Room);
                    tmpRoomTimeT = MeetingRoomAppT;
                    //this.SPResult.Visibility = Visibility.Collapsed;
                    this.RowEditResult.MaxHeight = 0;
                    //this.audititem.Visibility = Visibility.Visible;
                    SetReadOnly();
                    //audit.XmlObject = DataObjectToXml<T_OA_MEETINGROOMAPP>.ObjListToXml(tmpRoomTimeT, "OA"); 
                    
                    break;
            }
            

        }

        /// <summary>
        /// 门户调用审核
        /// </summary>
        /// <param name="actionenum"></param>
        /// <param name="MeetingRoomAppT"></param>
        /// <param name="Room"></param>
        public MeetingRoomAppForm(FormTypes actionenum, string StrRoomAppID)
        {
            InitializeComponent();
            InitEvent();
            actions = actionenum;            
            MeetingClient.GetMeetingRoomAppSingleInfoByAppIdAsync(StrRoomAppID);            
            SetReadOnly();
        }

        private void  SetReadOnly()
        {
            this.txtTel.IsEnabled = false;
            this.tpEndTime.IsEnabled = false;
            this.tpStartTime.IsEnabled = false;
            this.CompanyObject.IsEnabled = false;
            this.dpEndDate.IsEnabled = false;
            this.dpStartDate.IsEnabled = false;
            this.txtEditResult.IsEnabled = false;
            this.cbMeetingRoom.IsEnabled = false;
            this.btnLookUpPartyb.IsEnabled = false;

        }

        //private void InitAudit(T_OA_MEETINGROOMAPP MeetingRoomAppT)
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "MeetingRoomApp";
        //    entity.FormID = MeetingRoomAppT.MEETINGROOMAPPID;
        //    entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //    entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //    entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //    entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    audit.BindingData();
        //}


        

        private void combox_MeetingRoomSelectSource()
        {

            MeetingClient.GetMeetingRoomNameInfosToComboxAsync();

        }

        void MeetingRoomClient_GetMeetingRoomNameInfosToComboxCompleted(object sender, GetMeetingRoomNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbMeetingRoom.Items.Clear();
                this.cbMeetingRoom.ItemsSource = e.Result;
                this.cbMeetingRoom.DisplayMemberPath = "MEETINGROOMNAME";

                if (SelectedMeetingRoom.MEETINGROOMID != null)
                {
                    foreach (var item in cbMeetingRoom.Items)
                    {
                        T_OA_MEETINGROOM dict = item as T_OA_MEETINGROOM;
                        if (dict != null)
                        {
                            if (dict.MEETINGROOMNAME == SelectedMeetingRoom.MEETINGROOMNAME)
                            {
                                cbMeetingRoom.SelectedItem = item;
                                break;
                            }
                        }
                    }

                }
                else
                {
                    cbMeetingRoom.SelectedIndex = 0;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("FOUNDMEETINGROOM"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }

        }



        private void GetMeetingRoomAppInfo(T_OA_MEETINGROOMAPP RoomAppT,T_OA_MEETINGROOM RoomObj)
        {
            if (RoomAppT != null)
            {

                //cbMeetingRoom.SelectedItem = RoomAppT.MEETINGROOMNAME;
                
                
                txtTel.Text = RoomAppT.TEL;
                dpStartDate.Text = Convert.ToDateTime(RoomAppT.STARTTIME).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(RoomAppT.ENDTIME).ToShortDateString();

                //strStartTime = RoomAppT.STARTTIME.ToShortTimeString();
                //strEndTime = RoomAppT.ENDTIME.ToShortTimeString();

                tpStartTime.Value = RoomAppT.STARTTIME;
                tpEndTime.Value = RoomAppT.ENDTIME;

                SelectedMeetingRoom = RoomObj;
                CompanyObject.Text = RoomAppT.DEPARTNAME;
                combox_MeetingRoomSelectSource();
                //GetDepartmentNameByDepartmentID(RoomAppT.DEPARTNAME);
                //tpEndTime.SelectedItem = RoomAppT.ENDTIME.ToShortTimeString();
            }
            if (actions == FormTypes.Audit || actions == FormTypes.Browse)
            {
                this.Loaded += new RoutedEventHandler(MeetingRoomAppForm_Loaded);
                
            }

        }

        void MeetingRoomAppForm_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
        }


        #endregion


        #region 添加按钮事件


        private void SaveMeetingRoomApp()
        {
            string StrDepartment = "";  //部门            
            string StrStartDt = "";   //开始时间

            string StrStartTime = ""; //开始时：分
            string StrEndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            //string StrMeetingRoom = ""; //会议室名
            //string StrError = "";  //错误提示信息
            bool blError = true;  //是否正确
            string StrTel = ""; //联系电话

            //StrMeetingRoom = this.cbMeetingRoom.SelectedItem.ToString();

            StrTel = this.txtTel.Text.Trim().ToString();
            StrDepartmentName = this.CompanyObject.Text.Trim().ToString();
            if (string.IsNullOrEmpty(StrDepartmentName))
            {
                //StrError += "公司部门不能为空\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTDEPART"));
                
                return;
            }

            if (!string.IsNullOrEmpty(this.dpStartDate.Text.ToString()))
            {
                StrStartDt = this.dpStartDate.Text.ToString();
            }
            else
            {             
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                this.dpStartDate.Focus();
                return;

            }
            if (!string.IsNullOrEmpty(this.dpEndDate.Text.ToString()))
            {
                StrEndDt = this.dpEndDate.Text.ToString();

            }
            else
            {                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                this.dpEndDate.Focus();
                return;
                
            }

            if (!string.IsNullOrEmpty(this.tpStartTime.Value.ToString()))
            {
                StrStartTime = this.tpStartTime.Value.Value.ToString("HH:mm");

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                this.tpStartTime.Focus();
                return;
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.Value.ToString()))
            {
                StrEndTime = this.tpEndTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                this.tpEndTime.Focus();
                return;
            }


            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart >= DtEnd)
            {               
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                return;
               
            }
            if (cbMeetingRoom.SelectedIndex > -1)
            {                    
                SelectedMeetingRoom = cbMeetingRoom.SelectedItem as T_OA_MEETINGROOM;
                //StrMeetingType = SelectMeetingType.MEETINGTYPEID.ToString();
            }
            RefreshUI(RefreshedTypes.ShowProgressBar);
            if (actions == FormTypes.New)
            {
                tmpRoomTimeT.MEETINGROOMAPPID = System.Guid.NewGuid().ToString();                    
                tmpRoomTimeT.T_OA_MEETINGROOM = SelectedMeetingRoom;
                tmpRoomTimeT.DEPARTNAME = StrDepartmentName;
                tmpRoomTimeT.STARTTIME = DtStart;
                tmpRoomTimeT.ENDTIME = DtEnd;
                tmpRoomTimeT.CHECKSTATE = "0";  //申请状态
                //tmpRoomTimeT.CREATEDATE = System.DateTime.Now;
                tmpRoomTimeT.ISCANCEL = "1";
                tmpRoomTimeT.TEL = StrTel;
                
                tmpRoomTimeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                tmpRoomTimeT.OWNERDEPARTMENTID = StrDepartmentID;// Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                tmpRoomTimeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                tmpRoomTimeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpRoomTimeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpRoomTimeT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                tmpRoomTimeT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpRoomTimeT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpRoomTimeT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                tmpRoomTimeT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                tmpRoomTimeT.UPDATEUSERNAME = "";
                tmpRoomTimeT.UPDATEDATE = null;
                tmpRoomTimeT.UPDATEUSERID = "";
                                    

                try
                {                    
                    MeetingClient.MeetingRoomAppInfoAddAsync(tmpRoomTimeT,"Add");                    
                }
                catch (Exception ex)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                    string _text = "";
                    MessageWindow.Show<string>("", ex.ToString(), MessageIcon.Error, result => _text = result, "Default", Utility.GetResourceStr("CONFIRM"));
                }
            }
            if (actions == FormTypes.Edit || actions== FormTypes.Resubmit)
            {
                
                tmpRoomTimeT.DEPARTNAME = StrDepartmentName;
                tmpRoomTimeT.STARTTIME = DtStart;
                tmpRoomTimeT.ENDTIME = DtEnd;
                tmpRoomTimeT.TEL = StrTel;
                tmpRoomTimeT.T_OA_MEETINGROOM = SelectedMeetingRoom;
                tmpRoomTimeT.CHECKSTATE = "0";
                tmpRoomTimeT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                //tmpRoomTimeT.UPDATEDATE = System.DateTime.Now;
                tmpRoomTimeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID; ;
                


                try
                {                    
                    MeetingClient.MeetingRoomAppUpdateAsync(tmpRoomTimeT,"Edit");                    
                }
                catch (Exception ex)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Utility.ShowCustomMessage(MessageTypes.Message,Utility.GetResourceStr("ERROR"),ex.ToString());                    
                }
            }

            
        }
        


        void RoomAppClient_MeetingRoomAppInfoAddCompleted(object sender, MeetingRoomAppInfoAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Result == "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                        if (GlobalFunction.IsSaveAndClose(saveType))
                        {
                            RefreshUI(saveType);
                        }
                        else
                        {
                            actions = FormTypes.Edit;
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }


                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.Result.ToString(), "MEETINGROOMAPP"));
                        return;

                    }
                }
                else
                {
                    //MessageBox.Show("输入添加时遇到错误，请与管理员联系");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
            
        }


        #endregion

        #region 修改按钮事件

        
        void MeetingClient_MeetingRoomAppUpdateCompleted(object sender, MeetingRoomAppUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (!e.Cancelled)
                {
                    if (e.Error == null)
                    {
                        if (e.Result != "")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.Result.ToString(), "MEETINGROOMAPP"));
                        }
                        else
                        {
                            if (e.UserState.ToString() == "Edit")
                            {
                                InsertRoomTimeChangeInfo();
                                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "COMPANYDOC"));
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                                if (GlobalFunction.IsSaveAndClose(saveType))
                                {
                                    RefreshUI(saveType);
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
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message.ToString());
                    }
                    

                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"),ex.Message.ToString());
            }
        }


        #endregion

        #region 改变会议室时间
        //将会议室变更记录添加到 MEETINGROOMTIMECHANGE
        void InsertRoomTimeChangeInfo()
        {

            T_OA_MEETINGROOMTIMECHANGE MeetingTimeT = new T_OA_MEETINGROOMTIMECHANGE();
            MeetingTimeT.MEETINGROOMTIMECHANGEID = System.Guid.NewGuid().ToString();
            MeetingTimeT.T_OA_MEETINGROOMAPP = tmpRoomTimeT;
            MeetingTimeT.REASON = this.txtEditResult.Text.Trim().ToString();
            MeetingTimeT.STARTTIME = Convert.ToDateTime(tmpRoomTimeT.STARTTIME);
            MeetingTimeT.ENDTIME = Convert.ToDateTime(tmpRoomTimeT.ENDTIME);


            MeetingTimeT.UPDATEDATE = null;
            MeetingTimeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            MeetingTimeT.UPDATEUSERNAME = "";

            MeetingTimeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            MeetingTimeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            MeetingTimeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            MeetingTimeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            MeetingTimeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            MeetingTimeT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            MeetingTimeT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            MeetingTimeT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            MeetingTimeT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            MeetingTimeT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            MeetingTimeT.CREATEDATE = System.DateTime.Now;

            try
            {                
                MeetingClient.MeetingRoomTimeChangeAddAsync(MeetingTimeT);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }

        }

        void RoomTimeClient_MeetingRoomTimeChangeAddCompleted(object sender, MeetingRoomTimeChangeAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    
                    //RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
               
            }
            else
            {
                //MessageBox.Show("输入添加时遇到错误，请与管理员联系");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ADDFAILED"), Utility.GetResourceStr("ADDFAILED", "MEETINGROOMTIME"));
            }

        }

        #endregion       

        #region 选择部门

        private void CompanyObject_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    tmpRoomTimeT.DEPARTNAME = companyInfo.ObjectName;
                    StrDepartmentName = companyInfo.ObjectName;
                    StrDepartmentID = companyInfo.ObjectID;
                    CompanyObject.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }


        private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);
            //PostsObject.c

        }
        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    T_HR_DEPARTMENT department = new T_HR_DEPARTMENT();
                    department = e.Result;
                    //StrCompanyId = department.DEPARTMENTID;
                    //PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    //PostsObject.DataContext = department;
                    CompanyObject.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                }
            }
        }

        #endregion

        #region 添加并提交审核
        private void SumbitButton_Click(object sender, RoutedEventArgs e)
        {
            AuditFlag = true;


            SaveMeetingRoomApp();
        }
        #endregion

        

        #region 流程完成动作
        /// <summary>
        /// 提交审核完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (actions == FormTypes.Audit)
            //{
            //    AuditFlag = true;
            //}
            AuditFlag = true;
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
                if (tmpRoomTimeT != null)
                {
                    //isApprove = false;
                    tmpRoomTimeT.UPDATEDATE = DateTime.Now;
                    tmpRoomTimeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpRoomTimeT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpRoomTimeT.T_OA_MEETINGROOM = SelectedMeetingRoom;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing://审核中                            
                            tmpRoomTimeT.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful://审核通过
                            tmpRoomTimeT.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail://审核未通过
                            tmpRoomTimeT.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    MeetingClient.MeetingRoomAppUpdateAsync(tmpRoomTimeT);
                    //if (actions == FormTypes.New)
                    //{                        
                    //    MeetingClient.MeetingRoomAppInfoAddAsync(tmpRoomTimeT);
                    //}
                    //if (actions == FormTypes.Edit || actions == FormTypes.Audit)
                    //{                        
                    //    MeetingClient.MeetingRoomAppUpdateAsync(tmpRoomTimeT);
                    //}

                    
                }

                if (IsSuccess)
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUBMITAUDIT"), Utility.GetResourceStr("SUBMITAUDITSUCCESSED"));
                    
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

        #region IEntityEditor
        public string GetTitle()
        {

            if (actions == FormTypes.New)
            {
                return Utility.GetResourceStr("ADDTITLE", "MEETINGROOMAPP");
            }
            else if (actions == FormTypes.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "MEETINGROOMAPP");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "MEETINGROOMAPP");
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
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "2":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    AuditFlag = true;
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
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            if (actions != FormTypes.Browse && actions != FormTypes.Audit)
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
            }

            return items;
        }

        private void Close()
        {
            RefreshUI(saveType);
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

            if (actions != FormTypes.Browse)
            {
                SaveMeetingRoomApp();
            }
            
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion      

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_MEETINGROOMAPP>(tmpRoomTimeT, "OA");

            Utility.SetAuditEntity(entity, "MeetingRoomApp", tmpRoomTimeT.MEETINGROOMAPPID, strXmlObjectSource);
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
            if (tmpRoomTimeT.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpRoomTimeT.CHECKSTATE = state;
            MeetingClient.MeetingRoomAppUpdateAsync(tmpRoomTimeT, UserState);
            
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (tmpRoomTimeT != null)
                state = tmpRoomTimeT.CHECKSTATE;
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
