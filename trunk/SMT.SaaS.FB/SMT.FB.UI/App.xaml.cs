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
using System.IO.IsolatedStorage;
using SMT.FB.UI.CommForm;


namespace SMT.FB.UI
{
    public partial class App : Application
    {

        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();
        }


        public Grid rootGrid = new Grid();
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            EntryPointPage page = new EntryPointPage();
            page.Content = rootGrid;

            CultureInfo culture = new CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            // this.RootVisual = new MainPage();

            UserControl uc = new UserControl();

            this.RootVisual = page;
            //rootGrid.Style = (Style)this.Resources["loginbg"];
            InitTheme();
            // rootGrid.Children.Add(new MasterPageTest());
             rootGrid.Children.Add(new SMT.SaaS.LM.UI.AppMain.Login.Login());
        }

        private void InitTheme()
        {
            App.Current.Resources.MergedDictionaries.Clear();
            ResourceDictionary newStyle_blue = new ResourceDictionary();
            Uri uri = new Uri(string.Format("/SMT.SAAS.Themes;component/ShinyBlue.xaml"), UriKind.Relative);
            newStyle_blue.Source = uri;

            App.Current.Resources.MergedDictionaries.Add(newStyle_blue);

            ResourceDictionary newStyle_template = new ResourceDictionary();
            Uri uri_template = new Uri(string.Format("/SMT.SAAS.Themes;component/ControlTemplate.xaml"), UriKind.Relative);

            newStyle_template.Source = uri_template;
            App.Current.Resources.MergedDictionaries.Add(newStyle_template);

            
            //App.Current.Resources.MergedDictionaries.Add(ddd);
            //ResourceDictionary newStyle_blue = new ResourceDictionary();
            //Uri uri = new Uri("/SMT.SAAS.Themes;component/DeepBlue.xaml", UriKind.Relative);
            //newStyle_blue.Source = uri;

            //App.Current.Resources.MergedDictionaries.Add(newStyle_blue);

            //ResourceDictionary newStyle_template = new ResourceDictionary();
            //Uri uri_template = new Uri("/SMT.SAAS.Themes;component/ControlTemplate.xaml", UriKind.Relative);
            //newStyle_template.Source = uri_template;
            //App.Current.Resources.MergedDictionaries.Add(newStyle_template);


        }

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
            if (!System.Diagnostics.Debugger.IsAttached)
            {

                // 注意: 这使应用程序可以在已引发异常但尚未处理该异常的情况下
                // 继续运行。 
                // 对于生产应用程序，此错误处理应替换为向网站报告错误
                // 并停止应用程序。
                e.Handled = true;
                MessageBox.Show(e.ExceptionObject.Message);
                //Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });


            }
            else
            {
                MessageBox.Show(e.ExceptionObject.Message + e.ExceptionObject.StackTrace + e.ExceptionObject.ToString());
            }
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
                //System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");

                ChildWindow errorWin = new ErrorWindow("Unhandled Error in Silverlight Application", errorMsg);
                errorWin.Show();
            }
            catch (Exception)
            {
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CBAll = sender as CheckBox;
            var grid = AppConfig.FindParentControl<DataGrid>(CBAll);
            if (grid != null)
            {
                GridHelper.HandleAllCheckBoxClick(grid, "checkbox", CBAll.IsChecked.GetValueOrDefault());
            }
        }

        // internal static void Navigation(MainPage mainPage)
        // {
        //   throw new NotImplementedException();
        //}
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
