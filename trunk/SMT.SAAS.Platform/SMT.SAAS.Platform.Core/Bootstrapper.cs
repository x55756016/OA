using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SMT.SAAS.Platform.Core.Modularity;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: ϵͳ������ڣ���ϵͳ����һЩ��ʼ������
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core
{       
    /// <summary>
    /// ������࣬��������Ӧ�ó����ʼ����������
    /// </summary>
    /// <remarks>
    /// ��Ҫ��д���࣬���ڸ���������Ŀ�������������á�
    /// </remarks>
    public abstract class Bootstrapper
    {
        /// <summary>
        /// ϵͳĬ�ϵ���־�ࡣ���ڼ�¼ϵͳ���й����е�״̬�Լ��쳣��<see cref="ILoggerFacade"/>�� 
        /// </summary>
        /// <value>
        /// ��־ʵ����
        /// </value>
        protected ILoggerFacade Logger { get; set; }

        /// <summary>
        /// ϵͳģ��Ŀ¼��Ŀ¼Ϊ����ϵͳ��ʼ�����������ݡ�<see cref="IModuleCatalog"/> 
        /// </summary>
        /// <value>
        /// Ŀ¼ʵ����<see cref="IModuleCatalog"/>
        /// </value>
        protected IModuleCatalog ModuleCatalog { get; set; }

        protected IModuleInitializer ModuleInitializer { get; set; }


        /// <summary>
        /// �û��������������
        /// </summary>
        /// <value>
        /// �û���������ʵ����
        /// </value>
        public DependencyObject Shell { get; set; }

        /// <summary>
        /// ����һ����־�������<see cref="ILoggerFacade" />
        /// </summary>
        /// <remarks>
        /// ����һ��ʵ����<see cref="ILoggerFacade" />�ӿڵĶ���ʵ����
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Logger is added to the container which will dispose it when the container goes out of scope.")]
        protected virtual ILoggerFacade CreateLogger()
        {
            return Logging.Logger.Current;
        }

        /// <summary>
        /// ���У�����ϵͳ��������
        /// </summary>
        public void Run()
        {
            this.Run(true);
        }

        /// <summary>
        /// ����һ��Ĭ��ϵͳģ��Ŀ¼ʵ����<see cref="IModuleCatalog"/> 
        /// </summary>
        ///  <remarks>
        /// ����һ��<see cref="ModuleCatalog" />��Ĭ����ʵ��
        /// </remarks>
        protected virtual IModuleCatalog CreateModuleCatalog()
        {
            return new ModuleCatalog();
        }

        /// <summary>
        /// ����ϵͳģ��Ŀ¼����Ŀ¼���ݽ��г�ʼ����
        /// </summary>
        protected virtual void ConfigureModuleCatalog()
        {
        }

        /// <summary>
        /// ����ϵͳģ��ĳ�ʼ�����˷����ɸ��������߼���д��
        /// </summary>
        protected virtual void InitializeModules()
        {
            IModuleManager manager=null;
            //ͨ������ע����������һ��ʵ��
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

        #region ��������ͼ����---�ݲ�֧��

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
        /// ��ʼ��UI����
        /// </summary>
        protected virtual void InitializeShell()
        {
        }

        /// <summary>
        /// ���У�����ϵͳ��������
        /// </summary>
        /// <remarks>
        /// ��<param name="runWithDefaultConfiguration"/>Ϊ<see langword="true"/>��
        /// ��������ע��Ĭ�ϸ���Ӧ�ó�������,�����Ĭ�ϵĶ�����
        /// </remarks>
        public abstract void Run(bool runWithDefaultConfiguration);

        /// <summary>
        /// ����Ӧ�ó����UI��������
        /// </summary>
        /// <returns>
        /// Ӧ�ó����UI������
        /// </returns>
        /// <remarks>
        /// </remarks>
        protected abstract DependencyObject CreateShell();

        /// <summary>
        /// ��������λ�������ڴ�����������ʵ����
        /// </summary>
        protected abstract void ConfigureServiceLocator();
    }
}
