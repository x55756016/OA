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
    public partial class ApprovalForm_aud : BaseForm, IClient, IEntityEditor, IAudit//, IFileLoadedCompleted
    {
        #region 页面变量
        SmtOAPersonOfficeClient _VM = new SmtOAPersonOfficeClient();
        PublicServiceClient publicClient = new PublicServiceClient();
        PersonnelServiceClient psClient = new PersonnelServiceClient();
        /// <summary>
        /// 流程返回结果状态,便于审核费用时调用 
        /// </summary>
        SMT.SaaS.FrameworkUI.CheckStates _flwResult;
        private T_OA_APPROVALINFO approvalInfo = new T_OA_APPROVALINFO();
        public T_OA_APPROVALINFO ApprovalInfo { get { return approvalInfo; } set { approvalInfo = value; } }
        private FormTypes operationType;
        public string postLevel = string.Empty;
        public string depName = string.Empty;
        string approvalid = "";
        string StrApprovaltype = "";
        string StrApprovalOne = "";//事项审批中第一个父节点的  值
        string StrApprovalTwo = "";//事项审批中第二个父节点的 值
        string StrApprovalThird = "";//事项审批中第三个父节点 值

        string OwnerCompanyid = "";//所属公司
        string OwnerDepartmentid = "";//获取事项审批时用 用的部门ID
        string Ownerid = "";//所属员工
        string OwnerPostid = "";//所属岗位

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
        public ApprovalForm_aud()
        {
            InitializeComponent();
            operationType = FormTypes.New;
            approvalid = "";   
            this.Loaded += new RoutedEventHandler(ApprovalForm_aud_Loaded);
        }

        public ApprovalForm_aud(FormTypes ActionType, string SendDocID)
        {
            InitializeComponent();

            operationType = ActionType;
            approvalid = SendDocID;
            ////fbCtr.SetRemarkVisiblity(Visibility.Collapsed);//隐藏预算控件中的备注
            this.Loaded += new RoutedEventHandler(ApprovalForm_aud_Loaded);
        }

        private void InitEvent()
        {
            _VM.AddApporvalCompleted += new EventHandler<AddApporvalCompletedEventArgs>(AddApporvalCompleted);
            _VM.UpdateApporvalCompleted += new EventHandler<UpdateApporvalCompletedEventArgs>(UpdateApporvalCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            //ctrFile.Event_AllFilesFinished += new EventHandler<FileCountEventArgs>(ctrFile_Event_AllFilesFinished);

            _VM.Get_ApporvalCompleted += new EventHandler<Get_ApporvalCompletedEventArgs>(Get_ApporvalCompleted);
            fbCtr.SaveCompleted += new EventHandler<SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs>(fbCtr_SaveCompleted);
            _VM.GetApprovalTypeByCompanyandDepartmentidCompleted += new EventHandler<GetApprovalTypeByCompanyandDepartmentidCompletedEventArgs>(_VM_GetApprovalTypeByCompanyandDepartmentidCompleted);
            //personclient.GetEmployeeDetailByIDCompleted += new EventHandler<GetEmployeeDetailByIDCompletedEventArgs>(personclient_GetEmployeeDetailByIDCompleted);
            personclient.GetEmployeePostBriefByEmployeeIDCompleted += new EventHandler<GetEmployeePostBriefByEmployeeIDCompletedEventArgs>(personclient_GetEmployeePostBriefByEmployeeIDCompleted);
            publicClient.AddContentCompleted += new EventHandler<AddContentCompletedEventArgs>(publicClient_AddContentCompleted);
            publicClient.GetContentCompleted += new EventHandler<GetContentCompletedEventArgs>(publicClient_GetContentCompleted);

            psClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(psClient_GetEmployeeByIDCompleted);

            //_VM.Get_ApporvalTempletCompleted += new EventHandler<Get_ApporvalTempletCompletedEventArgs>(_VM_Get_ApporvalTempletCompleted);
            _VM.Get_ApporvalTempletByApporvalTypeCompleted += new EventHandler<Get_ApporvalTempletByApporvalTypeCompletedEventArgs>(_VM_Get_ApporvalTempletByApporvalTypeCompleted);
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


                        //CompanyName = e.Result.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
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
                Utility.InitFileLoad("Approval", approvalInfo.APPROVALID, operationType, uploadFile);
                InitUserInfo();
                postLevel = Common.CurrentLoginUserInfo.UserPosts[0].PostLevel.ToString();
                _VM.GetApprovalTypeByCompanyandDepartmentidAsync(OwnerCompanyid, OwnerDepartmentid);
                //专用于获取电话号码
                psClient.GetEmployeeByIDAsync(Common.CurrentLoginUserInfo.EmployeeID);
                //InitFBControl();
            }
            else
            {

                _VM.Get_ApporvalAsync(approvalid);

                if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
                {
                    SetControlsEnable();
                }
                Utility.InitFileLoad("Approval", approvalid, operationType, uploadFile);
            }

        }

        private void InitUserInfo()
        {
            OwnerCompanyid = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            OwnerDepartmentid = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            string StrName = "";
            StrName = Common.CurrentLoginUserInfo.EmployeeName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].PostName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName + "-" + Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            StrPostName = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            StrDepartmentName = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            StrCompanyName = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            txtOwnerName.Text = StrName;
            if (Common.CurrentLoginUserInfo.Telphone != null)
            {
                txtTel.Text = Common.CurrentLoginUserInfo.Telphone;
            }
            ToolTipService.SetToolTip(txtOwnerName, StrName);
        }


        private void SetControlsEnable()
        {
            this.txtSelectPost.IsEnabled = false;
            this.txtOwnerName.IsReadOnly = true;
            this.txtTel.IsReadOnly = true;
            this.txtTitle.IsReadOnly = true;
            this.ckbHasFee.IsEnabled = false;
            this.txtFee.IsReadOnly = true;
            //this.txtContent.IsDirty = true;
            this.txtContent.IsReadOnly = true;
            this.btnLookUpOwner.IsEnabled = false;

            //txtContent.HideControls();//隐藏富文本框的头部
            txtContent.BorderThickness = new Thickness(1.0);
            txtContent.BorderBrush = new SolidColorBrush(Colors.Gray);
        }
        #endregion

        #region 添加完成事件
        //添加完成
        void AddApporvalCompleted(object sender, AddApporvalCompletedEventArgs e)
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

                //暂时不考虑向预算中添加费用
                //if (ckbHasFee.IsChecked == true)
                //{
                //    if (approvalInfo.CHARGEMONEY > 0)
                //    {
                //        fbCtr.Order.ORDERID = approvalInfo.APPROVALID;
                //        fbCtr.Save(SMT.SaaS.FrameworkUI.CheckStates.UnSubmit);//提交费用
                //    }
                //}
                UserInfo User = new UserInfo();
                User.COMPANYID = approvalInfo.OWNERCOMPANYID;
                User.DEPARTMENTID = approvalInfo.OWNERDEPARTMENTID;
                User.POSTID = approvalInfo.OWNERPOSTID;
                User.USERID = approvalInfo.OWNERID;
                User.USERNAME = approvalInfo.OWNERNAME;
                publicClient.AddContentAsync(approvalInfo.APPROVALID, approvalInfo.CONTENT, approvalInfo.OWNERCOMPANYID, "OA", "T_OA_APPROVAL", User);
                
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.FormType = FormTypes.Edit;
                txtCode.Text = e.ApprovalCode;
                approvalInfo.APPROVALCODE = e.ApprovalCode;
                operationType = FormTypes.Edit;
                canSubmit = true;
                RefreshUI(RefreshedTypes.AuditInfo);
                RefreshUI(saveType);

                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "MATTERSAPPROVAL"),
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

        #region 初始化预算控件

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
                        //DictApproval = ents.ToList().FirstOrDefault();
                        //StrApprovalTypeName = DictApproval.DICTIONARYNAME;
                    }
                    else
                    {
                        //var entsfather = from p in Dicts
                        //           where p.DICTIONCATEGORY == "TYPEAPPROVAL" &&  lstApprovalids.Contains(p.DICTIONARYVALUE.ToString())
                        //           orderby p.ORDERNUMBER
                        //           select p;
                        //if (entsfather.Count() > 0)
                        //{
                        //    DictApproval = entsfather.ToList().FirstOrDefault();
                        //    if (DictApproval != null)
                        //    {
                        //        StrApprovalTypeName = DictApproval.DICTIONARYNAME;
                        //    }
                        //}
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

        #region 文件加载完事件


        //void ctrFile_Event_AllFilesFinished(object sender, FileCountEventArgs e)
        //{
        //    //_VM.Get_ApporvalAsync(approvalid);
        //}
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

        #region Grid加载事件

        //窗口加载事件 
        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //txtSelectPost.IsEnabled = false;
        }
        #endregion

        #region 初始化预算控件

        private void InitFBControl()
        {
            fbCtr.ApplyType = FrameworkUI.FBControls.ChargeApplyControl.ApplyTypes.BorrowApply;//借款选择
            if (operationType == FormTypes.New)
            {
                fbCtr.Order.ORDERID = "";
                //fbCtr.strExtOrderModelCode = "SXSP";
            }
            else
            {
                fbCtr.Order.ORDERID = approvalInfo.APPROVALID;//费用对象
            }
            fbCtr.strExtOrderModelCode = "SXSP";


            fbCtr.Order.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.CREATECOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.CREATEDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.CREATEPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.Order.OWNERCOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
            fbCtr.Order.OWNERCOMPANYNAME = Common.CurrentLoginUserInfo.UserPosts[0].CompanyName;
            fbCtr.Order.OWNERDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
            fbCtr.Order.OWNERDEPARTMENTNAME = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentName;
            fbCtr.Order.OWNERPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
            fbCtr.Order.OWNERPOSTNAME = Common.CurrentLoginUserInfo.UserPosts[0].PostName;
            fbCtr.Order.OWNERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.OWNERNAME = Common.CurrentLoginUserInfo.EmployeeName;


            fbCtr.Order.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
            fbCtr.Order.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

            fbCtr.InitDataComplete += (o, e) =>
            {
                Binding bding = new Binding();
                bding.Path = new PropertyPath("TOTALMONEY");
                this.txtFee.SetBinding(TextBox.TextProperty, bding);
                this.txtFee.DataContext = fbCtr.Order;
            };
            if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
            {
                fbCtr.InitData(false);
            }
            else
            {
                fbCtr.InitData();
            }


        }

        #endregion

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
                    
                    approvalInfo.TYPEAPPROVAL = approvalInfo.TYPEAPPROVAL.Replace(",","");                    
                    
                    _VM.AddApporvalAsync(approvalInfo, ApprovalCode);
                }
            }
            if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)  //修改报批件
            {
                if (DataValidation())
                {
                    approvalInfo.APPROVALTITLE = txtTitle.Text;
                    StrApprovaltype = StrApprovaltype.Replace(",","");
                    StrApprovalOne = StrApprovalOne.Replace(",", "");
                    StrApprovalTwo = StrApprovalTwo.Replace(",", "");
                    StrApprovalThird = StrApprovalThird.Replace(",", "");
                    approvalInfo.TYPEAPPROVAL = StrApprovaltype;
                    approvalInfo.TYPEAPPROVALONE = StrApprovalOne;//第1个父亲字典值
                    approvalInfo.TYPEAPPROVALTWO = StrApprovalTwo;//第2个父亲字典值
                    approvalInfo.TYPEAPPROVALTHREE = StrApprovalThird; //第3个父亲字典值
                    approvalInfo.TEL = txtTel.Text;
                    //approvalInfo.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
                    if (Convert.ToDecimal(txtFee.Text) > 0)
                    {
                        approvalInfo.ISCHARGE = "1";
                    }
                    else
                    {
                        approvalInfo.ISCHARGE = "0";
                    }
                    approvalInfo.CHARGEMONEY = Convert.ToDecimal(txtFee.Text);

                    //approvalInfo.CONTENT = txtContent.RichTextBoxContext;
                    approvalInfo.CONTENT = txtContent.Document;
                    approvalInfo.UPDATEDATE = System.DateTime.Now;
                    approvalInfo.UPDATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                    approvalInfo.UPDATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    _VM.UpdateApporvalAsync(approvalInfo, "Edit");
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
                approvalInfo.APPROVALCODE = sbAppCode.ToString();
                approvalInfo.APPROVALTITLE = txtTitle.Text;
                approvalInfo.TYPEAPPROVAL = StrApprovaltype;//事项审批类型
                approvalInfo.TYPEAPPROVALONE = StrApprovalOne;//第1个父亲字典值
                approvalInfo.TYPEAPPROVALTWO = StrApprovalTwo;//第2个父亲字典值
                approvalInfo.TYPEAPPROVALTHREE = StrApprovalThird; //第3个父亲字典值

                //approvalInfo.CONTENT = txtContent.RichTextBoxContext;

                approvalInfo.CONTENT = txtContent.Document;

                approvalInfo.TEL = txtTel.Text;

                //approvalInfo.ISCHARGE = ckbHasFee.IsChecked == true ? "1" : "0";
                if (Convert.ToDecimal(txtFee.Text) > 0)
                {
                    approvalInfo.ISCHARGE = "1";
                }
                else
                {
                    approvalInfo.ISCHARGE = "0";
                }
                approvalInfo.CHARGEMONEY = Convert.ToDecimal(txtFee.Text);

                approvalInfo.CREATEDATE = System.DateTime.Now;
                approvalInfo.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                approvalInfo.CREATECOMPANYID = Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                approvalInfo.CREATEDEPARTMENTID = Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                approvalInfo.CREATEPOSTID = Common.CurrentLoginUserInfo.UserPosts[0].PostID;
                approvalInfo.CREATEUSERNAME = Common.CurrentLoginUserInfo.EmployeeName;

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
            try
            {
                if (txtContent.Document.Count() == 0)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ApprovalCONTENT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "ApprovalCONTENT"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("事项审批内容错误，请重新填写后再试。");
            }
            if (string.IsNullOrEmpty(txtTel.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "TEL"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REQUIRED", "TEL"),
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
            if (string.IsNullOrEmpty(txtFee.Text))
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "费用不能为空，没有默认为0",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return false;
            }
            if (!string.IsNullOrEmpty(txtFee.Text))
            {
                if (!Utility.IsInt(txtFee.Text))
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), "费用必须为数字",
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return false;
                }
            }

            return true;
        }


        private void UpdateApporval()
        {
            _VM.UpdateApporvalAsync(approvalInfo);
        }
        void UpdateApporvalCompleted(object sender, UpdateApporvalCompletedEventArgs e)
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
                //暂时去掉该功能
                //if(approvalInfo.ISCHARGE=="1")
                //{
                //    if (approvalInfo.CHARGEMONEY > 0)
                //    {
                //        fbCtr.Order.ORDERID = approvalInfo.APPROVALID;
                //        fbCtr.Save(_flwResult);//提交费用
                //    }
                //}
                //审核时不更新上传文件

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
        void fbCtr_SaveCompleted(object sender, SMT.SaaS.FrameworkUI.FBControls.ChargeApplyControl.SaveCompletedArgs e)
        {

            if (e.Message != null && e.Message.Count() > 0)
            {
                //Utility.ShowMessageBox("AUDITFAILURE", true, false);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FAILURETOAPPROVE"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.CloseAndReloadData);
            }
        }
        void Get_ApporvalCompleted(object sender, Get_ApporvalCompletedEventArgs e)
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

                //txtContent.Document = approvalInfo.CONTENT;
                //txtContent.RichTextBoxContext = approvalInfo.CONTENT;
                txtTitle.Text = approvalInfo.APPROVALTITLE;
                txtCode.Text = approvalInfo.APPROVALCODE;

                txtTel.Text = string.IsNullOrEmpty(approvalInfo.TEL) ? "" : approvalInfo.TEL;
                ckbHasFee.IsChecked = approvalInfo.ISCHARGE == "1" ? true : false;

                _VM.GetApprovalTypeByCompanyandDepartmentidAsync(approvalInfo.OWNERCOMPANYID, approvalInfo.OWNERDEPARTMENTID);
                //if (txtFee.Visibility == Visibility.Visible)
                //{
                //    txtFee.Text = approvalInfo.CHARGEMONEY.ToString();
                //}
                //publicClient.GetContentAsync(approvalInfo.APPROVALID, approvalInfo.CONTENT, approvalInfo.OWNERCOMPANYID, "OA", "T_OA_APPROVAL", User);
                //publicClient.GetContentAsync(approvalInfo.APPROVALID);
                if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)
                {
                    OwnerCompanyid = approvalInfo.OWNERCOMPANYID;
                    OwnerDepartmentid = approvalInfo.OWNERDEPARTMENTID;
                    OwnerPostid = approvalInfo.OWNERPOSTID;
                }
                txtFee.Text = approvalInfo.CHARGEMONEY.ToString();
                if (ckbHasFee.IsChecked == true)
                {
                    fbCtr.Visibility = Visibility.Visible;
                }
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
                        fbCtr.Visibility = Visibility.Visible;
                    }
                    //if (!ctrFile._files.HasAccessory)
                    //{
                    //    SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 4);
                    //}
                }
                //InitFBControl();

                depName = Utility.GetDepartmentName(approvalInfo.OWNERDEPARTMENTID);//所属部门ID
                //string companyName = Utility.GetCompanyName(approvalInfo.OWNERCOMPANYID);
                //string postName = Utility.GetPostName(approvalInfo.OWNERPOSTID);
                //string StrName = approvalInfo.OWNERNAME + "-" + postName + "-" + depName + "-" + companyName;
                //txtOwnerName.Text = StrName;
                //ToolTipService.SetToolTip(txtOwnerName, StrName);
                ////_VM.GetApprovalTypeByCompanyandDepartmentidAsync(OwnerCompanyid, OwnerDepartmentid);
                //if (operationType == FormTypes.Edit)
                //{
                //    RefreshUI(RefreshedTypes.All);
                //}
                //else
                //{
                //    RefreshUI(RefreshedTypes.AuditInfo);
                //}
                //personclient.GetEmployeeDetailByIDAsync(approvalInfo.OWNERID);

                personclient.GetEmployeePostBriefByEmployeeIDAsync(approvalInfo.OWNERID);
                //if (operationType == FormTypes.Audit || operationType == FormTypes.Browse)
                //{
                //    //if (Common.CurrentLoginUserInfo.EmployeeID != approvalInfo.OWNERID)
                //    //{
                //    //    personclient.GetEmployeeDetailByIDAsync(approvalInfo.OWNERID);
                //    //}
                //    //else
                //    //{
                //    //    InitUserInfo();
                //    //}
                //    RefreshUI(RefreshedTypes.AuditInfo);
                //}


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

        #region CheckBox事件

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (ckbHasFee.IsChecked == true)
            {
                fbCtr.Visibility = Visibility.Visible;
            }
            else
            {
                fbCtr.Visibility = Visibility.Collapsed;
                SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 6);
            }
            //fbCtr.Visibility = ckbHasFee.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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
                    //SMT.SaaS.FrameworkUI.OrganizationControl.ExtOrgObj companyInfo = ent.FirstOrDefault();
                    //SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE empInfo = (SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE)companyInfo.ObjectInstance;
                    ////approvalInfo = new T_OA_APPROVALINFO();
                    //approvalInfo.OWNERCOMPANYID = empInfo.OWNERCOMPANYID;
                    //approvalInfo.OWNERDEPARTMENTID = empInfo.OWNERDEPARTMENTID;
                    //approvalInfo.OWNERID = empInfo.EMPLOYEEID;
                    //approvalInfo.OWNERNAME = empInfo.EMPLOYEECNAME;
                    //approvalInfo.OWNERPOSTID = empInfo.T_HR_EMPLOYEEPOST.FirstOrDefault().EMPLOYEEPOSTID;
                    //txtOwnerName.Text = companyInfo.ObjectName;
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

                    OwnerDepartmentid = deptid;

                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;
                    string corpName = corp.CNAME;//公司
                    StrCompanyName = corpName;
                    OwnerCompanyid = corpid;

                    Ownerid = userInfo.ObjectID;
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
                    //PersonnelServiceClient psClient = new PersonnelServiceClient();
                    psClient.GetEmployeeByIDAsync(userInfo.ObjectID);
                    //psClient.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(psClient_GetEmployeeByIDCompleted);
                    _VM.GetApprovalTypeByCompanyandDepartmentidAsync(OwnerCompanyid, OwnerDepartmentid);
                }
            };
            lookup.MultiSelected = true;
            lookup.Show();
        }

        #endregion

        #region 根据员工信息获取员工手机信息


        void psClient_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                T_HR_EMPLOYEE ownerInfo = e.Result;
                if (ownerInfo != null)
                {
                    if (!string.IsNullOrEmpty(ownerInfo.OFFICEPHONE))
                    {
                        txtTel.Text = ownerInfo.OFFICEPHONE;
                    }
                    else
                    {
                        if (ownerInfo.MOBILE != null)
                        {
                            txtTel.Text = ownerInfo.MOBILE;
                        }
                    }
                }
            }
            else
            {
                
                Logger.Current.Log(e.Error.ToString(), Category.Debug, Priority.Low);
            }
        }
        #endregion

        #region IAudit

        private string GetXmlString(string StrSource, T_OA_APPROVALINFO Info)
        {
            string StrReturn = "";
            try
            {
                SMT.SaaS.MobileXml.MobileXml mx = new MobileXml.MobileXml();


                List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
                AutoList.Add(basedata("T_OA_APPROVALINFO", "CHECKSTATE", approvalInfo.CHECKSTATE, "审核中"));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERCOMPANYID", approvalInfo.OWNERCOMPANYID, StrCompanyName));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERDEPARTMENTID", approvalInfo.OWNERDEPARTMENTID, StrDepartmentName));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERPOSTID", approvalInfo.OWNERPOSTID, StrPostName));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "OWNERID", approvalInfo.OWNERID, approvalInfo.OWNERNAME + "-" + StrPostName + "-" + StrDepartmentName + "-" + StrCompanyName));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
                AutoList.Add(basedata("T_OA_APPROVALINFO", "POSTLEVEL", postLevel, postLevel));

                StrReturn = mx.TableToXml(Info, "a", StrSource, AutoList);
                //string a = mx.TableToXml(Info, "a", str, AutoList);

            }
            catch (Exception ex)
            {
                Logger.Current.Log(ex.Message, Category.Debug, Priority.Low);
            }
            return StrReturn;
        }

        public void SetFlowRecordEntity(FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            if (operationType == FormTypes.Edit || operationType == FormTypes.Resubmit)
            {
                EntityBrowser browser = this.FindParentByType<EntityBrowser>();
                browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            }
            //entity.ModelCode = "T_OA_APPROVALINFO";
            
            
            string strXmlObjectSource = string.Empty;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("CHARGEMONEY", approvalInfo.CHARGEMONEY.ToString());
            parameters.Add("POSTLEVEL", postLevel);
            parameters.Add("DEPARTMENTNAME", depName);
            entity.SystemCode = "OA";
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML)) //返回的XML定义不为空时对业务对象进行填充
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, approvalInfo);
            //strXmlObjectSource = Utility.ObjListToXml<T_OA_APPROVALINFO>(approvalInfo, entity.BusinessObjectDefineXML);
            //strXmlObjectSource = Utility.ObjListToXml<T_OA_APPROVALINFO>(approvalInfo, parameters, "OA");
            //strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML,approvalInfo);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", approvalInfo.OWNERID);
            paraIDs.Add("CreatePostID", approvalInfo.OWNERPOSTID);
            paraIDs.Add("CreateDepartmentID", approvalInfo.OWNERDEPARTMENTID);
            paraIDs.Add("CreateCompanyID", approvalInfo.OWNERCOMPANYID);


            if (approvalInfo.CHECKSTATE == ((int)CheckStates.UnSubmit).ToString())
            {
                SMT.SaaS.FrameworkUI.Common.Utility.SetAuditEntity(entity, "T_OA_APPROVALINFO", approvalInfo.APPROVALID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_OA_APPROVALINFO", approvalInfo.APPROVALID, strXmlObjectSource);
            }
        }

        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            if (Common.CurrentLoginUserInfo.EmployeeID != approvalInfo.OWNERID)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }
            if ((operationType == FormTypes.Resubmit || operationType == FormTypes.Edit ))// && canSubmit == false)
            {
                //RefreshUI(RefreshedTypes.ShowProgressBar);
                if (!DataValidation())
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                    //ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    //    Utility.GetResourceStr("请先保存修改的记录"),
                    //Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    return;
                }
            }

        }

        public void OnSubmitCompleted(FrameworkUI.AuditControl.AuditEventArgs.AuditResult args)
        {
            Utility.InitFileLoad(FormTypes.Audit, uploadFile,approvalInfo.APPROVALID,false);
            string state = "";
            SetControlsEnable();
            string UserState = "Audit";
            
            switch (args)
            {
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Auditing:
                    state = Utility.GetCheckState(CheckStates.Approving);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.Approving;
                    isAuditing = true;
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Successful:
                    state = Utility.GetCheckState(CheckStates.Approved);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.Approved;
                    break;
                case SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Fail:
                    state = Utility.GetCheckState(CheckStates.UnApproved);
                    _flwResult = SMT.SaaS.FrameworkUI.CheckStates.UnApproved;
                    break;
            }

            RefreshUI(RefreshedTypes.HideProgressBar);
            if (_flwResult == SMT.SaaS.FrameworkUI.CheckStates.Approving)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }
            if (_flwResult == SMT.SaaS.FrameworkUI.CheckStates.Approved)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            }

            approvalInfo.CHECKSTATE = state;    
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(saveType);

            
            //Logger.Current.Log("事项审批进入了状态修改", Category.Debug, Priority.Low);
            
            
        }

        public string GetAuditState()
        {
            string state = "-1";
            if (approvalInfo != null)
            {

                state = approvalInfo.CHECKSTATE;//事项审批只要审核后就能看到转发按钮，切记，后面如果修改，要小心
                //if (operationType == FormTypes.Browse)
                //    state = "-1";
                if (operationType == FormTypes.Resubmit && !isAuditing)//是重新提交单据并且提交，目的为了重新提交审核后隐藏保存等按钮
                    state = "0";
            }

            return state;
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


        #endregion

        #region IForm 成员

        public void ClosedWCFClient()
        {
            _VM.DoClose();
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
            strXmlObjectSource = Utility.ObjListToXmlForTravel<T_OA_APPROVALINFO>(approvalInfo, "OA", parameters);
            ApprovalTypeList apptype = new ApprovalTypeList(StrOld, StrApprovaltype, lstApprovalids, OwnerCompanyid, OwnerDepartmentid, strXmlObjectSource);

            //ApprovalTypeList apptype = new ApprovalTypeList(StrOld, StrApprovaltype, lstApprovalids, OwnerCompanyid, OwnerDepartmentid, strXmlObjectSource);

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
                _VM.Get_ApporvalTempletByApporvalTypeAsync(StrApprovaltype);
                RefreshUI(RefreshedTypes.ShowProgressBar);
               
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

        void _VM_Get_ApporvalTempletByApporvalTypeCompleted(object sender, Get_ApporvalTempletByApporvalTypeCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            T_OA_APPROVALINFOTEMPLET templet = e.Result;
            if (templet != null)
            {
                txtTitle.Text = templet.APPROVALTITLE;
                txtContent.Document = templet.CONTENT;
            }
        }

        #endregion

        #region 获取公司名字
        private string GetCompanyName(string StrCompanyId)
        {
            string IsReturn = "";

            return IsReturn;
        }
        #endregion

        public void FileLoadedCompleted()
        {
            //_VM.Get_ApporvalAsync(approvalid);
            //if (!ctrFile._files.HasAccessory)
            //{
            //    if (operationType == FormTypes.Browse || operationType == FormTypes.Audit)
            //    {
            //        SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 4);
            //    }
            //}
        }

        private void HiddenGrid()
        {
            //if (uploadFile.f)
            //{
            //    if (operationType == FormTypes.Browse || operationType == FormTypes.Audit)
            //    {
            //        SMT.SaaS.FrameworkUI.Common.Utility.HiddenGridRow(this.LayGrid, 4);
            //    }
            //}
        }

        private void ControlsChanged()
        {
            canSubmit = false;
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            ControlsChanged();
        }
    }
}
