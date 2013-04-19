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

using SMT.Saas.Tools.PermissionWS;
using System.Globalization;
using System.Threading;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.HRM.UI
{
    public partial class Rein : ChildWindow
    {
        public Rein()
        {
            InitializeComponent();
            client.GetUserInfoCompleted += new EventHandler<GetUserInfoCompletedEventArgs>(client_GetUserInfoCompleted);
            client.GetUserPermissionByUserCompleted += new EventHandler<GetUserPermissionByUserCompletedEventArgs>(client_GetUserPermissionByUserCompleted);

            personelClient.GetEmployeeDetailByIDCompleted += new EventHandler<SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs>(personelClient_GetEmployeeDetailByIDCompleted);
        }

        void personelClient_GetEmployeeDetailByIDCompleted(object sender, SMT.Saas.Tools.PersonnelWS.GetEmployeeDetailByIDCompletedEventArgs e)
        {
            if (e.Result != null)
            {
               // SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.EmployeeInfo = e.Result;
            }
        }
        private PermissionServiceClient client = new PermissionServiceClient();
        private SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient personelClient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();

        private void OK_buton(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nam.Text) || string.IsNullOrEmpty(paw.Password))
            {
                MessageBox.Show("用户名或密码不能为空！");
            }
            else
            {
                client.GetUserInfoAsync(nam.Text);
            }
        }
        void client_GetUserInfoCompleted(object sender, GetUserInfoCompletedEventArgs e)
        {
            T_SYS_USER user = e.Result;
            if (user != null && user.PASSWORD == paw.Password)
            {
               // SMT.SAAS.Main.CurrentContext.Common.CurrentConfig.CurrentUser.UserInfo = user;
                //TODO: 暂时注销用户权限
                client.GetUserPermissionByUserAsync(user.SYSUSERID);
                
                personelClient.GetEmployeeDetailByIDAsync(user.EMPLOYEEID);

                MainPage mainPage = new MainPage();
                App.Navigation(mainPage);
            }
            else
            {
                MessageBox.Show("用户名或密码错误！");
            }
        }
        void client_GetUserPermissionByUserCompleted(object sender, GetUserPermissionByUserCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                List<SMT.Saas.Tools.PermissionWS.V_Permission> entPermList = e.Result.ToList();

                foreach (var fent in entPermList)
                {
                    SMT.SaaS.LocalData.V_UserPermissionUI tps = new SMT.SaaS.LocalData.V_UserPermissionUI();
                    tps = Common.CloneObject<SMT.Saas.Tools.PermissionWS.V_Permission, SMT.SaaS.LocalData.V_UserPermissionUI>(fent, tps);
                    Common.CurrentLoginUserInfo.PermissionInfoUI.Add(tps);
                }
            }
        }
        private void Cen_B(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

            CultureInfo culture = new CultureInfo(((RadioButton)sender).Tag.ToString());
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            SMT.SaaS.Globalization.Localization.UiCulture = culture;

            SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
            //string s = SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr.GetString("BANKID", culture);
        }
    }
}

