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
//using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.PersonnelWS;
using System.Collections.ObjectModel;
using System.Windows.Browser;
using System.Text;

namespace SMT.SaaS.OA.UI.Views.Meeting
{
    public partial class MeetingMangementDetailInfo : ChildWindow
    {

        
        private T_OA_MEETINGINFO tmpMeetingInfoT;
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private ObservableCollection<string> StrStaffList = new ObservableCollection<string>();  //员工ID
        private List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE> MeetingStaffs = new List<SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE>();


        public MeetingMangementDetailInfo(V_MeetingInfo obj)
        {
            InitializeComponent();
            
            tmpMeetingInfoT = obj.meetinginfo;
            GetMeetingStaffInfo(obj.meetinginfo);
            ShowMeetingDetailInfos();
            ShowMeetingUploadContent(); //回去与会人员会议内容
            ShowMeetingStaffInfos(); //获取与会人员附件信息
            ShowMeetingChangeInfos(); //会议时间变更
            ShowMeetingRoomTimeChangeInfos(); //获取会议室使用时间变更
            
        }

        void ShowMeetingDetailInfos()
        {
            this.tblTitle.Text = tmpMeetingInfoT.MEETINGTITLE + "会议基本信息";
            this.tblMeetingTitle.Text = tmpMeetingInfoT.MEETINGTITLE;
            //this.tblMeetingType.Text = tmpMeetingInfoT.MEETINGTYPE;
            this.tblDepartment.Text = tmpMeetingInfoT.DEPARTNAME;
            //this.tblMeetingRoom.Text = tmpMeetingInfoT.MEETINGROOMNAME;
            this.tblStartTime.Text = Convert.ToDateTime(tmpMeetingInfoT.STARTTIME).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.STARTTIME).ToLongTimeString();
            this.tblEndTime.Text = Convert.ToDateTime(tmpMeetingInfoT.ENDTIME).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.ENDTIME).ToLongTimeString();
            //this.tblMeetingContent.Text = HttpUtility.HtmlDecode( tmpMeetingInfoT.CONTENT);
            if (tmpMeetingInfoT.UPDATEDATE != null)
            {
                this.tblUpdateTime.Text = tmpMeetingInfoT.UPDATEDATE.ToString();
            }
            else
            {
                this.tblUpdateTime.Text = "";
            }
            this.tblEditer.Text = tmpMeetingInfoT.UPDATEUSERID;
            tblMemberCount.Text = tmpMeetingInfoT.COUNT.ToString();
            tblAddTime.Text = Convert.ToDateTime(tmpMeetingInfoT.CREATEDATE).ToLongDateString() + Convert.ToDateTime(tmpMeetingInfoT.CREATEDATE).ToLongTimeString();


        }

        #region 显示与会人员上传的内容

        void ShowMeetingUploadContent()
        {
            //MeetingContentManagementServiceClient ContentClient = new MeetingContentManagementServiceClient();

            MeetingClient.GetMeetingContentInfosByMeetngInfoIDCompleted += new EventHandler<GetMeetingContentInfosByMeetngInfoIDCompletedEventArgs>(ContentClient_GetMeetingContentInfosByMeetngInfoIDCompleted);
            MeetingClient.GetMeetingContentInfosByMeetngInfoIDAsync(tmpMeetingInfoT.MEETINGINFOID);

        }

        void ContentClient_GetMeetingContentInfosByMeetngInfoIDCompleted(object sender, GetMeetingContentInfosByMeetngInfoIDCompletedEventArgs e)
        {
            List<T_OA_MEETINGCONTENT> ListContent = new List<T_OA_MEETINGCONTENT>();
            ListContent = e.Result.ToList();
            StringBuilder sbInfos = new StringBuilder();
            foreach (T_OA_MEETINGCONTENT SingleInfo in ListContent)
            {
                
                sbInfos.Append("与会人：");
                if (MeetingStaffs.Count > 0)
                { 
                    foreach(SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee in MeetingStaffs)
                    {
                        if (SingleInfo.MEETINGUSERID == employee.EMPLOYEEID)
                        {
                            sbInfos.Append(employee.EMPLOYEECNAME);
                            sbInfos.Append("\n");
                        }
                    }
                }
                
                sbInfos.Append(HttpUtility.HtmlDecode( SingleInfo.CONTENT));
                sbInfos.Append("\n");
                if (string.IsNullOrEmpty(SingleInfo.UPDATEDATE.ToString()))
                {
                    //sbInfos.Append("<font color='red'>还没上传内容</font>\n");
                    sbInfos.Append("还没上传内容\n");
                }
                sbInfos.Append("\n\n");
            }
            
            string StrTmp = "";
            StrTmp = HttpUtility.HtmlEncode(sbInfos.ToString());
            tblUploadContent.Text = HttpUtility.HtmlDecode(StrTmp);
        }

        #endregion

        #region 显示与会人员上传的附件
        void ShowMeetingStaffInfos()
        {
            //MeetingStaffManagementServiceClient StaffClient = new MeetingStaffManagementServiceClient();
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDCompleted += new EventHandler<GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs>(StaffClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted);
            MeetingClient.GetAllMeetingStaffInfosByMeetingInfoIDAsync(tmpMeetingInfoT.MEETINGINFOID);
        }

        void StaffClient_GetAllMeetingStaffInfosByMeetingInfoIDCompleted(object sender, GetAllMeetingStaffInfosByMeetingInfoIDCompletedEventArgs e)
        {
            StringBuilder sbInfos = new StringBuilder();
            if (e.Result != null)
            {
                List<T_OA_MEETINGSTAFF> ListContent = new List<T_OA_MEETINGSTAFF>();
                ListContent = e.Result.ToList();
                
                foreach (T_OA_MEETINGSTAFF SingleInfo in ListContent)
                {

                    sbInfos.Append("与会人：");
                    //if (MeetingStaffs.Count > 0)
                    //{ 
                    //    foreach(T_HR_employee )
                    //}
                    if (MeetingStaffs.Count > 0)
                    {
                        foreach (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE employee in MeetingStaffs)
                        {
                            if (SingleInfo.MEETINGUSERID == employee.EMPLOYEEID)
                            {
                                sbInfos.Append(employee.EMPLOYEECNAME);
                                sbInfos.Append("\n");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(SingleInfo.FILENAME))
                    {
                        sbInfos.Append(SingleInfo.FILENAME);
                    }
                    else
                    {
                        sbInfos.Append("没有附件上传");
                    }

                    sbInfos.Append("\n\n");
                }
            }
            else
            {
                sbInfos.Append("暂无确认参会人员");
            }


            tblUploadAccessory.Text = sbInfos.ToString();
        }
        #endregion

        void ShowMeetingChangeInfos()
        { 

        }

        void ShowMeetingRoomTimeChangeInfos()
        { 

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #region 获取参会人员信息
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
                if (StrStaffList.Count > 0)
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
                MeetingStaffs = e.Result.ToList();
            }
        }
        #endregion
    }
}

