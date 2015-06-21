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

namespace SMT.SAAS.Platform.Xamls
{
    /// <summary>
    /// 主页面，用于显示、布局相关模块，快捷方式的定义控制等。
    /// </summary>
    public partial class MVCMainPage 
    {
        /// <summary>
        /// 打开菜单
        /// </summary>
        /// <param name="strModuleid"></param>
        private void LoadModule(string strModuleid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strModuleid))
                {
                    return;
                }

                _fromMenu = false;
                strCurModuleID = strModuleid;

                if (strModuleid == "NewsManager" || strModuleid == "SystemLog" || strModuleid == "CustomMenusSet")
                {
                    NavigationWorkPanel(strModuleid);
                }
                else
                {
                    if (ViewModel.Context.Managed != null)
                    {
                        if (ViewModel.Context.Managed.Catalog != null)
                        {
                            if (ViewModel.Context.Managed.Catalog.Count > 0)
                            {
                                bIsModuleLoaded = true;
                            }
                        }
                    }

                    if (!bIsModuleLoaded)
                    {
                        vm.GetModules();
                        return;
                    }

                    ShowModule();
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("打开模块'{0}'失败,请联系管理员！", ex.ToString());
                AppContext.SystemMessage(message);
                AppContext.ShowSystemMessageText();
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
            }
        }


        #region 显示模块
        /// <summary>
        /// 打开菜单
        /// </summary>
        private void ShowModule()
        {
            ModuleInfo moduleinfo;
            if (this.OpenType == "CREATENEWFORM")
            {
                moduleinfo = ViewModel.Context.Managed.Catalog.FirstOrDefault(m => m.ModuleName == strCurModuleID);
            }
            else
            {
                moduleinfo = ViewModel.Context.Managed.Catalog.FirstOrDefault(m => m.ModuleID == strCurModuleID);
            }
            if (moduleinfo != null)
            {
                if (moduleinfo.ModuleCode == "GiftApplyMaster" || moduleinfo.ModuleCode == "GiftPlan" || moduleinfo.ModuleCode == "SumGiftPlan")
                {
                    string strUrl = string.Empty;
                    try
                    {
                        HtmlWindow wd = HtmlPage.Window;
                        strUrl = moduleinfo.ModuleType.Substring(moduleinfo.ModuleType.IndexOf("[mvc]")).Replace("[mvc]", "");
                        strUrl = strUrl.Split(',')[0].Replace('.', '/');
                        if (strUrl.IndexOf('?') > -1)
                        {
                            strUrl = strUrl + "&uid=" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        }
                        else
                        {
                            strUrl = strUrl + "?uid=" + SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;
                        }
                        string strHost = SMT.SAAS.Main.CurrentContext.Common.HostAddress.ToString().Split('/')[0];
                        strUrl = "http://" + strHost + "/" + strUrl;
                        Uri uri = new Uri(strUrl);
                        HtmlPopupWindowOptions options = new HtmlPopupWindowOptions();
                        options.Directories = false;
                        options.Location = false;
                        options.Menubar = false;
                        options.Status = false;
                        options.Toolbar = false;
                        options.Left = 280;
                        options.Top = 100;
                        options.Width = 800;
                        options.Height = 600;
                        //HtmlPage.PopupWindow(uri, moduleinfo.ModuleCode, options);
                        //wd.Navigate(uri, "_bank");
                        string strWindow = System.DateTime.Now.ToString("yyMMddHHmsssfff");
                        wd.Navigate(uri, strWindow, "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");
                    }
                    catch
                    {
                        string msg = "模块链接异常：" + moduleinfo.ModuleType;
                        HtmlPage.Window.Invoke("loadCompletedSL", new string[] { "false", msg });
                        MessageBox.Show("模块链接异常：" + moduleinfo.ModuleType);
                        hideLoadingBar();
                    }
                }
                else
                {
                    CheckPermission(moduleinfo);
                }
            }
            else
            {
                string msg = "moduleinfo 未加载：moduleid：" + strCurModuleID;
                AppContext.SystemMessage(msg);
                //AppContext.ShowSystemMessageText();
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", new string[] { "false", msg });
            }
        }

        private void CheckPermission(t_sys_entitymen module)
        {
            _currentClickModule = module;

            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo == null)
            {
                return;
            }

            GetPermissionInfoUI();
        }

        private void GetPermissionInfoUI()
        {
            if (V_UserPermUILocalVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID) == false)
            {
                _services.GetUserMenuPermission(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, _currentClickModule.ModuleID);
            }
            else
            {
                //权限检查发现有变更时，权限需要重新从服务器获取
                if (SMT.SAAS.Main.CurrentContext.AppContext.IsPermUpdate)
                {
                    _services.GetUserMenuPermission(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, _currentClickModule.ModuleID);
                    return;
                }

                if (V_UserPermUILocalVM.IsExists(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID, _currentClickModule.ModuleID) == false)
                {
                    _services.GetUserMenuPermission(SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.SysUserID, _currentClickModule.ModuleID);
                    return;
                }

                GetPermissionInfoUIByLocal();
            }
        }

        private void GetPermissionInfoUIByLocal()
        {
            string strEmployeeID = SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.EmployeeID;

            List<SMT.SaaS.LocalData.V_UserPermissionUI> userPermissionUIs = new List<SMT.SaaS.LocalData.V_UserPermissionUI>();

            List<V_UserPermUILocal> userPermUILocals = V_UserPermUILocalVM.GetAllV_UserPermUILocal(strEmployeeID);
            List<V_CustomerPermission> customerPerms = V_CustomerPermissionVM.GetAllV_CustomerPermission(strEmployeeID);
            List<V_PermissionValue> permissionValues = V_PermissionValueVM.GetAllV_PermissionValue(strEmployeeID);
            List<V_OrgObject> v_OrgObjects = V_OrgObjectVM.GetAllV_OrgObject(strEmployeeID);

            if (userPermUILocals == null)
            {
                LogErrMsg("打开模块获取权限错误，未获取到任何权限。");
                return;
            }

            foreach (var item in userPermUILocals)
            {
                if (item.EntityMenuID != null)
                {
                    if (ViewModel.Context.CacheMenuPermissionList == null)
                    {
                        ViewModel.Context.CacheMenuPermissionList = new List<string>();
                    }

                    SMT.SaaS.LocalData.V_UserPermissionUI userPermissionUI = item.CloneObject<SMT.SaaS.LocalData.V_UserPermissionUI>(new SMT.SaaS.LocalData.V_UserPermissionUI());

                    V_CustomerPermission v_cusPerm = null;
                    foreach (var p in customerPerms)
                    {
                        if (p.UserModuleID != item.UserModuleID)
                        {
                            continue;
                        }

                        v_cusPerm = p;
                        break;
                    }

                    if (v_cusPerm == null)
                    {
                        userPermissionUIs.Add(userPermissionUI);
                        continue;
                    }

                    userPermissionUI.CustomerPermission = v_cusPerm.CloneObject<SMT.SaaS.LocalData.CustomerPermission>(new SMT.SaaS.LocalData.CustomerPermission());
                    List<SMT.SaaS.LocalData.PermissionValue> permValues = new List<SaaS.LocalData.PermissionValue>();
                    foreach (var d in permissionValues)
                    {
                        if (v_cusPerm == null)
                        {
                            break;
                        }

                        if (d.UserModuleID != v_cusPerm.UserModuleID)
                        {
                            continue;
                        }

                        SMT.SaaS.LocalData.PermissionValue permValue = d.CloneObject<SMT.SaaS.LocalData.PermissionValue>(new SaaS.LocalData.PermissionValue());
                        List<SMT.SaaS.LocalData.OrgObject> orgObjects = new List<SaaS.LocalData.OrgObject>();
                        foreach (var o in v_OrgObjects)
                        {
                            if (v_cusPerm == null)
                            {
                                break;
                            }

                            if (o.UserModuleID != d.UserModuleID)
                            {
                                continue;
                            }

                            SMT.SaaS.LocalData.OrgObject orgObject = o.CloneObject<SMT.SaaS.LocalData.OrgObject>(new SaaS.LocalData.OrgObject());
                            orgObjects.Add(orgObject);
                        }
                        permValue.OrgObjects.AddRange(orgObjects);
                        permValues.Add(permValue);
                    }

                    userPermissionUI.CustomerPermission.PermissionValue.AddRange(permValues);
                }
            }

            foreach (var u in userPermissionUIs)
            {
                if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI == null)
                {
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI = new List<SaaS.LocalData.V_UserPermissionUI>();
                }

                if (!SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI.Contains(u))
                {
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI.Add(u);
                }
            }

            if (ViewModel.Context.CacheMenuPermissionList.Contains(_currentClickModule.ModuleID) == false)
            {
                ViewModel.Context.CacheMenuPermissionList.Add(_currentClickModule.ModuleID);
            }

            GetModuleContent(_currentClickModule.ModuleName, _currentClickModule.Description);
        }

        /// <summary>
        /// 加载模块
        /// </summary>
        private void GetModuleContent(string ModuleName, string description)
        {
            try
            {
                ViewModel.Context.Managed.LoadModule(ModuleName);
            }
            catch (Exception ex)
            {
                AppContext.SystemMessage(string.Format("打开模块'{0}'产生异常！", description) + ex.ToString());
                AppContext.ShowSystemMessageText();
                if (_mainMenu != null)
                {
                    _mainMenu.hideLoading();
                }
                Logging.Logger.Current.Log(
                    "10000",
                    "Platform",
                    "请求模块.",
                    String.Format("打开模块'{0}'产生异常！", description),
                    ex,
                    Logging.Category.Exception,
                    Logging.Priority.High);

                string message = string.Format("打开模块'{0}'失败,请联系管理员！", description);
                HtmlPage.Window.Invoke("loadCompletedSL", new string[] { "false", message });
                MessageWindow.Show("提示", message, MessageIcon.Error, MessageWindowType.Default);
            }
            finally
            {
                hideLoadingBar();
            }
        }

        private void Managed_OnLoadModuleCompleted(object sender, ViewModel.LoadModuleEventArgs e)
        {
            try
            {
                if (_mainMenu != null)
                {
                    _mainMenu.hideLoading();
                }
                if (e.ModuleInstance != null)
                {
                    FrameworkElement content = e.ModuleInstance as FrameworkElement;
                    if (content != null)
                    {
                        if (content.Parent == null)
                        {
                            WorkHost.Visibility = Visibility.Visible;
                            //WebPartHost.Visibility = Visibility.Collapsed;
                            //WebPartHost.Stop();
                            WorkHost.Navigation(content, e.ModuleInfo.Description);
                            Common.AppContext.IsMenuOpen = _fromMenu;
                        }
                    }


                }
                else
                {
                    string message = string.Format("打开模块'{0}'失败,请联系管理员！", e.ModuleInfo.Description);
                    AppContext.SystemMessage(message);
                    AppContext.ShowSystemMessageText();
                    MessageWindow.Show("提示", message, MessageIcon.Error, MessageWindowType.Default);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("打开模块'{0}'失败,请联系管理员！", ex.ToString());
                AppContext.SystemMessage(message);
                AppContext.ShowSystemMessageText();
            }
            finally
            {
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
            }
        }
        #endregion
    }
}
