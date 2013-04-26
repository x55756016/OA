using System;
using System.Globalization;
using SMT.SAAS.Platform.Logging;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: Description
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core.Modularity
{
    /// <summary>
    /// 实现<see cref="IModuleInitializer"/>接口。
    /// 封装实现了对模块初始化的工作。
    /// </summary>
    public class ModuleInitializer : IModuleInitializer
    {
        private readonly IServiceLocator serviceLocator;
        private readonly ILoggerFacade loggerFacade;

        /// <summary>
        /// 初始化<see cref="ModuleInitializer"/>的实例。
        /// </summary>
        /// <param name="serviceLocator">
        /// 容器将会根据给定的模块类型进行解析。
        /// </param>
        /// <param name="loggerFacade">
        /// 用来记录日志。
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
        /// 根据给定的模块洗洗脑，初始化模块。
        /// </summary>
        /// <param name="moduleInfo">
        /// 用于初始化的模块。
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
        /// 处理模块初始化过程中的任何异常，使用<seealso cref="ILoggerFacade"/>记录，并且抛出
        /// <seealso cref="ModuleInitializeException"/>。
        /// 可以重写这个方法提供不同的方式。
        /// </summary>
        /// <param name="moduleInfo">
        /// 产生异常的模块信息
        /// </param>
        /// <param name="assemblyName">
        /// 程序集名称。
        /// </param>
        /// <param name="exception">
        /// 引起当前错误的异常。
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
        /// 根据规定的类型名称，使用容器解析一个<see cref="IModule"/>实例。
        /// </summary>
        /// <param name="moduleInfo">
        /// 要创建的模块。
        /// </param>
        /// <returns>
        /// <paramref name="moduleInfo"/>指定的模块实例。
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
        /// 根据规定的类型名称，使用容器解析一个<see cref="IModule"/>实例。
        /// </summary>
        /// <param name="typeName">
        /// 解析类型名称。这个类型必须实现<see cref="IModule"/>。
        /// </param>
        /// <returns>
        ///  <paramref name="typeName"/>类型的新实例。
        /// </returns>
        protected virtual IModule CreateModule(string typeName)
        {
            Type moduleType = Type.GetType(typeName);
            if (moduleType == null)
            {
                throw new ModuleInitializeException(string.Format(CultureInfo.CurrentCulture, Resources.FailedToGetType, typeName));
            }

            //判断类型后再创建实例
            if (moduleType.Equals(typeof(IModule)))
            {
                return this.serviceLocator.GetInstance(moduleType) as IModule;
            }
            return null;
        }
    }
}
