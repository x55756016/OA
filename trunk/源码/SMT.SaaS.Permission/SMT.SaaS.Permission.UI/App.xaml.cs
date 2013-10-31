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
using System.IO.IsolatedStorage;
//using SMT.SaaS.FrameworkUI.Themes;


namespace SMT.SaaS.Permission.UI
{
    public partial class App : Application
    {
        private IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        public static UIElement MainPage;
        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();            
            //设置从WCF服务取异常信息
            bool registerResult = WebRequest.RegisterPrefix("http://", System.Net.Browser.WebRequestCreator.ClientHttp);
            bool httpsResult = WebRequest.RegisterPrefix("https://", System.Net.Browser.WebRequestCreator.ClientHttp);

            
        }
        private void InitComonConverter()
        {            
            Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            //########################## end 加载golobal资源
            //加载OA项目的转换器DictionaryConverter
            Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.Permission.UI.DictionaryConverter());
          
            Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
        }

        private void InitTheme()
        {
            ResourceDictionary newStyle3 = new ResourceDictionary();
            Uri uri = new Uri("/SMT.SAAS.Themes;component/ShinyBlue.xaml", UriKind.Relative);
            newStyle3.Source = uri;
            Application.Current.Resources.MergedDictionaries.Add(newStyle3);

            ResourceDictionary newStyle_toolkit = new ResourceDictionary();
            Uri uri_toolkit = new Uri("/SMT.SAAS.Themes;component/ToolKitResource.xaml", UriKind.Relative);
            newStyle_toolkit.Source = uri_toolkit;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_toolkit);

            ResourceDictionary newStyle_template = new ResourceDictionary();
            Uri uri_template = new Uri("/SMT.SAAS.Themes;component/ControlTemplate.xaml", UriKind.Relative);
            newStyle_template.Source = uri_template;
            Application.Current.Resources.MergedDictionaries.Add(newStyle_template);
        }
        

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            EntryPointPage page = new EntryPointPage();
            page.Content = rootGrid;
            CultureInfo culture = new CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;

            //TODO: 按需取出字典值
            LoadDicts();

            this.RootVisual = page;
            InitTheme();
            InitComonConverter();
            rootGrid.Children.Add(new SMT.SaaS.Permission.UI.Form.Login());         
        }
        protected void LoadDicts()
        {
            PermissionServiceClient cliet = new PermissionServiceClient();
            cliet.GetSysDictionaryByCategoryCompleted += (o, e) =>
            {
                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                dicts = e.Result == null ? null : e.Result.ToList();
                if(dicts != null)
                    Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
     
            };
            //TODO: 按需取出字典值
            cliet.GetSysDictionaryByCategoryAsync("");
            //cliet.GetSysDictionaryByCategoryByUpdateDateAsync("",DateTime.Now);

            
        }
        public Grid rootGrid = new Grid();

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
                Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
        }
        private void DropDownToggle_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            AutoCompleteBox acb = null;
            while (fe != null && acb == null)
            {
                fe = VisualTreeHelper.GetParent(fe) as FrameworkElement;
                acb = fe as AutoCompleteBox;
            }
            if (acb != null)
            {
                if (string.IsNullOrEmpty(acb.SearchText))
                {
                    acb.Text = string.Empty;
                }
                acb.IsDropDownOpen = !acb.IsDropDownOpen;
            }

        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CBAll = sender as CheckBox;
            var grid = Utility.FindParentControl<DataGrid>(CBAll);
            if (grid != null)
            {
                GridHelper.HandleAllCheckBoxClick(grid, "checkbox", CBAll.IsChecked.GetValueOrDefault());
            }
        }

        private void CheckBoxS_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("CheckBox事件！");
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
