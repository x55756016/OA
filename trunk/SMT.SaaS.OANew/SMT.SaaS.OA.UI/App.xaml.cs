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
using System.Xml.Linq;
using System.Xml;

using SMT.SaaS.OA.UI.Class;
//using SMT.Saas.Tools.PermissionWS;
using SMT.Saas.Tools.PermissionWS;
using System.IO.IsolatedStorage;
using SMT.Saas.Tools.FlowDesignerWS;
using System.IO;
using System.Xml.Serialization;
using System.Text;


namespace SMT.SaaS.OA.UI
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

        public Grid rootGrid = new Grid();

        private void InitTheme()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
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
            InitTheme();
            EntryPointPage page = new EntryPointPage();
            page.Content = rootGrid;
            CultureInfo culture = new CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            SMT.SaaS.FrameworkUI.Validator.ValidatorService.ResourceMgr = SMT.SaaS.Globalization.Localization.ResourceMgr;
            // this.RootVisual = new MainPage();
            this.RootVisual = page;

            //rootGrid.Style = (Style)this.Resources["loginbg"];
            rootGrid.Children.Add(new Views.Login.Login());
            
            //已屏蔽,改为按需加载
            //安凯航,2011年7月17日
            //LoadDicts();
            //LoadDictss();
            //end

            InitComonConverter();

        }
        private void InitComonConverter()
        {
            //Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
            //Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            //########################## end 加载golobal资源
            //加载OA项目的转换器
            if (Application.Current.Resources["RentConvert"] == null)
                Application.Current.Resources.Add("RentConvert", new SMT.SaaS.OA.UI.RentFlagConverter());
            if (Application.Current.Resources["DictionaryConverter"] == null)
                Application.Current.Resources.Add("DictionaryConverter", new SMT.SaaS.OA.UI.DictionaryConverter());
            if (Application.Current.Resources["CustomDateConverter"] == null) 
                Application.Current.Resources.Add("CustomDateConverter", new SMT.SaaS.OA.UI.CustomDateConverter());
            if (Application.Current.Resources["StateConvert"] == null) 
                Application.Current.Resources.Add("StateConvert", new SMT.SaaS.OA.UI.CheckStateConverter());
            if (Application.Current.Resources["ModuleNameConverter"] == null) 
                Application.Current.Resources.Add("ModuleNameConverter", new SMT.SaaS.OA.UI.ModuleNameConverter());
            if (Application.Current.Resources["ObjectTypeConverter"] == null) 
                Application.Current.Resources.Add("ObjectTypeConverter", new SMT.SaaS.OA.UI.ObjectTypeConverter());
        }

        protected void LoadDicts()
        {

            PermissionServiceClient cliet = new PermissionServiceClient();
            cliet.GetSysDictionaryByCategoryCompleted += (o, e) =>
            {
                List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                dicts = e.Result == null ? null : e.Result.ToList();
                if (dicts != null)
                    Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
            };
            if (Application.Current.Resources["SYS_DICTIONARY"] == null)
            {
                cliet.GetAllSysDictionaryCompleted += (o, e) =>
                    {
                        List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
                        dicts = e.Result == null ? null : e.Result.ToList();
                        if (dicts != null)
                            Application.Current.Resources.Add("SYS_DICTIONARY", dicts);
                    };
                cliet.GetAllSysDictionaryAsync();
            }
            //TODO: 按需取出字典值
            //cliet.GetSysDictionaryByCategoryAsync("CHECKSTATE");
            cliet.GetSysDictionaryByCategoryAsync("");
            

            //PermissionServiceClient client = new PermissionServiceClient();
            //client.GetSysDictionaryByCategoryByUpdateDateCompleted += (o, e) =>
            //{
            //    //List<T_SYS_DICTIONARY> dicts = new List<T_SYS_DICTIONARY>();
            //    //dicts = e.Result == null ? null : e.Result.ToList();
            //    List<V_Dictionary> viewDicts = e.Result == null ? null : e.Result.ToList();
                
            //    List<T_SYS_DICTIONARY> dictionary = new List<T_SYS_DICTIONARY>();
            //    if (viewDicts != null)
            //    {
            //        foreach (V_Dictionary item in viewDicts)
            //        {
            //            T_SYS_DICTIONARY dictItem = new T_SYS_DICTIONARY();
            //            dictItem.DICTIONARYID = item.DICTIONARYID;
            //            dictItem.DICTIONARYNAME = item.DICTIONARYNAME;
            //            dictItem.DICTIONARYVALUE = item.DICTIONARYVALUE;
            //            dictItem.DICTIONCATEGORY = item.DICTIONCATEGORY;
            //            if (!string.IsNullOrEmpty(item.FATHERID))
            //            {
            //                dictItem.T_SYS_DICTIONARY2 = new T_SYS_DICTIONARY();
            //                dictItem.T_SYS_DICTIONARY2.DICTIONARYID = item.FATHERID;
                            
            //            }
            //            var ent = dictionary.Where(s => s.DICTIONARYID == item.DICTIONARYID).FirstOrDefault();
            //            if (ent != null)
            //            {
            //                dictionary.Remove(ent);
            //                dictionary.Add(dictItem);
            //            }
            //            else
            //            {
            //                dictionary.Add(dictItem);
            //            }
            //        }
            //    }
            //    if (Application.Current.Resources["SYS_DICTIONARY"] == null)
            //    {
            //        Application.Current.Resources.Add("SYS_DICTIONARY", dictionary);
            //    }
            //    //LoadCompanyInfo();
            //};
            ////TODO: 按需取出字典值
            ////client.GetSysDictionaryByCategoryAsync("");
            //DateTime dt = new DateTime();
            
            
            //client.GetSysDictionaryByCategoryByUpdateDateAsync("", dt);
            
            
        }

        protected void LoadDictss()
        {
            ServiceClient FlowDesigner = new ServiceClient();
            FlowDesigner.GetModelNameInfosComboxCompleted += (o, e) =>
            {
                List<FLOW_MODELDEFINE_T> dicts = new List<FLOW_MODELDEFINE_T>();
                dicts = e.Result == null ? null : e.Result.ToList();
                if (Application.Current.Resources["FLOW_MODELDEFINE_T"] == null)
                {
                    Application.Current.Resources.Add("FLOW_MODELDEFINE_T", dicts);
                }
            };
            //TODO: 获取模块
            FlowDesigner.GetModelNameInfosComboxAsync();
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
            Application.Current.Resources.Clear();
        }
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.Message);
            return;
            // 如果应用程序是在调试器外运行的，则使用浏览器的
            // 异常机制报告该异常。在 IE 上，将在状态栏中用一个 
            // 黄色警报图标来显示该异常，而 Firefox 则会显示一个脚本错误。
            //if (!System.Diagnostics.Debugger.IsAttached)
            //{
            //    // 注意: 这使应用程序可以在已引发异常但尚未处理该异常的情况下
            //    // 继续运行。 
            //    // 对于生产应用程序，此错误处理应替换为向网站报告错误
            //    // 并停止应用程序。
            //    e.Handled = true;
            //    MessageBox.Show(e.ExceptionObject.Message);
            //    //Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            //}
        }
        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
                //System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");

                //ChildWindow errorWin = new ErrorWindow("Unhandled Error in Silverlight Application", errorMsg);
                //errorWin.Show();
                Utility.ShowCustomMessage(MessageTypes.Error, Utility.GetResourceStr("ERROR"), errorMsg);
            }
            catch (Exception)
            {
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox CBAll = sender as CheckBox;
            var grid = Utility.FindParentControl<DataGrid>(CBAll);
            if (grid != null)
            {
                GridHelper.HandleAllCheckBoxClick(grid, "myChkBox", CBAll.IsChecked.GetValueOrDefault());
            }
        }

        // internal static void Navigation(MainPage mainPage)
        // {
        //   throw new NotImplementedException();
        //}
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
    ///// <summary>
    ///// 系统主入口
    ///// </summary>
    //public class EntryPointPage : UserControl
    //{
    //    public UIElement Content
    //    {
    //        get { return base.Content; }
    //        set { base.Content = value; }
    //    }
    //}
}
