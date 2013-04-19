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

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class CancelMeetingForm : BaseForm,IClient, IEntityEditor
    {
        public CancelMeetingForm()
        {
            InitializeComponent();
        }

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
        
        
        private T_OA_MEETINGINFO tmpMeetingInfo = new T_OA_MEETINGINFO();
        private string StrDepartment = "";  //部门
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        private bool AuditType = false; //是否提交了审核按钮
        private bool SaveAuditType = false; //保存提交审核标志
        
        
        List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> tmpMeetingMember = new List<ExtOrgObj>();
        
        
        public event refreshGridView ReloadDataEvent;
        
        private int SelectIndex = 0; //记录开始时间选择的位置
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;

        private FormTypes ActionType ;//动作标记
        
        
        private SMTLoading loadbar = new SMTLoading();
        
        
        

        private T_OA_MEETINGMESSAGE MessageObj; //会议通知

        private List<ExtOrgObj> issuanceExtOrgObj;
        private List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST> vemployeeObj = new List<SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST>();
        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        #endregion 
        
        #region 构造函数

        public CancelMeetingForm(FormTypes action, V_MeetingInfo MeetingT)
        {
            InitializeComponent();
            SetEnabled();
            //this.GetTimeHour();
            ActionType = action;
            PARENT.Children.Add(loadbar);
            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
                            
            GetMeetingInfoByInfo(MeetingT);
            GetMeetingStaffInfo(MeetingT.meetinginfo);
            GetMeetingMessageInfo(MeetingT.meetinginfo);
            

            MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);            
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);
            
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDCompleted += new EventHandler<GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs>(MeetingClient_GetMeetingRoomTreeInfosByCompanyIDCompleted);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);
            MeetingClient.GetMeetingMessageByIDCompleted += new EventHandler<GetMeetingMessageByIDCompletedEventArgs>(MeetingClient_GetMeetingMessageByIDCompleted);
            personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
            MeetingClient.UpdateMeetingNoticeInfoCompleted += new EventHandler<UpdateMeetingNoticeInfoCompletedEventArgs>(MeetingClient_UpdateMeetingNoticeInfoCompleted);
        }

        void MeetingClient_UpdateMeetingNoticeInfoCompleted(object sender, UpdateMeetingNoticeInfoCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result > -1)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CANCELMEETINGAPP"));
                    RefreshUI(RefreshedTypes.CloseAndReloadData);
                }
            }
        }

        void personclient_GetEmployeeDetailByParasCompleted(object sender, GetEmployeeDetailByParasCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                { 

                }
            }
        }

        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            string bbb = "";
            if (!e.Cancelled)
            {
                if (e.Result == 1)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGADD"));
                    RefreshUI(refreshType);                    
                    
                }
                //需要将修改记录添加到会议修改表中

            }
            else
            {
                //MessageBox.Show(e.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.ToString());
            }

        }

        void MeetingClient_GetMeetingMessageByIDCompleted(object sender, GetMeetingMessageByIDCompletedEventArgs e)
        {
            //throw new NotImplementedException();
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    this.txtMessageTitle.Text = e.Result.TITLE;                    
                    tbxMeetingContent.RichTextBoxContext = e.Result.CONTENT;
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
                    this.txtMeetingRoom.ItemsSource = e.Result.ToList();
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
            //this.tabMeetingInfo.IsEnabled = false;
            //this.tabmeetingmember.IsEnabled = false;

            this.txtTel.IsEnabled = false;
            this.tbxMeetingContent.IsEnabled = false;
            this.tbxMeetingTitle.IsEnabled = false;
            this.txtMeetingRoom.IsEnabled = false;
            
            this.cbxMeetingType.IsEnabled = false;
            this.cbxMeetingTypeTemplate.IsEnabled = false;
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
            loadbar.Start();
            if (MeetingInfoT != null)
            {
                tmpMeetingInfo = MeetingInfoT.meetinginfo;
                //cbxMeetingType.SelectedItem = MeetingInfoT.MEETINGTYPE;
                //cbMeetingRoom.SelectedItem = MeetingInfoT.MEETINGROOMNAME;
                txtMeetingRoom.Text = MeetingInfoT.meetingroom.MEETINGROOMNAME;
                txtMeetingRoom.SelectedItem = MeetingInfoT.meetingroom;
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
                
                tbxHostMembers.Text = MeetingInfoT.meetinginfo.HOSTNAME;
                
                tbxRecordMembers.Text = MeetingInfoT.meetinginfo.RECORDUSERNAME;
                
                combox_MeetingRoomSelectSource();
                combox_SelectSource();
                
            }
            
        }
        #endregion

        #region COMBOX 设置数据源

        private void combox_SelectSource()
        {
            loadbar.Start();
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            

        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
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
            loadbar.Stop();
        }

        private void combox_MeetingRoomSelectSource()
        {
            MeetingClient.GetMeetingRoomNameInfosToComboxAsync();
            
        }

        

       

        #endregion

        #region 增加取消会议通知

        private void SaveMeetingInfo()
        {
            try
            {
                tmpMeetingInfo.CHECKSTATE = "3";
                tmpMeetingInfo.UPDATEDATE = System.DateTime.Now;
                tmpMeetingInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                tmpMeetingInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                AddStaffMessage();
                MeetingClient.UpdateMeetingNoticeInfoAsync(tmpMeetingInfo,MessageObj);
                //MeetingClient.MeetingInfoUpdateByFormAsync(tmpMeetingInfo, MeetingStaffList, MeetingContentList, MessageObj);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 添加与会人员通知
        /// </summary>
        void AddStaffMessage()
        {
            string StrTitle = txtMessageTitle.Text.ToString();
            //string StrContent = txtMessageContent.Text.ToString();
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

        #endregion

        #region 添加按钮事件
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //ActionType
            //ActionType = Action.Add;
            this.SaveMeetingInfo();

        }

        #endregion

        
        #region 绑定参会人员
        
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
            return Utility.GetResourceStr("ADDTITLE", "CANCELMEETINGCONFIRM");;
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
                    Save();
                    break;
                case "1"://保存并关闭
                    refreshType = RefreshedTypes.CloseAndReloadData;                    
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

            

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("CANCELMEETINGCONFIRM"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/18_audit.png"

            };
            items.Add(item);
            
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

        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SelectMeetingRoom addFrm = new SelectMeetingRoom();
            
            EntityBrowser browser = new EntityBrowser(addFrm);

            browser.ReloadDataEvent += new EntityBrowser.refreshGridView(browser_ReloadDataEvent);
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
        }

        private void browser_ReloadDataEvent()
        { 

        }

        private void senddoctab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl TabC = sender as TabControl;
            if (TabC.SelectedIndex == 2)
            {
                if (!string.IsNullOrEmpty(tbxMeetingTitle.Text.ToString()))
                {
                    this.txtMessageTitle.Text = Utility.GetResourceStr("CANCEL") + tbxMeetingTitle.Text.ToString() + Utility.GetResourceStr("NOTICES"); ;
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
                        txtMessageContent.RichTextBoxContext = this.TransDate(DtStart, DtEnd, MeetingRoom, tbxMeetingTitle.Text.ToString());
                    }
                    
                }
                
            }

            
        }

        private byte[] TransDate(DateTime StartDate,DateTime EndDate,string MeetingRoom,string StrTitle)
        {
            //XX年XX月XX日，X点X分至X点X分，在XXX会议室举行XX会议，忘各位准时参加
            string StrReturn = "";
            //StrReturn = StartDate.Year.ToString() + Utility.GetResourceStr("YEAR") + StartDate.Month.ToString() + Utility.GetResourceStr("MONTH") +
            //    StartDate.Day.ToString() + Utility.GetResourceStr("DAY");
            //StrReturn = StartDate.ToShortDateString();
            StrReturn = Utility.GetResourceStr("NOWCANCEL") + StartDate.ToLongDateString().ToString() + ",";
            StrReturn += StartDate.ToShortTimeString() +Utility.GetResourceStr("TO")+ EndDate.ToShortTimeString()+".";
            StrReturn += Utility.GetResourceStr("IN") + MeetingRoom + Utility.GetResourceStr("MEETINGROOM") + Utility.GetResourceStr("CELEBRATE") + StrTitle + Utility.GetResourceStr("NOTICES") + ",";
            StrReturn += Utility.GetResourceStr("HOPEONTIMEVISIST");
            return System.Text.Encoding.Unicode.GetBytes(StrReturn); 
        }




        #region IForm 成员

        public void ClosedWCFClient()
        {
            personclient.DoClose();
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
