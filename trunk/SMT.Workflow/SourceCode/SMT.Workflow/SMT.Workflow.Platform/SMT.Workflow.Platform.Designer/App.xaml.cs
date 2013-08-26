﻿using System;
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

using SMT.Workflow.Platform.Designer.Class.Converter;
using SMT.SaaS.FrameworkUI.ChildWidow;

namespace SMT.Workflow.Platform.Designer
{
    public partial class App : Application
    {

        public App()
        {
            this.Startup += this.Application_Startup;
            this.Exit += this.Application_Exit;
            this.UnhandledException += this.Application_UnhandledException;

            InitializeComponent();
        }
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
            THost host = new THost();
            this.RootVisual = host;
            //SMT.Workflow.Platform.Designer.AppContext.Host = host;
            //this.RootVisual = new MainPage();
            //this.RootVisual = new TreeViewTest(); 
            //this.ap
            AppContext.Host = host;
            InitComonConverter();
        }
        private void InitComonConverter()
        {
            Application.Current.Resources.Add("GridHeaderConverter", new SMT.SaaS.Globalization.GridHeaderConverter());
            Application.Current.Resources.Add("ResourceConveter", new SMT.SaaS.Globalization.ResourceConveter());
            //########################## end 加载golobal资源
            //加载OA项目的转换器
            Application.Current.Resources.Add("RentConvert", new RentFlagConverter());
            Application.Current.Resources.Add("DictionaryConverter", new DictionaryConverter());
            Application.Current.Resources.Add("CustomDateConverter", new CustomDateConverter());
            Application.Current.Resources.Add("StateConvert", new CheckStateConverter());

        }
        private void Application_Exit(object sender, EventArgs e)
        {

        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // 如果应用程序是在调试器外运行的，则使用浏览器的
            // 异常机制报告该异常。在 IE 上，将在状态栏中用一个 
            // 黄色警报图标来显示该异常，而 Firefox 则会显示一个脚本错误。

            ComfirmWindow.ConfirmationBox("提示信息", e.ExceptionObject.Message, "确定");

            if (e.ExceptionObject != null)
            {
                MessageBox.Show((e.ExceptionObject).Message);
            }
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

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            }
            catch (Exception)
            {
            }
        }
    }
}
