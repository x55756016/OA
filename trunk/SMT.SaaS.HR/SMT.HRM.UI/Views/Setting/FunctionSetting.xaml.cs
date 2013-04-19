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
using System.Windows.Navigation;
using SMT.HRM.UI.Form.Setting;
using SMT.SaaS.FrameworkUI;
using SMT.HRM.UI.CommForm;
using System.Windows.Browser;

namespace SMT.HRM.UI.Views.Setting
{
    public partial class FunctionSetting : Page
    {
        public FunctionSetting()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FormTitleName.TextTitle.Text = e.Uri.ToString();
        }

        private void ButtonSetUserLimit_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("http://172.16.1.3/SmtOnline/Application/System/TestPage.html#/SysUserManagement"), "_blank");

            SMT.Saas.Tools.HrCommonServiceWS.HrCommonServiceClient client = new Saas.Tools.HrCommonServiceWS.HrCommonServiceClient();
            client.GetAppConfigByNameAsync("PermissionSystemPath");

            client.GetAppConfigByNameCompleted += new EventHandler<Saas.Tools.HrCommonServiceWS.GetAppConfigByNameCompletedEventArgs>(client_GetAppConfigByNameCompleted);
        }

        void client_GetAppConfigByNameCompleted(object sender, Saas.Tools.HrCommonServiceWS.GetAppConfigByNameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string _html = e.Result.ToString();

                System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(_html), "_blank");
            }
        }


        private void ButtonSetColor_Click(object sender, RoutedEventArgs e)
        {
            Load();
        }

        private void Load()
        {
            SetPageColor mode = new SetPageColor();
            EntityBrowser browser = new EntityBrowser(mode);
            mode.MinWidth = 300.0;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }

        private void btSystemSet_Click(object sender, RoutedEventArgs e)
        {
            SystemParamSetForm paramset = new SystemParamSetForm();
            EntityBrowser browser = new EntityBrowser(paramset);
            paramset.MinHeight = 400;
            browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { });
        }
    }
}
