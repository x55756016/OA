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
    public partial class AddMeetingContent : ChildWindow
    {


        private T_OA_MEETINGCONTENT tmpContentT;
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        public delegate void refreshGridView();
        
        public event refreshGridView ReloadDataEvent;


        private string[] lstring = new string[100];

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        public AddMeetingContent(T_OA_MEETINGCONTENT obj)
        {
            InitializeComponent();
            tmpContentT = obj;
            this.ChildWinkow.Title = "添加会议内容";
            this.lblTitle.Content = "更新个人会议内容";
            this.txtContent.Text = HttpUtility.HtmlDecode( obj.CONTENT);
        }



        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string StrContent = "";
            StrContent = HttpUtility.HtmlEncode( this.txtContent.Text.Trim().ToString());
            if (!string.IsNullOrEmpty(StrContent))
            {
                tmpContentT.CONTENT = StrContent;

                //MeetingContentManagementServiceClient ContentClient = new MeetingContentManagementServiceClient();
                MeetingClient.MeetingContentInfoUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(ContentClient_MeetingContentInfoUpdateCompleted);
                MeetingClient.MeetingContentInfoUpdateAsync(tmpContentT);

            }
            

        }

        void ContentClient_MeetingContentInfoUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                MessageBox.Show("更新成功！");
                this.ReloadData();
                this.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

