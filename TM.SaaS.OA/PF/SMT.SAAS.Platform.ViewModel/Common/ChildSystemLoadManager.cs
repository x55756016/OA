using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core;
using SMT.SAAS.Platform.Logging;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.Saas.Tools.PermissionWS;
using SMT.SAAS.Platform.Xamls.LoginPart;
using System.Reflection;
using System.Linq;

// 内容摘要: 托管管理，统一处理系统、模块、页面的请求以及处理其依赖关系。

namespace SMT.SAAS.Platform.ViewModel
{
    /// <summary>
    /// 托管管理，统一处理系统、模块、页面的请求以及处理其依赖关系。
    /// </summary>
    public class ChildSystemLoadManager
    {
        private MainPagePartManager MainPageManeger;
        private Assembly asmMain = null;
        private string strSystemType="";
        private V_UserMenuPermission menuInfo;
        /// <summary>
        /// 构造函数
        /// </summary>
        public ChildSystemLoadManager()
        {
            MainPageManeger = new MainPagePartManager();
            //MainPageManeger.NotifyUserMessageEvent += MainPageManeger_NotifyUserMessageEvent;
            MainPageManeger.FilePath = "ChildSystem";
            MainPageManeger.LoadDLLCompleted += new EventHandler(loadMainPage_LoadCompleted);
            MainPageManeger.UpdateDllCompleted += new EventHandler(MainPageManeger_UpdateDllCompleted);
        }


        /// <summary>
        /// 加载模块完成事件
        /// </summary>
        public event EventHandler<LoadModuleEventArgs> OnSystemLoadXapPacketCompleted;
        /// <summary>
        /// 加载子系统完成事件
        /// </summary>
        /// <param name="MenuCode"></param>
        public void LoadChildSystemModule(string MenuCode)
        {
            var entityMenu = (from ent in Context.CacheMenuAll
                                where ent.MENUCODE == MenuCode
                                select ent).FirstOrDefault();
            menuInfo = entityMenu;
            var systemTypeDic=(from ent in Context.CacheSystemType
                                where ent.DICTIONARYVALUE.Value.ToString() == entityMenu.SYSTEMTYPE
                                select ent).FirstOrDefault();
            strSystemType = systemTypeDic.SYSTEMNEED;
            MainPageManeger.LoadXapName = systemTypeDic.SYSTEMNEED;


            if (Context.allChildSystemAssembly.ContainsKey(systemTypeDic.SYSTEMNEED))
            {
                FrameworkElement MenuPage;
                if (Context.CacheMenuUIElement.ContainsKey(menuInfo.MENUCODE))
                {
                    MenuPage = Context.CacheMenuUIElement[menuInfo.MENUCODE] as FrameworkElement;
                }
                else
                {
                    asmMain = Context.allChildSystemAssembly[systemTypeDic.SYSTEMNEED] as Assembly;
                    string pageObjName = strSystemType.Replace(".xap", "") + ".Views" + menuInfo.URLADDRESS.Replace("/", ".");
                    MenuPage = asmMain.CreateInstance(pageObjName) as FrameworkElement;
                    Context.CacheMenuUIElement.Add(menuInfo.MENUCODE, MenuPage);
                }
                if (OnSystemLoadXapPacketCompleted != null)
                {
                    LoadModuleEventArgs arg = new LoadModuleEventArgs(MenuPage, menuInfo, null);
                    this.OnSystemLoadXapPacketCompleted(null, arg);
                }
            }
            else
            {
                MainPageManeger.VertionFileName = systemTypeDic.SYSTEMNEED + ".xml";
                MainPageManeger.dllVersionUpdataCheck();
            }
        }

      
        #region "Assembly管理器事件"
        /// <summary>
        /// 更新下载所有dll完毕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainPageManeger_UpdateDllCompleted(object sender, EventArgs e)
        {
            #region 自动登录系统        
            try
            {
                MainPageManeger.RunWorkerLoadAssemblyPart();
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(" 自动登录系统出错：" + ex.ToString());
                if (OnSystemLoadXapPacketCompleted != null)
                {
                    LoadModuleEventArgs arg = new LoadModuleEventArgs(null, menuInfo, null);
                    this.OnSystemLoadXapPacketCompleted(null, arg);
                }
            }
            #endregion
        }
        /// <summary>
        /// 加载所有dll完成并在主页面显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void loadMainPage_LoadCompleted(object sender, EventArgs e)
        {
            FrameworkElement MenuUIElement=null;
            try
            {
                MainPagePartManager loadmain = sender as MainPagePartManager;
                asmMain = loadmain.asmMain;


                string pageObjName = strSystemType.Replace(".xap", "") + ".Views" + menuInfo.URLADDRESS.Replace("/", ".");


                MenuUIElement = asmMain.CreateInstance(pageObjName) as FrameworkElement;

                Context.allChildSystemAssembly.Add(strSystemType, asmMain);
                Context.CacheMenuUIElement.Add(menuInfo.MENUCODE, MenuUIElement);
                //AppContext.AppHost.SetRootVisual(MainPage);
            }
            catch (Exception ex)
            {
                SMT.SAAS.Main.CurrentContext.AppContext.logAndShow(ex.ToString());
            }
            finally
            {
                if (OnSystemLoadXapPacketCompleted != null)
                {
                    LoadModuleEventArgs arg = new LoadModuleEventArgs(MenuUIElement, menuInfo, null);
                    this.OnSystemLoadXapPacketCompleted(null, arg);
                }
            }
        }
        #endregion
    }

    public class LoadModuleEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化<see cref="LoadModuleCompletedEventArgs"/>的新实例。
        /// </summary>
        /// <param name="moduleInfo">模块信息。</param>
        /// <param name="error">产生的异常。</param>
         public LoadModuleEventArgs(object moduleInstance, V_UserMenuPermission moduleInfo, Exception error)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }
            this.ModuleInstance = moduleInstance;
            this.ModuleInfo = moduleInfo;
            this.Error = error;
        }
        /// <summary>
        /// 获取模块信息。
        /// </summary>
        /// <value>The module info.</value>
        public V_UserMenuPermission ModuleInfo { get; private set; }

        /// <summary>
        /// 获取模块信息。
        /// </summary>
        /// <value>The module info.</value>
        public object ModuleInstance { get; private set; }
        /// <summary>
        /// 获取产生的异常。
        /// </summary>
        /// <value>
        /// 任何可能产生的异常，否则设置为NULL。
        /// </value>
        public Exception Error { get; private set; }

        /// <summary>
        ///  获取或设置一个值，该值指明错误是否已经由事件订阅对象处理。
        /// Gets or sets a value indicating whether the error has been handled by the event subscriber.
        /// </summary>
        /// <value>若事件已经处理为<c>true</c>; 否则为<c>false</c>.</value>
        /// <remarks>
        /// 若事件有错误，事件订阅对象未设置值为True，那么事件发布对象将会产生异常
        /// </remarks>
        public bool IsErrorHandled { get; set; }
    }
}
