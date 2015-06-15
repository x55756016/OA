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
using System.Reflection;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SAAS.Main.CurrentContext;

namespace SMT.SAAS.Platform.Xamls.MainPagePart
{
    public partial class MVCCraeteNewForm : UserControl
    {
        public ModuleInfo info;
        public MVCCraeteNewForm()
        {
            InitializeComponent();
        }

        // 当用户导航到此页面时执行。
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
        //}

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            OpenTask(info);
        }
        private void OpenTask(ModuleInfo moduleInfo)
        {
            Type moduleType = null;

            object instance = null;
            try
            {
                moduleType = Type.GetType(moduleInfo.ModuleType);
                instance = Activator.CreateInstance(moduleType);
                if (moduleInfo.InitParams != null && instance != null)
                {
                    //foreach (var item in moduleInfo.InitParams)
                    //{
                    //    PropertyInfo property = instance.GetType().GetProperty(item.Key);
                    //    property.SetValue(instance, item.Value, null);
                    //}
                }
                if (moduleInfo != null && instance != null)
                {
                    WindowsManager.ClearAllWindows();//清除所有打开的窗口
                    //WorkHost.Visibility = Visibility.Visible;
                    SMT.SaaS.FrameworkUI.EntityBrowser browser = new SaaS.FrameworkUI.EntityBrowser(instance);

                    parent.Child = browser;
                    //browser.Show<string>(DialogMode.Default, SMT.SAAS.Main.CurrentContext.Common.ParentLayoutRoot, "", (result) => { }, true);
                    if (browser.ParentWindow != null)
                    {
                        WindowsManager.MaxWindow(browser.ParentWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrMsg("新建任务打开异常,请查看系统日志:"+ex.ToString());              
            }
        }

        public void LogErrMsg(string msg)
        {
            AppContext.SystemMessage(msg);
            AppContext.ShowSystemMessageText();
        }
    }
}
