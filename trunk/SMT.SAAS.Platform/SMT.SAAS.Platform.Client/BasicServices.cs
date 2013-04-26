using System.Windows;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using SMT.Saas.Tools.PermissionWS;

//------------------------------------------------------------------------------
// 版权所有: 版权所有(C)2011 SMT-Online
// 内容摘要: 为提供创建WCF客户端提供支持，
//           后期将对其进行托管处理 :
//           1. 网络状态检测、服务端状态检测与处理 
//           2. 实现wcf client pool概念
// 完成日期：2011-04-21 
// 版    本：V1.0 
// 作    者：GaoY 
// 修 改 人：
// 修改时间： 
//------------------------------------------------------------------------------

namespace SMT.SAAS.Platform.Client
{
    public class BasicServices
    {
        private static PlatformWS.PlatformServicesClient client;
        private static string serviceAddress = "";

        private const string PLATFORM_SVC_NAME = "/Platform/PlatformServices.svc";
        private const string PERMISSION_SVC_NAME = "/System/PermissionService.svc";
        private const string USERLOGIN_SVC_NAME = "/System/MainUIServices.svc";

        private static BasicHttpBinding CreateBasicBinding()
        {
            return new BasicHttpBinding()
            {
                ReceiveTimeout = new System.TimeSpan(0, 30, 0),
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            };
        }

        private static CustomBinding CreateCustomBinding()
        {

            var elements = new List<BindingElement>();
            elements.Add(new BinaryMessageEncodingBindingElement());
            elements.Add(new HttpTransportBindingElement()
            {
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            });

            return new CustomBinding(elements)
            {
                ReceiveTimeout = new System.TimeSpan(0, 30, 0)

            };
        }

        private static EndpointAddress CreateAddress(string svcName)
        {
            if (!string.IsNullOrEmpty(SMT.SAAS.Main.CurrentContext.Common.HostAddress))
            {
                StringBuilder addressBuilder = new StringBuilder();
                addressBuilder.Append("http://");
                addressBuilder.Append(SMT.SAAS.Main.CurrentContext.Common.HostAddress.ToString());
                addressBuilder.Append(svcName);
                serviceAddress = addressBuilder.ToString();
            }
            else
            {
                SMT.SAAS.Main.CurrentContext.AppContext.SystemMessage("系统全局IP HostAddress参数为空 ");
            }

            return new EndpointAddress(serviceAddress);
        }

        private static PlatformWS.PlatformServicesClient CreateClient()
        {
            //http://localhost:15739/PlatformServices.svc
            //EndpointAddress address = new EndpointAddress("http://localhost:15739/PlatformServices.svc");
            //return new PlatformWS.PlatformServicesClient(CreateBasicBinding(), address);

            return new PlatformWS.PlatformServicesClient(CreateBasicBinding(), CreateAddress(PLATFORM_SVC_NAME));
        }

        private static PermissionServiceClient CreatePermissionClient()
        {
            //EndpointAddress address = new EndpointAddress("http://172.30.50.13/SmtOnline/Develop/Services/System/PermissionService.svc");

            return new PermissionServiceClient(CreateCustomBinding(), CreateAddress(PERMISSION_SVC_NAME));
        }

        private static UserLoginWS.MainUIServicesClient CreateUserLoginClient()
        {
            //http://portal.smt-online.net/Services/System/MainUIServices.svc

            //EndpointAddress address = new EndpointAddress("http://172.30.50.13/SmtOnline/Develop/Services/System/MainUIServices.svc");
            UserLoginWS.MainUIServicesClient client = new UserLoginWS.MainUIServicesClient(CreateCustomBinding(), CreateAddress(USERLOGIN_SVC_NAME));
            return client;
        }

        /// <summary>
        /// 创建一个平台的WCF服务
        /// </summary>
        public static PlatformWS.PlatformServicesClient PlatformClient
        {
            get { return CreateClient(); }
        }

        /// <summary>
        /// 创建一个权限的WCF服务
        /// </summary>
        public static PermissionServiceClient PermissionClient
        {
            get { return CreatePermissionClient(); }
        }

        /// <summary>
        /// 创建一个用户登录/注销的WCF服务
        /// </summary>
        public static UserLoginWS.MainUIServicesClient UserLoginClient
        {
            get { return CreateUserLoginClient(); }
        }
    }
}
