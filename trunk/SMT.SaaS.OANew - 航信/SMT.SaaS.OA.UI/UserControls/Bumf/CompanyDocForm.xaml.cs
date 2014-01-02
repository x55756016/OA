using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SMT.SaaS.OA.UI.SmtOACommonOfficeService;
//using SMT.Saas.Tools.OrganizationWS;
//using SMT.SaaS.FrameworkUI.FileUpload;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.OA.UI.Class;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SaaS.MobileXml;
using SMT.SaaS.PublicControls;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PublicInterfaceWS;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;


namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class CompanyDocForm : BaseForm, IClient, IEntityEditor, IAudit//, IFileLoadedCompleted
    {


        #region 初始化变量

        private T_OA_SENDDOC tmpSendDocT = new T_OA_SENDDOC();
        private string StrDepartmentID = ""; //部门ID
        private string StrAddUserID = "";//申请人ID
        //private string StrMainUserID = "1";//主送人
        //private string StrCopyUserID = "1";//抄送人ID
        private string tmpStrcbxDocType = "";//类型
        private T_OA_SENDDOCTYPE SelectDocType = new T_OA_SENDDOCTYPE();
        private string tmpStrcbxGrade = "";//级别
        private string tmpStrcbxProritity = ""; //缓急
        private string tmpBtnName = "";  //按钮名
        List<T_OA_SENDDOCTEMPLATE> tmpinfos = new List<T_OA_SENDDOCTEMPLATE>(); //模板名集合
        private bool AuditType = false; //是否提交了审核按钮
        private bool IsSave = false; //是否是只保存
        private SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult auditResult;   //审批结果
        public string postLevel = string.Empty;
        private string tempStrSendDocName = "";//发文标题
        private FormTypes action;  //动作
        public bool needsubmit = false;//提交审核
        public bool isSubmit = false;//提示保存方法是否是提交事件
        private bool canShowDistButton = false; //经过权限过滤后才决定能否显示发布按钮
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private RefreshedTypes saveType = RefreshedTypes.CloseAndReloadData;

        private SmtOACommonOfficeClient SendDocClient = new SmtOACommonOfficeClient();
        PublicServiceClient publicClient = new PublicServiceClient();
        private PersonnelServiceClient personClient = new PersonnelServiceClient();
        public delegate void refreshGridView();

        public event refreshGridView ReloadDataEvent;
        private string StrUpdateReturn = "";//修改数据时返回的字符串

        bool SelectChange = false;//下拉框动作
        bool IsSaveFlag = false;
        bool IsButtonSave = false;
        bool IsAudit = false; //是否审核状态，用于判断是否显示发布按钮
        string OwnerCompanyid = "";//所属公司
        string OwnerDepartmentid = "";//获取事项审批时用 用的部门ID
        string Ownerid = "";//所属员工
        string OwnerPostid = "";//所属岗位
        string tmpGrade = "";//公文级别中文名称
        string tmpProri = "";//公文缓急中文名称
        string tmpIsRed = "";//是否红头文件
        string PostName = "";//岗位名称
        string DepartmentName = "";//部门名称
        string CompanyName = "";//公司名称
        string StrOwnerName = "";//申请人
        string StrSendDepartment = "";//发文部门
        string CreateDepartmentName = "";
        string CreateCompanyName = "";
        string CreatePostName = "";


        private List<V_CompanyDocNum> ListNums = new List<V_CompanyDocNum>();//公文文号集合
        DictionaryManager DictManager = new DictionaryManager();
        List<string> ListDict = new List<string>(); //字典列表
        private V_BumfCompanySendDoc tmpViewCompanyDoc = new V_BumfCompanySendDoc();
        string tmpSendID = "";//公文ID供代办任务调用

        bool IsManageUse = false;//是否从管理页面调用构造函数

        private bool canSubmit = false;//能否提交审核

        private void InitComboxSource()
        {
            //RefreshUI(RefreshedTypes.ProgressBar);
            Combox_ItemSourceDocType();
        }
        #endregion

        #region 构造函数
        public CompanyDocForm(FormTypes operationType, V_BumfCompanySendDoc TypeObj)
        {
            InitializeComponent();
            InitEvent();
            ListDict.Add("COMPANYDOCGRADE");//公文级别
            ListDict.Add("COMPANYDOCPRIORITY");//公文缓急程度

            action = operationType;

            // by ldx
            // 2011-08-09
            // 这个是用来判断下拉框动作的，修改等文档类型、模版名称不能编辑
            if (action == FormTypes.New || action == FormTypes.Edit || action == FormTypes.Resubmit)
                SelectChange = true;
            else
            {
                cbxDocType.IsEnabled = false;
                cbxTemplateName.IsEnabled = false;
            }

            IsManageUse = true;
            tmpViewCompanyDoc = TypeObj;
            this.Loaded += new RoutedEventHandler(CompanyDocForm_Loaded);
            //从管理页面调用
            //InitMethodForManage(operationType, TypeObj);

        }


        public CompanyDocForm()
        {
            InitializeComponent();
            InitEvent();
            ListDict.Add("COMPANYDOCGRADE");//公文级别
            ListDict.Add("COMPANYDOCPRIORITY");//公文缓急程度

            action = FormTypes.New;

            SelectChange = true;

            V_BumfCompanySendDoc SendDocInfoT = new V_BumfCompanySendDoc();
            IsManageUse = true;
            tmpViewCompanyDoc = SendDocInfoT;
            this.Loaded += new RoutedEventHandler(CompanyDocForm_Loaded);
            

        }


        private void InitMethodForManage(FormTypes operationType, V_BumfCompanySendDoc TypeObj)
        {
            //if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
            //{
            //    ctrFile.FileState = SMT.SaaS.FrameworkUI.FileUpload.Constants.FileStates.FileBrowse;
            //}
            //ctrFile.SystemName = "OA";
            //ctrFile.ModelName = "CompanyDoc";
            //ctrFile.InitBtn(Visibility.Visible, Visibility.Collapsed);
            //ctrFile.EntityEditor = this;

            //ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(Event_AllFilesFinished);
            if (operationType == FormTypes.New)
            {
                tmpSendDocT.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                //SVAudit.Visibility = Visibility.Collapsed;
                if (!string.IsNullOrEmpty(Common.CurrentLoginUserInfo.Telphone))
                {
                    this.txtTel.Text = Common.CurrentLoginUserInfo.Telphone;
                }
                tmpSendDocT.SENDDOCID = System.Guid.NewGuid().ToString();
                Utility.InitFileLoad("CompanyDoc", tmpSendDocT.SENDDOCID, operationType, uploadFile);
                InitComboxSource();
                //this.cbxGrade.SelectedIndex = 0;
                //this.cbxProritity.SelectedIndex = 0;

                //获取电话号码（固话优先，手机次之）
                personClient.GetEmployeeDetailByIDCompleted += (o, e) =>
                {
                    if (e.Result != null && e.Result.T_HR_EMPLOYEE != null)
                    {
                        if (e.Result.T_HR_EMPLOYEE.MOBILE!=null)
                        {
                            this.txtTel.Text = e.Result.T_HR_EMPLOYEE.MOBILE;
                        }
                        if (e.Result.T_HR_EMPLOYEE.OFFICEPHONE != null)
                        {
                            this.txtTel.Text = e.Result.T_HR_EMPLOYEE.OFFICEPHONE;
                        }
                    }
                };
                personClient.GetEmployeeDetailByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
                //personClient.GetEmployeePostBriefByEmployeeIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
                InitUserInfo();
                StrDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                StrSendDepartment = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                PostsObject.Text = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                tmpSendDocT.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                tmpSendDocT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                tmpSendDocT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                tmpSendDocT.OWNERDEPARTMENTID = StrDepartmentID;
                StrSendDepartment = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
                tmpSendDocT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                string filter = "";
                tmpIsRed = "否";
                SendDocClient.GetCompanyDocNumsByUseridAsync(Common.CurrentLoginUserInfo.EmployeeID, "CREATEDATE", filter);
            }
            else
            {
                publicClient.GetContentAsync(TypeObj.OACompanySendDoc.SENDDOCID);
                SendDocClient.GetSendDocSingleInfoByIdAsync(TypeObj.OACompanySendDoc.SENDDOCID);
                //tmpSendDocT = TypeObj.OACompanySendDoc;
              
                SelectChange = false;
                Utility.InitFileLoad("CompanyDoc", TypeObj.OACompanySendDoc.SENDDOCID, operationType, uploadFile);
                if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)
                {
                    SelectDocType = TypeObj.doctype;
                    tmpSendDocT.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                    //GetSendDocInfo(TypeObj);
                    //InitComboxSource();
                    //RefreshUI(RefreshedTypes.AuditInfo);
                    //RefreshUI(RefreshedTypes.All);

                    //InitAudit(TypeObj.OACompanySendDoc);
                }
                if (operationType == FormTypes.Audit)
                {
                    SelectDocType = TypeObj.doctype;

                    SetReadOnly();
                   
                }

            }
        }

        private void InitUserInfo()
        {

            OwnerCompanyid = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            OwnerDepartmentid = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            OwnerPostid = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            Ownerid = Common.CurrentLoginUserInfo.UserPosts[0].EmployeePostID;
            PostName = Common.CurrentLoginUserInfo.UserPosts[0].PostName;//岗位名称
            DepartmentName = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;//部门名称
            CompanyName = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;//公司名称
            StrOwnerName = Common.CurrentLoginUserInfo.EmployeeName;//申请人
            string StrName = "";
            StrName = StrOwnerName + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;
            txtAddUser.Text = StrName;

            ToolTipService.SetToolTip(txtAddUser, StrName);
        }
        //private void Event_AllFilesFinished(object sender, FileCountEventArgs e)
        //{

        //}
        void CompanyDocForm_Loaded(object sender, RoutedEventArgs e)
        {
            //by luojie 替换提交事件处理
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            DictManager.LoadDictionary(ListDict);
            ShowDistributButton();
            //RefreshUI(RefreshedTypes.ShowProgressBar);
            //RefreshUI(RefreshedTypes.AuditInfo);
            //RefreshUI(RefreshedTypes.All);
        }

        //引擎调用2010-6-24
        public CompanyDocForm(FormTypes operationType, string SendDocID)
        {
            InitializeComponent();

            // by ldx
            // 2011-08-09
            // 这个是用来判断下拉框动作的，修改等文档类型、模版名称不能编辑
            action = operationType;
            if (action == FormTypes.New)
                SelectChange = true;
            else
            {
                cbxDocType.IsEnabled = false;
                cbxTemplateName.IsEnabled = false;
                if (action == FormTypes.Audit) IsAudit = true;
            }

            InitEvent();
            tmpSendID = SendDocID;
            ListDict.Add("COMPANYDOCGRADE");//公文级别
            ListDict.Add("COMPANYDOCPRIORITY");//公文缓急程度
            this.Loaded += new RoutedEventHandler(CompanyDocForm_Loaded);
            publicClient.GetContentAsync(SendDocID);
            //InitMethodToEngine(operationType, SendDocID);
        }
        /// <summary>
        /// 从代办任务调用
        /// </summary>
        /// <param name="operationType"></param>
        /// <param name="SendDocID"></param>
        private void InitMethodToEngine(FormTypes operationType, string SendDocID)
        {
            action = operationType;
            SendDocClient.GetSendDocSingleInfoByIdAsync(SendDocID);
            //this.tabitemaudit.Visibility = Visibility.Visible;
            //audit.XmlObject = DataObjectToXml<T_OA_SENDDOC>.ObjListToXml(tmpSendDocT, "OA"); 
            Utility.InitFileLoad("CompanyDoc", SendDocID, operationType, uploadFile);
            SetReadOnly();

            
        }


        private void InitEvent()
        {
            SendDocClient.SendDocAddCompleted += new EventHandler<SendDocAddCompletedEventArgs>(SendDocClient_SendDocAddCompleted);
            //audit.AuditCompleted += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_AuditCompleted);            
            SendDocClient.SendDocInfoUpdateCompleted += new EventHandler<SendDocInfoUpdateCompletedEventArgs>(SendDocClient_SendDocInfoUpdateCompleted);
            SendDocClient.GetDocTypeTemplateContentByDocTypeCompleted += new EventHandler<GetDocTypeTemplateContentByDocTypeCompletedEventArgs>(SendDocClient_GetDocTypeTemplateContentByDocTypeCompleted);
            SendDocClient.GetDocTypeInfosCompleted += new EventHandler<GetDocTypeInfosCompletedEventArgs>(SendDocClient_GetDocTypeInfosCompleted);
            SendDocClient.GetSendDocSingleInfoByIdCompleted += new EventHandler<GetSendDocSingleInfoByIdCompletedEventArgs>(SendDocClient_GetSendDocSingleInfoByIdCompleted);
            //personClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(personClient_GetEmployeeByIDCompleted);
            SendDocClient.GetCompanyDocNumsByUseridCompleted += new EventHandler<GetCompanyDocNumsByUseridCompletedEventArgs>(SendDocClient_GetCompanyDocNumsByUseridCompleted);
            //audit.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(audit_Auditing);
            //personClient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personclient_GetEmployeeDetailByIDCompleted);
            personClient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(personClient_GetEmployeePostBriefByEmployeeIDCompleted);
            DictManager.OnDictionaryLoadCompleted += new EventHandler<OnDictionaryLoadArgs>(DictManager_OnDictionaryLoadCompleted);
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);
   
        }

        #region 获取富文本框信息



        void publicClient_GetContentCompleted(object sender, GetContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtContent.Document = e.Result;
            }
        }
        #endregion
        void DictManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            if (e.Error == null && e.Result)
            {
                if (IsManageUse)
                {
                    InitMethodForManage(action, tmpViewCompanyDoc);
                }
                else
                {
                    InitMethodToEngine(action, tmpSendID);
                }
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "字典加载错误，请联系管理员",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        void personClient_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
                    employeepost = e.Result;
                    if (Application.Current.Resources["SYS_PostInfo"] != null)
                    {
                        if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>) != null)
                        {
                            if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == tmpSendDocT.OWNERPOSTID) != null)
                            {
                                if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault() != null)
                                {
                                    if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY != null)
                                    {
                                        PostName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                                    }
                                }
                            }
                        }
                    }

                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), StrPostName,Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    if (Application.Current.Resources["SYS_DepartmentInfo"] != null)
                    {
                        if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>) != null)
                        {
                            if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID) != null)
                            {
                                if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID).FirstOrDefault() != null)
                                {
                                    if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY != null)
                                    {
                                        DepartmentName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                                    }
                                }
                            }
                        }
                    }

                    if (Application.Current.Resources["SYS_CompanyInfo"] != null)
                    {
                        if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>) != null)
                        {
                            if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == tmpSendDocT.OWNERCOMPANYID) != null)
                            {
                                if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == tmpSendDocT.OWNERCOMPANYID).FirstOrDefault() != null)
                                {
                                    CompanyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == tmpSendDocT.OWNERCOMPANYID).FirstOrDefault().CNAME;
                                }
                            }
                        }
                    }
                    //PostName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
                    //DepartmentName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //CompanyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == tmpSendDocT.OWNERCOMPANYID).FirstOrDefault().CNAME;
                    StrOwnerName = e.Result.EMPLOYEENAME;
                    if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == tmpSendDocT.OWNERPOSTID) != null)
                    {
                        if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault() != null)
                        {
                            if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault().POSTLEVEL != null)
                            {
                                postLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == tmpSendDocT.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取出差人的岗位级别
                            }
                        }
                    }

                    string StrName = "";
                    //CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.EMPLOYEENAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;

                    txtAddUser.Text = StrName;
                    ToolTipService.SetToolTip(txtAddUser, StrName);
                    StrAddUserID = e.Result.EMPLOYEEID;
                    //RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }
        #endregion

        #region 获取员工信息
        void personclient_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {

                    string PostName = "";
                    string DepartmentName = "";
                    string CompanyName = "";
                    string StrName = "";
                    var posts = from ent in e.Result.EMPLOYEEPOSTS
                                where ent.T_HR_POST.POSTID == tmpSendDocT.OWNERPOSTID
                                select ent;
                    if (posts != null)
                    {
                        if (posts.Count() > 0)
                        {
                            PostName = posts.FirstOrDefault().T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                            postLevel = posts.FirstOrDefault().POSTLEVEL.ToString();
                        }
                    }
                    //PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    var departs = from ent in e.Result.EMPLOYEEPOSTS
                                  where ent.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID == tmpSendDocT.OWNERDEPARTMENTID
                                  select ent;
                    if (departs != null)
                    {
                        if (departs.Count() > 0)
                            //DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                            DepartmentName = departs.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    }
                    var companys = from ent in e.Result.EMPLOYEEPOSTS
                                   where ent.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == tmpSendDocT.OWNERCOMPANYID
                                   select ent;
                    if (companys != null)
                    {
                        if (companys.Count() > 0)
                            CompanyName = companys.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    }
                    //CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName;
                    //txtOwnerName.Text = StrName;

                    //PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    //DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    //StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName ;
                    txtAddUser.Text = StrName;
                    ToolTipService.SetToolTip(txtAddUser, StrName);
                    StrAddUserID = e.Result.T_HR_EMPLOYEE.EMPLOYEEID;
                    //RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }
        #endregion

        #region 获取公文编号


        void SendDocClient_GetCompanyDocNumsByUseridCompleted(object sender, GetCompanyDocNumsByUseridCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    ListNums = e.Result.ToList();
                    GetCurrentCompanyDocNum(ListNums, Common.CurrentLoginUserInfo.UserPosts[0].CompanyID, Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID);
                }
            }
        }
        /// <summary>
        /// 获取当前的公文编号
        /// </summary>
        /// <param name="list"></param>
        private void GetCurrentCompanyDocNum(List<V_CompanyDocNum> list, string StrDempartmentid, string StrCompanyid)
        {
            if (list.Count > 0)
            {
                var ents = from ent in list
                           where ent.OWNERCOMPANYID == StrDempartmentid && ent.OWNERDEPARTMENTID == StrCompanyid
                           select ent;
                if (ents.Count() > 0)
                {
                    this.txtNUM.Text = GetMaxCompanyDocNum(ents.ToList());
                }
            }
        }
        private string GetMaxCompanyDocNum(List<V_CompanyDocNum> listnums)
        {
            string StrResult = "";
            string StrYear = "";
            List<int> list = new List<int>();
            listnums.ForEach(item =>
            {
                if (!string.IsNullOrEmpty(item.NUM))
                    list.Add(GetCompanyNums(item.NUM));
            });
            if (list.Count() > 0)
            {
                StrResult = list.Max().ToString();
            }
            if (StrResult.Length > 4)
            {
                StrYear = StrResult.Substring(0, 4);//获取年份
                StrResult = StrResult.Substring(4, StrResult.Length - 4);//取后面的数字

                var ents = from ent in listnums
                           where ent.NUM != null && ent.NUM.Contains(StrResult)
                           select ent;
                if (ents.Count() > 0)
                {
                    string StrMax = ents.FirstOrDefault().NUM;
                    StrResult = GetNumber(StrMax);


                }
                else
                {
                    StrResult = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "[" + DateTime.Now.Year + "]01号";
                }
            }
            else
                StrResult = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "[" + DateTime.Now.Year + "]01号";
            return StrResult;

        }
        /// <summary>
        /// 根据公文编号  获取公文的序号
        /// </summary>
        /// <param name="CompanyDoc"></param>
        /// <returns></returns>
        private int GetCompanyNums(string StrCompanyDoc)
        {
            int IntResut = -1;
            try
            {

                int IntPosition = -1;//]或】所处的位置
                int IntStart = -1;//[所处的位置
                string StrYear = "";//[]里的数字
                string StrYearNum = "";
                //int IntLastChar=-1;//“号”所处的位置
                if (StrCompanyDoc.IndexOf(']') > 0)//英文状态下的]
                {
                    IntPosition = StrCompanyDoc.IndexOf(']');
                }
                if (StrCompanyDoc.IndexOf('】') > 0)//中文下半角]
                {
                    IntPosition = StrCompanyDoc.IndexOf('】');
                }
                if (StrCompanyDoc.IndexOf('[') > 0)//英文状态下的[
                {
                    IntStart = StrCompanyDoc.IndexOf('[');
                }
                if (StrCompanyDoc.IndexOf('【') > 0)//中文下半角【
                {
                    IntStart = StrCompanyDoc.IndexOf('【');
                }
                StrYear = StrCompanyDoc.Substring(IntStart + 1, IntPosition - IntStart - 1);
                if (IntPosition > 0)
                {
                    StrYearNum = StrYear + StrCompanyDoc.Substring(IntPosition + 1, StrCompanyDoc.Length - 2 - IntPosition);
                    IntResut = System.Convert.ToInt32(StrYearNum);// -2是去掉最后一个“号”字
                }

            }
            catch (Exception ex)
            {
                IntResut = 0;
            }
            return IntResut;
        }

        #endregion

        #region 获取公文信息


        void audit_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
        }

        void SendDocClient_GetSendDocSingleInfoByIdCompleted(object sender, GetSendDocSingleInfoByIdCompletedEventArgs e)
        {

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        T_OA_SENDDOC senddoc = new T_OA_SENDDOC();
                        senddoc = e.Result; 
                        tmpSendDocT = senddoc;
                        ShowDistributButton();
                        //HideDistributeButton();
                        if (tmpSendDocT.ISREDDOC == "0")
                        {
                            tmpIsRed = "否";
                        }
                        else
                        {
                            tmpIsRed = "是";
                        }
                        //personClient.getemployee
                        personClient.GetEmployeePostBriefByEmployeeIDAsync(tmpSendDocT.OWNERID);
                        //personClient.GetEmployeeDetailByIDAsync(tmpSendDocT.OWNERID);
                        GetDepartmentNameByDepartmentID(tmpSendDocT.DEPARTID);
                        this.txtZhuSend.Text = string.IsNullOrEmpty(tmpSendDocT.SEND) ? "" : tmpSendDocT.SEND;
                        this.txtChaoSend.Text = string.IsNullOrEmpty(tmpSendDocT.CC) ? "" : tmpSendDocT.CC;
                        if (!string.IsNullOrEmpty(tmpSendDocT.TEL))
                        {
                            this.txtTel.Text = tmpSendDocT.TEL;
                        }
                        this.txtTemplateTitle.Text = string.IsNullOrEmpty(tmpSendDocT.SENDDOCTITLE) ? "" : tmpSendDocT.SENDDOCTITLE;
                        this.txtNUM.Text = string.IsNullOrEmpty(tmpSendDocT.NUM) ? "" : tmpSendDocT.NUM;
                        if (tmpSendDocT.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || tmpSendDocT.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                        {
                            RefreshUI(RefreshedTypes.All);
                        }
                        switch (tmpSendDocT.ISREDDOC)
                        {
                            case "0":
                                this.RbtNo.IsChecked = true;
                                this.rbtYes.IsChecked = false;
                                break;
                            case "1":
                                this.RbtNo.IsChecked = false;
                                this.rbtYes.IsChecked = true;
                                break;
                        }

                        this.txtKey.Text = string.IsNullOrEmpty(tmpSendDocT.KEYWORDS) ? "" : tmpSendDocT.KEYWORDS;

                        //txtContent.RichTextBoxContext = tmpSendDocT.CONTENT;
                        //txtContent.Document = tmpSendDocT.CONTENT;
                        SelectDocType = senddoc.T_OA_SENDDOCTYPE;
                        //tmpStrcbxDocType = tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE;
                        tmpStrcbxGrade = tmpSendDocT.GRADED;
                        tmpGrade = tmpStrcbxGrade;
                        tmpStrcbxProritity = tmpSendDocT.PRIORITIES;
                        tmpProri = tmpStrcbxProritity;
                        if (!string.IsNullOrEmpty(tmpSendDocT.GRADED.ToString()))
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
                        if (!string.IsNullOrEmpty(tmpSendDocT.PRIORITIES.ToString()))
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
                        InitComboxSource();
                        if (action == FormTypes.Audit)
                        {
                            
                        }
                        if (action == FormTypes.Resubmit)
                        {
                            tmpSendDocT.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                        }
                        RefreshUI(RefreshedTypes.AuditInfo);
                        RefreshUI(RefreshedTypes.All);

                        //if (IsMainUser)
                        //{


                        //    IsMainUser = false;
                        //}


                        //InitAudit(senddoc);
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

        //private void HideDistributeButton()
        //{
        //    if (tmpSendDocT.CHECKSTATE != "2" || tmpSendDocT.ISDISTRIBUTE != "0")
        //    {
        //        var toolbar = GetToolBarItems();
        //        int count = 0;
        //        foreach (var t in toolbar)
        //        {
        //            if (t.Key == "3") toolbar.RemoveAt(count);
        //            count++;
        //        }
        //    }
        //    else
        //    {
        //        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
        //    }
        //}

        private void ShowDistributButton()
        {
            if (tmpSendDocT != null && tmpSendDocT.CHECKSTATE == "2" && tmpSendDocT.ISDISTRIBUTE == "0")
            {
                PermissionServiceClient PermClient = new PermissionServiceClient();
                PermClient.GetCustomerPermissionByUserIDAndEntityCodeCompleted += (to, te) =>
                {
                    if (te.Result != null)
                    {
                        canShowDistButton = true;
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                        //bar.ButtonContainer.Children.Clear();
                        if (IsAudit && canShowDistButton)
                        {
                            if (tmpSendDocT != null && tmpSendDocT.CHECKSTATE == "2" && tmpSendDocT.ISDISTRIBUTE == "0")
                            {
                                ImageButton btn;
                                string img;
                                btn = new ImageButton();
                                btn.TextBlock.Text = "发布";
                                img = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/mnu_actions.png";

                                btn.Image.Source = new BitmapImage(new Uri(img, UriKind.Relative));
                                btn.Style = (Style)Application.Current.Resources["ButtonToolBarStyle"];
                                btn.Click += new RoutedEventHandler(DistributeCompanyDoc);
                                bar.ButtonContainer.Children.Add(btn);
                            }
                        }    
                        //GetToolBarItems();
                    }
                };
                PermClient.GetCustomerPermissionByUserIDAndEntityCodeAsync(Common.CurrentLoginUserInfo.SysUserID, "T_OA_SENDDOC");
            }
        }


        private void SetReadOnly()
        {
            this.txtZhuSend.IsEnabled = false;
            this.txtChaoSend.IsEnabled = false;
            this.txtTel.IsEnabled = false;
            this.txtTemplateTitle.IsEnabled = false;
            this.txtNUM.IsEnabled = false;
            this.txtKey.IsEnabled = false;
            this.txtContent.IsEnabled = false;
            this.cbxDocType.IsEnabled = false;
            this.cbxGrade.IsEnabled = false;
            this.cbxProritity.IsEnabled = false;
            //ctrFile.IsEnabled = false;
            this.btnLookUpPartyb.IsEnabled = false;
            this.btnLookUpPeople.IsEnabled = false;
            this.cbxTemplateName.IsEnabled = false;
            this.RbtNo.IsEnabled = false;
            this.rbtYes.IsEnabled = false;
            //this.txtContent.HideControls();//屏蔽富文本框的头部
            this.txtContent.IsReadOnly = true;

            txtContent.BorderThickness = new Thickness(1.0);
            txtContent.BorderBrush = new SolidColorBrush(Colors.Gray);

        }
        private void GetSendDocInfo(V_BumfCompanySendDoc VSend)
        {
            try
            {
                if (VSend.OACompanySendDoc != null)
                {
                    tmpSendDocT = VSend.OACompanySendDoc;
                    GetDepartmentNameByDepartmentID(tmpSendDocT.DEPARTID);
                    this.txtZhuSend.Text = tmpSendDocT.SEND;
                    this.txtChaoSend.Text = tmpSendDocT.CC;
                    this.txtTel.Text = tmpSendDocT.TEL;
                    this.txtTemplateTitle.Text = tmpSendDocT.SENDDOCTITLE;
                    if (!string.IsNullOrEmpty(tmpSendDocT.NUM))
                    {
                        this.txtNUM.Text = tmpSendDocT.NUM;
                    }
                    this.txtKey.Text = tmpSendDocT.KEYWORDS;
                    //txtContent.RichTextBoxContext = tmpSendDocT.CONTENT;
                    //txtContent.Document = tmpSendDocT.CONTENT;
                    SelectDocType = VSend.doctype;
                    //tmpStrcbxDocType = tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE;
                    tmpStrcbxGrade = tmpSendDocT.GRADED;
                    tmpStrcbxProritity = tmpSendDocT.PRIORITIES;
                    if (!string.IsNullOrEmpty(tmpSendDocT.GRADED.ToString()))
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
                    if (!string.IsNullOrEmpty(tmpSendDocT.PRIORITIES.ToString()))
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
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        #endregion

        #region 填充级别信息
        private void Combox_ItemSourceDocType()
        {
            //SendDocClient.GetDocTypeNameInfosToComboxAsync();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            SendDocClient.GetDocTypeInfosAsync();

        }

        /// <summary>
        /// 隐藏一些公文类型
        /// </summary>
        /// <param name="sendTypeList"></param>
        /// <returns></returns>
        private ObservableCollection<T_OA_SENDDOCTYPE> HideSendType(ObservableCollection<T_OA_SENDDOCTYPE> sendTypeList)
        {
            //List<string> typeList = new List<string>();
            //typeList.Add("公文类型");
            //typeList.Add("一般紧急");
            //typeList.Add("通知");
            //typeList.Add("紧急通知");
            //typeList.Add("一般公文");
            string createName  = "杨双贵";//这个人建的不需要了，临时解决办法
            ObservableCollection<T_OA_SENDDOCTYPE> tempList = new ObservableCollection<T_OA_SENDDOCTYPE>();
            for (int i = 0; i < sendTypeList.Count; i++)
            {
                if (sendTypeList[i].CREATEUSERNAME != createName)
                {
                    tempList.Add(sendTypeList[i]);
                }
            }

            return tempList;
        }

        void SendDocClient_GetDocTypeInfosCompleted(object sender, GetDocTypeInfosCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result != null)
            {
               // this.cbxDocType.ItemsSource = e.Result;
                if (action == FormTypes.Edit || action == FormTypes.New || action == FormTypes.Resubmit)//如果为修改，新建，重新提交才隐藏，因为查看要看到原来的数据
                {
                    this.cbxDocType.ItemsSource = this.HideSendType(e.Result);
                }
                else
                {
                    this.cbxDocType.ItemsSource = e.Result;
                }
                this.cbxDocType.DisplayMemberPath = "SENDDOCTYPE";

                if (SelectDocType.SENDDOCTYPE != null)
                {
                    foreach (var item in cbxDocType.Items)
                    {
                        T_OA_SENDDOCTYPE dict = item as T_OA_SENDDOCTYPE;
                        if (dict != null)
                        {
                            if (dict.SENDDOCTYPE == SelectDocType.SENDDOCTYPE)
                            {

                                cbxDocType.SelectedItem = item;
                                break;
                            }
                        }
                    }

                }
                //else
                //{
                //    cbxDocType.SelectedIndex = 0;
                //}

            }
            //RefreshUI(RefreshedTypes.ProgressBar);
        }



        #endregion

        #region 取消按钮
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //this.DialogResult = false;
        }
        #endregion

        
        //by luojie 恢复btnSubmit_Click改变的内容
        private void BackToSubmit()
        {
            needsubmit = false;
            isSubmit = false;
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        #region 添加/修改/审核
        private void SaveSendDocInfo()
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string StrTitle = ""; //标题
            string StrNum = ""; //模板名
            string StrGrade = "";//级别

            string StrProritity = "";//缓急
            byte[] StrContent;//内容

            string StrSender = "";  //主送人
            string CopySender = "";//抄送人
            string StrTel = ""; //联系电话
            string StrKey = ""; //关键字
            T_SYS_DICTIONARY GradeObj = cbxGrade.SelectedItem as T_SYS_DICTIONARY;//级别
            T_SYS_DICTIONARY ProritityObj = cbxProritity.SelectedItem as T_SYS_DICTIONARY;//缓急
            StrTitle = this.txtTemplateTitle.Text.Trim().ToString();
            StrNum = this.txtNUM.Text.Trim().ToString();
            if (this.cbxGrade.SelectedIndex == 0 || this.cbxGrade.SelectedIndex == -1)
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCGRADENOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.cbxGrade.Focus();
                return;
            }
            if (this.cbxProritity.SelectedIndex == 0 || this.cbxProritity.SelectedIndex == -1)
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCPRIORITYNOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.cbxProritity.Focus();
                return;
            }
            StrGrade = GradeObj.DICTIONARYNAME.ToString();
            tmpGrade = StrGrade;
            StrProritity = ProritityObj.DICTIONARYNAME.ToString();
            tmpProri = StrProritity;
            StrKey = this.txtKey.Text.Trim().ToString();
            //Strtype = this.cbxDocType.SelectedItem.ToString();
            if (this.cbxDocType.SelectedIndex > -1)
            {
                SelectDocType = this.cbxDocType.SelectedItem as T_OA_SENDDOCTYPE;
            }
            else
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCTYPENOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.cbxDocType.Focus();
                return;
            }
            //StrContent = txtContent.RichTextBoxContext;
            try
            {
                StrContent = txtContent.Document;
            }
            catch (Exception ex)
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCCONTEXTERR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }
            CopySender = this.txtChaoSend.Text.Trim().ToString();
            StrSender = this.txtZhuSend.Text.Trim().ToString();
            StrTel = this.txtTel.Text.Trim().ToString();
            //if (!string.IsNullOrEmpty(StrTel))
            //{
            //    string StrRex = @"((\d{11})|^((\d{7,8})|(\d{4}|\d{3})-(\d{7,8})|(\d{4}|\d{3})-(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1})|(\d{7,8})-(\d{4}|\d{3}|\d{2}|\d{1}))$)";
            //    Regex regex = new Regex(StrRex);
            //    if (!regex.IsMatch(StrTel))
            //    {
            //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "请输入",
            //        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //        this.cbxDocType.Focus();
            //        return;
            //    }
            //}
            if (string.IsNullOrEmpty(StrTitle))
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCTITLENOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.txtTemplateTitle.Focus();
                return;

            }
            string StrFlag = "";

            if (this.rbtYes.IsChecked == true)
            {
                StrFlag = "1";
                tmpIsRed = "是";
                if (string.IsNullOrEmpty(StrKey))
                {
                    BackToSubmit();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("KEYWORDSNOTNULL"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.txtKey.Focus();
                    return;
                }
            }
            if (this.RbtNo.IsChecked == true)
            {
                StrFlag = "0";
                tmpIsRed = "否";
            }

            if (StrContent != null)
            {
                if (StrContent.Length == 0)
                {
                    BackToSubmit();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCCONTENTNOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    this.txtContent.Focus();
                    return;

                }
            }
            else
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SENDDOCCONTENTNOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                this.txtContent.Focus();
            }


            if (!string.IsNullOrEmpty(StrNum))
            {
                bool IsCheck = true;
                if (!((StrNum.IndexOf('[') > 0 && StrNum.IndexOf(']') > 0) || (StrNum.IndexOf('【') > 0 && StrNum.IndexOf('】') > 0)))
                {
                    IsCheck = false;
                }
                if (StrNum.Substring(StrNum.Length - 1, 1) != "号")
                {
                    IsCheck = false;
                }

                if (!IsCheck)
                {
                    BackToSubmit();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "公文编号格式不对，请参考集团本部人力[2011]01号或集团本部人力【2011】01号",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                int IntStart = -1;
                int IntEnd = -1;
                string StrYear = "";//[]里的数字

                //int IntLastChar=-1;//“号”所处的位置
                if (StrNum.IndexOf(']') > 0)//英文状态下的]
                {
                    IntEnd = StrNum.IndexOf(']');
                }
                if (StrNum.IndexOf('】') > 0)//中文下半角]
                {
                    IntEnd = StrNum.IndexOf('】');
                }
                if (StrNum.IndexOf('[') > 0)//英文状态下的[
                {
                    IntStart = StrNum.IndexOf('[');
                }
                if (StrNum.IndexOf('【') > 0)//中文下半角【
                {
                    IntStart = StrNum.IndexOf('【');
                }
                StrYear = StrNum.Substring(IntStart + 1, IntEnd - IntStart - 1);
                if (StrYear != DateTime.Now.Year.ToString())
                {
                    BackToSubmit();
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "公文编号格式不对，[]或【】里面应该为当前年份如：【2011】",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }



            }
            if (string.IsNullOrEmpty(StrDepartmentID))
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("DEPARTMENTNOTNULL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                return;
            }


            if (CheckCompanyDco())
            {

                if (action == FormTypes.New)
                {
                    
                    tmpSendDocT.GRADED = StrGrade;
                    tmpSendDocT.PRIORITIES = StrProritity;
                    tmpSendDocT.SENDDOCTITLE = StrTitle;
                    tmpSendDocT.SEND = StrSender;
                    tmpSendDocT.CC = CopySender;
                    tmpSendDocT.ISREDDOC = StrFlag;
                    tmpSendDocT.T_OA_SENDDOCTYPE = SelectDocType;

                    tmpSendDocT.CONTENT = StrContent;
                    tmpSendDocT.DEPARTID = StrDepartmentID;
                    tmpSendDocT.NUM = StrNum;
                    tmpSendDocT.KEYWORDS = StrKey;

                    tmpSendDocT.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    tmpSendDocT.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    tmpSendDocT.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpSendDocT.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                    tmpSendDocT.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpSendDocT.TEL = StrTel;
                    tmpSendDocT.UPDATEUSERNAME = "";
                    tmpSendDocT.UPDATEDATE = null;
                    tmpSendDocT.UPDATEUSERID = "";

                    tmpSendDocT.ISDISTRIBUTE = "0";
                    tmpSendDocT.ISSAVE = "0";

                    //tmpSendDocT.CREATEDATE = System.DateTime.Now;
                    tmpSendDocT.CHECKSTATE = "0";
                    tempStrSendDocName = StrTitle;

                    SendDocClient.SendDocAddAsync(tmpSendDocT, "Add");
                }
                if (action == FormTypes.Edit || action == FormTypes.Resubmit)
                {
                    tmpSendDocT.GRADED = StrGrade;
                    tmpSendDocT.PRIORITIES = StrProritity;
                    tmpSendDocT.SENDDOCTITLE = StrTitle;
                    tmpSendDocT.SEND = StrSender;
                    tmpSendDocT.CC = CopySender;
                    tmpSendDocT.ISREDDOC = StrFlag;
                    //tmpSendDocT.T_OA_SENDDOCTYPEReference = new EntityReferenceOfT_OA_SENDDOCTYPEUlJdEHjk();

                    //tmpSendDocT.T_OA_SENDDOCTYPEReference.EntityKey = SelectDocType.EntityKey;
                    tmpSendDocT.T_OA_SENDDOCTYPE = SelectDocType;
                    //tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE = SelectDocType.SENDDOCTYPE;
                    tmpSendDocT.CHECKSTATE = "0";
                    //tmpSendDocT.CONTENT = txtContent.RichTextBoxContext;
                    tmpSendDocT.CONTENT = txtContent.Document;
                    tmpSendDocT.DEPARTID = StrDepartmentID;
                    tmpSendDocT.NUM = StrNum;
                    tmpSendDocT.TEL = StrTel;
                    tmpSendDocT.KEYWORDS = StrKey;

                    //tmpSendDocT.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                    //tmpSendDocT.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                    //tmpSendDocT.OWNERID = StrAddUserID;
                    //tmpSendDocT.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    //tmpSendDocT.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;

                    tmpSendDocT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    tmpSendDocT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    //tmpSendDocT.UPDATEDATE = System.DateTime.Now;  取服务器端时间


                    tempStrSendDocName = StrTitle;

                    SendDocClient.SendDocInfoUpdateAsync(tmpSendDocT, StrUpdateReturn, "Edit");

                }
                //if (IsButtonSave)
                //{
                //    Save();
                //}
            }
            else
            {
                BackToSubmit();
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("信息获取失败,请重试"), Utility.GetResourceStr("COMFIRM"), MessageIcon.Error);
                return;
            }
        }

        /// <summary>
        ///     by luojie
        ///     提交按钮重载，提交前先保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
           
            needsubmit = true;
            isSubmit = true;

            #region 隐藏entitybrowser中的toolbar按钮
            entBrowser.BtnSaveSubmit.IsEnabled = false;
            if (entBrowser.EntityEditor is IEntityEditor)
            {
                List<ToolbarItem> bars = GetToolBarItems();
                if (bars != null)
                {
                    ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    if (bar != null)
                    {
                        bar.Visibility = Visibility.Collapsed;
                    }
                }
            }
            SaveSendDocInfo();
        }

        private void Save(FormTypes action)
        {
            try
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                if (tmpSendDocT.ISREDDOC == "0")
                    tmpIsRed = "否";
                else
                    tmpIsRed = "是";
                switch (action)
                {
                    case FormTypes.New:

                        SendDocClient.SendDocAddAsync(tmpSendDocT, "Add");
                        break;
                    case FormTypes.Edit:
                        SendDocClient.SendDocInfoUpdateAsync(tmpSendDocT, StrUpdateReturn, "Edit");
                        break;
                }
            }
            catch (Exception ex)
            {
                //Utility.ShowCustomMessage();
                RefreshUI(RefreshedTypes.HideProgressBar);
                return;
            }

        }

        private bool CheckCompanyDco()
        {
            List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            if (validators.Count > 0)
            {
                foreach (var h in validators)
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(h.ErrorMessage),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 修改按钮
        //private void UpdateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.SaveSendDocInfo(FormTypes.Edit);
        //}

        void SendDocClient_SendDocInfoUpdateCompleted(object sender, SendDocInfoUpdateCompletedEventArgs e)
        {

            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                else
                {
                    if (e.StrResult != "")
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr(e.StrResult, "COMPANYDOCUMENT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                    else
                    {

                        if (e.UserState.ToString() == "Edit")
                        {

                            UserInfo User = new UserInfo();
                            User.COMPANYID = tmpSendDocT.OWNERCOMPANYID;
                            User.DEPARTMENTID = tmpSendDocT.OWNERDEPARTMENTID;
                            User.POSTID = tmpSendDocT.OWNERPOSTID;
                            User.USERID = tmpSendDocT.OWNERID;
                            User.USERNAME = tmpSendDocT.OWNERNAME;
                            //publicClient.UpdateContentAsync(tmpSendDocT.APPROVALID, tmpSendDocT.CONTENT, tmpSendDocT.OWNERCOMPANYID, "OA", "T_OA_APPROVAL");               
                            publicClient.UpdateContentAsync(tmpSendDocT.SENDDOCID, tmpSendDocT.CONTENT, User);

                            

                            if (!isSubmit)
                            {
                                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                            }
                            else
                            {
                                saveType = RefreshedTypes.LeftMenu;
                            }
                            
                            if (GlobalFunction.IsSaveAndClose(saveType))
                            {
                                RefreshUI(saveType);
                            }
                        }
                        else if (e.UserState.ToString() == "Audit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        else if (e.UserState.ToString() == "Submit")
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                        }
                        canSubmit = true;
                        RefreshUI(RefreshedTypes.All);

                    }

                }
                //by luojie  提交功能
                if (needsubmit)
                {
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.ManualSubmit();
                    needsubmit = false;
                    isSubmit = false;

                    //#region 隐藏entitybrowser中的toolbar按钮
                    //entBrowser.BtnSaveSubmit.IsEnabled = false;
                    //if (entBrowser.EntityEditor is IEntityEditor)
                    //{
                    //    List<ToolbarItem> bars = GetToolBarItems();
                    //    if (bars != null)
                    //    {
                    //        ToolBar bar = SMT.SaaS.FrameworkUI.Common.Utility.FindChildControl<ToolBar>(entBrowser, "toolBar1");
                    //        if (bar != null)
                    //        {
                    //            bar.Visibility = Visibility.Collapsed;
                    //        }
                    //    }
                    //}
                    #endregion
                    if (refreshType == RefreshedTypes.CloseAndReloadData)
                    {
                        refreshType = RefreshedTypes.AuditInfo;
                        saveType = refreshType;
                    }
                    RefreshUI(saveType);
                }

            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }

        }
        #endregion

        #region 添加按钮

        //private void AddButton_Click(object sender, RoutedEventArgs e)
        //{
        //    this.SaveSendDocInfo(FormTypes.Add);

        //}

        void SendDocClient_SendDocAddCompleted(object sender, SendDocAddCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            try
            {
                //RefreshUI(RefreshedTypes.ProgressBar);
                if (!e.Cancelled)
                {
                    if (e.Error == null)
                    {
                        if (e.Result == "")
                        {
                            UserInfo User = new UserInfo();
                            User.COMPANYID = tmpSendDocT.OWNERCOMPANYID;
                            User.DEPARTMENTID = tmpSendDocT.OWNERDEPARTMENTID;
                            User.POSTID = tmpSendDocT.OWNERPOSTID;
                            User.USERID = tmpSendDocT.OWNERID;
                            User.USERNAME = tmpSendDocT.OWNERNAME;
                            publicClient.AddContentAsync(tmpSendDocT.SENDDOCID, tmpSendDocT.CONTENT, tmpSendDocT.OWNERCOMPANYID, "OA", "T_OA_SENDDOC", User);
                            
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                            if (GlobalFunction.IsSaveAndClose(saveType))
                            {
                                RefreshUI(saveType);
                            }
                            else
                            {
                                action = FormTypes.Edit;
                                canSubmit = true;
                                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                                entBrowser.FormType = FormTypes.Edit;
                                RefreshUI(RefreshedTypes.AuditInfo);
                                RefreshUI(RefreshedTypes.All);
                            }
                        }
                        else
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Result.ToString(), "COMPANYDOC"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                            return;
                        }
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
        }

        //提交审核流程
        void SendDocClient_SubmitCompanyDocFlowCompleted(object sender, SubmitCompanyDocFlowCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result == 1)
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "COMPANYDOC"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.Close);

                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("FAILED"), Utility.GetResourceStr("AUDITFAILED", "COMPANYDOC"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
            }
        }

        #endregion

        #region 提交审核按钮
        private void AddAndConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            AuditType = true;
            action = FormTypes.Audit;
            this.SaveSendDocInfo();
        }
        #endregion

        #region 公文类型模板变化

        private void cbxDocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectChange)
            {
                if (cbxDocType.SelectedItem == null)
                {
                    return;
                }
                else
                {
                    T_OA_SENDDOCTYPE DocType = new T_OA_SENDDOCTYPE();
                    DocType = cbxDocType.SelectedItem as T_OA_SENDDOCTYPE;
                    string StrType = DocType.SENDDOCTYPE.ToString();

                    SendDocClient.GetDocTypeTemplateContentByDocTypeAsync(StrType);
                }
            }
            else
            {
                SelectChange = true;
            }
        }

        void SendDocClient_GetDocTypeTemplateContentByDocTypeCompleted(object sender, GetDocTypeTemplateContentByDocTypeCompletedEventArgs e)
        {

            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    tmpinfos = e.Result.ToList();

                    //T_OA_SENDDOCTEMPLATE tmpTemplate = new T_OA_SENDDOCTEMPLATE();
                    //tmpTemplate.TEMPLATENAME = "请选择";
                    //tmpinfos.Insert(0, tmpTemplate);
                    //this.cbxTemplateName.Items.Clear();
                    this.cbxTemplateName.ItemsSource = tmpinfos;
                    this.cbxTemplateName.DisplayMemberPath = "TEMPLATENAME";
                    this.cbxTemplateName.SelectedIndex = 0;
                    //this.cbxTemplateName.ItemsSource = 
                }
                else
                {
                    cbxTemplateName.ItemsSource = null;
                    txtContent.Document = null;
                    txtTemplateTitle.Text = "";
                    cbxGrade.SelectedItem = null;
                    cbxProritity.SelectedItem = null;
                    //T_OA_SENDDOCTEMPLATE tmpTemplate = new T_OA_SENDDOCTEMPLATE();
                    //tmpTemplate.TEMPLATENAME = "请选择";
                    //tmpinfos.Insert(0,tmpTemplate);
                    //this.cbxTemplateName.ItemsSource = tmpinfos;
                    //this.cbxTemplateName.DisplayMemberPath = "TEMPLATENAME";
                    //this.cbxTemplateName.SelectedIndex = 0;                    
                }
            }
        }

        #endregion

        #region 模板名称变化

        private void cbxTemplateName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cbxName = sender as ComboBox;
            T_OA_SENDDOCTEMPLATE docTemplate = cbxName.SelectedItem as T_OA_SENDDOCTEMPLATE;
            if (docTemplate != null)
            {
                string TemplateID = docTemplate.SENDDOCTEMPLATEID;

                SendDocClient.GetDocTypeTemplateSingleInfoByIdCompleted += new EventHandler<GetDocTypeTemplateSingleInfoByIdCompletedEventArgs>(SendDocClient_GetDocTypeTemplateSingleInfoByIdCompleted);
                SendDocClient.GetDocTypeTemplateSingleInfoByIdAsync(TemplateID);
            }
        }

        void SendDocClient_GetDocTypeTemplateSingleInfoByIdCompleted(object sender, GetDocTypeTemplateSingleInfoByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    T_OA_SENDDOCTEMPLATE temTemplateT = e.Result as T_OA_SENDDOCTEMPLATE;
                    //string tmpStrcbxGrade = temTemplateT.GRADED;
                    //string tmpStrcbxProritity = temTemplateT.PRIORITIES;
                    if (action == FormTypes.New || action == FormTypes.Edit)
                    {
                        //txtContent.RichTextBoxContext = temTemplateT.CONTENT;
                        txtContent.Document = temTemplateT.CONTENT;
                        txtTemplateTitle.Text = temTemplateT.SENDDOCTITLE;
                    }

                    if (!string.IsNullOrEmpty(temTemplateT.GRADED.ToString()))
                    {
                        foreach (T_SYS_DICTIONARY Region in cbxGrade.Items)
                        {
                            if (Region.DICTIONARYNAME == temTemplateT.GRADED)
                            {
                                cbxGrade.SelectedItem = Region;
                                break;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(temTemplateT.PRIORITIES.ToString()))
                    {
                        foreach (T_SYS_DICTIONARY Region in cbxProritity.Items)
                        {
                            if (Region.DICTIONARYNAME == temTemplateT.PRIORITIES)
                            {
                                cbxProritity.SelectedItem = Region;
                                break;
                            }
                        }
                    }

                    //暂时取不到数据  里面有标签
                    //string StrContent = txtContent.GetRichTextbox().Xaml.Trim().ToString();
                    //if (StrContent.Length > 10)
                    //{
                    //    this.txtKey.Text = StrContent.Substring(0, 8);//取摸扳中前20个字默认为关键字
                    //}
                }
            }
        }

        #endregion

        #region 选择部门

        private void PostsObject_FindClick(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Department;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    PostsObject.Text = companyInfo.ObjectName;
                    StrDepartmentID = companyInfo.ObjectID;
                    StrSendDepartment = companyInfo.ObjectName;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        private void GetDepartmentNameByDepartmentID(string StrDepartmentID)
        {
            OrganizationServiceClient Organ = new OrganizationServiceClient();
            Organ.GetDepartmentByIdCompleted += new EventHandler<GetDepartmentByIdCompletedEventArgs>(Organ_GetDepartmentByIdCompleted);
            Organ.GetDepartmentByIdAsync(StrDepartmentID);

        }
        void Organ_GetDepartmentByIdCompleted(object sender, GetDepartmentByIdCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT department = new SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT();
                    department = e.Result;
                    StrDepartmentID = department.DEPARTMENTID;
                    PostsObject.Text = department.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    StrSendDepartment = PostsObject.Text;
                    //PostsObject.DisplayMemberPath = "T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME";
                    //PostsObject.DataContext = department;
                }
            }
        }

        #endregion

        #region 提交流程

        //private void SumbitFlow()
        //{
        //    if (tmpSendDocT != null)
        //    {
        //        SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity = this.audit.AuditEntity;
        //        entity.ModelCode = "CompanyDoc"; //会议申请模块
        //        entity.FormID = tmpSendDocT.SENDDOCID;
        //        entity.CreateCompanyID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
        //        entity.CreateDepartmentID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
        //        entity.CreatePostID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
        //        entity.CreateUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.CreateUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.EditUserID = Common.CurrentLoginUserInfo.EmployeeID;
        //        entity.EditUserName = Common.CurrentLoginUserInfo.EmployeeName;
        //        entity.Content = "doc audit";
        //        audit.XmlObject = DataObjectToXml<T_OA_SENDDOC>.ObjListToXml(tmpSendDocT, "OA"); 

        //        audit.Submit();
        //    }
        //}
        #endregion

        #region 审核动作

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

        private void SumbitCompleted()
        {
            try
            {
                if (tmpSendDocT != null)
                {
                    AuditType = true;
                    tmpSendDocT.UPDATEDATE = DateTime.Now;
                    tmpSendDocT.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    tmpSendDocT.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

                    switch (auditResult)
                    {
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                            tmpSendDocT.CHECKSTATE = Utility.GetCheckState(CheckStates.Approving);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                            tmpSendDocT.CHECKSTATE = Utility.GetCheckState(CheckStates.Approved);
                            break;
                        case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                            tmpSendDocT.CHECKSTATE = Utility.GetCheckState(CheckStates.UnApproved);
                            break;
                    }
                    if (action == FormTypes.Edit || action == FormTypes.Audit)
                    {
                        //EditAuditType = true;
                        tmpSendDocT.T_OA_SENDDOCTYPE = SelectDocType;
                        SendDocClient.SendDocInfoUpdateAsync(tmpSendDocT, StrUpdateReturn);

                    }
                    if (action == FormTypes.New)
                    {
                        tmpSendDocT.T_OA_SENDDOCTYPE = SelectDocType;
                        SendDocClient.SendDocAddAsync(tmpSendDocT);
                    }


                }
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
        }

        private void Cancel()
        {
            RefreshUI(RefreshedTypes.Close);

        }

        private void HandError()
        {
            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("AUDITFAILURE"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

        }
        #endregion

        #region LayoutRoot_SizeChanged
        private void LayoutRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            txtContent.Height = ((Grid)sender).ActualHeight * 0.35;
        }
        #endregion

        #region IEntityEditor 成员
        public string GetTitle()
        {
            string Strreturn = "";


            switch (action)
            {
                case FormTypes.New:
                    Strreturn = Utility.GetResourceStr("ADDTITLE", "COMPANYDOC");
                    break;
                case FormTypes.Edit:
                    Strreturn = Utility.GetResourceStr("EDITTITLE", "COMPANYDOC");
                    break;
                case FormTypes.Audit:
                    Strreturn = Utility.GetResourceStr("AUDITTITLE", "COMPANYDOC");
                    break;
            }
            return Strreturn;

        }

        public string GetStatus()
        {

            return "";
        }

        public void DoAction(string actionenum)
        {

            switch (actionenum)
            {
                case "0"://保存
                    IsSave = true;
                    AuditType = false;
                    saveType = RefreshedTypes.LeftMenu;
                    IsButtonSave = true;
                    SaveSendDocInfo();
                    break;
                case "1"://保存并关闭
                    saveType = RefreshedTypes.CloseAndReloadData;
                    AuditType = false;
                    IsButtonSave = true;
                    SaveSendDocInfo();

                    break;
                case "2"://提交审核
                    saveType = RefreshedTypes.CloseAndReloadData;
                    AuditType = true;
                    SaveSendDocInfo();
                    break;
                case "3":
                    //DistributeCompanyDoc();
                    break;

            }

        }

        private void DistributeCompanyDoc(object sender, RoutedEventArgs e)
        {
            bool isExistDelButton = false;
            if (action == FormTypes.Audit)
            {
                isExistDelButton = true;
            }
            AddDistrbuteForm AddWin = new AddDistrbuteForm(tmpSendDocT.SENDDOCID,isExistDelButton);

            EntityBrowser browser = new EntityBrowser(AddWin);
            browser.MinHeight = 380;
            browser.MinWidth = 600;
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { });
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

            if (action != FormTypes.Browse && action != FormTypes.Audit)
            {
                //ToolbarItem item = new ToolbarItem
                //{
                //    DisplayType = ToolbarItemDisplayTypes.Image,
                //    Key = "2", //提交审核
                //    Title = Utility.GetResourceStr("SUBMITAUDIT"),// "提交审核",
                //    ImageUrl = "/SMT.SaaS.OA.UI;Component/Images/18_audit.png"
                //};
                //items.Add(item);
                ToolbarItem item = new ToolbarItem
                {
                    DisplayType = ToolbarItemDisplayTypes.Image,
                    Key = "1", //保存并关闭
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

        public void ReloadData()
        {
            if (ReloadDataEvent != null)
            {
                ReloadDataEvent();
            }
        }
        #endregion

        #region IAudit 成员

        private string GetXmlString(string StrSource, T_OA_SENDDOC Info)
        {
            #region Xml字符串


            #endregion
            SMT.SaaS.MobileXml.MobileXml mx = new MobileXml.MobileXml();


            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            if (tmpSendDocT == null)
                return "";
            string StrCheck = "";
            switch (tmpSendDocT.CHECKSTATE)
            {
                case "0":
                    StrCheck = "未提交";
                    break;
                case "1":
                    StrCheck = "审核中";
                    break;
                case "2":
                    StrCheck = "审核通过";
                    break;
                case "3":
                    StrCheck = "审核不通过";
                    break;

            }
            AutoList.Add(basedata("T_OA_SENDDOC", "CHECKSTATE", tmpSendDocT.CHECKSTATE, StrCheck));
            AutoList.Add(basedata("T_OA_SENDDOC", "OWNERCOMPANYID", tmpSendDocT.OWNERCOMPANYID, CompanyName));
            AutoList.Add(basedata("T_OA_SENDDOC", "OWNERDEPARTMENTID", tmpSendDocT.OWNERDEPARTMENTID, DepartmentName));
            AutoList.Add(basedata("T_OA_SENDDOC", "OWNERPOSTID", tmpSendDocT.OWNERPOSTID, PostName));
            AutoList.Add(basedata("T_OA_SENDDOC", "OWNERID", tmpSendDocT.OWNERID, tmpSendDocT.OWNERNAME + "-" + PostName + "-" + DepartmentName + "-" + CompanyName));
            AutoList.Add(basedata("T_OA_SENDDOC", "GRADED", tmpSendDocT.GRADED, tmpGrade));
            AutoList.Add(basedata("T_OA_SENDDOC", "PRIORITIES", tmpSendDocT.PRIORITIES, tmpProri));
            AutoList.Add(basedata("T_OA_SENDDOC", "ISREDDOC", tmpSendDocT.ISREDDOC, tmpIsRed));
            AutoList.Add(basedata("T_OA_SENDDOC", "CONTENT", tmpSendDocT.SENDDOCID, tmpSendDocT.SENDDOCID));
            AutoList.Add(basedata("T_OA_SENDDOC", "AttachMent", tmpSendDocT.SENDDOCID, tmpSendDocT.SENDDOCID));
            AutoList.Add(basedata("T_OA_SENDDOC", "ISDISTRIBUTE", tmpSendDocT.ISDISTRIBUTE, tmpSendDocT.ISDISTRIBUTE == "0" ? "否" : "是"));
            AutoList.Add(basedata("T_OA_SENDDOC", "ISSAVE", tmpSendDocT.ISSAVE, tmpSendDocT.ISSAVE == "0" ? "否" : "是"));
            AutoList.Add(basedata("T_OA_SENDDOC", "DEPARTID", tmpSendDocT.DEPARTID, StrSendDepartment));
            if (tmpSendDocT.T_OA_SENDDOCTYPE != null)
            {
                //AutoList.Add(basedata("T_OA_SENDDOC", "SENDDOCTYPE", tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPEID, tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE));
                //把公文类型ID改为公文类型名字
                AutoList.Add(basedata("T_OA_SENDDOC", "SENDDOCTYPE", tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE, tmpSendDocT.T_OA_SENDDOCTYPE.SENDDOCTYPE));
            }

            string StrReturn = mx.TableToXml(Info, "a", StrSource, AutoList);

            return StrReturn;
        }

        private AutoDictionary basedata(string TableName, string Name, string Value, string Text)
        {
            string[] strlist = new string[4];
            strlist[0] = TableName;
            strlist[1] = Name;
            strlist[2] = Value;
            strlist[3] = Text;
            AutoDictionary ad = new AutoDictionary();
            ad.AutoDictionaryList(strlist);
            return ad;
        }

        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (action == FormTypes.Edit || action == FormTypes.Resubmit)
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            }
            string strXmlObjectSource = string.Empty;
            //entity.ModelCode = "T_OA_SENDDOC";            
            entity.SystemCode = "OA";
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML)) //返回的XML定义不为空时对业务对象进行填充
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, tmpSendDocT);

            Dictionary<string, string> parameters = new Dictionary<string, string>();


            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            if (tmpSendDocT != null)
            {
                paraIDs.Add("CreateUserID", tmpSendDocT.OWNERID);
                paraIDs.Add("CreatePostID", tmpSendDocT.OWNERPOSTID);
                paraIDs.Add("CreateDepartmentID", tmpSendDocT.OWNERDEPARTMENTID);
                paraIDs.Add("CreateCompanyID", tmpSendDocT.OWNERCOMPANYID);
            }

            if (tmpSendDocT.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetAuditEntity(entity, "T_OA_SENDDOC", tmpSendDocT.SENDDOCID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_OA_SENDDOC", tmpSendDocT.SENDDOCID, strXmlObjectSource);
            }
            //Utility.SetAuditEntity(entity, "T_OA_SENDDOC", tmpSendDocT.SENDDOCID, strXmlObjectSource);
        }
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != tmpSendDocT.OWNERID)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if ((action == FormTypes.Resubmit || action == FormTypes.Edit) && canSubmit == false)
            {
                //RefreshUI(RefreshedTypes.ShowProgressBar);
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void OnSubmitCompleted(SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            Utility.InitFileLoad(FormTypes.Audit, uploadFile, tmpSendDocT.SENDDOCID, false);
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
            if (tmpSendDocT.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            tmpSendDocT.CHECKSTATE = state;
            //cmsfc.UpdateContraApprovalAsync(tmpSendDocT, UserState);
            SendDocClient.SendDocInfoUpdateAsync(tmpSendDocT, StrUpdateReturn, UserState);
        }

        public string GetAuditState()
        {

            string state = "-1";
            if (tmpSendDocT != null)
                state = tmpSendDocT.CHECKSTATE;
            //if (action == FormTypes.Browse)
            //{
            //    state = "-1";
            //}
            return state;
        }

        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            SendDocClient.DoClose();
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

        #region 申请人
        private void btnLookUpPeople_FindClick(object sender, RoutedEventArgs e)
        {

            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    string Mobile = "";
                    string Tel = "";
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    //OwnerDepartmentid = deptid;
                    //depName = dept.ObjectName;//部门

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    PostName = postName;//岗位名称
                    DepartmentName = deptName;//部门名称
                    CompanyName = corpName;//公司名称

                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE != null)
                        Mobile = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE.ToString();
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE != null)
                        Tel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE.ToString();
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    txtAddUser.Text = StrEmployee;
                    //txtTel.Text = userInfo.te
                    tmpSendDocT.OWNERCOMPANYID = corpid;
                    tmpSendDocT.OWNERDEPARTMENTID = deptid;
                    tmpSendDocT.OWNERID = userInfo.ObjectID;
                    tmpSendDocT.OWNERPOSTID = postid;
                    tmpSendDocT.OWNERNAME = userInfo.ObjectName;
                    ToolTipService.SetToolTip(txtAddUser, StrEmployee);
                    txtTel.Text = string.Empty;
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        txtTel.Text = Mobile;
                    }
                    if (!string.IsNullOrEmpty(Tel))
                    {
                        if (string.IsNullOrEmpty(txtTel.Text.ToString()))
                        {
                            txtTel.Text = Tel;
                        }
                        else
                        {
                            txtTel.Text += ";" + Tel;
                        }
                    }

                    //SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //txtAddUser.Text = companyInfo.ObjectName;
                    StrAddUserID = userInfo.ObjectID;// companyInfo.ObjectID;
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }
        #endregion

        #region 单选按钮
        private void RbtNo_Click(object sender, RoutedEventArgs e)
        {
            this.RbtNo.IsChecked = true;
            this.rbtYes.IsChecked = false;
        }

        private void rbtYes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtYes.IsChecked = true;
            this.RbtNo.IsChecked = false;
        }
        #endregion

        #region 计算出新的文号
        public string GetNumber(string StrMax)
        {
            int IntPosition = -1;//]或】所处的位置
            int IntStart = -1;//[所处的位置
            string StrYearOld = string.Empty;// 获得[2011]
            string oldend = string.Empty; // 获得01
            string newend = string.Empty; // 计算出新的后面的编号：如02

            string strold = string.Empty; // 获得[2011]01
            string strnew = string.Empty; // 记录生成的编号如：[201102]

            if (StrMax.IndexOf(']') >= 0)//英文状态下的]
            {
                IntPosition = StrMax.IndexOf(']');
            }
            if (StrMax.IndexOf('】') >= 0)//中文下半角]
            {
                IntPosition = StrMax.IndexOf('】');
            }
            if (StrMax.IndexOf('[') >= 0)//英文状态下的[
            {
                IntStart = StrMax.IndexOf('[');
            }
            if (StrMax.IndexOf('【') >= 0)//中文下半角【
            {
                IntStart = StrMax.IndexOf('【');
            }

            StrYearOld = StrMax.Substring(IntStart, IntPosition - IntStart + 1); // 获得[2011]
            oldend = StrMax.Substring(IntPosition + 1, StrMax.Length - IntPosition - 2);

            strold = StrYearOld + oldend;


            newend = (Convert.ToInt32(oldend) + 1).ToString();
            string str = "";
            if (newend.Length < oldend.Length)
            {
                for (int i = 0; i < (oldend.Length - newend.Length); i++)
                {
                    str += "0";
                }
                Console.WriteLine("str:" + str);
                newend = str + newend;
            }
            //strnew = StrYearOld + newend;
            strnew = "【"+DateTime.Now.Year.ToString()+"】" + newend;
            string newStrMax = StrMax.Replace(strold, strnew);
            return newStrMax;
        }

        #endregion

        #region 显示上传控件
        
        
        public void FileLoadedCompleted()
        {
            //_VM.Get_ApporvalAsync(approvalid);
            //if (!ctrFile._files.HasAccessory)
            //{
            //    if (action == FormTypes.Browse || action == FormTypes.Audit)
            //    {
            //        SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(GridInfo, 9);//隐藏附件这行
            //    }
            //}
        }
        #endregion
    }
}
