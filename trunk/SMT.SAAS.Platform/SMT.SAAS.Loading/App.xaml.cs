using System;
using System.Windows;
using System.Windows.Media;
using SMT.SAAS.Main.CurrentContext;
using System.IO.IsolatedStorage;
using System.Collections;
using System.Windows.Browser;

namespace SMT.SAAS.Platform
{
    public partial class App : System.Windows.Application
    {
        public static IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
        /// <summary>
        /// Applicathon初始化
        /// </summary>
        public App()
        {
            //StyleManager.ApplicationTheme = new SummerTheme();
  
            //激活Silverlight
            System.Windows.Browser.HtmlPage.Plugin.Focus();
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;
            InitializeComponent();
        }
       
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                //读取系统初始参数，此参数为服务地址   
                if (string.IsNullOrEmpty(SMT.SAAS.Main.CurrentContext.Common.HostAddress))
                {
                    string hostAddress = "";
                    hostAddress = hostAddress + e.InitParams["p1"];
                    hostAddress = hostAddress + e.InitParams["p2"].Replace('.', '/');


                    //本地调试
                    //hostAddress = @"http://172.16.1.93:8080";
                    //hostAddress = hostAddress + @"/new/Services";///new.Services    
                    SMT.SAAS.Main.CurrentContext.Common.HostIP = e.InitParams["p1"];
                    SMT.SAAS.Main.CurrentContext.Common.HostAddress = hostAddress;
                    Application.Current.Resources.Add("PlatformWShost", hostAddress);
                }

                if (HtmlPage.Document.QueryString.Count > 0)
                {
                    IDictionary paras = HtmlPage.Document.QueryString as IDictionary;
                    string username = paras["loginaccount"].ToString();
                    string userpwd = paras["password"].ToString();
                    Application.Current.Resources.Add("username", username);
                    Application.Current.Resources.Add("userpwd", userpwd);
                    HtmlPage.Window.Invoke("SetPortalUrl", "Invoke");
                }
                //创建外壳容器Host。
                Host host = new Host();
                SMT.SAAS.Platform.Xamls.LoginPart.Login login = new Xamls.LoginPart.Login();                
                host.SetRoot(login);
                this.RootVisual = host;
                AppContext.AppHost = host;
                //存储已经创建过的容器
                //Common.AppContext.Host = host;
                //this.RootVisual = new SLtext();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
     
        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            string errorMsg = e.ExceptionObject.Message + " " + e.ExceptionObject.StackTrace+e.ExceptionObject.ToString();
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(errorMsg);
            SMT.SAAS.Main.CurrentContext.AppContext.ShowSystemMessageText();
            // If the app is running outside of the debugger then report the exception using
            // the browser's exception mechanism. On IE this will display it a yellow alert 
            // icon in the status bar and Firefox will display a script error.
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown
                // but not handled. 
                // For production applications this error handling should be replaced with something that will 
                // report the error to the website and stop the application.
                e.Handled = true;
                //Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
            }
            else
            {
               
                return;
            }
        }

        private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("抛出新的错误(\" Silverlight 应用程序中存在的未处理错误 " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 使用视图树查找子元素指定类型的父容器
        /// </summary>
        /// <typeparam name="T">父容器类型</typeparam>
        /// <param name="item">要检索的对象</param>
        /// <returns>查找后的结果</returns>
        public static T FindParentControl<T>(DependencyObject item) where T : class
        {
            if (item != null)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(item);
                T parentGrid = parent as T;
                return (parentGrid != null) ? parentGrid : FindParentControl<T>(parent);
            }
            return null;
        }

        //private void CheckBox_Click(object sender, RoutedEventArgs e)
        //{
        //    CheckBox CBAll = sender as CheckBox;
        //    var grid = FindParentControl<DataGrid>(CBAll);
        //    if (grid != null)
        //    {
        //        HandleAllCheckBoxClick(grid, "checkbox", CBAll.IsChecked.GetValueOrDefault());
        //    }
        //}

        /// <summary>
        /// 遍历GRID的首列，判断其是否为CheckBox并设置其选中状态。
        /// </summary>
        //public static void HandleAllCheckBoxClick(DataGrid grid, string cbxName, bool isSelectedAll)
        //{
        //    if (grid.ItemsSource != null)
        //    {
        //        if (isSelectedAll)//全选
        //        {
        //            foreach (object obj in grid.ItemsSource)
        //            {
        //                if (grid.Columns[0].GetCellContent(obj) != null)
        //                {
        //                    CheckBox cb1 = grid.Columns[0].GetCellContent(obj).FindName(cbxName) as CheckBox; //cb为
        //                    if (cb1 != null)
        //                    {
        //                        cb1.IsChecked = true;
        //                    }
        //                }
        //                grid.SelectedItems.Add(obj);

        //            }
        //        }
        //        else//取消
        //        {
        //            foreach (object obj in grid.ItemsSource)
        //            {
        //                if (grid.Columns[0].GetCellContent(obj) != null)
        //                {
        //                    CheckBox cb2 = grid.Columns[0].GetCellContent(obj).FindName(cbxName) as CheckBox;
        //                    if (cb2 != null)
        //                    {
        //                        cb2.IsChecked = false;
        //                    }
        //                }
        //                grid.SelectedItems.Remove(obj);
        //            }
        //        }
        //    }
        //}
    }
}
