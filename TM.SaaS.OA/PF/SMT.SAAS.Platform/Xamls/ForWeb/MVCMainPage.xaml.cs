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

// 内容摘要: 主页面，用于承载主窗口相关元素

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

        private string LoadComplete;
        

        //主菜单
        Xamls.MainPagePart.MenusPanel _mainMenu;
        SplashScreenViewModel vm;

        //延迟打开
        private DispatcherTimer _waitPopupTimer;

        //强制控制导航 此功能需要完善之，此实现时间关系只能简单处理
        private bool _fromMenu = false;

        SMT.SAAS.Platform.WebParts.AsyncTools ayTools = new SMT.SAAS.Platform.WebParts.AsyncTools();

        DateTime dtstart = DateTime.Now;
        DateTime dtend = DateTime.Now;

        SAAS.ClientUtility.DictionaryManager dicManager = new ClientUtility.DictionaryManager();
        #endregion

        #region 初始化页面 MVC初始化silverlight时调用
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

            dicManager.OnDictionaryLoadCompleted += dicManager_OnDictionaryLoadCompleted;
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

            if (System.Windows.Application.Current.Resources["LoadComplete"] != null)
            {
                LoadComplete = System.Windows.Application.Current.Resources["LoadComplete"] as string;
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("js LoadComplete 事件：" + LoadComplete);
            }
            if (System.Windows.Application.Current.Resources["CurrentSysUserID"] != null && System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                ayTools.InitAsyncCompleted += new EventHandler(ayTools_InitAsyncCompleted);
                bMVCOpen = true;
            }
            ViewModel.Context.MainPanel = WorkHost;
            //ViewModel.Context.MainPanel.DefaultContent = WebPartHost;
            WorkHost.Back += new EventHandler(WorkHost_Back);
            //IsPopupHelper();
            
            InitSystemParams();
            dtend = DateTime.Now;
            string strmsg = "初始化MainPage耗时： " + (dtend - dtstart).Milliseconds.ToString() + " 毫秒";
            SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage(strmsg);
            dtstart = DateTime.Now;
            if (!SMT.SAAS.Main.CurrentContext.AppContext.IsLoadingCompleted)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.IsLoadingCompleted = true;
            }
            //MVC初始化silverlight时调用
            LoadPublicDic();
        }
        
        /// <summary>
        /// 默认加载公共字典，后续将由相关控件自身处理，
        /// </summary>
        private void LoadPublicDic()
        {
            List<string> dicCategorys = new List<string>();
            dicCategorys.Add("CHECKSTATE");
            dicCategorys.Add("MANUALTYPE");
            showLoadingBar();
            dicManager.LoadDictionary(dicCategorys);
           
        }

        void dicManager_OnDictionaryLoadCompleted(object sender, OnDictionaryLoadArgs e)
        {
            ayTools.BeginRun();
        }

        void ayTools_InitAsyncCompleted(object sender, EventArgs e)
        {
            string strModuleid = string.Empty, strOptType = string.Empty, strMessageid = string.Empty, strConfig = string.Empty;
            List<string> strMvcSource = new List<string>();
            if (System.Windows.Application.Current.Resources["MvcOpenRecordSource"] != null)
            {
                strMvcSource = System.Windows.Application.Current.Resources["MvcOpenRecordSource"] as List<string>;
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
           
            //mvc默认加载不打开任何页面
            HtmlPage.Window.Invoke("loadCompletedSL", new string[]{"ture","加载成功"});
            this.hideLoadingBar();
            //OpenModuleWithMVC(strModuleid, strOptType, strMessageid, strConfig);
        }
        /// <summary>
        /// 初始化系统所需参数.用于为窗口系统提供支持
        /// </summary>
        private void InitSystemParams()
        {
            System.Windows.Controls.Window.Parent = this.windowParent;
            System.Windows.Controls.Window.TaskBar = this.taskbar;//任务栏
            System.Windows.Controls.Window.Wrapper = this;
            System.Windows.Controls.Window.IsShowtitle = true;
        }
        #endregion

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


        public void NavigationWorkPanel(string tag)
        {
            try
            {
                WorkHost.PanelContent = null;
                if (!bMVCOpen)
                {
                    WorkHost.Visibility = Visibility.Visible;
                }
                //WebPartHost.Visibility = Visibility.Collapsed;
                //WebPartHost.Stop();

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
                                _mainMenu = new Xamls.MainPagePart.MenusPanel();
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
            finally
            {
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
            }
        }
        #endregion

        #region 无用代码
        #region 导航到菜单时触发
        private void menu_ShortCutClick(object sender, OnShortCutClickEventArgs e)
        {
            //礼品特殊处理
            ModuleInfo info = e.Result;
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
                    _mainMenu.showLoading();
                }

                MVCMainPage.isDrag = false;


                //1. 检测菜单权限
                CheckPermission(e.Result);
            }
        }



        private void _services_OnGetMenuPermissionCompleted(object sender, EventArgs e)
        {
            GetPermissionInfoUIByLocal();
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
                //WebPartHost.Star();
                //WebPartHost.Visibility = Visibility.Visible;

                WorkHost.Visibility = Visibility.Collapsed;
                var content = WorkHost.PanelContent;
            }
        }
        #endregion

        #region mvc平台专属调用
        public string OpenType = string.Empty;
        public string MoudleType = string.Empty;
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
            AppContext.SystemMessage("OpenModuleWithMVC-------------"
          + "strModuleid:" + strModuleid + System.Environment.NewLine
           + "strOptType:" + strOptType + System.Environment.NewLine
              + "strMessageid:" + strMessageid + System.Environment.NewLine
                 + "strConfig:" + strConfig + System.Environment.NewLine);
            try
            {
                showLoadingBar();
                if (string.IsNullOrWhiteSpace(strOptType))
                {
                    return;
                }
                OpenType = strOptType.ToUpper();
                //MessageBox.Show(OpenType);
                switch (OpenType)
                {
                    case "MODULE":
                        LoadModule(strModuleid);
                        break;
                    case "TASK":
                        LoadTask(strMessageid, strConfig);
                        break;
                    case "RECORD":
                        //if (strConfig.Length > 40)
                        //{
                        //    LoadMyRecordByConfig(strConfig);
                        //}
                        //else
                        //{
                        //    LoadMyRecord(strConfig);
                        //}
                        LoadMyRecord(strConfig);
                        break;
                    case "TRAVELRECORD":
                        //MessageBox.Show(strConfig);
                        LoadMyRecordByConfig(strConfig);
                       
                        break;
                    case "NEWSMANAGER":
                        LoadNews(strModuleid);
                        break;
                    case "CREATENEWFORM":
                        MoudleType = strModuleid;
                        CreateNewForm(strModuleid);
                        break;
                }
            }
            catch (Exception ex)
            {
                hideLoadingBar();
                HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
                AppContext.SystemMessage(ex.ToString());
                AppContext.ShowSystemMessageText();
            }
        }


        /// <summary>
        /// 打开我的单据,工作计划中打开出差我的单据
        /// </summary>
        /// <param name="strConfig"></param>
        [ScriptableMember]
        public void LoadMyRecordByConfig(string strConfig)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strConfig))
                {
                    hideLoadingBar();
                    HtmlPage.Window.Invoke("loadCompletedSL", "Invoke");
                    return;
                }

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


               // SMT.SAAS.Platform.WebParts.Views.MyRecord myRecordView = new WebParts.Views.MyRecord();
               // myRecordView.ShowMyRecord(strConfig);
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
            try
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

        /// <summary>
        /// 打开我的单据
        /// </summary>
        /// <param name="strConfig"></param>
        private void LoadMyRecord(string strConfig)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(strConfig))
                {
                    return;
                }
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


        #region 进度条控制
        private void showLoadingBar()
        {
            gridBottom.Visibility = Visibility.Collapsed;
            body.Visibility = Visibility.Collapsed;
            taskbar.Visibility = Visibility.Collapsed;
            windowParent.Visibility = Visibility.Collapsed;
            WorkPanel.Visibility = Visibility.Collapsed;
            loading.Visibility = Visibility.Visible;
            loading.Start();
        }
        private void hideLoadingBar()
        {
            gridBottom.Visibility = Visibility.Visible;
            body.Visibility = Visibility.Visible;
            taskbar.Visibility = Visibility.Visible;
            windowParent.Visibility = Visibility.Visible;
            WorkPanel.Visibility = Visibility.Visible;
            loading.Visibility = Visibility.Collapsed;
            loading.Stop();
        }
        #endregion
    }
}
