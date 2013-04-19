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
using System.Diagnostics.CodeAnalysis;
using SMT.HRM.UI.Views.Login;
using SMT.SaaS.FrameworkUI;
using System.IO.IsolatedStorage;
using SMT.Saas.Tools.OrganizationWS;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.HRM.UI
{
    public partial class App : Application
    {
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        public App()
        {
            InitializeComponent();
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            

            //设置从WCF服务取异常信息
            bool registerResult = WebRequest.RegisterPrefix("http://", System.Net.Browser.WebRequestCreator.ClientHttp);
            bool httpsResult = WebRequest.RegisterPrefix("https://", System.Net.Browser.WebRequestCreator.ClientHttp);


        }

        private void InitTheme()
        {
            
            ResourceDictionary newStyle3 = new ResourceDictionary();
            Uri uri = new Uri("/SMT.SAAS.Themes;component/ShinyBlue.xaml", UriKind.Relative);
            newStyle3.Source = uri;

            App.Current.Resources.MergedDictionaries.Add(newStyle3);
            ResourceDictionary newStyle_template = new ResourceDictionary();

            Uri uri_template = new Uri("/SMT.SAAS.Themes;component/ControlTemplate.xaml", UriKind.Relative);
            newStyle_template.Source = uri_template;

            App.Current.Resources.MergedDictionaries.Add(newStyle_template);
        }

        private void InitComonConverter()
        {
            Application.Current.Resources.Add("ResourceWrapper", new SMT.HRM.UI.ResourceWrapper());
            Application.Current.Resources.Add("DictionaryConverter", new SMT.HRM.UI.DictionaryConverter());
            Application.Current.Resources.Add("CustomDateConverter", new SMT.HRM.UI.CustomDateConverter());
            Application.Current.Resources.Add("CheckConverter", new SMT.HRM.UI.CheckConverter());
            Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());            
        }

        public Grid rootGrid = new Grid();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EntryPointPage page = new EntryPointPage();
            page.Content = rootGrid;
            //TODO: 按需取出字典值
            LoadDicts();
            
            this.RootVisual = page;
            InitTheme();
            InitComonConverter();
             rootGrid.Children.Add(new Login());  
            //rootGrid.Children.Add(new Views.Salary.Payment());
            
            //测试
            //OrganizationWS.OrganizationServiceClient oclient = new SMT.Saas.Tools.OrganizationWS.OrganizationServiceClient();
            //oclient.GetCompanyByEntityPermAsync("4afd1d50-b84d-4ea3-8c81-0078c35fea5e", "0", "T_FB_CHARGEAPPLYMASTER");

            //PersonnelWS.PersonnelServiceClient pclient = new SMT.Saas.Tools.PersonnelWS.PersonnelServiceClient();
            //System.Collections.ObjectModel.ObservableCollection<string> ids = new System.Collections.ObjectModel.ObservableCollection<string>();
            //ids.Add("67A8D37C-15C6-4002-A740-1787EC5CEEDF");
            //ids.Add("11CC84DF-2B78-499f-AF14-FB8F36A5FE22");
            //pclient.GetEmployeeByIDsAsync(ids);

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
                        Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(e.Error.Message));
                    }
                    else
                    {
                        List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                        dicts = e.Result == null ? null : e.Result.ToList();
                        Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
                    }
                };
                //TODO: 按需取出字典值
                cliet.GetSysDictionaryByCategoryAsync("CHECKSTATE");
            }
            catch (Exception ex)
            {
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), Utility.GetResourceStr(ex.Message));
            }
        }

        //protected void LoadCompanyInfo()
        //{
        //    OrganizationServiceClient client = new OrganizationServiceClient();
        //    client.GetCompanyActivedCompleted += (o, e) =>
        //    {
        //        List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY> dicts = new List<SMT.Saas.Tools.OrganizationWS.T_HR_COMPANY>();
        //        dicts = e.Result == null ? null : e.Result.ToList();
        //        Application.Current.Resources.Add("SYS_CompanyInfo", dicts);
        //        //LoadDepartmentInfo();
        //    };
        //    client.GetCompanyActivedAsync(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID);
        //}

        public static void Navigation(UserControl newPage)
        {
            //获取当前的Appliaction实例 
            App currentApp = (App)Application.Current;
            // 修改当前显示页面内容. 
            currentApp.rootGrid.Children.Clear();
            currentApp.rootGrid.Children.Add(newPage);
        }

        private void Application_Exit(object sender, EventArgs e)
        {

        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // 如果应用程序是在调试器外运行的，则使用浏览器的
            // 异常机制报告该异常。在 IE 上，将在状态栏中用一个 
            // 黄色警报图标来显示该异常，而 Firefox 则会显示一个脚本错误。
            MessageBox.Show(e.ExceptionObject.ToString());
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // 注意: 这使应用程序可以在已引发异常但尚未处理该异常的情况下
                // 继续运行。 
                // 对于生产应用程序，此错误处理应替换为向网站报告错误
                // 并停止应用程序。
                e.Handled = true;
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });

                
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
                //System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");

                ComfirmWindow.ConfirmationBoxs("Unhandled Error in Silverlight Application", Utility.GetResourceStr(errorMsg),
                        Utility.GetResourceStr("CONFIRM"), MessageIcon.Error);
            }
            catch (Exception)
            {
            }
        }

        private void SelectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            
            CheckBox CBAll = sender as CheckBox;
            var grid = Utility.FindParentControl<DataGrid>(CBAll);
            if (grid != null)
            {                
                GridHelper.HandleAllCheckBoxClick(grid, "checkbox", CBAll.IsChecked.GetValueOrDefault());  
            }
        }

        /// <summary>
        /// 系统主入口
        /// </summary>
        public class EntryPointPage : UserControl
        {
            public UIElement Content
            {
                get { return base.Content; }
                set { base.Content = value; }
            }
        }
    }
}
