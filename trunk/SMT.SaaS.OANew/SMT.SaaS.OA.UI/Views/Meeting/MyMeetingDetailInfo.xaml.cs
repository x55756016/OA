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
using SMT.SaaS.FrameworkUI.ChildWidow;
namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MyMeetingDetailInfo : ChildWindow
    {


        private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();
        string StrMeetingInfoid = "";//会议申请ID
        public MyMeetingDetailInfo(V_MyMeetingInfosManagement objVMeeting)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MyMeetingDetailInfo_Loaded);
            ShowMeetingInfos(objVMeeting);
            StrMeetingInfoid = objVMeeting.OAMeetingInfoT.MEETINGINFOID;
        }

        void MyMeetingDetailInfo_Loaded(object sender, RoutedEventArgs e)
        {
            SendDocClient.GetMeetingInfoSingleInfoByIdCompleted += new System.EventHandler<GetMeetingInfoSingleInfoByIdCompletedEventArgs>(SendDocClient_GetMeetingInfoSingleInfoByIdCompleted);
            if (!string.IsNullOrEmpty(StrMeetingInfoid))
                SendDocClient.GetMeetingInfoSingleInfoByIdAsync(StrMeetingInfoid);

        }

        void SendDocClient_GetMeetingInfoSingleInfoByIdCompleted(object sender, GetMeetingInfoSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        T_OA_MEETINGINFO senddoc = new T_OA_MEETINGINFO();
                        senddoc = e.Result;
                        this.tblMeetingContent.RichTextBoxContext = senddoc.CONTENT;
                        tblMeetingContent.HideControls();
                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
            }
        }

        void ShowMeetingInfos(V_MyMeetingInfosManagement objV)
        {
            this.tblMeetingTitle.Text = objV.OAMeetingInfoT.MEETINGTITLE;
            //this.tblMeetingRoom.Text = objV.OAMeetingInfoT.MEETINGROOMNAME;
            this.tblStartTime.Text = Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortDateString() + " "+ Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortTimeString();
            this.tblEndTime.Text = Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortDateString() + " " + Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortTimeString();
            this.tblMeetingMember.Text = objV.OAMeetingInfoT.COUNT.ToString();
            //this.tblMeetingContent.Text = HttpUtility.HtmlDecode( objV.OAMeetingInfoT.CONTENT);
            this.tblCheckState.Text = this.GetMeetingCheckState(objV.OAMeetingInfoT.CHECKSTATE);
            this.tblIsCancel.Text = this.GetMeetingCancel( objV.OAMeetingInfoT.ISCANCEL);
            this.tblMyMeetingContent.Text = HttpUtility.HtmlDecode(objV.OAMeetingContentT.CONTENT);
            this.tblMyAccessory.Text = objV.OAMeetingStaffT.FILENAME;
            this.tblMycontentTime.Text = objV.OAMeetingContentT.UPDATEDATE.ToString();
            this.tblMyaccessoryTime.Text = objV.OAMeetingStaffT.UPDATEDATE.ToString();
            //this.tblMyMeetingContent
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

