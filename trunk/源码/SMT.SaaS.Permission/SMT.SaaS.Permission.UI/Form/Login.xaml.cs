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
using SMT.Saas.Tools.FileUploadWS;
//
using SMT.SaaS.FrameworkUI;
using SMT.SaaS.FrameworkUI.ChildWidow;
using SMT.SAAS.Main.CurrentContextLoadData;
using System.Collections.ObjectModel;


namespace SMT.SaaS.Permission.UI.Form
{
    public partial class Login : ChildWindow
    {
        private PermissionServiceClient client = new PermissionServiceClient();
        private PersonnelServiceClient personelClient = new PersonnelServiceClient();
        private OrganizationServiceClient organClient = new OrganizationServiceClient();
        private FileUploadManagerClient fileclient = new FileUploadManagerClient();
        
        List<V_UserPermissionUI> Permission = new List<V_UserPermissionUI>();
        UserLogin login;
        T_SYS_USER UserInfo = new T_SYS_USER();
        public Login()
        {
            InitializeComponent();

            //client.GetFlowUserInfoByRoleIDAsync("1b694223-19cf-48e3-93bd-7746021396c0");
            client.GetSysLeftMenuFilterPermissionToNewFrameAsync("DAD32DB2-B07A-49b1-9710-61158D81B863");
            
            client.GetSystemTypeByUserIDAsync("DAD32DB2-B07A-49b1-9710-61158D81B863", "");
            //client.GetSysLeftMenuFilterPermissionToNewFrameAsync("bfd9528c-1f4e-4b06-b9f8-704364d04f56");
            this.Loaded += new RoutedEventHandler(Login_Loaded);
            organClient.GetCompanyAllCompleted += new EventHandler<GetCompanyAllCompletedEventArgs>(organClient_GetCompanyAllCompleted);
            organClient.GetDepartmentAllCompleted += new EventHandler<GetDepartmentAllCompletedEventArgs>(organClient_GetDepartmentAllCompleted);
            organClient.GetPostAllCompleted += new EventHandler<GetPostAllCompletedEventArgs>(organClient_GetPostAllCompleted);
            //organClient.GetCompanyAllAsync("");
            //client.GetUserByEmployeeIDAsync("f12547ca-f2c5-44d5-aee1-f65cfb36761e");
            //client.GetSysLeftMenuFilterPermissionToNewFrameAndPermissionAsync("39e1cd45-5b9f-4958-b0fd-0bec222d22b5", "0");
            client.GetSysLeftMenuFilterPermissionToNewFrameAndPermissionCompleted += new EventHandler<GetSysLeftMenuFilterPermissionToNewFrameAndPermissionCompletedEventArgs>(client_GetSysLeftMenuFilterPermissionToNewFrameAndPermissionCompleted);

            //client.GetSysLeftMenuFilterPermissionToNewFrameAsync("4f07bde7-050e-42eb-ab3d-3852a3b64b3d");
            //ObservableCollection<string> ListDict = new ObservableCollection<string>(); //字典列表
            //ListDict.Add("SYSTEMTYPE");
            //client.GetDictionaryByCategoryArrayAsync(ListDict);
            //this.OKButton.IsEnabled = false;
            //fileclient.DownloadCompleted += new EventHandler<DownloadCompletedEventArgs>(fileclient_DownloadCompleted);
            //fileclient.DownloadAsync(@"http://portal.smt-online.net/Services/System\UpLoadFiles\集团本部\Platform\News\胡主席与黄总等企业家合影（胡锦涛主席右后方第二人为黄文辉董事长）.jpg");
            //client.GetUserPermissionByUserToUICompleted += new EventHandler<GetUserPermissionByUserToUICompletedEventArgs>(client_GetUserPermissionByUserToUICompleted);
            //client.GetUserPermissionByUserToUIAsync("2ec631ed-0fc0-4b7f-a86c-28581eeab068");
            //client.GetSysLeftMenuFilterPermissionToNewFrameAsync("d532b00b-b199-46e8-9c39-2e0d2fcd2a02");
            //client.getFbAdminByEmployeeIDAsync("18c76f63-d069-49a5-9a03-4969fc0acf15");
            //client.UserLoginAsync("ceshi1","aaa");
        }

        void client_GetSysLeftMenuFilterPermissionToNewFrameAndPermissionCompleted(object sender, GetSysLeftMenuFilterPermissionToNewFrameAndPermissionCompletedEventArgs e)
        {
            if (e.Result != null)
            { 

            }
        }

        void fileclient_DownloadCompleted(object sender, DownloadCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void client_GetUserPermissionByUserToUICompleted(object sender, GetUserPermissionByUserToUICompletedEventArgs e)
        {
            if (e.Result != null)
                Permission = e.Result.ToList();
            var ents = from ent in Permission
                       where ent.EntityMenuID == "ccf0b182-a339-4276-80ce-2b644e426a91"
                       select ent;
        }
        

       

        
        void Login_Loaded(object sender, RoutedEventArgs e)
        {
            App currentApp = (App)Application.Current;
            currentApp.rootGrid.Style = (Style)Application.Current.Resources["GridStyle4"];
        }

        #region 获取组织架构


        void organClient_GetPostAllCompleted(object sender, GetPostAllCompletedEventArgs e)
        {
            //this.OKButton.IsEnabled = true;
            //if (e.Error == null)
            //{
            //    if (e.Result != null)
            //    {
            //        App.Current.Resources.Add("SYS_PostInfo", e.Result.ToList());
            //        this.OKButton.IsEnabled = true;
            //        //if(IsFinished)
            //    }
            //}
        }

        void organClient_GetDepartmentAllCompleted(object sender, GetDepartmentAllCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_DepartmentInfo", e.Result.ToList());
                    //organClient.GetPostAllAsync("");
                }
            }
        }

        void organClient_GetCompanyAllCompleted(object sender, GetCompanyAllCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    App.Current.Resources.Add("SYS_CompanyInfo", e.Result.ToList());
                    organClient.GetDepartmentAllAsync("");
                }
            }

        }
        #endregion

        
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            
            string StrUserName = "";
            string StrPwd = "";
            StrUserName = this.nam.Text.ToString().Trim();
            StrPwd = this.paw.Password.ToString().Trim();
            if (string.IsNullOrEmpty(StrUserName))
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "用户名不能为空", Utility.GetResourceStr("CONFIRMBUTTON"));
                this.nam.Focus();
                return;
            }
            if (string.IsNullOrEmpty(StrPwd))
            {
                ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), "密码不能为空", Utility.GetResourceStr("CONFIRMBUTTON"));
                paw.Focus();
                return;
            }
            //client.SysUserInfoUpdateByUserIdandUsernameAsync("c16f11e6-6020-479b-970f-484f5f308b7e", "guojing", "33333");
            //client.UserLoginAsync(StrUserName,common);
            
            string aa="";
            //client.GetSystemTypeByUserIDAsync("70c153bb-111a-48ad-b605-541fb04b9497", aa);
            UserLogin login = new UserLogin(StrUserName, StrPwd);
            login.LoginedClick += (obj, ev) =>
            {
                //T_SYS_USER aa = new T_SYS_USER();
                if (login.LoginResult)
                {
                    string StrFlag = "";
                    string id="";
                    UserInfo = login.GetUserInfo();
                    MainPage mainPage = new MainPage();
                    Common.ParentLayoutRoot = mainPage.LayoutRoot;
                    AppConfig._CurrentStyleCode = 1;
                    Common.ParentLayoutRoot = mainPage.LayoutRoot;
                    App.Navigation(mainPage);
                    App.MainPage = mainPage;
                    if (App.Current.Resources["CurrentUserID"] == null)
                    {
                        App.Current.Resources.Add("CurrentUserID", UserInfo.EMPLOYEEID);
                    }
                    //Common.CurrentConfig.CurrentUser.UserInfo = login.User;
                    //client.GetUserPermissionByUserToUIAsync(UserInfo.SYSUSERID);
                    //client.GetUserPermissionByUserAsync(login.User.SYSUSERID);
                    //client.GetUserPermissionByUserToUIAsync(UserInfo.SYSUSERID);
                    //personelClient.GetEmployeeDetailByIDAsync(login.User.EMPLOYEEID);
                    //client.GetEntityMenuByUserAsync("1", login.User.SYSUSERID, StrFlag);
                    UserInfo.REMARK = "test";
                    //client.SysUserInfoUpdateAsync(UserInfo);
                    //client.SysUserInfoUpdateByUserIdandUsernameAsync("","guojing","33333");
                }
                else
                {
                    //用户名或密码错误
                    ComfirmWindow.ConfirmationBox(Utility.GetResourceStr("CONFIRMINFO"), Utility.GetResourceStr("USERNAMEORPASSWORDERROR"), Utility.GetResourceStr("CONFIRMBUTTON"));
                }
            };
            //if (!string.IsNullOrEmpty(StrUserName) && !string.IsNullOrEmpty(StrPwd))
            //{
            //    client.GetUserInfoAsync(this.nam.Text);
            //}
        }

        
        void client_SysUserLoginRecordInfoAddCompleted(object sender, SysUserLoginRecordInfoAddCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result)
                {
                    //Common.CurrentConfig.CurrentEmpploy.UserLoginRecord = tmpRecord;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            client.GetSysLeftMenuFilterPermissionToNewFrameAsync("85b414ab-87b3-4740-aef4-1d89f3f380cc");
            //DialogResult = false;
        }


        
    }
}

