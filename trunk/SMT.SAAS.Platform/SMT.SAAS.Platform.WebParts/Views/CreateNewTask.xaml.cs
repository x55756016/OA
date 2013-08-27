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
using SMT.SAAS.Platform.Client.PlatformWS;
using System.Reflection;
using System.Globalization;
using SMT.SAAS.Controls.Toolkit.Windows;

namespace SMT.SAAS.Platform.WebParts.Views
{
    public partial class CreateNewTask : UserControl
    {
        SMT.SAAS.Platform.Client.PlatformWS.PlatformServicesClient _services;
        private ModuleInfo _currentModule ;
        AsyncTools ayTools = new AsyncTools();
        private SMTLoading loadbar = new SMTLoading();
        public CreateNewTask()
        {
            InitializeComponent();
            PARENT.Children.Add(loadbar);
            this.Loaded += new RoutedEventHandler(CreateNewTask_Loaded);
        }

        void CreateNewTask_Loaded(object sender, RoutedEventArgs e)
        {            
            ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
            InitDate();
        }

        private void InitDate()
        {
            _services = new PlatformServicesClient();
            _services.GetTaskConfigInfoByUserCompleted += new EventHandler<Client.PlatformWS.GetTaskConfigInfoByUserCompletedEventArgs>(_services_GetTaskConfigInfoByUserCompleted);
            //_services.Endpoint.Address = new System.ServiceModel.EndpointAddress("http://localhost:15739/PlatformServices.svc");
            _services.GetTaskConfigInfoByUserAsync(Common.CurrentLoginUserInfo.SysUserID);
        }

        void _services_GetTaskConfigInfoByUserCompleted(object sender, Client.PlatformWS.GetTaskConfigInfoByUserCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    var catalog = e.Result.GroupBy(x => new { x.SystemType });
                    List<NewTaskInfo> itemsource = new List<NewTaskInfo>();
                    foreach (var systemMenu in catalog)
                    {

                        if (systemMenu.Key.SystemType != null)
                        {
                            ModuleInfo parentModule = e.Result.FirstOrDefault(item => item.ModuleCode == systemMenu.Key.SystemType);
                            if (parentModule != null)
                            {
                                NewTaskInfo menu = new NewTaskInfo();
                                menu.ModuleName = parentModule.ModuleName;

                                foreach (var tempMenu in systemMenu.ToList())
                                {
                                    menu.Items.Add(tempMenu);
                                }
                                itemsource.Add(menu);
                            }
                        }
                    }

                    itmcTaskList.ItemsSource = itemsource;
                }
            }
        }

        ModuleInfo moduleInfo = null;
        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            moduleInfo = (sender as HyperlinkButton).DataContext as ModuleInfo;
            if (moduleInfo == null)
            {
                return;
            }
            loadbar.Start();
            ayTools.BeginRun();            
        }

        void ayTools_InitAsyncCompleted(object sender, EventArgs e)
        {
            loadbar.Stop();
            if (moduleInfo != null)
            {
                string moduleName = GetModuleName(moduleInfo.SystemType);
                _currentModule = moduleInfo;
                if (moduleInfo.DependsOn.Count > 0)
                    moduleName = moduleInfo.DependsOn[0];

                CheckeDepends(moduleName);
            }
        }

        EventHandler<ViewModel.LoadModuleEventArgs> LoadTaskHandler = null;

        private void CheckeDepends(string moduleName)
        {
            var module = ViewModel.Context.Managed.Catalog.FirstOrDefault(item => item.ModuleName == moduleName);
            if (module != null)
            {
                ViewModel.Context.Managed.OnSystemLoadModuleCompleted += LoadTaskHandler = (o, e) =>
                {
                    ViewModel.Context.Managed.OnSystemLoadModuleCompleted -= LoadTaskHandler;
                    if (e.Error == null)
                    {
                        OpenTask(_currentModule);
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }
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
                    foreach (var item in moduleInfo.InitParams)
                    {
                        PropertyInfo property = instance.GetType().GetProperty(item.Key);
                        property.SetValue(instance, item.Value, null);
                    }
                }
                if (moduleInfo != null && instance != null)
                {
                    SMT.SaaS.FrameworkUI.EntityBrowser browser = new SaaS.FrameworkUI.EntityBrowser(instance);

                    browser.Show<string>(DialogMode.Default, Common.ParentLayoutRoot, "", (result) => { }, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("新建任务打开异常,请查看系统日志！");
                Logging.Logger.Current.Log("10000", "Platform", "新建任务", "新建任务打开异常", ex, Logging.Category.Exception, Logging.Priority.High);
            }
        }

        private string GetModuleName(string typeId)
        {
            string moduleName = string.Empty;
            switch (typeId)
            {
                case "14": moduleName = "SMT.TM.UI"; break;
                case "13": moduleName = "SMT.EM.UI"; break;
                case "10": moduleName = "SMT.EDM.UI"; break;
                case "1": moduleName = "SMT.SaaS.OA.UI"; break;
                case "6": moduleName = "SMT.SaaS.OA"; break;
                case "2": moduleName = "SMT.SaaS.LM.UI"; break;
                case "3": moduleName = "SMT.FB.UI"; break;
                case "0": moduleName = "SMT.HRM.UI"; break;
                case "4": moduleName = "SMT.RM.UI"; break;
                case "15": moduleName = "SMT.FBAnalysis.UI"; break;
                case "7": moduleName = "SMT.SaaS.Permission.UI"; break;
                case "8": moduleName = "SMT.FlowDesigner"; break;
                case "9": moduleName = "SMT.SaaS.EG"; break;
                default:
                    break;
            }
            return moduleName;
        }
    }

    public class NewTaskInfo : SMT.SAAS.Platform.Client.PlatformWS.ModuleInfo
    {
        public NewTaskInfo()
        {
            Items = new List<ModuleInfo>();
        }
        public List<SMT.SAAS.Platform.Client.PlatformWS.ModuleInfo> Items
        { get; set; }
    }
}
