using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: 
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ������Ŀ/ģ�����.������Ŀ/ģ��ĳ�ʼ�������صȡ�
    /// </summary>
    public partial class ModuleManager : IModuleManager, IDisposable
    {
        private readonly IModuleInitializer moduleInitializer;
        private readonly IModuleCatalog moduleCatalog;
        private readonly ILoggerFacade loggerFacade;
        private IEnumerable<IModuleTypeLoader> typeLoaders;
        private HashSet<IModuleTypeLoader> subscribedToModuleTypeLoaders = new HashSet<IModuleTypeLoader>();

        /// <summary>
        /// ��ʼ�� <see cref="ModuleManager"/>����ʵ����
        /// </summary>
        /// <param name="appInitializer">
        /// ���ڸ���ģ��ĳ�ʼ����
        /// </param>
        /// <param name="appCatalog">ϵͳ/ģ����ϢĿ¼.</param>
        /// <param name="logger">���ڼ�¼ģ����ء���ʼ�������е���־.</param>
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
        /// ϵͳ/ģ��Ŀ¼�б�����Ϊϵͳ/ģ���ʼ����������ϵ�Ĵ����ṩ���ݡ�<BR/>
        /// Ĭ���ɹ��캯���ṩ��
        /// </summary>
        protected IModuleCatalog ModuleCatalog
        {
            get { return this.moduleCatalog; }
        }

        #region ģ������¼��Լ�����

        /// <summary>
        /// �¼���ģ���̨���ؽ��ȡ�
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
        /// ϵͳ/ģ����سɹ���ʧ�ܵ�ʱ�򴥷���
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
        /// ����ϵͳ/�������<BR/>
        /// Ĭ�ϼ���Ŀ¼��״̬��ģ��״̬Ϊ<see cref="InitializationMode.WhenAvailable"/>��ģ��.
        /// </summary>
        public void Run()
        {
            //��ʼ����ϵͳĿ¼��Ϣ��
            this.moduleCatalog.Initialize();

            //���ر�ʶΪWhenAvailable��ģ��.
            this.LoadModulesWhenAvailable();
        }

        /// <summary>
        /// ����ģ�����ƣ���ȡģ����Ϣ����������г�ʼ����
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
        /// �����ȡ��ģ��ĳ�����Ϣ��˵�������Ѿ������ˣ���ô��ǰӦ�ó�����Ͳ���Ҫ�ٴμ��ء�
        /// </summary>
        protected virtual bool ModuleNeedsRetrieval(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");

            if (moduleInfo.State == ModuleState.NotStarted)
            {
                //�����ȡ��ģ��ĳ�����Ϣ��˵�������Ѿ������ˣ�
                //��ô��ǰӦ�ó�����Ͳ���Ҫ�ٴμ��ء�
                //�������ҵ����ͣ����ʶ�Ѿ����أ����޸���ϵͳ״̬Ϊ���Խ��г�ʼ����
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
        /// ����Ŀ¼��״̬Ϊ<see cref="InitializationMode.WhenAvailable"/>����ϵͳ.
        /// </summary>
        private void LoadModulesWhenAvailable()
        {
            //������״̬Ϊ InitializationMode.WhenAvailable ����ϵͳ��Ϣ����������ϵͳ��ʼ����
            IEnumerable<ModuleInfo> whenAvailableModules = this.moduleCatalog.Modules.Where(m => m.InitializationMode == InitializationMode.WhenAvailable);
            IEnumerable<ModuleInfo> modulesToLoadTypes = this.moduleCatalog.CompleteListWithDependencies(whenAvailableModules);
            if (modulesToLoadTypes != null)
            {
                this.LoadModuleTypes(modulesToLoadTypes);
            }
        }

        /// <summary>
        /// ���ݸ�����ϵͳ/ģ���б�������ϵͳ��
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
                //�ж�ģ��״̬
                if (moduleInfo.State == ModuleState.NotStarted)
                {
                    //����ģ��ϵͳ
                    if (this.ModuleNeedsRetrieval(moduleInfo))
                    {
                        this.BeginRetrievingModule(moduleInfo);
                    }
                    else
                    {
                        //��������ؽ�����ϵͳ����ô��ʶϵͳΪ���Խ��г�ʼ����
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
        /// ��ʼ���Ѿ����سɹ���ģ�顣
        /// </summary>
        protected void LoadModulesThatAreReadyForLoad()
        {
            bool keepLoading = true;
            while (keepLoading)
            {
                keepLoading = false;
                //��ȡ����״̬Ϊ��׼�����ص�ģ�顣
                IEnumerable<ModuleInfo> availableModules = this.moduleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

                foreach (ModuleInfo moduleInfo in availableModules)
                {
                    if ((moduleInfo.State != ModuleState.Initialized) && (this.AreDependenciesLoaded(moduleInfo)))
                    {
                        //�޸�״̬Ϊ������...ֻ�д�״̬����ϵͳ�ſ��Խ��м���
                        moduleInfo.State = ModuleState.Initializing;
                        this.InitializeModule(moduleInfo);
                        keepLoading = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Ϊ������ɵ���ϵͳ���г�ʼ��������
        /// </summary>
        /// <param name="appInfo">
        /// ��Ҫ���г�ʼ����ģ����Ϣ��
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
        /// ���ݸ�����ģ����Ϣ����ʼ��ϵͳ���м���
        /// </summary>
        /// <param name="appInfo">��ϵͳ��Ϣ</param>
        private void BeginRetrievingModule(ModuleInfo moduleInfo)
        {
            ModuleInfo moduleInfoToLoadType = moduleInfo;
            //��ȡһ��ģ����ؽ�����
            IModuleTypeLoader moduleTypeLoader = this.GetTypeLoaderForModule(moduleInfoToLoadType);
            //�޸�ģ��״̬Ϊ������.
            moduleInfoToLoadType.State = ModuleState.LoadingTypes;

            // Delegate += ��WPF��SL�еĹ�����ʽ��ͬ.
            //Ϊÿһ��������������һ�ζ���.
            if (!this.subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
            {
                moduleTypeLoader.ModuleDownloadProgressChanged += this.IModuleTypeLoader_ModuleDownloadProgressChanged;
                moduleTypeLoader.LoadModuleCompleted += this.IModuleTypeLoader_LoadModuleCompleted;
                this.subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
            }

            //ģ�����
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
                //��ʼ����ɺ�������״̬Ϊ �ɽ��г�ʼ������ReadyForInitialization
                if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
                {
                    e.ModuleInfo.State = ModuleState.ReadyForInitialization;
                }

                //�˻ص�������UI�߳������У�����������һ���ġ�
                //����Լ��ں�̨�����˼��ط�ʽ����ôҪ���ǽ��˷�������UI�̴߳���.
                //������ܻᷢ�����߳�����.
                this.LoadModulesThatAreReadyForLoad();
            }
            else
            {
                this.RaiseLoadModuleCompleted(e);

                //�������û�д����򽫴���д����־�����׳�ģ������쳣.
                if (!e.IsErrorHandled)
                {
                    this.HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
                }
            }
        }

        /// <summary>
        /// ����ģ����ع����е��쳣��ʹ����־��¼�쳣��Ϣ�����׳�һ��<seealso cref="ModuleTypeLoadingException"/>��
        /// �������Ĭ���ṩ��һ�����䴦���ʵ�֡�
        /// </summary>
        /// <param name="moduleInfo">
        /// �����쳣��ģ����Ϣ��
        /// </param>
        /// <param name="exception">
        /// ������ǰ�쳣�������쳣ԭ��
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

        #region IDisposable �ӿ�ʵ��

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
