using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI;
using SMT.SAAS.Main.CurrentContext;
using System.Collections.ObjectModel;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.SalaryWS;
//using Framework=SMT.SaaS.FrameworkUI;
namespace SMT.SaaS.Permission.UI.Form
{
    public partial class SysUserForms : UserControl, IEntityEditor
    {
        private T_SYS_USER sysUser = new T_SYS_USER();
        private T_HR_SYSTEMSETTING systemSetting = new T_HR_SYSTEMSETTING();
        private SalaryServiceClient salaryClient = new SalaryServiceClient();
        private string saveType = "0";       //保存方式 0:添加 1:关闭
        string StrCompanyID = "";
        //private string tmpDictionaryValue = ""; //字典值
        private RefreshedTypes refresh;
        private FormTypes formType;
        private ObservableCollection<string> ForbidInfosList = new ObservableCollection<string>();
        public FormTypes FormType
        {
            get { return formType; }
            set { formType = value; }
        }

        private PermissionServiceClient ServiceClent = new PermissionServiceClient();
        public SysUserForms(FormTypes type)
        {
            InitializeComponent();
            ////TODO:加操作用户信息
            FormType = type;
            InitParas("");
        }

        public SysUserForms(FormTypes type, string sysUserID)
        {
            InitializeComponent();
             FormType = type;
             InitParas(sysUserID);
             if (type == FormTypes.Browse)
             {
                 UnEnableFormControl();
             }
        }

        /// <summary>
        /// 禁用表单控件
        /// </summary>
        private void UnEnableFormControl()
        {
            txtUserName.IsEnabled = false;
            txtCompanyName.IsEnabled = false;
            btnLookUpPartyb.IsEnabled = false;
            txtPassword.IsEnabled = false;
            txtConfirmpwd.IsEnabled = false;
            //txtRemark.IsEnabled = false;
            txtRemark.IsReadOnly = true;
            rbtIsAutoyes.IsEnabled = false;
            rbtIsAutono.IsEnabled = false;
            btnSet.IsEnabled = false;
           
        }

        private void InitParas(string sysUserID)
        {
            ServiceClent.SysUserInfoUpdateCompleted += new EventHandler<SysUserInfoUpdateCompletedEventArgs>(SysUserClient_SysUserInfoUpdateCompleted);
            ServiceClent.GetUserByEmployeeIDCompleted += new EventHandler<GetUserByEmployeeIDCompletedEventArgs>(ServiceClent_GetUserByEmployeeIDCompleted);
            ServiceClent.GetUserByIDCompleted += new EventHandler<GetUserByIDCompletedEventArgs>(ServiceClent_GetUserByIDCompleted);
            ServiceClent.SysUserBatchUpdateCompleted += new EventHandler<SysUserBatchUpdateCompletedEventArgs>(SysUserClient_SysUserBatchUpdateCompleted);
            salaryClient.GetSystemParamSetPagingCompleted += new EventHandler<GetSystemParamSetPagingCompletedEventArgs>(salaryClient_GetSystemParamSetPagingCompleted);
            if (!string.IsNullOrEmpty(sysUserID))
            {
                this.txtUserName.IsEnabled = false;
                
                //ServiceClent.GetUserByIDAsync(sysUserID);               
                ServiceClent.GetUserByEmployeeIDAsync(sysUserID);

                
            } 
        }

        void salaryClient_GetSystemParamSetPagingCompleted(object sender, GetSystemParamSetPagingCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    systemSetting = e.Result.FirstOrDefault();
                    txtSalaryPwd.Password = SMT.SaaS.FrameworkUI.Common.Utility.Decrypt(systemSetting.PARAMETERVALUE);
                }
            }
        }

        void ServiceClent_GetUserByEmployeeIDCompleted(object sender, GetUserByEmployeeIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    sysUser = e.Result as T_SYS_USER;
                    this.tblemployeename.Text = sysUser.EMPLOYEENAME;
                    this.txtUserName.Text = sysUser.USERNAME;
                    this.txtPassword.Password = SMT.SaaS.FrameworkUI.Common.Utility.Decrypt( sysUser.PASSWORD);
                    this.txtConfirmpwd.Password = SMT.SaaS.FrameworkUI.Common.Utility.Decrypt(sysUser.PASSWORD);
                    this.txtRemark.Text = sysUser.REMARK == null ? "" : sysUser.REMARK;
                    if (!string.IsNullOrEmpty(sysUser.OWNERCOMPANYID))
                    {
                        GetCompanyNameByCompanyID(sysUser.OWNERCOMPANYID);
                    }
                    if (sysUser.ISMANGER == 1)
                    {
                        this.rbtIsAutoyes.IsChecked = true;
                    }
                    else
                    {
                        this.rbtIsAutono.IsChecked = true;
                    }
                    if (sysUser.ISENGINEMANAGER == "1")
                    {
                        this.rbtengine.IsChecked = true;
                    }
                    else
                    {
                        this.rbtengineno.IsChecked = true;
                    }
                    if (sysUser.ISFLOWMANAGER == "1")
                    {
                        this.rbtflow.IsChecked = true;
                    }
                    else
                    {
                        this.rbtflowno.IsChecked = true;
                    }
                    if (sysUser.STATE == "0")
                    {
                        this.tblstate.Text = "禁用";
                        //this.tblstate.Foreground = SystemColors.
                        btnSet.Content = Utility.GetResourceStr("STARTUSING");
                        //FormToolBar1.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png", Utility.GetResourceStr("STARTUSING")).Click += new RoutedEventHandler(SysUserSetIsActived_Click);
                    }
                    else
                    {
                        this.tblstate.Text = "正常";
                        btnSet.Content = Utility.GetResourceStr("FORBIDUSE");
                        //FormToolBar1.btnOtherAction("/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/ico_16_4424.png", Utility.GetResourceStr("FORBIDUSE")).Click += new RoutedEventHandler(SysUserManagement_Click);
                    }

                    #region 获取用户薪资密码
                    int pageCount = 0;
                    string filter = "";
                    System.Collections.ObjectModel.ObservableCollection<string> paras = new System.Collections.ObjectModel.ObservableCollection<string>();
                    filter += "MODELTYPE==@" + paras.Count().ToString();
                    paras.Add("4");
                    filter += " and OWNERID==@" + paras.Count().ToString();
                    paras.Add(sysUser.EMPLOYEEID);
                    salaryClient.GetSystemParamSetPagingAsync(1, 20, "PARAMETERNAME", filter, paras, pageCount, sysUser.EMPLOYEEID);

                    #endregion
                }
            }
        }

        void ServiceClent_GetUserByIDCompleted(object sender, GetUserByIDCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result != null)
                {
                    sysUser = e.Result as T_SYS_USER;
                    this.tblemployeename.Text = sysUser.EMPLOYEENAME;
                    this.txtUserName.Text = sysUser.USERNAME;
                    this.txtPassword.Password = SMT.SaaS.FrameworkUI.Common.Utility.Decrypt(sysUser.PASSWORD);
                    this.txtConfirmpwd.Password = SMT.SaaS.FrameworkUI.Common.Utility.Decrypt(sysUser.PASSWORD);
                    this.txtRemark.Text =  sysUser.REMARK==null ? "": sysUser.REMARK ;                    
                    if (!string.IsNullOrEmpty(sysUser.OWNERCOMPANYID))
                    {
                        GetCompanyNameByCompanyID(sysUser.OWNERCOMPANYID);
                    }
                    if (sysUser.ISMANGER == 1)
                    {
                        this.rbtIsAutoyes.IsChecked = true;
                    }
                    else
                    {
                        this.rbtIsAutono.IsChecked = true;
                    }
                    if (sysUser.ISFLOWMANAGER == "1")
                    {
                        this.rbtflow.IsChecked = true;
                    }
                    else
                    {
                        this.rbtflowno.IsChecked = true;
                    }
                    if (sysUser.ISENGINEMANAGER == "1")
                    {
                        this.rbtengine.IsChecked = true;
                    }
                    else
                    {
                        this.rbtengineno.IsChecked = true;
                    }
                }
            }
        }
        #region 修改系统用户
        void SysUserClient_SysUserInfoUpdateCompleted(object sender, SysUserInfoUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error != null)
                {
                    if (e.Result)
                    {
                        //ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "修改成功！", Utility.GetResourceStr("CONFIRMBUTTON"));
                        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("SYSTEMERRORPLEASELINKDADMIN"));
                    }
                }
                else
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("SUCCESSED"), Utility.GetResourceStr("UPDATESUCCESSED", "SYSUSERMANAGEMENT"));
                }
            }
            RefreshUI(refresh);
        }
        #endregion

        public SysUserForms()
        {
            InitializeComponent();
        }

        private void LayoutRoot_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {

        }

        #region IEntityEditor 成员

        public string GetTitle()
        {
            return Utility.GetResourceStr("BASEINFO");
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
                    refresh = RefreshedTypes.All;
                    break;
                case "1":
                    refresh = RefreshedTypes.CloseAndReloadData;
                    
                    break;
            }
            AddToClose();
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
            if (FormType == FormTypes.New || FormType == FormTypes.Edit)
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
                    ImageUrl = "/SMT.SaaS.FrameworkUI;Component/Images/ToolBar/16_SAVE.png"
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

        #region 修改用户信息
        public void AddToClose()
        {
            string StrUserName = "";
            string StrRemark = "";
            string StrPwd = "";
            string StrConfirmPwd = "";
            //string StrError = "";
            string IsManage = "0"; //是否是管理员
            string IsFlowManager = "0";//是否是流程管理员
            string IsEngineManager = "0";//是否是引擎管理员
            //bool IsError = true;
            StrUserName = this.txtUserName.Text.ToString().Trim();
            StrRemark = this.txtRemark.Text.ToString().Trim();
            StrPwd = this.txtPassword.Password.ToString();
            StrConfirmPwd = this.txtConfirmpwd.Password;
            if (string.IsNullOrEmpty(StrPwd) && string.IsNullOrEmpty(StrConfirmPwd))
            {

                //StrError += "密码不能为空\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PASSWORDNOTNULL"));
                return;

            }
            else
            {
                if (!CheckPwd(StrPwd))
                {
                    RefreshUI(RefreshedTypes.HideProgressBar);
                    return;
                }
            }
            if (rbtIsAutoyes.IsChecked == true)
            {
                IsManage = "1";
            }
            if (rbtIsAutono.IsChecked == true)
            {
                IsManage = "0";
            }
            if (rbtflow.IsChecked == true)
            {
                IsFlowManager = "1";
            }
            if (rbtflowno.IsChecked == true)
            {
                IsFlowManager = "0";
            }
            if (rbtengine.IsChecked == true)
            {
                IsEngineManager = "1";
            }
            if (rbtengineno.IsChecked == true)
            {
                IsEngineManager = "0";
            }
            if (!string.IsNullOrEmpty(StrPwd)  && string.IsNullOrEmpty(StrConfirmPwd))
            {
                
                //"确认密码不能为空\n";
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("CONFIRMPASSWORDNOTNULL"));
                return;
            }
            if (!string.IsNullOrEmpty(StrPwd) && !string.IsNullOrEmpty(StrConfirmPwd))
            {
                if (StrPwd != StrConfirmPwd)
                {
                    
                    //StrError += "密码输入不一致\n";
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PASSWORDNOTEQUAL"));
                    return;
                }
            }
            if (string.IsNullOrEmpty(StrUserName))
            {                
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("USERNAMENOTNULL"));
                return;
            }
            if (string.IsNullOrEmpty(StrCompanyID))
            {
                Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("STRINGNOTNULL", "RESPECTIVECOMPANIES"));
                return;
            }

            sysUser.USERNAME = StrUserName;
            sysUser.REMARK = StrRemark;
            sysUser.PASSWORD =SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(StrPwd);
            sysUser.ISMANGER = System.Convert.ToDecimal(IsManage);
            sysUser.UPDATEDATE = System.DateTime.Now;
            sysUser.UPDATEUSER = Common.CurrentLoginUserInfo.SysUserID ;
            sysUser.OWNERCOMPANYID = StrCompanyID;
            saveType = "1";
            sysUser.ISENGINEMANAGER = IsEngineManager;
            sysUser.ISFLOWMANAGER = IsFlowManager;
            //sysUser.OWNERDEPARTMENTID = "22";
            ServiceClent.SysUserInfoUpdateAsync(sysUser);

            //修改薪资密码

            systemSetting.PARAMETERVALUE = SMT.SaaS.FrameworkUI.Common.Utility.Encrypt(txtSalaryPwd.Password);
            systemSetting.OWNERID = sysUser.EMPLOYEEID;
            systemSetting.PARAMETERNAME = sysUser.EMPLOYEENAME;
            salaryClient.SystemParamSetUpdateAsync(systemSetting);
            
            //RefreshUI(RefreshedTypes.All);
        }

        /// <summary>
        /// added by luojie
        /// 用与验证密码的合法性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool CheckPwd(string password)
        {
            bool legalPwd = true;
            string rstMessage = string.Empty;
            if (password != null)
            {
                if (password.Length < 8)
                {
                    legalPwd = false;
                    rstMessage = "密码不能小于8位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                if (password.Length > 15)
                {
                    legalPwd = false;
                    rstMessage = "密码不能大于15位数";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }
                string ptnNum = @"\D[0-9]+";
                string ptnWord = @"[0-9][a-z_A-Z]+";
                Match matchNum = Regex.Match(password, ptnNum);
                Match matchWord = Regex.Match(password, ptnWord);
                if (!matchNum.Success && !matchWord.Success)
                {
                    legalPwd = false;
                    rstMessage = "密码必须是8-15位的英文与数字组合";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码必须是中英文结合的");
                }
                if (!string.IsNullOrWhiteSpace(rstMessage)) Utility.ShowCustomMessage(MessageTypes.Error, "错误", rstMessage);
            }
            else
            {
                legalPwd = false;
            }
            return legalPwd;
        }
        #endregion

        #region 关闭窗体
        public void Close()
        {
            saveType = "1";
            RefreshUI(RefreshedTypes.All);
        }
        #endregion

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
                    StrCompanyID = company.COMPANYID;
                    txtCompanyName.Text = company.CNAME;
                    
                }
            }
        }
        
        //组织架构树选择所属公司
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
                    var q =from up in Common.CurrentLoginUserInfo.UserPosts
                           where up.CompanyID ==companyInfo.ObjectID
                           select ent;

                    if (q.Count() > 0)
                    {
                        StrCompanyID = companyInfo.ObjectID;
                        txtCompanyName.Text = companyInfo.ObjectName;
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"),"不能修改此所属公司！", Utility.GetResourceStr("CONFIRMBUTTON"));
                    }
                }
            };
            lookup.MultiSelected = false;
            lookup.Show();
        }

        #endregion

        private void rbtIsAutoyes_Click(object sender, RoutedEventArgs e)
        {
            this.rbtIsAutoyes.IsChecked = true;
            this.rbtIsAutono.IsChecked = false;
        }

        private void tbtIsAutono_Click(object sender, RoutedEventArgs e)
        {
            this.rbtIsAutoyes.IsChecked = false;
            this.rbtIsAutono.IsChecked = true;
        }

        private void BtnSet_FindClick(object sender, RoutedEventArgs e)
        {
            //string SysUserID = "";
            
            ForbidInfosList.Add(sysUser.EMPLOYEEID);

            if (ForbidInfosList.Count() > 0)
            {
                string Result = "";
                ComfirmWindow com = new ComfirmWindow();
                com.OnSelectionBoxClosed += (obj, result) =>
                {
                    
                    if (sysUser.STATE == "1")
                    {
                        ServiceClent.SysUserBatchUpdateAsync(ForbidInfosList, Common.CurrentLoginUserInfo.SysUserID, Common.CurrentLoginUserInfo.EmployeeName, "0");
                    }
                    else
                    {
                        ServiceClent.SysUserBatchUpdateAsync(ForbidInfosList, Common.CurrentLoginUserInfo.SysUserID, Common.CurrentLoginUserInfo.EmployeeName, "1");
                    }

                };
                if (sysUser.STATE == "1")
                {
                    com.SelectionBox("禁用用户", "确定禁用用户吗？", ComfirmWindow.titlename, Result);
                }
                if (sysUser.STATE == "0")
                {
                    com.SelectionBox("启用用户", "确定启用用户吗？", ComfirmWindow.titlename, Result);
                }
            }
            
        }

        void SysUserClient_SysUserBatchUpdateCompleted(object sender, SysUserBatchUpdateCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {

                if (e.Result == "")
                {
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "设置成功！", Utility.GetResourceStr("CONFIRMBUTTON"));
                    //this.LoadData();
                }
                else
                {
                    if (e.Result == "error")
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "系统出现错误，请与管理员联系！", Utility.GetResourceStr("CONFIRMBUTTON"));
                        return;
                    }
                    else
                    {
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), e.Result.ToString(), Utility.GetResourceStr("CONFIRMBUTTON"));
                        //this.LoadData();

                    }
                }
            }
            RefreshUI(RefreshedTypes.CloseAndReloadData);
        }

        #region 流程管理员

        private void rbtflow_Checked(object sender, RoutedEventArgs e)
        {
            rbtflow.IsChecked = true;
            rbtflowno.IsChecked = false;
        }

        private void rbtflowno_Checked(object sender, RoutedEventArgs e)
        {
            rbtflow.IsChecked = false;
            rbtflowno.IsChecked = true;
        }
        #endregion

        #region 是引擎管理员

        private void rbtengine_Checked(object sender, RoutedEventArgs e)
        {
            rbtengine.IsChecked = true;
            rbtengineno.IsChecked = false;
        }
        #endregion

        #region 不是引擎管理员

        private void rbtengineno_Checked(object sender, RoutedEventArgs e)
        {
            rbtengine.IsChecked = false;
            rbtengineno.IsChecked = true;
        }
        #endregion

        

    }
}
