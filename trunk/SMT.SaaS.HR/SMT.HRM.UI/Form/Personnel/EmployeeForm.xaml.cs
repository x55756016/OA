namespace SMT.HRM.UI.Form.Personnel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using SMT.Saas.Tools.PermissionWS;
    using SMT.Saas.Tools.PersonnelWS;
    using SMT.Saas.Tools.SalaryWS;
    using SMT.SaaS.FrameworkUI;
    using SMT.SaaS.FrameworkUI.ChildWidow;
    using SMT.SAAS.Main.CurrentContext;
    
    /// <summary>
    ///员工个人档案
    /// </summary>
    public partial class EmployeeForm : BaseForm, IEntityEditor, IClient
    {
        /// <summary>
        /// The loaded employee information.
        /// </summary>
        private T_HR_EMPLOYEE Employee { get; set; }

        /// <summary>
        /// The loaded system user information.
        /// </summary>
        private T_SYS_USER SysUser { get; set; }

        /// <summary>
        /// Service of Attendance.
        /// </summary>
        private SMT.Saas.Tools.AttendanceWS.T_HR_ATTENDANCESOLUTION AttendanceSolution { get; set; }

        /// <summary>
        /// Information of insurance.
        /// </summary>
        private List<T_HR_EMPLOYEEINSURANCE> ListEmployeeInsurance { get; set; }

        /// <summary>
        /// Information of Salary.
        /// </summary>
        private T_HR_SALARYARCHIVE SalaryArchive { get; set; }

        /// <summary>
        /// Information of Contract.
        /// </summary>
        private List<T_HR_EMPLOYEECONTRACT> ListContract { get; set; }

        /// <summary>
        /// Information of Post.
        /// </summary>
        private List<T_HR_EMPLOYEEPOST> ListPost { get; set; }

        ///// <summary>
        ///// Information of post change.
        ///// </summary>
        private List<V_EMPLOYEEPOSTCHANGE> ListPostchange { get; set; }

        /// <summary>
        /// Record the current educate history.
        /// </summary>
        private List<T_HR_EDUCATEHISTORY> ListEdu { get; set; } 

        /// <summary>
        /// Record the educate history before.
        /// </summary>
        private List<T_HR_EDUCATEHISTORY> OldEdu { get; set; } 

        /// <summary>
        /// Record the current word history.
        /// </summary>
        private List<T_HR_EXPERIENCE> ListExp { get; set; }

        /// <summary>
        /// Record the work history before.
        /// </summary>
        private List<T_HR_EXPERIENCE> OldExp { get; set; }

        /// <summary>
        /// About pension.
        /// </summary>
        private T_HR_PENSIONMASTER Pesion { get; set; }

        /// <summary>
        /// Resume-which is needed by education and jobs.
        /// </summary>
        private T_HR_RESUME Resume { get; set; }

        /// <summary>
        /// Record the state of this form.
        /// </summary>
        public FormTypes FormType { get; set; }

        /// <summary>
        /// Client for Personnel.
        /// </summary>
        private PersonnelServiceClient client = new PersonnelServiceClient();

        /// <summary>
        /// Useless.
        /// </summary>
        private string CompanyID { get; set; }

        /// <summary>
        /// Owner ID of this Form.
        /// </summary>
        private string Employeeid { get; set; }

        /// <summary>
        /// Client for AttendanceService.
        /// </summary>
        private SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient clientAtt = new SMT.Saas.Tools.AttendanceWS.AttendanceServiceClient();
        /// <summary>
        /// Client for Permission Service.
        /// </summary>
        private SMT.Saas.Tools.PermissionWS.PermissionServiceClient permClient = new SMT.Saas.Tools.PermissionWS.PermissionServiceClient();
        /// <summary>
        /// Client for Salary Service.
        /// </summary>
        private SalaryServiceClient salaryClient = new SalaryServiceClient();
        // private SmtOADocumentAdminWS.SmtOADocumentAdminClient OAClient = new SMT.HRM.UI.SmtOADocumentAdminWS.SmtOADocumentAdminClient();
        /// <summary>
        /// Should close this form. false means not close.
        /// </summary>
        private bool closeFormFlag = false;
        private SMTLoading loadbar = new SMTLoading();

        /// <summary>
        /// The default initialization of this form.
        /// </summary>
        public EmployeeForm()
        {
            
            //Employee.SOCIALSERVICEYEAR=
        }
        /// <summary>
        /// Real usable initialization.
        /// </summary>
        /// <param name="type">FormTypes</param>
        /// <param name="employeeID">ID of Employee</param>
        public EmployeeForm(FormTypes type, string employeeID)
        {
            this.InitializeComponent();

            this.FormType = type;
            this.Employeeid = employeeID;
            this.Loaded += (sender, args) =>
            {
                InitPara(employeeID);
            };
            //InitPara(employeeID);
        }

        #region IEntityEditor
        /// <summary>
        /// Get the title of this Form.
        /// </summary>
        /// <returns>Title's name.</returns>
        public string GetTitle()
        {
            return Utility.GetResourceStr("EMPLOYEE");
        }
        /// <summary>
        /// Get the status of this form.
        /// </summary>
        /// <returns>Status.</returns>
        public string GetStatus()
        {
            return Employee != null ? Employee.EMPLOYEESTATE : string.Empty;
        }
        /// <summary>
        /// If do the close action.
        /// </summary>
        /// <param name="actionType"></param>
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
            }
        }
        /// <summary>
        /// Left layout.
        /// </summary>
        /// <returns>左栏项</returns>
        public List<NavigateItem> GetLeftMenuItems()
        {
            List<NavigateItem> items = new List<NavigateItem>();
            NavigateItem item = new NavigateItem
            {
                Title = Utility.GetResourceStr("DETAILINFO"),
                Tooltip = Utility.GetResourceStr("DETAILINFO")
            };
            items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("EMPLOYEEENTRY"),
            //    Tooltip = Utility.GetResourceStr("EMPLOYEECONTRACT"),
            //    Url = "/Personnel/EmployeeEntry.xaml"
            //};
            //items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("EMPLOYEECONTRACT"),
            //    Tooltip = Utility.GetResourceStr("EMPLOYEECONTRACT"),
            //    Url = "/Personnel/EmployeeContract.xaml"
            //};
            //items.Add(item);
            //item = new NavigateItem
            //{
            //    Title = Utility.GetResourceStr("WELFAREPAYMENTDETAILS"),
            //    Tooltip = Utility.GetResourceStr("WELFAREPAYMENTDETAILS"),
            //    Url = "/Personnel/WelfarePaymentDetails.xaml"
            //};
            //items.Add(item);
            return items;
        }
        /// <summary>
        /// Layout of toolbar:add,alter,delete and so on.
        /// </summary>
        /// <returns>工具栏项</returns>
        public List<ToolbarItem> GetToolBarItems()
        {
            List<ToolbarItem> items = new List<ToolbarItem>();
            if (FormType != FormTypes.Browse)
            {
                items = Utility.CreateFormSaveButton();
            }

            return items;
        }
        /// <summary>
        /// 刷新页面事件
        /// </summary>
        public event UIRefreshedHandler OnUIRefreshed;
        /// <summary>
        /// Function of Refresh this form.
        /// </summary>
        /// <param name="type"></param>
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
        /// <summary>
        /// 释放各种服务资源
        /// </summary>
        public void ClosedWCFClient()
        {
            // throw new NotImplementedException();
            client.DoClose();
            clientAtt.DoClose();
            permClient.DoClose();
            salaryClient.DoClose();
        }
        /// <summary>
        /// 检查菜单是否变化
        /// </summary>
        /// <returns>True变化</returns>
        public bool CheckDataContenxChange()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 用处未知
        /// </summary>
        /// <param name="entity"></param>
        public void SetOldEntity(object entity)
        {
            throw new NotImplementedException();
        }
        #endregion
        /// <summary>
        /// 初始化-订阅各种事件
        /// </summary>
        /// <param name="employeeID">用户ID</param>
        private void InitPara(string employeeID)
        {
            LayoutRoot.Children.Add(loadbar);
            client.GetEmployeeByIDCompleted += new EventHandler<GetEmployeeByIDCompletedEventArgs>(this.Client_GetEmployeeByIDCompleted);
            client.EmployeeUpdateCompleted += new EventHandler<EmployeeUpdateCompletedEventArgs>(this.Client_EmployeeUpdateCompleted);
            //考勤方案
            clientAtt.GetAttendanceSolutionByEmployeeIDCompleted += new EventHandler<SMT.Saas.Tools.AttendanceWS.GetAttendanceSolutionByEmployeeIDCompletedEventArgs>(ClientAtt_GetAttendanceSolutionByEmployeeIDCompleted);
            //异动
            client.EmployeePostChangeViewPagingCompleted += new EventHandler<EmployeePostChangeViewPagingCompletedEventArgs>(this.Client_EmployeePostChangeViewPagingCompleted);

            //福利发放标准
            // OAClient.GetWelfareStandardByIdCompleted += new EventHandler<SMT.HRM.UI.SmtOADocumentAdminWS.GetWelfareStandardByIdCompletedEventArgs>(OAthis.Client_GetWelfareStandardByIdCompleted);
            //保险
            client.EmployeeInsurancePagingCompleted += new EventHandler<EmployeeInsurancePagingCompletedEventArgs>(this.Client_EmployeeInsurancePagingCompleted);
            //合同
            client.EmployeeContractPagingCompleted += new EventHandler<EmployeeContractPagingCompletedEventArgs>(this.Client_EmployeeContractPagingCompleted);
            //社保档案
            //  client.PensionMasterPagingCompleted += new EventHandler<PensionMasterPagingCompletedEventArgs>(this.Client_PensionMasterPagingCompleted);
            client.GetPensionMasterByEmployeeIDCompleted += new EventHandler<GetPensionMasterByEmployeeIDCompletedEventArgs>(this.Client_GetPensionMasterByEmployeeIDCompleted);
            //岗位
            client.GetPostsActivedByEmployeeIDCompleted += new EventHandler<GetPostsActivedByEmployeeIDCompletedEventArgs>(this.Client_GetPostsActivedByEmployeeIDCompleted);
            // 简历
            client.GetResumeByNumberCompleted += new EventHandler<GetResumeByNumberCompletedEventArgs>(this.Client_GetResumeByNumberCompleted);
            client.ExperienceUpdateCompleted += new EventHandler<ExperienceUpdateCompletedEventArgs>(this.Client_ExperienceUpdateCompleted);
            client.ExperienceDeleteCompleted += new EventHandler<ExperienceDeleteCompletedEventArgs>(this.Client_ExperienceDeleteCompleted);
            client.EducateHistoryUpdateCompleted += new EventHandler<EducateHistoryUpdateCompletedEventArgs>(this.Client_EducateHistoryUpdateCompleted);
            client.EducateHistoryDeleteCompleted+=new EventHandler<EducateHistoryDeleteCompletedEventArgs>(this.Client_EducateHistoryDeleteCompleted);

            client.IsExistFingerPrintIDCompleted += new EventHandler<IsExistFingerPrintIDCompletedEventArgs>(this.Client_IsExistFingerPrintIDCompleted);
            //获取工作和教育经历
            client.GetExperienceAllCompleted += new EventHandler<GetExperienceAllCompletedEventArgs>(this.Client_GetExperienceAllCompleted);
            client.GetEducateHistoryAllCompleted += new EventHandler<GetEducateHistoryAllCompletedEventArgs>(this.Client_GetEducateHistoryAllCompleted);
            //修改用户名
            permClient.SysUserInfoAddORUpdateCompleted += new EventHandler<SysUserInfoAddORUpdateCompletedEventArgs>(this.PermClient_SysUserInfoAddORUpdateCompleted);
            permClient.GetUserByEmployeeIDCompleted += new EventHandler<GetUserByEmployeeIDCompletedEventArgs>(this.PermClient_GetUserByEmployeeIDCompleted);
            //permClient.GetSysDictionaryByCategoryCompleted += new EventHandler<SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs>(permissionClient_GetSysDictionaryByCategoryCompleted);
            //获取字典中的岗位级别,一次性加载系统字典
          //permClient.GetSysDictionaryByCategoryAsync("POSTLEVEL");
            permClient.GetUserByEmployeeIDAsync(employeeID);
            client.GetEmployeeByIDAsync(employeeID);
            loadbar.Start();
            if (FormType == FormTypes.Browse)
            {
                DisableControls();
            }
        }

        #region 完成事件
        //获取岗位级别字典
        //void permissionClient_GetSysDictionaryByCategoryCompleted(object sender, SMT.Saas.Tools.PermissionWS.GetSysDictionaryByCategoryCompletedEventArgs e)
        //{
        //    if (e.Error != null && !string.IsNullOrEmpty(e.Error.Message))
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
        //    }
        //    else
        //    {
        //        List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
        //        dicts = e.Result == null ? null : e.Result.ToList();
        //        Application.Current.Resources.Add("POSTLEVEL_DICTIONARY", dicts);
        //    }
        //}

        /// <summary>
        /// 判断员工指纹编号是否重复
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_IsExistFingerPrintIDCompleted(object sender, IsExistFingerPrintIDCompletedEventArgs e)
        {
            string strMsg = string.Empty;
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == true)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("FINGERPRINTIDEXIST"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("FINGERPRINTIDEXIST"),
                Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                }
                else
                {
                    if (e.UserState.ToString() == "Save")
                    {
                        client.EmployeeUpdateAsync(Employee, CompanyID, strMsg);
                    }
                }
            }
        }

        /// <summary>
        /// 简历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_GetResumeByNumberCompleted(object sender, GetResumeByNumberCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    Resume = e.Result;
                    if (e.UserState.ToString() == "Edu")
                    {
                        client.GetEducateHistoryAllAsync(Resume.RESUMEID);
                    }
                    else
                    {
                        client.GetExperienceAllAsync(Resume.RESUMEID);
                    }
                }
                else
                {
                    Resume = new T_HR_RESUME();
                    Resume.RESUMEID = System.Guid.NewGuid().ToString();
                    Resume.IDCARDNUMBER = Employee.IDNUMBER;
                    Resume.NAME = Employee.EMPLOYEECNAME;
                    ObservableCollection<T_HR_EDUCATEHISTORY> colEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();
                    ObservableCollection<T_HR_EXPERIENCE> colExp = new ObservableCollection<T_HR_EXPERIENCE>();
                    client.ResumeAddAsync(Resume, colExp, colEdu, "RESUME");
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        private void Client_ExperienceUpdateCompleted(object sender,ExperienceUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result != null && e.Result >= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "消息", "修改成功");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, "错误", "修改失败");
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        private void Client_EducateHistoryUpdateCompleted(object sender, EducateHistoryUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result != null && e.Result >= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "消息", "修改成功");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, "错误", "修改失败");
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        private void Client_EducateHistoryDeleteCompleted(object sender, EducateHistoryDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result != null && e.Result >= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "消息", "删除成功");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, "错误", "删除出错");
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        private void Client_ExperienceDeleteCompleted(object sender, ExperienceDeleteCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (e.Result != null && e.Result >= 0)
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, "消息", "删除成功");
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Error, "错误", "删除出错");
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        void Client_GetEducateHistoryAllCompleted(object sender, GetEducateHistoryAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                DtGridEdu.ItemsSource = e.Result;
                if (e.Result != null)
                {
                    OldEdu = e.Result.ToList();
                }
            }
        }
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="sender">对象</param>
        /// <param name="e">事件</param>
        void Client_GetExperienceAllCompleted(object sender, GetExperienceAllCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                DtGridExp.ItemsSource = e.Result;
                if (e.Result != null)
                {
                    OldExp = e.Result.ToList();
                }
            }
        }
        ///获取所有有效岗位
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_GetPostsActivedByEmployeeIDCompleted(object sender, GetPostsActivedByEmployeeIDCompletedEventArgs e)
        {
            ListPost = null;
              if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    ListPost = e.Result.ToList();
                    foreach (T_HR_EMPLOYEEPOST item in ListPost)
                    {
                        if (item.ISAGENCY == "1")
                        {
                            item.ISAGENCY = Utility.GetResourceStr("ISAGENCY");
                        }
                        else
                        {
                            item.ISAGENCY = string.Empty;
                        }
                    }
                }
                DtPostGrid.ItemsSource = ListPost;
            }
        }
        /// <summary>
        /// Callback:Search the info of Post.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_EmployeePostChangeViewPagingCompleted(object sender, EmployeePostChangeViewPagingCompletedEventArgs e)
        {
            ListPostchange = null;
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    ListPostchange = e.Result.ToList();
                    //foreach (V_EMPLOYEEPOSTCHANGE item in ListPostchange)
                    //{
                    //    if (item.EMPLOYEEPOSTCHANGE.ISAGENCY == "1")
                    //    {
                    //        item.EMPLOYEEPOSTCHANGE.ISAGENCY = Utility.GetResourceStr("ISAGENCY");
                    //    }
                    //    else
                    //    {
                    //        item.EMPLOYEEPOSTCHANGE.ISAGENCY = string.Empty;
                    //    }

                    //}
                }
                DtPostChange.ItemsSource = ListPostchange;
                dataPager.PageCount = e.pageCount;
            }
        }


        /// <summary>
        /// 社保
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_GetPensionMasterByEmployeeIDCompleted(object sender, GetPensionMasterByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    Pesion = e.Result;
                    PensionGrid.DataContext = Pesion;
                    //缴交时间不用这里读取，从员工个人档案里面的缴交日期读取
                    //DateTime dt = new DateTime();
                    //if (!string.IsNullOrEmpty(Pesion.SOCIALSERVICEYEAR))
                    //{
                    //    DateTime.TryParse(Pesion.SOCIALSERVICEYEAR, out dt);
                    //    dtpStartWorkTime.Text = dt.ToShortDateString();
                    //}
                    //else
                    //{
                    //    dtpStartWorkTime.Text = string.Empty;
                    //}
                }
            }
            BindData();
        }
        //void Client_PensionMasterPagingCompleted(object sender, PensionMasterPagingCompletedEventArgs e)
        //{
        //    List<T_HR_PENSIONMASTER> list = new List<T_HR_PENSIONMASTER>();
        //    if (e.Error != null && e.Error.Message != string.Empty)
        //    {
        //         ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"),Utility.GetResourceStr("CONFIRM"), MessageIcon.Error); 
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            list = e.Result.ToList();
        //            tabPension.DataContext = list[0];
        //        }
        //        //DtGrid.ItemsSource = list;
        //        //dataPager.PageCount = e.pageCount;
        //    }
        //}
        /// <summary>
        /// 合同
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_EmployeeContractPagingCompleted(object sender, EmployeeContractPagingCompletedEventArgs e)
        {
            ListContract = null;
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    ListContract = e.Result.Where(s => s.CHECKSTATE == "2").ToList();//条件为2，审核通过
                }
                DtGrid.ItemsSource = ListContract;
                dataPagerContract.PageCount = e.pageCount;
            }
        }
        /// <summary>
        /// 保险
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_EmployeeInsurancePagingCompleted(object sender, EmployeeInsurancePagingCompletedEventArgs e)
        {
            ListEmployeeInsurance = null;
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    ListEmployeeInsurance = e.Result.ToList();
                }
                DtInsurance.ItemsSource = ListEmployeeInsurance;
                dataPagerInsurance.PageCount = e.pageCount; ;
            }
        }

        //void OAClient_GetWelfareStandardByIdCompleted(object sender, SMT.HRM.UI.SmtOADocumentAdminWS.GetWelfareStandardByIdCompletedEventArgs e)
        //{
        //    if (e.Error != null && e.Error.Message != string.Empty)
        //    {
        //        ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
        //    }
        //    else
        //    {
        //        if (e.Result != null)
        //        {
        //            //DtWelfares.ItemsSource = e.Result;
        //        }
        //    }
        //}

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_EmployeeUpdateCompleted(object sender, EmployeeUpdateCompletedEventArgs e)
        {

            if (e.Error != null && e.Error.Message != string.Empty)
            {
                if (e.Error.Message == "Repetition")
                {
                    // Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("REPETITIONTWOPARAS", "EMPLOYEECODE"));
                }
                else
                {
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                }
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                //Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"));
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("MODIFYSUCCESSED"),
            Utility.GetResourceStr("CONFIRM"), MessageIcon.Information);
                RefreshUI(RefreshedTypes.HideProgressBar);
                if (closeFormFlag)
                {
                    RefreshUI(RefreshedTypes.Close);
                }
                RefreshUI(RefreshedTypes.All);
            }
        }
        /// <summary>
        /// 根据员工ID 获取用户信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PermClient_GetUserByEmployeeIDCompleted(object sender, GetUserByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                SysUser = e.Result;
            }
        }
        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PermClient_SysUserInfoAddORUpdateCompleted(object sender, SysUserInfoAddORUpdateCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                RefreshUI(RefreshedTypes.HideProgressBar);
            }
            else
            {
                if (!string.IsNullOrEmpty(e.strMsg))
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("USERNAMEREPETION"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("USERNAMEREPETION"),
                       Utility.GetResourceStr("CONFIRM"), MessageIcon.Exclamation);
                    RefreshUI(RefreshedTypes.HideProgressBar);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Employee.FINGERPRINTID))
                    {
                        client.IsExistFingerPrintIDAsync(Employee.FINGERPRINTID, Employee.EMPLOYEEID, "Save");
                    }
                    else
                    {
                        string strMsg = string.Empty;
                        client.EmployeeUpdateAsync(Employee, CompanyID, strMsg);
                    }
                }
            }
        }

        /// <summary>
        /// 查询员工基本信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Client_GetEmployeeByIDCompleted(object sender, GetEmployeeByIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                if (e.Result == null)
                {
                    //Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"));
                    ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("NOTFOUND"),
               Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
                    return;
                }
                Employee = e.Result;
                if (Employee.HASCHILDREN == "1")
                {
                    chkHas.IsChecked = true;
                }
                else
                {
                    chkHas.IsChecked = false;
                }
                //experience.btnAdd.Visibility = Visibility.Collapsed;
                //educateHistory.btnAdd.Visibility = Visibility.Collapsed;
                //if (Employee.T_HR_RESUME != null)
                //{
                //    experience.LoadData(FormTypes.Browse, Employee.T_HR_RESUME.RESUMEID, Employee.T_HR_RESUME);
                //    educateHistory.LoadData(FormTypes.Browse, Employee.T_HR_RESUME.RESUMEID, Employee.T_HR_RESUME);
                //}
                client.GetPensionMasterByEmployeeIDAsync(Employee.EMPLOYEEID); //员工社保档案              
            }
            loadbar.Stop();
        }
        /// <summary>
        /// 查询考勤方案
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClientAtt_GetAttendanceSolutionByEmployeeIDCompleted(object sender, SMT.Saas.Tools.AttendanceWS.GetAttendanceSolutionByEmployeeIDCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != string.Empty)
            {
                ComfirmWindow.ConfirmationBoxs(Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("ERRORINFO"), Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            else
            {
                AttendanceSolution = e.Result;
                LoadAttendanceSolution();
            }
        }
        #endregion
        /// <summary>
        /// 数据绑定
        /// </summary>
        private void BindData()
        {
            tbcContainer.DataContext = Employee;
        }
        /// <summary>
        /// 绑定考勤方案
        /// </summary>
        private void LoadAttendanceSolution()
        {
            cbxkAttendanceType.IsEnabled = false;
            cbxkCardType.IsEnabled = false;
            cbxkWorkDayType.IsEnabled = false;
            cbxkAnnualLeaveSet.IsEnabled = false;
            btnBrowseAttSol.Visibility = Visibility.Visible;
            txtAttSolName.IsEnabled = false;
            txtAttSolWorkMode.IsEnabled = false;
            txtWorkTime.IsEnabled = false;

            if (AttendanceSolution == null)
            {
                btnBrowseAttSol.Visibility = Visibility.Collapsed;
                return;
            }

            txtAttSolName.Text = AttendanceSolution.ATTENDANCESOLUTIONNAME;
            btnBrowseAttSol.Tag = AttendanceSolution.ATTENDANCESOLUTIONID;
            txtAttSolWorkMode.Text = AttendanceSolution.WORKMODE.ToString();
            txtWorkTime.Text = AttendanceSolution.WORKTIMEPERDAY.ToString();

            SetComboBoxSelectItem(cbxkAttendanceType, AttendanceSolution.ATTENDANCETYPE);
            SetComboBoxSelectItem(cbxkCardType, AttendanceSolution.CARDTYPE);
            SetComboBoxSelectItem(cbxkWorkDayType, AttendanceSolution.WORKDAYTYPE);
            SetComboBoxSelectItem(cbxkAnnualLeaveSet, AttendanceSolution.ANNUALLEAVESET);
        }

        /// <summary>
        /// 设置ComboBox选取项
        /// </summary>
        /// <param name="cbxk">指定的ComboBox</param>
        /// <param name="strItemValue">选取项的值</param>
        private void SetComboBoxSelectItem(SMT.HRM.UI.AppControl.DictionaryComboBox cbxk, string strItemValue)
        {
            if (cbxk.Items.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < cbxk.Items.Count; i++)
            {
                SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY entDic = cbxk.Items[i] as SMT.Saas.Tools.PermissionWS.T_SYS_DICTIONARY;

                if (entDic.DICTIONARYVALUE.ToString() == strItemValue)
                {
                    cbxk.SelectedIndex = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 员工个人档案保存
        /// </summary>
        /// <returns>always true</returns>
        private bool Save()
        {
            if (string.IsNullOrEmpty(Employee.SOCIALSERVICEYEAR))
            {
                Utility.ShowCustomMessage(MessageTypes.Caution, Utility.GetResourceStr("CAUTION"), Utility.GetResourceStr("请输入员工社保缴纳起始时间"));
                return false;
            }
            Employee.SOCIALSERVICEYEAR = Convert.ToDateTime(Employee.SOCIALSERVICEYEAR).ToString("yyyy-MM-dd");
            //缴交日期字段采用员工个人档案表里面的字段SOCIALSERVICEYEAR
            //if (Pesion != null)
            //{                
            //    Pesion.SOCIALSERVICEYEAR = dtpStartWorkTime.SelectedDate.Value.ToShortDateString();
            //    client.PensionMasterUpdateAsync(Pesion);
            //}
            //else
            //{
            //    Pesion = new T_HR_PENSIONMASTER();
            //    Pesion.PENSIONMASTERID = Guid.NewGuid().ToString();
            //    Pesion.OWNERID = Employee.EMPLOYEEID;
            //    Pesion.T_HR_EMPLOYEE = new T_HR_EMPLOYEE();
            //    Pesion.T_HR_EMPLOYEE.EMPLOYEEID = Employee.EMPLOYEEID;

            //    //Pesion.OWNERPOSTID = Employee;
            //    Pesion.OWNERDEPARTMENTID = Employee.OWNERDEPARTMENTID;
            //    Pesion.OWNERCOMPANYID = Employee.OWNERCOMPANYID;
            //    Pesion.CHECKSTATE = "2";
            //    Pesion.REMARK = "员工个人档案修改工作年限时新增";
            //    Pesion.ISVALID = "1";
            //    Pesion.SOCIALSERVICEYEAR = dtpStartWorkTime.SelectedDate.Value.ToShortDateString();
            //    string str = string.Empty; ;
            //    client.PensionMasterAddAsync(Pesion, str);
            //}
            RefreshUI(RefreshedTypes.ShowProgressBar);
            string strMsg = string.Empty;
            if (!SMT.SaaS.FrameworkUI.Common.Utility.CheckDataIsValid(Group1))
            {
                //  RefreshUI(RefreshedTypes.HideProgressBar);
                RefreshUI(RefreshedTypes.HideProgressBar);
                return false;
            }
            else
            {
                if (chkHas.IsChecked.HasValue && chkHas.IsChecked.Value == true)
                {
                    Employee.HASCHILDREN = "1";
                }
                else
                {
                    Employee.HASCHILDREN = "0";
                }
                //Employee.SOCIALSERVICEYEAR= dtpStartWorkTime.SelectedDate.Value.ToShortDateString();
                Employee.UPDATEUSERID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                Employee.UPDATEDATE = DateTime.Now;
                //if (Employee.EMPLOYEEENAME != sysUser.USERNAME)
                //{
                string mes = string.Empty;
                SysUser.USERNAME = Employee.EMPLOYEEENAME;
                permClient.SysUserInfoAddORUpdateAsync(SysUser, mes);
                //}
                //else
                //{
                //    if (!string.IsNullOrEmpty(Employee.FINGERPRINTID))
                //    {
                //        client.IsExistFingerPrintIDAsync(Employee.FINGERPRINTID, Employee.EMPLOYEEID, "Save");
                //    }
                //    else
                //    {
                //        client.EmployeeUpdateAsync(Employee, CompanyID, strMsg);
                //    }
                //}
            }
            return true;
        }

        /// <summary>
        /// 查看考勤方案详细
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBrowseAttSol_Click(object sender, RoutedEventArgs e)
        {
            if (AttendanceSolution == null)
            {
                return;
            }

            string strAttendanceSolutionID = AttendanceSolution.ATTENDANCESOLUTIONID;
            SMT.HRM.UI.Form.Attendance.AttSolRdForm formAttSolRd = new SMT.HRM.UI.Form.Attendance.AttSolRdForm(FormTypes.Browse, strAttendanceSolutionID);
            EntityBrowser browser = new EntityBrowser(formAttSolRd);
          
            browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, string.Empty, (result) => { });
        }
        #region  无用
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
        /// <summary>
        /// useless
        /// </summary>
        /// <param name="sender">null</param>
        /// <param name="e">null</param>
        private void GridPager_Click(object sender, RoutedEventArgs e)
        {
        }
        /// <summary>
        /// useless
        /// </summary>
        /// <param name="sender">null</param>
        /// <param name="e">null</param>
        private void GridPager_Click_1(object sender, RoutedEventArgs e)
        {
        }
        /// <summary>
        /// useless
        /// </summary>
        /// <param name="sender">null</param>
        /// <param name="e">null</param>
        private void GridPager_Click_2(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        #region 禁用控件
        /// <summary>
        /// 查看等模式时禁用控件不允许输入
        /// </summary>
        void DisableControls()
        {
            txtEmployeeCode.IsReadOnly = true;
            txtEmployeeName.IsReadOnly = true;
            txtEmployeeEName.IsReadOnly = true;
            txtBankCardNumber.IsReadOnly = true;
            txtBankID.IsReadOnly = true;
            cbxEmployeeSex.IsEnabled = false;
            imgSelect.IsEnabled = false;
            cbxNation.IsEnabled = false;
            dateBirthday.IsEnabled = false;
            cbxBloodType.IsEnabled = false;
            txtFingerPrintId.IsReadOnly = true;
            txtIDNumber.IsReadOnly = true;
            txtHeight.IsReadOnly = true;
            cbxSecondLanguage.IsEnabled = false;
            cbxSecondLanguageDegree.IsEnabled = false;
            cbxEmployeeState.IsEnabled = false;
            cbxProfessionalTitle.IsEnabled = false;
            cbxTopEducation.IsEnabled = false;
            cbxPolitics.IsEnabled = false;
            cbxIDType.IsEnabled = false;
            txtWorkingAge.IsReadOnly = true;

            cbxMarriage.IsEnabled = false;
            chkHas.IsEnabled = false;
            cbxProvince.IsEnabled = false;
            txtRegresidence.IsReadOnly = true;
            cbxResidenceType.IsEnabled = false;
            cbxRegresidenceType.IsEnabled = false;
            txtRemarkEmployeeInfo.IsReadOnly = true;

            txtOtherCommunicate.IsReadOnly = true;
            txtUrgencyPerson.IsReadOnly = true;
            txtUrgencyContact.IsReadOnly = true;
            txtOfficePhone.IsReadOnly = true;
            txtEmail.IsReadOnly = true;
            txtMobile.IsReadOnly = true;
            txtFamilyAddress.IsReadOnly = true;
            txtCurrentAddress.IsReadOnly = true;
            //txtName.IsEnabled = false;
            //txtPostSalary.IsEnabled = false;
            //txtSecurityallowance.IsEnabled = false;
            //txtHousingallowance.IsEnabled = false;
            //txtAreadifallowance.IsEnabled = false;
            //txtFoodallowance.IsEnabled = false;
            //txtOtheradddeduct.IsEnabled = false;
            //txtOtheradddeductdesc.IsEnabled = false;
            //txtPersonalsiratio.IsEnabled = false;
            //txtPersonalincomeratio.IsEnabled = false;
            //txtOthersubjoin.IsEnabled = false;
            //txtOthersubjoindesc.IsEnabled = false;
            //txtHousingallowancededuct.IsEnabled = false;
            //txtBankID.IsEnabled = false;
            //txtBankCardNumber.IsEnabled = false;

            //   DtWelfares.IsReadOnly = true;
            txtEName.IsReadOnly = true;
            txtCardID.IsReadOnly = true;
            txtComputerNO.IsReadOnly = true;
            txtCity.IsReadOnly = true;
            txtStartDate.IsReadOnly = true;
            txtLastDate.IsReadOnly = true;
            cbIsvalid.IsEnabled = false;
            txtRemark.IsReadOnly = true;

            DtInsurance.IsReadOnly = true;
            DtPostChange.IsReadOnly = true;
            DtGrid.IsReadOnly = true;

            btnAddEdu.IsEnabled = false;
            btnAddExp.IsEnabled = false;
        }
        #endregion

        #region 选项卡切换 加载数据
        /// <summary>
        ///加载考勤方案
        /// </summary>
        void LoadAttenanceSolution()
        {
            //if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Employee, "T_HR_ATTENDANCESOLUTION", SMT.SaaS.FrameworkUI.OperationType.Browse, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
            //{
            if (AttendanceSolution == null)
            {
                clientAtt.GetAttendanceSolutionByEmployeeIDAsync(Employeeid);
            }
            //}
        }
        /// <summary>
        /// 加载社保
        /// </summary>
        void LoadPensionMaster()
        {
           // if (Employee == null) return;
           // if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Employee, "T_HR_PENSIONMASTER", SMT.SaaS.FrameworkUI.OperationType.Browse, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
           // {
                if (Pesion == null)
                {
                    client.GetPensionMasterByEmployeeIDAsync(Employeeid);
                }
           // }
        }
        /// <summary>
        /// 保险
        /// </summary>
        /// <param name="employeeID"></param>
        void LoadInsurance(string employeeID)
        {
           // if (Employee == null) return;
          //  if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Employee, "T_HR_EMPLOYEEINSURANCE", SMT.SaaS.FrameworkUI.OperationType.Browse, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
           // {
                if (ListEmployeeInsurance == null)
                {
                    int pageCount = 0;
                    string filter = string.Empty;
                    string strState = string.Empty;
                    ObservableCollection<object> paras = new ObservableCollection<object>();
                    if (!string.IsNullOrEmpty(employeeID))
                    {

                        filter += "T_HR_EMPLOYEE.EMPLOYEEID==@" + paras.Count().ToString();
                        paras.Add(employeeID);
                    }
                    client.EmployeeInsurancePagingAsync(dataPagerInsurance.PageIndex, dataPagerInsurance.PageSize, "INSURANCENAME", filter,
                        paras, pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                }
           // }
        }
        /// <summary>
        /// 合同
        /// </summary>
        /// <param name="employeeID"></param>
        void LoadContract(string employeeID)
        {
           // if (Employee == null) return;
          //  if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Employee, "T_HR_EMPLOYEECONTRACT", SMT.SaaS.FrameworkUI.OperationType.Browse, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
          //  {
                if (ListContract == null)
                {
                    int pageCount = 0;
                    string filter = string.Empty;
                    string strState = string.Empty;
                    System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

                    if (!string.IsNullOrEmpty(employeeID))
                    {

                        filter += "T_HR_EMPLOYEE.EMPLOYEEID==@" + paras.Count().ToString();
                        paras.Add(employeeID);
                    }

                    client.EmployeeContractPagingAsync(dataPagerContract.PageIndex, dataPagerContract.PageSize, "T_HR_EMPLOYEE.EMPLOYEECODE", filter, paras,
                        pageCount, strState, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
                }
           // }
        }
        /// <summary>
        /// 员工异动
        /// </summary>
        /// <param name="employeeID"></param>
        void LoadPostChange(string employeeID)
        {
            //这个Employee加载不出来，本地可能加载，很奇怪，然后这里又进行了权限判断，也不知道为什么，注释掉，有问题再处理
           //if (Employee == null) return;
           // if (SMT.SaaS.FrameworkUI.Common.Utility.ToolBarButtonOperationPermission(Employee, "T_HR_EMPLOYEEPOSTCHANGE", SMT.SaaS.FrameworkUI.OperationType.Browse, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID))
           // {
                if (ListPostchange == null)
                {
                    int pageCount = 0;
                    string filter = string.Empty;
                    string strState = string.Empty;
                    System.Collections.ObjectModel.ObservableCollection<object> paras = new System.Collections.ObjectModel.ObservableCollection<object>();

                    if (!string.IsNullOrEmpty(employeeID))
                    {

                        filter += "EMPLOYEEPOSTCHANGE.T_HR_EMPLOYEE.EMPLOYEEID==@" + paras.Count().ToString();
                        paras.Add(employeeID);
                    }

                    client.EmployeePostChangeViewPagingAsync(dataPager.PageIndex, dataPager.PageSize, "EMPLOYEEPOSTCHANGE.CHANGEDATE", filter,
                        paras, pageCount, SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, strState);
                }
           // }
        }
        /// <summary>
        /// 加载岗位
        /// </summary>
        void LoadPost()
        {
            if (ListPost == null)
            {
                client.GetPostsActivedByEmployeeIDAsync(Employeeid);
            }
        }
        /// <summary>
        /// 加载简历
        /// </summary>
        /// <param name="type">简历类型</param>
        void LoadResume(string type)
        {
            if (Employee == null)
            {
                return;
            }
            if (Resume == null)
            {
                client.GetResumeByNumberAsync(Employee.IDNUMBER, type);
            }
            else
            {
                if (type == "Edu")
                {
                    if (ListEdu == null)
                    {
                        client.GetEducateHistoryAllAsync(Resume.RESUMEID);
                    }
                }
                else
                {
                    if (ListExp == null)
                    {
                        client.GetExperienceAllAsync(Resume.RESUMEID);
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// 各标签切换的动作
        /// </summary>
        /// <param name="sender">点击的标签对象</param>
        /// <param name="e">事件</param>
        private void TbcContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbcContainer != null)
            {
                switch (tbcContainer.SelectedIndex)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2: 
                        LoadPost();
                        LoadPostChange(Employeeid);
                        break;
                    case 3: 
                        LoadAttenanceSolution();//加载考勤方案
                        break;
                    case 4: LoadPensionMaster();
                        break;
                    case 5: LoadInsurance(Employeeid);
                        break;
                    case 6: LoadContract(Employeeid);
                        break;
                    case 7: LoadResume("Exp");
                        break;
                    case 8: LoadResume("Edu");
                        break;
                }
            }
        }
        /// <summary>
        /// 检查指纹ID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtFingerPrintId_LostFocus(object sender, RoutedEventArgs e)
        {
            string fid = (sender as TextBox).Text;
            if (!string.IsNullOrEmpty(fid))
            {
                client.IsExistFingerPrintIDAsync(fid, Employee.EMPLOYEEID, "Change");
            }
        }

        #region 修改工作教育经历 by luojie
        /// <summary>
        /// 按钮：新增
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddExp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //新增一个空的条目
                ObservableCollection<T_HR_EXPERIENCE> listExp = DtGridExp.ItemsSource as ObservableCollection<T_HR_EXPERIENCE>;
                if (listExp == null)
                {
                    listExp = new ObservableCollection<T_HR_EXPERIENCE>();
                }

                T_HR_EXPERIENCE newExp = new T_HR_EXPERIENCE();    
                listExp.Add(newExp);
                DtGridExp.ItemsSource = listExp;
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，新增工作经历出错，请重新打开页面或联系管理员。");
            }
        }

        /// <summary>
        /// 按钮：修改工作经历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnModifyExp_Click(object sender, RoutedEventArgs e)
        {
            Button btnEdit = sender as Button;
            if (btnEdit.Content.ToString() == "修改")
            {
                SetCurrentExp(sender);
                btnEdit.Content = "保存";
            }
            else
            {
                UpdateExp(sender);
                btnEdit.Content = "修改";
            }
        }

        /// <summary>
        /// 通过按钮对象设置使当前datatemplate可用
        /// </summary>
        /// <param name="sender">button对象</param>
        private void SetCurrentExp(object sender)
        {
            try
            {
                if (!IsEditType())
                {
                    return;
                }
                Button curBtn = sender as Button;
                //获取当前datatemplate下的grid对象
                var curGrid = VisualTreeHelper.GetParent(curBtn) as Grid;
                int ctrNum = VisualTreeHelper.GetChildrenCount(curGrid);
                //遍历此控件下的子控件，选择性可用
                for (int i = 0; i < ctrNum; i++)
                {
                    string curType = VisualTreeHelper.GetChild(curGrid, i).GetType().ToString();
                    switch (curType)
                    {
                        case "System.Windows.Controls.TextBox":
                            //TextBox 使可用
                            var curTextBox = VisualTreeHelper.GetChild(curGrid, i) as TextBox;
                            curTextBox.IsReadOnly = false;
                            curTextBox.IsEnabled = true;
                            break;
                        case "System.Windows.Controls.StackPanel":
                            //只有两个时间控件在StackPanel中
                            var curSP = VisualTreeHelper.GetChild(curGrid, i) as StackPanel;
                            var childGrid = VisualTreeHelper.GetChild(curSP, 0) as Grid;
                            var curFromDate = VisualTreeHelper.GetChild(childGrid, 0) as DatePicker;
                            var curToDate = VisualTreeHelper.GetChild(childGrid, 2) as DatePicker;
                            curFromDate.IsEnabled = true;
                            curToDate.IsEnabled = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，修改工作经历出错，请重新打开页面或联系管理员。");
            }
        }
        
        /// <summary>
        /// 按钮：删除工作经历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelExp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsEditType())
                {
                    return;
                }

                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<T_HR_EXPERIENCE> ListExp = DtGridExp.ItemsSource as ObservableCollection<T_HR_EXPERIENCE>;
                    ObservableCollection<T_HR_EXPERIENCE> paramExp = new ObservableCollection<T_HR_EXPERIENCE>();
                    if (ListExp.Count > 0)
                    {
                        T_HR_EXPERIENCE delExp = DtGridExp.SelectedItem as T_HR_EXPERIENCE;
                        paramExp.Add(delExp);
                        RefreshUI(RefreshedTypes.ShowProgressBar);
                        client.ExperienceDeleteAsync(paramExp);
                        ListExp.RemoveAt(DtGridExp.SelectedIndex);
                        DtGridExp.ItemsSource = ListExp;
                    }
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, string.Empty);
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，修改工作经历出错，请重新打开页面或联系管理员。");
            }
        }

        /// <summary>
        /// 按钮：增加教育经历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddEdu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //新增一个空的条目
                ObservableCollection<T_HR_EDUCATEHISTORY> listEdu = DtGridEdu.ItemsSource as ObservableCollection<T_HR_EDUCATEHISTORY>;
                T_HR_EDUCATEHISTORY newEdu = new T_HR_EDUCATEHISTORY();
                newEdu.EDUCATEHISTORYID = System.Guid.NewGuid().ToString();
                if (listEdu == null)
                {
                    listEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();
                }
                listEdu.Add(newEdu);
                DtGridEdu.ItemsSource = listEdu;
                if (listEdu != null)
                {
                    this.ListEdu = listEdu.ToList();
                }
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，新增教育经历出错，请重新打开页面或联系管理员。");
            }
        }

        /// <summary>
        /// 按钮：修改教育经历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnModifyEdu_Click(object sender, RoutedEventArgs e)
        {
            Button btnEdit = sender as Button;
            if (btnEdit.Content.ToString() == "修改")
            {
                SetCurrentExp(sender);
                btnEdit.Content = "保存";
            }
            else
            {
                UpdateEdu(sender);
                btnEdit.Content = "修改";
            }
        }
        /// <summary>
        /// 按钮：删除教育经历
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelEdu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!IsEditType())
                {
                    return;
                }
                
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    ObservableCollection<T_HR_EDUCATEHISTORY> ListEdu = DtGridEdu.ItemsSource as ObservableCollection<T_HR_EDUCATEHISTORY>;
                    ObservableCollection<T_HR_EDUCATEHISTORY> paramEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();
                    T_HR_EDUCATEHISTORY delEdu = DtGridEdu.SelectedItem as T_HR_EDUCATEHISTORY;
                    paramEdu.Add(delEdu);
                    RefreshUI(RefreshedTypes.ShowProgressBar);
                    client.EducateHistoryDeleteAsync(paramEdu);

                    ListEdu.RemoveAt(DtGridEdu.SelectedIndex);
                    DtGridEdu.ItemsSource = ListEdu;
                };
                com.SelectionBox(Utility.GetResourceStr("DELETECONFIRM"), Utility.GetResourceStr("DELETEALTER"), ComfirmWindow.titlename, string.Empty);
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，修改教育经历出错，请重新打开页面或联系管理员。");
            }
        }
        /// <summary>
        /// 检测是否是修改状态
        /// </summary>
        /// <returns>True means it's editable</returns>
        private bool IsEditType()
        {
            bool isEdit=true;
            if (FormTypes.Edit != FormType)
            {
                isEdit = false;
                Utility.ShowCustomMessage(MessageTypes.Caution, "注意", "很抱歉，仅修改状态下能进行此操作。");
            }
            return isEdit;
        }
        /// <summary>
        /// 修改工作经历
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateExp(Object sender)
        {
            try
            {
                if (!IsEditType())
                {
                    return;
                }
                Button curBtn = sender as Button;
                T_HR_EXPERIENCE newExp = DtGridExp.SelectedItem as T_HR_EXPERIENCE; //新的工作经历
                if (newExp == null)
                {
                    newExp = new T_HR_EXPERIENCE();
                }
                if (newExp.EXPERIENCEID == null)
                {
                    newExp.EXPERIENCEID = System.Guid.NewGuid().ToString();
                }
                if (newExp.CREATEUSERID == null)
                {
                    newExp.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                }
                
                //获取当前datatemplate下的grid对象
                var curGrid = VisualTreeHelper.GetParent(curBtn) as Grid;
                int ctrNum = VisualTreeHelper.GetChildrenCount(curGrid);

                //遍历此控件下的子控件，选择性可用
                for (int i = 0; i < ctrNum; i++)
                {
                    string curType = VisualTreeHelper.GetChild(curGrid, i).GetType().ToString();
                    switch (curType)
                    {
                        case "System.Windows.Controls.TextBox":
                            //TextBox 使可用
                            var curTextBox = VisualTreeHelper.GetChild(curGrid, i) as TextBox;
                            SetExpValue(newExp, curTextBox);
                            curTextBox.IsReadOnly = true;
                            curTextBox.IsEnabled = false;
                            break;
                        case "System.Windows.Controls.StackPanel":
                            //只有两个时间控件在StackPanel中
                            var curSP = VisualTreeHelper.GetChild(curGrid, i) as StackPanel;
                            var childGrid = VisualTreeHelper.GetChild(curSP, 0) as Grid;
                            var curFromDate = VisualTreeHelper.GetChild(childGrid, 0) as DatePicker;
                            var curToDate = VisualTreeHelper.GetChild(childGrid, 2) as DatePicker;
                            newExp.STARTDATE = curFromDate.Text;
                            newExp.ENDDATE = curToDate.Text;
                            curFromDate.IsEnabled = false;
                            curToDate.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                }
                ObservableCollection<T_HR_EXPERIENCE> colExp = new ObservableCollection<T_HR_EXPERIENCE>();
                
                newExp.T_HR_RESUME = Resume;
                colExp.Add(newExp);
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.ExperienceUpdateAsync(colExp);
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，修改工作经历出错，请重新打开页面或联系管理员。");
            }
        }

        /// <summary>
        /// 修改工作经历
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateEdu(Object sender)
        {
            try
            {
                if (!IsEditType())
                {
                    return;
                }
                Button curBtn = sender as Button;
                T_HR_EDUCATEHISTORY newEdu = DtGridEdu.SelectedItem as T_HR_EDUCATEHISTORY; //新的工作经历
                if (newEdu == null)
                {
                    newEdu = new T_HR_EDUCATEHISTORY();
                }
                //获取当前datatemplate下的grid对象
                var curGrid = VisualTreeHelper.GetParent(curBtn) as Grid;
                int ctrNum = VisualTreeHelper.GetChildrenCount(curGrid);

                //遍历此控件下的子控件，选择性可用
                for (int i = 0; i < ctrNum; i++)
                {
                    string curType = VisualTreeHelper.GetChild(curGrid, i).GetType().ToString();
                    switch (curType)
                    {
                        case "System.Windows.Controls.TextBox":
                            //TextBox 使可用
                            var curTextBox = VisualTreeHelper.GetChild(curGrid, i) as TextBox;
                            SetEduValue(newEdu, curTextBox);
                            curTextBox.IsReadOnly = true;
                            curTextBox.IsEnabled = false;
                            break;
                        case "System.Windows.Controls.StackPanel":
                            //只有两个时间控件在StackPanel中
                            var curSP = VisualTreeHelper.GetChild(curGrid, i) as StackPanel;
                            var childGrid = VisualTreeHelper.GetChild(curSP, 0) as Grid;
                            var curFromDate = VisualTreeHelper.GetChild(childGrid, 0) as DatePicker;
                            var curToDate = VisualTreeHelper.GetChild(childGrid, 2) as DatePicker;
                            newEdu.STARTDATE = curFromDate.Text;
                            newEdu.ENDDATE = curToDate.Text;
                            curFromDate.IsEnabled = false;
                            curToDate.IsEnabled = false;
                            break;
                        default:
                            break;
                    }
                }
                ObservableCollection<T_HR_EDUCATEHISTORY> colEdu = new ObservableCollection<T_HR_EDUCATEHISTORY>();
                if (newEdu.EDUCATEHISTORYID == null)
                {
                    newEdu.EDUCATEHISTORYID = System.Guid.NewGuid().ToString();
                }
                if (newEdu.EDUCATEHISTORYID == null)
                {
                    newEdu.CREATEUSERID = Common.CurrentLoginUserInfo.EmployeeID;
                }
                newEdu.T_HR_RESUME = Resume;
                colEdu.Add(newEdu);
                RefreshUI(RefreshedTypes.ShowProgressBar);
                client.EducateHistoryUpdateAsync(colEdu);
            }
            catch
            {
                Utility.ShowCustomMessage(MessageTypes.Error, "错误", "很抱歉，修改工作经历出错，请重新打开页面或联系管理员。");
            }
        }

        /// <summary>
        /// 根据相应的控件名返回工作经历结果
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="textbox"></param>
        private void SetExpValue(T_HR_EXPERIENCE ent, TextBox textbox)
        {
            switch (textbox.Name)
            {
                case "tbExpCompany":
                    ent.COMPANYNAME = textbox.Text;
                    break;
                case "tbExpPost":
                    ent.POST = textbox.Text;
                    break;
                case "tbExpSalary":
                    ent.SALARY = textbox.Text;
                    break;
                case "tbExpWork":
                    ent.JOBDESCRIPTION = textbox.Text;
                    break;
                case "tbExpRemark":
                    ent.REMARK = textbox.Text;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 根据相应的控件名返回工作经历结果
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="textbox"></param>
        private void SetEduValue(T_HR_EDUCATEHISTORY ent, TextBox textbox)
        {
            switch (textbox.Name)
            {
                case "tbSpecial":
                    ent.SPECIALTY = textbox.Text;
                    break;
                case "tbEduSchool":
                    ent.SCHOONAME = textbox.Text;
                    break;
                case "tbEDUCATIONHISTORY":
                    ent.EDUCATIONHISTORY = textbox.Text;
                    break;
                case "tbEDUCATIONPROPERTIE":
                    ent.EDUCATIONPROPERTIE = textbox.Text;
                    break;
                case "tbEduREMARK":
                    ent.REMARK = textbox.Text;
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
