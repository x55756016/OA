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

using SMT.SaaS.FrameworkUI;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SaaS.FrameworkUI.AuditControl;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.OrganizationControl;
using SMT.Saas.Tools.SalaryWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SaaS.MobileXml;
//using SMT.Saas.Tools.DailyManagementWS;
using System.Collections.ObjectModel;

namespace SMT.HRM.UI.Form.Personnel
{
    public partial class EmployeePostChangeForm : BaseForm, IEntityEditor, IAudit, IClient
    {
        #region 初始化
        public FormTypes FormType { get; set; }
        private T_HR_EMPLOYEEPOSTCHANGE postChange;
        private T_HR_EMPLOYEEPOST epost;
        private SMT.Saas.Tools.OrganizationWS.T_HR_POST ent;
        private string fromPostLevel = string.Empty;
        private string toPostLevel = string.Empty;
        public bool isMainPostChanged = false;

        public T_HR_EMPLOYEEPOSTCHANGE PostChange
        {
            get { return postChange; }
            set
            {
                postChange = value;
                this.DataContext = value;
            }
        }  
        public string createUserName;
        private bool canSubmit = false;//能否提交审核
        private bool closeFormFlag = false;//是否关闭窗体 false 表示不关闭
        private List<ToolbarItem> ToolbarItems = new List<ToolbarItem>();
        private List<T_HR_EMPLOYEEPOST> RepeatPostChecker;//用于检查已存在职位
        //DailyManagementServicesClient DMClient = new DailyManagementServicesClient();//用于获取未还款数据
        //SMT.Saas.Tools.FBServiceWS.FBServiceClient fbClient = new SMT.Saas.Tools.FBServiceWS.FBServiceClient();//列出未还款用 luojie
        //bool isExistLoan = false;//用于判断是否有借款，true为有
        public bool needsubmit = false;//提交审核,
        public bool isSubmit = false;//用于给保存方法判断是点击了保存还是提交
        public bool isSave = false; //是否作了保存操作
        private RefreshedTypes refreshType = RefreshedTypes.CloseAndReloadData;
        private string changeID;
        PersonnelServiceClient client;
        SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient orgClient;
        SMT.Saas.Tools.FlowWFService.ServiceClient flowClient;
        public EmployeePostChangeForm(FormTypes type, string strID)
        {
            InitializeComponent();
            FormType = type;
            changeID = strID;
            InitParas(strID);
        }
        /// <summary>
        /// 无参的构造函数，为了平台的待办任务的创建
        /// add by ldx
        /// </summary>
        public EmployeePostChangeForm()
        {
            InitializeComponent();
            
            FormType = FormTypes.New;
            changeID = "";
            InitParas(changeID);
        }

        private void InitParas(string strID)
        {
            client = new PersonnelServiceClient();
            flowClient = new Saas.Tools.FlowWFService.ServiceClient();
            client.GetEmployeePostChangeByIDCompleted += new EventHandler<GetEmployeePostChangeByIDCompletedEventArgs>(client_GetEmployeePostChangeByIDCompleted);
            client.EmployeePostChangeAddCompleted += new EventHandler<EmployeePostChangeAddCompletedEventArgs>(client_EmployeePostChangeAddCompleted);
            client.EmployeePostChangeUpdateCompleted += new EventHandler<EmployeePostChangeUpdateCompletedEventArgs>(client_EmployeePostChangeUpdateCompleted);
            client.EmployeePostChangeDeleteCompleted += new EventHandler<EmployeePostChangeDeleteCompletedEventArgs>(client_EmployeePostChangeDeleteCompleted);
            client.GetEmployeePostByIDCompleted += new EventHandler<GetEmployeePostByIDCompletedEventArgs>(client_GetEmployeePostByIDCompleted);
            client.EmployeePostUpdateCompleted += new EventHandler<EmployeePostUpdateCompletedEventArgs>(client_EmployeePostUpdateCompleted);
            client.EmployeePostAddCompleted += new EventHandler<EmployeePostAddCompletedEventArgs>(client_EmployeePostAddCompleted);
            client.EmployeePostChangeCompleted += new EventHandler<EmployeePostChangeCompletedEventArgs>(client_EmployeePostChangeCompleted);
            client.GetEmployeeToEngineCompleted += new EventHandler<GetEmployeeToEngineCompletedEventArgs>(client_GetEmployeeToEngineCompleted);
            client.GetAllPostByEmployeeIDCompleted += new EventHandler<GetAllPostByEmployeeIDCompletedEventArgs>(client_GetAllPostByEmployeeIdCompeted);
            client.CheckBusinesstripCompleted += new EventHandler<CheckBusinesstripCompletedEventArgs>(Client_CheckBusinesstripCompleted);
            client.GetPostsActivedByEmployeeIDCompleted += new EventHandler<GetPostsActivedByEmployeeIDCompletedEventArgs>(client_GetPostsActivedByEmployeeIDCompleted);
            orgClient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            orgClient.GetPostByIdCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs>(orgClient_GetPostByIdCompleted);
            orgClient.GetPostNumberCompleted += new EventHandler<SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs>(orgClient_GetPostNumberCompleted);
            //fbClient.GetLeavingUserCompleted += new EventHandler<SMT.Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs>(fbClient_GetLeavingUserCompleted);
            //flowClient.IsExistFlowDataByUserIDAsync("","");
            flowClient.IsExistFlowDataByUserIDCompleted += new EventHandler<Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs>(flowClient_IsExistFlowDataByUserIDCompleted);
            //DMClient.GetPersonAccountListByMultSearchCompleted += new EventHandler<GetPersonAccountListByMultSearchCompletedEventArgs>(DMClient_GetPersonAccountListByMultSearchCompleted);
            
            this.Loaded += new RoutedEventHandler(EmployeePostChangeForm_Loaded);
            #region 原来的
            /*
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            */
            #endregion
        }

        
        void client_GetPostsActivedByEmployeeIDCompleted(object sender, GetPostsActivedByEmployeeIDCompletedEventArgs e)
        {
            //zhangwei modify 2014-3-6 员工主键岗位异动判断是否选择的是兼职岗位
            if (e != null)
            {
                var employeePostList = e.Result;
                var isExists = employeePostList.Where(t => t.T_HR_POST.POSTID == ent.POSTID && t.ISAGENCY == "1" && t.EDITSTATE == "1").FirstOrDefault();
                if (isExists == null)
                {
                    MessageBox.Show("请选择兼职岗位");
                    return;
                }
                lkPost.DataContext = ent;
                HandlePostChanged(ent);
            }
        }

        void client_EmployeePostChangeDeleteCompleted(object sender, EmployeePostChangeDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
              Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("DELETESUCCESS"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                entBrowser.Close();
            }
            FormType = FormTypes.Browse;
            RefreshUI(RefreshedTypes.All);
        }

        void EmployeePostChangeForm_Loaded(object sender, RoutedEventArgs e)
        {
            //by luojie 重载提交
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.BtnSaveSubmit.Click -= new RoutedEventHandler(entBrowser.btnSubmit_Click);
            entBrowser.BtnSaveSubmit.Click += new RoutedEventHandler(BtnSaveSubmit_Click);
            //tbcContainer.Visibility = Visibility.Collapsed;
            #region 原来的
            if (FormType == FormTypes.Browse || FormType == FormTypes.Audit)
            {
                this.IsEnabled = false;
            }
            #endregion
            if (FormType == FormTypes.New)
            {
                PostChange = new T_HR_EMPLOYEEPOSTCHANGE();
                PostChange.POSTCHANGEID = Guid.NewGuid().ToString();
                PostChange.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                PostChange.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                PostChange.CREATEDATE = DateTime.Now;
                postChange.CHANGEDATE = DateTime.Now.ToString();
                epost = new T_HR_EMPLOYEEPOST();
                epost.EMPLOYEEPOSTID = Guid.NewGuid().ToString();
                createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                SetToolBar();
            }
            else
            {
                RefreshUI(RefreshedTypes.ShowProgressBar);
                lkEmployeeName.IsEnabled = false;
                client.GetEmployeePostChangeByIDAsync(changeID);
                
            }
            if (isMainPostChanged)
            {
                this.chkIsAgency.Visibility=Visibility.Collapsed;
                IsAgencyLabel.Text = "员工主兼职互换";
            }
        }

        //by luojie 
        //工具栏提交按钮的重载方法
        void BtnSaveSubmit_Click(object sender, RoutedEventArgs e)
        {
            RefreshUI(RefreshedTypes.ShowProgressBar);
                       
            needsubmit = true;
            isSubmit = true;
            
            Save();
        }

        /// <summary>
        ///     回到提交前的状态
        /// </summary>
        private void BackToSubmit()
        {
            RefreshUI(RefreshedTypes.AuditInfo);
            needsubmit = false;
            //isSubmit = false;

            //隐藏工具栏 不允许二次提交
            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>(); 
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
            #endregion
            if (refreshType == RefreshedTypes.CloseAndReloadData)
            {
                //refreshType = RefreshedTypes.AuditInfo;
                refreshType = RefreshedTypes.HideProgressBar;
            }
            
        }

        //by luojie 
        //编辑或查看的时候如果是外部异动则显示未还款数据
        //private void ShowDtBorrowMoney()
        //{
        //    if (PostChange.FROMCOMPANYID !=null && postChange.TOCOMPANYID!=null)
        //    {
        //        if (PostChange.FROMCOMPANYID != PostChange.TOCOMPANYID && isExistLoan)
        //        {
        //            //DtBorrowMoney.Visibility = Visibility.Visible;
        //            tbcContainer.Visibility = Visibility.Visible;
        //        }
        //        else
        //        {
        //            tbcContainer.Visibility = Visibility.Collapsed;
        //        }
        //    }
        //    else
        //    {
        //        tbcContainer.Visibility = Visibility.Collapsed;
        //    }
        //}


        void flowClient_IsExistFlowDataByUserIDCompleted(object sender, Saas.Tools.FlowWFService.IsExistFlowDataByUserIDCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                lkEmployeeName.DataContext = null;
                needsubmit = false;
                isSubmit = false;
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.Result))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    needsubmit = false;
                    isSubmit = false;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.Result),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    List<T_HR_POST> dictp = Application.Current.Resources["SYS_PostInfo"] as List<T_HR_POST>;
                    if (dictp != null)
                    {
                        var ent = from c in dictp
                                  where c.FATHERPOSTID == PostChange.OWNERPOSTID
                                  select c;
                        if (ent.Count() > 0)
                        {
                            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("该岗位下存在从属岗位，异动要注意"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                        }
                    }
                    SaveChange();
                }
            }
        }







        #endregion
        #region 完成事件
        /// <summary>
        /// 获取创建人信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeeToEngineCompleted(object sender, GetEmployeeToEngineCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    createUserName = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeName;
                }
                else
                {
                    createUserName = e.Result[0].EMPLOYEENAME;
                }
                RefreshUI(RefreshedTypes.AuditInfo);
                SetToolBar();

            }

        }

        /// <summary>
        /// 异动审核通过修改员工岗位信息 并修改薪资档案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeePostChangeCompleted(object sender, EmployeePostChangeCompletedEventArgs e)
        {
            //if (e.Error != null && e.Error.Message != "")
            //{
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
            //}
            //else
            //{
            //    SalaryServiceClient salaryClient = new SalaryServiceClient();   

            //}
        }
        /// <summary>
        /// 获取岗位空缺
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetPostNumberCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostNumberCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result <= 0 && (PostChange.FROMPOSTID != ent.POSTID))
                {
                    lkPost.DataContext = null;
                    txtToCompany.Text = string.Empty;
                    txtToDepartment.Text = string.Empty;
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("POSTNUMBERFULL", "POST"),
                   Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    lkPost.DataContext = ent;
                    toPostLevel = ent.POSTLEVEL.ToString();
                    foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxPostLevel.ItemsSource)
                    {
                        if (item.DICTIONARYVALUE == ent.POSTLEVEL)
                        {
                            cbxPostLevel.SelectedItem = item;
                        }
                    }
                    HandlePostChanged(ent);
                }

            }
        }


        /// <summary>
        /// 根据ID获取员工异动岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeePostByIDCompleted(object sender, GetEmployeePostByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    epost = e.Result;
                    if (epost.ISAGENCY == "1")
                    {
                        chkIsAgency.IsChecked = true;
                    }

                    toPostLevel = epost.POSTLEVEL.ToString();

                    foreach (SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY item in cbxPostLevel.ItemsSource)
                    {
                        if (item.DICTIONARYVALUE == epost.POSTLEVEL)
                        {
                            cbxPostLevel.SelectedItem = item;
                        }
                    }
                }
            }
            RefreshUI(RefreshedTypes.HideProgressBar);

        }

        /// <summary>
        /// 根据id获取岗位异动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_GetEmployeePostChangeByIDCompleted(object sender, GetEmployeePostChangeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }

                PostChange = e.Result;
                lkEmployeeName.DataContext = PostChange.T_HR_EMPLOYEE;
                if (FormType == FormTypes.Resubmit)
                {
                    canSubmit = true;
                    PostChange.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();
                }
                //异动前
                if (!string.IsNullOrEmpty(postChange.FROMPOSTID))
                {
                    orgClient.GetPostByIdAsync(PostChange.FROMPOSTID, "Forward");
                }
                else
                {
                    orgClient.GetPostByIdAsync(PostChange.TOPOSTID, "Back");
                }

                client.GetEmployeePostByIDAsync(PostChange.EMPLOYEEPOSTID);
                if (PostChange.FROMPOSTLEVEL != null)
                {
                    fromPostLevel = PostChange.FROMPOSTLEVEL.ToString();
                    foreach (T_SYS_DICTIONARY item in cbxFromPostLevel.ItemsSource)
                    {
                        if (item.DICTIONARYVALUE == PostChange.FROMPOSTLEVEL)
                        {
                            cbxFromPostLevel.SelectedItem = item;
                            break;
                        }
                    }

                }
                if (PostChange.TOPOSTLEVEL != null)
                {
                    toPostLevel = PostChange.TOPOSTLEVEL.ToString();

                }

                client.GetAllPostByEmployeeIDAsync(e.Result.T_HR_EMPLOYEE.EMPLOYEEID);

                //判断是否有未还款
                //ShowDtBorrowMoney();
                //GetPersonAccountData();

            }
        }
        /// <summary>
        /// 根据id获取岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void orgClient_GetPostByIdCompleted(object sender, SMT.Saas.Tools.OrganizationWS.GetPostByIdCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //if (e.Result == null)
                //{
                //    Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                //    return;
                //}
                if (e.UserState.ToString() == "Forward")
                {
                    txtFromCompanyID.Text = e.Result.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    txtFromDepartmentID.Text = e.Result.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    txtFromPostID.Text = e.Result.T_HR_POSTDICTIONARY.POSTNAME;
                    //fromPostLevel = e.Result.POSTLEVEL.ToString();
                    //异动后
                    orgClient.GetPostByIdAsync(PostChange.TOPOSTID, "Back");
                }
                else if (e.UserState.ToString() == "Back" && e.Result!=null)
                {
                    lkPost.DataContext = e.Result;
                    txtToCompany.Text = e.Result.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                    txtToDepartment.Text = e.Result.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
                    //toPostLevel = e.Result.POSTLEVEL.ToString();
                    // RefreshUI(RefreshedTypes.AuditInfo);
                    //client.GetEmployeeByIDAsync(PostChange.CREATEUSERID);
                    if (postChange.CHECKSTATE == Convert.ToInt32(CheckStates.Approved).ToString()
                    || postChange.CHECKSTATE == Convert.ToInt32(CheckStates.UnApproved).ToString())
                    {
                        RefreshUI(RefreshedTypes.AuditInfo);
                        SetToolBar();
                    }
                    else
                    {
                        System.Collections.ObjectModel.ObservableCollection<string> CreateUserIDs = new System.Collections.ObjectModel.ObservableCollection<string>();
                        CreateUserIDs.Add(postChange.CREATEUSERID);
                        client.GetEmployeeToEngineAsync(CreateUserIDs);
                    }
                }
                else
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    // fromPostLevel = e.Result.POSTLEVEL.ToString();
                }
            }
        }
        /// <summary>
        /// 新增岗位异动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void client_EmployeePostChangeAddCompleted(object sender, EmployeePostChangeAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDSUCCESSED", "EMPLOYEEPOSTCHANGE"));
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
                //RefreshUI(RefreshedTypes.All);
                epost.CREATEDATE = DateTime.Now;
                client.EmployeePostAddAsync(epost);
            }
        }

        /// <summary>
        /// 更新岗位异动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeePostChangeUpdateCompleted(object sender, EmployeePostChangeUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                needsubmit = false;
                isSubmit = false;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.strMsg));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr(e.strMsg),
                  Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    needsubmit = false;
                    isSubmit = false;
                    return;
                }
                if (e.UserState.ToString() == "Edit")
                {
                    client.EmployeePostUpdateAsync(epost);
                }
                else if (e.UserState.ToString() == "Audit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("AUDITSUCCESSED", "EMPLOYEEPOSTCHANGE"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"),
           Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
                else if (e.UserState.ToString() == "Submit")
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUBMITSUCCESSED", "EMPLOYEEPOSTCHANGE"));
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    RefreshUI(RefreshedTypes.AuditInfo);
                    RefreshUI(RefreshedTypes.All);
                }
            }
            ///by luojie 转至提交按钮原来的方法
            if (needsubmit)
            {
                    try
                    {
                        EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                        entBrowser.ManualSubmit();
                        BackToSubmit();                 
                    }
                    catch (Exception )
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("提交失败"),
                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

                    }
                
            }
        }

        /// <summary>
        /// 新增岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeePostAddCompleted(object sender, EmployeePostAddCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);

            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("ADDDATASUCCESSED"),
             Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    FormType = FormTypes.Edit;
                    EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
                    entBrowser.FormType = FormTypes.Edit;
                    RefreshUI(RefreshedTypes.AuditInfo);
                }
                ToolbarItems = Utility.CreateFormEditButton();
                ToolbarItems.Add(ToolBarItems.Delete);
                RefreshUI(RefreshedTypes.All);

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }
        /// <summary>
        /// 修改岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_EmployeePostUpdateCompleted(object sender, EmployeePostUpdateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                //by luojie 如果是提交事件则不显示保存成功
                if (!isSubmit)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                    isSubmit = false;
                }
                canSubmit = true;
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                else
                {
                    RefreshUI(RefreshedTypes.AuditInfo);
                }

                RefreshUI(RefreshedTypes.All);

            }
            RefreshUI(RefreshedTypes.HideProgressBar);
        }

        /// <summary>
        ///     Added by 罗捷
        ///     根据员工ID获取所有的Post，并赋值给RepeatPostChecker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">AllPost</param>
        void client_GetAllPostByEmployeeIdCompeted(object sender, GetAllPostByEmployeeIDCompletedEventArgs e)
        {
            RefreshUI(RefreshedTypes.HideProgressBar);
            if (e.Error != null)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RepeatPostChecker = new List<T_HR_EMPLOYEEPOST>();
                if (e.Result.Count() > 0)
                {                 
                    RepeatPostChecker = e.Result.ToList();
                }
                else
                {
                    RepeatPostChecker = null;
                }
            }
        }

        /// <summary>
        /// 未还款 luojie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //void fbClient_GetLeavingUserCompleted(object sender, SMT.Saas.Tools.FBServiceWS.GetLeavingUserCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != "")
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        DtBorrowMoney.ItemsSource = e.Result;
                
        //    }
        //}

        #endregion
        /// <summary>
        /// 生成按钮
        /// </summary>
        private void SetToolBar()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEEPOSTCHANGE", PostChange.OWNERID,
            //        PostChange.OWNERPOSTID, PostChange.OWNERDEPARTMENTID, PostChange.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (PostChange != null)
                {
                    if (PostChange.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEPOSTCHANGE", PostChange.OWNERID,
                    PostChange.OWNERPOSTID, PostChange.OWNERDEPARTMENTID, PostChange.OWNERCOMPANYID);
            }
            RefreshUI(RefreshedTypes.All);
        }

        #region IEntityEditor
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEEPOSTCHANGE");
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
                    Save();
                    break;
                case "1":
                    closeFormFlag = true;
                    Save();
                    // Cancel();
                    break;
                case "Delete":
                    Delete(postChange.POSTCHANGEID);
                    break;
            }
        }

        private void Delete(string id)
        {
            string Result = "";
            string strMsg = string.Empty;
            //提示是否删除
            ComfirmWindow com = new ComfirmWindow();
            com.OnSelectionBoxClosed += (obj, result) =>
            {
                client.EmployeePostChangeDeleteAsync(new System.Collections.ObjectModel.ObservableCollection<string>(new List<string>() { id }));
            };
            com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("确定要删除吗？"), ComfirmWindow.titlename, Result);
        }

        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            return items;
        }
        public List<ToolbarItem> GetToolBarItems()
        {
            if (FormType == FormTypes.New)
                ToolbarItems = Utility.CreateFormSaveButton();
            //else
            //    ToolbarItems = Utility.CreateFormSaveButton("T_HR_EMPLOYEEPOSTCHANGE", PostChange.OWNERID,
            //        PostChange.OWNERPOSTID, PostChange.OWNERDEPARTMENTID, PostChange.OWNERCOMPANYID);

            else if (FormType == FormTypes.Edit)
            {
                ToolbarItems = Utility.CreateFormEditButton();
                if (PostChange != null)
                {
                    if (PostChange.CHECKSTATE == "0")
                    {
                        ToolbarItems.Add(ToolBarItems.Delete);
                    }
                }
            }
            else if (FormType == FormTypes.Browse)
            {
                ToolbarItems = new List<ToolbarItem>();
            }
            else
            {
                ToolbarItems = Utility.CreateFormEditButton("T_HR_EMPLOYEEPOSTCHANGE", PostChange.OWNERID,
                    PostChange.OWNERPOSTID, PostChange.OWNERDEPARTMENTID, PostChange.OWNERCOMPANYID);
            }
            return ToolbarItems;
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
        #region IClient
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            orgClient.DoClose();
            flowClient.DoClose();
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
        #region mobilexml
        private string GetXmlString(string StrSource, T_HR_EMPLOYEEPOSTCHANGE Info)
        {
            SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY ownerCompany = (Application.Current.Resources["SYS_CompanyInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>).Where(s => s.COMPANYID == Info.OWNERCOMPANYID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT ownerDepartment = (Application.Current.Resources["SYS_DepartmentInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT>).Where(s => s.DEPARTMENTID == Info.OWNERDEPARTMENTID).FirstOrDefault();
            SMT.Saas.Tools.OrganizationWS.T_HR_POST ownerPost = (Application.Current.Resources["SYS_PostInfo"] as List<SMT.Saas.Tools.OrganizationWS.T_HR_POST>).Where(s => s.POSTID == Info.OWNERPOSTID).FirstOrDefault();
            string ownerCompanyName = string.Empty;
            string ownerDepartmentName = string.Empty;
            string ownerPostName = string.Empty;
            if (ownerCompany != null)
            {
                ownerCompanyName = ownerCompany.CNAME;
            }
            if (ownerDepartment != null)
            {
                ownerDepartmentName = ownerDepartment.T_HR_DEPARTMENTDICTIONARY == null ? "" : ownerDepartment.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            }
            if (ownerPost != null)
            {
                ownerPostName = ownerPost.T_HR_POSTDICTIONARY == null ? "" : ownerPost.T_HR_POSTDICTIONARY.POSTNAME;
            }

            decimal? stateValue = Convert.ToDecimal("1");
            string checkState = string.Empty;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY checkStateDict = (Application.Current.Resources["SYS_DICTIONARY"] as List<SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY>).Where(s => s.DICTIONCATEGORY == "CHECKSTATE" && s.DICTIONARYVALUE == stateValue).FirstOrDefault();
            checkState = checkStateDict == null ? "" : checkStateDict.DICTIONARYNAME;

            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY POSTCHANGCATEGORY = cbPostCategory.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY TOPOSTLEVEL = cbxPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY FROMPOSTLEVEL = cbxFromPostLevel.SelectedItem as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;
            SMT.SaaS.MobileXml.MobileXml mx = new SMT.SaaS.MobileXml.MobileXml();
            List<SMT.SaaS.MobileXml.AutoDictionary> AutoList = new List<SMT.SaaS.MobileXml.AutoDictionary>();
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "CHECKSTATE", "1", checkState));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "POSTCHANGCATEGORY", POSTCHANGCATEGORY == null ? "0" : POSTCHANGCATEGORY.DICTIONARYVALUE.ToString(), POSTCHANGCATEGORY == null ? "" : POSTCHANGCATEGORY.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "TOPOSTLEVEL", TOPOSTLEVEL == null ? "0" : TOPOSTLEVEL.DICTIONARYVALUE.ToString(), TOPOSTLEVEL == null ? "" : TOPOSTLEVEL.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "FROMPOSTLEVEL", FROMPOSTLEVEL == null ? "0" : FROMPOSTLEVEL.DICTIONARYVALUE.ToString(), FROMPOSTLEVEL == null ? "" : FROMPOSTLEVEL.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "POSTLEVEL", TOPOSTLEVEL == null ? "0" : TOPOSTLEVEL.DICTIONARYVALUE.ToString(), TOPOSTLEVEL == null ? "" : TOPOSTLEVEL.DICTIONARYNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "EMPLOYEECNAME", Info.T_HR_EMPLOYEE.EMPLOYEECNAME, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "OWNER", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEEID));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "CREATEUSERNAME", createUserName, createUserName));

            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "TOPOSTID", Info.TOPOSTID, (lkPost.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST) == null ? "" : (lkPost.DataContext as SMT.Saas.Tools.OrganizationWS.T_HR_POST).T_HR_POSTDICTIONARY.POSTNAME));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "TODEPARTMENTID", Info.TODEPARTMENTID, string.IsNullOrEmpty(txtToDepartment.Text) ? "" : txtToDepartment.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "TOCOMPANYID", Info.TOCOMPANYID, string.IsNullOrEmpty(txtToCompany.Text) ? "" : txtToCompany.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "FROMPOSTID", Info.FROMPOSTID, string.IsNullOrEmpty(txtFromPostID.Text) ? "" : txtFromPostID.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "FROMDEPARTMENTID", Info.FROMDEPARTMENTID, string.IsNullOrEmpty(txtFromDepartmentID.Text) ? "" : txtFromDepartmentID.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "FROMCOMPANYID", Info.FROMCOMPANYID, string.IsNullOrEmpty(txtFromCompanyID.Text) ? "" : txtFromCompanyID.Text));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "ISAGENCY", Info.ISAGENCY, Info.ISAGENCY == "0" ? "非兼职" : "兼职"));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "EMPLOYEEID", Info.T_HR_EMPLOYEE.EMPLOYEEID, Info.T_HR_EMPLOYEE.EMPLOYEECNAME));

            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "OWNERCOMPANYID", Info.OWNERCOMPANYID, ownerCompanyName));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "OWNERDEPARTMENTID", Info.OWNERDEPARTMENTID, ownerDepartmentName));
            AutoList.Add(basedata("T_HR_EMPLOYEEPOSTCHANGE", "OWNERPOSTID", Info.OWNERPOSTID, ownerPostName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "TYPEAPPROVAL", approvalInfo.TYPEAPPROVAL, StrApprovalTypeName));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "CONTENT", approvalInfo.APPROVALID, approvalInfo.APPROVALID));
            //AutoList.Add(basedata("T_OA_APPROVALINFO", "AttachMent", approvalInfo.APPROVALID, approvalInfo.APPROVALID));

            string a = mx.TableToXml(Info, null, StrSource, AutoList);

            return a;
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
        #region IAudit
        void AuditCtrl_Auditing(object sender, SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs e)
        {
            //if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID != Post.OWNERID)
            //{
            //    RefreshUI(RefreshedTypes.HideProgressBar);
            //    e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
            //    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
            //        Utility.GetResourceStr("OPERATINGWITHOUTAUTHORITY"),
            //    Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
            //    return;
            //}
            if (FormType == FormTypes.Resubmit && canSubmit == false)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                e.Result = SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs.AuditResult.Cancel;
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"),
                    Utility.GetResourceStr("请先保存修改的记录"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                return;
            }

        }
        public void SetFlowRecordEntity(SMT.SaaS.FrameworkUI.AuditControl.Flow_FlowRecord_T entity)
        {
            // Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEPOSTCHANGE", PostChange.POSTCHANGEID);

            EntityBrowser browser = this.FindParentByType<EntityBrowser>();
            browser.AuditCtrl.Auditing += new EventHandler<SMT.SaaS.FrameworkUI.AuditControl.AuditEventArgs>(AuditCtrl_Auditing);
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("EMPLOYEECNAME", PostChange.T_HR_EMPLOYEE.EMPLOYEECNAME);
            para.Add("EMPLOYEEID", PostChange.T_HR_EMPLOYEE.EMPLOYEEID);
            para.Add("FROMPOSTLEVEL", fromPostLevel);
            para.Add("TOPOSTLEVEL", toPostLevel);
            para.Add("POSTLEVEL", toPostLevel);
            para.Add("CREATEUSERNAME", createUserName);
            para.Add("OWNER", PostChange.T_HR_EMPLOYEE.EMPLOYEECNAME);

            Dictionary<string, string> paraIDs = new Dictionary<string, string>();
            paraIDs.Add("CreateUserID", PostChange.T_HR_EMPLOYEE.EMPLOYEEID);
            paraIDs.Add("CreatePostID", PostChange.TOPOSTID);
            paraIDs.Add("CreateDepartmentID", PostChange.TODEPARTMENTID);
            paraIDs.Add("CreateCompanyID", PostChange.TOCOMPANYID);

            entity.SystemCode = "HR";
            string strXmlObjectSource = string.Empty;
            // strXmlObjectSource = Utility.ObjListToXml<T_HR_EMPLOYEEPOSTCHANGE>(PostChange, para, "HR", para2, null);
            if (!string.IsNullOrEmpty(entity.BusinessObjectDefineXML))
                strXmlObjectSource = this.GetXmlString(entity.BusinessObjectDefineXML, PostChange);
            if (PostChange.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEPOSTCHANGE", PostChange.POSTCHANGEID, strXmlObjectSource, paraIDs);
            }
            else
            {
                Utility.SetAuditEntity(entity, "T_HR_EMPLOYEEPOSTCHANGE", PostChange.POSTCHANGEID, strXmlObjectSource);
            }

        }
        public void OnSubmitCompleted(AuditEventArgs.AuditResult args)
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
            if (PostChange.CHECKSTATE == Convert.ToInt32(CheckStates.UnSubmit).ToString())
            {
                UserState = "Submit";
            }
            PostChange.CHECKSTATE = state;
            if (UserState.ToString() == "Audit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            else if (UserState.ToString() == "Submit")
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("SUCCESSSUBMITAUDIT"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
            }
            RefreshUI(RefreshedTypes.HideProgressBar);
            RefreshUI(RefreshedTypes.AuditInfo);
            RefreshUI(RefreshedTypes.All);
            //client.EmployeePostChangeUpdateAsync(PostChange, strMsg, UserState);
            //if (args == AuditEventArgs.AuditResult.Successful)
            //{
            //    ChangeEmployeeOwner(postChange);
            //}
        }
        /// <summary>
        /// 修改员工的所属公司 部门 岗位
        /// </summary>
        /// <param name="ent"></param>
        public void ChangeEmployeeOwner(T_HR_EMPLOYEEPOSTCHANGE ent)
        {
            T_HR_EMPLOYEE employee = ent.T_HR_EMPLOYEE;

            if (ent.POSTCHANGCATEGORY == "0")
            {
                employee.OWNERPOSTID = ent.TOPOSTID;
                employee.OWNERDEPARTMENTID = ent.TODEPARTMENTID;
            }
            else if (ent.POSTCHANGCATEGORY == "1")
            {
                employee.OWNERDEPARTMENTID = ent.TODEPARTMENTID;
                employee.OWNERPOSTID = ent.TOPOSTID;
                employee.OWNERCOMPANYID = ent.TOCOMPANYID;
            }
            //   client.EmployeeUpdateAsync(employee, ent.TOCOMPANYID);
            client.EmployeePostChangeAsync(ent);


        }
        public string GetAuditState()
        {
            string state = "-1";
            if (PostChange != null)
                state = PostChange.CHECKSTATE;
            if (FormType == FormTypes.Browse)
            {
                state = "-1";
            }
            return state;
        }
        #endregion
        #region 保存
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        private bool Save()
        {
            // List<SMT.SaaS.FrameworkUI.Validator.ValidatorBase> validators = Group1.ValidateAll();
            RefreshUI(RefreshedTypes.ShowProgressBar);
            isSave = true;

            if (!CheckData())
            {
                needsubmit = false;
                isSubmit = false;
            }

            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                needsubmit = false;
                isSubmit = false;
                return false;
            }
            else
            {
                try
                {
                    epost.T_HR_POST = new T_HR_POST();
                    epost.T_HR_POST.POSTID = postChange.TOPOSTID;
                    epost.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    epost.T_HR_EMPLOYEE.EMPLOYEEID = postChange.T_HR_EMPLOYEE.EMPLOYEEID;
                    //为了区分开 新入职和异动未审核 将 EDITSTATE设置为2 
                    // epost.EDITSTATE = Convert.ToInt32(EditStates.UnActived).ToString();
                    epost.EDITSTATE = "2";
                    epost.CHECKSTATE = Convert.ToInt32(CheckStates.UnSubmit).ToString();

                    epost.POSTLEVEL = (cbxPostLevel.SelectedItem as T_SYS_DICTIONARY).DICTIONARYVALUE;
                    PostChange.TOPOSTLEVEL = epost.POSTLEVEL;
                    toPostLevel = epost.POSTLEVEL.ToString();
                    PostChange.EMPLOYEEPOSTID = epost.EMPLOYEEPOSTID;

                    // by luojie
                    //下面重制整个员工主岗位异动的判断,异动判断放入getpersonAccountData中
                    isSave = true;
                    //by luojie
                    if (GetCheckerCount() > 0)//判断是否有重复岗位
                    {
                        needsubmit = false;
                        isSubmit = false;
                    }
                    else if (chkIsAgency.IsChecked == true)
                    {
                        epost.ISAGENCY = "1";
                        PostChange.ISAGENCY = "1";
                        SaveChange();
                    }
                    else
                    {
                        epost.ISAGENCY = "0";
                        PostChange.ISAGENCY = "0";
                        //GetPersonAccountData();
                        //判断是否是主兼职岗位互换
                        if (isMainPostChanged == false)
                        {
                            //判断是否有正在进行的出差（包括未提交、审核中的出差申请和出差报销
                            if (PostChange.FROMPOSTID != PostChange.TOPOSTID)
                                client.CheckBusinesstripAsync(PostChange.T_HR_EMPLOYEE.EMPLOYEEID);
                            else
                                SaveChange();
                        }
                        else
                        {
                            SaveChange();
                        }
                    }
                    
                }
                catch (Exception )
                {
                    
                    needsubmit = false;
                    isSubmit = false;
                }
               
            }
            return true;
        }

        private bool CheckData()
        {
            if (string.IsNullOrEmpty(lkEmployeeName.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "EMPLOYEENAME"),
                 Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (string.IsNullOrEmpty(lkPost.TxtLookUp.Text))
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POST"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            if (cbxPostLevel.SelectedItem == null)
            {
                //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "POST"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("STRINGNOTNULL", "POSTLEVEL"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            return true;
        }


        //by luojie
        //添加离职或跨公司异动的未还款判断
        //获取相应人员的借款
        //public void GetPersonAccountData()
        //{
        //    RefreshUI(RefreshedTypes.ShowProgressBar);
        //    if (PostChange == null)
        //    {
        //        return;
        //    }
        //    if (string.IsNullOrWhiteSpace(PostChange.FROMCOMPANYID))
        //    {
        //        return;
        //    }
        //    if (PostChange.T_HR_EMPLOYEE == null)
        //    {
        //        return;
        //    }
        //    T_FB_PERSONACCOUNT temp = new T_FB_PERSONACCOUNT();
        //    string filter = "";    //查询过滤条件
        //    ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();   //参数值
        //    if (!string.IsNullOrEmpty(PostChange.FROMCOMPANYID))
        //    {
        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            filter += " and ";
        //        }
        //        filter += "@" + paras.Count().ToString() + ".Contains(OWNERCOMPANYID) ";
        //        paras.Add(PostChange.FROMCOMPANYID);
        //    }
        //    if (!string.IsNullOrEmpty(PostChange.T_HR_EMPLOYEE.EMPLOYEEID))
        //    {
        //        if (!string.IsNullOrEmpty(filter))
        //        {
        //            filter += " and ";
        //        }
        //        filter += "@" + paras.Count().ToString() + ".Contains(OWNERID) ";
        //        paras.Add(PostChange.T_HR_EMPLOYEE.EMPLOYEEID);
        //    }
        //    DMClient.GetPersonAccountListByMultSearchAsync(filter, paras, "OWNERID");
        //}

        //void DMClient_GetPersonAccountListByMultSearchCompleted(object sender, GetPersonAccountListByMultSearchCompletedEventArgs e)
        //{
        //    DtBorrowMoney.ItemsSource = null;
        //    try
        //    {
        //        //DtBorrowMoney.ClearValue
        //        if (e.Error != null)
        //        {

        //            ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //        }
        //        else
        //        {
        //            if (e.Error == null)
        //            {
        //                if (e.Result != null && e.Result.Count > 0)
        //                {
        //                    DtBorrowMoney.ItemsSource = e.Result.Where(c => c.BORROWMONEY > 0).ToList();
        //                    if (e.Result.Where(c => c.BORROWMONEY > 0).ToList().Count() > 0)
        //                    {
        //                        isExistLoan = true;
        //                        //ShowDtBorrowMoney();
        //                    }
        //                }
        //            }
        //        }

        //        if (isSave)
        //        {
        //            bool isMoneyClean = true;
        //            //外部异动和离职则进入是否有借还款的判断 兼职异动则不作判断
        //            if ((PostChange.POSTCHANGCATEGORY == "1" || PostChange.POSTCHANGCATEGORY == "3"))
        //            {
        //                List<T_FB_PERSONACCOUNT> bors = DtBorrowMoney.ItemsSource as List<T_FB_PERSONACCOUNT>;
        //                //兼职异动不用作任何判断
        //                if (bors != null && chkIsAgency.IsChecked==false)
        //                {
        //                    if (bors.Count() > 0)
        //                    {
        //                        RefreshUI(RefreshedTypes.HideProgressBar);
        //                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未处理借款"),
        //                            Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //                        isMoneyClean = false; //有借款则不能进入下个阶段
        //                        tbcContainer.Visibility = Visibility.Visible;
        //                        needsubmit = false;
        //                        isSubmit = false;
        //                        return;
        //                    }

        //                }
        //            }



        //            //by luojie
        //            if (GetCheckerCount() > 0 && !BanOuterPostChange())//判断是否有重复岗位
        //            {
        //                needsubmit = false;
        //                isSubmit = false;
        //            }
        //            else if (chkIsAgency.IsChecked == true)
        //            {
        //                epost.ISAGENCY = "1";
        //                PostChange.ISAGENCY = "1";
        //                SaveChange();
        //            }
        //            else
        //            {
        //                epost.ISAGENCY = "0";
        //                PostChange.ISAGENCY = "0";
        //                if (isMoneyClean)
        //                {
        //                    if (PostChange.POSTCHANGCATEGORY == "1" || PostChange.POSTCHANGCATEGORY == "3")
        //                    {
        //                        flowClient.IsExistFlowDataByUserIDAsync(PostChange.T_HR_EMPLOYEE.EMPLOYEEID, PostChange.FROMPOSTID);
        //                    }
        //                    else
        //                    {
        //                        SaveChange();
        //                    }
        //                }
        //                else
        //                {
        //                    //ShowDtBorrowMoney();
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        RefreshUI(RefreshedTypes.HideProgressBar);
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("有未知错误"),
        //     Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
        //    }
        //}

        /// <summary>
        /// 保存并关闭当前窗口
        /// </summary>
        public void Cancel()
        {
            bool flag = false;
            flag = Save();

            if (!flag)
            {
                return;
            }

            EntityBrowser entBrowser = this.FindParentByType<EntityBrowser>();
            entBrowser.Close();
        }
        #endregion

        /// <summary>
        /// 选择异动岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkPost_FindClick(object sender, EventArgs e)
        {
            OrganizationLookupForm lookup = new OrganizationLookupForm();
            lookup.SelectedObjType = OrgTreeItemTypes.Post;
            lookup.SelectedClick += (obj, ev) =>
            {
                ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.T_HR_POST;
                if (ent != null)
                {
                    if (isMainPostChanged == true)
                    {
                        client.GetPostsActivedByEmployeeIDAsync(PostChange.OWNERID);
                    }
                    else
                    {
                        orgClient.GetPostNumberAsync(ent.POSTID);
                    }
                }
            };
            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        /// <summary>
        /// 显示异动后信息（异动后的岗位 公司 ，异动类型）
        /// </summary>
        /// <param name="ent"></param>
        private void HandlePostChanged(SMT.Saas.Tools.OrganizationWS.T_HR_POST ent)
        {
            ///TODO:ADD 公司赋值
            PostChange.TOCOMPANYID = ent.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
            PostChange.TODEPARTMENTID = ent.T_HR_DEPARTMENT.DEPARTMENTID;
            PostChange.TOPOSTID = ent.POSTID;

            // PostChange.OWNERID = employee.EMPLOYEEID;
            PostChange.OWNERPOSTID = PostChange.TOPOSTID;
            PostChange.OWNERDEPARTMENTID = PostChange.TODEPARTMENTID;
            PostChange.OWNERCOMPANYID = PostChange.TOCOMPANYID;

            txtToCompany.Text = ent.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
            txtToDepartment.Text = ent.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME;
            //判断异动类型
            //by luojie 外部异动则添加未还款的判断
            if (PostChange.FROMCOMPANYID != null)
            {
                if (postChange.TOCOMPANYID == postChange.FROMCOMPANYID)
                {
                    cbPostCategory.SelectedValue = "0";
                    //tbcContainer.Visibility = Visibility.Collapsed;
                    BanOuterPostChange(false);
                }
                else
                {
                    cbPostCategory.SelectedValue = "1";
                    //GetPersonAccountData();
                    //tbcContainer.Visibility = Visibility.Visible;
                    //兼职异动的话不禁止外部异动
                    if (chkIsAgency.IsChecked == false)
                        BanOuterPostChange(true);
                    else
                        BanOuterPostChange(false);
                }
            }

        }

        /// <summary>
        /// 选择异动人员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LookUp_FindClick(object sender, EventArgs e)
        {
            #region 废弃代码
            //Dictionary<string, string> cols = new Dictionary<string, string>();
            //cols.Add("EMPLOYEECODE", "T_HR_EMPLOYEE.EMPLOYEECODE");
            //cols.Add("EMPLOYEECNAME", "T_HR_EMPLOYEE.EMPLOYEECNAME");
            //cols.Add("EMPLOYEEENAME", "T_HR_EMPLOYEE.EMPLOYEEENAME");
            //LookupForm lookup = new LookupForm(SMT.Saas.Tools.OrganizationWS.EntityNames.Employee,
            //    typeof(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST[]), cols);

            //lookup.SelectedClick += (o, ev) =>
            //{
            //    SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent = lookup.SelectedObj as SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST;

            //    if (ent != null)
            //    {
            //        lkEmployeeName.DataContext = ent.T_HR_EMPLOYEE;
            //        HandleEmployeeChanged(ent);
            //    }
            //};

            //lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            #endregion 

            #region
            OrganizationLookup lookup = new OrganizationLookup();

            lookup.SelectedObjType = OrgTreeItemTypes.Personnel;
            lookup.SelectedClick += (obj, ev) =>
            {
                //  SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE ent = lookup.SelectedObj[0].ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                List<ExtOrgObj> ent = lookup.SelectedObj as List<ExtOrgObj>;

                if (ent != null && ent.Count > 0)
                {
                    
                    ExtOrgObj companyInfo = ent.FirstOrDefault();
                    ExtOrgObj post = (ExtOrgObj)companyInfo.ParentObject;
                    string postid = post.ObjectID;
                    //  fromPostLevel=(post as SMT.Saas.Tools.OrganizationWS.T_HR_POST).POSTLEVEL.ToString();

                    ExtOrgObj dept = (ExtOrgObj)post.ParentObject;
                    string deptid = dept.ObjectID;

                    // ExtOrgObj corp = (ExtOrgObj)dept.ParentObject;
                    SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY corp = (dept.ObjectInstance as SMT.Saas.Tools.OrganizationWS.T_HR_DEPARTMENT).T_HR_COMPANY;
                    string corpid = corp.COMPANYID;

                    T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
                    temp = ent.FirstOrDefault().ObjectInstance as SMT.Saas.Tools.PersonnelWS.T_HR_EMPLOYEE;
                    
                    
                    //如果是员工主兼互换，必须选择主岗位 zhangwei modify 2014-3-6
                    var selectPost = temp.T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault();
                    if (isMainPostChanged && selectPost.ISAGENCY != "0")
                    {
                        MessageBox.Show("请选择主岗位");
                        return;
                    }

                    //异动前岗位级别
                    pgCaution.Text = string.Empty;
                    if (temp.T_HR_EMPLOYEEPOST != null)
                    {
                        // fromPostLevel = temp.T_HR_EMPLOYEEPOST.FirstOrDefault().POSTLEVEL.ToString();
                        //postChange.FROMPOSTLEVEL = temp.T_HR_EMPLOYEEPOST.FirstOrDefault().POSTLEVEL;

                        ///Modified by 罗捷
                        ///做是否是兼职岗位的判断
                        ///modified by zhangwei 去掉兼职岗位判断 2013-12-2
                        //string isAgencyPara = temp.T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().ISAGENCY;
                        //BanChangeAgencyPost(isAgencyPara);            


                        postChange.FROMPOSTLEVEL = temp.T_HR_EMPLOYEEPOST.Where(s => s.T_HR_POST.POSTID == postid).FirstOrDefault().POSTLEVEL;
                        fromPostLevel = postChange.FROMPOSTLEVEL.ToString();
                        foreach (T_SYS_DICTIONARY item in cbxFromPostLevel.ItemsSource)
                        {
                            if (item.DICTIONARYVALUE == postChange.FROMPOSTLEVEL)
                            {
                                cbxFromPostLevel.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    lkEmployeeName.DataContext = temp;

                    PostChange.EMPLOYEECODE = temp.EMPLOYEECODE;
                    PostChange.EMPLOYEENAME = temp.EMPLOYEECNAME;
                    PostChange.OWNERID = temp.EMPLOYEEID;

                    PostChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
                    postChange.T_HR_EMPLOYEE.EMPLOYEEID = temp.EMPLOYEEID;
                    postChange.T_HR_EMPLOYEE.EMPLOYEECNAME = temp.EMPLOYEECNAME;
                    postChange.T_HR_EMPLOYEE.OWNERPOSTID = temp.OWNERPOSTID;
                    postChange.T_HR_EMPLOYEE.OWNERDEPARTMENTID = temp.OWNERDEPARTMENTID;
                    postChange.T_HR_EMPLOYEE.OWNERCOMPANYID = temp.OWNERCOMPANYID;
                    postChange.T_HR_EMPLOYEE.EMPLOYEECODE = temp.EMPLOYEECODE;
                    postChange.T_HR_EMPLOYEE.EMPLOYEEENAME = temp.EMPLOYEEENAME;

                    // txtFromCompanyID.Text = corp.ObjectName;
                    txtFromCompanyID.Text = corp.CNAME;
                    txtFromDepartmentID.Text = dept.ObjectName;
                    txtFromPostID.Text = post.ObjectName;

                    PostChange.FROMCOMPANYID = corp.COMPANYID;
                    PostChange.FROMDEPARTMENTID = dept.ObjectID;
                    PostChange.FROMPOSTID = post.ObjectID;
                    client.GetAllPostByEmployeeIDAsync(PostChange.T_HR_EMPLOYEE.EMPLOYEEID);
                    orgClient.GetPostByIdAsync(PostChange.FROMPOSTID, "");
                    //luojie 获取借款信息
                    isSave = false;
                    if (pgCaution.Visibility==Visibility.Collapsed)
                        ShowOuterPostchangeState();
                    //GetPersonAccountData();
                }

            };

            lookup.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
            #endregion

        }

        /// <summary>
        ///     Added by 罗捷
        ///     判断为兼职岗位时不能异动的函数
        /// </summary>
        /// <param name="isAgency">是否是兼职岗位：1为是兼职岗位，0为不是兼职岗位</param>
        private void BanChangeAgencyPost(string isAgency)
        {
            if (isAgency == "1")
            {
                lkPost.IsEnabled = false;
                cbxPostLevel.IsEnabled = false;
                cbxPostLevel.SelectedItem = null;
                pgCaution.Text = "兼职岗位不能异动,请重新选择";
                pgCaution.Visibility = Visibility.Visible;
            }
            else
            {
                pgCaution.Text = "主岗位异动";
                pgCaution.Visibility = Visibility;
                lkPost.IsEnabled = true;
                cbxPostLevel.IsEnabled = true;
                pgCaution.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Add     :luojie
        /// Date    :2012-11-07
        /// Forwhat :禁止外部异动的提醒信息
        /// </summary>
        /// <param name="isPostChange"></param>
        private void BanOuterPostChange(bool isOuter)
        {
            if (isOuter)
            {
                cbxPostLevel.IsEnabled = false;
                cbxPostLevel.SelectedItem = null;
                pgCaution.Text = "现已禁止进行主岗位的外部异动";
                pgCaution.Visibility = Visibility;
            }
            else
            {
                cbxPostLevel.IsEnabled = true;
                pgCaution.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Added by 罗捷
        ///     检查岗位异动的异动后岗位是否已在员工已有岗位中
        /// </summary>
        /// <returns>当返回大于0，即存在这样的数据，0为不存在，-1为RepeatPostChecker没有值</returns>
        private int GetCheckerCount()
        {
            int count = -1;
            if (RepeatPostChecker != null && RepeatPostChecker.Count()>0)
            {
                //只变岗位级别的情况
                if (PostChange.FROMPOSTID==PostChange.TOPOSTID && chkIsAgency.IsChecked==false)
                {
                    var Checker = from c in RepeatPostChecker
                                  where c.T_HR_POST.POSTID == PostChange.TOPOSTID && c.EDITSTATE == "1" && c.CHECKSTATE == "2"
                                  && c.POSTLEVEL == postChange.TOPOSTLEVEL
                                  select c;
                    count = Checker.Count();
                    if (count > 0)
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("该员工已在此岗位且级别未变，不能异动"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
                //其他情况，什么也别说了，不让异动
                else
                {
                    var Checker = from c in RepeatPostChecker
                                  where c.T_HR_POST.POSTID == PostChange.TOPOSTID && c.EDITSTATE == "1" && c.CHECKSTATE == "2"
                                  select c;
                    count = Checker.Count();
                    if (count > 0)
                    {
                        RefreshUI(RefreshedTypes.HideProgressBar);
                        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("该员工已在此岗位任职,不能异动"),
                             Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    }
                }
            }
            return count;
        }

        private void DisableOldPost(string employeeId, string postId)
        {
            List<ExtOrgObj> ent = new List<ExtOrgObj>();
        }

        #region  无用
        private void HandleEmployeeChanged(SMT.Saas.Tools.OrganizationWS.V_EMPLOYEEPOST ent)
        {
            //给员工赋值
            T_HR_EMPLOYEE temp = new T_HR_EMPLOYEE();
            temp.EMPLOYEEID = ent.T_HR_EMPLOYEE.EMPLOYEEID;
            PostChange.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
            postChange.T_HR_EMPLOYEE.EMPLOYEEID = temp.EMPLOYEEID;

            if (ent.EMPLOYEEPOSTS[0] != null)
            {
                txtFromCompanyID.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.CNAME;
                txtFromDepartmentID.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_DEPARTMENTDICTIONARY.DEPARTMENTNAME; ;
                txtFromPostID.Text = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_POSTDICTIONARY.POSTNAME;

                PostChange.FROMCOMPANYID = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.T_HR_COMPANY.COMPANYID;
                PostChange.FROMDEPARTMENTID = ent.EMPLOYEEPOSTS[0].T_HR_POST.T_HR_DEPARTMENT.DEPARTMENTID;
                PostChange.FROMPOSTID = ent.EMPLOYEEPOSTS[0].T_HR_POST.POSTID;
                // PostChange.FROMPOSTLEVEL = en
            }
            else
            {
                txtFromCompanyID.Text = "";
                txtFromDepartmentID.Text = "";
                txtFromPostID.Text = "";
                PostChange.FROMCOMPANYID = "";
                PostChange.FROMDEPARTMENTID = "";
                PostChange.FROMPOSTID = "";
            }
        }
        #endregion
        void SaveChange()
        {
            if (isMainPostChanged == true)
            {
                PostChange.ISAGENCY = "3";//用此表示为主兼职岗位互换
            }
            
            string strMsg = string.Empty;
            if (FormType == FormTypes.New)
            {
                //所属

                PostChange.CREATECOMPANYID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].CompanyID;
                PostChange.CREATEDEPARTMENTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].DepartmentID;
                PostChange.CREATEPOSTID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserPosts[0].PostID;


                epost.CREATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

                client.EmployeePostChangeAddAsync(PostChange, strMsg);
                needsubmit = false;
                isSubmit = false;
            }
            else
            {

                PostChange.UPDATEDATE = System.DateTime.Now;
                PostChange.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID; ;
                epost.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                client.EmployeePostChangeUpdateAsync(PostChange, strMsg, "Edit");

            }
        }

        private void GridPagerThing_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GridPagerMoney_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chkIsAgency_Click(object sender, RoutedEventArgs e)
        {
            //ShowDtBorrowMoney();
            if(!(pgCaution.Text == "兼职岗位不能异动，请重新选择" && pgCaution.Visibility==Visibility.Visible))
                ShowOuterPostchangeState();
        }

        /// <summary>
        /// 判断现在外部异动的状态
        /// </summary>
        private void ShowOuterPostchangeState()
        {
            if (PostChange != null && PostChange.FROMCOMPANYID != null && PostChange.TOCOMPANYID != null
                && PostChange.FROMCOMPANYID != PostChange.TOCOMPANYID)
                cbPostCategory.SelectedValue = "1";

            if (chkIsAgency.IsChecked == false && cbPostCategory.SelectedValue == "1")
            {
                BanOuterPostChange(true);
            }
            else if(cbPostCategory.SelectedValue!=null)
            {
                BanOuterPostChange(false);
            }
        }

        private void Client_CheckBusinesstripCompleted(object sender, CheckBusinesstripCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null && string.IsNullOrWhiteSpace(e.Error.Message))
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, "错误", Utility.GetResourceStr("获取出差信息出错"));
                    return;
                }
                if (e.Result != null)
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    Dictionary<string, string> BusinesstripInfo = e.Result;
                    string result = "未提交或未审核的出差申请和出差报销\r\n";
                    foreach (var r in BusinesstripInfo)
                    {
                        if (r.Key.IndexOf("BUSINESSTRIP") >= 0)
                        {
                            result += string.Format("出差申请:{0};\r\n", r.Value);
                        }
                        else
                        {
                            result += string.Format("出差报销:{0};\r\n", r.Value);
                        }
                    }
                    Utility.ShowCustomMessage(MessageTypes.Message, "提醒", result);
                }
                else
                {
                    //使异动继续进行
                    SaveChange();
                }
            }
            catch (Exception)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", Utility.GetResourceStr("解析出差信息出错"));
            }
        }

        private bool BanOuterPostChange()
        {
            bool CanGoOn = true;
            if (PostChange.POSTCHANGCATEGORY!=null && PostChange.POSTCHANGCATEGORY=="1")
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                Utility.ShowCustomMessage(MessageTypes.Caution, "提醒", Utility.GetResourceStr("禁止进行外部异动"));
                CanGoOn = false;
            }
            return CanGoOn;
        }
    }
}
