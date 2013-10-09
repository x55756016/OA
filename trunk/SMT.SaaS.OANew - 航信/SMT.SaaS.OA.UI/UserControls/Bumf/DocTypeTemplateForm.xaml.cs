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
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
using System.Windows.Browser;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;

using SMT.SaaS.PublicControls;
using Telerik.Windows.Documents.FormatProviders.Pdf;

using SMT.Saas.Tools.PublicInterfaceWS;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class DocTypeTemplateForm : BaseForm,IClient,IEntityEditor
    {

        #region 初始化变量
        private T_OA_SENDDOCTEMPLATE tmpDocTypeTemplateT = new T_OA_SENDDOCTEMPLATE();
        
        private string tmpStrcbxDocType = "";//类型
        private string tmpStrcbxGrade = "";//级别
        private string tmpStrcbxProritity = ""; //缓急
        private string tmpStrDocTypeName = ""; //模板类型
        private string tempStrDocTemplateName = "";//模板名
        private SmtOACommonOfficeClient DocTypeTemplateClient = new SmtOACommonOfficeClient();
        private Action action;
        //private string saveType = "1";
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;       //保存方式 0:保存 1:保存并关闭 
        PublicServiceClient publicClient = new PublicServiceClient();
        
        private void InitComboxSource()
        {
            Combox_ItemSourceDocType();            
        }
        #endregion


        #region 填充类型信息
        private void Combox_ItemSourceDocType()
        {
            DocTypeTemplateClient.GetDocTypeNameInfosToComboxAsync();

        }

        void DocTypeTemplateClient_GetDocTypeNameInfosToComboxCompleted(object sender, GetDocTypeNameInfosToComboxCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                this.cbxDocType.Items.Clear();

                this.cbxDocType.ItemsSource = e.Result;
                //this.cbxDocType.DisplayMemberPath = "SENDDOCTYPE";
                if (tmpStrcbxDocType == "")
                {
                    this.cbxDocType.SelectedIndex = 0;//默认选中第一项
                }
                else
                {
                    foreach (string Region in cbxDocType.Items)
                    {
                        if (Region == tmpStrcbxDocType)
                        {
                            this.cbxDocType.SelectedItem = tmpStrcbxDocType;
                            break;
                        }
                    }
                    
                }

            }
        }
        #endregion

        #region 构造函数

        public DocTypeTemplateForm(Action action, T_OA_SENDDOCTEMPLATE TypeObj)
        {
            InitializeComponent();
            if(TypeObj != null)
                tmpDocTypeTemplateT = TypeObj;
            this.action = action;
            this.Loaded += new RoutedEventHandler(DocTypeTemplateForm_Loaded);
            
        }

        void DocTypeTemplateForm_Loaded(object sender, RoutedEventArgs e)
        {
            DocTypeTemplateClient.DocTypeTemplateAddCompleted += new EventHandler<DocTypeTemplateAddCompletedEventArgs>(DocTypeTemplateClient_DocTypeTemplateAddCompleted);
            DocTypeTemplateClient.DocTypeTemplateInfoUpdateCompleted += new EventHandler<DocTypeTemplateInfoUpdateCompletedEventArgs>(DocTypeTemplateClient_DocTypeTemplateInfoUpdateCompleted);
            DocTypeTemplateClient.GetDocTypeNameInfosToComboxCompleted += new EventHandler<GetDocTypeNameInfosToComboxCompletedEventArgs>(DocTypeTemplateClient_GetDocTypeNameInfosToComboxCompleted);
            publicClient.AddContentCompleted += new EventHandler<AddContentCompletedEventArgs>(publicClient_AddContentCompleted);
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);

            if (action != Action.Add)
            {
                if (action == Action.Read)
                {
                    this.txtTemplateName.IsEnabled = false;
                    this.txtTemplateTitle.IsEnabled = false;
                    this.cbxDocType.IsEnabled = false;
                    this.cbxGrade.IsEnabled = false;
                    this.cbxProritity.IsEnabled = false;
                    //this.txtContent.HideControls();//屏蔽富文本框的头部
                    this.txtContent.IsReadOnly = true;
                               
                    
                    txtContent.BorderThickness = new Thickness(1.0);
                    txtContent.BorderBrush = new SolidColorBrush(Colors.Gray);
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                GetDocTypeTemplateDetailInfo(tmpDocTypeTemplateT);
            }
            //else
            //{
            //    this.cbxGrade.SelectedIndex = 0;
            //    this.cbxProritity.SelectedIndex = 0;
            //}

            InitComboxSource();
        }

        void publicClient_GetContentCompleted(object sender, GetContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtContent.Document = e.Result;
            }
        }
        
        void publicClient_AddContentCompleted(object sender, AddContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result)
                {
                    string aa = "";
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), e.Error.Message,
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }

        #endregion

        private void GetDocTypeTemplateDetailInfo(T_OA_SENDDOCTEMPLATE TypeTemplateObj)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (TypeTemplateObj != null)
            {
                tmpDocTypeTemplateT = TypeTemplateObj;
                this.txtTemplateTitle.Text = TypeTemplateObj.SENDDOCTITLE;
                this.txtTemplateName.Text = TypeTemplateObj.TEMPLATENAME;
                                
                //txtContent.RichTextBoxContext = TypeTemplateObj.CONTENT;
                publicClient.GetContentAsync(TypeTemplateObj.SENDDOCTEMPLATEID);
                tmpStrcbxDocType = TypeTemplateObj.SENDDOCTYPE;
                tmpStrcbxGrade = TypeTemplateObj.GRADED;
                tmpStrcbxProritity = TypeTemplateObj.PRIORITIES;
                this.cbxDocType.SelectedItem = TypeTemplateObj.SENDDOCTYPE;
                //this.cbxGrade.SelectedItem = TypeTemplateObj.GRADED;
                //this.cbxProritity.SelectedItem = TypeTemplateObj.PRIORITIES;

                if (!string.IsNullOrEmpty(TypeTemplateObj.GRADED.ToString()))
                {
                    foreach (T_SYS_DICTIONARY Region in cbxGrade.Items)
                    {
                        if (Region.DICTIONARYNAME == tmpStrcbxGrade)
                        {
                            cbxGrade.SelectedItem = Region;
                            break;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(TypeTemplateObj.PRIORITIES.ToString()))
                {
                    foreach (T_SYS_DICTIONARY Region in cbxProritity.Items)
                    {
                        if (Region.DICTIONARYNAME == tmpStrcbxProritity)
                        {
                            cbxProritity.SelectedItem = Region;
                            break;
                        }
                    }
                }


            }
        }

        void DocTypeTemplateClient_DocTypeTemplateInfoUpdateCompleted(object sender, DocTypeTemplateInfoUpdateCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.StrResult != "")
                    {
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.StrResult, "DOCTYPETEMPLATE"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.StrResult, "DOCTYPETEMPLATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                    else
                    {
                        UserInfo User = new UserInfo();
                        User.COMPANYID = tmpDocTypeTemplateT.OWNERCOMPANYID;
                        User.DEPARTMENTID = tmpDocTypeTemplateT.OWNERDEPARTMENTID;
                        User.POSTID = tmpDocTypeTemplateT.OWNERPOSTID;
                        User.USERID = tmpDocTypeTemplateT.OWNERID;
                        User.USERNAME = tmpDocTypeTemplateT.OWNERNAME;
                        //publicClient.UpdateContentAsync(tmpDocTypeTemplateT.SENDDOCTEMPLATEID, tmpDocTypeTemplateT.CONTENT, tmpDocTypeTemplateT.OWNERCOMPANYID, "OA", "T_OA_SENDDOCTEMPLATE", User);
                        publicClient.UpdateContentAsync(tmpDocTypeTemplateT.SENDDOCTEMPLATEID,tmpDocTypeTemplateT.CONTENT,User);
                        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "DOCTYPETEMPLATE"));
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "DOCTYPETEMPLATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);

                        RefreshUI(saveType);
                    }
                }
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), e.Error.Message);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
        }

        void DocTypeTemplateClient_DocTypeTemplateAddCompleted(object sender, DocTypeTemplateAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ProgressBar);
            if (!e.Cancelled)
            {
                if (e.Result == "")
                {
                    action = Action.Edit;
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "DOCTYPETEMPLATE"));
                    UserInfo User = new UserInfo();
                    User.COMPANYID = tmpDocTypeTemplateT.OWNERCOMPANYID;
                    User.DEPARTMENTID = tmpDocTypeTemplateT.OWNERDEPARTMENTID;
                    User.POSTID = tmpDocTypeTemplateT.OWNERPOSTID;
                    User.USERID = tmpDocTypeTemplateT.OWNERID;
                    User.USERNAME = tmpDocTypeTemplateT.OWNERNAME;
                    publicClient.AddContentAsync(tmpDocTypeTemplateT.SENDDOCTEMPLATEID, tmpDocTypeTemplateT.CONTENT, tmpDocTypeTemplateT.OWNERCOMPANYID, "OA", "T_OA_SENDDOCTEMPLATE", User);
                    RefreshUI(saveType); 
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "DOCTYPETEMPLATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    
                }
                else
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "DOCTYPETEMPLATE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "DOCTYPETEMPLATE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
            }
        }

        #region IEntityEditor
        public string GetTitle()
        {
            if (action == Action.Add)
            {
                return Utility.GetResourceStr("ADDTITLE", "DOCTYPETEMPLATE");
            }
            else if (action == Action.Edit)
            {
                return Utility.GetResourceStr("EDITTITLE", "DOCTYPETEMPLATE");
            }
            else
            {
                return Utility.GetResourceStr("VIEWTITLE", "DOCTYPETEMPLATE");
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

        private bool CheckDocTypeTemplate()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 保存
        private void Save()
        {
            try
            {
                string StrTitle = ""; //标题
                string StrTmeplateName = ""; //模板名
                string StrGrade = "";//级别
                string Strtype = "";//类型
                string StrProritity = "";//缓急
                //string StrContent = "";//内容
                string StrError = ""; //记录错误信息
                

                StrTitle = this.txtTemplateTitle.Text.Trim().ToString();
                StrTmeplateName = this.txtTemplateName.Text.Trim().ToString();
                if (this.cbxGrade.SelectedIndex == 0 || this.cbxGrade.SelectedIndex == -1)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SENDDOCGRADENOTNULL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCGRADENOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                if (this.cbxProritity.SelectedIndex == 0 || this.cbxProritity.SelectedIndex == -1)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "PRIORITY"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "PRIORITY"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                T_SYS_DICTIONARY GradeObj = cbxGrade.SelectedItem as T_SYS_DICTIONARY;//级别
                T_SYS_DICTIONARY ProritityObj = cbxProritity.SelectedItem as T_SYS_DICTIONARY;//缓急
                StrGrade = GradeObj.DICTIONARYNAME.ToString();
                StrProritity = ProritityObj.DICTIONARYNAME.ToString();
                Strtype = this.cbxDocType.SelectedItem.ToString();
                //StrContent = this.txtContent.Text.Trim().ToString();
                if (string.IsNullOrEmpty(StrTitle))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "SENDDOCTITLE"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "SENDDOCTITLE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(StrTmeplateName))
                {
                    
                    //StrError += "模板名称不能为空\n";
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "OATEMPLATENAME"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "OATEMPLATENAME"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                //else
                //{
                //    StrContent = HttpUtility.HtmlEncode(StrContent);
                //}
                if (txtContent.Document.Count() ==0)
                {

                    //StrError += "模板内容不能为空\n";
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "CONTENT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "CONTENT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.txtContent.Focus();
                    return;
                }
                else
                {
                    //if (StrContent.Length < 10)
                    //{
                    //    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("CONTENTNOTLESSTHANTEN"));
                    //    this.txtContent.Focus();
                    //    return;
                    //}
                }
                //if (this.cbxGrade.SelectedIndex ==0)
                //{
                //    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SENDDOCGRADENOTNULL"));
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCGRADENOTNULL"),
                //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //    return;
                //}
                //if (this.cbxProritity.SelectedIndex ==0)
                //{
                //    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("WARNING"), Utility.GetResourceStr("REQUIRED", "PRIORITY"));
                //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "PRIORITY"),
                //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                //    return;
                //}
                if (string.IsNullOrEmpty(Strtype))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("SENDDOCPRIORITYNOTNULL"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCPRIORITYNOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                if (CheckDocTypeTemplate())
                {
                    RefreshUI(RefreshedTypes.ProgressBar);
                    if (action == Action.Add)
                    {
                        tmpDocTypeTemplateT.SENDDOCTEMPLATEID = System.Guid.NewGuid().ToString();
                        tmpDocTypeTemplateT.SENDDOCTITLE = StrTitle;
                        tmpDocTypeTemplateT.TEMPLATENAME = StrTmeplateName;
                        tmpDocTypeTemplateT.SENDDOCTYPE = Strtype;
                        tmpDocTypeTemplateT.PRIORITIES = StrProritity;
                        tmpDocTypeTemplateT.GRADED = StrGrade;                        
                        tmpDocTypeTemplateT.CONTENT = txtContent.Document;
                        tmpDocTypeTemplateT.CREATEDATE = System.DateTime.Now;

                        tmpDocTypeTemplateT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpDocTypeTemplateT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpDocTypeTemplateT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                        tmpDocTypeTemplateT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpDocTypeTemplateT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpDocTypeTemplateT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                        tmpDocTypeTemplateT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                        tmpDocTypeTemplateT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                        tmpDocTypeTemplateT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                        tmpDocTypeTemplateT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;

                        tmpDocTypeTemplateT.UPDATEUSERNAME = "";
                        tmpDocTypeTemplateT.UPDATEDATE = null;
                        tmpDocTypeTemplateT.UPDATEUSERID = "";
                        
                        DocTypeTemplateClient.DocTypeTemplateAddAsync(tmpDocTypeTemplateT);
                    }
                    else
                    {
                        if (tmpDocTypeTemplateT != null)
                        {
                            tmpDocTypeTemplateT.SENDDOCTITLE = StrTitle;
                            tmpDocTypeTemplateT.TEMPLATENAME = StrTmeplateName;
                            tmpDocTypeTemplateT.SENDDOCTYPE = Strtype;
                            tmpDocTypeTemplateT.PRIORITIES = StrProritity;
                            tmpDocTypeTemplateT.GRADED = StrGrade;
                                                        
                            tmpDocTypeTemplateT.CONTENT = txtContent.Document;
                            tmpDocTypeTemplateT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                            tmpDocTypeTemplateT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                            tmpDocTypeTemplateT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                            tmpDocTypeTemplateT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeTemplateT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;


                            tmpDocTypeTemplateT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                            tmpDocTypeTemplateT.UPDATEDATE = System.DateTime.Now;
                            tmpDocTypeTemplateT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                            tmpStrDocTypeName = Strtype;
                            tempStrDocTemplateName = StrTmeplateName;
                            string StrReturn = "";
                            DocTypeTemplateClient.DocTypeTemplateInfoUpdateAsync(tmpDocTypeTemplateT,StrReturn);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        private void SaveAndClose()
        {
            Save();         
        }

        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            DocTypeTemplateClient.DoClose();
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
