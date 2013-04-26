using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using SMT.SAAS.Platform.Core;
using SMT.SAAS.Platform.Logging;
using SMT.SAAS.Platform.Core.Modularity;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 托管管理，统一处理系统、模块、页面的请求以及处理其依赖关系。
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.ViewModel
{
    /// <summary>
    /// 托管管理，统一处理系统、模块、页面的请求以及处理其依赖关系。
    /// </summary>
    public class Managed : Bootstrapper
    {
        private bool useDefaultConfiguration = true;
        private IModuleManager manager = null;
        private ServiceLocator serviceLocator = null;
        public event EventHandler<LoadModuleEventArgs> OnLoadModuleCompleted;
        public event EventHandler<LoadModuleEventArgs> OnSystemLoadModuleCompleted;

        public List<ModuleInfo> Catalog { get; set; }
       // public List<ModuleInfo> SystemCatalog { get; set; }

        /// <summary>
        /// 初始化主容器，目前默认设置为MainPage
        /// </summary>
        protected override DependencyObject CreateShell()
        {
            return this.Shell != null ? this.Shell : null;
        }

        /// <summary>
        /// 配置本地服务定位器。目前实现机制为使用反射创建依赖对象。而非IOC容器。
        /// </summary>
        protected override void ConfigureServiceLocator()
        {
        }

        /// <summary>
        /// 配置系统模块目录。此目录根据系统权限、平台基础信息最终生成而得到的。
        /// </summary>
        protected override void ConfigureModuleCatalog()
        {
            foreach (var item in Catalog)
            {
                ModuleCatalog.AddModule(item);
            }
        }

        /// <summary>
        /// 初始化主容器，目前默认设置为MainPage
        /// </summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
        }

        /// <summary>
        /// 启动模块初始化。启动模块初始化需要依赖的对象：
        /// <see cref="IServiceLocator"/>、<see cref="IModuleInitializer"/>、<see cref="ILoggerFacade"/>
        /// </summary>
        protected override void InitializeModules()
        {
            try
            {
                serviceLocator = new ServiceLocator();
                ModuleInitializer initializer = new Core.Modularity.ModuleInitializer(serviceLocator, Logger);
                manager = new ModuleManager(initializer, ModuleCatalog, Logger);
                manager.LoadModuleCompleted += new EventHandler<LoadModuleCompletedEventArgs>(manager_LoadModuleCompleted);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("IModuleCatalog"))
                {
                    throw new InvalidOperationException(Resources.NullModuleCatalogException);
                }
                throw;
            }

            if (manager != null)
                manager.Run();

        }

        void manager_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {

            if (e.ModuleInfo.UseState != "1")
            {
                object content = serviceLocator.GetInstance(e.ModuleInfo);

                if (this.OnLoadModuleCompleted != null)
                    this.OnLoadModuleCompleted(this, new LoadModuleEventArgs(content, e.ModuleInfo, e.Error));
            }
            else
            {
                if (this.OnSystemLoadModuleCompleted != null)
                    this.OnSystemLoadModuleCompleted(this, new LoadModuleEventArgs(null, e.ModuleInfo, e.Error));
                
            }
        }

        /// <summary>
        /// 运行托管程序。
        /// 负责统一处理系统、模块、页面等依赖关系。
        /// </summary>
        /// <param name="runWithDefaultConfiguration"></param>
        public override void Run(bool runWithDefaultConfiguration)
        {
            this.useDefaultConfiguration = runWithDefaultConfiguration;

            //获取日志对象
            this.Logger = this.CreateLogger();

            if (this.Logger == null)
            {
                throw new InvalidOperationException(Resources.NullLoggerFacadeException);
            }

            this.Logger.Log(Resources.LoggerCreatedSuccessfully, Category.Debug, Priority.Low);

            this.Logger.Log(Resources.CreatingModuleCatalog, Category.Debug, Priority.Low);

            //创建目录
            this.ModuleCatalog = this.CreateModuleCatalog();

            if (this.ModuleCatalog == null)
            {
                throw new InvalidOperationException(Resources.NullModuleCatalogException);
            }

            this.Logger.Log(Resources.ConfiguringModuleCatalog, Category.Debug, Priority.Low);

            //配置目录
            this.ConfigureModuleCatalog();

            this.Logger.Log(Resources.ConfiguringServiceLocatorSingleton, Category.Debug, Priority.Low);
            //配置定位器、暂未使用
            this.ConfigureServiceLocator();

            this.Logger.Log(Resources.CreatingShell, Category.Debug, Priority.Low);

            this.Logger.Log(Resources.InitializingModules, Category.Debug, Priority.Low);
            //初始化模块
            this.InitializeModules();

            //创建主容器
            this.Shell = this.CreateShell();

            this.Logger.Log(Resources.InitializingShell, Category.Debug, Priority.Low);

            //初始化主容器
            this.InitializeShell();

            this.Logger.Log(Resources.BootstrapperSequenceCompleted, Category.Debug, Priority.Low);
        }

        public void LoadModule(string moduleName)
        {
            if (this.manager != null)
                manager.LoadModule(moduleName);
        }
    }

    public class LoadModuleEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化<see cref="LoadModuleCompletedEventArgs"/>的新实例。
        /// </summary>
        /// <param name="moduleInfo">模块信息。</param>
        /// <param name="error">产生的异常。</param>
        public LoadModuleEventArgs(object moduleInstance, ModuleInfo moduleInfo, Exception error)
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
        public ModuleInfo ModuleInfo { get; private set; }

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
