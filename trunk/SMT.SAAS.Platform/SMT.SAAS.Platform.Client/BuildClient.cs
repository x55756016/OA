
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using SMT.Saas.Tools.PermissionWS;
namespace SMT.SAAS.ClientServices
{
    public class BuildClient
    {
        private static string serviceAddress = "http://prreits.smt-online.net/new/Services/Platform/PlatformServices.svc";

        private const string PERMISSION_SVC_NAME = "/System/PermissionService.svc";

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
            if (Application.Current.Resources.Contains("PlatformWShost"))
            {
                StringBuilder addressBuilder = new StringBuilder();
                addressBuilder.Append("http://");
                addressBuilder.Append(Application.Current.Resources["PlatformWShost"].ToString());
                addressBuilder.Append(svcName);
                serviceAddress = addressBuilder.ToString();
            }

            return new EndpointAddress(serviceAddress);
        }
        private static PermissionServiceClient CreatePermissionClient()
        {
            //return new PermissionServiceClient(CreateCustomBinding(), CreateAddress(PERMISSION_SVC_NAME));
            return new PermissionServiceClient();
        }

        /// <summary>
        /// 创建一个权限的WCF服务
        /// </summary>
        public static PermissionServiceClient PermissionClient
        {
            get { return CreatePermissionClient(); }
        }

    }
}
