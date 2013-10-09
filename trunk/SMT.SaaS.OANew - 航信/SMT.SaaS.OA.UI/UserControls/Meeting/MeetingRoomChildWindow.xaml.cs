using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls.Meeting
{
    public partial class MeetingRoomChildWindow : BaseForm, IClient, IEntityEditor
    {

        #region 初始化数据
        private Action actions;
        private SMTLoading loadbar = new SMTLoading();
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private string StrAddUser = "";
        private string StrAddPosition = "";
        private string StrCompanyId = "";
        private string StrDepartmentID = "";
        private T_OA_MEETINGROOM DataContextRoom { get; set; }
        //private string saveType = "1";       //保存方式 0:保存 1:保存并关闭
        RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;
        public delegate void refreshGridView();
        private T_OA_MEETINGROOM tmpmeetingRoom = new T_OA_MEETINGROOM();
        private T_OA_MEETINGROOM roomObj;
        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        public MeetingRoomChildWindow(Action operationType, T_OA_MEETINGROOM RoomObj)
        {
            InitializeComponent();
            actions = operationType;
            roomObj = RoomObj;
            this.Loaded += new RoutedEventHandler(MeetingRoomChildWindow_Loaded);
        }

        void MeetingRoomChildWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MeetingClient.MeetingRoomdAddCompleted += new EventHandler<MeetingRoomdAddCompletedEventArgs>(RoomClient_MeetingRoomdAddCompleted);
            MeetingClient.MeetingRoomUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingClient_MeetingRoomUpdateCompleted);
            MeetingClient.GetMeetingRoomByIdCompleted += new EventHandler<GetMeetingRoomByIdCompletedEventArgs>(MeetingClient_GetMeetingRoomByIDCompleted);
            switch (actions)
            {
                case Action.Add:
                    //this.tbtitle.Text = Utility.GetResourceStr("ADDTITLE", "MEETINGROOM"); 

                    break;
                case Action.Edit:
                    //this.tbtitle.Text = Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                    this.txtRoom.IsEnabled = false;
                    Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                    GetMeetingRoomByID(roomObj);
                    break;
                case Action.Read:
                    //设为只读

                    SetReadOnly();
                    Utility.GetResourceStr("VIEWTITLE", "MEETINGROOM");
                    GetMeetingRoomByID(roomObj);
                    break;
            }
        }


        void SetReadOnly()
        {
            this.IsEnabled = false; //设置该页面为只读

        }

        #endregion

        #region 获取会议室信息

        private void BindData()
        {
            this.DataContext = DataContextRoom;
        }

        private void GetMeetingRoomByID(T_OA_MEETINGROOM RoomObj)
        {
            if (!string.IsNullOrEmpty(RoomObj.MEETINGROOMNAME))
            {

                txtRoom.Text = RoomObj.MEETINGROOMNAME;
            }
            tmpmeetingRoom = RoomObj;
            GetCompanyNameByCompanyID(RoomObj.COMPANYID);
            //备注
            txtPosition.Text = RoomObj.LOCATION.IsNull() ? string.Empty : RoomObj.LOCATION;
            txtremark.Text = RoomObj.REMARK.IsNull() ? string.Empty : RoomObj.REMARK;
            txtseat.Text = RoomObj.SEAT.ToString();
            txtarea.Text = RoomObj.AREA.ToString();
            if (RoomObj.ROSTRUM == "1")
            {
                this.chxRostrum.IsChecked = true;
            }
            else
            {
                this.chxRostrum.IsChecked = false;
            }
            if (RoomObj.VIDEO == "1")
            {
                this.chxVideo.IsChecked = true;
            }
            else
            {
                this.chxVideo.IsChecked = false;
            }
            if (RoomObj.AUDIO == "1")
            {
                this.chxAudio.IsChecked = true;

            }
            else
            {
                this.chxAudio.IsChecked = false;
            }
            if (RoomObj.NETWORK == "1")
            {
                this.chxNetwork.IsChecked = true;
            }
            else
            {
                this.chxNetwork.IsChecked = false;
            }
            if (RoomObj.WIFI == "1")
            {
                this.chxWifi.IsChecked = true;
            }
            else
            {
                this.chxWifi.IsChecked = false;
            }
            if (RoomObj.WATERDISPENSER == "1")
            {
                this.chxWaterDispenser.IsChecked = true;
            }
            else
            {
                this.chxWaterDispenser.IsChecked = false;
            }
            if (RoomObj.TEL == "1")
            {
                this.chxTel.IsChecked = true;
            }
            else
            {
                this.chxTel.IsChecked = false;
            }
            if (RoomObj.PROJECTOR == "1")
            {
                this.chxProjector.IsChecked = true;
            }
            else
            {
                this.chxProjector.IsChecked = false;
            }
            if (RoomObj.AIRCONDITIONING == "1")
            {
                this.chxAirConditioning.IsChecked = true;
            }
            else
            {
                this.chxAirConditioning.IsChecked = false;
            }
        }


        void MeetingClient_GetMeetingRoomByIDCompleted(object sender, GetMeetingRoomByIdCompletedEventArgs e)
        {
            T_OA_MEETINGROOM Room = e.Result;
            tmpmeetingRoom = Room;
            if (Room != null)
            {
                //MeetingManagementServiceClient mrClient = new MeetingManagementServiceClient();
                if (!string.IsNullOrEmpty(Room.MEETINGROOMNAME))
                {
                    txtRoom.Text = Room.MEETINGROOMNAME;
                }

                GetCompanyNameByCompanyID(Room.COMPANYID);
                //备注
                txtremark.Text = Room.REMARK;
            }
        }

        #endregion

        #region 填充公司名称
        private void GetCompanyNameByCompanyID(string StrCompanyID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();

            Organ.GetCompanyByIdCompleted += new EventHandler<GetCompanyByIdCompletedEventArgs>(Organ_GetCompanyByIdCompleted);
            Organ.GetCompanyByIdAsync(StrCompanyID);
        }
        void Organ_GetCompanyByIdCompleted(object sender, GetCompanyByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    T_HR_COMPANY company = new T_HR_COMPANY();
                    company = e.Result;
                    StrCompanyId = company.COMPANYID;

                    txtcompany.Text = company.CNAME;
                }
            }
        }
        #endregion

        #region 事件按钮

        void RoomClient_MeetingRoomdAddCompleted(object sender, MeetingRoomdAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "MEETINGROOM"));
                    actions = Action.Edit;
                    this.txtRoom.IsEnabled = false;
                    RefreshUI(saveType);
                    RefreshUI(RefreshedTypes.All);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "MEETINGROOM"));
                    return;
                }
            }
        }

        void MeetingClient_MeetingRoomUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {

                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGROOM"));
                RefreshUI(saveType);
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), e.Error.Message);
                return;
                //RefreshUI(saveType);
            }

        }

        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            if (actions == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "MEETINGROOM");
            }
            else if (actions == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "MEETINGROOM");
            }
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
                    saveType = RefreshedTypes.LeftMenu;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();

            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (actions != Action.Read)
            {
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
            }
            return items;
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

        #region 保存
        private void Save()
        {
            try
            {
                if (actions == Action.Read)            //查看
                {
                    RefreshUI(saveType);
                }
                else
                {
                    if (Check())
                    {
                        string name = "";
                        string remark = "";
                        string address = "";
                        string IsRostrum = "";//主席台
                        string IsVideo = "";//视频
                        string IsAudio = "";//音频
                        string IsNetwork = "";//有线网络
                        string IsWifi = "";//无线网络
                        string IsTel = "";//电话
                        string IsProjector = "";//投影仪
                        string IsAirConditioning = "";//空调
                        string IsWaterDispenser = "";//风扇
                        string StrSeat = "";
                        string StrArea = ""; //房间面积
                        StrSeat = this.txtseat.Text.ToString();
                        StrArea = this.txtarea.Text.ToString();
                        if (!string.IsNullOrEmpty(StrSeat))
                        {
                            if (!(System.Convert.ToInt64(StrSeat) > 0))
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SEATCOUNTMUSTLARGERZERO"));
                                return;
                            }
                        }

                        if (!string.IsNullOrEmpty(StrArea))
                        {
                            if (!(System.Convert.ToInt64(StrArea) > 0))
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ROOMAREAMUSTLARGERZERO"));
                                return;
                            }
                        }

                        name = txtRoom.Text.ToString();
                        remark = txtremark.Text.ToString();
                        address = txtPosition.Text.ToString();
                        if (this.chxRostrum.IsChecked == true)
                        {
                            IsRostrum = "1";
                        }
                        else
                        {
                            IsRostrum = "0";
                        }
                        if (this.chxVideo.IsChecked == true)
                        {
                            IsVideo = "1";
                        }
                        else
                        {
                            IsVideo = "0";
                        }
                        if (this.chxAudio.IsChecked == true)
                        {
                            IsAudio = "1";
                        }
                        else
                        {
                            IsAudio = "0";
                        }
                        if (this.chxNetwork.IsChecked == true)
                        {
                            IsNetwork = "1";
                        }
                        else
                        {
                            IsNetwork = "0";
                        }
                        if (this.chxWifi.IsChecked == true)
                        {
                            IsWifi = "1";
                        }
                        else
                        {
                            IsWifi = "0";
                        }
                        if (this.chxProjector.IsChecked == true)
                        {
                            IsProjector = "1";
                        }
                        else
                        {
                            IsProjector = "0";
                        }
                        if (this.chxTel.IsChecked == true)
                        {
                            IsTel = "1";
                        }
                        else
                        {
                            IsTel = "0";
                        }
                        if (this.chxWaterDispenser.IsChecked == true)
                        {
                            IsWaterDispenser = "1";
                        }
                        else
                        {
                            IsWaterDispenser = "0";
                        }
                        if (this.chxAirConditioning.IsChecked == true)
                        {
                            IsAirConditioning = "1";
                        }
                        else
                        {
                            IsAirConditioning = "0";
                        }

                        if (actions == Action.Add)
                        {
                            T_OA_MEETINGROOM MeetingRoom = new T_OA_MEETINGROOM();

                            MeetingRoom.MEETINGROOMID = System.Guid.NewGuid().ToString();

                            MeetingRoom.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            MeetingRoom.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            MeetingRoom.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            MeetingRoom.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            MeetingRoom.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            MeetingRoom.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            MeetingRoom.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            MeetingRoom.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            MeetingRoom.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            MeetingRoom.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            MeetingRoom.UPDATEUSERNAME = "";

                            MeetingRoom.ROSTRUM = IsRostrum;
                            MeetingRoom.VIDEO = IsVideo;
                            MeetingRoom.AUDIO = IsAudio;
                            MeetingRoom.WIFI = IsWifi;
                            MeetingRoom.WATERDISPENSER = IsWaterDispenser;
                            MeetingRoom.NETWORK = IsNetwork;
                            MeetingRoom.TEL = IsTel;
                            MeetingRoom.PROJECTOR = IsProjector;
                            MeetingRoom.AIRCONDITIONING = IsAirConditioning;

                            MeetingRoom.AREA = System.Convert.ToInt64(StrSeat);
                            MeetingRoom.SEAT = System.Convert.ToInt64(StrSeat);



                            MeetingRoom.MEETINGROOMNAME = name;
                            MeetingRoom.REMARK = remark;
                            MeetingRoom.LOCATION = address;

                            MeetingRoom.CREATEDATE = System.DateTime.Now;
                            MeetingRoom.UPDATEUSERID = "";
                            MeetingRoom.UPDATEDATE = null;
                            MeetingRoom.COMPANYID = StrCompanyId;

                            if (!string.IsNullOrEmpty(name))
                            {
                                try
                                {

                                    tmpmeetingRoom = MeetingRoom;
                                    RefreshUI(RefreshedTypes.ShowProgressBar);
                                    MeetingClient.MeetingRoomdAddAsync(MeetingRoom);

                                }
                                catch (Exception ex)
                                {
                                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                                    RefreshUI(RefreshedTypes.ProgressBar);
                                    return;
                                }
                            }
                            else
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "MEETINGROOM"));
                                this.txtRoom.Focus();
                                return;
                            }
                        }
                        else
                        {
                            T_OA_MEETINGROOM MeetingRoom = new T_OA_MEETINGROOM();
                            MeetingRoom = tmpmeetingRoom;

                            name = txtRoom.Text.ToString();

                            remark = txtremark.Text.ToString();
                            MeetingRoom.MEETINGROOMID = tmpmeetingRoom.MEETINGROOMID;
                            MeetingRoom.MEETINGROOMNAME = name;
                            MeetingRoom.REMARK = remark;
                            MeetingRoom.LOCATION = address;

                            MeetingRoom.ROSTRUM = IsRostrum;
                            MeetingRoom.VIDEO = IsVideo;
                            MeetingRoom.AUDIO = IsAudio;
                            MeetingRoom.WIFI = IsWifi;
                            MeetingRoom.WATERDISPENSER = IsWaterDispenser;
                            MeetingRoom.NETWORK = IsNetwork;
                            MeetingRoom.TEL = IsTel;
                            MeetingRoom.PROJECTOR = IsProjector;
                            MeetingRoom.AIRCONDITIONING = IsAirConditioning;

                            MeetingRoom.AREA = System.Convert.ToInt64(txtarea.Text.ToString());
                            MeetingRoom.SEAT = System.Convert.ToInt64(txtseat.Text.ToString());

                            MeetingRoom.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            MeetingRoom.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            MeetingRoom.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            MeetingRoom.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            MeetingRoom.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                            MeetingRoom.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                            MeetingRoom.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                            MeetingRoom.UPDATEDATE = System.DateTime.Now;
                            MeetingRoom.COMPANYID = StrCompanyId;


                            try
                            {
                                //RoomClient.MeetingRoomUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingRoom_UpdateCompleted);
                                RefreshUI(RefreshedTypes.ProgressBar);
                                MeetingClient.MeetingRoomUpdateAsync(MeetingRoom);


                            }
                            catch (Exception ex)
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                                RefreshUI(RefreshedTypes.ProgressBar);
                                return;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                return;
            }
        }

        private void Close()
        {
            RefreshUI(saveType);
        }

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
        #endregion


        private void CompanyObject_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Company;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();

                    StrCompanyId = companyInfo.ObjectID;
                    txtcompany.Text = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
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
