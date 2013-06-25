using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
//using SMT.Saas.Tools.PermissionWS;
//using SMT.Saas.Tools.PersonnelWS;
//using SMT.Saas.Tools.OrganizationWS;
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;

using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContextLoadData;
using SMT.SaaS.FrameworkUI.ProgressBar;
using System.IO.IsolatedStorage;

namespace SMT.SaaS.OA.UI.Views.Login
{
    public partial class Login : ChildWindow
    {
        #region 初始化变量

        SMTLoading2 loadbar = new SMTLoading2();
        
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        //用户登录类
        UserLogin login;
        //用户信息
        T_SYS_USER UserInfo = new T_SYS_USER();
        List<T_HR_DEPARTMENT> allDepartments;
        List<T_HR_POST> entlist;
        List<T_HR_COMPANY> allCompanys;

        public Login()
        {            
            InitializeComponent();
            
            this.Loaded += new RoutedEventHandler(Login_Loaded);
            //加载所有公司、部门、岗位信息
            //organClient.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(organClient_GetCompanyAllCompleted);
            //organClient.GetDepartmentAllCompleted += new EventHandler<GetDepartmentAllCompletedEventArgs>(organClient_GetDepartmentAllCompleted);
            //organClient.GetPostAllCompleted += new EventHandler<GetPostAllCompletedEventArgs>(organClient_GetPostAllCompleted);
            //organClient.GetCompanyAllAsync("");

            //organClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(organClient_GetALLCompanyViewCompleted);
            //organClient.GetAllDepartmentViewCompleted += new EventHandler<GetAllDepartmentViewCompletedEventArgs>(organClient_GetAllDepartmentViewCompleted);
            //organClient.GetAllPostViewCompleted += new EventHandler<GetAllPostViewCompletedEventArgs>(organClient_GetAllPostViewCompleted);

            organClient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(organClient_GetCompanyActivedCompleted);
            organClient.GetDepartmentActivedCompleted += new EventHandler<GetDepartmentActivedCompletedEventArgs>(organClient_GetDepartmentActivedCompleted);
            organClient.GetPostActivedCompleted += new EventHandler<GetPostActivedCompletedEventArgs>(organClient_GetPostActivedCompleted);
            

            //登录控件禁用  在加载完组织架构后 再激活
            //this.OK.IsEnabled = false;
            
            
        }


        #endregion


        #region 窗体加载完成事件
        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("usName"))
            {
                string usName = IsolatedStorageSettings.ApplicationSettings["usName"] as string;
                UserName.Text = usName;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
            {
                string Password = IsolatedStorageSettings.ApplicationSettings["Password"] as string;
                this.paw.Password = Password;
            }
           
            UserName.Focus();
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Style = (Style)Application.Current.Resources["GridStyle4"];
        }
        #endregion


        #region 登录按钮
        private void OK_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                string StrUserName = "";//用户名
                string StrPwd = "";//密码
                string StrCode = ""; //验证码
                StrUserName = UserName.Text.ToString().Trim();
                StrPwd = paw.Password.ToString().Trim();
                //暂时不使用
                StrCode = YanZM.Text.ToString().Trim();


                if (string.IsNullOrEmpty(StrUserName))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("USERNAMENOTNULL"));
                    UserName.Focus();
                    return;
                }
                if (string.IsNullOrEmpty(StrPwd))
                {
                    Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PASSWORDNOTNULL"));
                    paw.Focus();
                    return;

                }
                this.OK.IsEnabled = false;
                login = new UserLogin(StrUserName, StrPwd);
                loadbar.setloadingMessage("开始登录");
                login.LoginedClick += (obj, ev) =>
                {

                    if (login.LoginResult)
                    {
                        UserInfo = login.GetUserInfo();

                        Common.CurrentLoginUserInfo.UserName = UserInfo.USERNAME;
                        Common.CurrentLoginUserInfo.UserPwd = UserInfo.PASSWORD;                        
                        if (App.Current.Resources["CurrentUserID"] == null)
                        {
                            App.Current.Resources.Add("CurrentUserID", UserInfo.EMPLOYEEID);
                        }
                        //organClient.GetCompanyActivedAsync(UserInfo.EMPLOYEEID);
                        LoadStart();
                        LoadCompanyInfo();
                        if (!IsolatedStorageSettings.ApplicationSettings.Contains("usName"))
                        {
                            IsolatedStorageSettings.ApplicationSettings.Add("usName",this.UserName.Text);
                        }
                        else
                        {
                            IsolatedStorageSettings.ApplicationSettings["usName"] = this.UserName.Text;
                        }
                        if (!IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
                        {
                            IsolatedStorageSettings.ApplicationSettings.Add("Password",paw.Password);
                        }
                        else
                        {
                            IsolatedStorageSettings.ApplicationSettings["Password"] = paw.Password;
                        }
                    }
                    else
                    {
                        this.OK.IsEnabled = true;
                        //用户名或密码错误
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("USERNAMEORPASSWORDERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    }
                };
            }
            catch (Exception ex)
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), ex.ToString(), Utility.GetResourceStr("CONFIRMBUTTON"));
            }


        }
        #endregion

        #region 组织架构

        /// <summary>
        /// 读取公司信息
        /// </summary>
        private void LoadCompanyInfo()
        {
            string strOwnerId = string.Empty;
            if (Common.CurrentLoginUserInfo != null)
            {
                strOwnerId = Common.CurrentLoginUserInfo.EmployeeID;
            }
            loadbar.setloadingMessage("开始获取公司信息......");
            organClient.GetCompanyActivedAsync(strOwnerId);
            //organClient.GetALLCompanyViewAsync(strOwnerId);
        }


        void organClient_GetPostActivedCompleted(object sender, GetPostActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_PostInfo", e.Result.ToList());
                    loadbar.setloadingMessage("获取部门信息完毕，正在初始化系统......");
                    LoadEnd(true);
                }
            }
        }

        void organClient_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_DepartmentInfo", e.Result.ToList());

                    string strOwnerId = string.Empty;
                    if (Common.CurrentLoginUserInfo != null)
                    {
                        strOwnerId = Common.CurrentLoginUserInfo.EmployeeID;
                    }
                    loadbar.setloadingMessage("获取部门信息完毕，开始获取岗位公司信息......");
                    organClient.GetPostActivedAsync(strOwnerId);
                }
            }
        }

        void organClient_GetCompanyActivedCompleted(object sender, GetCompanyActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_CompanyInfo", e.Result.ToList());

                    string strOwnerId = string.Empty;
                    if (Common.CurrentLoginUserInfo != null)
                    {
                        strOwnerId = Common.CurrentLoginUserInfo.EmployeeID;
                    }
                    loadbar.setloadingMessage("获取公司信息完毕，开始获取部门公司信息......");
                    organClient.GetDepartmentActivedAsync(strOwnerId);
                }
            }

        }


        /// <summary>
        /// 加载动画进度开始
        /// </summary>
        private void LoadStart()
        {
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Children.Remove(this);
            currentApp.rootGrid.Children.Add(loadbar);
            loadbar.Start();
        }
        /// <summary>
        /// 加载动画进度终止，跳转到首页
        /// </summary>
        private void LoadEnd(bool bflag)
        {
            App currentApp = (App)Application.Current;
            loadbar.Stop();
            currentApp.rootGrid.Children.Remove(loadbar);

            if (bflag)
            {
                //MainPage mainPage = new MainPage();
                //Common.ParentLayoutRoot = mainPage.LayoutRoot;
                //AppConfig._CurrentStyleCode = 1;
                //App.Navigation(mainPage);
                //App.MainPage = mainPage;

                MainPage mainPage = new MainPage();
                SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot = mainPage.LayoutRoot;
                App.Navigation(mainPage);
            }
        }
        #endregion

        
    }
}

