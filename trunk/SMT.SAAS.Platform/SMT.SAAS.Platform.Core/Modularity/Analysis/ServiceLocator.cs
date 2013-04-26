using System;
using System.Globalization;
using System.Reflection;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 实现IServiceLocator 接口，为创建类型实例提供支持。
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Core
{
    /// <summary>
    /// 实现IServiceLocator 接口，为创建类型实例提供支持。
    /// </summary>
    public class ServiceLocator : IServiceLocator
    {

        public System.Collections.Generic.IEnumerable<TService> GetAllInstances<TService>()
        {
            return null;
        }

        public System.Collections.Generic.IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return null;
        }

        public TService GetInstance<TService>()
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            return this.GetInstance(serviceType, string.Empty);
        }

        public object GetInstance(Type serviceType, string key)
        {
            try
            {
                return Activator.CreateInstance(serviceType);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(CultureInfo.CurrentCulture, Resources.FailedTypeInstance, serviceType.AssemblyQualifiedName), ex);
            }
        }

        public object GetInstance(Modularity.ModuleInfo moduleInfo)
        {
            Type moduleType = null;
            try
            {
                if (string.IsNullOrEmpty(moduleInfo.ModuleType))
                {
                    Logging.Logger.Current.Log(
                   "10000",
                   "Platform",
                   "创建具体模块.",
                 string.Format(CultureInfo.CurrentCulture, Resources.ModuleTypeIsNullOrEmpty, moduleInfo.Description),
                 null, Logging.Category.Exception, Logging.Priority.High);
                    ;

                    return null;
                }

                moduleType = Type.GetType(moduleInfo.ModuleType);
                object instance = GetInstance(moduleType);
                if (moduleInfo.InitParams != null && instance != null)
                {
                    foreach (var item in moduleInfo.InitParams)
                    {
                        PropertyInfo property = instance.GetType().GetProperty(item.Key);
                        property.SetValue(instance, item.Value, null);
                    }
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format(CultureInfo.CurrentCulture, Resources.FailedTypeInstance, moduleType.AssemblyQualifiedName), ex);
            }
        }
    }
}
