using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using System;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MyMeetingDetailInfoForm : BaseForm,IClient, IEntityEditor
    {
        
        private RefreshedTypes refreshType = RefreshedTypes.All;
        string MeetingContentID = "";
        private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();
        string StrMeetingInfoid = "";//会议申请ID
        public MyMeetingDetailInfoForm(V_MyMeetingInfosManagement objVMeeting)
        {
            InitializeComponent();
            ctrFile.SystemName = "OA";
            ctrFile.ModelName = "MeetingApp";
            ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
            ShowMeetingInfos(objVMeeting);
            StrMeetingInfoid = objVMeeting.OAMeetingInfoT.MEETINGINFOID;
            this.Loaded += new RoutedEventHandler(MyMeetingDetailInfoForm_Loaded);
        }

        void MyMeetingDetailInfoForm_Loaded(object sender, RoutedEventArgs e)
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
            tblTitle.Text = Utility.GetResourceStr("MEETINGDETAIL"); 
            this.tblMeetingTitle.Text = objV.OAMeetingInfoT.MEETINGTITLE;
            if (!string.IsNullOrEmpty(objV.meetingroom.LOCATION) && !string.IsNullOrEmpty(objV.meetingroom.MEETINGROOMNAME))
            {
                this.tblMeetingRoom.Text = "[" + objV.meetingroom.LOCATION + "]" + " " + objV.meetingroom.MEETINGROOMNAME;
            }
            else
            {
                this.tblMeetingRoom.Text = objV.meetingroom.MEETINGROOMNAME;
            }
            this.tblStartTime.Text = Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortDateString() + " " + Convert.ToDateTime(objV.OAMeetingInfoT.STARTTIME).ToShortTimeString();
            this.tblEndTime.Text = Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortDateString() + " " + Convert.ToDateTime(objV.OAMeetingInfoT.ENDTIME).ToShortTimeString();
            this.tblMeetingMember.Text = objV.OAMeetingInfoT.COUNT.ToString();                        
            //tblMeetingContent.RichTextBoxContext = objV.OAMeetingInfoT.CONTENT;
            if (!string.IsNullOrEmpty(objV.OAMeetingContentT.CONTENT))
            {
                this.tblMyMeetingContent.Text = objV.OAMeetingContentT.CONTENT;
            }
            else
            {
                this.tblMyMeetingContent.Text = "还没有上传会议内容";
            }
            //this.tblMyAccessory.Text = objV.OAMeetingStaffT.FILENAME;
            //this.tblMyMeetingContent.RichTextBoxContext = objV.OAMeetingContentT.CONTENT;
            
            MeetingContentID=objV.OAMeetingContentT.MEETINGCONTENTID;
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
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
            };

            items.Add(item);

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
        private void Save()
        {

        }

        private void SaveAndClose()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            ctrFile.Load_fileData(MeetingContentID);
            ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            ctrFile.InitBtn(Visibility.Collapsed, Visibility.Collapsed);
        }

        #region IForm 成员

        public void ClosedWCFClient()
        {
            //throw new System.NotImplementedException();
        }

        public bool CheckDataContenxChange()
        {
            throw new System.NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
