using System;
using SMT.SAAS.Platform.WebParts.NewsWS;
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
        private NewsCallBackWS.NewsCallBackClient CreateCallBack()
        {
            try
            {
                var serviceName = "/Main/NewsCallBack.svc";
                if (!string.IsNullOrEmpty(SMT.SAAS.Main.CurrentContext.Common.HostAddress))
                {
                    StringBuilder addressBuilder = new StringBuilder();
                    addressBuilder.Append("http://");
                    addressBuilder.Append(SMT.SAAS.Main.CurrentContext.Common.HostAddress.ToString());
                    addressBuilder.Append(serviceName);
                    serviceAddress = addressBuilder.ToString();
                }


                EndpointAddress address = new EndpointAddress(serviceAddress);
                CustomBinding binding = new CustomBinding(
                    new PollingDuplexBindingElement()
                    {
                        InactivityTimeout = TimeSpan.FromDays(1),
                        ClientPollTimeout = TimeSpan.FromDays(1)
                    },
                    new BinaryMessageEncodingBindingElement(),
                    new HttpTransportBindingElement());
                return new NewsCallBackWS.NewsCallBackClient(binding, address);
                //DuplexMode=PollingDuplexMode.SingleMessagePerPoll,

            }
            catch (Exception ex)
            {
                return null;
            }
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
        public NewsCallBackWS.NewsCallBackClient CallBackClient
        {
            get { return CreateCallBack(); }
        }
        
        /// <summary>
        /// 创建一个消息引擎服务实例
        /// </summary>
        public EngineWcfGlobalFunctionClient EngineClient
        {
            get { return CreateEngineClient(); }
        }
    }
}
