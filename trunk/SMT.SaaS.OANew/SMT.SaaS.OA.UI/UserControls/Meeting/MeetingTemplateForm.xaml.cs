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
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class MeetingTemplateForm : BaseForm,IClient, IEntityEditor
    {
        private SmtOACommonOfficeClient MeetingClient = new SmtOACommonOfficeClient();
        private string strMeetingType = ""; //会议类型
        private Action action;
        private T_OA_MEETINGTEMPLATE meetingtemplate = new T_OA_MEETINGTEMPLATE();
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;
        private T_OA_MEETINGTYPE SelectMeetingType = new T_OA_MEETINGTYPE();
        private string tmeplateId;
        private SMTLoading loadbar = new SMTLoading(); 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">操作类型</param>
        /// <param name="tmeplateID">模板id</param>
        /// <param name="typeName">会议类型</param>
        public MeetingTemplateForm(Action action,string tmeplateID,string typeName)
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MeetingTemplateForm_Loaded);
            strMeetingType = typeName;
            this.action = action;
            this.tmeplateId = tmeplateID;
        }

        private void InitEvent()
        {
            PARENT.Children.Add(loadbar);
            MeetingClient.GetMeetingTypeTemplateSingleInfoByIdCompleted += new EventHandler<GetMeetingTypeTemplateSingleInfoByIdCompletedEventArgs>(TemplateClient_GetMeetingTypeTemplateSingleInfoByIdCompleted);
            MeetingClient.MeetingTypeTemplateAddCompleted += new EventHandler<MeetingTypeTemplateAddCompletedEventArgs>(TemplateClient_MeetingTypeTemplateAddCompleted);
            //MeetingClient.MeetingTypeTemplateUpdateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(TemplateClient_MeetingTypeTemplateUpdateCompleted);
            MeetingClient.MeetingTypeTemplateUpdateCompleted += new EventHandler<MeetingTypeTemplateUpdateCompletedEventArgs>(MeetingClient_MeetingTypeTemplateUpdateCompleted);
            MeetingClient.GetMeetingTypeNameInfosToComboxCompleted += new EventHandler<GetMeetingTypeNameInfosToComboxCompletedEventArgs>(typeClient_GetMeetingTypeNameInfosToComboxCompleted);
          
            loadbar.Start();
     
            if (action == Action.Edit || action == Action.Read)
            {
                if (action == Action.Read)
                {
                    this.txtTemplateName.IsEnabled = false;
                    this.cbMeetingType.IsEnabled = false;

                    txtTemplateContent.HideControls();
                }
                GetMeetingTypeTemplateByTemplateID(tmeplateId);
            }
            else
            {
                combox_SelectSource();
            }
        }

        void MeetingTemplateForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
        }

        void MeetingClient_MeetingTypeTemplateUpdateCompleted(object sender, MeetingTypeTemplateUpdateCompletedEventArgs e)
        {
            loadbar.Stop();
            if (!e.Cancelled)
            {
                //MessageBox.Show("会议类型模板信息修改成功！");
                if (e.Error == null)
                {
                    if (e.Result == "")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGTYPETEMPLATE"));
                        RefreshUI(saveType);
                    }
                    else
                    {
                        if (e.Result != "ERROR")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.Result, "MEETINGTYPETEMPLATE"));
                        }
                        else
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("SYSTEMERRORPLEASELINKDADMIN"));
                        }
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                    return;
                }

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                //MessageBox.Show(e.ToString());
            }
            
        }


        #region COMBOX 设置数据源

        private void combox_SelectSource()
        {            
            MeetingClient.GetMeetingTypeNameInfosToComboxAsync();
            
        }

        void typeClient_GetMeetingTypeNameInfosToComboxCompleted(object send, GetMeetingTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbMeetingType.Items.Clear();
                this.cbMeetingType.ItemsSource = e.Result;
                this.cbMeetingType.DisplayMemberPath = "MEETINGTYPE";
      
                if (SelectMeetingType.MEETINGTYPEID != null)
                {
                    foreach (var item in cbMeetingType.Items)
                    {
                        T_OA_MEETINGTYPE dict = item as T_OA_MEETINGTYPE;
                        if (dict != null)
                        {
                            if (dict.MEETINGTYPE == SelectMeetingType.MEETINGTYPE)
                            {
                                cbMeetingType.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    //cbMeetingType.SelectedItem = SelectMeetingType;
                }
                else
                {
                    cbMeetingType.SelectedIndex = 0;
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("FOUNDMEETINGTYPE"), Utility.GetResourceStr("CONFIRMBUTTON"));
            }
            loadbar.Stop();
        }
        #endregion

        //从管理模板出获取数据
        private void GetMeetingTypeTemplateByTemplateID(string StrTemplateID)
        {
            MeetingClient.GetMeetingTypeTemplateSingleInfoByIdAsync(StrTemplateID);
        }

        void TemplateClient_GetMeetingTypeTemplateSingleInfoByIdCompleted(object sender, GetMeetingTypeTemplateSingleInfoByIdCompletedEventArgs e)
        {

            if (e.Result != null)
            {
                T_OA_MEETINGTEMPLATE TemplateTable = e.Result;
                txtTemplateName.Text = TemplateTable.TEMPLATENAME;
                
                txtTemplateContent.RichTextBoxContext = TemplateTable.CONTENT;
                SelectMeetingType = TemplateTable.T_OA_MEETINGTYPE;
                meetingtemplate = TemplateTable;
                combox_SelectSource();
            }


        }


        #region IEntityEditor
        public string GetTitle()
        {
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "MEETINGTEMPLATE");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "MEETINGTEMPLATE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "MEETINGTEMPLATE");
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
                string StrTemplateName = "";
                string StrMeetingType = "";
                byte[] StrContent;
                
                StrTemplateName = txtTemplateName.Text.ToString();
                if (cbMeetingType.SelectedIndex > -1)
                {                    
                    SelectMeetingType = cbMeetingType.SelectedItem as T_OA_MEETINGTYPE;
                    StrMeetingType = SelectMeetingType.MEETINGTYPEID.ToString();
                }
                StrContent = txtTemplateContent.RichTextBoxContext;

                
                if (string.IsNullOrEmpty(StrMeetingType))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "MEETINGTYPE"));                                        
                }
                if (string.IsNullOrEmpty(StrTemplateName))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "OATEMPLATENAME"));
                }

                if (StrContent == null)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "TEMPLATECONTENT"));
                    return;
                }
                
                if (CheckMeetingTemplate())
                {
                    loadbar.Start();
                    //T_OA_MEETINGTEMPLATE TemplateT = new T_OA_MEETINGTEMPLATE();
                    if (action == Action.Add)
                    {

                        meetingtemplate.MEETINGTEMPLATEID = System.Guid.NewGuid().ToString();
                        meetingtemplate.TEMPLATENAME = StrTemplateName;

                        meetingtemplate.T_OA_MEETINGTYPE = new T_OA_MEETINGTYPE();
                        meetingtemplate.T_OA_MEETINGTYPE=SelectMeetingType;
                        //meetingtemplate.T_OA_MEETINGTYPE.MEETINGTYPEID = StrMeetingType;
                        
                        meetingtemplate.CONTENT = txtTemplateContent.RichTextBoxContext;
                        meetingtemplate.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        meetingtemplate.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        meetingtemplate.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        meetingtemplate.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        meetingtemplate.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        meetingtemplate.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        meetingtemplate.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        meetingtemplate.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        meetingtemplate.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        meetingtemplate.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                        meetingtemplate.UPDATEUSERNAME = "";

                        meetingtemplate.UPDATEDATE = null;
                        meetingtemplate.UPDATEUSERID = "";
                        meetingtemplate.CREATEDATE = System.DateTime.Now;

                        try
                        {
                            
                            MeetingClient.MeetingTypeTemplateAddAsync(meetingtemplate);
                        }
                        catch (Exception ex)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
                        }
                    }
                    else
                    {

                        //meetingtemplate.CONTENT = StrContent;
                        
                        meetingtemplate.CONTENT = txtTemplateContent.RichTextBoxContext;
                        meetingtemplate.TEMPLATENAME = StrTemplateName;
                        
                        meetingtemplate.T_OA_MEETINGTYPE = SelectMeetingType;
                        meetingtemplate.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        meetingtemplate.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        meetingtemplate.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        meetingtemplate.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        meetingtemplate.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        meetingtemplate.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        meetingtemplate.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                        meetingtemplate.UPDATEDATE = System.DateTime.Now;
                        try
                        {
                            
                            MeetingClient.MeetingTypeTemplateUpdateAsync(meetingtemplate);

                        }
                        catch (Exception ex)
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
            }
        }

        void TemplateClient_MeetingTypeTemplateAddCompleted(object sender, MeetingTypeTemplateAddCompletedEventArgs e)
        {
            loadbar.Stop();
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "MEETINGTYPETEMPLATE"));
                    action = Action.Edit;
                    RefreshUI(saveType);
                }
                else
                {
                    if (e.Result == "ERROR")
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"));
                    }
                    else
                    {
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result, "MEETINGTYPETEMPLATE"));
                    }
                    //MessageBox.Show(e.Result.ToString());

                }
            }
            else
            {
                //MessageBox.Show("输入添加时遇到错误，请与管理员联系");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                
                return;
            }
            
        }

        void TemplateClient_MeetingTypeTemplateUpdateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                //MessageBox.Show("会议类型模板信息修改成功！");
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MEETINGTYPETEMPLATE"));
                RefreshUI(saveType);

            }
            else
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                //MessageBox.Show(e.ToString());
            }
            loadbar.Stop();
        }
        private bool CheckMeetingTemplate()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    return false;
                }
            }
            return true;
        }

        private void SaveAndClose()
        {
            //saveType = "1";
            Save();
        }
        #endregion

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
