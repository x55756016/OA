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
using System.Windows.Data;
using System.Globalization;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class EmceeMemberMeetingForm : BaseForm,IClient, IEntityEditor
    {
        public EmceeMemberMeetingForm()
        {
            InitializeComponent();
        }
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private RefreshedTypes refreshType = RefreshedTypes.All;
        T_OA_MEETINGSTAFF tmpStaff = new T_OA_MEETINGSTAFF();
        string StrContentID = "";

        public EmceeMemberMeetingForm(T_OA_MEETINGINFO meetinginfo,V_MyMeetingInfosManagement objVMeeting,T_OA_MEETINGROOM RoomObj)
        {
            InitializeComponent();
            tmpStaff = objVMeeting.OAMeetingStaffT;
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "MeetingApp";
            //ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            //ShowMeetingInfos(meetinginfo,objVMeeting,RoomObj);
            StrContentID = objVMeeting.OAMeetingContentT.MEETINGCONTENTID;
        }

        void ShowMeetingInfos(T_OA_MEETINGINFO meetinginfo, V_MyMeetingInfosManagement objV, T_OA_MEETINGROOM RoomObj)
        {
            tblTitle.Text = Utility.GetResourceStr("MEETINGDETAIL");
            this.tblMeetingTitle.Text = meetinginfo.MEETINGTITLE;
            this.tblMeetingRoom.Text =  RoomObj.MEETINGROOMNAME;
            this.tblStartTime.Text = Convert.ToDateTime(meetinginfo.STARTTIME).ToShortDateString() + " "+ Convert.ToDateTime(meetinginfo.STARTTIME).ToShortTimeString();
            this.tblEndTime.Text = Convert.ToDateTime(meetinginfo.ENDTIME).ToShortDateString() + " " + Convert.ToDateTime(meetinginfo.ENDTIME).ToShortTimeString();
            tblMeetingContent.RichTextBoxContext = meetinginfo.CONTENT;
            this.tblMyMeetingContent.Text = HttpUtility.HtmlDecode(objV.OAMeetingContentT.CONTENT);
            
            
            
            MeetingClient.UpdateMeetingStaffsByEmcceCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingClient_UpdateMeetingStaffsByEmcceCompleted);
        }

        void MeetingClient_UpdateMeetingStaffsByEmcceCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("CONFIRMSUCCESSED"));
                return;
            }
        }

        void UpdateStaffVisist(string flag)
        {
            tmpStaff.ISOK = flag;
            tmpStaff.UPDATEDATE = System.DateTime.Now;
            tmpStaff.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            tmpStaff.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
            MeetingClient.UpdateMeetingStaffsByEmcceAsync(tmpStaff);
        }

        private string GetMeetingCheckState(string StrState)
        {
            string StrReturn = "";
            switch (StrState)
            {
                case "0":
                    StrReturn = "未提交";
                    break;
                case "1":
                    StrReturn = "审核中";
                    break;
                case "2":
                    StrReturn = "审核通过";
                    break;
                case "3":
                    StrReturn = "审核未通过";
                    break;
            }
            return StrReturn;
        }

        private string GetMeetingCancel(string StrCancel)
        {
            string StrReturn = "";
            switch (StrCancel)
            {
                case "0":
                    StrReturn = "取消";
                    break;
                case "1":
                    StrReturn = "正常";
                    break;                
            }
            return StrReturn;
        }



        #region IEntityEditor
        public string GetTitle()
        {

            return Utility.GetResourceStr("VIEWTITLE", "MEETINGADD");

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
                    Save("1");//通过
                    break;
                case "1":
                    Save("2");//不通过
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
                Title = Utility.GetResourceStr("NOPASSED"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/16_convertactivity.png"
            };
            if (tmpStaff.ISOK != "1")
            {
                items.Add(item);
            }
            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("PASSED"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/Tool/16_convertactivity.png"
            };
            if (tmpStaff.ISOK != "2")
            {
                items.Add(item);
            }

            return items;
        }

        private void Close()
        {
            RefreshUI(RefreshedTypes.Close);
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
        private void Save(string flag)
        {
            UpdateStaffVisist(flag);
        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //ctrFile.Load_fileData(StrContentID);
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
