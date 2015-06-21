using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.SAAS.Platform.Model.Services;
using SMT.SAAS.Platform.Xamls.MainPagePart;
using System.IO.IsolatedStorage;
using System.Windows.Threading;
using SMT.SAAS.ClientUtility;
using SMT.SAAS.Platform.ViewModel.SplashScreen;
using SMT.SAAS.Main.CurrentContext;
using SMT.SaaS.LocalData.ViewModel;
using SMT.SaaS.LocalData.Tables;
using System.Reflection;
using SMT.SAAS.Controls.Toolkit.Windows;
using SMT.SAAS.Platform.WebParts;

namespace SMT.SAAS.Platform.Xamls
{
    /// <summary>
    /// 主页面，用于显示、布局相关模块，快捷方式的定义控制等。
    /// </summary>
    public partial class MVCMainPage 
    {
        ModuleInfo moduleInfo = null;
        ModuleInfo _currentCreateNewModule = null;
        AsyncTools ayToolsCtreate = new AsyncTools();
        /// <summary>
        /// 打开菜单
        /// </summary>
        /// <param name="strModuleid"></param>
        private void CreateNewForm(string strModuleid)
        {
            try
            {
                moduleInfo = new ModuleInfo();
                moduleInfo.ModuleName = strModuleid;
                moduleInfo.SystemType = MoudleType.Split(',')[1];
                moduleInfo.ModuleType = this.MoudleType;
                string moduleName = moduleInfo.SystemType;
                _currentCreateNewModule = moduleInfo;
                //LoadModule(moduleName);
                //return;
                if (string.IsNullOrWhiteSpace(strModuleid))
                {
                    LogErrMsg("模块id为空");
                    hideLoadingBar();
                    HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
                    return;
                }

               

                ayToolsCtreate.InitAsyncCompleted+=InitAsyncHandler=(oo,vv)=>
                    {
                        ayToolsCtreate.InitAsyncCompleted -= InitAsyncHandler;
                        if (moduleInfo != null)
                        {
                            _currentCreateNewModule = moduleInfo;
                            if (moduleInfo.DependsOn.Count > 0)
                                moduleName = moduleInfo.DependsOn[0];

                            CheckeDepends(moduleName);
                        }
                    };
                ayToolsCtreate.BeginRun();    
            }
            catch (Exception ex)
            {
                LogErrMsg("CreateNewForm" + ex.ToString()); 
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
            }
                 
        }
        EventHandler InitAsyncHandler = null;
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
                        try
                        {
                            MVCCraeteNewForm content = new MVCCraeteNewForm();
                            content.info = _currentCreateNewModule;
                            if (content != null)
                            {
                                if (content.Parent == null)
                                {
                                    WorkHost.Visibility = Visibility.Visible;
                                    WorkHost.Navigation(content, e.ModuleInfo.Description);
                                    Common.AppContext.IsMenuOpen = _fromMenu;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        finally
                        {
                            hideLoadingBar();
                            HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
                        }
                    }
                };

                ViewModel.Context.Managed.LoadModule(moduleName);
            }

        }    

        public void LogErrMsg(string msg)
        {
            AppContext.SystemMessage(msg);
            AppContext.ShowSystemMessageText();
        }
    }
}
