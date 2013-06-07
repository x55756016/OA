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
    public partial class MainPage : UserControl
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

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainPage()
        {
            dtstart = DateTime.Now;

            vm = new SplashScreenViewModel();
            //vm.InitCompleted += new EventHandler(vm_InitCompleted);
            vm.Run(new List<ModuleInfo>());         //伪初始化容器，加速MainPage加载,仅实际加载平台样式文件

            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _services = new CommonServices();
            _services.OnGetMenuPermissionCompleted += new EventHandler(_services_OnGetMenuPermissionCompleted);
            ViewModel.Context.Managed.OnLoadModuleCompleted += new EventHandler<ViewModel.LoadModuleEventArgs>(Managed_OnLoadModuleCompleted);
            vm.InitCompleted += new EventHandler(vm_InitCompleted);
            ayTools.BeginRun();

            ViewModel.Context.MainPanel = WorkHost;
            ViewModel.Context.MainPanel.DefaultContent = WebPartHost;
            WorkHost.Back += new EventHandler(WorkHost_Back);
            //IsPopupHelper();

            dragShortCut.MouseLeftButtonUp += new MouseButtonEventHandler(dragShortCut_MouseLeftButtonUp);
            dragShortCut.MouseMove += new MouseEventHandler(dragShortCut_MouseMove);
            shortCutPanel.MouseMove += new MouseEventHandler(shortCutPanel_MouseMove);
            shortCutPanel.MouseLeftButtonUp += new MouseButtonEventHandler(shortCutPanel_MouseLeftButtonUp);
            shortCutManager.OnShortCutClick += new EventHandler<OnShortCutClickArgs>(shortCutManager_OnShortCutClick);
            shortCutManager.ShortCutMouseDown += new EventHandler<MouseButtonEventArgs>(shortCutManager_ShortCutMouseDown);
            shortCutManager.ShortCutMouseMove += new EventHandler<MouseEventArgs>(shortCutManager_ShortCutMouseMove);

            startMenu.OnClick += new EventHandler(startMenu_OnClick);
            LoadPublicDic();
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseLeftButtonUp);
            panel.CompleteClick += new EventHandler<Controls.DockPanels.ClickEventArgs>(panel_CompleteClick);
            _waitPopupTimer = new DispatcherTimer();
            _waitPopupTimer.Interval = new TimeSpan(0, 0, 8);
            _waitPopupTimer.Tick += new EventHandler(_waitPopupTimer_Tick);
            _waitPopupTimer.Start();

            RegisterOnBeforeUnload();

            LoadImgNews();
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

        void panel_CompleteClick(object sender, Controls.DockPanels.ClickEventArgs e)
        {
            bool _lock = e.islock;
            if (_lock)
            {
                LeftColumn1.Visibility = Visibility.Collapsed;
            }
            else
            {
                LeftColumn1.Visibility = Visibility.Visible;
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

        void shortCut_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShortCut source = sender as ShortCut;

            NavigationWorkPanel(source.Tag.ToString());
            _fromMenu = false;
        }

        public void NavigationWorkPanel(string tag)
        {
            try
            {
                WorkHost.PanelContent = null;
                WorkHost.Visibility = Visibility.Visible;
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
                                _mainMenu.MenuItemMouseDown += new EventHandler<MouseButtonEventArgs>(menu_MenuItemMouseDown);
                                _mainMenu.ShortCutClick += new EventHandler<OnShortCutClickEventArgs>(menu_ShortCutClick);
                                _mainMenu.MenuItemMouseMove += new EventHandler<MouseEventArgs>(menu_MenuItemMouseMove);
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
        private void dragShortCut_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrag = false;
            dragShortCut.ReleaseMouseCapture();
            dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
            Debug.WriteLine("dragShortCut_MouseLeftButtonUp__false");

            if (isDelete)
            {
                isDelete = false;
                #region 检测删除面板
                var startMenuS = from element in VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), gridBottom)
                                 where element is SMT.SAAS.Platform.Xamls.MainPagePart.Start
                                 select element;

                if (startMenuS.Count() >= 1)
                {
                    SMT.SAAS.Platform.Xamls.MainPagePart.Start start = startMenuS.FirstOrDefault() as SMT.SAAS.Platform.Xamls.MainPagePart.Start;
                    if (start != null)
                    {
                        ViewModel.MainPage.ShortCutViewModel shortcutVM = (dragShortCut.DataContext as ViewModel.MainPage.ShortCutViewModel);

                        shortCutManager.RemoveItem(shortcutVM.ShortCutID);
                    }
                }
                #endregion
            }
            else
            {
                #region 检测添加面板
                var ShortCutManagers = from element in VisualTreeHelper.FindElementsInHostCoordinates(e.GetPosition(null), grdShortCut)
                                       where element is SMT.SAAS.Platform.Xamls.MainPagePart.ShortCutManager
                                       select element;

                if (ShortCutManagers.Count() >= 1)
                {
                    SMT.SAAS.Platform.Xamls.MainPagePart.ShortCutManager customMenus = ShortCutManagers.FirstOrDefault() as SMT.SAAS.Platform.Xamls.MainPagePart.ShortCutManager;
                    if (customMenus != null)
                    {
                        ViewModel.Menu.MenuViewModel menuvm = (dragShortCut.DataContext as ViewModel.Menu.MenuViewModel);

                        ViewModel.MainPage.ShortCutViewModel vm = new ViewModel.MainPage.ShortCutViewModel()
                        {
                            AssemplyName = menuvm.Content.ModuleType == null ? "NULL" : menuvm.Content.ModuleType,
                            FullName = menuvm.Content.ModuleType == null ? "NULL" : menuvm.Content.ModuleType,
                            Titel = menuvm.MenuName,
                            IconPath = menuvm.MenuIconPath,
                            ModuleID = menuvm.MenuID,
                            ShortCutID = menuvm.MenuID,
                            IsSysNeed = "0",
                            ModuleName = menuvm.Content.ModuleName,
                            UserState = "1"
                        };

                        customMenus.AddItem(vm);
                    }
                }
                #endregion
            }

        }

        private void shortCutManager_ShortCutMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                ShortCut item = sender as ShortCut;
                MainPage.isDrag = true;
                Point p = e.GetPosition(shortCutPanel);
                Point p2 = e.GetPosition(item);

                Canvas.SetLeft(dragShortCut, p.X - p2.X);
                Canvas.SetTop(dragShortCut, p.Y - p2.Y);
                dragShortCut.DataContext = item.DataContext;
                dragShortCut.Visibility = System.Windows.Visibility.Visible;
                dragShortCut.Opacity = 0.5;
            }
        }

        private void shortCutManager_ShortCutMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShortCut item = sender as ShortCut;

            MainPage.isDrag = true;
            isDelete = true;
            Point p = e.GetPosition(shortCutPanel);
            Point p2 = e.GetPosition(item);

            Canvas.SetLeft(dragShortCut, p.X - p2.X);
            Canvas.SetTop(dragShortCut, p.Y - p2.Y);

            dragShortCut.DataContext = item.DataContext;

            dragShortCut.Titel = item.Titel;
            dragShortCut.Icon = item.Icon;

            dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
            dragShortCut.Opacity = 0.5;
        }

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

        private void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapsedDragItem();
        }

        private void shortCutPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollapsedDragItem();
        }

        private void CollapsedDragItem()
        {
            isDrag = false;
            isDelete = false;
            Debug.WriteLine("shortCutPanel_MouseLeftButtonUp__false");
            dragShortCut.ReleaseMouseCapture();
            dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void shortCutPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                dragShortCut.CaptureMouse();
                Point p = e.GetPosition(shortCutPanel);
                dragShortCut.Opacity = 0.5;
                dragShortCut.Visibility = System.Windows.Visibility.Visible;
                Canvas.SetLeft(dragShortCut, p.X - Offset.X);
                Canvas.SetTop(dragShortCut, p.Y - Offset.Y);
            }
            else
            {
                dragShortCut.ReleaseMouseCapture();
                dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void dragShortCut_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void menu_MenuItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShortCut item = sender as ShortCut;

            MainPage.isDrag = true;
            Point p = e.GetPosition(shortCutPanel);
            Point p2 = e.GetPosition(item);

            Canvas.SetLeft(dragShortCut, p.X - p2.X);
            Canvas.SetTop(dragShortCut, p.Y - p2.Y);

            dragShortCut.DataContext = item.DataContext;
            dragShortCut.Titel = item.Titel;
            dragShortCut.Icon = item.Icon;
            dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
            dragShortCut.Opacity = 0.5;
        }

        private void menu_MenuItemMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrag)
            {
                ShortCut item = sender as ShortCut;
                MainPage.isDrag = true;
                Point p = e.GetPosition(shortCutPanel);
                Point p2 = e.GetPosition(item);

                Canvas.SetLeft(dragShortCut, p.X - p2.X);
                Canvas.SetTop(dragShortCut, p.Y - p2.Y);
                dragShortCut.DataContext = item.DataContext;
                dragShortCut.Visibility = System.Windows.Visibility.Visible;
                dragShortCut.Opacity = 0.5;
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

                dragShortCut.Visibility = System.Windows.Visibility.Collapsed;
                MainPage.isDrag = false;


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
                AppContext.SystemMessage(string.Format("打开模块'{0}'产生异常！", description)+ex.ToString());
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

        #region 加载图片新闻

        private void LoadImgNews()
        {
            WebParts.Views.RollImageNews w = new WebParts.Views.RollImageNews();
            if (w != null)
            {
                ShowinfoPanel.Children.Add(w);
            }
        }

        #endregion

        private void startMenu_OnClick(object sender, EventArgs e)
        {
            _fromMenu = false;
            NavigationWorkPanel("CustomMenusSet");
        }

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

        #region 注销，使用JS检测用户关闭IE动作

        private void Logout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.OnSelectionBoxClosed += (obj, result) =>
            {
                OnLogOff();
                string strHost = SMT.SAAS.Main.CurrentContext.Common.HostIP;
                string strUrl = "http://" + strHost + "/";

                HtmlWindow wd = HtmlPage.Window;
                Uri uri = new Uri(strUrl);
                wd.Navigate(uri);
            };

            MessageWindow.Show<string>("请选择", "确定注销当前用户？", MessageIcon.Question,
               GetResult, "Default", titlename);

        }

        private void GetResult(string result)
        {
            if (titlename[0] == result)
                OnSelectionBoxClosed(this, new OnSelectionBoxClosedArgs(result));
        }

        public void RegisterOnBeforeUnload()
        {
            //将此页面注册为JS可以调用的对象
            const string scriptableObjectName = "Bridge";
            HtmlPage.RegisterScriptableObject(scriptableObjectName, this);

            //监听Javascript事件
            string pluginName = HtmlPage.Plugin.Id;

            HtmlPage.Window.Eval(string.Format(
                @"window.onunload=function(){{
                var slApp = document.getElementById('{0}');
                var result = slApp.Content.{1}.OnLogOff();
                
                }}", pluginName, scriptableObjectName)
                );

            HtmlPage.Window.Eval(
                @"window.onbeforeunload=function(){{
                if(document.body.clientWidth-event.clientX< 170&&event.clientY< 0||event.altKey) 
                {
                var msg=' 已 注 销 用 户 信 息。\n\n 点  确认 或 关闭  自 动 退 出 系 统！'
                window.event.returnValue = '点击确认退出神州通集团协同办公平台.';
                }
                }}"
            );


        }

        /// <summary>
        /// 用户注销
        /// </summary>
        /// <returns></returns>
        [ScriptableMember]
        public bool OnLogOff()
        {

            System.Windows.Application.Current.Resources.Remove("CurrentUserID");//用户ID
            System.Windows.Application.Current.Resources.Remove("SYS_CompanyInfo");//当前用户公司信息
            System.Windows.Application.Current.Resources.Remove("SYS_DepartmentInfo");//当前用户部门
            System.Windows.Application.Current.Resources.Remove("SYS_PostInfo");//当前用户岗位
            System.Windows.Application.Current.Resources.Remove("SYS_DICTIONARY");//字典
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSCompanyInfoALL");
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSDepartmentInfoALL");
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSPostInfoALL");
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSCompanyInfo");
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSDepartmentInfo");
            System.Windows.Application.Current.Resources.Remove("ORGTREESYSPostInfo");

            ViewModel.Context.Clear();

            if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo != null)
            {
                if (SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI != null)
                {
                    SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo.PermissionInfoUI.Clear();
                }
                SMT.SAAS.Main.CurrentContext.Common.CurrentLoginUserInfo = null;
            }
            DictionaryManager.IsLoad = false;
            Thread.Sleep(500);

            return Common.AppContext.LogOff;
        }

        #endregion

        #region 弹出新闻

        void _waitPopupTimer_Tick(object sender, EventArgs e)
        {
            _waitPopupTimer.Tick -= _waitPopupTimer_Tick;
            _waitPopupTimer.Stop();

            SMT.SAAS.Platform.WebParts.News.PopupNews popup = new WebParts.News.PopupNews();
            popup.PoputNews();

            IsPopupHelper();
        }
        private void IsPopupHelper()
        {
            //if(!AppContext.isloaded) return;
            /// <summary>
            /// 独立存储
            /// </summary>
            IsolatedStorageSettings AppSettings = IsolatedStorageSettings.ApplicationSettings;
            /// <summary>
            /// 存储最后一次访问的用户名KEY
            /// </summary>
            const string POPUPKEY = "POPUPHELPER";
            string GUID = "A5F1C933-3F79-4E1B-BFAB-A76A257AB422";
            bool ispopup = false;

            if (AppSettings.Contains(POPUPKEY))
                ispopup = (bool)AppSettings[POPUPKEY];

            if (!ispopup)
            {
                Helper helper = new Helper() { Height = 300, Width = 600 };
                System.Windows.Controls.Window.Show("温馨提示", "", GUID, false, true, helper, null);
                AppSettings.Add(POPUPKEY, true);
            }
        }
        #endregion

        #region 平台状态栏：包括即时通讯，手机版，版本更新文件地址
        private static string versionID = Guid.NewGuid().ToString();
        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string strHost = SMT.SAAS.Main.CurrentContext.Common.HostAddress.ToString().Split('/')[0];
            string strUrl = "http://" + strHost + "/" + "imprint.html";
            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri(strUrl);
            string url = string.Empty;
            if (System.Windows.Application.Current.Resources.Contains("UpdateVersionUrl"))
            {
                url = System.Windows.Application.Current.Resources["UpdateVersionUrl"].ToString();
                uri = new Uri(url);
            }

            wd.Navigate(uri, "_bank", "directories=no,fullscreen=no,menubar=no,resizable=yes,scrollbars=yes,status=no,titlebar=no,toolbar=no");
            //System.Windows.Controls.Window.Show("版本信息", "", versionID, true, true, new MainPagePart.VersionInfo(), null);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri("http://portal.smt-online.net/old/");

            string url = string.Empty;
            if (System.Windows.Application.Current.Resources.Contains("IMDownloadUrl"))
            {
                url = System.Windows.Application.Current.Resources["IMDownloadUrl"].ToString();
                uri = new Uri(url);
            }

            wd.Navigate(uri, "_bank");
        }
        private void hyPhoneClient_Click(object sender, RoutedEventArgs e)
        {
            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri("http://3g.smt-online.net/portal/");

            string url = string.Empty;
            if (System.Windows.Application.Current.Resources.Contains("PhoneDownloadUrl"))
            {
                url = System.Windows.Application.Current.Resources["PhoneDownloadUrl"].ToString();
                uri = new Uri(url);
            }
            
            wd.Navigate(uri, "_bank");

        }

        private void hyIMClient_Click(object sender, RoutedEventArgs e)
        {
            HtmlWindow wd = HtmlPage.Window;
            Uri uri = new Uri("http://smtonlineim.sinomaster.com/download.htm");
            string url = string.Empty;
            if (System.Windows.Application.Current.Resources.Contains("IMDownloadUrl"))
            {
                url = System.Windows.Application.Current.Resources["IMDownloadUrl"].ToString();
                uri = new Uri(url);
            }
            wd.Navigate(uri, "_bank");
        }
        #endregion
    }

    public class OnSelectionBoxClosedArgs : EventArgs
    {
        public OnSelectionBoxClosedArgs(string result)
        {
            this.Result = result;
        }
        public string Result { get; set; }
    }

}
