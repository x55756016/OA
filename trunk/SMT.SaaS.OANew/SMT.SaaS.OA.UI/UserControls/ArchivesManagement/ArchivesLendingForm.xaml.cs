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

using SMT.SaaS.OA.UI.SmtOADocumentAdminService;
using SMT.SaaS.OA.UI.Class;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.Views.ArchivesManagement;

namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ArchivesLendingForm : BaseForm,IClient,IEntityEditor,IAudit
    {        
        private string lendingID; //借阅ID
        private SmtOADocumentAdminClient client;        
        //private V_ArchivesLending archiveLending;
        private T_OA_LENDARCHIVES lendingArchives;
        private DataActionFlag actionFlag = DataActionFlag.Normal;
        private CFrmArchivesSearch searchFrm;
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        private string checkstate = "";
        private Action action;
        private FormTypes FormTypeAction;
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();

        #region 初始化
        public ArchivesLendingForm(Action action, string lendingID, string checkstate)
        {
            InitializeComponent();
            this.action = action;
            this.lendingID = lendingID;
            this.checkstate = checkstate;
            this.Loaded += new RoutedEventHandler(Form_Loaded);
        }

        /// <summary>
        /// View调用的Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Form_Loaded(object sender, RoutedEventArgs e)
        {
            switch (action)
            {
                case Action.Add:
                    FormTypeAction = FormTypes.New;
                    break;
                case Action.Edit:
                    FormTypeAction = FormTypes.Edit;
                    break;
                case Action.AUDIT:
                    FormTypeAction = FormTypes.Audit;
                    break;
                case Action.Read:
                    FormTypeAction = FormTypes.Browse;
                    break;
                case Action.ReSubmit:
                    FormTypeAction = FormTypes.Resubmit;
                    break;
            }
            InitEvent();
            this.sDate.SelectedDate = DateTime.Now;
            this.eDate.SelectedDate = DateTime.Now.AddDays(7);
            if (action != Action.Add)
            {
                if (action == Action.AUDIT)
                {
                    actionFlag = DataActionFlag.SubmitComplete;
                }
                InitData();
            }
            else
            {
                lendingArchives = new T_OA_LENDARCHIVES();
                lendingArchives.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
            }
            if (checkstate != ((int)CheckStates.UnSubmit).ToString() && checkstate != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
            {
                if (action != Action.Add)
                {
                    SetReadOnly();
                }
            }
            else
            {
                if (action != Action.AUDIT || action != Action.Read)
                {
                    SetToolBar();
                }
            }
        }
        //引擎调用
        public ArchivesLendingForm(FormTypes actionformtype, string lendingID)
        {
            InitializeComponent();
            this.lendingID = lendingID;
            this.Loaded += new RoutedEventHandler(ArchivesLendingForm_Loaded);
        }

        /// <summary>
        /// 引擎调用的Loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ArchivesLendingForm_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();
            this.sDate.SelectedDate = DateTime.Now;
            this.eDate.SelectedDate = DateTime.Now.AddDays(7);
            this.checkstate = "1";
            this.action = Action.AUDIT;
            FormTypeAction = FormTypes.Audit;
            if (action == Action.AUDIT)
            {
                actionFlag = DataActionFlag.SubmitComplete;
            }
            InitData();


            if (checkstate != ((int)CheckStates.UnSubmit).ToString() && checkstate != ((int)CheckStates.UnApproved).ToString())   //只有未提交和未通过才能修改
            {
                SetReadOnly();
            }
            else
            {
                SetToolBar();
            }
        }

        private void SetReadOnly()
        {
            this.sDate.IsEnabled = false;
            this.eDate.IsEnabled = false;
        }

        private void SetToolBar()
        {
            ToolbarItems = CreateFormNewButton();
            if (checkstate == ((int)CheckStates.UnSubmit).ToString())
            {
                //scvAudit.Visibility = Visibility.Collapsed;
            }
        }

        private void InitEvent()
        {
            client = new SmtOADocumentAdminClient();
            client.AddArchivesLendingCompleted += new EventHandler<AddArchivesLendingCompletedEventArgs>(client_AddArchivesLendingCompleted);
            client.UpdateArchivesLendingCompleted += new EventHandler<UpdateArchivesLendingCompletedEventArgs>(client_UpdateArchivesLendingCompleted);
            client.GetLendingListByLendingIdCompleted += new EventHandler<GetLendingListByLendingIdCompletedEventArgs>(client_GetLendingListByLendingIdCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
        }

        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }        

        private void InitData()
        {
            if (!string.IsNullOrEmpty(lendingID))
            {
                client.GetLendingListByLendingIdAsync(lendingID);
            }
        }



        #endregion

        #region 完成事件      

        /// <summary>
        /// 获取借阅记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_GetLendingListByLendingIdCompleted(object sender, GetLendingListByLendingIdCompletedEventArgs e)
        {
            try
            {
                if (e.Result != null)
                {
                    lendingArchives = e.Result.ToList().First();
                    if (actionFlag == DataActionFlag.SubmitFlow)
                    {
                        actionFlag = DataActionFlag.SubmitComplete;
                        //SumbitFlow();
                        return;
                    }
                    if (action == Action.AUDIT)
                    {
                        //audit.XmlObject = DataObjectToXml<T_OA_LENDARCHIVES>.ObjListToXml(lendingArchives, "OA"); 
                        
                    }
                    this.txtTitle.Text = lendingArchives.T_OA_ARCHIVES.ARCHIVESTITLE;
                    this.txtCompany.Text = Utility.GetCompanyName(lendingArchives.T_OA_ARCHIVES.COMPANYID);
                    this.sDate.SelectedDate = lendingArchives.STARTDATE;
                    this.eDate.SelectedDate = lendingArchives.PLANENDDATE;
                    //BindAduitInfo();
                    if (FormTypeAction == FormTypes.Resubmit)
                    {
                        lendingArchives.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                    }
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }


        /// <summary>
        /// 更新完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UpdateArchivesLendingCompleted(object sender, UpdateArchivesLendingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {
                        
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {
                        if (e.UserState.ToString() == "Edit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                            if (GlobalFunction.IsSaveAndClose(refreshType))
                            {
                                RefreshUI(refreshType);
                            }
                            else
                            {
                                InitData();
                            }                            

                        }
                        else if (e.UserState.ToString() == "Audit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                        }
                        RefreshUI(RefreshedTypes.All);



                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    actionFlag = DataActionFlag.SubmitComplete;
                        //    SumbitFlow();
                        //}
                        //else
                        //{
                            
                        //    if (actionFlag == DataActionFlag.SubmitComplete)
                        //    {
                        //        if (action == Action.Add || action == Action.Edit)
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUSSESSEDSUBMIT", "ARCHIVELENDING"));
                        //        }
                        //        else
                        //        {
                        //            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "ARCHIVELENDING"));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "ARCHIVELENDING"));
                        //        if (GlobalFunction.IsSaveAndClose(refreshType))
                        //        {
                        //            RefreshUI(RefreshedTypes.CloseAndReloadData);
                        //        }
                        //        else
                        //        {
                                    
                        //            InitData();
                        //        }
                        //    }
                            
                        //    RefreshUI(refreshType);
                        //}
                    }
                }
                else
                {                    
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        /// <summary>
        /// 新增完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_AddArchivesLendingCompleted(object sender, AddArchivesLendingCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error == null)
                {
                    if (e.Result != "")
                    {   
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result));
                    }
                    else
                    {

                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                        if (GlobalFunction.IsSaveAndClose(refreshType))
                        {
                            RefreshUI(refreshType);
                        }
                        else
                        {
                            FormTypeAction = FormTypes.Edit;
                            action = Action.Edit;
                            this.lendingID = lendingArchives.LENDARCHIVESID;
                            InitData();
                            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                            entBrowser.FormType = FormTypes.Edit;
                            RefreshUI(RefreshedTypes.AuditInfo);
                            RefreshUI(RefreshedTypes.All);
                        }



                        //HtmlPage.Window.Alert("新增借阅记录成功！");
                        //if (actionFlag == DataActionFlag.SubmitFlow)
                        //{
                        //    client.GetLendingListByLendingIdAsync(lendingArchives.LENDARCHIVESID);
                        //    //actionFlag = DataActionFlag.SubmitComplete;
                        //    //SumbitFlow();
                        //}
                        //else
                        //{
                            
                            
                        //    if (GlobalFunction.IsSaveAndClose(refreshType))
                        //    {                                
                        //        RefreshUI(RefreshedTypes.CloseAndReloadData);
                        //    }
                        //    else
                        //    {
                                
                        //        //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "ARCHIVELENDING"));
                        //        this.action = Action.Edit;
                        //        this.lendingID = lendingArchives.LENDARCHIVESID;
                        //        InitData();
                        //    }                            
                        //}
                    }
                }
                else
                {
                    //HtmlPage.Window.Alert(e.ToString());
                    
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), e.Error.Message);
                }
            }
            catch (Exception ex)
            {                
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }


        /// <summary>
        /// 提交审核完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void audit_AuditCompleted(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            auditResult = e.Result;
            switch (auditResult)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    //todo 审核中
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel:
                    //todo 取消
                    Cancel();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    //todo 终审通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    //todo 审核不通过
                    SumbitCompleted();
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Error:
                    //todo 审核异常
                    HandError();
                    break;
            }
        }
        #endregion

        #region 自定义事件
        /// <summary>
        /// 选择档案
        /// </summary>
        private void ChooseArchives()
        {
            searchFrm = new CFrmArchivesSearch();
            searchFrm.Closed += new EventHandler(searchFrm_Closed);
            searchFrm.Show();
        }

        private void searchFrm_Closed(object sender, EventArgs e)
        {
            if (searchFrm.returnValue != null)
            {
                if (action == Action.Add)
                {

                    lendingArchives = new T_OA_LENDARCHIVES();
                }
                //archiveLending.archives.ARCHIVESID = searchFrm.returnValue.archivesID;
                //archiveLending.archives.COMPANYID = searchFrm.returnValue.companyID;
                lendingArchives.T_OA_ARCHIVES = searchFrm.returnValue;
                this.txtCompany.Text = Utility.GetCompanyName(searchFrm.returnValue.COMPANYID);
                this.txtTitle.Text = searchFrm.returnValue.ARCHIVESTITLE;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            try
            {                
                if (string.IsNullOrEmpty(this.txtTitle.Text.Trim()))
                {
                    //HtmlPage.Window.Alert("档案标题不能为空!");
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SELECTFIRST", "ARCHIVE"));
                    return;
                }
                if (string.IsNullOrEmpty(sDate.SelectedDate.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "LENDTIME"));
                    return;
                }
                if (string.IsNullOrEmpty(eDate.SelectedDate.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EXPECTEDRETURNTIME"));
                    return;
                }
                if (Convert.ToDateTime(sDate.SelectedDate.ToString()) > Convert.ToDateTime(eDate.SelectedDate.ToString()))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DATEGREATERERROR", "LENDTIME,EXPECTEDRETURNTIME"));
                    return;
                }
                RefreshUI(RefreshedTypes.ShowProgressBar);
                

                //lendingArchives.T_OA_ARCHIVES = archiveLending.archives;
                //lendingArchives.T_OA_ARCHIVES.ARCHIVESID = archiveLending.archives.ARCHIVESID;
                //lendingArchives.USERID = Common.CurrentLoginUserInfo.EmployeeID;  //是否要从组织架构里选
                lendingArchives.STARTDATE = Convert.ToDateTime(this.sDate.SelectedDate);
                lendingArchives.PLANENDDATE = Convert.ToDateTime(this.eDate.SelectedDate);
                if (action == Action.Add)
                {                    
                    lendingArchives.LENDARCHIVESID = System.Guid.NewGuid().ToString();
                    lendingArchives.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    lendingArchives.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    lendingArchives.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    lendingArchives.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    lendingArchives.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    lendingArchives.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                    lendingArchives.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    lendingArchives.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    lendingArchives.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    lendingArchives.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    lendingArchives.CREATEDATE = DateTime.Now;
                    lendingArchives.CHECKSTATE = Utility.GetCheckState(CheckStates.UnSubmit);
                    client.AddArchivesLendingAsync(lendingArchives,"Add");
                }
                else
                {
                    lendingArchives.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    lendingArchives.CHECKSTATE = "0";
                    lendingArchives.UPDATEDATE = DateTime.Now;

                    lendingArchives.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    client.UpdateArchivesLendingAsync(lendingArchives,"Edit");
                }
            }
            catch (Exception ex)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), ex.Message.ToString());
            }
        }

        /// <summary>
        /// 提交流程按钮
        /// </summary>
        private void SumbitAudit()
        {
            actionFlag = DataActionFlag.SubmitFlow;
            Save();   
        }

        /// <summary>
        /// 提交流程
        /// </summary>
        //private void SumbitFlow()
        //{
        //    if (lendingArchives != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "archivesLending";
        //        entity.FormID = lendingArchives.LENDARCHIVESID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        audit.XmlObject = DataObjectToXml<T_OA_LENDARCHIVES>.ObjListToXml(lendingArchives, "OA"); 
        //        audit.Submit();
        //    }
        //}

        /// <summary>
        /// 绑定审核控件数据
        /// </summary>
        //public void BindAduitInfo()
        //{
        //    SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //    entity.ModelCode = "archivesLending";
        //    entity.FormID = lendingArchives.LENDARCHIVESID;
        //    entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //    entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //    entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //    entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //    entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //    audit.BindingData();
        //}

        private void SumbitCompleted()
        {
            try
            {
                if (lendingArchives != null)
                {
                    lendingArchives.UPDATEDATE = DateTime.Now; 
                    lendingArchives.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    lendingArchives.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            lendingArchives.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            lendingArchives.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            lendingArchives.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    client.UpdateArchivesLendingAsync(lendingArchives);
                }
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), ex.ToString());
            }
        }

        private void Cancel()
        {
           //todo close
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        private void HandError()
        {
            Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"));
            //todo close and refresh
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }       
        #endregion

        #region IEntityEditor
        public string GetTitle()
        {
            //return Utility.GetResourceStr("COMPANY");
            string StrReturn = "";
            switch (action)
            { 
                case Action.Add:
                    StrReturn = Utility.GetResourceStr("ADDTITLE", "ARCHIVELENDING");
                    break;
                case Action.Edit:
                    StrReturn = Utility.GetResourceStr("EDITTITLE", "ARCHIVELENDING");
                    break;
                case Action.Read:
                    StrReturn = Utility.GetResourceStr("VIEWTITLE", "ARCHIVELENDING");
                    break;
                case Action.AUDIT:
                    StrReturn = Utility.GetResourceStr("AUDITTITLE", "ARCHIVELENDING");
                    break;
            }
            return StrReturn;
            
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
                    refreshType = RefreshedTypes.All;
                    Save();
                    break;
                case "1":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    Save();
                    break;
                case "2":
                    refreshType = RefreshedTypes.CloseAndReloadData;
                    SumbitAudit();
                    break;
                case "3":
                    ChooseArchives();
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
            if (FormTypeAction != FormTypes.Browse && FormTypeAction != FormTypes.Audit)
            {
                return CreateFormNewButton();
            }
            else
            {
                return ToolbarItems;
            }
            //if (action == Action.Add)
            //{
            //    return CreateFormNewButton();
            //}
            //else
            //{
            //    return ToolbarItems;
            //}
        }

        private List<ToolbarItem> CreateFormNewButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            //ToolbarItem item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "2",
            //    Title = Utility.GetResourceStr("SUBMITAUDIT"),
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            
            //items.Add(item);

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                //Title = Utility.GetResourceStr("SAVEANDCLOSE"),
                //ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_saveClose.png"
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

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "3",
                Title = Utility.GetResourceStr("CHOOSEARCHIVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_addView.png"
            };

            items.Add(item);


            return items;
        }

        private List<ToolbarItem> CreateFormEditButton()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            ToolbarItem item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "0",
                Title = Utility.GetResourceStr("SAVE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_save.png"
            };

            items.Add(item);

            item = new ToolbarItem
            {
                DisplayType = ToolbarItemDisplayTypes.Image,
                Key = "1",
                Title = Utility.GetResourceStr("CLOSE"),
                ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_Close.png"
            };

            items.Add(item);

            

            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "5",
            //    Title = "提交审核",
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "6",
            //    Title = "审核",//"审核"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/18_audit.png"
            //};

            //items.Add(item);
            //item = new ToolbarItem
            //{
            //    DisplayType = ToolbarItemDisplayTypes.Image,
            //    Key = "7",
            //    Title = "审核不通过",//"审核不通过"
            //    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png"
            //};

            //items.Add(item);
            
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

        #region IAudit 成员

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            string strXmlObjectSource = string.Empty;
            strXmlObjectSource = Utility.ObjListToXml<T_OA_LENDARCHIVES>(lendingArchives, "OA");

            Utility.SetAuditEntity(entity, "archivesLending", lendingArchives.LENDARCHIVESID, strXmlObjectSource);
        }

        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            string state = "";
            string UserState = "Audit";
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    break;
            }
            if (lendingArchives.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            lendingArchives.CHECKSTATE = state;
            client.UpdateArchivesLendingAsync(lendingArchives,UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (lendingArchives != null)
                state = lendingArchives.CHECKSTATE;
            if (FormTypeAction == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }

        #endregion




        #region IForm 成员

        public void ClosedWCFClient()
        {
            client.DoClose();
        }

        public bool CheckDataContenxChange()
        {
            return true;
            //throw new NotImplementedException();
        }

        public void SetOldEntity(object entity)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }

        
}
