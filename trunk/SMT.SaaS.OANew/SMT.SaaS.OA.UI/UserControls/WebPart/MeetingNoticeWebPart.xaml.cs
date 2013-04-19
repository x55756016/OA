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
    public partial class MeetingNoticeWebPart : BaseForm,IClient,IEntityEditor
    {
        
        #region
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        
        private ObservableCollection<T_OA_MEETINGCONTENT> MeetingContentList = new ObservableCollection<T_OA_MEETINGCONTENT>();
        private ObservableCollection<T_OA_MEETINGSTAFF> MeetingStaffList = new ObservableCollection<T_OA_MEETINGSTAFF>();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        public delegate void refreshGridView();        
        private T_OA_MEETINGINFO tmpMeetingInfo = new T_OA_MEETINGINFO();        
        private SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST employeepost;
        public event refreshGridView ReloadDataEvent;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private SMTLoading loadbar = new SMTLoading();
        private ObservableCollection<string> StrCompanyIDsList = new ObservableCollection<string>();  //获取公司ID
        private ObservableCollection<string> StrDepartmentIDsList = new ObservableCollection<string>();  //获取部门ID
        private ObservableCollection<string> StrPositionIDsList = new ObservableCollection<string>();  //获取岗位ID

        private T_OA_MEETINGMESSAGE MessageObj; //会议通知

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

        public MeetingNoticeWebPart(string StrNoticeId)
        {
            InitializeComponent();
            tblMeetingContent.HideControls();
            MeetingClient.GetMeetingNoticeByNoticeIDCompleted += new EventHandler<GetMeetingNoticeByNoticeIDCompletedEventArgs>(MeetingClient_GetMeetingNoticeByNoticeIDCompleted);
            MeetingClient.GetMeetingNoticeByNoticeIDAsync(StrNoticeId);
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(MeetingClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            personclient.GetEmployeeDetailByIDsCompleted +=new EventHandler<GetEmployeeDetailByIDsCompletedEventArgs>(personclient_GetEmployeeDetailByIDsCompleted);
            
        }

        void MeetingClient_GetMeetingNoticeByNoticeIDCompleted(object sender, GetMeetingNoticeByNoticeIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    V_MeetingInfo MeetingInfo = e.Result;
                    T_OA_MEETINGINFO InfoT = MeetingInfo.meetinginfo;
                    this.tblDepartment.Text = InfoT.DEPARTNAME;
                    this.tblMeetingTitle.Text = InfoT.MEETINGTITLE;                                        
                    tblMeetingContent.RichTextBoxContext = InfoT.CONTENT;
                    this.tblHost.Text = InfoT.HOSTNAME;
                    this.tblRecorder.Text = InfoT.RECORDUSERNAME;
                    this.tblTel.Text = InfoT.TEL;
                    this.tblMeetingRoom.Text = InfoT.T_OA_MEETINGROOM.MEETINGROOMNAME;
                    this.tblMeetingType.Text = InfoT.T_OA_MEETINGTYPE.MEETINGTYPE;
                    this.tblmessagetitle.Text = MeetingInfo.meetingmessage.TITLE;
                    this.tblmessagecontent.Text = MeetingInfo.meetingmessage.CONTENT.ToString();
                    this.tblmessagetel.Text = MeetingInfo.meetingmessage.TEL;
                    GetMeetingStaffInfo(InfoT);
                    
                }
            }
            //throw new NotImplementedException();
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
            return Utility.GetResourceStr("MEETINGNOTICE");            
        }

        public string GetStatus()
        {
            return "";
        }
        

        public void DoAction(string actionType)
        {
            RefreshUI(RefreshedTypes.Close);
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
                Title = Utility.GetResourceStr("CLOSE"),
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

            
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        
        private void dgmember_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST StaffV = (SMT.Saas.Tools.PersonnelWS.V_EMPLOYEEPOST)e.Row.DataContext;

            int index = e.Row.GetIndex();
            var cell = dgmember.Columns[0].GetCellContent(e.Row) as TextBlock;
            cell.Text = (index + 1).ToString();　
            

        }





        #region IForm 成员

        public void ClosedWCFClient()
        {
            MeetingClient.DoClose();
            personclient.DoClose();
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
