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
using System.Windows.Browser;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;



namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class EditMeetingTime : ChildWindow
    {
        
        private string strStartTime = ""; //开始时间 时：分
        private string strEndTime = ""; //结束时间
        public event refreshGridView ReloadDataEvent;
        public delegate void refreshGridView();

        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        private T_OA_MEETINGINFO tmpMeetintInfoT;
        public EditMeetingTime(T_OA_MEETINGINFO meetintInfoT)
        {
            InitializeComponent();
            
            this.GetTimeHour();

            this.ChildWinkow.Title = "变更"+meetintInfoT.MEETINGTITLE+"会议时间";
            this.lblTitle.Content = "变更" + meetintInfoT.MEETINGTITLE + "会议时间";
            this.DPStart.Text = Convert.ToDateTime(meetintInfoT.STARTTIME).ToShortDateString();
            this.DPEnd.Text = Convert.ToDateTime(meetintInfoT.ENDTIME).ToShortDateString();
            this.tpStartTime.SelectedItem = Convert.ToDateTime(meetintInfoT.STARTTIME).ToShortTimeString();
            this.tpEndTime.SelectedItem = Convert.ToDateTime(meetintInfoT.ENDTIME).ToShortTimeString();

            tmpMeetintInfoT = meetintInfoT; 
        }


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
            this.tpStartTime.ItemsSource = strTime;
            if (strStartTime == "")
            {
                this.tpStartTime.SelectedIndex = 0;
            }
            else
            {
                this.tpStartTime.SelectedItem = strStartTime;
            }
            this.tpEndTime.ItemsSource = strTime;
            if (strEndTime == "")
            {
                this.tpEndTime.SelectedIndex = 0;
            }
            else
            {
                this.tpEndTime.SelectedItem = strEndTime;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string StrResult = ""; //变更原因
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string StrEndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            string StrError = "";  //错误提示信息
            bool blError = true;  //是否正确

            
            if (!string.IsNullOrEmpty(this.DPStart.Text.ToString()))
            {
                StrStartDt = this.DPStart.Text.ToString();
            }
            else
            {
                //MessageBox.Show("会议开始时间不能为空");
                StrError += "会议开始时间不能为空\n";
                blError = false;
            }
            if (!string.IsNullOrEmpty(this.DPEnd.Text.ToString()))
            {
                StrEndDt = this.DPEnd.Text.ToString();

            }
            else
            {
                StrError += "会议结束时间不能为空\n";
                blError = false;
                //return;
            }
           
            if (!string.IsNullOrEmpty(this.txtResult.Text.Trim().ToString()))
            {
                StrResult = HttpUtility.HtmlEncode(this.txtResult.Text.Trim().ToString());
            }
            else
            {
                StrError += "会议时间变更原因不能为空！\n";
                blError = false;
            }
            if (!string.IsNullOrEmpty(this.tpStartTime.SelectedItem.ToString()))
            {
                StrStartTime = this.tpStartTime.SelectedItem.ToString();
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.SelectedItem.ToString()))
            {
                StrEndTime = this.tpEndTime.SelectedItem.ToString();
            }

            
            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart >= DtEnd)
            {
                StrError += "会议开始时间必须小于结束时间！\n";
                blError = false;
            }

            if (blError)
            {
                //MeetingInfoManagementServiceClient MeetingInfoClient = new MeetingInfoManagementServiceClient();
                T_OA_MEETINGINFO MeetingInfoT = new T_OA_MEETINGINFO();
                MeetingInfoT.MEETINGINFOID = tmpMeetintInfoT.MEETINGINFOID;
                //MeetingInfoT.MEETINGROOMNAME = tmpMeetintInfoT.MEETINGROOMNAME;
                MeetingInfoT.MEETINGTITLE = tmpMeetintInfoT.MEETINGTITLE;
                //MeetingInfoT.MEETINGTYPE = tmpMeetintInfoT.MEETINGTYPE;
                MeetingInfoT.ISCANCEL = tmpMeetintInfoT.ISCANCEL;
                MeetingInfoT.CHECKSTATE = tmpMeetintInfoT.CHECKSTATE;               
                MeetingInfoT.DEPARTNAME = tmpMeetintInfoT.DEPARTNAME;
                MeetingInfoT.STARTTIME = DtStart;
                MeetingInfoT.ENDTIME = DtEnd;
                MeetingInfoT.COUNT = tmpMeetintInfoT.COUNT;
                MeetingInfoT.CONTENT = tmpMeetintInfoT.CONTENT;
                MeetingInfoT.CREATEDATE = tmpMeetintInfoT.CREATEDATE;
                MeetingInfoT.CREATEUSERID = tmpMeetintInfoT.CREATEUSERID;
                MeetingInfoT.UPDATEDATE = System.DateTime.Now;
                MeetingInfoT.UPDATEUSERID = "1";//tmpMeetintInfoT.UPDATEUSERID;

                tmpMeetintInfoT = MeetingInfoT;

                //MeetingInfoClient.MeetingInfoUpdateCompleted +=new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
                MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
                MeetingClient.MeetingInfoUpdateAsync(MeetingInfoT);

                


            }
            else
            {
                MessageBox.Show(StrError);
            }
        }


        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                //MeetingTimeChangeManagementServiceClient MeetingTimeClient = new MeetingTimeChangeManagementServiceClient();
                T_OA_MEETINGTIMECHANGE MeetingTimeT = new T_OA_MEETINGTIMECHANGE();
                MeetingTimeT.MEETINGTIMECHANGEID = System.Guid.NewGuid().ToString();
                //MeetingTimeT.MEETINGINFOID = tmpMeetintInfoT.MEETINGINFOID;
                MeetingTimeT.STARTTIME = Convert.ToDateTime(tmpMeetintInfoT.STARTTIME);
                MeetingTimeT.ENDTIME = Convert.ToDateTime(tmpMeetintInfoT.ENDTIME);
                MeetingTimeT.CREATEDATE = Convert.ToDateTime(tmpMeetintInfoT.CREATEDATE);
                MeetingTimeT.CREATEUSERID = tmpMeetintInfoT.CREATEUSERID;
                MeetingTimeT.UPDATEDATE = tmpMeetintInfoT.UPDATEDATE;
                MeetingTimeT.UPDATEUSERID = tmpMeetintInfoT.UPDATEUSERID;
                MeetingTimeT.REASON = this.txtResult.Text.Trim().ToString();

                MeetingClient.MeetingTimeChangeAddCompleted += new EventHandler<MeetingTimeChangeAddCompletedEventArgs>(MeetingTimeClient_MeetingTimeChangeAddCompleted);
                MeetingClient.MeetingTimeChangeAddAsync(MeetingTimeT);
            }
        }

        void MeetingTimeClient_MeetingTimeChangeAddCompleted(object sender, MeetingTimeChangeAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != "")
                {
                    MessageBox.Show(e.Result.ToString());
                }
                else
                {
                    MessageBox.Show("会议时间变更成功！");
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

