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
using SMT.SaaS.OA.UI.Class;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PublicInterfaceWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MeetingTypeForm : BaseForm,IClient, IEntityEditor
    {
        //public MeetingTypeForm()
        //{
        //    //InitializeComponent();
        //}

        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;
        private Action action;
        private string StrConverID = ""; //召集人ID
        
        T_OA_MEETINGTYPE TypeT;
        T_OA_MEETINGTYPE tmpTypeT = new T_OA_MEETINGTYPE();

        PublicServiceClient publicClient = new PublicServiceClient();
        
        //private bool IsSave = false; //保存后可以修改 将其设置为true 则进行修改
        #region 页面初始化

        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }

        public MeetingTypeForm(Action actionS, string TypeID)
        {
            MeetingClient.MeetingTypeAddCompleted += new EventHandler<MeetingTypeAddCompletedEventArgs>(MeetingType_MeetingTypeAddCompleted);
            MeetingClient.MeetingTypeUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MeetingType_MeetingTypeUpdateCompleted);
            MeetingClient.GetMeetingTypeSingleInfoByIdCompleted += new EventHandler<GetMeetingTypeSingleInfoByIdCompletedEventArgs>(MeetingType_GetMeetingTypeSingleInfoByIdCompleted);
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);
            InitializeComponent();
            
            TypeT = new T_OA_MEETINGTYPE();
            this.action = actionS;
            if (action == Action.Edit || action == Action.Read)
            {
                if (action == Action.Read)
                {
                    tbxMeetingType.IsEnabled = false;
                    txtRemindDay.IsEnabled = false;
                    txtCycle.IsEnabled = false;
                    txtConvener.IsEnabled = false;
                    btnFindHostMember.IsEnabled = false;
                    tbxDemo.IsEnabled = false;
                    rbtIsAutoyes.IsEnabled = false;
                    rbtIsAutoyes.IsEnabled = false;

                    //txtContent.HideControls();
                    this.txtContent.IsEnabled = false;
                    this.txtContent.IsReadOnly = true;

                    txtContent.BorderThickness = new Thickness(1.0);
                    txtContent.BorderBrush = new SolidColorBrush(Colors.Gray);
                }
                
                GetMeetingTypeByID(TypeID);
            }            
            else
            {
                StrConverID= Common.CurrentLoginUserInfo.EmployeeID;
                txtConvener.Text = Common.CurrentLoginUserInfo.EmployeeName;
                this.LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
            }
            
            //GetTestUserByName();
        }
        #endregion

        #region 获取富文本框信息



        void publicClient_GetContentCompleted(object sender, GetContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtContent.Document = e.Result;
            }
        }
        #endregion

        private void GetMeetingTypeByID(string TypeId)
        {   
            MeetingClient.GetMeetingTypeSingleInfoByIdAsync(TypeId);            
        }

        void MeetingType_GetMeetingTypeSingleInfoByIdCompleted(object sender, GetMeetingTypeSingleInfoByIdCompletedEventArgs e)
        {
            T_OA_MEETINGTYPE TypeTable = e.Result;
            if (TypeTable != null)
            {
                if (!string.IsNullOrEmpty(TypeTable.MEETINGTYPE))
                {
                    tbxMeetingType.Text = TypeTable.MEETINGTYPE;
                }
                if (!string.IsNullOrEmpty(TypeTable.REMARK))
                {
                    tbxDemo.Text = TypeTable.REMARK;
                }
                //TypeT = TypeTable;
                tmpTypeT = TypeTable;
                if (!string.IsNullOrEmpty(TypeTable.CONVENERNAME))
                {
                    this.txtConvener.Text = TypeTable.CONVENERNAME;
                }
                StrConverID = TypeTable.CONVENER;
                this.txtCycle.Text = TypeTable.CYCLE.ToString();
                this.txtRemindDay.Text = TypeTable.REMINDDAY.ToString();     
                //txtContent.RichTextBoxContext = TypeTable.CONTENT;
                publicClient.GetContentAsync(TypeTable.MEETINGTYPEID);
                if (TypeTable.ISAUTO == "1")
                {
                    this.rbtIsAutoyes.IsChecked = true;
                }
                else
                {
                    this.rbtIsAutono.IsChecked = true;
                    this.LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
                }
            }
        }

        #region "Update from"

      

        void client2_TestUsersUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("更新成功！");
            //this.ReloadData();
            //this.Close();
        }
        #endregion
        
        #region 关闭按钮
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }
        #endregion
        
        #region  添加按钮事件

        

        void MeetingType_MeetingTypeAddCompleted(object sender, MeetingTypeAddCompletedEventArgs e)
        {

            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result =="")
                {
                    UserInfo User = new UserInfo();
                    User.COMPANYID = TypeT.OWNERCOMPANYID;
                    User.DEPARTMENTID = TypeT.OWNERDEPARTMENTID;
                    User.POSTID = TypeT.OWNERPOSTID;
                    User.USERID = TypeT.OWNERID;
                    User.USERNAME = TypeT.OWNERNAME;
                    publicClient.AddContentAsync(TypeT.MEETINGTYPEID, TypeT.CONTENT, TypeT.OWNERCOMPANYID, "OA", "T_OA_MEETINGTYPE", User);

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "MEETINGTYPE"));
                    this.tbxMeetingType.IsEnabled = false;
                    action = Action.Edit; //将保存状态改为  修改

                    RefreshUI(saveType);
                }
                else
                {

                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "MEETINGTYPE"));
                }
            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            

        }

          

        

        void MeetingType_MeetingTypeUpdateCompleted(object sender,System.ComponentModel.AsyncCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                UserInfo User = new UserInfo();
                User.COMPANYID = TypeT.OWNERCOMPANYID;
                User.DEPARTMENTID = TypeT.OWNERDEPARTMENTID;
                User.POSTID = TypeT.OWNERPOSTID;
                User.USERID = TypeT.OWNERID;
                User.USERNAME = TypeT.OWNERNAME;
                //publicClient.UpdateContentAsync(tmpSendDocT.APPROVALID, tmpSendDocT.CONTENT, tmpSendDocT.OWNERCOMPANYID, "OA", "T_OA_APPROVAL");               
                publicClient.UpdateContentAsync(TypeT.MEETINGTYPEID, TypeT.CONTENT, User);
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGTYPE"));
                RefreshUI(saveType);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
            }
            
        }
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "MEETINGTYPE");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "MEETINGTYPE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "MEETINGTYPE");
            }
            
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
            if (action != Action.Read)
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
                string name = "";
                string remark = "";
                string StrCycle = "";
                string StrDays = "";
                byte[] StrContent ;
                string StrIsAuto = "";
                string StrConverName = "";
                StrConverName = this.txtConvener.Text.ToString();
                name = tbxMeetingType.Text.ToString();
                remark = tbxDemo.Text.ToString();
                StrCycle = this.txtCycle.Text.ToString();
                StrDays = this.txtRemindDay.Text.ToString();
                //StrContent = txtContent.RichTextBoxContext;
                StrContent = txtContent.Document;
                if (this.rbtIsAutoyes.IsChecked == true)
                {
                    StrIsAuto = "1";
                }
                if(rbtIsAutono.IsChecked == true)
                {
                    StrIsAuto = "0";
                }
                if (!string.IsNullOrEmpty(StrCycle) && !string.IsNullOrEmpty(StrDays))
                {
                    if (int.Parse(StrCycle) < int.Parse(StrDays))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AHEADCAUTIONDAYSNOTCYLDAYS"));//提前提醒天数不能大于周期
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(StrCycle) && string.IsNullOrEmpty(StrDays))
                {
                    //周期不为空 则提前提醒天数不能为空
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "AHEADCAUTIONDAYS"));//提前提醒天数不能为空
                    return;
                }
                else if (string.IsNullOrEmpty(StrCycle) && !string.IsNullOrEmpty(StrDays))
                {
                    //提前提醒天数不为空 则周期不能为空
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "CYLDAYS"));//周期不能为空
                    return;
                }
                if (StrContent != null)
                {
                    if (StrContent.Length == 0)
                    {

                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("会议模版内容"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                        this.txtContent.Focus();
                        return;

                    }
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("会议模版内容"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.txtContent.Focus();
                }
                
                if (action == Action.Add)
                {
                    //string StrCONVENER
                    TypeT = new T_OA_MEETINGTYPE();
                    
                    TypeT.MEETINGTYPEID = System.Guid.NewGuid().ToString();
                    TypeT.MEETINGTYPE = name;
                    TypeT.REMARK = remark;
                    if (string.IsNullOrEmpty(name))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REQUIRED", "MEETINGTYPE"));
                        return;
                    }

                    TypeT.CONVENER = StrConverID;
                    TypeT.CONVENERNAME = StrConverName;
                    TypeT.CONTENT = StrContent;
                    TypeT.ISAUTO = StrIsAuto;
                    if (!string.IsNullOrEmpty(StrCycle))
                    {
                        TypeT.CYCLE = System.Convert.ToInt32(StrCycle);
                    }
                    if (!string.IsNullOrEmpty(StrDays))
                    {
                        TypeT.REMINDDAY = System.Convert.ToInt32(StrDays);
                    }

                    TypeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    TypeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    TypeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    TypeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    TypeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    TypeT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    TypeT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    TypeT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    TypeT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    TypeT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    TypeT.UPDATEUSERNAME = "";

                    TypeT.UPDATEDATE = null;
                    TypeT.UPDATEUSERID = "";
                    TypeT.CREATEDATE = System.DateTime.Now;
                    tmpTypeT = TypeT;  //将该实体赋给变量  便于 修改
                    RefreshUI(RefreshedTypes.ProgressBar);
                    MeetingClient.MeetingTypeAddAsync(TypeT);                    
                }
                else
                {

                    
                    TypeT = tmpTypeT;
                    
                    if (string.IsNullOrEmpty(name))
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("REQUIRED", "MEETINGTYPE"));
                        return;
                    }
                    if (StrIsAuto == "0")
                    {
                        StrCycle = "";
                        StrDays = "";//如果设为不是自动发起 则将周期、提前提醒天数清空
                    }
                    if (!string.IsNullOrEmpty(StrCycle))
                    {
                        TypeT.CYCLE = System.Convert.ToInt32(StrCycle);
                    }
                    if (!string.IsNullOrEmpty(StrDays))
                    {
                        TypeT.REMINDDAY = System.Convert.ToInt32(StrDays);
                    }
                    TypeT.CONVENER = StrConverID;
                    TypeT.CONVENERNAME = StrConverName;
                    TypeT.CONTENT = StrContent;
                    TypeT.ISAUTO = StrIsAuto;
                    TypeT.MEETINGTYPE = name;
                    TypeT.REMARK = remark;
                    TypeT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    TypeT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                    TypeT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    TypeT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    TypeT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    TypeT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    TypeT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                    TypeT.UPDATEDATE = System.DateTime.Now;
                    RefreshUI(RefreshedTypes.ProgressBar);
                    MeetingClient.MeetingTypeUpdateAsync(TypeT);
                }
            }
            catch (Exception ex)
            {
                //HtmlPage.Window.Alert(ex.ToString());
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                return;
            }
        }

        private void SaveAndClose()
        {
            
            Save();
        }


        


        #endregion

        private void rbtIsAutoyes_Click(object sender, RoutedEventArgs e)
        {            
            this.LayoutRoot.RowDefinitions[2].Height = new GridLength(30);
        }

        private void tbtIsAutono_Click(object sender, RoutedEventArgs e)
        {            
            this.LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
            //LayoutRoot.RowDefinitions.Remove(gridauto);
        }

        private void btnFindHostMember_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //tmpHostMember = companyInfo;
                    StrConverID = companyInfo.ObjectID;
                    txtConvener.Text = companyInfo.ObjectName;
                    
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        private void txtCycle_KeyUp(object sender, KeyEventArgs e)
        {
            GlobalFunction.TextBoxInputInt(sender, e, "CYCLEDAYS");            
        }

        private void txtRemindDay_KeyUp(object sender, KeyEventArgs e)
        {
            GlobalFunction.TextBoxInputInt(sender, e, "AHEADCAUTIONDAYS"); 
            
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
