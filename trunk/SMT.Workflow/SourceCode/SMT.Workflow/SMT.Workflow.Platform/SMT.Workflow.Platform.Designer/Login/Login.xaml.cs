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

using SMT.Workflow.Platform.Designer.UControls;
using SMT.Workflow.Platform.Designer.Views;
//using SMT.Workflow.Platform.Designer.PermissionService;
using SMT.Workflow.Platform.Designer.Utils;

using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Main.CurrentContextLoadData;



namespace SMT.Workflow.Platform.Designer.Login
{
    public partial class Login : UserControl
    {
        //PermissionServiceClient client = new PermissionServiceClient();
        #region 龙康才
        SMT.Saas.Tools.PermissionWS.PermissionServiceClient client = new Saas.Tools.PermissionWS.PermissionServiceClient();
        //用户登录类
        UserLogin login;
        //用户信息
        SMT.Saas.Tools.PermissionWS.T_SYS_USER loginUserInfo = new Saas.Tools.PermissionWS.T_SYS_USER();
        #endregion
        SMT.Saas.Tools.PermissionWS.T_SYS_USER UserInfo = new SMT.Saas.Tools.PermissionWS.T_SYS_USER();

        public Login()
        {
            InitializeComponent();

            //client.UserLoginCompleted += new EventHandler<UserLoginCompletedEventArgs>(client_UserLoginCompleted);
        }

        //private void client_UserLoginCompleted(object sender, UserLoginCompletedEventArgs e)
        //{
        //    //try
        //    //{
        //        if (e != null)
        //        {
        //            Utility.CurrentUser = e.Result;
        //            AppContext.Host.SetRoot(new MainPage());
        //        }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    MessageBox.Show(ex.Message.ToString());
        //    //    btnLogin.IsEnabled = true;
        //    //    pBar.Stop();
        //    //}
        //}

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtUserId.Text.Trim()))
                {
                    //MessageBox.Show("用户名不能为空!");
                    ComfirmWindow.ConfirmationBox("提示信息", "用户名不能为空!", "确定");
                    txtUserId.Focus();
                    return;
                }
              
                if (string.IsNullOrEmpty(txtPassword.Password.Trim()))
                {
                    //MessageBox.Show("用户密码不能为空!");
                    ComfirmWindow.ConfirmationBox("提示信息", "用户密码不能为空!", "确定");
                    txtPassword.Focus();
                    return;
                }

                btnLogin.IsEnabled = false;
                pBar.Start();
                #region 龙康才
                login = new UserLogin(txtUserId.Text.Trim(), txtPassword.Password.Trim());
                login.LoginedClick += (obj, ev) =>
                {
                    if (login.LoginResult)
                    {
                        loginUserInfo = login.GetUserInfo();
                        Utility.CurrentUser = new  T_SYS_USER();
                        Utility.CurrentUser.CREATEDATE = loginUserInfo.CREATEDATE;
                        Utility.CurrentUser.CREATEUSER = loginUserInfo.CREATEUSER;
                        Utility.CurrentUser.EMPLOYEECODE = loginUserInfo.EMPLOYEECODE;
                        Utility.CurrentUser.EMPLOYEEID = loginUserInfo.EMPLOYEEID;
                        Utility.CurrentUser.EMPLOYEENAME = loginUserInfo.EMPLOYEENAME;
                       // Utility.CurrentUser.EntityKey = loginUserInfo.EntityKey;
                        Utility.CurrentUser.ISENGINEMANAGER = loginUserInfo.ISENGINEMANAGER;
                        Utility.CurrentUser.ISFLOWMANAGER = loginUserInfo.ISFLOWMANAGER;
                        Utility.CurrentUser.ISMANGER = loginUserInfo.ISMANGER;
                        Utility.CurrentUser.OWNERCOMPANYID = loginUserInfo.OWNERCOMPANYID;
                        Utility.CurrentUser.OWNERDEPARTMENTID = loginUserInfo.OWNERDEPARTMENTID;
                        Utility.CurrentUser.OWNERID = loginUserInfo.OWNERID;
                        Utility.CurrentUser.OWNERPOSTID = loginUserInfo.OWNERPOSTID;
                        Utility.CurrentUser.PASSWORD = loginUserInfo.PASSWORD;
                        Utility.CurrentUser.REMARK = loginUserInfo.REMARK;
                        Utility.CurrentUser.STATE = loginUserInfo.STATE;
                        Utility.CurrentUser.SYSUSERID = loginUserInfo.SYSUSERID;
                      //  Utility.CurrentUser.T_SYS_USERROLE = loginUserInfo.T_SYS_USERROLE;
                        Utility.CurrentUser.UPDATEDATE = loginUserInfo.UPDATEDATE;
                        Utility.CurrentUser.UPDATEUSER = loginUserInfo.UPDATEUSER;
                        Utility.CurrentUser.USERNAME = loginUserInfo.USERNAME;
                      
                        AppContext.Host.SetRoot(new MainPage());
                    }
                    else
                    {
                        //用户名或密码错误
                        ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("USERNAMEORPASSWORDERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                    }
                };
                #endregion
               // client.UserLoginAsync(txtUserId.Text.Trim(), UserLogin.Encrypt(txtPassword.Password.Trim()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                btnLogin.IsEnabled = true;
                pBar.Stop();
            }
        }
    }
}
