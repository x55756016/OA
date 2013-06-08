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

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 主页面，用于承载主窗口相关元素
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Xamls
{
    /// <summary>
    /// 主页面，用于显示、布局相关模块，快捷方式的定义控制等。
    /// </summary>
    public partial class MVCMainPage : UserControl
    {
        #region 私有成员
        //当前请求的模块信息
        private ModuleInfo _currentClickModule;
        //Model层的公共服务
        private CommonServices _services = null;
        //偏移量
        private static Point Offset = new Point(25, 18);
        //控制面板中的快捷方式是否拖拽
        private static bool isDrag = false;
        //控制是否处于删除面板中拖拽
        private static bool isDelete = false;
        //注销的按钮
        private static string[] titlename = { "确认", "取消" };
        //注销选择事件
        private event EventHandler<OnSelectionBoxClosedArgs> OnSelectionBoxClosed;

        //主菜单
        Xamls.MainPagePart.CustomMenusSet _mainMenu;
        SplashScreenViewModel vm;

        //延迟打开
        private DispatcherTimer _waitPopupTimer;

        //强制控制导航 此功能需要完善之，此实现时间关系只能简单处理
        private bool _fromMenu = false;

        SMT.SAAS.Platform.WebParts.AsyncTools ayTools = new SMT.SAAS.Platform.WebParts.AsyncTools();
        #endregion

        DateTime dtstart = DateTime.Now;
        DateTime dtend = DateTime.Now;

        private bool bMVCOpen = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public MVCMainPage()
        {
            dtstart = DateTime.Now;

            vm = new SplashScreenViewModel();
            //vm.InitCompleted += new EventHandler(vm_InitCompleted);
            vm.Run(new List<ModuleInfo>());         //伪初始化容器，加速MainPage加载,仅实际加载平台样式文件

            InitializeComponent();

            HtmlPage.RegisterScriptableObject("MvcToSl", this);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _services = new CommonServices();
            _services.OnGetMenuPermissionCompleted += new EventHandler(_services_OnGetMenuPermissionCompleted);
            ViewModel.Context.Managed.OnLoadModuleCompleted += new EventHandler<ViewModel.LoadModuleEventArgs>(Managed_OnLoadModuleCompleted);
            vm.InitCompleted += new EventHandler(vm_InitCompleted);

            ayTools.BeginRun();

            if (System.Windows.Application.Current.Resources["CurrentSysUserID"] != null && System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
                bMVCOpen = true;
                loading.Start();
            }

            ViewModel.Context.MainPanel = WorkHost;
            ViewModel.Context.MainPanel.DefaultContent = WebPartHost;
            WorkHost.Back += new EventHandler(WorkHost_Back);
            //IsPopupHelper();

            

            LoadPublicDic();

            
            InitSystemParams();

            dtend = DateTime.Now;
            string strmsg = "初始化MainPage耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
            dtstart = DateTime.Now;
            if (!SMT.SAAS.Main.CurrentContext.AppContext.IsLoadingCompleted)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.IsLoadingCompleted = true;
            }
        }



        /// <summary>
        /// 默认加载公共字典，后续将由相关控件自身处理，
        /// </summary>
        private void LoadPublicDic()
        {
            SAAS.ClientUtility.DictionaryManager dicManager = new ClientUtility.DictionaryManager();
            List<string> dicCategorys = new List<string>();
            dicCategorys.Add("CHECKSTATE");
            dicCategorys.Add("MANUALTYPE");

            dicManager.LoadDictionary(dicCategorys);
        }

        /// <summary>
        /// 初始化系统所需参数.用于为窗口系统提供支持
        /// </summary>
        private void InitSystemParams()
        {
            System.Windows.Controls.Window.Parent = this.windowParent;
            System.Windows.Controls.Window.TaskBar = this.taskbar;
            System.Windows.Controls.Window.Wrapper = this;
            System.Windows.Controls.Window.IsShowtitle = true;
        }

        public void NavigationWorkPanel(string tag)
        {
            try
            {
                WorkHost.PanelContent = null;
                if (!bMVCOpen)
                {
                    WorkHost.Visibility = Visibility.Visible;
                }
                WebPartHost.Visibility = Visibility.Collapsed;
                WebPartHost.Stop();

                UserControl workitem = null;
                string titel = "";
                switch (tag)
                {
                    case "NewsManager": workitem = new WebParts.Views.NewsManager(); titel = "新闻管理"; break;
                    case "SystemLog": workitem = new SMT.SAAS.Platform.Xamls.SystemLogger(); titel = " 系统日志"; break;
                    case "CustomMenusSet":
                        {
                            titel = "菜单列表";
                            if (_mainMenu == null)
                            {
                                _mainMenu = new Xamls.MainPagePart.CustomMenusSet();
                                _mainMenu.ShortCutClick += new EventHandler<OnShortCutClickEventArgs>(menu_ShortCutClick);
                                workitem = _mainMenu;
                            }
                            else
                            {
                                workitem = _mainMenu;
                            }
                            break;
                        }
                    default: break;
                }

                WorkHost.Navigation(workitem, titel);
                Common.AppContext.IsMenuOpen = _fromMenu;
            }
            catch (Exception ex)
            {
                string strmsg = "点击菜单时发生错误，原因：" + ex.ToString();
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
            }
        }

        #region 快捷键控制
        
        private string strCurModuleID = string.Empty;
        private bool bIsModuleLoaded = false;

        /// <summary>
        /// 点击菜单图标事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shortCutManager_OnShortCutClick(object sender, OnShortCutClickArgs e)
        {
            _fromMenu = false;
            strCurModuleID = e.ModuleID;

            if (e.ModuleID == "NewsManager" || e.ModuleID == "SystemLog" || e.ModuleID == "CustomMenusSet")
            {
                NavigationWorkPanel(e.ModuleID);
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

        void vm_InitCompleted(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(strCurModuleID))
            {
                return;
            }

            ShowModule();
        }

        /// <summary>
        /// 打开菜单
        /// </summary>
        private void ShowModule()
        {
            ModuleInfo moduleinfo = ViewModel.Context.Managed.Catalog.FirstOrDefault(m => m.ModuleID == strCurModuleID);
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
                        MessageBox.Show("模块链接异常：" + moduleinfo.ModuleType);
                    }
                }
                else
                {
                    CheckPermission(moduleinfo);
                }
            }
        }
        #endregion

        #region 模块加载、快捷方式、菜单
        private void menu_ShortCutClick(object sender, OnShortCutClickEventArgs e)
        {
            //礼品特殊处理
            ModuleInfo info = e.Result.Content;
            if (info.ModuleCode == "GiftApplyMaster" || info.ModuleCode == "GiftPlan" || info.ModuleCode == "SumGiftPlan")
            {
                string strUrl = string.Empty;
                try
                {
                    HtmlWindow wd = HtmlPage.Window;
                    strUrl = info.ModuleType.Substring(info.ModuleType.IndexOf("[mvc]")).Replace("[mvc]", "");
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
                    options.Status = false;
                    options.Resizeable = true;
                    options.Left = 280;
                    options.Top = 100;
                    options.Width = 800;
                    options.Height = 600;
                    //HtmlPage.PopupWindow(uri, info.ModuleCode, options);
                    string strWindow = System.DateTime.Now.ToString("yyMMddHHmsssfff");
                    wd.Navigate(uri, strWindow, "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");
                }
                catch
                {
                    MessageBox.Show("模块链接异常：" + info.ModuleType);
                }
            }
            else
            {
                _fromMenu = true;
                if (_mainMenu != null)
                {
                    _mainMenu.Start();
                }

                MVCMainPage.isDrag = false;


                //1. 检测菜单权限
                CheckPermission(e.Result.Content);
            }
        }

        private void CheckPermission(ModuleInfo module)
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

        private void _services_OnGetMenuPermissionCompleted(object sender, EventArgs e)
        {
            GetPermissionInfoUIByLocal();
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
                    _mainMenu.Stop();
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
                MessageWindow.Show("提示", message, MessageIcon.Error, MessageWindowType.Default);
            }
        }

        private void Managed_OnLoadModuleCompleted(object sender, ViewModel.LoadModuleEventArgs e)
        {
            if (_mainMenu != null)
            {
                _mainMenu.Stop();
            }
            if (e.ModuleInstance != null)
            {
                FrameworkElement content = e.ModuleInstance as FrameworkElement;
                if (content != null)
                {
                    if (content.Parent == null)
                    {
                        WorkHost.Visibility = Visibility.Visible;
                        WebPartHost.Visibility = Visibility.Collapsed;
                        WebPartHost.Stop();

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
        #endregion

        private void WorkHost_Back(object sender, EventArgs e)
        {
            //处理关闭动作，判断导航模式
            if (Common.AppContext.IsMenuOpen)
            {
                _fromMenu = false;
                NavigationWorkPanel("CustomMenusSet");
            }
            else
            {
                WebPartHost.Star();
                WebPartHost.Visibility = Visibility.Visible;

                WorkHost.Visibility = Visibility.Collapsed;
                var content = WorkHost.PanelContent;
            }
        }        

        #region mvc平台专属调用

        void ayTools_InitAsyncCompleted(object sender, EventArgs e)
        {
            loading.Stop();
            string strModuleid = string.Empty, strOptType = string.Empty, strMessageid = string.Empty, strConfig = string.Empty;
            List<string> strMvcSource = new List<string>();
            if (System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                strMvcSource = System.Windows.Application.Current.Resources["MvcOpenRecordSource"] as List<string>;
            }

            bool isFirst = false;
            if (System.Windows.Application.Current.Resources["isFirstOpen"] != null)
            {
                isFirst = (bool)System.Windows.Application.Current.Resources["isFirstOpen"];
            }
            if (isFirst)
            {
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
                return;
            }


            if (strMvcSource == null)
            {
                return;
            }

            if (strMvcSource.Count() != 4)
            {
                return;
            }

            strModuleid = strMvcSource[0];
            strOptType = strMvcSource[1];
            strMessageid = strMvcSource[2];
            strConfig = strMvcSource[3];

            OpenModuleWithMVC(strModuleid, strOptType, strMessageid, strConfig);
        }

        /// <summary>
        /// 打开指定的菜单，待办任务，新闻或我的单据记录
        /// </summary>
        /// <param name="strModuleid"></param>
        /// <param name="strOptType"></param>
        /// <param name="strMessageid"></param>
        /// <param name="strConfig"></param>
        [ScriptableMember]
        public void OpenModuleWithMVC(string strModuleid, string strOptType, string strMessageid, string strConfig)
        {
            AppContext.SystemMessage("strModuleid:" + strModuleid
                + "strOptType:" + strOptType
                   + "strMessageid:" + strMessageid
                      + "strConfig:" + strConfig);
            AppContext.ShowSystemMessageText();
            if (string.IsNullOrWhiteSpace(strOptType))
            {
                return;
            }

            switch (strOptType.ToUpper())
            {
                case "MODULE":
                    LoadModule(strModuleid);
                    break;
                case "TASK":
                    LoadTask(strMessageid, strConfig);
                    break;
                case "RECORD":
                    LoadMyRecord(strConfig);
                    break;
                case "NEWSMANAGER":
                    LoadNews(strModuleid);
                    break;
            }
        }

        /// <summary>
        /// 打开新闻
        /// </summary>
        /// <param name="strModuleid"></param>
        private void LoadNews(string strModuleid)
        {
            if (string.IsNullOrWhiteSpace(strModuleid))
            {
                return;
            }

            NavigationWorkPanel(strModuleid);
        }

        /// <summary>
        /// 打开待办任务
        /// </summary>
        /// <param name="strMessageid"></param>
        /// <param name="strConfig"></param>
        private void LoadTask(string strMessageid, string strConfig)
        {
            if (string.IsNullOrWhiteSpace(strMessageid) || string.IsNullOrWhiteSpace(strConfig))
            {
                return;
            }

            SMT.SAAS.Platform.WebParts.Views.MVCPendingTaskManager pendingTaskView = new WebParts.Views.MVCPendingTaskManager(strMessageid, "open");
            pendingTaskView.isMyrecord = false;
            if (ViewModel.Context.MainPanel != null)
            {
                if (ViewModel.Context.MainPanel.DefaultContent != null)
                {
                    IWebpart webpart = ViewModel.Context.MainPanel.DefaultContent as IWebpart;
                    if (webpart != null)
                        webpart.Stop();

                }
                ViewModel.Context.MainPanel.Navigation(pendingTaskView, "待办任务");

            }
        }

        /// <summary>
        /// 打开我的单据
        /// </summary>
        /// <param name="strConfig"></param>
        private void LoadMyRecord(string strConfig)
        {
            if (string.IsNullOrWhiteSpace(strConfig))
            {
                return;
            }

            //SMT.SAAS.Platform.WebParts.Views.MyRecord myRecordView = new WebParts.Views.MyRecord();
            //myRecordView.ShowMyRecord(strConfig);

            SMT.SAAS.Platform.WebParts.Views.MVCPendingTaskManager pendingTaskView = new WebParts.Views.MVCPendingTaskManager("", "open");
            pendingTaskView.isMyrecord = true;
            pendingTaskView.applicationUrl = strConfig;
            if (ViewModel.Context.MainPanel != null)
            {
                if (ViewModel.Context.MainPanel.DefaultContent != null)
                {
                    IWebpart webpart = ViewModel.Context.MainPanel.DefaultContent as IWebpart;
                    if (webpart != null)
                        webpart.Stop();

                }
                ViewModel.Context.MainPanel.Navigation(pendingTaskView, "我的单据");

            }
        }

        /// <summary>
        /// 打开菜单
        /// </summary>
        /// <param name="strModuleid"></param>
        private void LoadModule(string strModuleid)
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
        #endregion

    }
}
