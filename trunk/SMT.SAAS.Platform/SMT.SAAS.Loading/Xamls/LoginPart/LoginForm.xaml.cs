using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.IO.IsolatedStorage;
using System.Windows.Resources;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using SMT.SAAS.Main.CurrentContext;
using SMT.SAAS.Platform.MainUIWS;
using SMT.SAAS.Platform.EmployeeInfoWS;
using SMT.SaaS.LocalData;
using System.Windows.Browser;

namespace SMT.SAAS.Platform.Xamls.LoginPart
{
    public partial class LoginForm : Grid
    {
        private ItemBox parentWindow;
        private V_EMPLOYEEDETAIL employee;
        private SMT.SAAS.Platform.EmployeeInfoWS.V_UserLogin sysUser;
        private UIElement MainPage;
        private bool isFirstUser = true;
        private bool isUserloginComplete = false;
        private Assembly asmMain = null;
        #region 全局时间，用来记录操作时间
        DateTime dtstart = DateTime.Now;
        DateTime dtend = DateTime.Now;
        #endregion
        private const string USERKEY = "USERNAME";
        /// <summary>
        /// 本地xap保存的文件夹名称
        /// </summary>
        private  string ApplicationPath = "SmtPortal";

        /// <summary>
        /// 本地版本文件路径
        /// </summary>
        private string dllVersionFilePath =@"SmtPortal/DllVersion.xml";
        /// <summary>
        /// 检测是否已开始登录
        /// </summary>
        private bool isAutoloaded = false;

        EmployeeInfoServiceClient EmployeeInfoClient;
        MainUIServicesClient sysloginClinet;

        private MainPagePartManager MainPageManeger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(LoginForm_Loaded);
            this.KeyUp += new System.Windows.Input.KeyEventHandler(LoginForm_KeyUp);
            sysloginClinet = new MainUIServicesClient();
            sysloginClinet.SystemLoginCompleted += new EventHandler<SystemLoginCompletedEventArgs>(sysloginClinet_SystemLoginCompleted);

            EmployeeInfoClient = new EmployeeInfoServiceClient();
            EmployeeInfoClient.getEmployeeInfobyLoginCompleted += new EventHandler<getEmployeeInfobyLoginCompletedEventArgs>(EmployeeInfoClient_getEmployeeInfobyLoginCompleted); ;

            employee = new V_EMPLOYEEDETAIL();
            MainPageManeger = new MainPagePartManager();
            MainPageManeger.Loginform = this;
            MainPageManeger.isFirstUser = this.isFirstUser;
            MainPageManeger.dllVersionFilePath = this.dllVersionFilePath;
            MainPageManeger.LoadCompleted += new EventHandler(loadMainPage_LoadCompleted);
            MainPageManeger.UpdateDllCompleted += new EventHandler(MainPageManeger_UpdateDllCompleted);
        }

        #region 页面控件事件

        void LoginForm_Loaded(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;

            if (App.AppSettings.Contains(USERKEY))
                txbUserName.Text = App.AppSettings[USERKEY].ToString();

            if (Application.Current.Resources["username"] != null
                    && Application.Current.Resources["userpwd"] != null)
            {
                this.txbUserName.Text = Application.Current.Resources["username"].ToString();
                this.txbUserPassword.Password = Application.Current.Resources["userpwd"].ToString();
                isAutoloaded = true;
                MainPageManeger.isAutoloaded = this.isAutoloaded;
            }

            if (string.IsNullOrEmpty(txbUserName.Text))
            {
                txbUserName.Focus();
                //txbUserName.Text = "smtwangl";
                //txbUserPassword.Password = "wl2012";
            }
            else
            {
                txbUserPassword.Focus();
            }
            if (IosManager.ExistsFile(dllVersionFilePath))
            {
                //判断所有文件是否在本地存在
                SMT.SaaS.LocalData.Tables.V_UserLogin us = new SaaS.LocalData.Tables.V_UserLogin();
                //us = SMT.SaaS.LocalData.ViewModel.V_UserLoginVM.Get_V_UserLogin(txbUserName.Text);
                if (IsolatedStorageSettings.ApplicationSettings.Contains("user"))
                {
                    us = IsolatedStorageSettings.ApplicationSettings["user"] as SMT.SaaS.LocalData.Tables.V_UserLogin;
                }
                if (!string.IsNullOrEmpty(us.EMPLOYEEID))
                {
                    isFirstUser = false;
                    MainPageManeger.isFirstUser = false;
                    NotifyUserMessage("系统正在检测更新，请您稍等......");
                    MainPageManeger.dllVersionUpdataCheck();
                }
                else
                {
                    isFirstUser = true;
                    btnLogin.IsEnabled = true;
                    NotifyUserMessage("您第一次使用系统，需要下载较多内容，登录后请您耐心等候");
                }
            }
            else
            {
                isFirstUser = true;
            }

            if (isFirstUser)
            {
                btnLogin.IsEnabled = true;
                txtLoadingMessage.Text = "您第一次使用系统，需要下载较多内容，登录后请您耐心等候";
            }
        }

        void LoginForm_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                userloading();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            userloading();
        }

        private void txbYZCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            if (!string.IsNullOrEmpty(txtBox.Text))
            {
                string txtCode = txtBox.Text.Trim();
                if (txtCode.Length == 4)
                {
                }
            }
        }

        public void SetparentWindow(ItemBox login)
        {
            this.parentWindow = login;
        }


        public void NotifyUserMessage(string message)
        {
            this.Dispatcher.BeginInvoke(
                           delegate
                           {
                               txtLoadingMessage.Text = message;
                           });

        }

        public void EnableLoginBtn(bool enable)
        {
            this.Dispatcher.BeginInvoke(
                           delegate
                           {
                               this.btnLogin.IsEnabled = enable;
                           });

        }

        public void setbtnLoginEnable(bool enable)
        {
            this.Dispatcher.BeginInvoke(
                           delegate
                           {
                               btnLogin.IsEnabled = enable;
                           });
        }
        #endregion

        #region "Assembly管理器事件"
        /// <summary>
        /// 更新下载所有dll完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPageManeger_UpdateDllCompleted(object sender, EventArgs e)
        {
            #region 自动登录系统
            isFirstUser = false;
            try
            {
                if (isUserloginComplete)
                {
                    MainPageManeger.RunWorkerLoadAssemblyPart();
                }
                else
                {
                    if (isFirstUser == false && isAutoloaded == true)
                    {
                        NotifyUserMessage("系统自动登录中，请您登录");
                        userloading();
                    }
                    else
                    {
                        NotifyUserMessage("系统检测更新完毕，请您登录");
                        EnableLoginBtn(true);
                    }
                }
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(" 自动登录系统出错：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            #endregion
        }

        /// <summary>
        /// 加载所有dll完成并在主页面显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void loadMainPage_LoadCompleted(object sender, EventArgs e)
        {
            try
            {
                MainPagePartManager loadmain = sender as MainPagePartManager;
                asmMain = loadmain.asmMain;

                MainPage = asmMain.CreateInstance("SMT.SAAS.Platform.Xamls.MainPage") as UIElement;
                AppContext.AppHost.SetRootVisual(MainPage);
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region 系统用户登录
        /// <summary>
        /// 系统登录
        /// </summary>
        public void userloading()
        {
            btnLogin.IsEnabled = false;
            try
            {
                if (!IosManager.CheckeSpace())
                {
                    try
                    {
                        if (!IosManager.AddSpace())
                        {
                            MessageBox.Show("请增加独立存储空间，否则系统无法运行");
                            btnLogin.IsEnabled = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("增加独立存储空间出错，请联系管理员" + ex.ToString());
                    }
                }

                if (!IosManager.ExistsFile(ApplicationPath))
                {
                    IosManager.CreatePath(ApplicationPath);
                }
                if (!IsAllFilled())
                {
                    btnLogin.IsEnabled = true;
                    return;
                }
                if (!App.AppSettings.Contains(USERKEY))
                    App.AppSettings.Add(USERKEY, txbUserName.Text);
                else
                    App.AppSettings[USERKEY] = txbUserName.Text;

                string UserPwdMD5 = string.Empty;
                if (Application.Current.Resources["userpwd"] != null)
                {
                    UserPwdMD5 = Application.Current.Resources["userpwd"].ToString();
                }
                else
                {
                    UserPwdMD5 = MD5.GetMd5String(this.txbUserPassword.Password);
                }
                sysloginClinet.SystemLoginAsync(this.txbUserName.Text, UserPwdMD5);

                txtLoadingMessage.Text = "开始登录系统，请稍等......";
                DateTime dtstart = DateTime.Now;
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("开始登录系统，请稍等......");

                //sysloginClinet.SystemLoginAsync(this.txbUserName.Text, Utility.Encrypt(this.txbUserPassword.Password));

            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统错误，请联系管理员：" + ex.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
                btnLogin.IsEnabled = true;
            }

        }

        /// <summary>
        /// 用户登录完成事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sysloginClinet_SystemLoginCompleted(object sender, SystemLoginCompletedEventArgs e)
        {
            if (e.Error != null && e.Error.Message != "")
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            else
            {
                if (e.Result == null)
                {
                    txtLoadingMessage.Text = "用户名密码错误，请重试";
                    btnLogin.IsEnabled = true;
                    return;
                }
                else
                {
                    string pwdChecker = CheckPwd(this.txbUserPassword.Password);
                    if (!string.IsNullOrEmpty(pwdChecker))
                    {
                        btnLogin.IsEnabled = true;
                        txtLoadingMessage.Text = "若已修改密码，请重新登录.";
                        this.parentWindow.NavigateToRegistration();
                        return;
                    }

                    //txtLoadingMessage.Text = "登录成功，开始获取员工信息......";
                    sysUser = new EmployeeInfoWS.V_UserLogin();
                    sysUser.EMPLOYEEID = e.Result.EMPLOYEEID;
                    sysUser.ISMANAGER = e.Result.ISMANAGER;
                    sysUser.SYSUSERID = e.Result.SYSUSERID;
                    NotifyUserMessage("登录成功，正在初始化系统，请稍等......");
                    //登录成功，获取员工信息
                    EmployeeInfoClient.getEmployeeInfobyLoginAsync(sysUser.EMPLOYEEID);

                    SMT.SaaS.LocalData.Tables.V_UserLogin us = new SaaS.LocalData.Tables.V_UserLogin();
                    us.UserName = txbUserName.Text;
                    us.EMPLOYEEID = sysUser.EMPLOYEEID;
                    us.ISMANAGER = sysUser.ISMANAGER;
                    us.SYSUSERID = sysUser.SYSUSERID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = new LoginUserInfo();
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID = us.EMPLOYEEID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID = us.SYSUSERID;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.IsManager = us.ISMANAGER == "1" ? true : false;
                    //SMT.SaaS.LocalData.ViewModel.V_UserLoginVM.SaveV_UserLogin(txbUserName.Text, us);
                    if (!IsolatedStorageSettings.ApplicationSettings.Contains("user"))
                    {
                        IsolatedStorageSettings.ApplicationSettings.Add("user", us);
                    }
                }
            }
        }
        /// <summary>
        /// 获取员工信息事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EmployeeInfoClient_getEmployeeInfobyLoginCompleted(object sender, getEmployeeInfobyLoginCompletedEventArgs e)
        {
            dtend = DateTime.Now;
            string strmsg = "登录系统完成耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);

            if (e.Error != null && e.Error.Message != "")
            {
                NotifyUserMessage("获取员工信息错误,请联系管理员");
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(e.Error.Message); MessageBox.Show(e.Error.ToString());
                SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            }
            else
            {
                if (e.Result != null)
                {
                    employee = e.Result;
                    employee.sysuser = sysUser;
                    SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("已获取到employee信息");
                    NotifyUserMessage("获取员工信息成功，正在初始化系统，请稍等......");
                    bool isAdmin = employee.sysuser.ISMANAGER == "1" ? true : false;
                    var postlist
                        = from ent in employee.EMPLOYEEPOSTS
                          orderby ent.ISAGENCY
                          select new SMT.SaaS.LocalData.V_EMPLOYEEPOSTBRIEF
                          {
                              EMPLOYEEPOSTID = ent.EMPLOYEEPOSTID,
                              POSTID = ent.POSTID,
                              PostName = ent.PostName,
                              DepartmentID = ent.DepartmentID,
                              DepartmentName = ent.DepartmentName,
                              CompanyID = ent.CompanyID,
                              CompanyName = ent.CompanyName,
                              ISAGENCY = ent.ISAGENCY,
                              POSTLEVEL = ent.POSTLEVEL
                          };

                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = SMT.SAAS.Main.CurrentContext.Common.GetLoginUserInfo(
                            employee.EMPLOYEEID, employee.EMPLOYEENAME,
                            employee.EMPLOYEECODE, employee.EMPLOYEESTATE,
                            employee.sysuser.SYSUSERID, employee.OFFICEPHONE,
                            employee.SEX, postlist.ToList(),
                            employee.WORKAGE, employee.PHOTO, isAdmin);
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI = new List<V_UserPermissionUI>();
                    //SMT.SAAS.Main.CurrentContext.LoginUserInfo.LoginRecordID = _LoginRecord;
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.UserName = txbUserName.Text;
                    if (isFirstUser == true)
                    {
                        NotifyUserMessage("登录成功，开始获取系统更新，请稍等......");
                        isUserloginComplete = true;
                        MainPageManeger.dllVersionUpdataCheck();
                    }
                    else
                    {
                        //MainPage = asmMain.CreateInstance("SMT.SAAS.Platform.Xamls.MainPage") as UIElement;
                        MainPageManeger.RunWorkerLoadAssemblyPart();
                    }
                }
                else
                {
                    txtLoadingMessage.Text = "获取员工信息出错，请联系管理员";
                    btnLogin.IsEnabled = true;
                    return;
                }

            }
        }

        #endregion

        #region 修改密码事件触发

        /// <summary>
        /// 修改密码事件触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.parentWindow.NavigateToRegistration();
        }

        #endregion

        #region 密码检测
        /// <summary>
        /// added by luojie
        /// 用与验证密码的合法性 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string CheckPwd(string password)
        {
            string rstMessage = string.Empty;//提示语
            if (password != null)
            {
                if (password.Length < 8)
                {
                    rstMessage = "为了安全，协同办公系统登录密码已要求为8-15位英文与数字的组合，请您修改密码后再登录。";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码不能小于八位");
                }

                string ptnNum = @"\D[0-9]+";//字母开头后面必须跟数字
                string ptnWord = @"[0-9][a-z_A-Z]+";//数字开头后面必须跟字母
                Match matchNum = Regex.Match(password, ptnNum);
                Match matchWord = Regex.Match(password, ptnWord);
                if (!matchNum.Success && !matchWord.Success)
                {
                    rstMessage = "为了安全，协同办公系统登录密码已要求为8-15位英文与数字的组合，请您修改密码后再登录。";
                    //Utility.ShowCustomMessage(MessageTypes.Error, "错误", "密码必须是中英文结合的");
                }
            }

            return rstMessage;
        }

        /// <summary>
        /// 检测用户名和密码是否已填写
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private bool IsAllFilled()
        {
            string pwd = this.txbUserPassword.Password;
            if (string.IsNullOrWhiteSpace(txbUserName.Text))
            {
                txtLoadingMessage.Text = "请输入用户名.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.txbUserPassword.Password))
            {
                txtLoadingMessage.Text = "请输入密码.";
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion

    }
}
