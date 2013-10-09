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
using System.Windows.Navigation;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;

using SMT.SaaS.OA.UI;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.CommForm;
//using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SaaS.OA.UI.UserControls.Meeting
{
    public partial class MeetingRoomForm : BaseForm,IClient, IEntityEditor
    {
        public MeetingRoomForm()
        {
            InitializeComponent();
        }

        #region 初始化
        private Action actions;
        private SMTLoading loadbar = new SMTLoading(); 
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private string StrAddUser = "";
        private string StrAddPosition = "";
        private string StrCompanyId = "";
        private string StrDepartmentID = "";
        
        //private string saveType = "1";       //保存方式 0:保存 1:保存并关闭
        RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;
        public delegate void refreshGridView();
        private T_OA_MEETINGROOM tmpmeetingRoom = new T_OA_MEETINGROOM();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
       
        
        //private string testUsername = "Add";
        public MeetingRoomForm(Action operationType, T_OA_MEETINGROOM RoomObj)
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            loadbar.Start();
            MeetingClient.MeetingRoomdAddCompleted += new EventHandler<MeetingRoomdAddCompletedEventArgs>(RoomClient_MeetingRoomdAddCompleted);
            MeetingClient.GetMeetingRoomByIdCompleted += new EventHandler<GetMeetingRoomByIdCompletedEventArgs>(MeetingClient_GetMeetingRoomByIDCompleted);
            MeetingClient.MeetingRoomUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingRoom_UpdateCompleted);
            switch (operationType)
            { 
                case Action.Add:
                    //this.tbtitle.Text = Utility.GetResourceStr("ADDTITLE", "MEETINGROOM"); ;
                    break;
                case Action.Edit:
                    //this.tbtitle.Text = Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                    this.tbxMeetingRoomName.IsReadOnly = true;
                    Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                    GetMeetingRoomByID(RoomObj);
                    break;
                case Action.Read:
                    //this.tbtitle.Text = Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                    this.tbxMeetingRoomName.IsReadOnly = true;
                    this.tbxDemo.IsEnabled = false;
                    this.tbxMeetingRoomName.IsEnabled = false;
                    this.tbxMeetingAddress.IsEnabled = false;
                    Utility.GetResourceStr("VIEWTITLE", "MEETINGROOM");
                    GetMeetingRoomByID(RoomObj);
                    break;
            }
            if (operationType == Action.Edit)
            {
                //this.tbtitle.Text = Utility.GetResourceStr("EDITTITLE", "MEETINGROOM"); 
                this.tbxMeetingRoomName.IsReadOnly = true;
                Utility.GetResourceStr("EDITTITLE", "MEETINGROOM");
                GetMeetingRoomByID(RoomObj);
            }
            actions = operationType;
            loadbar.Stop();
            
          
        }

        #endregion

        #region 获取会议室信息

        private void GetMeetingRoomByID(T_OA_MEETINGROOM RoomObj)
        {
            if (!string.IsNullOrEmpty(RoomObj.MEETINGROOMNAME))
            {
                tbxMeetingRoomName.Text = RoomObj.MEETINGROOMNAME;
            }

            GetCompanyNameByCompanyID(RoomObj.COMPANYID);
            //备注
            
            tbxMeetingAddress.Text = RoomObj.LOCATION;
            tbxDemo.Text = RoomObj.REMARK;
            
            
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
                    tbxMeetingRoomName.Text = Room.MEETINGROOMNAME;
                }
                
                GetCompanyNameByCompanyID(Room.COMPANYID);
                //备注
                
                tbxDemo.Text = Room.REMARK;
            }
        }

        #endregion

        #region 添加COMPLATED        

        void RoomClient_MeetingRoomdAddCompleted(object sender, MeetingRoomdAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "MEETINGROOM"));
                    actions = Action.Edit;
                    tbxMeetingRoomName.IsEnabled = false;
                    RefreshUI(saveType);
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString()));
                    return;
                }
            }
            else
            {
                MessageBox.Show(e.ToString());
            }
            loadbar.Stop();
            
        }

        

        #endregion

        #region 修改按钮
        

        void MeetingRoom_UpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGROOM"));
                RefreshUI(saveType);

            }
            else
            {
                //MessageBox.Show(e.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), e.Error.Message);
                return;
                //RefreshUI(saveType);
            }
            loadbar.Stop();
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

                        name = tbxMeetingRoomName.Text.ToString();
                        remark = tbxDemo.Text.ToString();
                        address = tbxMeetingAddress.Text.ToString();
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
                                    loadbar.Start();
                                    tmpmeetingRoom = MeetingRoom;
                                    MeetingClient.MeetingRoomdAddAsync(MeetingRoom);
                                }
                                catch (Exception ex)
                                {
                                    //HtmlPage.Window.Alert(ex.ToString());
                                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());

                                }
                            }
                            else
                            {
                                //MessageBox.Show("会议室名不能为空");
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "MEETINGROOM"));
                                this.tbxMeetingRoomName.Focus();
                                return;
                            }
                        }
                        else
                        {
                            T_OA_MEETINGROOM MeetingRoom = new T_OA_MEETINGROOM();
                            MeetingRoom = tmpmeetingRoom;
                            
                            name = tbxMeetingRoomName.Text.ToString();
                            remark = tbxDemo.Text.ToString();
                            MeetingRoom.MEETINGROOMID = tmpmeetingRoom.MEETINGROOMID;
                            MeetingRoom.MEETINGROOMNAME = name;
                            MeetingRoom.REMARK = remark;
                            MeetingRoom.LOCATION = address;

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
                                loadbar.Start();
                                MeetingClient.MeetingRoomUpdateAsync(MeetingRoom);


                            }
                            catch (Exception ex)
                            {
                                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "MEETINGROOM"));

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
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

        //#region 选择公司
        
        
        //private void CompanyObject_FindClick(object sender, EventArgs e)
        //{
        //    //OrganizationLookupForm lookup = new OrganizationLookupForm();
        //    //lookup.SelectedObjType = OrgTreeItemTypes.Company;


        //    //lookup.SelectedClick += (obj, ev) =>
        //    //{
        //    //    CompanyObject.DataContext = lookup.SelectedObj;
        //    //    if (lookup.SelectedObj is T_HR_COMPANY)
        //    //    {
        //    //        T_HR_COMPANY tmp = lookup.SelectedObj as T_HR_COMPANY;
        //    //        //tmpmeetingRoom.COMPANYID = tmp.COMPANYID;
        //    //        StrCompanyId = tmp.COMPANYID;
        //    //        CompanyObject.DisplayMemberPath = "CNAME";
                    
        //    //    }
        //    //};
        //    //lookup.Show();
        //}
        //#endregion


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
                    CompanyObject.Text = company.CNAME;
                    //CompanyObject.DisplayMemberPath = "CNAME";
                    //CompanyObject.DataContext = company;
                }
            }
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
                    CompanyObject.Text = companyInfo.ObjectName;
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
