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
using SMT.SAAS.Main.CurrentContext;
using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PersonnelWS;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.LM.UI.AppMain;
using SMT.SaaS.FrameworkUI.Common;
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.FB.UI;
using SMT.FB.UI.Common;
using System.IO.IsolatedStorage;

using CurrentContext = SMT.SAAS.Main.CurrentContext;
using SMT.SAAS.Main.CurrentContextLoadData;

namespace SMT.SaaS.LM.UI.AppMain.Login
{
    public partial class Login : ChildWindow
    {

        #region 不用
        //#region 初始化变量

        //private PermissionServiceClient client = new PermissionServiceClient();
        //private OrganizationServiceClient organClient = new OrganizationServiceClient();
        ////用户登录类
        //UserLogin login;
        ////用户信息
        //T_SYS_USER UserInfo = new T_SYS_USER();

        //public Login()
        //{
        //    InitializeComponent();

        //    this.Loaded += new RoutedEventHandler(Login_Loaded);
        //    //加载所有公司、部门、岗位信息
        //    organClient.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(organClient_GetCompanyAllCompleted);
        //    organClient.GetDepartmentAllCompleted += new EventHandler<GetDepartmentAllCompletedEventArgs>(organClient_GetDepartmentAllCompleted);
        //    organClient.GetPostAllCompleted += new EventHandler<GetPostAllCompletedEventArgs>(organClient_GetPostAllCompleted);
        //    organClient.GetCompanyAllAsync("");
        //    //登录控件禁用  在加载完组织架构后 再激活
        //    this.OK.IsEnabled = false;


        //}
        //#endregion

        //#region 窗体加载完成事件
        //void Login_Loaded(object sender, RoutedEventArgs e)
        //{
        //    UserName.Focus();
        //    App currentApp = (App)Application.Current;
        //    currentApp.rootGrid.Style = (Style)Application.Current.Resources["GridStyle4"];
        //}
        //#endregion

        //#region 获取组织架构


        //void organClient_GetPostAllCompleted(object sender, GetPostAllCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            App.Current.Resources.Add("SYS_PostInfo", e.Result.ToList());
        //            this.OK.IsEnabled = true;
        //            //if(IsFinished)
        //        }
        //    }
        //}

        //void organClient_GetDepartmentAllCompleted(object sender, GetDepartmentAllCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            App.Current.Resources.Add("SYS_DepartmentInfo", e.Result.ToList());
        //            organClient.GetPostAllAsync("");
        //        }
        //    }
        //}

        //void organClient_GetCompanyAllCompleted(object sender, GetCompanyAllCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        if (e.Result != null)
        //        {
        //            App.Current.Resources.Add("SYS_CompanyInfo", e.Result.ToList());
        //            organClient.GetDepartmentAllAsync("");
        //        }
        //    }

        //}
        //#endregion

        //#region 登录按钮
        //private void OK_Click(object sender, RoutedEventArgs e)
        //{

        //    string StrUserName = "";//用户名
        //    string StrPwd = "";//密码
        //    string StrCode = ""; //验证码
        //    StrUserName = UserName.Text.ToString().Trim();
        //    StrPwd = paw.Password.ToString().Trim();
        //    //暂时不使用
        //    StrCode = YanZM.Text.ToString().Trim();


        //    if (string.IsNullOrEmpty(StrUserName))
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("USERNAMENOTNULL"));
        //        UserName.Focus();
        //        return;
        //    }
        //    if (string.IsNullOrEmpty(StrPwd))
        //    {
        //        Utility.ShowCustomMessage(MessageTypes.Message, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr("PASSWORDNOTNULL"));
        //        paw.Focus();
        //        return;

        //    }

        //    login = new UserLogin(StrUserName, StrPwd);
        //    login.LoginedClick += (obj, ev) =>
        //    {

        //        if (login.LoginResult)
        //        {
        //            UserInfo = login.GetUserInfo();
        //            LoadDicts();

        //            MainPage mainPage = new MainPage();
        //            Common.ParentLayoutRoot = mainPage.LayoutRoot;
        //            SMT.SAAS.Main.CurrentContext.AppConfig._CurrentStyleCode = 1;
        //            Common.ParentLayoutRoot = mainPage.LayoutRoot;
        //            App.Navigation(mainPage);
                   
        //            if (App.Current.Resources["CurrentUserID"] == null)
        //            {
        //                App.Current.Resources.Add("CurrentUserID", UserInfo.EMPLOYEEID);
        //            }
        //        }
        //        else
        //        {
        //            //用户名或密码错误
        //            ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("USERNAMEORPASSWORDERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
        //        }
        //    };


        //}
        //#endregion

        //protected void LoadDicts()
        //{
        //    try
        //    {
        //        PermissionServiceClient cliet = new PermissionServiceClient();

        //        cliet.GetSysDictionaryByCategoryCompleted += (o, e) =>
        //        {
        //            if (e.Error != null && e.Error.Message != "")
        //            {
        //                CommonFunction.ShowErrorMessage(Utility.GetResourceStr(e.Error.Message));
        //            }
        //            else
        //            {
        //                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
        //                dicts = e.Result == null ? null : e.Result.ToList();
        //                if (Application.Current.Resources.Contains("SYS_DICTIONARY"))
        //                {
        //                    Application.Current.Resources.Remove("SYS_DICTIONARY");
        //                }
        //                Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
        //            }
        //        };
        //        //TODO: 按需取出字典值
        //        cliet.GetSysDictionaryByCategoryAsync("");
        //    }
        //    catch (Exception ex)
        //    {
        //        CommonFunction.ShowErrorMessage(Utility.GetResourceStr(ex.Message));
        //    }
        //}

        #endregion

        public bool isHR = false;
        public bool isDict = false;
        #region 初始化变量

        IsolatedStorageSettings settings = IsolatedStorageSettings.SiteSettings;
        private PermissionServiceClient client = new PermissionServiceClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        //用户登录类
        UserLogin login;
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
            // organClient.GetCompanyActivedAsync("");
            //登录控件禁用  在加载完组织架构后 再激活
            this.OK.IsEnabled = false;

            settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("SYS_PostInfo"))
            {
                if (settings["SYS_PostInfo"] != null)
                {
                    App.Current.Resources.Add("SYS_PostInfo", settings["SYS_PostInfo"]);
                    App.Current.Resources.Add("SYS_CompanyInfo", settings["SYS_CompanyInfo"]);
                    App.Current.Resources.Add("SYS_DepartmentInfo", settings["SYS_DepartmentInfo"]);
                }
            }

            if (settings.Contains("UserName"))
            {
                UserName.Text = Convert.ToString(settings["UserName"]);
                paw.Password = Convert.ToString(settings["UserPWD"]);               
            }


            //新平台测试字典使用
            isDict = true;
            isHR = true;
            RefreshBtn();
            //LoadDicts();
            
        }


        #endregion

        public void RefreshBtn()
        {
            this.OK.IsEnabled = isHR && isDict;
        }
        #region 窗体加载完成事件
        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            UserName.Focus();
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Style = (Style)Application.Current.Resources["GridStyle4"];
        }
        #endregion

        #region 获取组织架构
        void organClient_GetDepartmentActivedCompleted(object sender, GetDepartmentActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_DepartmentInfo", e.Result.ToList());
                    string strEmployeeID = string.Empty;
                    if (CurrentContext.Common.CurrentLoginUserInfo != null)
                    {
                        strEmployeeID = CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    }
                    organClient.GetPostActivedAsync(strEmployeeID);
                }
            }
        }

        void organClient_GetPostActivedCompleted(object sender, GetPostActivedCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_PostInfo", e.Result.ToList());

                    MainPage mainPage = new MainPage();

                    //AppConfig._CurrentStyleCode = 1;
                    Common.ParentLayoutRoot = mainPage.LayoutRoot;
                    App.Navigation(mainPage);
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
                    string strEmployeeID = string.Empty;
                    if (CurrentContext.Common.CurrentLoginUserInfo != null)
                    {
                        strEmployeeID = CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    }
                    organClient.GetDepartmentActivedAsync(strEmployeeID);
                }
            }
        }        
        #endregion

        #region 登录按钮
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
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

            //StrPwd = Utility.AESEncrypt(StrPwd);

            login = new UserLogin(StrUserName, StrPwd);
            login.LoginedClick += (obj, ev) =>
            {

                if (login.LoginResult)
                {
                    UserInfo = login.GetUserInfo();
                    
               
                    if (App.Current.Resources["CurrentUserID"] == null)
                    {
                        App.Current.Resources.Add("CurrentUserID", UserInfo.EMPLOYEEID);
                    }
                    settings["UserName"] = StrUserName;
                    settings["UserPWD"] = StrPwd;
                    settings.Save();

                    string strEmployeeID = string.Empty;
                    if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
                    {
                        strEmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                    }
                    organClient.GetCompanyActivedAsync(strEmployeeID);  
                }
                else
                {
                    MessageBox.Show("用户或密码错误!");
                    //用户名或密码错误
                    //     ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("USERNAMEORPASSWORDERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
            };


        }
        #endregion


        private void SaveData()
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // Request 5MB more space in bytes.
                    Int64 spaceToAdd = 15242880;
                    Int64 curAvail = store.AvailableFreeSpace;

                    // If available space is less than
                    // what is requested, try to increase.
                    if (curAvail < spaceToAdd)
                    {

                        // Request more quota space.
                        if (!store.IncreaseQuotaTo(store.Quota + spaceToAdd))
                        {
                            // The user clicked NO to the
                            // host's prompt to approve the quota increase.
                            return;
                        }
                        else
                        {

                        }
                    }
                }

                if (!settings.Contains("SYS_PostInfo"))
                {
                    settings["SYS_DepartmentInfo"] = App.Current.Resources["SYS_DepartmentInfo"];
                    settings["SYS_PostInfo"] = App.Current.Resources["SYS_PostInfo"];
                    settings["SYS_CompanyInfo"] = App.Current.Resources["SYS_CompanyInfo"];
                    settings.Save();
                }

            }
            catch (IsolatedStorageException)
            {
                // TODO: Handle that store could not be accessed.

            }


        }
        private void hyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            if (settings.Contains("SYS_DepartmentInfo"))
            {
                settings.Remove("SYS_DepartmentInfo");
            }
            if (settings.Contains("SYS_PostInfo"))
            {
                settings.Remove("SYS_PostInfo");
            }
            if (settings.Contains("SYS_CompanyInfo"))
            {
                settings.Remove("SYS_CompanyInfo");
            }

            if (App.Current.Resources.Contains("SYS_DepartmentInfo"))
            {
                App.Current.Resources.Remove("SYS_DepartmentInfo");
            }

            if (App.Current.Resources.Contains("SYS_PostInfo"))
            {
                App.Current.Resources.Remove("SYS_PostInfo");
            }
            if (App.Current.Resources.Contains("SYS_CompanyInfo"))
            {
                App.Current.Resources.Remove("SYS_CompanyInfo");
            }
        }

        protected void LoadDicts()
        {
            try
            {
                PermissionServiceClient cliet = new PermissionServiceClient();

                cliet.GetSysDictionaryByCategoryCompleted += (o, e) =>
                {
                    if (e.Error != null && e.Error.Message != "")
                    {
                        CommonFunction.ShowErrorMessage(Utility.GetResourceStr(e.Error.Message));
                    }
                    else
                    {
                        List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                        dicts = e.Result == null ? null : e.Result.ToList();
                        if (Application.Current.Resources.Contains("SYS_DICTIONARY"))
                        {
                            Application.Current.Resources.Remove("SYS_DICTIONARY");
                        }
                        Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
                        isDict = true;
                        RefreshBtn();
                       
                    }
                };
                //TODO: 按需取出字典值
                cliet.GetSysDictionaryByCategoryAsync("");
            }
            catch (Exception ex)
            {
                MessageBox.Show(Utility.GetResourceStr(ex.Message));
            }
        }
    }
}

