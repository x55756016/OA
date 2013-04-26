using System;
using System.Globalization;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// ��Ȩ����: ��Ȩ����(C)2011 SMT-Online
// ����ժҪ: Description
// ������ڣ�2011-04-21 
// ��    ����V1.0 
// ��    �ߣ�GaoY 
// �� �� �ˣ�
// �޸�ʱ�䣺 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// ʵ��<see cref="IModuleInitializer"/>�ӿڡ�
    /// ��װʵ���˶�ģ���ʼ���Ĺ�����
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILoggerFacade loggerFacade;

        /// <summary>
        /// ��ʼ��<see cref="ModuleInitializer"/>��ʵ����
        /// </summary>
        /// <param name="serviceLocator">
        /// ����������ݸ�����ģ�����ͽ��н�����
        /// </param>
        /// <param name="loggerFacade">
        /// ������¼��־��
        /// </param>
        public ModuleInitializer(IServiceLocator serviceLocator, ILoggerFacade loggerFacade)
        {
            if (serviceLocator == null)
            {
                throw new ArgumentNullException("serviceLocator");
            }

            if (loggerFacade == null)
            {
                throw new ArgumentNullException("loggerFacade");
            }

            this.serviceLocator = serviceLocator;
            this.loggerFacade = loggerFacade;
        }

        /// <summary>
        /// ���ݸ�����ģ��ϴϴ�ԣ���ʼ��ģ�顣
        /// </summary>
        /// <param name="moduleInfo">
        /// ���ڳ�ʼ����ģ�顣
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catches Exception to handle any exception thrown during the initialization process with the HandleModuleInitializationError method.")]
        public void Initialize(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");

            IModule moduleInstance = null;
            try
            {
                moduleInstance = this.CreateModule(moduleInfo);

                if (moduleInstance != null)
                    moduleInstance.Initialize();

            }
            catch (Exception ex)
            {
                this.HandleModuleInitializationError(
                    moduleInfo,
                    moduleInstance != null ? moduleInstance.GetType().Assembly.FullName : null,
                    ex);
            }
        }

        /// <summary>
        /// ����ģ���ʼ�������е��κ��쳣��ʹ��<seealso cref="ILoggerFacade"/>��¼�������׳�
        /// <seealso cref="ModuleInitializeException"/>��
        /// ������д��������ṩ��ͬ�ķ�ʽ��
        /// </summary>
        /// <param name="moduleInfo">
        /// �����쳣��ģ����Ϣ
        /// </param>
        /// <param name="assemblyName">
        /// �������ơ�
        /// </param>
        /// <param name="exception">
        /// ����ǰ������쳣��
        /// </param>
        /// <exception cref="ModuleInitializeException"></exception>
        public virtual void HandleModuleInitializationError(ModuleInfo moduleInfo, string assemblyName, Exception exception)
        {
            if (moduleInfo == null) throw new ArgumentNullException("moduleInfo");
            if (exception == null) throw new ArgumentNullException("exception");

            Exception moduleException;

            if (exception is ModuleInitializeException)
            {
                moduleException = exception;
            }
            else
            {
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    moduleException = new ModuleInitializeException(moduleInfo.ModuleName, assemblyName, exception.Message, exception);
                }
                else
                {
                    moduleException = new ModuleInitializeException(moduleInfo.ModuleName, exception.Message, exception);
                }
            }

            this.loggerFacade.Log(moduleException.ToString(), Category.Exception, Priority.High);

            throw moduleException;
        }

        /// <summary>
        /// ���ݹ涨���������ƣ�ʹ����������һ��<see cref="IModule"/>ʵ����
        /// </summary>
        /// <param name="moduleInfo">
        /// Ҫ������ģ�顣
        /// </param>
        /// <returns>
        /// <paramref name="moduleInfo"/>ָ����ģ��ʵ����
        /// </returns>
        protected virtual IModule CreateModule(ModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("moduleInfo");
            }

            if (string.IsNullOrEmpty(moduleInfo.ModuleType))
                return null;

            return this.CreateModule(moduleInfo.ModuleType);
        }

        /// <summary>
        /// ���ݹ涨���������ƣ�ʹ����������һ��<see cref="IModule"/>ʵ����
        /// </summary>
        /// <param name="typeName">
        /// �����������ơ�������ͱ���ʵ��<see cref="IModule"/>��
        /// </param>
        /// <returns>
        ///  <paramref name="typeName"/>���͵���ʵ����
        /// </returns>
        protected virtual IModule CreateModule(string typeName)
        {
            Type moduleType = Type.GetType(typeName);
            if (moduleType == null)
            {
                throw new ModuleInitializeException(string.Format(CultureInfo.CurrentCulture, Resources.FailedToGetType, typeName));
            }

            //�ж����ͺ��ٴ���ʵ��
            if (moduleType.Equals(typeof(IModule)))
            {
                return this.serviceLocator.GetInstance(moduleType) as IModule;
            }
            return null;
        }
    }
}
