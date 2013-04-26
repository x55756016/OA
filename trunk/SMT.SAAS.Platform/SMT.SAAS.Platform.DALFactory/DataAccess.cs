using System.Configuration;
using SMT.SAAS.Platform.Model;
using SMT.SAAS.Platform.DAL.Interface;
using System.Reflection;
using System.Diagnostics;
using System;
//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为子系统(应用)信息的数据访问提供接口规范
// 完成日期：2011-04-08 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------
namespace SMT.SAAS.Platform.DALFactory
{
    /// <summary>
    /// 获取数据访问成实例。<br/>
    /// 创建过程中会自动根据配置的工厂类型FactoryType创建对应的数据访问层。
    /// </summary>
    public sealed class DataAccess  
    {
        private static readonly string typePath = ConfigurationManager.AppSettings["FactoryType"];
        private DataAccess() { }

        public static IModuleInfoDAL CreateModuleInfo()
        {
            return CreateObject("ModuleInfoDAL") as IModuleInfoDAL;
        }

        public static IModuleUpdateLogDAL CreateModuleUpdateLog()
        {
            return CreateObject("ModuleUpdateLogDAL") as IModuleUpdateLogDAL;
        }

        public static IPublicPartDAL CreatePublicPart()
        {
            return CreateObject("PublicPartDAL") as IPublicPartDAL;
        }

        public static IResourceDAL CreateResource()
        {
            return CreateObject("ResourceDAL") as IResourceDAL;
        }

        public static IShortCutDAL CreateShortCut()
        {
            return CreateObject("ShortCutDAL") as IShortCutDAL;
        }

        public static ISystemConfigDAL CreateSystemConfig()
        {
            return CreateObject("SystemConfigDAL") as ISystemConfigDAL;
        }

        public static ISystemErrorDAL CreateSystemError()
        {
            return CreateObject("SystemErrorDAL") as ISystemErrorDAL;
        }

        public static IWebPartDAL CreateWebPart()
        {
            return CreateObject("WebPartDAL") as IWebPartDAL;
        }
        
        private static object CreateObject(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                string className = typePath + "." + name;
                return Assembly.Load(typePath).CreateInstance(className);
            }
            catch (System.Exception ex)
            {
                throw new Exception("创建工厂对象产生异常：" + name, ex);
            }
        }

    }
}
