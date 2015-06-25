using System;
using SMT.Saas.Tools.PlatformWS;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using SMT.Saas.Tools.EngineWS;

namespace SMT.SAAS.Platform.WebParts.Models
{
    /// <summary>
    /// 创建基础服务/可维护成工厂.
    /// </summary>
    public class BasicServices
    {
        private static string serviceAddress = string.Empty;

        private PlatformServicesClient CreateServices()
        {
            return new PlatformServicesClient();
        }
        
        private EngineWcfGlobalFunctionClient CreateEngineClient()
        {
            return new EngineWcfGlobalFunctionClient();
        }

        /// <summary>
        ///创建一个平台服务的实例 
        /// </summary>
        public PlatformServicesClient PlatformClient
        {
            get { return CreateServices(); }

        }
        /// <summary>
        /// 创建一个回调新闻的服务实例
        /// </summary>
        
        /// <summary>
        /// 创建一个消息引擎服务实例
        /// </summary>
        public EngineWcfGlobalFunctionClient EngineClient
        {
            get { return CreateEngineClient(); }
        }
    }
}
