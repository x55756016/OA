using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 用于项目/模块管理.负责项目/模块的初始化、加载等。
    /// </summary>
    public partial class ModuleManager : IModuleManager, IDisposable
    {
        private readonly IModuleInitializer moduleInitializer;
        private readonly IModuleCatalog moduleCatalog;
        private readonly ILoggerFacade loggerFacade;
        private IEnumerable<IModuleTypeLoader> typeLoaders;
        private HashSet<IModuleTypeLoader> subscribedToModuleTypeLoaders = new HashSet<IModuleTypeLoader>();

        /// <summary>
        /// 初始化 <see cref="ModuleManager"/>的新实例。
        /// </summary>
        /// <param name="appInitializer">
        /// 用于负责模块的初始化。
        /// </param>
        /// <param name="appCatalog">系统/模块信息目录.</param>
        /// <param name="logger">用于记录模块加载、初始化过程中的日志.</param>
        public ModuleManager(IModuleInitializer moduleInitializer, IModuleCatalog moduleCatalog, ILoggerFacade loggerFacade)
        {
            if (moduleInitializer == null)
            {
                throw new ArgumentNullException("moduleInitializer");
            }

            if (moduleCatalog == null)
            {
                throw new ArgumentNullException("moduleCatalog");
            }

            if (loggerFacade == null)
            {
                throw new ArgumentNullException("loggerFacade");
            }

            this.moduleInitializer = moduleInitializer;
            this.moduleCatalog = moduleCatalog;
            this.loggerFacade = loggerFacade;
        }

        /// <summary>
        /// 系统/模块目录列表，用于为系统/模块初始化、依赖关系的处理提供数据。<BR/>
        /// 默认由构造函数提供。
        /// </summary>
        protected IModuleCatalog ModuleCatalog
        {
            get { return this.moduleCatalog; }
        }

        #region 模块加载事件以及触发

        /// <summary>
        /// 事件，模块后台加载进度。
        /// </summary>
        public event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        private void RaiseModuleDownloadProgressChanged(ModuleDownloadProgressChangedEventArgs e)
        {
            if (this.ModuleDownloadProgressChanged != null)
            {
                this.ModuleDownloadProgressChanged(this, e);
            }
        }

        /// <summary>
        /// 系统/模块加载成功或失败的时候触发。
        /// </summary>
        public event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;

        private void RaiseLoadModuleCompleted(ModuleInfo moduleInfo, Exception error)
        {
            this.RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
        }

        private void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
        {
            if (this.LoadModuleCompleted != null)
            {
                this.LoadModuleCompleted(this, e);
            }
        }

        #endregion

        /// <summary>
        /// 启动系统/管理程序。<BR/>
        /// 默认加载目录中状态中模块状态为<see cref="InitializationMode.WhenAvailable"/>的模块.
        /// </summary>
        public void Run()
        {
            //初始化子系统目录信息。
            this.moduleCatalog.Initialize();

            //加载标识为WhenAvailable的模块.
            this.LoadModulesWhenAvailable();
        }

        /// <summary>
        /// 根据模块名称，获取模块信息，并对其进行初始化。
        /// </summary>
        /// <param name="moduleName"></param>
        public void LoadModule(string moduleName)
        {
            IEnumerable<ModuleInfo> module = this.moduleCatalog.Modules.Where(m => m.ModuleName == moduleName);
            if (module == null || module.Count() != 1)
            {
                throw new ModuleNotFoundException(moduleName, string.Format(CultureInfo.CurrentCulture, Resources.ModuleNotFound, moduleName));
            }

            IEnumerable<ModuleInfo> modulesToLoad = this.moduleCatalog.CompleteListWithDependencies(module);

            this.LoadModuleTypes(modulesToLoad);
        }

        /// <summary>
        /// 如果获取到模块的程序集信息则说明程序集已经加载了，那么当前应用程序域就不需要再次加载。
        /// </summary>
        protected virtual bool ModuleNeedsRetrieval(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");

            if (moduleInfo.State == ModuleState.NotStarted)
            {
                //如果获取到模块的程序集信息则说明程序集已经加载了，
                //那么当前应用程序域就不需要再次加载。
                //若可以找到类型，则标识已经加载，并修改子系统状态为可以进行初始化。
                bool isAvailable = false;
                if (moduleInfo.ModuleType != null)
                    isAvailable = Type.GetType(moduleInfo.ModuleType) != null;

                if (isAvailable)
                {
                    moduleInfo.State = ModuleState.ReadyForInitialization;
                }

                return !isAvailable;
            }

            return false;
        }

        /// <summary>
        /// 加载目录中状态为<see cref="InitializationMode.WhenAvailable"/>的子系统.
        /// </summary>
        private void LoadModulesWhenAvailable()
        {
            //检索出状态为 InitializationMode.WhenAvailable 的子系统信息，并启动子系统初始化。
            IEnumerable<ModuleInfo> whenAvailableModules = this.moduleCatalog.Modules.Where(m => m.InitializationMode == InitializationMode.WhenAvailable);
            IEnumerable<ModuleInfo> modulesToLoadTypes = this.moduleCatalog.CompleteListWithDependencies(whenAvailableModules);
            if (modulesToLoadTypes != null)
            {
                this.LoadModuleTypes(modulesToLoadTypes);
            }
        }

        /// <summary>
        /// 根据给定的系统/模块列表，加载子系统。
        /// </summary>
        /// <param name="appInfos"></param>
        private void LoadModuleTypes(IEnumerable<ModuleInfo> moduleInfos)
        {
            if (moduleInfos == null)
            {
                return;
            }

            foreach (ModuleInfo moduleInfo in moduleInfos)
            {
                //判断模块状态
                if (moduleInfo.State == ModuleState.NotStarted)
                {
                    //启动模块系统
                    if (this.ModuleNeedsRetrieval(moduleInfo))
                    {
                        this.BeginRetrievingModule(moduleInfo);
                    }
                    else
                    {
                        //若无需加载解析子系统，那么标识系统为可以进行初始化。
                        moduleInfo.State = ModuleState.ReadyForInitialization;
                    }
                }
                else if (moduleInfo.State == ModuleState.Initialized)
                {
                    this.RaiseLoadModuleCompleted(moduleInfo, null);
                }
            }

            this.LoadModulesThatAreReadyForLoad();
        }

        /// <summary>
        /// 初始化已经加载成功的模块。
        /// </summary>
        protected void LoadModulesThatAreReadyForLoad()
        {
            bool keepLoading = true;
            while (keepLoading)
            {
                keepLoading = false;
                //获取所有状态为已准备加载的模块。
                IEnumerable<ModuleInfo> availableModules = this.moduleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

                foreach (ModuleInfo moduleInfo in availableModules)
                {
                    if ((moduleInfo.State != ModuleState.Initialized) && (this.AreDependenciesLoaded(moduleInfo)))
                    {
                        //修改状态为加载中...只有此状态的子系统才可以进行加载
                        moduleInfo.State = ModuleState.Initializing;
                        this.InitializeModule(moduleInfo);
                        keepLoading = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 为加载完成的子系统进行初始化操作。
        /// </summary>
        /// <param name="appInfo">
        /// 需要进行初始化的模块信息。
        /// </param>
        private void InitializeModule(ModuleInfo moduleInfo)
        {
            if (moduleInfo.State == ModuleState.Initializing)
            {
                this.moduleInitializer.Initialize(moduleInfo);
                moduleInfo.State = ModuleState.Initialized;
                this.RaiseLoadModuleCompleted(moduleInfo, null);
            }
        }

        /// <summary>
        /// 根据给定的模块信息，开始对系统进行加载
        /// </summary>
        /// <param name="appInfo">子系统信息</param>
        private void BeginRetrievingModule(ModuleInfo moduleInfo)
        {
            ModuleInfo moduleInfoToLoadType = moduleInfo;
            //获取一个模块加载解析器
            IModuleTypeLoader moduleTypeLoader = this.GetTypeLoaderForModule(moduleInfoToLoadType);
            //修改模块状态为加载中.
            moduleInfoToLoadType.State = ModuleState.LoadingTypes;

            // Delegate += 在WPF和SL中的工作方式不同.
            //为每一个加载器仅进行一次订阅.
            if (!this.subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
            {
                moduleTypeLoader.ModuleDownloadProgressChanged += this.IModuleTypeLoader_ModuleDownloadProgressChanged;
                moduleTypeLoader.LoadModuleCompleted += this.IModuleTypeLoader_LoadModuleCompleted;
                this.subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
            }

            //模块加载
            moduleTypeLoader.LoadModuleType(moduleInfo);
        }

        private void IModuleTypeLoader_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
        {
            this.RaiseModuleDownloadProgressChanged(e);
        }

        private void IModuleTypeLoader_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                //初始化完成后，设置其状态为 可进行初始化，即ReadyForInitialization
                if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
                {
                    e.ModuleInfo.State = ModuleState.ReadyForInitialization;
                }

                //此回调可能在UI线程上运行，但并不不是一定的。
                //如果自己在后台定义了加载方式。那么要考虑将此方法交给UI线程处理.
                //否则可能会发生跨线程问题.
                this.LoadModulesThatAreReadyForLoad();
            }
            else
            {
                this.RaiseLoadModuleCompleted(e);

                //如果错误没有处理，则将错误写入日志，并抛出模块解析异常.
                if (!e.IsErrorHandled)
                {
                    this.HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
                }
            }
        }

        /// <summary>
        /// 处理模块加载过程中的异常，使用日志记录异常信息，并抛出一个<seealso cref="ModuleTypeLoadingException"/>。
        /// 这个方法默认提供了一个对其处理的实现。
        /// </summary>
        /// <param name="moduleInfo">
        /// 产生异常的模块信息。
        /// </param>
        /// <param name="exception">
        /// 产生当前异常的真正异常原因。
        /// </param>
        /// <exception cref="ModuleTypeLoadingException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        protected virtual void HandleModuleTypeLoadingError(ModuleInfo moduleInfo, Exception exception)
        {
            if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");

            ModuleTypeLoadingException moduleTypeLoadingException = exception as ModuleTypeLoadingException;

            if (moduleTypeLoadingException == null)
            {
                moduleTypeLoadingException = new ModuleTypeLoadingException(moduleInfo.ModuleName, exception.Message, exception);
            }

            this.loggerFacade.Log(moduleTypeLoadingException.Message, Category.Exception, Priority.High);

            throw moduleTypeLoadingException;
        }

        private bool AreDependenciesLoaded(ModuleInfo moduleInfo)
        {
            IEnumerable<ModuleInfo> requiredModules = this.moduleCatalog.GetDependentModules(moduleInfo);
            if (requiredModules == null)
            {
                return true;
            }

            int notReadyRequiredModuleCount =
                requiredModules.Count(requiredModule => requiredModule.State != ModuleState.Initialized);

            return notReadyRequiredModuleCount == 0;
        }

        private IModuleTypeLoader GetTypeLoaderForModule(ModuleInfo moduleInfo)
        {
            foreach (IModuleTypeLoader typeLoader in this.ModuleTypeLoaders)
            {
                if (typeLoader.CanLoadModuleType(moduleInfo))
                {
                    return typeLoader;
                }
            }

            throw new ModuleTypeLoaderNotFoundException(moduleInfo.ModuleName, String.Format(CultureInfo.CurrentCulture, Resources.NoRetrieverCanRetrieveModule, moduleInfo.ModuleName), null);
        }

        #region IDisposable 接口实现

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the associated <see cref="IModuleTypeLoader"/>s.
        /// </summary>
        /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            foreach (IModuleTypeLoader typeLoader in this.ModuleTypeLoaders)
            {
                IDisposable disposableTypeLoader = typeLoader as IDisposable;
                if (disposableTypeLoader != null)
                {
                    disposableTypeLoader.Dispose();
                }
            }
        }

        #endregion
    }
}
