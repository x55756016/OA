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
using System.Globalization;
using System.Threading;
using SMT.Saas.Tools.PermissionWS;
using SMT.SaaS.FrameworkUI.Helper;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ProgressBar;
using SMT.Saas.Tools.OrganizationWS;
using SMT.Saas.Tools.FlowDesignerWS;
using SMT.SAAS.Main.CurrentContext;

using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PersonnelWS;
using SMT.SAAS.Main.CurrentContextLoadData;
using System.IO.IsolatedStorage;

namespace SMT.HRM.UI.Views.Login
{
    public partial class Login : ChildWindow
    {

        #region 初始化变量
        SMTLoading2 loadbar = new SMTLoading2();
        private PermissionServiceClient client = new PermissionServiceClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        private PersonnelServiceClient personnel = new PersonnelServiceClient();
        //用户信息
        T_SYS_USER UserInfo = new T_SYS_USER();

        public Login()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Login_Loaded);
            //加载所有公司、部门、岗位信息
            organClient.GetCompanyActivedCompleted += new EventHandler<GetCompanyActivedCompletedEventArgs>(organClient_GetCompanyActivedCompleted);
            organClient.GetDepartmentActivedCompleted += new EventHandler<GetDepartmentActivedCompletedEventArgs>(organClient_GetDepartmentActivedCompleted);
            organClient.GetPostActivedCompleted += new EventHandler<GetPostActivedCompletedEventArgs>(organClient_GetPostActivedCompleted);
            //organClient.GetDepartmentViewAsync("f12547ca-f2c5-44d5-aee1-f65cfb36761e", "3", "ContactDetailsView");
            //organClient.GetCompanyViewAsync("f12547ca-f2c5-44d5-aee1-f65cfb36761e", "3", "ContactDetailsView");
            //登录控件禁用  在加载完组织架构后 再激活
            this.OK.IsEnabled = true;
            organClient.GetALLCompanyViewCompleted += new EventHandler<GetALLCompanyViewCompletedEventArgs>(organClient_GetALLCompanyViewCompleted);
            //organClient.GetALLCompanyViewAsync("6ba49ec8-feb0-4f78-b801-2b8ea5387ab3");
            //organClient.GetAllDepartmentViewAsync("6ba49ec8-feb0-4f78-b801-2b8ea5387ab3");
            //organClient.GetAllPostViewAsync("6ba49ec8-feb0-4f78-b801-2b8ea5387ab3");
            //personnel.GetEmployeeLeadersAsync("bc02cea5-b3c8-4a2d-8487-c1e6da9a3d97", 0);
            
        }

        #endregion

        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Style = (Style)Application.Current.Resources["GridStyle4"];
            //personnel.GetEmployeeLeadersAsync("0716eed5-51ce-4e98-a7ed-5de394e736af", 0);
            if (IsolatedStorageSettings.ApplicationSettings.Contains("usName"))
            {
                string usName = IsolatedStorageSettings.ApplicationSettings["usName"] as string;
                this.nam.Text = usName;
            }
            if (IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
            {
                string Password = IsolatedStorageSettings.ApplicationSettings["Password"] as string;
                this.paw.Password = Password;
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_buton(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nam.Text) || string.IsNullOrEmpty(paw.Password))
            {
                MessageBox.Show("用户名或密码不能为空！");
            }
            else
            {

                this.OK.IsEnabled = false;
                UserLogin log = new UserLogin(nam.Text, paw.Password);
                log.LoginedClick += (obj, ev) =>
                {
                    if (log.LoginResult)
                    {
                        UserInfo = log.GetUserInfo();
                        // client.GetUserInfoAsync(nam.Text);
                        // 加载读取数据滚动条
                        if (App.Current.Resources["CurrentUserID"] == null)
                        {
                            App.Current.Resources.Add("CurrentUserID", UserInfo.EMPLOYEEID);
                        }
                        Common.CurrentLoginUserInfo.UserName = UserInfo.USERNAME;
                        Common.CurrentLoginUserInfo.UserPwd = UserInfo.PASSWORD;

                        LoadStart();
                        // 加载组织机构字典
                        LoadCompanyInfo();
                        if (IsolatedStorageSettings.ApplicationSettings.Contains("usName"))
                        {
                            IsolatedStorageSettings.ApplicationSettings["usName"] = nam.Text;
                        }
                        else
                        {
                            IsolatedStorageSettings.ApplicationSettings.Add("usName", nam.Text);
                        }
                        if (IsolatedStorageSettings.ApplicationSettings.Contains("Password"))
                        {
                            IsolatedStorageSettings.ApplicationSettings["Password"] = paw.Password;
                        }
                        else
                        {
                            IsolatedStorageSettings.ApplicationSettings.Add("Password", paw.Password);
                        }
                    }
                    else
                    {
                        //用户名或密码错误
                        this.OK.IsEnabled = true;
                        tbErrMsg.Text = Utility.GetResourceStr("USERNAMEORPASSWORDERROR");
                    }
                };


            }
        }
        
        #region 获取组织架构

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
                    organClient.GetDepartmentActivedAsync(strOwnerId);
                }
            }

        }


        void organClient_GetALLCompanyViewCompleted(object sender, GetALLCompanyViewCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string a = e.Error.ToString();
            }
            else
            {
                List<V_COMPANY> aa = e.Result.ToList();
            }
            //throw new NotImplementedException();
        }

        #endregion

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
                MainPage mainPage = new MainPage();
                SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot = mainPage.LayoutRoot;

                App.Navigation(mainPage);
            }
        }
        
        private void Cen_B(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        #region 语言选择
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CultureInfo culture = new CultureInfo(((RadioButton)sender).Tag.ToString());
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            SMT.SaaS.Globalization.Localization.UiCulture = culture;

            SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
        }
        #endregion
    }
}

