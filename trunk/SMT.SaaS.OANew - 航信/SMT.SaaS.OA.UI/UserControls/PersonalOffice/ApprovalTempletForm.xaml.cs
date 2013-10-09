////将添加、修改、查看、审核整合到此页面
///这样省的改动流程、引擎的配置，方便以后UI修改的时候要改
///多个页面
///
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.OA.UI.SmtOAPersonOfficeService;
using SMT.SAAS.Main.CurrentContext;
//using SMT.SaaS.FrameworkUI.FileUpload;
using SMT.Saas.Tools.PersonnelWS;
using System.Windows.Data;
using SMT.Saas.Tools.PermissionWS;
using System.Windows.Media;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.ClientServices;
using SMT.SAAS.ClientUtility;
using SMT.SaaS.PublicControls;
using Telerik.Windows.Documents.FormatProviders.Pdf;
using SMT.SaaS.MobileXml;
using SMT.Saas.Tools.PublicInterfaceWS;
using SMT.SAAS.Platform.Logging;
using SMT.SaaS.FrameworkUI.Common;



namespace SMT.SaaS.OA.UI.UserControls
{
    public partial class ApprovalTempletForm : BaseForm, IClient, IEntityEditor//, IFileLoadedCompleted
    {
        #region 页面变量
        SmtOAPersonOfficeClient OaPersonOfficeClient = new SmtOAPersonOfficeClient();
        PublicServiceClient publicClient = new PublicServiceClient();
        PersonnelServiceClient psClient = new PersonnelServiceClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private T_OA_APPROVALINFOTEMPLET approvalInfo = new T_OA_APPROVALINFOTEMPLET();
        public T_OA_APPROVALINFOTEMPLET ApprovalInfo { get { return approvalInfo; } set { approvalInfo = value; } }
        private FormTypes operationType;
        public string postLevel = string.Empty;
        public string depName = string.Empty;
        string approvalid = "";
        string StrApprovaltype = "";
        string StrApprovalOne = "";//事项审批中第一个父节点的  值
        string StrApprovalTwo = "";//事项审批中第二个父节点的 值
        string StrApprovalThird = "";//事项审批中第三个父节点 值

        string OWNERCOMPANYID = "";//所属公司
        string OWNERDEPARTMENTID = "";//获取事项审批时用 用的部门ID
        string OWNERID = "";//所属员工
        string OWNERPOSTID = "";//所属岗位

        string StrDepartmentName = "";//所属公司名称
        string StrCompanyName = "";//所属于部门名称
        string StrPostName = "";//所属岗位名称
        string StrOwnerName = "";//所属人名
        string StrApprovalTypeName = "";//事项审批类型名称
        ObservableCollection<string> lstApprovalids = new ObservableCollection<string>();
        PersonnelServiceClient personclient = new PersonnelServiceClient();
        private RefreshedTypes saveType;
        private bool canSubmit = false;//能否提交审核

        bool isAuditing = false;//用于重提提交后判断页面状态
        #endregion

        #region 构造函数
        /// <summary>
        /// 无参构造函数，供平台中待办新建调用
        /// </summary>
        public ApprovalTempletForm()
        {
            InitializeComponent();
            operationType = FormTypes.New;
            approvalid = "";   
            this.Loaded += new RoutedEventHandler(ApprovalForm_aud_Loaded);
        }

        public ApprovalTempletForm(FormTypes ActionType, string SendDocID)
        {
            InitializeComponent();
            operationType = ActionType;
            approvalid = SendDocID;
            this.Loaded += new RoutedEventHandler(ApprovalForm_aud_Loaded);
        }

        private void InitEvent()
        {
            OaPersonOfficeClient.AddApporvalTempletCompleted += _VM_AddApporvalTempletCompleted;
            OaPersonOfficeClient.Get_ApporvalTempletCompleted += new EventHandler<Get_ApporvalTempletCompletedEventArgs>(_VM_Get_ApporvalTempletCompleted);
            OaPersonOfficeClient.UpdateApporvalTempletCompleted += OaPersonOfficeClient_UpdateApporvalTempletCompleted;
            OaPersonOfficeClient.GetApprovalTypeByCompanyandDepartmentidCompleted += new EventHandler<GetApprovalTypeByCompanyandDepartmentidCompletedEventArgs>(_VM_GetApprovalTypeByCompanyandDepartmentidCompleted);
            personclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(personclient_GetEmployeePostBriefByEmployeeIDCompleted);
            publicClient.AddContentCompleted += new EventHandler<AddContentCompletedEventArgs>(publicClient_AddContentCompleted);
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);


            //psClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(psClient_GetEmployeeByIDCompleted);
        }




        #region 获取富文本框的内容


        void publicClient_GetContentCompleted(object sender, GetContentCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                txtContent.Document = e.Result;
            }
        }
        #endregion

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

        void personclient_GetEmployeePostBriefByEmployeeIDCompleted(object sender, GetEmployeePostBriefByEmployeeIDCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    string StrName = "";
                    if (e.Result != null)
                    {
                        //postLevel = e.Result.EMPLOYEEPOSTS[0].POSTLEVEL.ToString();
                        V_EMPLOYEEDETAIL employeepost = new V_EMPLOYEEDETAIL();
                        employeepost = e.Result;
                        //string PostName = "";
                        //string DepartmentName = "";
                        //string CompanyName = "";


                        if (Application.Current.Resources["SYS_PostInfo"] != null)
                        {
                            if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>) != null)
                            {
                                if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == approvalInfo.OWNERPOSTID) != null)
                                {
                                    if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault() != null)
                                    {
                                        if ((Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY != null)
                                        {
                                            StrPostName = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(c => c.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault().T_HR_POSTDICTIONARY.POSTNAME;
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
                                if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == approvalInfo.OWNERDEPARTMENTID) != null)
                                {
                                    if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == approvalInfo.OWNERDEPARTMENTID).FirstOrDefault() != null)
                                    {
                                        if ((Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == approvalInfo.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY != null)
                                        {
                                            StrDepartmentName = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(c => c.DEPARTMENTID == approvalInfo.OWNERDEPARTMENTID).FirstOrDefault().T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                                        }
                                    }
                                }
                            }
                        }

                        if (Application.Current.Resources["SYS_CompanyInfo"] != null)
                        {
                            if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>) != null)
                            {
                                if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == approvalInfo.OWNERCOMPANYID) != null)
                                {
                                    if ((Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == approvalInfo.OWNERCOMPANYID).FirstOrDefault() != null)
                                    {
                                        StrCompanyName = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(c => c.COMPANYID == approvalInfo.OWNERCOMPANYID).FirstOrDefault().CNAME;
                                    }
                                }
                            }
                        }


                        if (employeepost.EMPLOYEEPOSTS != null)
                        {
                            if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == approvalInfo.OWNERPOSTID) != null)
                            {
                                if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault() != null)
                                {
                                    if (employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault().POSTLEVEL != null)
                                    {
                                        postLevel = employeepost.EMPLOYEEPOSTS.Where(s => s.POSTID == approvalInfo.OWNERPOSTID).FirstOrDefault().POSTLEVEL.ToString();//获取申请人的岗位级别
                                    }
                                }
                            }
                        }
                        StrName = e.Result.EMPLOYEENAME + "-" + StrPostName + "-" + StrDepartmentName + "-" + StrCompanyName;


                    }
                    else
                    {
                        if (approvalInfo != null)
                        {
                            StrName = approvalInfo.OWNERNAME;
                        }
                    }
                    if (!string.IsNullOrEmpty(StrName))
                    {
                        txtOwnerName.Text = StrName;
                        ToolTipService.SetToolTip(txtOwnerName, StrName);
                    }
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log("事项审批中获取员工详细信息" + ex.Message, Category.Debug, Priority.Low);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "获取员工信息失败",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        private void InitFormType() 
        {
            if (operationType == FormTypes.New)
            {
                approvalInfo.APPROVALID = System.Guid.NewGuid().ToString();
                approvalInfo.CHECKSTATE = ((int)CheckStates.UnSubmit).ToString();
                approvalInfo.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                approvalInfo.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                approvalInfo.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
                approvalInfo.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                approvalInfo.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                //Utility.InitFileLoad("Approval", approvalInfo.APPROVALID, operationType, uploadFile);
                InitUserInfo();
                postLevel = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
                OaPersonOfficeClient.GetApprovalTypeByCompanyandDepartmentidAsync(OWNERCOMPANYID, OWNERDEPARTMENTID);
                //专用于获取电话号码
                psClient.GetEmployeeByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
              
            }
            else
            {

                OaPersonOfficeClient.Get_ApporvalTempletAsync(approvalid);

                if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
                {
                    SetControlsEnable();
                }
                //Utility.InitFileLoad("Approval", approvalid, operationType, uploadFile);
            }

        }

        private void InitUserInfo()
        {
            OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            string StrName = "";
            StrName = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            StrPostName = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            StrDepartmentName = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            StrCompanyName = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtOwnerName.Text = StrName;
            if (Common.CurrentLoginUserInfo.Telphone != null)
            {
                //txtTel.Text = Common.CurrentLoginUserInfo.Telphone;
            }
            ToolTipService.SetToolTip(txtOwnerName, StrName);
        }


        private void SetControlsEnable()
        {
            this.txtSelectPost.IsEnabled = false;
            this.txtOwnerName.IsReadOnly = true;
            //this.txtTel.IsReadOnly = true;
            this.txtTitle.IsReadOnly = true;
            //this.ckbHasFee.IsEnabled = false;
            //this.txtFee.IsReadOnly = true;
            //this.txtContent.IsDirty = true;
            this.txtContent.IsReadOnly = true;
            this.btnLookUpOwner.IsEnabled = false;

            //txtContent.HideControls();//隐藏富文本框的头部
            txtContent.BorderThickness = new Thickness(1.0);
            txtContent.BorderBrush = new SolidColorBrush(Colors.Gray);
        }
        #endregion

        #region 获取员工信息

        void personclient_GetEmployeeDetailByIDCompleted(object sender, GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    //postLevel = e.Result.EMPLOYEEPOSTS[0].POSTLEVEL.ToString();

                    string StrName = "";
                    var posts = from ent in e.Result.EMPLOYEEPOSTS
                                where ent.T_HR_POST.POSTID == approvalInfo.OWNERPOSTID
                                select ent;
                    if (posts != null)
                    {
                        if (posts.Count() > 0)
                        {
                            StrPostName = posts.FirstOrDefault().T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                            postLevel = posts.FirstOrDefault().POSTLEVEL.ToString();
                        }
                    }
                    //PostName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;
                    var departs = from ent in e.Result.EMPLOYEEPOSTS
                                  where ent.T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID == approvalInfo.OWNERDEPARTMENTID
                                  select ent;
                    if (departs != null)
                    {
                        if (departs.Count() > 0)
                            //DepartmentName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                            StrDepartmentName = departs.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    }
                    var companys = from ent in e.Result.EMPLOYEEPOSTS
                                   where ent.T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID == approvalInfo.OWNERCOMPANYID
                                   select ent;
                    if (companys != null)
                    {
                        if (companys.Count() > 0)
                            StrCompanyName = companys.FirstOrDefault().T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    }
                    //CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    StrName = e.Result.T_HR_EMPLOYEE.EMPLOYEECNAME + "-" + StrPostName + "-" + StrDepartmentName + "-" + StrCompanyName;
                    txtOwnerName.Text = StrName;
                    ToolTipService.SetToolTip(txtOwnerName, StrName);
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
            }
        }
        #endregion

        #region 获取公司或部门的事项审批类型


        void _VM_GetApprovalTypeByCompanyandDepartmentidCompleted(object sender, GetApprovalTypeByCompanyandDepartmentidCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                lstApprovalids.Clear();
                lstApprovalids = e.Result;
                //lstApprovalids = e.Result;
                if (operationType == FormTypes.New)
                {
                    List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

                    if (Dicts == null)
                        return;

                    T_SYS_DICTIONARY DictApproval = new T_SYS_DICTIONARY();

                    var ents = from p in Dicts
                               where p.DICTIONCATEGORY == "TYPEAPPROVAL" && p.T_SYS_DICTIONARY2 != null && lstApprovalids.Contains(p.DICTIONARYVALUE.ToString())
                               orderby p.ORDERNUMBER
                               select p;
                    //txtSelectPost
                    if (ents.Count() > 0)
                    {
                    }
                    else
                    {
                    }
                    StrApprovaltype = "";
                    txtSelectPost.TxtSelectedApprovalType.Text = "";
                    if (DictApproval != null)
                    {
                        if (!string.IsNullOrEmpty(DictApproval.DICTIONARYID))//存在则赋值
                        {
                            txtSelectPost.TxtSelectedApprovalType.Text = DictApproval.DICTIONARYNAME;
                            StrApprovaltype = DictApproval.DICTIONARYVALUE.ToString();
                            GetFatherApprovalType(StrApprovaltype, "first");
                        }
                    }
                }
            }
        }

        #endregion


        #region FormLoaded事件

        void ApprovalForm_aud_Loaded(object sender, RoutedEventArgs e)
        {
            InitEvent();

            if (!string.IsNullOrEmpty(approvalid))
            {
                publicClient.GetContentAsync(approvalid);
            }
            InitFormType();

            //先取消提交方式，订阅新的提交保存功能 luojie
        }
        #endregion


        #region 添加删除修改

        #region 保存
        private void Save()
        {
            if (operationType == FormTypes.New)
            {
                if (AddApporval())
                {
                    approvalInfo.APPROVALTITLE = approvalInfo.APPROVALTITLE.Replace('"', '“');
                    string ApprovalCode = "";
                    RefreshUI(RefreshedTypes.ShowProgressBar);

                    approvalInfo.TYPEAPPROVAL = approvalInfo.TYPEAPPROVAL.Replace(",", "");

                    OaPersonOfficeClient.AddApporvalTempletAsync(approvalInfo, ApprovalCode);
                }
            }
            if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)  //修改报批件
            {
                if (DataValidation())
                {
                    approvalInfo.APPROVALTITLE = txtTitle.Text;
                    StrApprovaltype = StrApprovaltype.Replace(",", "");
                    StrApprovalOne = StrApprovalOne.Replace(",", "");
                    StrApprovalTwo = StrApprovalTwo.Replace(",", "");
                    StrApprovalThird = StrApprovalThird.Replace(",", "");
                    approvalInfo.TYPEAPPROVAL = StrApprovaltype;
                    approvalInfo.TYPEAPPROVALONE = StrApprovalOne;//第1个父亲字典值
                    approvalInfo.TYPEAPPROVALTWO = StrApprovalTwo;//第2个父亲字典值
                    approvalInfo.TYPEAPPROVALTHREE = StrApprovalThird; //第3个父亲字典值

                    approvalInfo.ISCHARGE = "0";
                    //approvalInfo.CHARGEMONEY = Convert.ToDecimal(txtFee.Text);

                    //approvalInfo.CONTENT = txtContent.RichTextBoxContext;
                    approvalInfo.CONTENT = txtContent.Document;
                    approvalInfo.UPDATEDATE = System.DateTime.Now;
                    approvalInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    approvalInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    OaPersonOfficeClient.UpdateApporvalTempletAsync(approvalInfo, "Edit");
                }
            }
            //UpdateApporval();
        }

        private bool AddApporval()
        {
            bool IsResult = true;
            if (DataValidation())
            {

                string sbAppCode = "";
                sbAppCode = "BPJ" + string.Format("{0:yyyyMMdd}", System.DateTime.Now);
                //approvalInfo.ApprovalCode = sbAppCode.ToString();
                approvalInfo.APPROVALTITLE = txtTitle.Text;
                approvalInfo.TYPEAPPROVAL = StrApprovaltype;//事项审批类型
                approvalInfo.TYPEAPPROVALONE = StrApprovalOne;//第1个父亲字典值
                approvalInfo.TYPEAPPROVALTWO = StrApprovalTwo;//第2个父亲字典值
                approvalInfo.TYPEAPPROVALTHREE = StrApprovalThird; //第3个父亲字典值

                //approvalInfo.CONTENT = txtContent.RichTextBoxContext;

                approvalInfo.CONTENT = txtContent.Document;

                approvalInfo.TEL = " ";

                approvalInfo.ISCHARGE = "0";
                //if (Convert.ToDecimal(txtFee.Text) > 0)
                //{
                //    approvalInfo.ISCHARGE = "1";
                //}
                //else
                //{
                //    approvalInfo.ISCHARGE = "0";
                //}
                approvalInfo.ISCHARGE = "0";
                approvalInfo.CHARGEMONEY = 0;

                approvalInfo.CREATEDATE = System.DateTime.Now;
                approvalInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                approvalInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                approvalInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                approvalInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                approvalInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                approvalInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                approvalInfo.UPDATEDATE = System.DateTime.Now;
                approvalInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            }
            else
            {
                IsResult = false;
            }
            return IsResult;
        }

        private bool DataValidation()
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "APPROVALTITLE"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "事项审批标题不能为空！",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (txtContent.Document.Count() == 0)
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ApprovalCONTENT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ApprovalCONTENT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (string.IsNullOrEmpty(StrApprovaltype))
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "TEL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "TYPEAPPROVAL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }

            return true;
        }


        private void UpdateApporval()
        {
            OaPersonOfficeClient.UpdateApporvalTempletAsync(approvalInfo);
        }

        void _VM_Get_ApporvalTempletCompleted(object sender, Get_ApporvalTempletCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                approvalInfo = e.Result;
                if (approvalInfo.CHECKSTATE != "0")
                {
                    if (!(approvalInfo.CHECKSTATE == "3" && operationType == FormTypes.Resubmit))
                    {
                        SetControlsEnable();
                    }
                }
                if (approvalInfo.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || approvalInfo.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                {
                    RefreshUI(RefreshedTypes.All);
                }
                txtTitle.Text = approvalInfo.APPROVALTITLE;
                //txtCode.Text = approvalInfo.ApprovalCode;

                //txtTel.Text = string.IsNullOrEmpty(approvalInfo.Tel) ? "" : approvalInfo.Tel;
                //ckbHasFee.IsChecked = approvalInfo.ISCHARGE == "1" ? true : false;

                OaPersonOfficeClient.GetApprovalTypeByCompanyandDepartmentidAsync(approvalInfo.OWNERCOMPANYID, approvalInfo.OWNERDEPARTMENTID);

                if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)
                {
                    OWNERCOMPANYID = approvalInfo.OWNERCOMPANYID;
                    OWNERDEPARTMENTID = approvalInfo.OWNERDEPARTMENTID;
                    OWNERPOSTID = approvalInfo.OWNERPOSTID;
                }
                //txtFee.Text = approvalInfo.CHARGEMONEY.ToString();
                //if (ckbHasFee.IsChecked == true)
                //{
                //    //fbCtr.Visibility = Visibility.Visible;
                //}
                if (Application.Current.Resources["SYS_DICTIONARY"] != null)
                {
                    var ents = from a in Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>
                               where a.DICTIONCATEGORY == "TYPEAPPROVAL" && a.DICTIONARYVALUE == System.Convert.ToInt32(approvalInfo.TYPEAPPROVAL)
                               select a;
                    if (ents.Count() > 0)
                    {
                        StrApprovalTypeName = ents.FirstOrDefault().DICTIONARYNAME;//事项审批名称
                        txtSelectPost.TxtSelectedApprovalType.Text = StrApprovalTypeName;
                        StrApprovaltype = approvalInfo.TYPEAPPROVAL;
                    }

                }
                StrApprovalOne = string.IsNullOrEmpty(approvalInfo.TYPEAPPROVALONE) ? "" : approvalInfo.TYPEAPPROVALONE;
                StrApprovalTwo = string.IsNullOrEmpty(approvalInfo.TYPEAPPROVALTWO) ? "" : approvalInfo.TYPEAPPROVALTWO;
                StrApprovalThird = string.IsNullOrEmpty(approvalInfo.TYPEAPPROVALTHREE) ? "" : approvalInfo.TYPEAPPROVALTHREE;

                if (operationType == FormTypes.Resubmit)//重新提交
                {
                    approvalInfo.CHECKSTATE = (Convert.ToInt32(CheckStates.UnSubmit)).ToString();
                }


                if (operationType == FormTypes.Browse || operationType == FormTypes.Audit)
                {
                    if (approvalInfo.ISCHARGE == "0")
                    {

                        if (approvalInfo.CHARGEMONEY == 0)
                        {
                            SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 5);

                        }
                        SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 6);
                    }
                    else
                    {
                        //fbCtr.Visibility = Visibility.Visible;
                    }
                }
                depName = Utility.GetDepartmentName(approvalInfo.OWNERDEPARTMENTID);//所属部门ID
                personclient.GetEmployeePostBriefByEmployeeIDAsync(approvalInfo.OWNERID);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, "提示","该单据不存在或已被删除，请联系管理员");
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "该单据不存在或已被删除，请联系管理员",
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        private void Close()
        {
            RefreshUI(RefreshedTypes.Close);
        }
        #endregion

        void OaPersonOfficeClient_UpdateApporvalTempletCompleted(object sender, UpdateApporvalTempletCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {


                if (e.Error != null && e.Error.Message != "")
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }

                if (e.UserState.ToString() == "Edit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "MATTERSAPPROVAL"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else if (e.UserState.ToString() == "Submit")
                {

                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                canSubmit = true;
                if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)
                {
                    UserInfo User = new UserInfo();
                    User.COMPANYID = approvalInfo.OWNERCOMPANYID;
                    User.DEPARTMENTID = approvalInfo.OWNERDEPARTMENTID;
                    User.POSTID = approvalInfo.OWNERPOSTID;
                    User.USERID = approvalInfo.OWNERID;
                    User.USERNAME = approvalInfo.OWNERNAME;
                    //publicClient.UpdateContentAsync(approvalInfo.APPROVALID, approvalInfo.CONTENT, approvalInfo.OWNERCOMPANYID, "OA", "T_OA_APPROVAL");               
                    publicClient.UpdateContentAsync(approvalInfo.APPROVALID, approvalInfo.CONTENT, User);
                    //ctrFile.FormID = approvalInfo.APPROVALID;
                    //ctrFile.Save();
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(saveType);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "修改失败",
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }

        void _VM_AddApporvalTempletCompleted(object sender, AddApporvalTempletCompletedEventArgs e)
        {
            //提交保存为完成
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Result > 0)
            {
                if (e.Error != null && e.Error.Message != "")
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERROR"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
                string xml = "";
                UserInfo User = new UserInfo();
                User.COMPANYID = approvalInfo.OWNERCOMPANYID;
                User.DEPARTMENTID = approvalInfo.OWNERDEPARTMENTID;
                User.POSTID = approvalInfo.OWNERPOSTID;
                User.USERID = approvalInfo.OWNERID;
                User.USERNAME = approvalInfo.OWNERNAME;
                publicClient.AddContentAsync(approvalInfo.APPROVALID, approvalInfo.CONTENT, approvalInfo.OWNERCOMPANYID, "OA", "T_OA_APPROVAL", User);

                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                //txtCode.Text = e.ApprovalCode;
                //approvalInfo.app = e.ApprovalCode;
                operationType = FormTypes.Edit;
                canSubmit = true;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(saveType);

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), "保存事项审批模板成功",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;

            }
            else
            {

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SAVEFAILED", "MATTERSAPPROVAL"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
        }
        #endregion

        #region IEntityEditor

        public string GetTitle()
        {
            string StrReturn = "";
            if (operationType == FormTypes.New)
            {
                StrReturn = Utility.GetResourceStr("ADDTITLE", "MATTERSAPPROVAL");
            }
            if (operationType == FormTypes.Edit)
            {
                StrReturn = Utility.GetResourceStr("EDITTITLE", "MATTERSAPPROVAL");
            }
            if (operationType == FormTypes.Browse)
            {
                StrReturn = Utility.GetResourceStr("VIEWTITLE", "MATTERSAPPROVAL");
            }
            if (operationType == FormTypes.Audit)
            {
                StrReturn = Utility.GetResourceStr("AUDIT", "MATTERSAPPROVAL");
            }
            return StrReturn;
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
                    saveType = RefreshedTypes.All;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    Save();
                    break;
                case "1":
                    saveType = RefreshedTypes.CloseAndReloadData;
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnSubmit;
                    Save();
                    break;
            }
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("InfoDetail"),
                Tooltip = Utility.GetResourceStr("InfoDetail")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();

            if (operationType != FormTypes.Browse && operationType != FormTypes.Audit)
            {

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
        #endregion

        #region 选择申请人


        private void btnLookUpOwner_Click(object sender, RoutedEventArgs e)
        {
            SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup lookup = new SMT.SaaS.FrameworkUI.OrganizationControl.OrganizationLookup();
            lookup.SelectedObjType = SMT.SaaS.FrameworkUI.OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj> ent = lookup.SelectedObj as List<SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj>;
                if (ent != null && ent.Count > 0)
                {
                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj userInfo = ent.FirstOrDefault();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj post = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)userInfo.ParentObject;
                    string postid = post.ObjectID;
                    string postName = post.ObjectName;//岗位
                    StrPostName = postName;
                    postLevel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().POSTLEVEL.ToString();

                    SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj dept = (SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;
                    string deptName = dept.ObjectName;//部门
                    depName = dept.ObjectName;//部门
                    StrDepartmentName = depName;

                    OWNERDEPARTMENTID = deptid;

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    StrCompanyName = corpName;
                    OWNERCOMPANYID = corpid;

                    OWNERID = userInfo.ObjectID;
                    approvalInfo.OWNERCOMPANYID = corpid;
                    approvalInfo.OWNERDEPARTMENTID = deptid;
                    approvalInfo.OWNERID = userInfo.ObjectID;
                    approvalInfo.OWNERNAME = userInfo.ObjectName;
                    approvalInfo.OWNERPOSTID = postid;
                    //txtOwnerName.Text = userInfo.ObjectName;
                    string Mobile = "";
                    string Tel = "";
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE != null)
                        Mobile = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).MOBILE.ToString();
                    if ((ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE != null)
                        Tel = (ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE).OFFICEPHONE.ToString();
                    string StrEmployee = userInfo.ObjectName + "-" + post.ObjectName + "-" + dept.ObjectName + "-" + corp.CNAME;
                    txtOwnerName.Text = StrEmployee;
                    StrOwnerName = StrEmployee;
                    //txtTel.Text = userInfo.te
                    ToolTipService.SetToolTip(txtOwnerName, StrEmployee);
                    //txtTel.Text = string.Empty;
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        //txtTel.Text = Mobile;
                    }
                    if (!string.IsNullOrEmpty(Tel))
                    {
                        //if (string.IsNullOrEmpty(txtTel.Text.ToString()))
                        //{
                        //    txtTel.Text = Tel;
                        //}
                        //else
                        //{
                        //    txtTel.Text += ";" + Tel;
                        //}
                    }
                    //PersonnelServiceClient psClient = new PersonnelServiceClient();
                    psClient.GetEmployeeByIDAsync(userInfo.ObjectID);
                    //psClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(psClient_GetEmployeeByIDCompleted);
                    OaPersonOfficeClient.GetApprovalTypeByCompanyandDepartmentidAsync(OWNERCOMPANYID, OWNERDEPARTMENTID);
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }

        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            OaPersonOfficeClient.DoClose();
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

        #region 选择事项审核类型

        private void txtSelectApprovalType_SelectClick(object sender, EventArgs e)
        {
            SelectApprovalType txt = (SelectApprovalType)sender;
            string StrOld = txt.TxtSelectedApprovalType.Text.ToString();
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            //parameters.Add("CHARGEMONEY", fbCtr.Order.TOTALMONEY.ToString());
            parameters.Add("CHARGEMONEY", approvalInfo.CHARGEMONEY.ToString());
            parameters.Add("POSTLEVEL", postLevel);
            parameters.Add("DEPARTMENTNAME", depName);
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_APPROVALINFOTEMPLET>(approvalInfo, "OA", parameters);
            ApprovalTypeList apptype = new ApprovalTypeList(StrOld, StrApprovaltype, lstApprovalids, OWNERCOMPANYID, OWNERDEPARTMENTID, strXmlObjectSource);

            apptype.SelectedClicked += (obj, ea) =>
            {
                StrApprovaltype = "";
                string StrPost = apptype.Result.Keys.FirstOrDefault();
                if (!string.IsNullOrEmpty(StrPost))
                {
                    txt.TxtSelectedApprovalType.Text = StrPost;
                    StrApprovalTypeName = StrPost;//用于传递给手机
                }
                StrApprovaltype = apptype.Result[apptype.Result.Keys.FirstOrDefault()].ToString();
                //根据选择回来的审批类型获取父值
                //将父级的值清为空
                StrApprovalOne = "";
                StrApprovalTwo = "";
                StrApprovalThird = "";
                GetFatherApprovalType(StrApprovaltype, "first");

            };
            var windows = SMT.SAAS.Controls.Toolkit.Windows.ProgramManager.ShowProgram(Utility.GetResourceStr("SELECTAPPROVALTYPE"), "", "123", apptype, false, false, null);
            if (apptype is ApprovalTypeList)
            {
                (apptype as ApprovalTypeList).Close += (o, args) =>
                {
                    windows.Close();
                };
            }
        }
        /// <summary>
        /// 获取 选取的事项审批的类型 父级的 字典值
        /// </summary>
        /// <param name="apptype"></param>
        /// <param name="forcount"></param>
        private void GetFatherApprovalType(string apptype, string forcount)
        {
            //获取缓存--字典值
            try
            {
                List<T_SYS_DICTIONARY> Dicts = Application.Current.Resources["SYS_DICTIONARY"] as List<T_SYS_DICTIONARY>;

                if (Dicts == null)
                {
                    return;
                }
                List<T_SYS_DICTIONARY> TopApproval = new List<T_SYS_DICTIONARY>();
                //获取  事项审批类型的字典集合
                var ents = from p in Dicts
                           where p.DICTIONCATEGORY == "TYPEAPPROVAL" && p.DICTIONARYVALUE == System.Convert.ToInt16(apptype)
                           orderby p.ORDERNUMBER
                           select p;
                T_SYS_DICTIONARY dict = new T_SYS_DICTIONARY();
                if (ents.Count() > 0)
                {
                    dict = ents.FirstOrDefault();
                    //获取父值的信息
                    if (dict.T_SYS_DICTIONARY2 != null)
                    {
                        var firstents = from ent in Dicts
                                        where ent.DICTIONCATEGORY == "TYPEAPPROVAL" && (ent.DICTIONARYID == dict.T_SYS_DICTIONARY2.DICTIONARYID && dict.T_SYS_DICTIONARY2 != null)
                                        orderby ent.ORDERNUMBER
                                        select ent;
                        if (firstents.Count() > 0)
                        {
                            if (forcount == "first")
                            {
                                StrApprovalOne = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalOne, "second");
                            }
                            if (forcount == "second")
                            {
                                StrApprovalTwo = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalTwo, "second");
                            }
                            if (forcount == "third")
                            {
                                StrApprovalThird = firstents.FirstOrDefault().DICTIONARYVALUE.ToString();
                                GetFatherApprovalType(StrApprovalThird, "third");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Log("获取事项审批类型" + ex.Message, Category.Debug, Priority.Low);
                throw (ex);
            }
        }

        #endregion
        
    }
}
