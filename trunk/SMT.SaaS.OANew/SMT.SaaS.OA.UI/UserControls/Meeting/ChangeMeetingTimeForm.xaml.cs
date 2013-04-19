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
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ChangeMeetingTimeForm : BaseForm, IClient, IEntityEditor
    {
        public ChangeMeetingTimeForm()
        {
            InitializeComponent();
        }

        private string strStartTime = ""; //开始时间 时：分
        private string strEndTime = ""; //结束时间
        public event refreshGridView ReloadDataEvent;
        public delegate void refreshGridView();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;
        private V_MeetingInfo tmpvmeeting = new V_MeetingInfo();
        private FormTypes actiontype;
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private T_OA_MEETINGMESSAGE MessageObj; //会议通知
        DateTime TmpDtStart = new DateTime();
        DateTime TmpDtEnd = new DateTime();
        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        private T_OA_MEETINGINFO tmpMeetintInfoT;
        public ChangeMeetingTimeForm(FormTypes action, V_MeetingInfo meetintInfoT)
        {
            InitializeComponent();
            actiontype = action;
            //this.GetTimeHour();

            tmpvmeeting = meetintInfoT;

            this.lblTitle.Content = "变更" + meetintInfoT.meetinginfo.MEETINGTITLE + "会议时间";
            this.DPStart.Text = Convert.ToDateTime(meetintInfoT.meetinginfo.STARTTIME).ToShortDateString();
            this.DPEnd.Text = Convert.ToDateTime(meetintInfoT.meetinginfo.ENDTIME).ToShortDateString();
            this.tpStartTime.Value = meetintInfoT.meetinginfo.STARTTIME;
            this.tpEndTime.Value = meetintInfoT.meetinginfo.ENDTIME;
            TmpDtStart = Convert.ToDateTime(meetintInfoT.meetinginfo.STARTTIME);
            TmpDtEnd = Convert.ToDateTime(meetintInfoT.meetinginfo.ENDTIME);
            tmpMeetintInfoT = meetintInfoT.meetinginfo;
            tmpMeetintInfoT.T_OA_MEETINGROOM = meetintInfoT.meetingroom;
            tmpMeetintInfoT.T_OA_MEETINGTYPE = meetintInfoT.meetingtype;
            this.txtMessageTel.Text = meetintInfoT.meetinginfo.TEL;
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

        private void Save()
        {
            string StrResult = ""; //变更原因
            string StrStartDt = "";   //开始时间
            string StrStartTime = ""; //开始时：分
            string StrEndDt = "";    //结束时间
            string StrEndTime = ""; //结束时：分
            string StrError = "";  //错误提示信息



            if (!string.IsNullOrEmpty(this.DPStart.Text.ToString()))
            {
                StrStartDt = this.DPStart.Text.ToString();
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTNULL"));
                return;

            }
            if (!string.IsNullOrEmpty(this.DPEnd.Text.ToString()))
            {
                StrEndDt = this.DPEnd.Text.ToString();

            }
            else
            {
                //StrError += "会议结束时间不能为空\n";                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ENDTIMENOTNULL"));
                return;
            }

            if (!string.IsNullOrEmpty(this.txtResult.Text.Trim().ToString()))
            {
                StrResult = HttpUtility.HtmlEncode(this.txtResult.Text.Trim().ToString());
            }
            else
            {
                //StrError += "会议时间变更原因不能为空！\n";                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGTIMECHANGENOTNULL"));
            }
            if (!string.IsNullOrEmpty(tpStartTime.Value.Value.ToString()))
            {
                StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGSTARTTIMENOTNULL"));
                return;
            }
            if (!string.IsNullOrEmpty(this.tpEndTime.Value.Value.ToString()))
            {
                StrEndTime = this.tpEndTime.Value.Value.ToString("HH:mm");
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGENDTIMENOTNULL"));
                return;
            }



            DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
            DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
            if (DtStart >= DtEnd)
            {
                //StrError += "会议开始时间必须小于结束时间！\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STARTTIMENOTGREATENDTIME"));
                return;
            }
            //开始时间、结束时间不能小于当前时间 
            if (DtStart <= System.DateTime.Now)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "STARTTIME"));
                return;
            }
            if (DtEnd <= System.DateTime.Now)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CANNOTSMOLLERTHANTHECURRENTTIME", "ENDTIME"));
                return;
            }


            if (TmpDtStart == DtStart && TmpDtEnd == DtEnd)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("MEETINGTIMENOTCHANGE"));
                return;
            }
            if (Check())
            {
                tmpMeetintInfoT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpMeetintInfoT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                tmpMeetintInfoT.UPDATEDATE = System.DateTime.Now;
                tmpMeetintInfoT.STARTTIME = DtStart;
                tmpMeetintInfoT.ENDTIME = DtEnd;

                //T_OA_MEETINGINFO MeetingInfoT = new T_OA_MEETINGINFO();
                //MeetingInfoT.MEETINGINFOID = tmpMeetintInfoT.MEETINGINFOID;
                //MeetingInfoT.T_OA_MEETINGROOM = tmpvmeeting.meetingroom;
                //MeetingInfoT.T_OA_MEETINGROOM.MEETINGROOMNAME = tmpvmeeting.meetingroom.MEETINGROOMNAME;
                //MeetingInfoT.MEETINGTITLE = tmpMeetintInfoT.MEETINGTITLE;
                //MeetingInfoT.T_OA_MEETINGTYPE = tmpvmeeting.meetingtype;
                //MeetingInfoT.T_OA_MEETINGTYPE.MEETINGTYPE = tmpvmeeting.meetingtype.MEETINGTYPE;
                //MeetingInfoT.ISCANCEL = tmpMeetintInfoT.ISCANCEL;
                //MeetingInfoT.CHECKSTATE = tmpMeetintInfoT.CHECKSTATE;
                //MeetingInfoT.DEPARTNAME = tmpMeetintInfoT.DEPARTNAME;
                //MeetingInfoT.STARTTIME = DtStart;
                //MeetingInfoT.ENDTIME = DtEnd;
                //MeetingInfoT.COUNT = tmpMeetintInfoT.COUNT;
                //MeetingInfoT.CONTENT = tmpMeetintInfoT.CONTENT;
                //MeetingInfoT.CREATEDATE = tmpMeetintInfoT.CREATEDATE;
                //MeetingInfoT.CREATEUSERID = tmpMeetintInfoT.CREATEUSERID;
                //MeetingInfoT.UPDATEDATE = System.DateTime.Now;
                //MeetingInfoT.UPDATEUSERID = "";

                //tmpMeetintInfoT = MeetingInfoT;

                //MeetingInfoClient.MeetingInfoUpdateCompleted +=new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
                AddStaffMessage();
                MeetingClient.UpdateMeetingNoticeInfoAsync(tmpMeetintInfoT, MessageObj);
                //MeetingClient.MeetingInfoUpdateCompleted += new EventHandler<MeetingInfoUpdateCompletedEventArgs>(MeetingInfoClient_MeetingInfoUpdateCompleted);
                //MeetingClient.MeetingInfoUpdateAsync(tmpMeetintInfoT);


            }
        }

        void AddStaffMessage()
        {
            string StrTitle = txtMessageTitle.Text.ToString();
            //string StrContent = txtMessageContent.Text.ToString();
            string StrTel = txtMessageTel.Text.ToString();
            MessageObj = new T_OA_MEETINGMESSAGE();
            MessageObj.MEETINGMESSAGEID = System.Guid.NewGuid().ToString();

            MessageObj.T_OA_MEETINGINFO = tmpvmeeting.meetinginfo;// new T_OA_MEETINGINFO();// tmpMeetingInfo;
            //MessageObj.T_OA_MEETINGINFO.MEETINGINFOID = tmpMeetingInfo.MEETINGINFOID;

            MessageObj.TITLE = StrTitle;

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


        void MeetingInfoClient_MeetingInfoUpdateCompleted(object sender, MeetingInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {

                T_OA_MEETINGTIMECHANGE MeetingTimeT = new T_OA_MEETINGTIMECHANGE();
                MeetingTimeT.MEETINGTIMECHANGEID = System.Guid.NewGuid().ToString();
                //MeetingTimeT.T_OA_MEETINGINFO = tmpMeetintInfoT;
                //MeetingTimeT.T_OA_MEETINGINFO.MEETINGINFOID = tmpMeetintInfoT.MEETINGINFOID;
                MeetingTimeT.MEETINGINFOID = tmpMeetintInfoT.MEETINGINFOID;

                MeetingTimeT.STARTTIME = Convert.ToDateTime(tmpMeetintInfoT.STARTTIME);
                MeetingTimeT.ENDTIME = Convert.ToDateTime(tmpMeetintInfoT.ENDTIME);
                MeetingTimeT.REASON = this.txtResult.Text.Trim().ToString();

                MeetingTimeT.CREATEDATE = Convert.ToDateTime(tmpMeetintInfoT.CREATEDATE);
                MeetingTimeT.CREATEUSERID = tmpMeetintInfoT.CREATEUSERID;
                MeetingTimeT.CREATECOMPANYID = tmpMeetintInfoT.CREATECOMPANYID;
                MeetingTimeT.CREATEDEPARTMENTID = tmpMeetintInfoT.CREATECOMPANYID;
                MeetingTimeT.CREATEPOSTID = tmpMeetintInfoT.CREATEPOSTID;
                MeetingTimeT.CREATEUSERNAME = tmpMeetintInfoT.CREATEUSERNAME;

                MeetingTimeT.UPDATEDATE = tmpMeetintInfoT.UPDATEDATE;
                MeetingTimeT.UPDATEUSERID = tmpMeetintInfoT.UPDATEUSERID;
                MeetingTimeT.UPDATEUSERNAME = tmpMeetintInfoT.UPDATEUSERNAME;

                MeetingTimeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                MeetingTimeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                MeetingTimeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                MeetingTimeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                MeetingTimeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

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
                    //MessageBox.Show(e.Result.ToString());
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                    return;
                }
                else
                {
                    //MessageBox.Show("会议时间变更成功！");
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MEETINGTIMEUPDATESUCCESSED"));
                    RefreshUI(saveType);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
        }




        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("MEETINGCHANGETIME");

        }
        public string GetStatus()
        {
            //return EmployeeEntry != null ? EmployeeEntry.CHECKSTATE : "";
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
                    SaveAndClose();
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
            //item = new NavigateItem
            //{
            //    Title = "员工资料",
            //    Tooltip = "员工详细",
            //    Url = "/Personnel/Employee"
            //};
            //items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (actiontype != FormTypes.Browse)
            {
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1",
                    Title = Utility.GetResourceStr("CHANGEANDCLOSE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
                };
                items.Add(item);
                item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "0",
                    Title = Utility.GetResourceStr("CHANGE"),
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"

                };
                items.Add(item);
            }

            return items;
        }

        private void SaveAndClose()
        {
            Save();
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


        //验证
        private bool Check()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            return true;
        }

        private void senddoctab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl TabC = sender as TabControl;
            if (TabC.SelectedIndex == 1)
            {
                string StrTitle = tmpvmeeting.meetinginfo.MEETINGTITLE;
                if (!string.IsNullOrEmpty(StrTitle))
                {

                    this.txtMessageTitle.Text = StrTitle + Utility.GetResourceStr("MEETINGCHANGENOTICES"); ;
                    String MeetingRoom = tmpvmeeting.meetinginfo.T_OA_MEETINGROOM.MEETINGROOMNAME; ;
                    string StrStartDt = "";
                    string StrStartTime = "";
                    string StrEndDt = "";
                    string StrEndTime = "";
                    StrStartDt = this.DPStart.Text.ToString();
                    StrEndDt = this.DPEnd.Text.ToString();
                    StrStartTime = tpStartTime.Value.Value.ToString("HH:mm");
                    StrEndTime = tpEndTime.Value.Value.ToString("HH:mm");
                    DateTime DtStart = System.Convert.ToDateTime(StrStartDt + " " + StrStartTime);
                    DateTime DtEnd = System.Convert.ToDateTime(StrEndDt + " " + StrEndTime);
                    T_OA_MEETINGMESSAGE aa = new T_OA_MEETINGMESSAGE();


                    if (!string.IsNullOrEmpty(MeetingRoom) && (DtStart < DtEnd))
                    {
                        txtMessageContent.RichTextBoxContext = this.TransDate(Convert.ToDateTime(tmpvmeeting.meetinginfo.STARTTIME), Convert.ToDateTime(tmpvmeeting.meetinginfo.ENDTIME), DtStart, DtEnd, MeetingRoom, StrTitle);
                    }

                }

            }
        }

        /// <summary>
        /// 会议变更时间
        /// </summary>
        /// <param name="OldStartDate"></param>
        /// <param name="OldEndDate"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <param name="MeetingRoom"></param>
        /// <param name="StrTitle"></param>
        /// <returns></returns>
        private byte[] TransDate(DateTime OldStartDate, DateTime OldEndDate, DateTime StartDate, DateTime EndDate, string MeetingRoom, string StrTitle)
        {
            //原定于XX年XX月XX日，X点X分至X点X分的会议，变更为XX年XX月XX日X点X分至X点X分，忘各位及时做好调整
            string StrReturn = "";
            //StrReturn = StartDate.Year.ToString() + Utility.GetResourceStr("YEAR") + StartDate.Month.ToString() + Utility.GetResourceStr("MONTH") +
            //    StartDate.Day.ToString() + Utility.GetResourceStr("DAY");
            //StrReturn = StartDate.ToShortDateString();
            StrReturn = Utility.GetResourceStr("OLDDECIDE") + OldStartDate.ToLongDateString().ToString() + ",";
            StrReturn += OldStartDate.ToShortTimeString() + Utility.GetResourceStr("TO") + OldEndDate.ToShortTimeString() + ".";
            StrReturn += Utility.GetResourceStr("NOWEDIT") + StartDate.ToLongDateString().ToString() + ",";
            StrReturn += StartDate.ToShortTimeString() + Utility.GetResourceStr("TO") + EndDate.ToShortTimeString() + ".";
            StrReturn += Utility.GetResourceStr("IN") + MeetingRoom + Utility.GetResourceStr("MEETINGROOM") + Utility.GetResourceStr("CELEBRATE") + StrTitle + Utility.GetResourceStr("NOTICES") + ",";
            StrReturn += Utility.GetResourceStr("HOPEEVERYONEINTIMECHANGE");
            //return StrReturn.ToArray<byte>;
            return System.Text.Encoding.Unicode.GetBytes(StrReturn);

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
