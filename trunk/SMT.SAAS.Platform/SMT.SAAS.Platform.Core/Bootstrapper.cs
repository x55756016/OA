using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 系统启动入口，对系统进行一些初始化配置
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core
{       
    /// <summary>
    /// 抽象基类，用于引导应用程序初始化和启动。
    /// </summary>
    /// <remarks>
    /// 需要重写此类，用于根据自身项目需求对其进行配置。
    /// </remarks>
    public abstract class Bootstrapper
    {
        /// <summary>
        /// 系统默认的日志类。用于记录系统运行过程中的状态以及异常。<see cref="ILoggerFacade"/>。 
        /// </summary>
        /// <value>
        /// 日志实例。
        /// </value>
        protected ILoggerFacade Logger { get; set; }

        /// <summary>
        /// 系统模块目录，目录为整个系统初始化的依赖数据。<see cref="IModuleCatalog"/> 
        /// </summary>
        /// <value>
        /// 目录实例。<see cref="IModuleCatalog"/>
        /// </value>
        protected IModuleCatalog ModuleCatalog { get; set; }

        protected IModuleInitializer ModuleInitializer { get; set; }


        /// <summary>
        /// 用户界面的主容器。
        /// </summary>
        /// <value>
        /// 用户界面容器实例。
        /// </value>
        public DependencyObject Shell { get; set; }

        /// <summary>
        /// 创建一个日志管理程序。<see cref="ILoggerFacade" />
        /// </summary>
        /// <remarks>
        /// 返回一个实现了<see cref="ILoggerFacade" />接口的对象实例。
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Logger is added to the container which will dispose it when the container goes out of scope.")]
        protected virtual ILoggerFacade CreateLogger()
        {
            return Logging.Logger.Current;
        }

        /// <summary>
        /// 运行，启动系统引导程序。
        /// </summary>
        public void Run()
        {
            this.Run(true);
        }

        /// <summary>
        /// 创建一个默认系统模块目录实例。<see cref="IModuleCatalog"/> 
        /// </summary>
        ///  <remarks>
        /// 返回一个<see cref="ModuleCatalog" />的默认新实例
        /// </remarks>
        protected virtual IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog();
        }

        /// <summary>
        /// 配置系统模块目录，对目录数据进行初始化。
        /// </summary>
        protected virtual void ConfigureModuleCatalog()
        {
        }

        /// <summary>
        /// 启动系统模块的初始化。此方法可根据自身逻辑重写。
        /// </summary>
        protected virtual void InitializeModules()
        {
            IModuleManager manager=null;
            //通过依赖注入容器返回一个实例
            //IModuleManager manager = ServiceLocator.Current.GetInstance<IModuleManager>();
             manager.Run();
        }

        /// <summary>
        /// 
        /// Registers the <see cref="Type"/>s of the Exceptions that are not 
        /// considered root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected virtual void RegisterFrameworkExceptionTypes()
        {
            //ExceptionExtensions.RegisterFrameworkExceptionType(
            //    typeof(Microsoft.Practices.ServiceLocation.ActivationException));
        }

        #region 区域与视图管理---暂不支持

        //        protected virtual RegionAdapterMappings ConfigureRegionAdapterMappings()
//        {
//            RegionAdapterMappings regionAdapterMappings = ServiceLocator.Current.GetInstance<RegionAdapterMappings>();
//            if (regionAdapterMappings != null)
//            {
//#if SILVERLIGHT
//                regionAdapterMappings.RegisterMapping(typeof(TabControl), ServiceLocator.Current.GetInstance<TabControlRegionAdapter>());
//#endif
//                regionAdapterMappings.RegisterMapping(typeof(Selector), ServiceLocator.Current.GetInstance<SelectorRegionAdapter>());
//                regionAdapterMappings.RegisterMapping(typeof(ItemsControl), ServiceLocator.Current.GetInstance<ItemsControlRegionAdapter>());
//                regionAdapterMappings.RegisterMapping(typeof(ContentControl), ServiceLocator.Current.GetInstance<ContentControlRegionAdapter>());
//            }

//            return regionAdapterMappings;
//        }

      
        //protected virtual IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        //{
        //    var defaultRegionBehaviorTypesDictionary = ServiceLocator.Current.GetInstance<IRegionBehaviorFactory>();

        //    if (defaultRegionBehaviorTypesDictionary != null)
        //    {
        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(AutoPopulateRegionBehavior.BehaviorKey,
        //                                                          typeof(AutoPopulateRegionBehavior));

        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(BindRegionContextToDependencyObjectBehavior.BehaviorKey,
        //                                                          typeof(BindRegionContextToDependencyObjectBehavior));

        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(RegionActiveAwareBehavior.BehaviorKey,
        //                                                          typeof(RegionActiveAwareBehavior));

        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(SyncRegionContextWithHostBehavior.BehaviorKey,
        //                                                          typeof(SyncRegionContextWithHostBehavior));

        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(RegionManagerRegistrationBehavior.BehaviorKey,
        //                                                          typeof(RegionManagerRegistrationBehavior));

        //        defaultRegionBehaviorTypesDictionary.AddIfMissing(RegionMemberLifetimeBehavior.BehaviorKey,
        //                                          typeof(RegionMemberLifetimeBehavior));

        //    }

        //    return defaultRegionBehaviorTypesDictionary;
        //}
        #endregion

        /// <summary>
        /// 初始化UI容器
        /// </summary>
        protected virtual void InitializeShell()
        {
        }

        /// <summary>
        /// 运行，启动系统引导程序。
        /// </summary>
        /// <remarks>
        /// 若<param name="runWithDefaultConfiguration"/>为<see langword="true"/>，
        /// 在容器中注册默认复合应用程序库服务,这个是默认的动作。
        /// </remarks>
        public abstract void Run(bool runWithDefaultConfiguration);

        /// <summary>
        /// 创建应用程序的UI主容器。
        /// </summary>
        /// <returns>
        /// 应用程序的UI主容器
        /// </returns>
        /// <remarks>
        /// </remarks>
        protected abstract DependencyObject CreateShell();

        /// <summary>
        /// 创建服务定位器，用于创建请求对象的实例。
        /// </summary>
        protected abstract void ConfigureServiceLocator();
    }
}
