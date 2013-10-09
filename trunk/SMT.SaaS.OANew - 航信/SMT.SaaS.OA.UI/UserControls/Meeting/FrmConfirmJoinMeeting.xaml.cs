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
    public partial class FrmConfirmJoinMeeting : BaseForm,IClient, IEntityEditor
    {
        public FrmConfirmJoinMeeting()
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
        private string strEndTime = ""; //结束时间
        private string strMeetingRoom = ""; //会议室
        private string tmpTemplateContent = ""; //模板内容
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

        private Action ActionType ;//动作标记
        string StrRecordID = "";//会议记录人ID
        string StrHostID = "";//主持人ID
        private SMTLoading loadbar = new SMTLoading();
        private string first = "";//获取参会人员
        private string second = "";//主持人
        private string third = "";//记录人

        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID

        private T_OA_MEETINGMESSAGE MessageObj; //会议通知
        private T_OA_MEETINGSTAFF tmpstaff = new T_OA_MEETINGSTAFF();
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

        #endregion 
        
        #region 构造函数

        public FrmConfirmJoinMeeting(V_MyMeetingInfosManagement MeetingT)
        {
            InitializeComponent();
            
            //this.GetTimeHour();
            
            PARENT.Children.Add(loadbar);
            issuanceExtOrgObj = new List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>();
            SetEnabled();
            GetMeetingInfoByInfo(MeetingT);
            tmpstaff = MeetingT.OAMeetingStaffT;
            GetMeetingStaffInfo(MeetingT.OAMeetingInfoT);
            
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);            
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDCompleted += new EventHandler<GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs>(MeetingClient_GetMeetingRoomTreeInfosByCompanyIDCompleted);
            personclient.GetEmployeeDetailByIDsCompleted += new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            MeetingClient.GetMeetingRoomTreeInfosByCompanyIDAsync(Common.CurrentLoginUserInfo.UserPosts[0].CompanyID);            
            personclient.GetEmployeeDetailByParasCompleted += new EventHandler<GetEmployeeDetailByParasCompletedEventArgs>(personclient_GetEmployeeDetailByParasCompleted);
            MeetingClient.MeetingStaffUpdateInfosCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingStaffClient_MeetingStaffUpdateInfosCompleted);
        }
        void MeetingStaffClient_MeetingStaffUpdateInfosCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CONFIRMVISISTMEETINGSUCCESSED"));//确认参会成功
                                
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                return;
            }
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
                        BindData();
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message.ToString()));
            }
        }

       
        void MeetingClient_GetMeetingRoomTreeInfosByCompanyIDCompleted(object sender, GetMeetingRoomTreeInfosByCompanyIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    
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
            this.tbxMeetingContent.HideControls();

            this.tbxMeetingTitle.IsEnabled = false;            
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

        private void GetMeetingInfoByInfo(V_MyMeetingInfosManagement MeetingInfoT)
        {
            loadbar.Start();
            if (MeetingInfoT != null)
            {
                tmpMeetingInfo = MeetingInfoT.OAMeetingInfoT;
                //cbxMeetingType.SelectedItem = MeetingInfoT.MEETINGTYPE;
                //cbMeetingRoom.SelectedItem = MeetingInfoT.MEETINGROOMNAME;
                txtMeetingRoom.Text = MeetingInfoT.meetingroom.MEETINGROOMNAME;
                //txtMeetingRoom.SelectedItem = MeetingInfoT.meetingroom;
                SelectMeetingRoom = MeetingInfoT.meetingroom;
                SelectMeetingType = MeetingInfoT.meetingtype;
                
                tbxMeetingTitle.Text = MeetingInfoT.OAMeetingInfoT.MEETINGTITLE;
                tbxMeetingContent.RichTextBoxContext = MeetingInfoT.OAMeetingInfoT.CONTENT;
                dpStartDate.Text = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.STARTTIME).ToShortDateString();
                dpEndDate.Text = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.ENDTIME).ToShortDateString();
                txtTel.Text = MeetingInfoT.OAMeetingInfoT.TEL;
                PostsObject.Text = MeetingInfoT.OAMeetingInfoT.DEPARTNAME;
                strStartTime = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.STARTTIME).ToShortTimeString();
                tpStartTime.Value = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.STARTTIME);
                tpEndTime.Value = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.ENDTIME);
                strEndTime = Convert.ToDateTime(MeetingInfoT.OAMeetingInfoT.ENDTIME).ToShortTimeString();
                tbxHostMembers.Text = MeetingInfoT.OAMeetingInfoT.HOSTNAME;
                StrHostID = MeetingInfoT.OAMeetingInfoT.HOSTID;
                tbxRecordMembers.Text = MeetingInfoT.OAMeetingInfoT.RECORDUSERNAME;
                StrRecordID = MeetingInfoT.OAMeetingInfoT.RECORDUSERID;
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

        #region 增加修改

        private void SaveMeetingInfo()
        {
            string StrFlag = "";
            string StrContent = "";
            StrContent = this.txtMessageContent.Text.ToString();
            if (rbtvisist.IsChecked == true)
            {
                StrFlag = "1";
            }
            if (rbtvisistupload.IsChecked == true)
            {
                StrFlag = "2";
            }
            if (rbtnovisist.IsChecked == true)
            {
                StrFlag = "3";
            }
            if (StrFlag == "2" || StrFlag == "3")
            {
                if (string.IsNullOrEmpty(StrContent))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOVISISTRESULTNOTNULL"));
                    return;
                }
            }
            
            tmpstaff.CONFIRMFLAG = StrFlag;
            tmpstaff.REMARK = StrContent;
            tmpstaff.UPDATEDATE = System.DateTime.Now;
            tmpstaff.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            tmpstaff.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            MeetingClient.MeetingStaffUpdateInfosAsync(tmpstaff);
            
        }

        #endregion

        #region 添加按钮事件
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.SaveMeetingInfo();
        }


        
        #endregion

        #region 修改按钮事件

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            
            this.SaveMeetingInfo();
        }

        

        #endregion 

        #region

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
           
        }


        void RoomApp_MeetingRoomAppInfoAddCompleted(object sender, MeetingRoomAppInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
               
            }
        }

        #endregion

        

        #region 会议类型改变
        private void cbxMeetingType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string StrSelMeetingType = "";
            if (cbxMeetingType.Items.Count > 0)
            {
                T_OA_MEETINGTYPE MeetingType = this.cbxMeetingType.SelectedItem as T_OA_MEETINGTYPE;
                StrSelMeetingType = MeetingType.MEETINGTYPE.ToString();
                //tbxMeetingContent.Text = MeetingType.CONTENT.ToString();
                
                //MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted += new EventHandler<GetMeetingTypeTemplateNameInfosByMeetingTypeCompletedEventArgs>(TemplateClient_GetMeetingTypeTemplateNameInfosByMeetingTypeCompleted);
                //MeetingClient.GetMeetingTypeTemplateNameInfosByMeetingTypeAsync(StrSelMeetingType);
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
                    tbxMeetingContent.RichTextBoxContext = template.CONTENT;
                }
            }
        }
        
        #endregion
        
        
        
        

        

        #region 开始时间改变
        private void tpStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.tpEndTime.SelectedIndex = tpStartTime.SelectedIndex + 1;
        }
        #endregion

       

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("CONFIRMTOVISISTMEETING");
            
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
            this.SaveMeetingInfo();            
        }

        private void SaveAndClose()
        {
            Save();
            //RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        
        
        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {            
            int index = e.Row.GetIndex();
            var cell = dgmember.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();　
         
        }



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
